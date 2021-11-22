using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleMenuScript : MonoBehaviour
{
    [SerializeField]
    private BattleManagerScript _battleManager;

    [SerializeField]
    private Text _turnOrderDisplay;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Button _attackButton;

    [SerializeField]
    private LayerMask _descriptionLayer;

    [SerializeField]
    private CanvasRenderer _hoverPanel;

    // Global namespace please
    private Dictionary<string, Type> TAGTYPE = new Dictionary<string, Type> { { "BattleActor", typeof(BattleActorScript) } };
    private Dictionary<string, string> BUTTONDESCRIPTION = new Dictionary<string, string> { { "AttackButton", "Hit one target" }, { "Skill", "Select a skill" } };

    public PlayerEvent _buttonResponseEvent { get; set; }

    public void Start()
    {
        _buttonResponseEvent = new PlayerEvent();
        _buttonResponseEvent.AddListener(PlayerEventHandler);
        _hoverPanel.gameObject.SetActive(false);
    }

    private void PlayerEventHandler(string eventValue)
    {
        switch (eventValue)
        {
            case "allDisable":
                _attackButton.interactable = false;
                break;
            case "allEnable":
                _attackButton.interactable = true;
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        HoverDisplay();
        UpdateTurnOrderDisplay();
        UpdateHealthBars(_battleManager._playerActorObjects);
        UpdateHealthBars(_battleManager._enemyActorObjects);
        UpdateDead(_battleManager._deadPlayerActorObjects);
        UpdateDead(_battleManager._deadEnemyActorObjects);
    }

    private void HoverDisplay()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray mouseRay = _camera.ScreenPointToRay(mousePosition);
        RaycastHit rayCollisionResult;
        var panelText = (Text)_hoverPanel.GetComponentInChildren<Text>();
        if (Physics.Raycast(mouseRay, out rayCollisionResult, 10000f, _descriptionLayer))
        {
            _hoverPanel.gameObject.SetActive(true);
            // Relying on anchor point for placement. Probably should have some screen edge detection
            _hoverPanel.transform.position = mousePosition;
            GameObject collidedObject = rayCollisionResult.transform.gameObject.transform.root.gameObject;
            var script = (BattleActorScript)collidedObject.GetComponent(typeof(BattleActorScript));
            if (!panelText)
            {
                Debug.Log("No text object on description panel");
                return;
            }

            switch (collidedObject.tag)
            {
                case "BattleActor":
                    panelText.text = script._battleActor.stats.longDescription;
                    break;
                case "Button":
                    panelText.text = BUTTONDESCRIPTION[collidedObject.name];
                    break;
                default:
                    _hoverPanel.gameObject.SetActive(false);
                    break;
            }
        }
        // UI elements can't be physics raycasted against
        else
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                var collidedObject = EventSystem.current.currentSelectedGameObject;
                switch (collidedObject.tag)
                {
                    case "Button":
                        panelText.text = BUTTONDESCRIPTION[collidedObject.name];
                        break;
                    default:
                        _hoverPanel.gameObject.SetActive(false);
                        break;
                }
            }
            else
                _hoverPanel.gameObject.SetActive(false);
        }

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
            // Vector3 inTheGround = new Vector3(actorObject.transform.position.x, 0, actorObject.transform.position.z);
            // actorObject.transform.position = inTheGround;
            var actorScript = (BattleActorScript)actorObject.GetComponent(typeof(BattleActorScript));
            actorScript.SetDead();
        }
    }

    private string GetDisplayName(GameObject obj)
    {
        var script = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
        return script._battleActor.stats.displayName;
    }
}
