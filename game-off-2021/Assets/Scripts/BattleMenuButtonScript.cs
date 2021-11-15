using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMenuButtonScript : MonoBehaviour
{
    [SerializeField]
    private BattleManagerScript _battleManager;

    public void InvokeEvent(string eventValue)
    {
        _battleManager._playerInputEvent.Invoke(eventValue);
    }
}
