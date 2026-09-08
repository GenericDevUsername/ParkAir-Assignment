using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ParkAir___Assignment.Menus
{
  internal class SettingsTab : ITab
  {
    public string TabName = "Settings";
    public readonly JObject Settings;

    public SettingsTab(string fp)
    {
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new JsonTextReader(file);
      JObject o2 = (JObject)JToken.ReadFrom(reader);
      
      Console.WriteLine(o2.ToString());
    }
    public void HandleKeypress(ConsoleKeyInfo key)
    {
      Console.WriteLine("SettingsPotato");
    }
  }
}
