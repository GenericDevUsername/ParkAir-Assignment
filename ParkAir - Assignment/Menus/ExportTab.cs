using System.Diagnostics;

namespace ParkAir___Assignment.Menus;

public class ExportTab : ITab
{

  public string TabName { get; set; } = "Export";
  public string ScreenString { get; }
  public bool Restart { get; } = false;
  public Gui? _gui { get; private set; }
  public bool Tabber { get; } = true;
  public void HandleKeypress(ConsoleKeyInfo key)
  {

  }
  public string Screen()
  {
    return "";
  }
  public void RestartTab()
  {
    
  }
  public void Register(Gui gui)
  {
    this._gui = gui;
  }
}