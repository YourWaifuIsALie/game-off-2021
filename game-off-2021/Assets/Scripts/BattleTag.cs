public class BattleTag
{
    public string name { get; set; }
    public string shortDescription { get; set; }
    public string longDescription { get; set; }
}

// TODO: Move this somewhere sensible
public class EffectCondition
{
    // TODO: Standard way to store json/read in Unity
    // logic: { and: { origin: [womanizer], target: [woman]}},
    // dynamic logic = JObject.Parse("{number:1000, str:'string', array: [1,2,3,4,5,6]}");
    // var filePath = Application.persistentDataPath + "/gamedata.json";
    // string fileContents = File.ReadAllText(filePath);
    // File.WriteAllText(filePath, "{\"test\": \"hi\"}");
    // using Newtonsoft.Json;
    // using System.IO;
    public EffectCondition()
    {

    }

    public bool evaluate(BattleActor origin, BattleActor target)
    {
        // TODO: Parse boolean logic
        return false;
    }
}