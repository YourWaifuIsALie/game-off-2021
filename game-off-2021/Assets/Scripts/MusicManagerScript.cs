using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerScript : MonoBehaviour
{
    [SerializeField]
    private List<AudioSource> _backgroundMusicList;

    [SerializeField]
    private AudioSource _buggedMusic;

    [SerializeField]
    private OptionsManagerScript _optionsManagerScript;

    private int _currentMusicIndex;
    private AudioSource _currentMusic;

    public void Start()
    {
        _currentMusicIndex = 0;
        _currentMusic = _backgroundMusicList[_currentMusicIndex];

    }

    public void LateUpdate()
    {
        // There should be a flag for when a change is made in a menu/to the json but alas
        foreach (AudioSource audio in _backgroundMusicList)
        {
            audio.volume = _optionsManagerScript.GetVolume(audio.clip.name);
        }
    }

    public void SwitchMusic(string value)
    {
        if (value == "next")
        {
            _currentMusic.Stop();
            _currentMusicIndex += 1;
            if (_currentMusicIndex > _backgroundMusicList.Count)
                _currentMusicIndex = 0;
            _currentMusic = _backgroundMusicList[_currentMusicIndex];
            _currentMusic.Play();
        }
        else
        {
            Debug.Log("PLAY BUG");
            _currentMusic.Stop();
            _buggedMusic.Play();
        }
    }

}
