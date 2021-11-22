using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class BattleManagerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject _actorPrefab;

    [SerializeField]
    private GameObject _battleMenu;

    [SerializeField]
    private Camera _battleCamera;

    [SerializeField]
    private LayerMask _selectableLayer;

    [SerializeField]
    private List<GameObject> _ignoreSelectionList;

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

    enum BattleState { Inactive, Start, Input, Action, Waiting, Cleanup, End }
    private BattleState _currentState;

    public List<GameObject> _playerActorObjects { get; set; }
    public List<GameObject> _enemyActorObjects { get; set; }
    public List<GameObject> _deadPlayerActorObjects { get; set; }
    public List<GameObject> _deadEnemyActorObjects { get; set; }
    public List<GameObject> _currentTurnOrder { get; set; }
    public List<GameObject> _nextTurnOrder { get; set; }
    private int _currentActorIndex;
    public string _currentPlayerAction { get; set; }
    public bool _waitingForPlayerSelection { get; set; }
    private GameObject _currentPlayerSelected;
    private IBattleAction _currentActorAction;

    public GameObject _currentActor { get; set; }
    public GameObject _currentTargetActor { get; set; }

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

        dynamic createdObject = BattleActorFactory.Make(_allActors["mainCharacter"], _allActions, _allTags);
        _playerActorObjects.Add(CreateActorObject(createdObject, false));

        _playerInputEvent = new PlayerEvent();
        _playerInputEvent.AddListener(PlayerEventHandler);
        _animationEvent = new PlayerEvent();
        _animationEvent.AddListener(AnimationEventHandler);

        _battleMenu.SetActive(false);

        _waitingForPlayerSelection = false;

        _currentActor = null;
        _currentTargetActor = null;

        _currentState = BattleState.Inactive;
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
        _enemyActorObjects = new List<GameObject>();
        _deadEnemyActorObjects = new List<GameObject>();
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

        _battleMenu.SetActive(true);

        Debug.Log("Starting battle: " + battleSetup.displayName + " (" + battleSetup.type + ")");
        //Debug.Log(JsonConvert.SerializeObject(battleSetup.enemies));

        _currentState = BattleState.Start;
    }

    private void LoadNextBattle()
    {
        if (_playerActorObjects.Count != 0)
        {
            foreach (GameObject obj in _enemyActorObjects)
                Destroy(obj);
            foreach (GameObject obj in _deadEnemyActorObjects)
                Destroy(obj);
            SetupBattle("TestBattle1");
        }
        else
            _battleMenu.SetActive(false);
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

        var i = 0;
        foreach (var actor in _playerActorObjects)
        {
            actor.transform.position = playLocations[i];
            i++;
        }

        // TODO implement math for position
        Vector3[] enemyLocations = new Vector3[] {
            new Vector3(15, 2, 15),
            new Vector3(15, 2, 5),
            new Vector3(10, 2, 20),
            new Vector3(25, 2, 15),
            new Vector3(25, 2, 5),
            new Vector3(20, 2, 20)
        };

        i = 0;
        foreach (var actor in _enemyActorObjects)
        {
            actor.transform.position = enemyLocations[i];
            i++;
        }
    }

    private IEnumerator TransitionIn()
    {
        // TODO some fancy animation
        foreach (var obj in _playerActorObjects)
        {
            BattleActorScript battleActor = GetScriptComponent<BattleActorScript>(obj);
            battleActor.PlayAnimation("Stop");
        }
        yield return new WaitForSeconds(0.5f);
        _currentState = BattleState.Input;
    }

    private IEnumerator TransitionOut()
    {
        // TODO some fancy animation
        foreach (var obj in _playerActorObjects)
        {
            BattleActorScript battleActor = GetScriptComponent<BattleActorScript>(obj);
            battleActor.PlayAnimation("Run");
        }
        yield return new WaitForSeconds(2);
        _currentState = BattleState.End;
    }

    private void GetInput()
    {
        if (_playerActorObjects.Contains(_currentActor))
        {
            if (_currentPlayerAction != null)
            {
                if (!_waitingForPlayerSelection)
                {
                    GetScriptComponent<BattleMenuScript>(_battleMenu)._buttonResponseEvent.Invoke("allDisable");
                    _waitingForPlayerSelection = true;
                }

                // True = cancel action/go back
                if (PlayerSelectionUpdate())
                {
                    ResetPlayerActionState();
                    return;
                }
                Dictionary<string, IBattleAction> possibleActionsDict = GetScriptComponent<BattleActorScript>(_currentActor)._battleActor.stats.actions;
                if (possibleActionsDict.ContainsKey(_currentPlayerAction))
                {
                    _currentActorAction = possibleActionsDict[_currentPlayerAction];

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
                            else
                            {
                                Debug.Log("Selected actor not found in enemy list");
                            }

                        }
                        else // Illegal selection
                        {
                            ResetPlayerActionState();
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
        DeselectPlayerSelection();
        _currentPlayerAction = null;
        _waitingForPlayerSelection = false;
        _currentPlayerSelected = null;
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
                    return true;
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
        StartCoroutine(ArtificialSlowdown(1));
        _currentState = BattleState.Waiting;
    }

    public IEnumerator ArtificialSlowdown(int seconds)
    {
        Debug.Log($"[{_currentActor.name}] is [{_currentActorAction.stats.displayName}ing] [{_currentTargetActor.name}]");
        BattleActorScript originActor = GetScriptComponent<BattleActorScript>(_currentActor);
        var origin = originActor._battleActor;
        var target = GetScriptComponent<BattleActorScript>(_currentTargetActor)._battleActor;

        originActor.PlayAnimation("Attack");
        _currentActorAction.Act(origin, new List<IBattleActor> { target });
        yield return new WaitForSeconds(seconds);
        _currentState = BattleState.Cleanup;
    }

    public T GetScriptComponent<T>(GameObject obj) where T : MonoBehaviour
    {
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

            _currentActorIndex += 1;
            if (_currentActorIndex >= _currentTurnOrder.Count)
            {
                _currentTurnOrder = _nextTurnOrder;
                _currentActorIndex = 0;
            }
            _currentActor = _currentTurnOrder[_currentActorIndex];
            ResetPlayerActionState();
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
        _currentState = BattleState.Waiting;
    }

    private void EndBattle()
    {
        ResetPlayerActionState();
        if (_playerActorObjects.Count == 0)
        {
            Debug.Log("YOU LOSE");
        }
        else
        {
            Debug.Log("VICTORY");
        }
        StartCoroutine(TransitionOut());
        _currentState = BattleState.Waiting;
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