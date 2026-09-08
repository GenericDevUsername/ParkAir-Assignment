namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class SettingsCategory
  {
    public readonly string Name;
    public readonly string FilePath;
    internal SettingsTab _tab { get; }
    public List<Setting> Settings = new();

    public SettingsCategory(string name, SettingsTab tab)
    {
      this._tab = tab;
      this.FilePath = tab.SettingsFile;
      this.Name = name;
    }
  }
}
