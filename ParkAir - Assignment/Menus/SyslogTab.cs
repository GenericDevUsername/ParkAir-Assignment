using Newtonsoft.Json.Linq;
using System.Net;
using ParkAir___Assignment.Syslog;
using System.Drawing;

namespace ParkAir___Assignment.Menus;

public class SyslogTab : ITab
{
  public SyslogServer sysServer;
  public string TabName { get; set; } = "Syslog";
  public string ScreenString => "";
  public bool Tabber => true;
  public Gui? _gui { get; private set; }
  
  private readonly JObject _settings;

  private readonly Dictionary<int, string> _color = new()
  {
    {0, "Emergency"},
    {1, "Alert"},
    {2, "Critical"},
    {3, "Error"},
    {4, "Warning"},
    {5, "Notice"},
    {6, "Informational"},
    {7, "Debug"}
  };

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
    sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
                                      (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
                                      (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );
    
    this._settings = settings.Settings;

    sysServer.Start();
    t_backgroundListenerThread = new Thread(ScreenRefresh);
    t_backgroundListenerThread.Start();
  }

  public void RefreshNetwork(SettingsTab settings)
  {
    sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
                                  (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
                                  (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );
    sysServer.Start();

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
      string color = new SettingsHandlers.Color().FromSetting((string)this._settings.SelectToken($"Colors.{this._color[(int)log.priority - (((int)log.priority / 8) * 8)]}.Selected"));
      screenReturn += $"│ {log.timestamp.ToLongTimeString()} │{color} {$"[{this._color[(int)log.priority - (((int)log.priority/8)*8)].ToUpper()}]",-15}\x1b[0m │ {(log.hostname != "-" ? log.hostname : "None")} {(log.message.Length > 1 ? log.message : "None")}\x1b[0m\n";
      this._gui._debug[2] = log.sysString;
    }
    
    return screenReturn;
  }

  public void Register(Gui gui)
  {
    _gui = gui;
  }
}