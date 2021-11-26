using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class OptionsManagerScript : MonoBehaviour
{
    private string DATAPATH;
    private string CONFIGNAME = "config.json";
    private string CONFIGFILE;
    private Dictionary<string, string> _optionsDictionary;
    private Dictionary<string, string> _defaultOptionsDictionary = new Dictionary<string, string> {
        {"masterVolume", "100"},
        {"backgroundVolume", "100"},
        {"effectVolume", "100"},
        {"resolution", "1920x1080"},
        {"windowMode", "fullscreen"}
    };
    private bool _isChange;

    public void Start()
    {
        _isChange = false;
        DATAPATH = Application.persistentDataPath;
        CONFIGFILE = Path.Combine(DATAPATH, CONFIGNAME);
        try
        {
            string filein = File.ReadAllText(CONFIGFILE);
            _optionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filein);
            if (_optionsDictionary == null)
            {
                Debug.Log("Fail serialize. Invalid options file. Deleting...");
                File.Delete(CONFIGFILE);
                _optionsDictionary = new Dictionary<string, string>();
            }
            foreach (KeyValuePair<string, string> element in _defaultOptionsDictionary)
            {
                if (!_optionsDictionary.ContainsKey(element.Key))
                    _optionsDictionary[element.Key] = _defaultOptionsDictionary[element.Key];
            }
            File.WriteAllText(CONFIGFILE, JsonConvert.SerializeObject(_optionsDictionary));
        }
        catch (System.IO.FileNotFoundException)
        {
            FileStream newFile = File.Create(CONFIGFILE);
            newFile.Close();

            File.WriteAllText(CONFIGFILE, JsonConvert.SerializeObject(_defaultOptionsDictionary));
            _optionsDictionary = _defaultOptionsDictionary;
        }
        catch (Exception exc)
        {
            Debug.Log($"Unexpected exception: {exc}");
            if (File.Exists(CONFIGFILE))
                File.Delete(CONFIGFILE);
            _optionsDictionary = _defaultOptionsDictionary;
        }

        SetWindowMode();
    }

    public void Update()
    {
        if (_isChange)
        {
            ;
        }
    }

    public void SetWindowMode()
    {
        switch (_optionsDictionary["windowMode"])
        {
            case "fullscreen":
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case "window":
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case "borderlessWindow":
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                break;
        }
    }
}
