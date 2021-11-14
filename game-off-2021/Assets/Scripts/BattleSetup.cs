using System.Collections.Generic;
using UnityEngine;

public class BattleSetup
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string type { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }
    public List<string> enemyStrings { get; set; }
    public List<IBattleActor> enemies { get; set; }

    public BattleSetup()
    {
        enemies = new List<IBattleActor>();
    }
}

public class BattleSetupFactory
{
    // TODO some fancy way to deserialize directly and safely
    public static void make(BattleSetup setup, Dictionary<string, IBattleActor> allActors)
    {
        foreach (var enemyString in setup.enemyStrings)
            setup.enemies.Add(allActors[enemyString]);
    }
}