using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class BattleManagerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject _actorPrefab;

    [SerializeField]
    private GameObject _damagePopupPrefab;

    [SerializeField]
    private GameObject _battleMenu;

    [SerializeField]
    private GameObject _defeatMenu;

    [SerializeField]
    private Camera _battleCamera;

    [SerializeField]
    private LayerMask _selectableLayer;

    [SerializeField]
    private List<GameObject> _ignoreSelectionList;

    [SerializeField]
    private GameObject _levelLoader;

    [SerializeField]
    private BugManagerScript _bugManager;

    // TODO proper loading of arbitrary assets
    [SerializeField]
    private List<Sprite> _allSprites;
    [SerializeField]
    private List<RuntimeAnimatorController> _allAnimators;


    public PlayerEvent _playerInputEvent { get; set; }
    public PlayerEvent _animationEvent { get; set; }

    private System.Random _rng;

    private Dictionary<string, BattleTag> _allTags;
    private Dictionary<string, IBattleEffect> _allEffects;
    private Dictionary<string, IBattleAction> _allActions;
    private Dictionary<string, BattleActor> _allActors;
    private Dictionary<string, IAttackDamageEffect> _attackDamageEffects;

    private static string DATAPATH = Path.Combine(Application.streamingAssetsPath, "Data");
    private static string BATTLEPATH = Path.Combine(DATAPATH, "Battles");

    enum BattleState { Inactive, Start, Input, Action, Waiting, WaitingStart, WaitingEnd, Cleanup, End }
    private BattleState _currentState;

    public List<GameObject> _playerActorObjects { get; set; }
    public List<GameObject> _enemyActorObjects { get; set; }
    public List<GameObject> _deadPlayerActorObjects { get; set; }
    public List<GameObject> _deadEnemyActorObjects { get; set; }
    public List<GameObject> _currentTurnOrder { get; set; }
    public List<GameObject> _nextTurnOrder { get; set; }
    private int _currentActorIndex;
    public string _currentPlayerAction { get; set; }
    public IBattleAction _currentSelectedAction { get; set; }
    public bool _waitingForPlayerSelection { get; set; }
    private GameObject _currentPlayerSelected;
    private IBattleAction _currentActorAction;
    private string _currentBattle;

    public GameObject _currentActor { get; set; }
    public GameObject _currentTargetActor { get; set; }

    // TODO proper relative postions based on field/camera position/screen size
    // (0,2,5) = front middle, (0,2,30) = back middle, (-15, 2, 15) = left middle
    Vector3[] playLocations = new Vector3[] {
            new Vector3(-15, 2, 15),
            new Vector3(-15, 2, 5),
            new Vector3(-10, 2, 20),
            new Vector3(-25, 2, 15),
            new Vector3(-25, 2, 5),
            new Vector3(-20, 2, 20)
        };

    // TODO implement math for position
    Vector3[] enemyLocations = new Vector3[] {
            new Vector3(15, 2, 15),
            new Vector3(15, 2, 5),
            new Vector3(10, 2, 20),
            new Vector3(25, 2, 15),
            new Vector3(25, 2, 5),
            new Vector3(20, 2, 20)
        };

    public void Start()
    {
        _rng = new System.Random();

        _allTags = new Dictionary<string, BattleTag>();
        _allEffects = new Dictionary<string, IBattleEffect>();
        _allActions = new Dictionary<string, IBattleAction>();
        _allActors = new Dictionary<string, BattleActor>();
        _attackDamageEffects = new Dictionary<string, IAttackDamageEffect>();
        LoadTags();
        LoadEffects();
        LoadActions();
        LoadActors();

        _playerActorObjects = new List<GameObject>();
        _enemyActorObjects = new List<GameObject>();
        _deadPlayerActorObjects = new List<GameObject>();
        _deadEnemyActorObjects = new List<GameObject>();

        _playerInputEvent = new PlayerEvent();
        _playerInputEvent.AddListener(PlayerEventHandler);
        _animationEvent = new PlayerEvent();
        _animationEvent.AddListener(AnimationEventHandler);

        _battleMenu.SetActive(false);
        _defeatMenu.SetActive(false);

        _waitingForPlayerSelection = true;
        _currentPlayerAction = null;
        _currentSelectedAction = null;

        _currentActor = null;
        _currentTargetActor = null;

        _currentState = BattleState.Inactive;

        SetupBattle("Battle0");
    }

    private void PlayerEventHandler(string eventValue)
    {
        _currentPlayerAction = eventValue;
        // Debug.Log($"Player event received: {eventValue}");
    }
    private void AnimationEventHandler(string eventValue)
    {
        switch (eventValue)
        {
            case "Hurt":
                BattleActorScript actor = GetScriptComponent<BattleActorScript>(_currentTargetActor);
                actor.PlayAnimation("Hurt");
                break;
            default:
                break;
        }
    }


    public void Update()
    {
        switch (_currentState)
        {
            case BattleState.Inactive:
                break;
            case BattleState.Start:
                StartBattle();
                break;
            case BattleState.Input:
                GetInput();
                break;
            case BattleState.Action:
                ExecuteTurn();
                break;
            case BattleState.Waiting:
                break;
            case BattleState.WaitingStart:
            case BattleState.WaitingEnd:
                UpdatePlayerLocation();
                break;
            case BattleState.Cleanup:
                SettleTurn();
                break;
            case BattleState.End:
                LoadNextBattle();
                break;
            default:
                break;
        }
    }

    public void SetupBattle(string inputPath)
    {
        // Cull all objects for safety
        if (_enemyActorObjects != null && _enemyActorObjects.Count > 0)
            foreach (GameObject obj in _enemyActorObjects)
                Destroy(obj);
        if (_deadEnemyActorObjects != null && _deadEnemyActorObjects.Count > 0)
            foreach (GameObject obj in _deadEnemyActorObjects)
                Destroy(obj);
        if (_deadPlayerActorObjects != null && _deadPlayerActorObjects.Count > 0)
            foreach (GameObject obj in _deadPlayerActorObjects)
                Destroy(obj);
        if (_playerActorObjects != null && _playerActorObjects.Count > 0)
            foreach (GameObject obj in _playerActorObjects)
                Destroy(obj);

        _enemyActorObjects = new List<GameObject>();
        _deadEnemyActorObjects = new List<GameObject>();
        _deadPlayerActorObjects = new List<GameObject>();
        _playerActorObjects = new List<GameObject>();

        // Recreate player everytime to avoid issues
        dynamic createdObject = BattleActorFactory.Make(_allActors["mainCharacter"], _allActions, _allTags);
        _playerActorObjects.Add(CreateActorObject(createdObject, false));

        // Load battle file
        _currentBattle = inputPath;
        string battleFilePath = inputPath;
        if (!Path.HasExtension(battleFilePath))
            battleFilePath = battleFilePath + ".json";
        string filein = File.ReadAllText(Path.Combine(BATTLEPATH, battleFilePath));
        BattleSetup battleSetup = JsonConvert.DeserializeObject<BattleSetup>(filein);
        BattleSetupFactory.Make(battleSetup, _allActors, _allActions, _allTags);

        foreach (IBattleActor enemyActor in battleSetup.enemies)
            _enemyActorObjects.Add(CreateActorObject(enemyActor, true));

        SetupActorLocations();
        _nextTurnOrder = _currentTurnOrder = DetermineTurnOrder();
        _currentActorIndex = 0;
        _currentActor = _currentTurnOrder[_currentActorIndex];

        // Change skill buttons
        Dictionary<string, IBattleAction> possibleActionsDict = GetScriptComponent<BattleActorScript>(_playerActorObjects[0])._battleActor.stats.actions;
        GetScriptComponent<BattleMenuScript>(_battleMenu).GenerateSkillButtons(possibleActionsDict.Values.ToList());

        _battleMenu.SetActive(true);

        Debug.Log("Starting battle: " + battleSetup.displayName + " (" + battleSetup.type + ")");
        _currentState = BattleState.Start;
    }

    private void LoadNextBattle()
    {
        // Victory
        if (_playerActorObjects.Count != 0)
        {
            SetupBattle("Battle0");
        }
        else
        {
            _battleMenu.SetActive(false);
            _defeatMenu.SetActive(true);
        }

    }

    public void RetryBattle()
    {
        SetupBattle(_currentBattle);
    }

    private GameObject CreateActorObject(IBattleActor actor, bool isFlipped)
    {
        GameObject obj = (GameObject)Instantiate(_actorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        var script = GetScriptComponent<BattleActorScript>(obj);
        script._battleActor = actor;
        obj.name = actor.stats.name;
        script._battleManager = this;
        var graphicScript = (BattleActorGraphicScript)script.battleActorGraphics.GetComponent(typeof(BattleActorGraphicScript));
        graphicScript.SetGraphics(_allSprites[0], _allAnimators[0]);
        graphicScript.isFlipped = isFlipped;
        return obj;
    }

    private List<GameObject> DetermineTurnOrder()
    {
        var output = new List<GameObject>();
        // OrderedList in descending order
        var priorityQueue = new SortedList<int, List<GameObject>>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        List<GameObject> value;
        foreach (var actor in _playerActorObjects)
        {
            BattleActor battleActor = GetScriptComponent<BattleActorScript>(actor)._battleActor.stats;

            if (priorityQueue.TryGetValue(battleActor.currentSpeed, out value))
                value.Add(actor);
            else
                priorityQueue.Add(battleActor.currentSpeed, new List<GameObject> { actor });
        }

        foreach (var actor in _enemyActorObjects)
        {
            BattleActor battleActor = GetScriptComponent<BattleActorScript>(actor)._battleActor.stats;

            if (priorityQueue.TryGetValue(battleActor.currentSpeed, out value))
                value.Add(actor);
            else
                priorityQueue.Add(battleActor.currentSpeed, new List<GameObject> { actor });
        }

        foreach (KeyValuePair<int, List<GameObject>> item in priorityQueue)
        {
            // Debug.Log($"Item: {item.Value} with priority: {item.Key}");
            // TODO properly randomize within each bucket so that the randomization doesn't change if all actors are the same
            foreach (GameObject obj in item.Value)
            {
                output.Add(obj);
            }
        }
        return output;
    }

    private void SetupActorLocations()
    {
        var i = 0;
        foreach (var actor in _playerActorObjects)
        {
            actor.transform.position = playLocations[i];
            i++;
        }

        i = 0;
        foreach (var actor in _enemyActorObjects)
        {
            actor.transform.position = enemyLocations[i];
            i++;
        }
    }

    private IEnumerator TransitionIn()
    {
        // It's a real mess without a defined animation manager eh?
        // I assume this should be a centrally-managed system of animation events and callbacks
        // 
        // If I was smart I would have tied the running animation to the state of the actor object
        // and let the change in position cause the run. Instead we have this manually timed mess.
        int i = 0;
        foreach (var obj in _playerActorObjects)
        {
            BattleActorScript battleActor = GetScriptComponent<BattleActorScript>(obj);
            battleActor.PlayAnimation("Run");
            Vector3 startLocation = playLocations[i];
            startLocation.x -= 20;
            obj.transform.position = startLocation;
            i += 1;
        }
        yield return new WaitForSeconds(1);
        foreach (var obj in _playerActorObjects)
        {
            BattleActorScript battleActor = GetScriptComponent<BattleActorScript>(obj);
            battleActor.PlayAnimation("Stop");
        }
        yield return new WaitForSeconds(1);
        _currentState = BattleState.Input;
    }

    private IEnumerator TransitionOut()
    {
        foreach (var obj in _playerActorObjects)
        {
            BattleActorScript battleActor = GetScriptComponent<BattleActorScript>(obj);
            battleActor.PlayAnimation("Run");
        }
        yield return new WaitForSeconds(0.35f);
        _currentState = BattleState.WaitingEnd;
        yield return new WaitForSeconds(0.65f);
        var script = (LevelLoaderScript)_levelLoader.GetComponent(typeof(LevelLoaderScript));
        StartCoroutine(script.FadeScreen());
        yield return new WaitForSeconds(1); // Wait for screen black before continuing load

        _currentState = BattleState.End;
    }

    private void UpdatePlayerLocation()
    {
        if (_currentState == BattleState.WaitingStart)
        {
            int i = 0;
            foreach (var obj in _playerActorObjects)
            {
                obj.transform.position = Vector3.Lerp(obj.transform.position, playLocations[i], 1 * Time.deltaTime);
                i += 1;
            }

        }
        else if (_currentState == BattleState.WaitingEnd)
        {
            int i = 0;
            foreach (var obj in _playerActorObjects)
            {
                Vector3 endLocation = playLocations[i];
                endLocation.x += 10;
                obj.transform.position = Vector3.Lerp(obj.transform.position, endLocation, 0.4f * Time.deltaTime);
                i += 1;
            }
        }
    }

    private void GetInput()
    {
        // True = cancel action/go back
        // Reset selection if player clicks away
        if (PlayerSelectionUpdate())
        {
            ResetPlayerActionState();
            DeselectPlayerSelection();
            return;
        }

        // If player has selected an action, disable buttons
        if (_waitingForPlayerSelection && _currentPlayerAction != null)
        {
            GetScriptComponent<BattleMenuScript>(_battleMenu)._buttonResponseEvent.Invoke("allDisable");
            _waitingForPlayerSelection = false;
        }

        // Player turn
        if (_playerActorObjects.Contains(_currentActor))
        {
            if (_currentPlayerAction != null)
            {
                Dictionary<string, IBattleAction> possibleActionsDict = GetScriptComponent<BattleActorScript>(_currentActor)._battleActor.stats.actions;
                if (possibleActionsDict.ContainsKey(_currentPlayerAction))
                {
                    _currentActorAction = possibleActionsDict[_currentPlayerAction];
                    _currentSelectedAction = _currentActorAction;
                    if (_currentPlayerSelected != null)
                    {
                        // TODO Allow selection/targeting of non-enemy objects
                        if (_enemyActorObjects.Contains(_currentPlayerSelected))
                        {
                            _currentTargetActor = _enemyActorObjects.Find(x => x.GetInstanceID() == _currentPlayerSelected.GetInstanceID());
                            if (_currentTargetActor != null)
                            {
                                _currentPlayerAction = null;
                                _currentState = BattleState.Action;
                            }

                        }
                        else if (_playerActorObjects.Contains(_currentPlayerSelected))
                        {
                            // Repeated code but whatever we're in quick and dirty season
                            _currentTargetActor = _playerActorObjects.Find(x => x.GetInstanceID() == _currentPlayerSelected.GetInstanceID());
                            if (_currentTargetActor != null)
                            {
                                _currentPlayerAction = null;
                                _currentState = BattleState.Action;
                            }
                        }
                        else
                        {
                            ResetPlayerActionState();
                            DeselectPlayerSelection();
                            return;
                        }
                    }
                }
                else
                {
                    _currentPlayerAction = null;
                    Debug.Log("Illegal player action");
                }

            }
            // Debug.Log($"Player turn for: {_currentActor.name}");
        }
        else
        {
            // Debug.Log($"Enemy turn for: {_currentActor.name}");

            // Randomly decide action
            Dictionary<string, IBattleAction> possibleActionsDict = GetScriptComponent<BattleActorScript>(_currentActor)._battleActor.stats.actions;
            List<String> possibleActionsStr = new List<string>(possibleActionsDict.Keys);
            string randomKey = possibleActionsStr[_rng.Next(possibleActionsStr.Count)];
            _currentActorAction = possibleActionsDict[randomKey];

            // TODO add logic for valid target selection
            // Randomly decide target
            int randomIndex = _rng.Next(_playerActorObjects.Count);
            _currentTargetActor = _playerActorObjects[randomIndex];

            _currentState = BattleState.Action;
        }
    }

    private void ResetPlayerActionState()
    {
        _currentPlayerAction = null;
        _currentSelectedAction = null;
        _waitingForPlayerSelection = true;
        GetScriptComponent<BattleMenuScript>(_battleMenu)._buttonResponseEvent.Invoke("allEnable");

    }

    private bool PlayerSelectionUpdate()
    {
        // Sets _currentPlayerSelected directly
        // Return value is to check for "empty click go back" functionality
        // true = go back, false = no action/proper selection
        // TODO options variable to rebind keys
        // TODO add keyboard selection
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseClickPosition = Input.mousePosition;
            Ray clickRay = _battleCamera.ScreenPointToRay(mouseClickPosition);
            RaycastHit rayCollisionResult;
            if (Physics.Raycast(clickRay, out rayCollisionResult, 10000f, _selectableLayer))
            {
                GameObject collidedObject = rayCollisionResult.transform.gameObject.transform.root.gameObject;
                if (!_ignoreSelectionList.Contains(collidedObject))
                {
                    DeselectPlayerSelection();
                    _currentPlayerSelected = collidedObject;
                    // Debug.Log($"Player selected: {_currentPlayerSelected.name}");

                    BattleActorScript selectedScript = GetScriptComponent<BattleActorScript>(_currentPlayerSelected);
                    selectedScript.SetSelected(true);
                    return false;
                }
                else
                {
                    // Did player click UI element?
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.position = Input.mousePosition;
                    List<RaycastResult> interfaceRays = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, interfaceRays);
                    if (interfaceRays.Count > 0)
                    {
                        // Debug.Log($"False: {interfaceRays.Count}");
                        return false;
                    }
                    else
                    {
                        // Debug.Log($"True: {interfaceRays.Count}");
                        return true;
                    }
                }
            }
            else
                return true;
        }
        return false;
    }

    private void DeselectPlayerSelection()
    {
        if (_currentPlayerSelected)
        {
            BattleActorScript oldSelected = GetScriptComponent<BattleActorScript>(_currentPlayerSelected);
            oldSelected.SetSelected(false);
            _currentPlayerSelected = null;
        }
    }

    private void ExecuteTurn()
    {
        StartCoroutine(ArtificialSlowdown(1.5f));
        _currentState = BattleState.Waiting;
    }

    public IEnumerator ArtificialSlowdown(float seconds)
    {
        // Debug.Log($"[{_currentActor.name}] is [{_currentActorAction.stats.displayName}ing] [{_currentTargetActor.name}]");
        BattleActorScript originActor = GetScriptComponent<BattleActorScript>(_currentActor);
        var origin = originActor._battleActor;
        var target = GetScriptComponent<BattleActorScript>(_currentTargetActor)._battleActor;

        originActor.PlayAnimation("Attack");
        yield return new WaitForSeconds(seconds);

        var damage = _currentActorAction.Act(origin, new List<IBattleActor> { target });
        // Popup text here instead of via animation event because we're laaaazy
        Vector3 textPosition = _currentTargetActor.transform.position;
        textPosition.y += 4;
        DamagePopupScript temp = Instantiate(_damagePopupPrefab, textPosition, Quaternion.identity).GetComponent<DamagePopupScript>();
        temp.SetDamage(-damage); // Negate for more sensible popup display
        if (damage > 0)
            temp.SetColor(Color.red);
        else if (damage < 0)
            temp.SetColor(Color.green);
        else
            temp.SetColor(Color.gray);

        _currentState = BattleState.Cleanup;
    }

    public T GetScriptComponent<T>(GameObject obj) where T : MonoBehaviour
    {
        // Mostly unnecessary because `T variable = obj.GetComponent<T>()` is pretty short
        return (T)obj.GetComponent(typeof(T));
    }

    private void SettleTurn()
    {
        // If end condition
        bool isBattleOver = false;
        CheckAndRemoveDeadObjects(_playerActorObjects, _deadPlayerActorObjects);
        CheckAndRemoveDeadObjects(_enemyActorObjects, _deadEnemyActorObjects);
        if (_playerActorObjects.Count == 0 || _enemyActorObjects.Count == 0)
            isBattleOver = true;

        if (isBattleOver)
        {
            EndBattle();
        }
        else
        {
            // TODO compare planned turn order vs. next turn order
            // TODO graphical event system for recalculated turn order animation?
            _nextTurnOrder = DetermineTurnOrder();
            _currentTurnOrder = _nextTurnOrder; // TODO to account for dead actors

            if (_playerActorObjects.Contains(_currentActor))
                ResetPlayerActionState();

            _currentActorIndex += 1;
            if (_currentActorIndex >= _currentTurnOrder.Count)
            {
                _currentTurnOrder = _nextTurnOrder;
                _currentActorIndex = 0;
            }
            _currentActor = _currentTurnOrder[_currentActorIndex];
            _currentState = BattleState.Input;
        }

    }

    private void CheckAndRemoveDeadObjects(List<GameObject> livingList, List<GameObject> deadList)
    {
        foreach (var actorObject in livingList)
        {
            var actorScript = GetScriptComponent<BattleActorScript>(actorObject);
            var actor = actorScript._battleActor;
            if (!actor.stats.CheckAlive())
            {
                if (actor.stats.bugged == "integerOverflow")
                {
                    _bugManager.IntegerOverflow();
                }
                actorScript.PlayAnimation("Death");
                deadList.Add(actorObject);
            }
        }
        livingList.RemoveAll(x => GetScriptComponent<BattleActorScript>(x)._battleActor.stats.isAlive == false);
    }

    private void StartBattle()
    {
        Debug.Log("BATTLE START");
        StartCoroutine(TransitionIn());
        _currentState = BattleState.WaitingStart;
    }

    private void EndBattle()
    {
        ResetPlayerActionState();
        DeselectPlayerSelection();
        if (_playerActorObjects.Count == 0)
        {
            Debug.Log("YOU LOSE");
        }
        else
        {
            Debug.Log("VICTORY");
        }
        _currentState = BattleState.Waiting;
        StartCoroutine(TransitionOut());
    }

    // TODO generic loader + factory setup?
    private void LoadTags()
    {
        // TODO safety check on inputs please
        string filein = File.ReadAllText(Path.Combine(DATAPATH, "BattleTags.json"));
        List<BattleTag> elementList = JsonConvert.DeserializeObject<List<BattleTag>>(filein);

        foreach (var element in elementList)
        {
            if (element is BattleTag)
                _allTags[element.name] = element;
        }
    }

    private void LoadEffects()
    {
        // TODO safety check on inputs please
        string filein = File.ReadAllText(Path.Combine(DATAPATH, "BattleEffects.json"));
        List<BattleEffect> elementList = JsonConvert.DeserializeObject<List<BattleEffect>>(filein);

        foreach (var element in elementList)
        {
            dynamic createdObject = BattleEffectFactory.make(element);

            // TODO move in-line with the factory
            createdObject.stats.condition = FixDeserialize(createdObject.stats.condition);
            // Debug.Log(JsonConvert.SerializeObject(createdObject.stats.condition));

            if (createdObject is IBattleEffect)
                _allEffects[createdObject.stats.name] = createdObject;
            if (createdObject is IAttackDamageEffect)
                _attackDamageEffects[createdObject.stats.name] = createdObject;
        }
    }

    private void LoadActions()
    {
        // TODO safety check on inputs please
        string filein = File.ReadAllText(Path.Combine(DATAPATH, "BattleActions.json"));
        List<BattleAction> elementList = JsonConvert.DeserializeObject<List<BattleAction>>(filein);

        foreach (var element in elementList)
        {
            dynamic createdObject = BattleActionFactory.Make(element, _allEffects);
            if (createdObject is IBattleAction)
                _allActions[createdObject.stats.name] = createdObject;
        }
    }

    private void LoadActors()
    {
        // TODO safety check on inputs please
        string filein = File.ReadAllText(Path.Combine(DATAPATH, "BattleActors.json"));
        List<BattleActor> elementList = JsonConvert.DeserializeObject<List<BattleActor>>(filein);

        foreach (var element in elementList)
        {
            if (element is BattleActor)
                _allActors[element.name] = element;
        }
    }

    private Dictionary<string, dynamic> FixDeserialize(Dictionary<string, dynamic> dict)
    {
        // TODO move somewhere else
        // A function to recursively go into nested dictionaries to fix the typing
        // Faster to write this than figure out a "better" way to deserialize properly
        Dictionary<string, dynamic> outputDict = new Dictionary<string, dynamic>();
        foreach (KeyValuePair<string, dynamic> element in dict)
        {
            switch (element.Value)
            {
                // Every field value is a List of Dicts or strings
                case Newtonsoft.Json.Linq.JArray _:
                    List<dynamic> newList = new List<dynamic>();
                    foreach (var arrayElement in element.Value)
                    {
                        switch (arrayElement)
                        {
                            case Newtonsoft.Json.Linq.JObject _: // Dict
                                Dictionary<string, dynamic> deserializedObject = arrayElement.ToObject<Dictionary<string, object>>();
                                newList.Add(FixDeserialize(deserializedObject));
                                break;
                            case Newtonsoft.Json.Linq.JValue _: // String
                                newList.Add(arrayElement.ToString());
                                break;
                            default:
                                break;
                        }
                    }
                    outputDict[element.Key] = newList;
                    break;
                default:
                    Debug.Log(element.Key + ": " + element.Value);
                    Debug.Log("Non-list element as dictionary value");
                    break;
            }
        }
        return outputDict;

    }

    /*
    private void RunUnitTests()
    {
        //TODO figure out actual unit testing mechanism
        var actorOrigin = _allActors["rubberActor"];
        var actorTarget = new List<IBattleActor> { _allActors["glueActor"] };
        actorOrigin.stats.actions["logicTest1"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["glueActor"];
        actorTarget = new List<IBattleActor> { _allActors["rubberActor"] };
        actorOrigin.stats.actions["logicTest1"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["glueActor"];
        actorTarget = new List<IBattleActor> { _allActors["glueActor"] };
        actorOrigin.stats.actions["logicTest1"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["rubberActor"];
        actorTarget = new List<IBattleActor> { _allActors["rubberActor"] };
        actorOrigin.stats.actions["logicTest1"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["positiveActor"];
        actorTarget = new List<IBattleActor> { _allActors["negativeActor"] };
        actorOrigin.stats.actions["logicTest2"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["negativeActor"];
        actorTarget = new List<IBattleActor> { _allActors["positiveActor"] };
        actorOrigin.stats.actions["logicTest2"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["negativeActor"];
        actorTarget = new List<IBattleActor> { _allActors["negativeActor"] };
        actorOrigin.stats.actions["logicTest2"].Act(actorOrigin, actorTarget);

        actorOrigin = _allActors["positiveActor"];
        actorTarget = new List<IBattleActor> { _allActors["positiveActor"] };
        actorOrigin.stats.actions["logicTest2"].Act(actorOrigin, actorTarget);
    }
    */
}