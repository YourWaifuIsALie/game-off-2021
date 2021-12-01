using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class OptionsManagerScript : MonoBehaviour
{
    [SerializeField]
    private Slider _masterVolumeSlider;
    [SerializeField]
    private Slider _backgroundVolumeSlider;
    [SerializeField]
    private Slider _effectVolumeSlider;
    [SerializeField]
    private Dropdown _windowModeDropdown;
    [SerializeField]
    private Dropdown _resolutionDropdown;

    private string DATAPATH;
    private string CONFIGNAME = "config.json";
    private string CONFIGFILE;
    private Dictionary<string, string> _optionsDictionary;
    private Dictionary<string, string> _defaultOptionsDictionary = new Dictionary<string, string> {
        {"masterVolume", "1.0"},
        {"backgroundVolume", "1.0"},
        {"effectVolume", "1.0"},
        {"resolution", "1920x1080"},
        {"windowMode", "Fullscreen"}
    };

    // Because we're bad we'll just track the sounds base volumes here too
    private Dictionary<string, float> _maxSoundVolumes = new Dictionary<string, float> {
        {"01town2", 0.2f}, // Main menu
        {"04forest3", 0.15f}, // Battle 1
        {"02store2", 0.15f}, // Battle 2
        {"Metal Impact 7", 0.15f}, // Hit effect
        {"Magic Spell_Simple Swoosh_6", 0.2f}, // Magic effect
        {"SB_Mangled_Scream_10", 0.25f} // Bug effect
    };

    [SerializeField]
    private bool _canChange;

    private List<string> _backgroundSoundList = new List<string> { "01town2", "04forest3", "02store2" };
    private List<string> _effectSoundList = new List<string> { "Metal Impact 7", "Magic Spell_Simple Swoosh_6", "SB_Mangled_Scream_10" };

    private List<string> _windowModeList = new List<string> { "Fullscreen", "Window Borderless", "Window" };
    // private List<string> _resolutionList = new List<string> { "1920x1080", "1920x1200", "1600x1200", "1600x900", "1280x800", "1280x720", "1024x768", "640x480" };
    // Since I'm not properly scaling things for relative resolutions, remove the problematic ones
    // Non-16:9 resolutions also don't load so instead of fixing we say bye-bye
    private List<string> _resolutionList = new List<string> { "1920x1080", "1600x900", "1280x720" };

    public void Start()
    {
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

        // I should do further error checking for bad values after setup but whatever
        // SetWindowMode();
        SetOptionsMenu();
        SetResolution();
    }

    public void SaveOptions()
    {
        File.WriteAllText(CONFIGFILE, JsonConvert.SerializeObject(_optionsDictionary));
    }

    public void Update()
    {
        // There should be a flag for when a change is made in a menu/to the json but alas I deleted it
        if (_canChange)
        {
            _optionsDictionary["masterVolume"] = _masterVolumeSlider.value.ToString();
            _optionsDictionary["backgroundVolume"] = _backgroundVolumeSlider.value.ToString();
            _optionsDictionary["effectVolume"] = _effectVolumeSlider.value.ToString();
            _optionsDictionary["windowMode"] = _windowModeDropdown.options[_windowModeDropdown.value].text;
            _optionsDictionary["resolution"] = _resolutionDropdown.options[_resolutionDropdown.value].text;
            SetWindowMode();
            SetResolution();
        }
    }

    public void SetOptionsMenu()
    {
        if (_masterVolumeSlider)
            _masterVolumeSlider.value = GetOptionFloat("masterVolume");
        if (_backgroundVolumeSlider)
            _backgroundVolumeSlider.value = GetOptionFloat("backgroundVolume");
        if (_effectVolumeSlider)
            _effectVolumeSlider.value = GetOptionFloat("effectVolume");
        if (_windowModeDropdown)
        {
            var check = _windowModeDropdown.options.FindIndex(x => x.text == _optionsDictionary["windowMode"]);
            if (check != -1)
                _windowModeDropdown.value = check;
        }
        if (_resolutionDropdown)
        {
            var check = _resolutionDropdown.options.FindIndex(x => x.text == _optionsDictionary["resolution"]);
            if (check != -1)
                _resolutionDropdown.value = check;
        }
    }

    public float GetVolume(string name)
    {
        float masterVolume = GetOptionFloat("masterVolume");
        float backgroundVolume = GetOptionFloat("backgroundVolume");
        float effectVolume = GetOptionFloat("effectVolume");
        if (_backgroundSoundList.Contains(name))
        {
            return _maxSoundVolumes[name] * masterVolume * backgroundVolume;
        }
        else
        {
            return _maxSoundVolumes[name] * masterVolume * effectVolume;
        }
    }

    private float GetOptionFloat(string key)
    {
        float output = 1f;
        try
        {
            output = float.Parse(_optionsDictionary[key]);
        }
        catch (Exception)
        {
            Debug.Log($"Error reading {key} from config json. Setting to max.");
        }
        if (output > 1f)
            return 1f;
        else if (output < 0f)
            return 0f;
        else
            return output;
    }

    public void SetWindowMode()
    {
        switch (_optionsDictionary["windowMode"])
        {
            case "Fullscreen":
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case "Window":
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case "Window Borderless":
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                break;
        }
    }

    public void SetResolution()
    {
        try
        {
            string[] resolution = _optionsDictionary["resolution"].Split('x');
            // Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), Screen.fullScreenMode);
            // Last minute bug fix
            switch (_optionsDictionary["windowMode"])
            {
                case "Fullscreen":
                    Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), FullScreenMode.ExclusiveFullScreen);
                    break;
                case "Window":
                    Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), FullScreenMode.Windowed);
                    break;
                case "Window Borderless":
                    Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), FullScreenMode.FullScreenWindow);
                    break;
                default:
                    break;
            }
        }
        catch (Exception exc)
        {
            Debug.Log($"Error updating screen resolution: {exc}");
        }
    }
}
