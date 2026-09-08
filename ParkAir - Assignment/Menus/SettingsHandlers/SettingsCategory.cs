namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class SettingsCategory
  {
    public readonly string Name;
    public List<Setting> Settings = new();

    public SettingsCategory(string name)
    {
      this.Name = name;
    }
  }
}
