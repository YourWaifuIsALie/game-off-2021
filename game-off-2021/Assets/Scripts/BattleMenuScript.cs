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

    [SerializeField]
    private GameObject _camera;

    public void Update()
    {
        UpdateTurnOrderDisplay();
        UpdateHealthBars();
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

    private void UpdateHealthBars()
    {
        foreach (var obj in _battleManager._playerActorObjects)
        {
            var actorScript = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
            int currentHealth = actorScript._battleActor.stats.currentHealth;
            var maxHealth = actorScript._battleActor.stats.maxHealth;
            var graphicsScript = (BattleActorGraphicScript)actorScript.battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
            graphicsScript.UpdateHealth(currentHealth, maxHealth);
            graphicsScript.UpdateName(actorScript._battleActor.stats.displayName);
            graphicsScript.RotateTowards(_camera.transform);

        }
        foreach (var obj in _battleManager._enemyActorObjects)
        {
            var actorScript = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
            int currentHealth = actorScript._battleActor.stats.currentHealth;
            var maxHealth = actorScript._battleActor.stats.maxHealth;
            var graphicsScript = (BattleActorGraphicScript)actorScript.battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
            graphicsScript.UpdateHealth(currentHealth, maxHealth);
            graphicsScript.UpdateName(actorScript._battleActor.stats.displayName);
            graphicsScript.RotateTowards(_camera.transform);
        }
    }

    private string GetDisplayName(GameObject obj)
    {
        var script = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
        return script._battleActor.stats.displayName;
    }
}
