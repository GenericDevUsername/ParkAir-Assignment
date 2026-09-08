using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace ParkAir___Assignment.Menus.SettingsHandlers;

public class Setting
{
  public int Line = 0;
  public string Name { get; }
  public string InputType { get; }

  private SettingsCategory Category { get; }
  public JObject? Value { get; }

  public Setting(SettingsCategory category, KeyValuePair<string, JToken?> setting, int selectionIndex)
  {
    Category = category;
    Value = setting.Value.Value<JObject>();
    Name = (string)Value["Name"];
    InputType = (string)Value["InputType"];

    Value.PropertyChanged += Value_PropertyChanged;
  }


  private void Value_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    StreamWriter file = File.CreateText(Category.FilePath);
    JsonTextWriter writer = new(file);
    writer.Formatting = Formatting.Indented;
    Category.Tab.Settings.WriteToAsync(writer);
    file.Close();
  }

  public void Set(string newValue)
  {
    Value["Selected"] = newValue;
  }

  public void Reset()
  {
    Value["Selected"] = Value["Default"];
  }
}