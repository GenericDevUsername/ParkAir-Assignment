using System.Net.Mail;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class Setting
  {
    public string Key { get; }
    public SettingsCategory Category { get; }
    public string Value { get; }

    public Setting(SettingsCategory category, KeyValuePair<string, JToken?> setting)
    {
      this.Category = category;
      this.Key = setting.Key;
      
    }
  }
}
