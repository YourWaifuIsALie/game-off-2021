using System.Collections.Generic;
using UnityEngine;

public class BattleAction
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string type { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }
    public List<string> effectStrings { get; set; }
    public List<IBattleEffect> effects { get; set; }

    public BattleAction()
    {
        effects = new List<IBattleEffect>();
    }
}

public class BattleActionFactory
{
    // TODO some fancy way to deserialize directly and safely
    public static IBattleAction make(BattleAction action, Dictionary<string, IBattleEffect> allEffects)
    {
        foreach (var effectString in action.effectStrings)
            action.effects.Add(allEffects[effectString]);

        switch (action.type)
        {
            case "ActionAttackBasic":
                // Debug.Log("Make ActionAttackBasic");
                return new ActionAttackBasic(action);
            default:
                Debug.Log("Unexpected BattleAction type");
                return null;
        }
    }
}

public interface IBattleAction
{
    void act(IBattleActor origin, List<IBattleActor> targets);
}
public class ActionAttackBasic : IBattleAction
{
    public BattleAction stats { get; set; }

    public ActionAttackBasic(BattleAction inStats)
    {
        stats = inStats;
    }

    public void act(IBattleActor origin, List<IBattleActor> targets)
    {
        if (targets.Count > 1)
        {
            ; // TODO: determine multi-target behavior. Presumably, base attack to every target
        }
        else
        {
            IBattleActor target = targets[0];
            int damage = 0;
            foreach (var effect in stats.effects)
                if (effect is IAttackDamageEffect)
                    damage = ((IAttackDamageEffect)effect).process(origin, target, damage);
            if (damage > 0)
                target.stats.currentHealth = target.stats.currentHealth - damage;
        }
    }
}
