using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ParkAir___Assignment.Menus.SettingsHandlers;
using System.Text.RegularExpressions;

namespace ParkAir___Assignment.Menus
{
  public class SettingsTab : ITab
  {
    public string TabName { get; set; } = "Settings";
    public string ScreenString { get; private set; } = "";
    public int CurrentIndex { get; private set; } = -1;
    public JObject Settings { get; private set; }

    public string SettingsFile = "";
    public bool Tabber { get; private set; } = true;
    private int _resetter = 0;
    private List<SettingsCategory> _settings = new();
    private List<Setting> _selectionIndex = new();
    public Gui? _gui { get; private set; }

    int Mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }

    public SettingsTab(string fp)
    {
      this.SettingsFile = fp;
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new(file);
      Settings = (JObject)JToken.ReadFrom(reader);
      file.Close();
      BuildSettings();
    }

    public void Register(Gui gui)
    {
      this._gui = gui;
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
      SettingsCategory builtCategory = new(name, this);
      foreach (var setting in settings.Value<JObject>())
      {
        Setting settingClass = new(builtCategory, setting, this._selectionIndex.Count);
        builtCategory.Settings.Add(settingClass);
        this._selectionIndex.Add(settingClass);
      }

      this._settings.Add(builtCategory);
    }

    private void ChangeSetting(Setting setting, ConsoleKeyInfo ctx)
    {
      switch (setting.InputType)
      {
        case "Color":
          // Get The Next Value
          if (ctx.Key == ConsoleKey.Enter || ctx.Key == ConsoleKey.RightArrow)
          {
            setting.Set(new Color().Next((string)setting.Value["Selected"]));
          }
          else if (ctx.Key == ConsoleKey.LeftArrow)
          {
            setting.Set(new Color().Previous((string)setting.Value["Selected"]));
          }
          break;

        case "SingleSelect":
          string response = (string)setting.Value["Selected"];
          List<string> indexArray = setting.Value.SelectToken("Options").ToObject<string[]>().ToList<string>();
          int currentIndex = indexArray.IndexOf(response);
          // Get The Next Value
          if ((ctx.Key == ConsoleKey.Enter || ctx.Key == ConsoleKey.RightArrow) && currentIndex > -1)
          {
            setting.Set(indexArray[Mod(currentIndex + 1, indexArray.Count)]);
          }
          else if (ctx.Key == ConsoleKey.LeftArrow && currentIndex > -1)
          {
            setting.Set(indexArray[Mod(currentIndex - 1, indexArray.Count)]);
          }
          break;

        case "IP":
          string newIp = this._gui.Input(top: setting.Line, left: 64, prefill: (string)setting.Value["Selected"], spaceholder: '_', max: 26, length: 26, customError: "Not a valid IP!", regexCheck: new Regex(@"^(((([lL]ocal[hH]ost)|(([2]([0-4][0-9]|[5][0-5])|[0-1]?[0-9]?[0-9])[.]){3}(([2]([0-4][0-9]|[5][0-5])|[0-1]?[0-9]?[0-9]))))+)$"));
          setting.Set(newIp);
          break;

        case "Port":
          string newPort = this._gui.Input(top: setting.Line, left: 64, prefill: (string)setting.Value["Selected"], spaceholder: '_', max: 5, length: 26, customError: "Use valid port (0 - 65535)!", regexCheck: new Regex(@"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$"));
          setting.Set(newPort);
          break;
      }
    }

    private void ResetToDefaults()
    {
      foreach (Setting setting in this._selectionIndex)
      {
        setting.Reset();
      }
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {
      if (CurrentIndex < _selectionIndex.Count && this._resetter > 0)
      {
        this._resetter = 0;
      }
      switch (key.Key)
      {
        case ConsoleKey.DownArrow:
          CurrentIndex = CurrentIndex + 1 == this._selectionIndex.Count + 1 ? CurrentIndex : CurrentIndex + 1;
          break;

        case ConsoleKey.UpArrow:
          CurrentIndex = CurrentIndex - 1 == -2 ? CurrentIndex : CurrentIndex - 1;
          break;

        case ConsoleKey.Enter:
          if (CurrentIndex >= 0 && CurrentIndex < _selectionIndex.Count)
          { 
            ChangeSetting(_selectionIndex[CurrentIndex], key);
          }
          if (CurrentIndex >= _selectionIndex.Count && this._resetter > 0)
          {
            _resetter++;
            if (_resetter == 3)
            {
              ResetToDefaults();
              _resetter = 0;
            }
          }
          else if (CurrentIndex >= _selectionIndex.Count)
          {
            this._resetter++;
          }
          break;

        case ConsoleKey.RightArrow:
          if (CurrentIndex >= 0 && CurrentIndex <= _selectionIndex.Count)
          {
            ChangeSetting(_selectionIndex[CurrentIndex], key);
          }
          break;

        case ConsoleKey.LeftArrow:
          if (CurrentIndex >= 0 && CurrentIndex <= _selectionIndex.Count)
          {
            ChangeSetting(_selectionIndex[CurrentIndex], key);
          }
          break;


      };

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
            $"{(string?)setting.Value["Selected"]}{new(' ', 21 - ((string?)setting.Value["Selected"]).Length)}[{new Color().FromSetting((string?)setting.Value["Selected"])}▓▓▓\x1b[0m]";
          offset += new Color().FromSetting((string?)setting.Value["Selected"]).Length + 4;
          break;
      }

      return
        $"│[{(CurrentIndex >= 0 ? (CurrentIndex < 0 || CurrentIndex > _selectionIndex.Count ? 0 : CurrentIndex) >= 0 && (CurrentIndex < 0 || CurrentIndex > _selectionIndex.Count ? 0 : CurrentIndex) < this._selectionIndex.Count && this._selectionIndex[CurrentIndex < 0 || CurrentIndex > _selectionIndex.Count ? 0 : CurrentIndex] == setting ? ">" : " " : " ")}] {setting.Name}{new(' ', 85 - setting.Name.Length - valueDisplay.Length + offset)}{valueDisplay} │";
    }

    public string Screen()
    {
      List<string> screenLines = new();
      int line = this._gui.Tabs.Count > 1 ? 3 : 1;
      foreach (SettingsCategory category in this._settings)
      {
        screenLines.Add($"│{category.Name}:{new(' ', 89 - category.Name.Length)}│");
        foreach (Setting setting in category.Settings)
        {
          line++;
          screenLines.Add(SettingString(setting));
          setting.Line = line;
        }

        screenLines.Add($"│{new string(' ', 90)}│");
      }

      screenLines.Add($"│[{(CurrentIndex == _selectionIndex.Count ? ">" : " ")}] Reset to Defaults {(CurrentIndex == _selectionIndex.Count ? (this._resetter > 0 ? $"\x1b[31m(Press Enter {3 - this._resetter} More Time(s) to Confirm)\x1b[0m" : "(Press Enter)") : " ")}{new(' ', 85 - 18 - (CurrentIndex == _selectionIndex.Count ? (this._resetter > 0 ? 38 : 12) : 0))}│");

      screenLines.Add($"└{new string('─', 90)}┘");

      return string.Join("\n", screenLines.ToArray());
    }

  }
}
