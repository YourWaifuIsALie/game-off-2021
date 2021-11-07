using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigateMenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject currentMenu;

    private GameObject previousMenu;
    [SerializeField]
    private GameObject levelLoader;


    public void ChangeMenu(GameObject menu)
    {
        // Activating/deactivating canvases changes the priority on display
        // Ensure all canvases have the proper sort order
        menu.SetActive(true);
        currentMenu.SetActive(false);
        SetRelativeBack(menu);

    }

    public void ChangeScene(string scene)
    {
        var script = (LevelLoaderScript)levelLoader.GetComponent(typeof(LevelLoaderScript));
        StartCoroutine(script.LoadLevel(scene));
    }


    public void PreviousMenu()
    {
        if (previousMenu != null)
        {
            ChangeMenu(previousMenu);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetPreviousMenu(GameObject menu)
    {
        previousMenu = menu;
    }
    public void SetRelativeBack(GameObject menu)
    {
        // Don't know why returns would be null in Unity, so general try-catch instead of specific if-else
        try
        {
            var relativeCanvas = menu.transform.Find("RelativeControlsCanvas").gameObject;
            var backButton = relativeCanvas.transform.Find("BackButton").gameObject;
            var script = (NavigateMenuScript)backButton.GetComponent(typeof(NavigateMenuScript));
            script.SetPreviousMenu(currentMenu);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
