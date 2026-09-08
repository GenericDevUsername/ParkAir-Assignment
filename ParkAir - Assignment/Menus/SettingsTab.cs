using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParkAir___Assignment.Menus.SettingsHandlers;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace ParkAir___Assignment.Menus
{
  public class SettingsTab : ITab
  {
    private readonly List<Setting> _selectionIndex = new();
    private readonly List<SettingsCategory> _settings = new();

    public readonly string SettingsFile;
    private int _resetter;

    public SettingsTab(string fp)
    {
      this.SettingsFile = fp;
      StreamReader file = File.OpenText(fp);
      JsonTextReader reader = new(file);
      Settings = (JObject)JToken.ReadFrom(reader);
      file.Close();
      BuildSettings();
    }

    private int CurrentIndex { get; set; } = -1;
    public JObject Settings { get; }
    public string TabName { get; set; } = "Settings";
    public string ScreenString { get; } = "";
    public bool Restart { get; } = false;
    public bool Tabber { get; private set; } = true;
    public Gui? _gui { get; private set; }

    public void Register(Gui gui)
    {
      _gui = gui;
    }

    /// <summary>
    /// Handle keypress from user while within the current tab
    /// </summary>
    /// <param name="key">The key pressed.</param>
    public void HandleKeypress(ConsoleKeyInfo key)
    {
      if (CurrentIndex < this._selectionIndex.Count && this._resetter > 0) this._resetter = 0;
      switch (key.Key)
      {
        case ConsoleKey.DownArrow:
          CurrentIndex = CurrentIndex + 1 == this._selectionIndex.Count + 1 ? CurrentIndex : CurrentIndex + 1;
          break;

        case ConsoleKey.UpArrow:
          CurrentIndex = CurrentIndex - 1 == -2 ? CurrentIndex : CurrentIndex - 1;
          break;

        case ConsoleKey.Enter:
          if (CurrentIndex >= 0 && CurrentIndex < this._selectionIndex.Count)
            ChangeSetting(this._selectionIndex[CurrentIndex], key);
          if (CurrentIndex >= this._selectionIndex.Count && this._resetter > 0)
          {
            this._resetter++;
            if (this._resetter == 3)
            {
              ResetToDefaults();
              this._resetter = 0;
            }
          }
          else if (CurrentIndex >= this._selectionIndex.Count)
          {
            this._resetter++;
          }

          break;

        case ConsoleKey.RightArrow:
          if (CurrentIndex >= 0 && CurrentIndex < this._selectionIndex.Count)
            ChangeSetting(this._selectionIndex[CurrentIndex], key);
          break;

        case ConsoleKey.LeftArrow:
          if (CurrentIndex >= 0 && CurrentIndex < this._selectionIndex.Count)
            ChangeSetting(this._selectionIndex[CurrentIndex], key);
          break;
      }

      Tabber = CurrentIndex < 0;
    }

    public void RestartTab()
    {
    }

    public string Screen()
    {
      List<string> screenLines = new();
      var line = _gui is { Tabs.Count: > 1 } ? 3 : 1;
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

      screenLines.Add(
        $"│[{(CurrentIndex == this._selectionIndex.Count ? ">" : " ")}] Reset to Defaults {(CurrentIndex == this._selectionIndex.Count ? this._resetter > 0 ? $"\x1b[31m(Press Enter {3 - this._resetter} More Time(s) to Confirm)\x1b[0m" : "(Press Enter)" : " ")}{new(' ', 85 - 18 - (CurrentIndex == this._selectionIndex.Count ? this._resetter > 0 ? 38 : 12 : 0))}│");

      screenLines.Add($"└{new string('─', 90)}┘");

      return string.Join("\n", screenLines.ToArray());
    }

    private int Mod(int k, int n)
    {
      return (k %= n) < 0 ? k + n : k;
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
      foreach (var setting in settings.Value<JObject>()!)
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
            setting.Set(new Color().Next((string)setting.Value["Selected"]));
          else if (ctx.Key == ConsoleKey.LeftArrow)
            setting.Set(new Color().Previous((string)setting.Value["Selected"]));
          break;

        case "SingleSelect":
          var response = (string)setting.Value["Selected"];
          List<string> indexArray = setting.Value.SelectToken("Options").ToObject<string[]>().ToList();
          var currentIndex = indexArray.IndexOf(response);
          // Get The Next Value
          if ((ctx.Key == ConsoleKey.Enter || ctx.Key == ConsoleKey.RightArrow) && currentIndex > -1)
            setting.Set(indexArray[Mod(currentIndex + 1, indexArray.Count)]);
          else if (ctx.Key == ConsoleKey.LeftArrow && currentIndex > -1)
            setting.Set(indexArray[Mod(currentIndex - 1, indexArray.Count)]);
          break;

        case "IP":
          // Get host name
          List<IPAddress> localIPs = (from netInterface in NetworkInterface.GetAllNetworkInterfaces()
            select netInterface.GetIPProperties()
            into ipProps
            from addr in ipProps.UnicastAddresses
            select addr.Address).ToList();

          List<string> autocompleteList = localIPs.Select(ip => ip.ToString()).ToList();

          var newIp = _gui.Input(top: setting.Line, left: 64, prefill: (string)setting.Value["Selected"],
            spaceholder: '_', max: 26, length: 26, customError: "Not a valid IP!", autocomplete: autocompleteList,
            regexCheck: new(
              @"^((((([lL]ocal[hH]ost)|(([2]([0-4][0-9]|[5][0-5])|[0-1]?[0-9]?[0-9])[.]){3}(([2]([0-4][0-9]|[5][0-5])|[0-1]?[0-9]?[0-9]))))+))|((([0-9a-fA-F]{0,4})\:){2,7})([0-9a-fA-F]{0,4})$"));
          setting.Set(newIp);
          break;

        case "Port":
          var newPort = _gui.Input(top: setting.Line, left: 64, prefill: (string)setting.Value["Selected"],
            spaceholder: '_', max: 5, length: 26, customError: "Use valid port (0 - 65535)!",
            regexCheck: new(
              @"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$"));
          setting.Set(newPort);
          break;
      }

      if ((bool)setting.Value["RequiresRestart"]) _gui.RefreshSettings();
    }

    private void ResetToDefaults()
    {
      foreach (Setting setting in this._selectionIndex) setting.Reset();
      _gui.RefreshSettings();
    }

    private string SettingString(Setting setting)
    {
      var valueDisplay = "";
      var offset = 0;
      var settingType = (string)setting.Value["InputType"];
      switch (settingType)
      {
        case "SingleSelect":
          JArray options = (JArray)setting.Value["Options"];
          valueDisplay = "";
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
        $"│[{(CurrentIndex >= 0 ? (CurrentIndex < 0 || CurrentIndex > this._selectionIndex.Count ? 0 : CurrentIndex) >= 0 && (CurrentIndex < 0 || CurrentIndex > this._selectionIndex.Count ? 0 : CurrentIndex) < this._selectionIndex.Count && this._selectionIndex[CurrentIndex < 0 || CurrentIndex > this._selectionIndex.Count ? 0 : CurrentIndex] == setting ? ">" : " " : " ")}] {setting.Name}{new(' ', 85 - setting.Name.Length - valueDisplay.Length + offset)}{valueDisplay} │";
    }
  }
}
