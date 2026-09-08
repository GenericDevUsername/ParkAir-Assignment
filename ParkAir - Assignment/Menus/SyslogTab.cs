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
  
  private readonly Thread t_backgroundListenerThread;
  private void ScreenRefresh()
  {
    while (true)
    {
      if (_gui?.Tabs[_gui._tabIndex] == this)
      {
        _gui?.ScreenUpdate();
      }
      Thread.Sleep((int)3E3);
    }
  }

  public SyslogTab(SettingsTab settings)
  {
    this.sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
                                      (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
                                      (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );
    
    this.sysServer.Start();
    this.t_backgroundListenerThread = new(new ThreadStart(ScreenRefresh));
    this.t_backgroundListenerThread.Start();
  }

  public void HandleKeypress(ConsoleKeyInfo key)
  {
  }

  public string Screen()
  {
    List<SysMessage> logs = this.sysServer.GetLogs();
    List<SysMessage> lastTen = logs.Skip(Math.Max(0, logs.Count - 17)).ToList();
    string screenReturn = "";
    foreach (SysMessage log in lastTen)
    {
      screenReturn += log.sysString + "\n";
    }

    return screenReturn;
  }

  public void Register(Gui gui)
  {
    _gui = gui;
  }
}