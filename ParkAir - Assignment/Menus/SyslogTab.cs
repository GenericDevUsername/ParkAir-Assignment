using ParkAir___Assignment.Menus.SettingsHandlers;
using ParkAir___Assignment.Syslog;
using System.Net;

namespace ParkAir___Assignment.Menus;

public class SyslogTab : ITab
{

  private readonly Dictionary<int, string> _color = new()
  {
    { 0, "Emergency" },
    { 1, "Alert" },
    { 2, "Critical" },
    { 3, "Error" },
    { 4, "Warning" },
    { 5, "Notice" },
    { 6, "Informational" },
    { 7, "Debug" }
  };

  private readonly SettingsTab _settings;

  private readonly Thread t_backgroundListenerThread;
  public SyslogServer sysServer;

  public SyslogTab(SettingsTab settings)
  {
    sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
        (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
        (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
    );

    _settings = settings;

    sysServer.Start();
    t_backgroundListenerThread = new Thread(ScreenRefresh);
    t_backgroundListenerThread.Start();
  }
  public string TabName { get; set; } = "Syslog";
  public string ScreenString => "";
  public bool Tabber => true;
  public Gui? _gui { get; private set; }
  public bool Restart { get; } = true;

  public void RestartTab()
  {
    sysServer._listeningPort = (int)_settings.Settings.SelectToken("Network.ListeningPort.Selected");
    sysServer._listeningIP = IPAddress.Parse((string)_settings.Settings.SelectToken("Network.ListeningIp.Selected"));
    sysServer.Restart();

  }

  public void HandleKeypress(ConsoleKeyInfo key)
  {
  }

  public string Screen()
  {
    List<SysMessage> logs = sysServer.GetLogs();
    List<SysMessage> lastTen = logs.Skip(Math.Max(0, logs.Count - 17)).ToList();
    string screenReturn = "";
    foreach (SysMessage log in lastTen)
    {
      string color = new Color().FromSetting((string)_settings.Settings.SelectToken($"Colors.{_color[log.priority - log.priority / 8 * 8]}.Selected"));
      screenReturn += $"│ {log.timestamp.ToLongTimeString()} │{color} {$"[{_color[log.priority - log.priority / 8 * 8].ToUpper()}]",-15}\x1b[0m │ {(log.hostname != "-" ? log.hostname : "None")} {(log.message.Length > 1 ? log.message : "None")}\x1b[0m\n";
      _gui._debug[2] = log.sysString;
    }

    return screenReturn;
  }

  public void Register(Gui gui)
  {
    _gui = gui;
  }
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
}