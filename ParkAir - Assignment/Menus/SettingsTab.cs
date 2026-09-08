using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ParkAir___Assignment.Menus.SettingsHandlers;

namespace ParkAir___Assignment.Menus
{
  internal class SettingsTab : ITab
  {
    public string TabName { get; set; } = "Settings";
    public string ScreenString { get; } = "";
    public JObject Settings { get; private set; }
    private List<SettingsCategory> _settings = new();

    public SettingsTab(string fp) 
    {
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new(file);
      this.Settings = (JObject)JToken.ReadFrom(reader);
      BuildSettings();
    }

    private void BuildSettings()
    {
      foreach (var category in this.Settings)
      {
        if (category.Value is null)
        {
          continue;
        }
        if (category.Key == "FirstLaunch" || category.Value.Type != JTokenType.Object)
        {
          continue; 
        }
        BuildCategory(category.Key, category.Value);
      }
    }

    private void BuildCategory(string name, JToken settings)
    {
      SettingsCategory builtCategory = new(name);
      foreach (var setting in settings.Value<JObject>())
      {
        builtCategory.Settings.Add(new(builtCategory, setting));
      }
      this._settings.Add(builtCategory);
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {

    }

    public string Screen()
    {
      return "test";
    }
    
  }
}
