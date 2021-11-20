using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

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
    public static void Make(BattleSetup setup, Dictionary<string, BattleActor> allActors, Dictionary<string, IBattleAction> allActions, Dictionary<string, BattleTag> allTags)
    {
        Dictionary<string, int> identicalEnemyCount = new Dictionary<string, int>();
        int value;
        foreach (var enemyString in setup.enemyStrings)
        {
            // TODO only append names to numbers if there's duplicates
            if (identicalEnemyCount.TryGetValue(enemyString, out value))
            {
                value += 1;
                identicalEnemyCount[enemyString] = value;
            }
            else
            {
                value = 1;
                identicalEnemyCount[enemyString] = value;
            }
            setup.enemies.Add(CopyBattleActor(allActors[enemyString], allActions, allTags, value));
        }
    }

    private static IBattleActor CopyBattleActor(BattleActor parent, Dictionary<string, IBattleAction> allActions, Dictionary<string, BattleTag> allTags, int dupeNumber)
    {
        // Lazy way to copy an object using pre-existing deserializer
        string serializedParent = JsonConvert.SerializeObject(parent);
        BattleActor newBattleActor = JsonConvert.DeserializeObject<BattleActor>(serializedParent);

        IBattleActor createdObject = BattleActorFactory.Make(newBattleActor, allActions, allTags);
        if (createdObject is IBattleActor)
        {
            if (dupeNumber > 0)
                createdObject.stats.displayName = createdObject.stats.displayName + $" {dupeNumber}";
            return createdObject;
        }

        return null;

    }
}