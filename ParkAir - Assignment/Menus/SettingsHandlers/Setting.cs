using System.Net.Mail;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class Setting
  {
    public string Key { get; }
    public string Name { get; }
    public int SelectionIndex { get; }
    public SettingsCategory Category { get; }
    public JObject? Value { get; }

    public Setting(SettingsCategory category, KeyValuePair<string, JToken?> setting, int selectionIndex)
    {
      Category = category;
      Key = setting.Key;
      SelectionIndex = selectionIndex;
      Value = setting.Value.Value<JObject>();
      Name = (string)Value["Name"];

    }
  }
}
