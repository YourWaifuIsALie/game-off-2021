using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingScript : MonoBehaviour
{
    [SerializeField]
    private GameObject _backdrop;
    private int _videoTime = 13 + 5;
    void Start()
    {
        _backdrop.SetActive(false);
        StartCoroutine(EndGame());
    }

    public IEnumerator EndGame()
    {
        yield return new WaitForSeconds(_videoTime);
        Application.Quit();
    }
}
