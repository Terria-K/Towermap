using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Towermap;

public class SaveState 
{
    public string TFPath { get; set; }
    public List<string> RecentTowers { get; set; }

    public SaveState()
    {
        RecentTowers = new List<string>();
    }

    public void AddToRecent(string towerPath)
    {
        RecentTowers.Add(towerPath);
    }
}

[JsonSerializable(typeof(SaveState))]
internal partial class SaveStateContext : JsonSerializerContext
{

}