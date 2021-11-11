using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
}
