using System.Collections.Generic;

public class BattleActor
{
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

    public List<BattleTag> tags { get; set; }
    public List<BattleAction> actions { get; set; }

    public BattleActor(int health, int mana, int attack, int defense, int speed, int inLevel, List<BattleTag> inTags, List<BattleAction> inActions)
    {
        maxHealth = health;
        maxMana = mana;
        maxAttack = attack;
        maxDefense = defense;
        maxSpeed = speed;
        level = inLevel;
        tags = inTags;
        actions = inActions;
        isAlive = true;
    }

}

public class TestActor : BattleActor
{
    public TestActor() : base(10, 10, 1, 1, 1, 0, new List<BattleTag>(), new List<BattleAction>())
    {
        this.actions.Add(new ActionAttackBasic());
    }
}