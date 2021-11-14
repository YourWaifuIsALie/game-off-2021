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

    private Dictionary<string, BattleTag> _allTags;
    private Dictionary<string, IBattleEffect> _allEffects;
    private Dictionary<string, IBattleAction> _allActions;
    private Dictionary<string, IBattleActor> _allActors;
    private Dictionary<string, IAttackDamageEffect> _attackDamageEffects;

    private static string DATAPATH = Path.Combine(Application.streamingAssetsPath, "Data");
    private static string BATTLEPATH = Path.Combine(DATAPATH, "Battles");

    enum BattleState { Inactive, Start, Input, Waiting, Cleanup }
    private BattleState _currentState;

    private List<GameObject> _playerActorObjects, _enemyActorObjects;

    private List<GameObject> _currentTurnOrder, _nextTurnOrder;

    public void Start()
    {
        _allTags = new Dictionary<string, BattleTag>();
        _allEffects = new Dictionary<string, IBattleEffect>();
        _allActions = new Dictionary<string, IBattleAction>();
        _allActors = new Dictionary<string, IBattleActor>();
        _attackDamageEffects = new Dictionary<string, IAttackDamageEffect>();
        LoadTags();
        LoadEffects();
        LoadActions();
        LoadActors();

        _playerActorObjects = new List<GameObject>();
        _enemyActorObjects = new List<GameObject>();
        _playerActorObjects.Add(CreateActorObject(_allActors["mainCharacter"]));

        _currentState = BattleState.Inactive;
    }

    public void Update()
    {
        switch (_currentState)
        {
            case BattleState.Inactive:
                break;
            case BattleState.Start:
                TransitionIn();
                break;
            case BattleState.Input:
                GetInput();
                break;
            case BattleState.Waiting:
                ExecuteTurn();
                break;
            case BattleState.Cleanup:
                SettleTurn();
                break;
            default:
                break;
        }
    }

    public void SetupBattle(string inputPath)
    {
        _enemyActorObjects = new List<GameObject>();
        string battleFilePath = inputPath;
        if (!Path.HasExtension(battleFilePath))
            battleFilePath = battleFilePath + ".json";
        string filein = File.ReadAllText(Path.Combine(BATTLEPATH, battleFilePath));
        BattleSetup battleSetup = JsonConvert.DeserializeObject<BattleSetup>(filein);
        BattleSetupFactory.make(battleSetup, _allActors);

        foreach (IBattleActor enemyActor in battleSetup.enemies)
            _enemyActorObjects.Add(CreateActorObject(enemyActor));

        SetupActorLocations();
        _nextTurnOrder = _currentTurnOrder = DetermineTurnOrder();

        Debug.Log("Starting battle: " + battleSetup.displayName + " (" + battleSetup.type + ")");
        //Debug.Log(JsonConvert.SerializeObject(battleSetup.enemies));

        _currentState = BattleState.Start;
    }

    private GameObject CreateActorObject(IBattleActor actor)
    {
        GameObject obj = (GameObject)Instantiate(_actorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        var script = (BattleActorScript)obj.GetComponent(typeof(BattleActorScript));
        script._battleActor = actor;
        obj.name = actor.stats.name;
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
            var script = (BattleActorScript)actor.GetComponent(typeof(BattleActorScript));
            BattleActor battleActor = script._battleActor.stats;

            if (priorityQueue.TryGetValue(battleActor.currentSpeed, out value))
                value.Add(actor);
            else
                priorityQueue.Add(battleActor.currentSpeed, new List<GameObject> { actor });
        }

        foreach (var actor in _enemyActorObjects)
        {
            var script = (BattleActorScript)actor.GetComponent(typeof(BattleActorScript));
            BattleActor battleActor = script._battleActor.stats;
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
            new Vector3(-15, 2, 30),
            new Vector3(-25, 2, 15),
            new Vector3(-25, 2, 5),
            new Vector3(-25, 2, 30)
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
            new Vector3(15, 2, 30),
            new Vector3(25, 2, 15),
            new Vector3(25, 2, 5),
            new Vector3(25, 2, 30)
        };

        i = 0;
        foreach (var actor in _enemyActorObjects)
        {
            actor.transform.position = enemyLocations[i];
            i++;
        }
    }

    private void TransitionIn()
    {
        // TODO some fancy animation
        _currentState = BattleState.Input;
    }

    private void GetInput()
    {
        _currentState = BattleState.Waiting;
    }

    private void ExecuteTurn()
    {
        _currentState = BattleState.Cleanup;
    }

    private void SettleTurn()
    {
        // If end condition
        bool isBattleOver = false;
        if (isBattleOver)
        {
            EndBattle();
        }
        else
        {
            // TODO compare planned turn order vs. next turn order
            _nextTurnOrder = DetermineTurnOrder();

            // TODO advance turn or change turn order if at end
            _currentState = BattleState.Input;
        }

    }

    private void EndBattle()
    {
        _currentState = BattleState.Inactive;
    }

    // TODO
    // Start Battle (also from json, random battle would just generate json in memory?)
    // FSM - Start, Input (player or AI), Action, Cleanup
    // Turn tracking
    // Turn order
    // UI

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
            dynamic createdObject = BattleActionFactory.make(element, _allEffects);
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
            dynamic createdObject = BattleActorFactory.make(element, _allActions, _allTags);
            if (createdObject is IBattleActor)
                _allActors[createdObject.stats.name] = createdObject;
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

    private void RunUnitTests()
    {
        //TODO figure out actual unit testing mechanism
        var actorOrigin = _allActors["rubberActor"];
        var actorTarget = new List<IBattleActor> { _allActors["glueActor"] };
        actorOrigin.stats.actions["logicTest1"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["glueActor"];
        actorTarget = new List<IBattleActor> { _allActors["rubberActor"] };
        actorOrigin.stats.actions["logicTest1"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["glueActor"];
        actorTarget = new List<IBattleActor> { _allActors["glueActor"] };
        actorOrigin.stats.actions["logicTest1"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["rubberActor"];
        actorTarget = new List<IBattleActor> { _allActors["rubberActor"] };
        actorOrigin.stats.actions["logicTest1"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["positiveActor"];
        actorTarget = new List<IBattleActor> { _allActors["negativeActor"] };
        actorOrigin.stats.actions["logicTest2"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["negativeActor"];
        actorTarget = new List<IBattleActor> { _allActors["positiveActor"] };
        actorOrigin.stats.actions["logicTest2"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["negativeActor"];
        actorTarget = new List<IBattleActor> { _allActors["negativeActor"] };
        actorOrigin.stats.actions["logicTest2"].act(actorOrigin, actorTarget);

        actorOrigin = _allActors["positiveActor"];
        actorTarget = new List<IBattleActor> { _allActors["positiveActor"] };
        actorOrigin.stats.actions["logicTest2"].act(actorOrigin, actorTarget);
    }
}