using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Glastraki : MonoBehaviour
{
    private ControlerScript controlerScript;
    private BluetoothController bluetoothController;
    public int num;
    public int limit;
    public TMP_Text pinakida;
    public TMP_Text text;
    public Image statusLED;
    private Coroutine coroutine;
    public Slider slider;
    public bool havemsg;
    private string msgforpi;
    public Image thecolor;
    private float nextnot = 0;
    public bool auto = true;
    void Start()
    {
        controlerScript = GameObject.FindGameObjectWithTag("Controler").GetComponent<ControlerScript>();
        bluetoothController = GameObject.FindGameObjectWithTag("Controler").GetComponent<BluetoothController>();
        pinakida.text = "G" + num;
        havemsg = false;
        StartCoroutine(UpdAte());
        coroutine = StartCoroutine(Timer());
    }

    private IEnumerator UpdAte()
    {

        string msgfrompi = bluetoothController.ReceiveData(num);
        if (msgfrompi != null)
        {
            Debug.Log(msgfrompi);
            if (msgfrompi[0] == '?')
            {
                Debug.Log("MsgStart");
                msgfrompi = msgfrompi[1..];
            }
            try {
                if (msgfrompi[1] == '1')
                    statusLED.color = Color.green;
                else
                    statusLED.color = Color.yellow;

                text.text = msgfrompi[3..];
                if (int.Parse(msgfrompi[4..]) > limit && nextnot < Time.realtimeSinceStartup)
                {
                    nextnot = Time.realtimeSinceStartup + 30;
                    controlerScript.ScokeDitected(num);
                }
            }
            catch { }
            StopCoroutine(coroutine);
            coroutine = StartCoroutine(Timer());
        }
        if (havemsg && !auto)
        {
            Debug.Log("31\t" + msgforpi);
            bluetoothController.SendData(msgforpi, num);
            msgforpi = null;
            havemsg = false;
        }
        yield return new WaitForSecondsRealtime(1);
        StartCoroutine(UpdAte());
    }

    public void SliderSlids(float num)
    {
        int numm = (int)(num * 510);
        num *= 2;
        if (numm <= 255)
        {
            msgforpi = "?r=" + 0 + "&y=" + numm + "&g=" + (255 - numm) + "&a=0;";
            thecolor.color = new(num, 1, 0);
        }
        else 
        {
            numm -= 255;
            num -= 1;
            msgforpi = "?r=" + numm + "&y=" + (255 - numm) + "&g=" + 0 + "&a=0;";
            thecolor.color = new(255, 1 - num, 0);
        }
        
        havemsg = true;
    }

    public void WhenButtonPressed()
    {
        controlerScript.SelectedGO = controlerScript.glastres[num];
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(3);
        statusLED.color = Color.red;
    }
}
