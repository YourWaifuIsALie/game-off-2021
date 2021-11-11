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

    public bool evaluateCondition(BattleActor origin, BattleActor target)
    {
        // TODO: Parse boolean logic
        return false;
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
                Debug.Log("Make EffectBasicAttack");
                return new EffectBasicAttack(effect);
            case "EffectPercentModifyDamage":
                Debug.Log("Make EffectPercentModifyDamage");
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
    int process(BattleActor origin, BattleActor target, int damage);
}

public class EffectBasicAttack : IAttackDamageEffect
{
    public BattleEffect stats { get; set; }
    private int _additionalDamage;

    // Calculate damage from attack and defense values
    public EffectBasicAttack(BattleEffect inStats)
    {
        stats = inStats;
        _additionalDamage = stats.effectValues.ContainsKey(BattleEffect.ADD) ? (int)stats.effectValues[BattleEffect.ADD] : 0;
    }

    public int process(BattleActor origin, BattleActor target, int damage)
    {
        int moreDamage = origin.currentAttack + _additionalDamage - target.currentDefense;
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

    public int process(BattleActor origin, BattleActor target, int damage)
    {
        if (stats.evaluateCondition(origin, target))
            return (int)(damage * _percentChange);  // TODO: determinism worries? non-float ways to calculate?
        else
            return damage;
    }
}
