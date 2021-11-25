using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderScript : MonoBehaviour
{
    [SerializeField]
    private Animator _sceneTransition;
    [SerializeField]
    private float _transitionTime = 1f;

    private float _inAndWaitTime = 2f;

    public IEnumerator LoadLevel(string scene)
    {
        _sceneTransition.SetTrigger("Start");
        yield return new WaitForSeconds(_transitionTime);
        SceneManager.LoadScene(scene);
    }

    public IEnumerator FadeScreen()
    {
        _sceneTransition.SetTrigger("Start");
        yield return new WaitForSeconds(_inAndWaitTime);
        _sceneTransition.SetTrigger("End");
        // yield return new WaitForSeconds(_transitionTime);
    }
    public IEnumerator AppearScreen()
    {
        _sceneTransition.SetTrigger("End");
        yield return new WaitForSeconds(_transitionTime);
    }


}
