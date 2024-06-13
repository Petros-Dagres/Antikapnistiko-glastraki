using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothController : MonoBehaviour
{

    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaObject _pluginInstance;

    void Start()
    {
        InitializePlugin("com.example.unitypluginn.PluginInstance");
    }

    void InitializePlugin(string pluginName)
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        _pluginInstance = new AndroidJavaObject(pluginName);
        if (_pluginInstance == null)
        {
            Debug.Log("Something wend wrong");
            return;
        }
        _pluginInstance?.CallStatic("receiveUnityActivity", unityActivity);
    }

    public void Toast(string msg)
    {
        _pluginInstance?.Call("Toast", msg);
    }

    public void ConnectToDevice(string deviceAddress, int deviceNum)
    {
        // Call the connectToDevice method of the BluetoothManager class
        _pluginInstance?.Call<bool>("connectToDevice", deviceAddress, deviceNum);
    }

    public void SendData(string data, int deviceNum)
    {
        // Call the send method of the BluetoothManager class
        _pluginInstance?.Call("send", data, deviceNum);
    }

    public string ReceiveData(int deviceNum)
    {
        return _pluginInstance?.Call<string>("receiveData", deviceNum);
    }

    public void CloseConnection(int deviceNum)
    {
        // Call the closeConnection method of the BluetoothManager class
        _pluginInstance?.Call("closeConnection", deviceNum);
    }

    public string[] ScanForDevices()
    {
        string[] devicesInfo = _pluginInstance.CallStatic<string[]>("getPairedDeviceNamesAndAddresses");
        if (devicesInfo != null)
        {
            foreach (string deviceInfo in devicesInfo)
            {
                Debug.Log("Device Info: " + deviceInfo);
            }
            return devicesInfo;
        }
        else
        {
            Toast("No paired devices found.");
            Debug.Log("No paired devices found.");
            return null;
        }
    }
}


