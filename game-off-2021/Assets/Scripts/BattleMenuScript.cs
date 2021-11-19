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
        UpdateHealthBars(_battleManager._playerActorObjects);
        UpdateHealthBars(_battleManager._enemyActorObjects);
        UpdateDead(_battleManager._deadPlayerActorObjects);
        UpdateDead(_battleManager._deadEnemyActorObjects);
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

    private void UpdateHealthBars(List<GameObject> actorObjects)
    {
        foreach (var actorObject in actorObjects)
        {
            var actorScript = (BattleActorScript)actorObject.GetComponent(typeof(BattleActorScript));
            int currentHealth = actorScript._battleActor.stats.currentHealth;
            var maxHealth = actorScript._battleActor.stats.maxHealth;
            var graphicsScript = (BattleActorGraphicScript)actorScript.battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
            graphicsScript.UpdateHealth(currentHealth, maxHealth);
            graphicsScript.UpdateName(actorScript._battleActor.stats.displayName);
            graphicsScript.RotateTowards(_camera.transform);
        }
    }

    private void UpdateDead(List<GameObject> actorObjects)
    {
        foreach (var actorObject in actorObjects)
        {
            Vector3 inTheGround = new Vector3(actorObject.transform.position.x, 0, actorObject.transform.position.z);
            actorObject.transform.position = inTheGround;
        }
    }

    private string GetDisplayName(GameObject obj)
    {
        var script = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
        return script._battleActor.stats.displayName;
    }
}
