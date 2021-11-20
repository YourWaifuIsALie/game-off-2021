using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActorScript : MonoBehaviour
{
    public GameObject battleActorGraphics;

    public IBattleActor _battleActor { get; set; }
    private bool _isSelected;

    public BattleActorScript()
    {
        _isSelected = false;
    }
    public void SetSelected(bool value)
    {
        _isSelected = value;
        var graphicsScript = (BattleActorGraphicScript)battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
        graphicsScript.UpdateSelected(value);
    }
}
