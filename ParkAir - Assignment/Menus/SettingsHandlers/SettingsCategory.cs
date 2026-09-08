namespace ParkAir___Assignment.Menus.SettingsHandlers;

public class SettingsCategory
{
    public readonly string FilePath;
    public readonly string Name;
    public readonly List<Setting> Settings = new();

    public SettingsCategory(string name, SettingsTab tab)
    {
        Tab = tab;
        FilePath = tab.SettingsFile;
        Name = name;
    }

    internal SettingsTab Tab { get; }
}