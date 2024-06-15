package com.example.unitypluginn;

import android.app.Activity;
import android.util.Log;
import android.widget.Toast;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

import androidx.annotation.NonNull;

import java.io.IOException;
import java.io.OutputStream;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

public class PluginInstance {
    private static Activity unityActivity;
    private static BluetoothAdapter bluetoothAdapter;

    private BluetoothDevice[] device = new BluetoothDevice[69];
    private BluetoothSocket[] socket = new BluetoothSocket[69];
    private OutputStream[] outputStream = new OutputStream[69];
    private InputStream[] inputStream = new InputStream[69];
    private boolean[] isConnected = new boolean[69]; // Track connection state

    public static void receiveUnityActivity(Activity tActivity) {
        unityActivity = tActivity;
    }

    public void Toast(String msg) {
        if (unityActivity != null) {
            unityActivity.runOnUiThread(() -> Toast.makeText(unityActivity, msg, Toast.LENGTH_SHORT).show());
        } else {
            Log.e("PluginInstance", "Unity activity is null. Cannot show toast.");
        }
    }

    public PluginInstance() {
        bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
    }

    public boolean connectToDevice(String deviceAddress, int deviceNum) {
        if (bluetoothAdapter == null || !bluetoothAdapter.isEnabled()) {
            Log.e("BluetoothConnection", "Bluetooth is not enabled or not available.");
            return false;
        }

        if (deviceAddress == null || deviceAddress.isEmpty()) {
            Log.e("BluetoothConnection", "Device address is null or empty.");
            return false;
        }

        try {
            device[deviceNum] = bluetoothAdapter.getRemoteDevice(deviceAddress);
        } catch (IllegalArgumentException e) {
            Log.e("BluetoothConnection", "Invalid device address.", e);
            return false;
        }

        new Thread(() -> {
            UUID uuid = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB"); // SPP UUID for HC-06

            for (int attempt = 1; attempt <= 10; attempt++) {
                try {
                    socket[deviceNum] = device[deviceNum].createRfcommSocketToServiceRecord(uuid);
                    bluetoothAdapter.cancelDiscovery(); // Cancel discovery to speed up the connection
                    socket[deviceNum].connect();

                    outputStream[deviceNum] = socket[deviceNum].getOutputStream();
                    inputStream[deviceNum] = socket[deviceNum].getInputStream();
                    isConnected[deviceNum] = true; // Mark as connected

                    Log.i("BluetoothConnection", "Successfully connected to device on attempt " + attempt);
                    return;
                } catch (IOException e) {
                    Log.e("BluetoothConnection", "Error during Bluetooth connection attempt " + attempt, e);
                    closeSocket(deviceNum); // Ensure socket is closed on error

                    if (attempt == 10) {
                        Log.e("BluetoothConnection", "Failed to connect after 10 attempts.");
                        return;
                    }

                    // Wait before retrying
                    try {
                        Thread.sleep(2000);
                    } catch (InterruptedException ie) {
                        Log.e("BluetoothConnection", "Sleep interrupted during retry wait.", ie);
                    }
                }
            }
        }).start();

        return true;
    }

    private void closeSocket(int deviceNum) {
        try {
            if (socket[deviceNum] != null) {
                socket[deviceNum].close();
                socket[deviceNum] = null;  // Set to null to avoid reuse
            }
            isConnected[deviceNum] = false; // Mark as disconnected
        } catch (IOException e) {
            Log.e("BluetoothConnection", "Error closing socket.", e);
        }
    }

    public void send(@NonNull String message, int deviceNum) {
        new Thread(() -> {
            try {
                if (outputStream[deviceNum] != null) {
                    outputStream[deviceNum].write(message.getBytes());
                    Log.i("BluetoothConnection", "Message sent: " + message);
                } else {
                    Log.e("BluetoothConnection", "OutputStream is null, cannot send message.");
                }
            } catch (IOException e) {
                Log.e("BluetoothConnection", "Error sending message.", e);
            }
        }).start();
    }

    public String receiveData(int deviceNum) {
        if (!isConnected[deviceNum]) {
            Log.e("BluetoothConnection", "Socket is not connected.");
            return null;
        }

        StringBuilder receivedData = new StringBuilder();
        try {
            if (inputStream[deviceNum] != null) {
                byte[] buffer = new byte[1024];
                int bytes;
                if ((bytes = inputStream[deviceNum].read(buffer)) != -1) {
                    receivedData.append(new String(buffer, 0, bytes));
                    Log.i("BluetoothConnection", "Received data: " + receivedData.toString());
                } else {
                    Log.e("BluetoothConnection", "No data received.");
                }
            } else {
                Log.e("BluetoothConnection", "InputStream is null, cannot receive data.");
            }
        } catch (IOException e) {
            Log.e("BluetoothConnection", "Error receiving data.", e);
        }

        return receivedData.toString();
    }

    public void closeConnection(int deviceNum) {
        new Thread(() -> {
            try {
                if (outputStream[deviceNum] != null) {
                    outputStream[deviceNum].close();
                    outputStream[deviceNum] = null;  // Set to null to avoid reuse
                }
                if (socket[deviceNum] != null) {
                    socket[deviceNum].close();
                    socket[deviceNum] = null;  // Set to null to avoid reuse
                }
                isConnected[deviceNum] = false; // Mark as disconnected
            } catch (IOException e) {
                Log.e("BluetoothConnection", "Error closing connection.", e);
            }
        }).start();
    }

    public static String[] getPairedDeviceNamesAndAddresses() {
        List<String> devicesInfo = new ArrayList<>();

        if (bluetoothAdapter != null) {
            for (BluetoothDevice device : bluetoothAdapter.getBondedDevices()) {
                String deviceInfo = device.getName() + "|" + device.getAddress();
                devicesInfo.add(deviceInfo);
            }
        }

        return devicesInfo.toArray(new String[0]);
    }
}






