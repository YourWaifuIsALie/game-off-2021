using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMenuButtonScript : MonoBehaviour
{
    // Copy-pasting some NavigateMenuScript code because messy is easy and quick
    [SerializeField]
    public BattleManagerScript _battleManager;

    [SerializeField]
    public GameObject _currentMenu;

    [SerializeField]
    public GameObject _otherBackButton;

    private GameObject _previousMenu;


    public void InvokeEvent(string eventValue)
    {
        _battleManager._playerInputEvent.Invoke(eventValue);
    }

    public void ChangeMenu(GameObject menu)
    {
        menu.SetActive(true);
        _currentMenu.SetActive(false);
        if (_otherBackButton)
            SetRelativeBack(menu, _otherBackButton);
    }

    public void SetRelativeBack(GameObject menu, GameObject backButton)
    {
        try
        {
            BattleMenuButtonScript script = backButton.GetComponent<BattleMenuButtonScript>();
            script.SetPreviousMenu(_currentMenu);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SetPreviousMenu(GameObject menu)
    {
        _previousMenu = menu;
    }
    public void PreviousMenu()
    {
        if (_previousMenu != null)
        {
            ChangeMenu(_previousMenu);
        }
    }

}
