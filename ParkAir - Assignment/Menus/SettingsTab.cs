using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ParkAir___Assignment.Menus.SettingsHandlers;

namespace ParkAir___Assignment.Menus
{
  internal class SettingsTab : ITab
  {
    public string TabName { get; set; } = "Settings";
    public string ScreenString { get; private set; } = "";
    public int CurrentIndex { get; private set; } = -1;
    public JObject Settings { get; private set; }
    public bool Tabber { get; private set; } = true;
    private List<SettingsCategory> _settings = new();
    private List<Setting> _selectionIndex = new();

    public SettingsTab(string fp)
    {
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new(file);
      Settings = (JObject)JToken.ReadFrom(reader);
      BuildSettings();
    }

    private void BuildSettings()
    {
      foreach (var category in Settings)
      {
        if (category.Value is null) continue;
        if (category.Key == "FirstLaunch" || category.Value.Type != JTokenType.Object) continue;
        BuildCategory(category.Key, category.Value);
      }
    }

    private void BuildCategory(string name, JToken settings)
    {
      SettingsCategory builtCategory = new(name);
      foreach (var setting in settings.Value<JObject>())
      {
        Setting settingClass = new(builtCategory, setting, this._selectionIndex.Count);
        builtCategory.Settings.Add(settingClass);
        this._selectionIndex.Add(settingClass);
      }

      this._settings.Add(builtCategory);
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {
      if (key.Key == ConsoleKey.DownArrow)
        CurrentIndex = CurrentIndex + 1 == this._selectionIndex.Count ? CurrentIndex : CurrentIndex + 1;
      else if (key.Key == ConsoleKey.UpArrow) CurrentIndex = CurrentIndex - 1 == -2 ? CurrentIndex : CurrentIndex - 1;

      if (CurrentIndex >= 0)
        Tabber = false;
      else Tabber = true;
    }

    private string SettingString(Setting setting)
    {
      var lineString = "";
      var valueDisplay = "";
      var offset = 0;
      var settingType = (string)setting.Value["InputType"];
      switch (settingType)
      {
        case "SingleSelect":
          JArray options = (JArray)setting.Value["Options"];
          valueDisplay = $"";
          foreach (string option in options)
            valueDisplay +=
              $"[{((string)setting.Value["Selected"] == option ? "X" : " ")}] {option}{new(' ', (int)setting.Value["OptionGap"])}";
          valueDisplay = valueDisplay.Remove(valueDisplay.Length - (int)setting.Value["OptionGap"],
            (int)setting.Value["OptionGap"]);
          break;

        case "IP":
          valueDisplay =
            $"{(string?)setting.Value["Selected"]}{new string('_', 26 - ((string?)setting.Value["Selected"]).Length)}";
          break;

        case "Port":
          valueDisplay =
            $"{(string?)setting.Value["Selected"]}{new string('_', 26 - ((string?)setting.Value["Selected"]).Length)}";
          break;

        case "Color":
          valueDisplay =
            $"{(string?)setting.Value["Selected"]}{new('_', 21 - ((string?)setting.Value["Selected"]).Length)}[{new Color().FromSetting((string?)setting.Value["Selected"])}▓▓▓\x1b[0m]";
          offset += new Color().FromSetting((string?)setting.Value["Selected"]).Length + 4;
          break;
      }

      return
        $"│[{(CurrentIndex >= 0 ? this._selectionIndex[CurrentIndex < 0 ? 0 : CurrentIndex] == setting ? ">" : " " : " ")}] {setting.Name}{new(' ', 85 - setting.Name.Length - valueDisplay.Length + offset)}{valueDisplay} │";
    }

    public string Screen()
    {
      List<string> screenLines = new();
      foreach (SettingsCategory category in this._settings)
      {
        screenLines.Add($"│{category.Name}:{new(' ', 89 - category.Name.Length)}│");
        foreach (Setting setting in category.Settings) screenLines.Add(SettingString(setting));

        if (this._settings[this._settings.Count - 1 < 0 ? 0 : this._settings.Count - 1] != category)
          screenLines.Add($"│{new string(' ', 90)}│");
      }

      screenLines.Add($"└{new string('─', 90)}┘");

      return string.Join("\n", screenLines.ToArray());
    }

  }
}
