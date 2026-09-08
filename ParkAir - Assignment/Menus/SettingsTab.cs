using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ParkAir___Assignment.Menus
{
  internal class SettingsTab : ITab
  {
    public string TabName { get; set; } = "Settings"; 
    public string Screen { get; set; } = "Settings"; 
    public readonly JObject Settings;

    public SettingsTab(string fp)
    {
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new JsonTextReader(file);
      this.Settings = (JObject)JToken.ReadFrom(reader);
    }
    public void HandleKeypress(ConsoleKeyInfo key)
    {

    }
  }
}
