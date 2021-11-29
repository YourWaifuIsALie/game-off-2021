using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActorScript : MonoBehaviour
{
    [SerializeField]
    public BattleManagerScript _battleManager;

    [SerializeField]
    private AudioSource _hurtSound;
    [SerializeField]
    private AudioSource _magicSound;
    [SerializeField]
    private AudioSource _bugSound;
    [SerializeField]
    private ParticleSystem _hurtVisual;
    [SerializeField]
    private ParticleSystem _magicVisual;

    [SerializeField]
    public OptionsManagerScript _optionsManager;

    public GameObject battleActorGraphics;

    public IBattleActor _battleActor { get; set; }
    private bool _isSelected;

    public void Start()
    {
        _isSelected = false;
        _hurtSound.volume = _optionsManager.GetVolume(_hurtSound.clip.name);
        _magicSound.volume = _optionsManager.GetVolume(_magicSound.clip.name);
        _bugSound.volume = _optionsManager.GetVolume(_bugSound.clip.name);
    }

    public void SetSelected(bool value)
    {
        _isSelected = value;
        var graphicsScript = (BattleActorGraphicScript)battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
        graphicsScript.UpdateSelected(value);
    }
    public void PlayAnimation(string value)
    {
        var graphicsScript = (BattleActorGraphicScript)battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
        graphicsScript.PlayAnimation(value);
    }
    public void PlaySound(string value)
    {
        switch (value)
        {
            case "Hurt":
                _hurtSound.Play();
                _hurtVisual.Play();
                break;
            case "Heal":
                _magicSound.Play();
                _magicVisual.Play();
                break;
            case "Bug":
                _bugSound.Play();
                break;
            default:
                break;
        }
    }
    public void SetDead()
    {
        var graphicsScript = (BattleActorGraphicScript)battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
        graphicsScript.SetTextColor(Color.gray);
    }
}
