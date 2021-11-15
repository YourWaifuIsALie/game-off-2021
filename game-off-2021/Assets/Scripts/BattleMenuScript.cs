using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuScript : MonoBehaviour
{
    [SerializeField]
    private BattleManagerScript _battleManager;

    [SerializeField]
    private Text _turnOrderDisplay;

    public void Update()
    {
        UpdateTurnOrderDisplay();
    }

    // TODO graphical event system for recalculated turn order animation?
    // TODO colorcode/symbol (brackets vs curlybrace?) based on on side
    private void UpdateTurnOrderDisplay()
    {
        List<GameObject> turnOrder = _battleManager._currentTurnOrder;
        string turnOrderString = "";
        foreach (GameObject obj in turnOrder)
        {
            string displayName = GetDisplayName(obj);
            if (obj == _battleManager._currentActor)
                turnOrderString += " <b>[" + displayName + "]</b> ";
            else
                turnOrderString += " [" + displayName + "] ";
        }
        _turnOrderDisplay.text = turnOrderString;
    }

    private string GetDisplayName(GameObject obj)
    {
        var script = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
        return script._battleActor.stats.displayName;
    }
}
