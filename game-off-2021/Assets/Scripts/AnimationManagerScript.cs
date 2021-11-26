using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManagerScript : MonoBehaviour
{
    // Can use AnimationEvent type for access to all possible event params
    public void AnimationEvent(string value)
    {
        // Quick and dirty encoding of additional information in string
        string invokeValue = $"{value},{transform.root.gameObject.GetInstanceID().ToString()}";
        BattleActorScript rootScript = transform.root.gameObject.GetComponent<BattleActorScript>();
        rootScript._battleManager._animationEvent.Invoke(invokeValue);
    }
}
