using System.Net.Mail;
using System.Security.Cryptography;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class Setting
  {
    public string Key { get; }
    public string Name { get; }
    public string InputType { get; }
    public int Line = 0;

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
      InputType = (string)Value["InputType"];

      Value.PropertyChanged += Value_PropertyChanged;
    }

    private void Value_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      StreamWriter file = File.CreateText(this.Category.FilePath);
      JsonTextWriter writer = new JsonTextWriter(file);
      writer.Formatting = Formatting.Indented;
      this.Category._tab.Settings.WriteToAsync(writer);
      file.Close();
    }

    public void Set(string newValue)
    {
      this.Value["Selected"] = newValue;
    }

    public void Reset()
    {
      this.Value["Selected"] = this.Value["Default"];
    }
  }
}
