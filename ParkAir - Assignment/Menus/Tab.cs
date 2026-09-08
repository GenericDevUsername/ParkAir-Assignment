namespace ParkAir___Assignment.Menus
{
  public interface ITab
  {
    public string TabName { get; set; }
    public string ScreenString { get; }

    internal Gui? _gui { get; }
    public bool Tabber { get; }
    internal void HandleKeypress(ConsoleKeyInfo key);
    internal string Screen();

    internal void Register(Gui gui);
  }
}
