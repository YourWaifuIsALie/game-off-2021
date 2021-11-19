using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

using UnityEngine;

public class BattleEffect
{
    // Not a superclass or interface because I'm too lazy to figure out the property way to serialize/deserialize
    // I'm sure this is a solved problem
    public static string ADD = "add";
    public static string MUL = "mul";

    // JSON serialization
    public string name { get; set; }
    public string displayName { get; set; }
    public string type { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }
    public Dictionary<string, dynamic> effectValues { get; set; }
    public Dictionary<string, dynamic> condition { get; set; }

    public static BattleEffect deserialize(string jsonString)
    {
        return JsonConvert.DeserializeObject<BattleEffect>(jsonString);
    }

    public bool evaluateCondition(IBattleActor origin, IBattleActor target)
    {
        // 4 keywords - "and, or, origin, target"
        // Value is always a list of dicts
        // If value is and/or, logic applies to all fields in dicts within the list until another and/or is reached

        // If no top-level and/or, use implicit and
        return evaluateConditionRecur(origin.stats, target.stats, condition, "and");
    }

    public bool evaluateConditionRecur(BattleActor origin, BattleActor target, Dictionary<string, dynamic> tree, string lastCall)
    {
        // TODO cleanup and optimize
        // Debug.Log("RECURSE");
        // Debug.Log(JsonConvert.SerializeObject(tree));
        var outerBooleanList = new List<bool>();
        foreach (KeyValuePair<string, dynamic> element in tree)
        {
            // Debug.Log(element.Key + ": " + element.Value.GetType());
            var booleanList = new List<bool>();
            switch (element.Key)
            {
                case "and":
                    // Debug.Log("AND");
                    foreach (Dictionary<string, dynamic> subelement in element.Value)
                        booleanList.Add(evaluateConditionRecur(origin, target, subelement, "and"));
                    outerBooleanList.Add(!booleanList.Any(x => x == false));
                    break;
                case "or":
                    // Debug.Log("OR");
                    foreach (Dictionary<string, dynamic> subelement in element.Value)
                        booleanList.Add(evaluateConditionRecur(origin, target, subelement, "or"));
                    outerBooleanList.Add(booleanList.Any(x => x == true));
                    break;
                case "origin":
                    // Debug.Log("ORIGIN");
                    foreach (KeyValuePair<string, BattleTag> tag in origin.tags)
                        foreach (var str in element.Value)
                            booleanList.Add(str == tag.Value.type);
                    if (lastCall == "and")
                        outerBooleanList.Add(!booleanList.Any(x => x == false));
                    else
                        outerBooleanList.Add(booleanList.Any(x => x == true));
                    break;
                case "target":
                    // Debug.Log("TARGET");
                    foreach (KeyValuePair<string, BattleTag> tag in target.tags)
                        foreach (var str in element.Value)
                            booleanList.Add(str == tag.Value.type);
                    if (lastCall == "and")
                        outerBooleanList.Add(!booleanList.Any(x => x == false));
                    else
                        outerBooleanList.Add(booleanList.Any(x => x == true));
                    break;
                default:
                    Debug.Log("Illegal condition keyword");
                    return false;
            }
        }

        if (lastCall == "and")
            return !outerBooleanList.Any(x => x == false);
        else
            return outerBooleanList.Any(x => x == true);
    }
}

public class BattleEffectFactory
{
    // TODO some fancy way to deserialize directly and safely
    public static IBattleEffect make(BattleEffect effect)
    {
        switch (effect.type)
        {
            case "EffectBasicAttack":
                // Debug.Log("Make EffectBasicAttack");
                return new EffectBasicAttack(effect);
            case "EffectPercentModifyDamage":
                // Debug.Log("Make EffectPercentModifyDamage");
                return new EffectPercentModifyDamage(effect);
            default:
                Debug.Log("Unexpected BattleEffect type");
                return null;
        }
    }
}

public interface IBattleEffect
{
    BattleEffect stats { get; set; }
}
public interface IAttackDamageEffect : IBattleEffect
{
    int Process(IBattleActor origin, IBattleActor target, int damage);
}

public class EffectBasicAttack : IAttackDamageEffect
{
    // TODO can combine both scale and multiplication effects, probably
    public BattleEffect stats { get; set; }
    private int _additionalDamage;

    // Calculate damage from attack and defense values
    public EffectBasicAttack(BattleEffect inStats)
    {
        stats = inStats;
        _additionalDamage = stats.effectValues.ContainsKey(BattleEffect.ADD) ? (int)stats.effectValues[BattleEffect.ADD] : 0;
    }

    public int Process(IBattleActor origin, IBattleActor target, int damage)
    {
        int moreDamage = origin.stats.currentAttack + _additionalDamage - target.stats.currentDefense;
        if (moreDamage > 0)
            return damage + moreDamage;
        else
            return damage;
    }
}

public class EffectPercentModifyDamage : IAttackDamageEffect
{
    public BattleEffect stats { get; set; }
    private float _percentChange;

    // Modify current damage based on percentage if tags match 
    public EffectPercentModifyDamage(BattleEffect inStats)
    {
        stats = inStats;
        _percentChange = stats.effectValues.ContainsKey(BattleEffect.MUL) ? (float)stats.effectValues[BattleEffect.MUL] : 1.0f;
    }

    public int Process(IBattleActor origin, IBattleActor target, int damage)
    {
        // Debug.Log(origin.stats.name + " acting on " + target.stats.name + " -- " + stats.evaluateCondition(origin, target));
        if (stats.evaluateCondition(origin, target))
            return (int)(damage * _percentChange);  // TODO: determinism worries? non-float ways to calculate?
        else
            return damage;
    }
}
