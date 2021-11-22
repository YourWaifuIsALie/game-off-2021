using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManagerScript : MonoBehaviour
{
    public void AnimationEvent(string value)
    {
        BattleActorScript rootScript = transform.root.gameObject.GetComponent<BattleActorScript>();
        rootScript._battleManager._animationEvent.Invoke(value);
    }
}
