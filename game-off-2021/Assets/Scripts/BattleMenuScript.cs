using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BattleMenuScript : MonoBehaviour
{
    [SerializeField]
    private BattleManagerScript _battleManager;

    [SerializeField]
    private TextMeshProUGUI _turnOrderDisplay;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Button _attackButton;

    [SerializeField]
    private LayerMask _descriptionLayer;

    [SerializeField]
    private CanvasRenderer _hoverPanel;

    [SerializeField]
    private GameObject _buttonPrefab;

    [SerializeField]
    private GameObject _skillMenuCanvas;

    [SerializeField]
    private Text _currentActionText;

    [SerializeField]
    private BugManagerScript _bugManager;

    // Global namespace please
    private Dictionary<string, Type> TAGTYPE = new Dictionary<string, Type> { { "BattleActor", typeof(BattleActorScript) } };

    // This should probably just be taken from the action description. Oh well.
    private Dictionary<string, string> BUTTONDESCRIPTION = new Dictionary<string, string> {
        { "AttackButton", "Deal 5 damage to a single target" },
        { "SkillButton", "Choose to use a special skill" },
        { "basicHeal", "Heal one target by 5 health" },
        { "byteSwap", "DEBUG: Swap health endianess\n// why even have two formats? ffs"} }
    ;

    public PlayerEvent _buttonResponseEvent { get; set; }
    public PlayerEvent _animationEvent { get; set; }

    private List<GameObject> _skillButtons;

    public void Start()
    {
        _buttonResponseEvent = new PlayerEvent();
        _buttonResponseEvent.AddListener(PlayerEventHandler);
        _hoverPanel.gameObject.SetActive(false);

        _skillButtons = new List<GameObject>();
    }

    private void PlayerEventHandler(string eventValue)
    {
        switch (eventValue)
        {
            case "allDisable":
                _attackButton.interactable = false;
                foreach (Button obj in _skillMenuCanvas.GetComponentsInChildren<Button>())
                    if (obj.name != "Back")
                        obj.interactable = false;
                break;
            case "allEnable":
                _attackButton.interactable = true;
                foreach (Button obj in _skillMenuCanvas.GetComponentsInChildren<Button>())
                    if (obj.name != "Back")
                        obj.interactable = true;
                break;
            default:
                break;
        }
    }

    public void LateUpdate()
    {
        HoverDisplay();
        UpdateTurnOrderDisplay();
        UpdateHealthBars(_battleManager._playerActorObjects);
        UpdateHealthBars(_battleManager._enemyActorObjects);
        UpdateHealthBars(_battleManager._deadEnemyActorObjects);
        UpdateHealthBars(_battleManager._deadPlayerActorObjects);
        UpdateDead(_battleManager._deadPlayerActorObjects);
        UpdateDead(_battleManager._deadEnemyActorObjects);
        if (_battleManager._currentSelectedAction != null)
            _currentActionText.text = _battleManager._currentSelectedAction.stats.displayName;
        else
            _currentActionText.text = " ";
        // Debug.Log($"Bug level: {_bugManager.bugLevel}");
    }

    private void HoverDisplay()
    {
        // TODO anchor hover display on object so play can mouse over to get further description on keywords
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
                    var stringBuild = "";
                    //Level
                    stringBuild += $"Level {script._battleActor.stats.level}\n";
                    // Tags
                    if (script._battleActor.stats.tags.Count > 0)
                    {
                        foreach (KeyValuePair<string, BattleTag> element in script._battleActor.stats.tags)
                            stringBuild += $"[{element.Value.displayName}] ";
                        stringBuild += "\n";
                    }
                    // Stats
                    stringBuild += $"ATK: {script._battleActor.stats.currentAttack} | DEF: {script._battleActor.stats.currentDefense}";
                    //Description
                    if (_battleManager._currentPlayerAction == null)
                        stringBuild += $"\n---\n{script._battleActor.stats.longDescription}";
                    panelText.text = stringBuild;
                    break;
                default:
                    _hoverPanel.gameObject.SetActive(false);
                    break;
            }
        }
        // UI elements can't be physics raycasted against
        else
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> interfaceRays = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, interfaceRays);
            if (interfaceRays.Count == 1)
            {
                _hoverPanel.gameObject.SetActive(true);
                _hoverPanel.transform.position = Input.mousePosition;
                RaycastResult interfaceRay = interfaceRays[0];

                switch (interfaceRay.gameObject.tag)
                {
                    case "Button":
                        panelText.text = BUTTONDESCRIPTION[interfaceRay.gameObject.name];
                        break;
                    case "TurnOrder":
                        panelText.text = $"The unit turn order. Current unit's turn is bolded";
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
                turnOrderString += " <b>{" + displayName + "}</b> ";
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

    public void GenerateSkillButtons(List<IBattleAction> actionList)
    {
        float xLocation = 450;
        float yLocation = 40;
        // Cull old buttons
        if (_skillButtons != null && _skillButtons.Count > 0)
            foreach (GameObject obj in _skillButtons)
                Destroy(obj);
        _skillButtons = new List<GameObject>();

        foreach (var action in actionList)
        {
            if (action.stats.name != "basicAttack")
            {
                GameObject obj = (GameObject)Instantiate(_buttonPrefab, new Vector3(xLocation, yLocation, 0), Quaternion.identity);
                obj.transform.SetParent(_skillMenuCanvas.transform, false);
                obj.name = action.stats.name;
                _skillButtons.Add(obj);

                BattleMenuButtonScript script = obj.GetComponent<BattleMenuButtonScript>();
                script._battleManager = _battleManager;
                script._currentMenu = _skillMenuCanvas;
                script._otherBackButton = null;

                Button button = obj.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                UnityEngine.Events.UnityAction buttonCallback = () => script.InvokeEvent(action.stats.name);
                button.onClick.AddListener(buttonCallback);

                Text buttonText = obj.GetComponentInChildren<Text>();
                buttonText.text = action.stats.displayName;
                xLocation += 100;
            }
        }
    }
}
