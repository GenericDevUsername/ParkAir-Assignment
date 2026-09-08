using System.Net;
using ParkAir___Assignment.Syslog;

namespace ParkAir___Assignment.Menus;

public class SyslogTab : ITab
{
  public SyslogServer sysServer;
  public string TabName { get; set; } = "Syslog";
  public string ScreenString => "";
  public bool Tabber => true;
  public Gui? _gui { get; private set; }

  public SyslogTab(SettingsTab settings)
  {
    this.sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
                                      (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
                                      (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );
    
    this.sysServer.Start();
  }

  public void HandleKeypress(ConsoleKeyInfo key)
  {
  }

  public string Screen()
  {
    return "";
  }

  public void Register(Gui gui)
  {
    _gui = gui;
  }
}