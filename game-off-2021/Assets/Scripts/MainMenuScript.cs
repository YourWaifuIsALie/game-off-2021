using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    private OptionsManagerScript _optionsManager;

    private bool _isFirstLoad;

    public void Awake()
    {
        // Awake -> OnEnable -> Start
        _isFirstLoad = true;
    }

    public void OnEnable()
    {
        // A hack to save the options menu settings
        if (_isFirstLoad)
        {
            _isFirstLoad = false;
        }
        else
        {
            _optionsManager.SaveOptions();
        }
    }
}
