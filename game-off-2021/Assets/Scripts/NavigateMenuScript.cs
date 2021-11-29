using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
public class NavigateMenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject _currentMenu;

    private GameObject _previousMenu;
    [SerializeField]
    private GameObject _levelLoader;

    [SerializeField]
    private GameObject _battleManager = default;

    public void ChangeMenu(GameObject menu)
    {
        // Activating/deactivating canvases changes the priority on display
        // Ensure all canvases have the proper sort order
        menu.SetActive(true);
        _currentMenu.SetActive(false);
        SetRelativeBack(menu);

    }

    public void ChangeScene(string scene)
    {
        var script = (LevelLoaderScript)_levelLoader.GetComponent(typeof(LevelLoaderScript));
        StartCoroutine(script.LoadLevel(scene));
    }


    public void PreviousMenu()
    {
        if (_previousMenu != null)
        {
            ChangeMenu(_previousMenu);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetPreviousMenu(GameObject menu)
    {
        _previousMenu = menu;
    }
    public void SetRelativeBack(GameObject menu)
    {
        // Don't know why returns would be null in Unity, so general try-catch instead of specific if-else
        try
        {
            var relativeCanvas = menu.transform.Find("RelativeControlsCanvas").gameObject;
            var backButton = relativeCanvas.transform.Find("BackButton").gameObject;
            var script = (NavigateMenuScript)backButton.GetComponent(typeof(NavigateMenuScript));
            script.SetPreviousMenu(_currentMenu);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void StartTestBattle(string filepath)
    {
        _currentMenu.SetActive(false);

        var script = (BattleManagerScript)_battleManager.GetComponent(typeof(BattleManagerScript));
        script.SetupBattle(filepath);
    }

    public void RetryBattle()
    {
        // Disable button so player can't spam click
        gameObject.GetComponent<Button>().enabled = false;
        var script = (BattleManagerScript)_battleManager.GetComponent(typeof(BattleManagerScript));
        script.RetryBattle();
    }

    public void Testfunction()
    {
        // Random function to test things as needed
    }
}