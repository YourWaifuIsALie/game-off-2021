using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class BattleManagerScript : MonoBehaviour
{
    private Dictionary<string, BattleTag> _allTags;
    private Dictionary<string, IBattleEffect> _allEffects;
    private Dictionary<string, IBattleAction> _allActions;
    private Dictionary<string, IBattleActor> _allActors;
    private Dictionary<string, IAttackDamageEffect> _attackDamageEffects;

    private string DATAPATH = Path.Combine(Application.streamingAssetsPath, "Data");

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

        // TODO remove condition testing
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
            // createdObject.stats.effectValues = FixDeserialize(createdObject.stats.effectValues);
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
}