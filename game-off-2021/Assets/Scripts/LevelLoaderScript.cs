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
    public IEnumerator LoadLevel(string scene)
    {
        _sceneTransition.SetTrigger("Start");
        yield return new WaitForSeconds(_transitionTime);
        SceneManager.LoadScene(scene);
    }

}
