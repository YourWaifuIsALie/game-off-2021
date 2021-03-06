using System.Collections.Generic;
using UnityEngine;

public class BattleActor
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string type { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }

    public int maxHealth { get; set; }
    public int maxMana { get; set; }
    public int maxAttack { get; set; }
    public int maxDefense { get; set; }
    public int maxSpeed { get; set; }

    public int currentHealth { get; set; }
    public int currentMana { get; set; }
    public int currentAttack { get; set; }
    public int currentDefense { get; set; }
    public int currentSpeed { get; set; }

    public int level { get; set; }
    public bool isAlive { get; set; }
    public string bugged { get; set; }

    public List<string> tagStrings { get; set; }
    public Dictionary<string, BattleTag> tags { get; set; }
    public List<string> actionStrings { get; set; }
    public Dictionary<string, IBattleAction> actions { get; set; }

    public static int MAXHEALTH = 32767;
    public static int MINHEALTH = -32768;

    public BattleActor()
    {
        tags = new Dictionary<string, BattleTag>();
        actions = new Dictionary<string, IBattleAction>();
    }

    public void InitializeActor()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentAttack = maxAttack;
        currentDefense = maxDefense;
        currentSpeed = maxSpeed;
        isAlive = true;
        bugged = "no";
    }

    public bool CheckAlive()
    {
        if (currentHealth <= 0)
        {
            isAlive = false;
            return false;
        }
        else if (currentHealth > MAXHEALTH)
        {
            // Overflow two bytes of health
            // Signed negative is twos-complement representation
            currentHealth = 0 - (currentHealth - MAXHEALTH);
            isAlive = false;
            bugged = "integerOverflow";
            return false;
        }
        return true;
    }

}

public class BattleActorFactory
{
    // TODO some fancy way to deserialize directly and safely
    public static IBattleActor Make(BattleActor actor, Dictionary<string, IBattleAction> allActions, Dictionary<string, BattleTag> allTags)
    {
        foreach (var tagString in actor.tagStrings)
            actor.tags[tagString] = (allTags[tagString]);

        foreach (var actionString in actor.actionStrings)
            actor.actions[actionString] = (allActions[actionString]);

        actor.InitializeActor();

        switch (actor.type)
        {
            case "BasicActor":
                // Debug.Log("Make BasicActor");
                return new BasicActor(actor);
            default:
                Debug.Log("Unexpected BattleActor type");
                return null;
        }
    }
}
public interface IBattleActor
{
    BattleActor stats { get; set; }
}

public class BasicActor : IBattleActor
{
    public BattleActor stats { get; set; }

    public BasicActor(BattleActor inStats)
    {
        stats = inStats;
    }
}