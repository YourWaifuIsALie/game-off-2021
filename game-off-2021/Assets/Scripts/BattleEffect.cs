using System.Collections.Generic;
using Newtonsoft.Json;


public class BattleEffect
{
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

public abstract class AttackDamageEffect : BattleEffect
{
    public abstract int process(BattleActor origin, BattleActor target, int damage);
}

public class EffectBasicAttack : AttackDamageEffect
{
    // Calculate damage from attack and defense values
    public EffectBasicAttack() { }

    public override int process(BattleActor origin, BattleActor target, int damage)
    {
        int moreDamage = origin.currentAttack - target.currentDefense;
        if (moreDamage > 0)
            return damage + moreDamage;
        else
            return damage;
    }
}

public class EffectPercentModifyDamage : AttackDamageEffect
{
    // TODO: Figure out set values of effectValues and use that
    public float percentChange { get; set; }

    // Modify current damage based on percentage if tags match 
    public EffectPercentModifyDamage() { }

    public override int process(BattleActor origin, BattleActor target, int damage)
    {
        if (this.evaluateCondition(origin, target))
            return (int)(damage * percentChange);  // TODO: determinism worries? non-float ways to calculate?
        else
            return damage;
    }
}
