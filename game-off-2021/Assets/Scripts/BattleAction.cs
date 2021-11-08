using System.Collections.Generic;

public abstract class BattleAction
{
    public string name { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }

    public abstract void act(BattleActor origin, BattleActor[] targets);

}

public class ActionAttackBasic : BattleAction
{
    public List<AttackDamageEffect> effects { get; set; }
    public ActionAttackBasic()
    {
        // TODO: Use EffectLoader and read from JSON types
        effects = new List<AttackDamageEffect>();
        effects.Add(new EffectBasicAttack());
        effects.Add(new EffectPercentModifyDamage());
    }

    public override void act(BattleActor origin, BattleActor[] targets)
    {
        if (targets.Length > 1)
        {
            ; // TODO: determine multi-target behavior. Presumably, base attack to every target
        }
        else
        {
            BattleActor target = targets[0];
            int damage = 0;
            foreach (var effect in effects)
                damage = effect.process(origin, target, damage);
            if (damage > 0)
                target.currentHealth = target.currentHealth - damage;
        }
    }
}
