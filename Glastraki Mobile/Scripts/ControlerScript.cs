using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Unity.Notifications.Android;
using UnityEngine.Localization.Settings;

public class ControlerScript : MonoBehaviour
{
    public RequestPermissionScript requestPermissionScript;
    public BluetoothController bluetoothController;
    private IDataService dataService = new JsonDataService();
    public GameObject ScrollV;
    public TMP_Dropdown dropdown;
    public string[] yourdivicesAdress = { "", "", "", "", "", "", "", "", "" };
    public int selecteddrint = 0;
    public GameObject glastrakiameros;
    public GameObject glastraki;
    public GameObject[] glastres = new GameObject[69];
    public GameObject SelectedGO;
    public GameObject GreenSlider;
    public GameObject RedSlider;
    public GameObject EditStuff;
    public GameObject GlastrakiEdit;
    public bool editing;
    public GameObject daImage;
    public TMP_InputField limit;
    public UnityEngine.UI.Toggle toggle;
    public GameObject BGEdit;
    public TMP_InputField fileadr;
    // Start is called before the first frame update
    void Start()
    {
        LanguageSelection(0);
        requestPermissionScript.AskForPermissionsNEI();
        editing = false;
        AndroidNotificationChannel channel = new()
        {
            Id = "channel1",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic Notification"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BigButtonPress()
    {
        SelectedGO = daImage;
    }

    public void EditButtonPress()
    {
        ScrollV.GetComponent<RectTransform>().offsetMin = new(0, 400);
    }
    public void DeleteButtonPress()
    {
        if (SelectedGO != null && SelectedGO != daImage)
        {
            int num = SelectedGO.GetComponent<Glastraki>().num;
            Destroy(SelectedGO);
            bluetoothController.CloseConnection(num);
            bluetoothController.Toast("G" + num + " has been Deleted");
        }
        editing = true;
        TheothereditButtonPress();
    }
    public void SaveButtonPress()
    {

    }
    public void ExitButtonPress()
    {
        ScrollV.GetComponent<RectTransform>().offsetMin = new(0, 0);
        editing = false;
    }
    public void AddButtonPress()
    {
        Debug.Log("ch0");
        string[] yourdivices = bluetoothController.ScanForDevices();
        Debug.Log("ch1");
        for (int i = 0; i < yourdivices.Length; i++)
        {
            Debug.Log("ch2");
            if (yourdivices[i] != null)
                for (int j = 0; j < yourdivices[i].Length; j++)
                {
                    Debug.Log("ch3");
                    if (yourdivices[i][j] == '|')
                    {
                        Debug.Log("ch4");
                        dropdown.options.Add(new TMP_Dropdown.OptionData(yourdivices[i][0..(j - 1)], null));
                        yourdivicesAdress[i] = yourdivices[i][(j + 1)..];
                        Debug.Log("ch5");
                    }
                }
        }
        dropdown.RefreshShownValue();
    }

    public void WhenDropdownValueChanged(int theval) { selecteddrint = theval; }
    public void TheatheraddButtonPress()
    {
        Debug.Log("ch21" + yourdivicesAdress[selecteddrint]);
        dropdown.ClearOptions();
        Debug.Log("ch22");
        for (int i = 0; i < glastres.Length; i++)
        {
            if (glastres[i] == null)
            {
                glastres[i] = Instantiate(glastraki, glastrakiameros.transform);
                glastres[i].GetComponent<Glastraki>().num = i;
                bluetoothController.ConnectToDevice(yourdivicesAdress[selecteddrint], i);
                break;
            }
        }
        Debug.Log("ch23");
        bluetoothController.Toast("Connecting to: " + yourdivicesAdress.Length);
    }
    public void TheothereditButtonPress()
    {
        if (SelectedGO != null)
        {
            if (editing == false)
            {
                if (SelectedGO != daImage)
                {
                    GreenSlider.transform.localPosition = new(SelectedGO.transform.localPosition.x, GreenSlider.transform.localPosition.y);
                    RedSlider.transform.localPosition = new(RedSlider.transform.localPosition.x, SelectedGO.transform.localPosition.y);
                    GreenSlider.GetComponent<Scrollbar>().value = (SelectedGO.transform.localPosition.y + 4850) / 9200;
                    RedSlider.GetComponent<Scrollbar>().value = (SelectedGO.transform.localPosition.x - 4850) / 9200;
                    toggle.isOn = SelectedGO.GetComponent<Glastraki>().auto;
                    limit.text = SelectedGO.GetComponent<Glastraki>().limit.ToString();
                    EditStuff.SetActive(true);
                    GlastrakiEdit.SetActive(true);
                }
                else
                {
                    fileadr.text = null;
                    BGEdit.SetActive(true);
                }
                editing = true;
            }
            else
            {
                GlastrakiEdit.SetActive(false);
                BGEdit.SetActive(false);
                EditStuff.SetActive(false);
                editing = false;
            }
        }
    }

    public void GreenSliderSlids(float theval)
    {
        if (editing && SelectedGO != daImage)
        {
            float thebettervall = (theval * 9200 - 4850);
            SelectedGO.transform.localPosition = new(SelectedGO.transform.localPosition.x, thebettervall);
            RedSlider.transform.localPosition = new(RedSlider.transform.localPosition.x, thebettervall);
        }
    }

    public void RedSliderSlids(float theval)
    {
        if (editing && SelectedGO != daImage)
        {
            float thebettervall = 4850 - theval * 9200;
            SelectedGO.transform.localPosition = new(thebettervall, SelectedGO.transform.localPosition.y);
            GreenSlider.transform.localPosition = new(thebettervall, GreenSlider.transform.localPosition.y);
        }
    }

    public void ScokeDitected(int glastranum)
    {
        bluetoothController.Toast("Smoke ditected in :) G" + glastranum);

        AndroidNotification notification = new()
        {
            Title = "Smoke Detected",
            Text = "Smoke detected in G" + glastranum,
            FireTime = DateTime.Now
        };

        AndroidNotificationCenter.SendNotification(notification, "channel1");
    }

    public void NewLimit(string theval)
    {
        if (SelectedGO != null && SelectedGO != daImage)
        {
            SelectedGO.GetComponent<Glastraki>().limit = int.Parse(theval);
            bluetoothController.SendData("?r=0&y=0&g=0&l=" + theval + "&a=" + IsAuto(SelectedGO.GetComponent<Glastraki>().auto) + ";", SelectedGO.GetComponent<Glastraki>().num);
        }
    }

    private int IsAuto(bool theval)
    {
        if (theval)
            return 1;
        else 
            return 0;
    }

    public void WhenAutoToglePressed(bool auto)
    {
        if (editing && SelectedGO != daImage)
        {
            SelectedGO.GetComponent<Glastraki>().auto = auto;
            if (auto)
                bluetoothController.SendData("?r=0&y=0&g=0&l="+ SelectedGO.GetComponent<Glastraki>().limit + "&a=1;", SelectedGO.GetComponent<Glastraki>().num);
            else
                bluetoothController.SendData("?r=0&y=0&g=0&l="+ SelectedGO.GetComponent<Glastraki>().limit + "&a=0;", SelectedGO.GetComponent<Glastraki>().num);
        }
    }

    public void ReplacedaImage(string filePath)
    {
        
        if (File.Exists(filePath))
        {

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new(2, 2);
            texture.LoadImage(fileData); 
            Rect rect = new(0, 0, texture.width, texture.height);
            Sprite newSprite = Sprite.Create(texture, rect, new(0.5f, 0.5f));
            daImage.GetComponent<UnityEngine.UI.Image>().sprite = newSprite;
        }
        else
        {
            Debug.LogError("File not found at: " + filePath);
            bluetoothController.Toast("File not found");
        }
    }

    public void LanguageSelection(Int32 language)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[language];
    }
}

