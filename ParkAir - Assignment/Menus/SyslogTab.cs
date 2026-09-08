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

  private bool _pause = false;

  private string _ipfilter = "";
  private int _severityFilter = -1;
  private int _resetter;

  private int _selectint = -1;


  private readonly SettingsTab _settings;

  private readonly Thread t_backgroundListenerThread;
  private SyslogServer _sysServer;

  public SyslogTab(SettingsTab settings)
  {
    _sysServer = new SyslogServer(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
        (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
        (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
    );

    _settings = settings;

    _sysServer.Start();
    t_backgroundListenerThread = new Thread(ScreenRefresh);
    t_backgroundListenerThread.Start();
  }


  public string TabName { get; set; } = "Syslog";
  public string ScreenString { get; private set; } = "";
  public bool Tabber { get; private set; } = true;
  public Gui? _gui { get; private set; }
  public bool Restart { get; } = true;
 
  private static int Mod(int k, int n)
  {
    return (k %= n) < 0 ? k + n : k;
  }

  public void RestartTab()
  {
    _sysServer._listeningPort = (int)_settings.Settings.SelectToken("Network.ListeningPort.Selected");
    _sysServer._listeningIP =
        IPAddress.Parse((string)_settings.Settings.SelectToken("Network.ListeningIp.Selected"));
    _sysServer.Restart();

  }

  private void DeleteLocalStore()
  {
    this._sysServer._queue.Clear();
    this._sysServer._clearLogs = true;
    this._sysServer._log = new SlidingBuffer<SysMessage>(250);
  }

  private void ChangeFilter(int index, ConsoleKey action)
  {
    switch (index)
    {
      case 0:
        _pause = true;
        List<SysMessage> logsList = _sysServer.GetLogs();
        List<string> autocompleteList = (from log in logsList select log.hostname).Distinct().ToList();

        _ipfilter = _gui.Input(top: 3, left: 64, prefill: _ipfilter != "" ? _ipfilter : "",
            spaceholder: '_', max: 26, length: 26, autocomplete: autocompleteList);
        _pause = false;
        break;

      case 1:
        _severityFilter = Mod(_severityFilter + (action == ConsoleKey.LeftArrow ? 0 : 2), _color.Count + 1) - 1;
        break;
    }
    if (_gui?.Tabs[_gui._tabIndex] == this && !_pause) _gui?.ScreenUpdate();
  }

  public void HandleKeypress(ConsoleKeyInfo key)
  {
    if (_selectint < 3 - 1 && _resetter > 0) _resetter = 0;
    switch (key.Key)
    {
      case ConsoleKey.DownArrow:
        _selectint = _selectint + 1 == 3 - 1 + 1 ? _selectint : _selectint + 1;
        break;

      case ConsoleKey.UpArrow:
        _selectint = _selectint - 1 == -2 ? _selectint : _selectint - 1;
        break;

      case ConsoleKey.Enter:
        if (_selectint is >= 0 and < 3 - 1)
        {
          ChangeFilter(_selectint, key.Key);
        }
        switch (_selectint)
        {
          case >= 3 - 1 when _resetter > 0:
          {
            _resetter++;
            if (_resetter == 3)
            {
              DeleteLocalStore();
              _resetter = 0;
            }

            break;
          }
          case >= 3 - 1:
            _resetter++;
            break;
        }

        break;

      case ConsoleKey.RightArrow:
        if (_selectint is >= 0 and < 3 - 1)
          ChangeFilter(_selectint, key.Key);
        break;

      case ConsoleKey.LeftArrow:
        if (_selectint is >= 0 and < 3 - 1)
          ChangeFilter(_selectint, key.Key);
        break;
    }

    Tabber = _selectint < 0;
  }

  public string Screen()
  {
    List<SysMessage> logs = _sysServer.GetLogs();
    if (_severityFilter >= 0)
    {
      logs = (from log in logs where log.severity == _severityFilter select log).ToList();
    }

    if (_ipfilter != "")
    {
      logs = (from log in logs where log.hostname.ToUpper().Contains(_ipfilter.ToUpper()) select log).ToList();
    }


    List<SysMessage> lastAmount = logs.Skip(Math.Max(0, logs.Count - 15)).ToList();
    string screenReturn = "";

    string ipFilterString = $"{_ipfilter}{new string('_', 26 - _ipfilter.Length)}";
    screenReturn += $"│[{(_selectint == 0 ? ">" : " ")}] Filter IP{new string(' ', 85 - 9 - ipFilterString.Length)}{ipFilterString} │\n";


    string sevcolor = $"{(_severityFilter >= 0 ? new Color().FromSetting((string)_settings.Settings.SelectToken($"Colors.{_color[_severityFilter]}.Selected")) : "")}";
    int offset = _severityFilter >= 0 ? sevcolor.Length + 4 : 0;
    string sevFilterString = $"{(_severityFilter >= 0 ? $"{sevcolor}{_color[_severityFilter]}\x1b[0m" : "UNSET")}";
    screenReturn += $"│[{(_selectint == 1 ? ">" : " ")}] Filter Type{new string(' ', 83 - 11 - sevFilterString.Length + offset)}[{sevFilterString}] │\n";
    screenReturn += $"│{new string(' ', 90)}│\n";
    screenReturn +=
        $"│[{(_selectint == 3 - 1 ? ">" : " ")}] Delete Local Store {(_selectint == 3 - 1 ? _resetter > 0 ? $"\x1b[31m(Press Enter {3 - _resetter} More Time(s) to Confirm)\x1b[0m" : "(Press Enter)" : " ")}{new string(' ', 85 - 19 - (_selectint == 3 - 1 ? _resetter > 0 ? 38 : 12 : 0))}│\n";
    screenReturn += $"├──────────┬─────────────────┬{new string('─', 61)}┤\n";

    int i = 0;
    foreach (SysMessage log in lastAmount)
    {
      string? color = new Color().FromSetting((string)_settings.Settings.SelectToken($"Colors.{_color[log.severity]}.Selected"));
      screenReturn += $"│ {log.timestamp.ToLongTimeString()} │{color} {$"[{_color[log.severity].ToUpper()}]",-15}\x1b[0m │ {log.sysString}\x1b[0m\n";
      i++;
    }

    while (i < 15)
    {
      screenReturn += $"│          │                 │\n";
      i++;
    }
    screenReturn += $"└──────────┴─────────────────┴{new string('─', 61)}┘";

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
      try
      {
        if (_gui != null && (_gui._tabIndex < 0 || _gui._tabIndex >= _gui?.Tabs.Count)) _gui._tabIndex = -1;
        if (_gui?.Tabs[_gui._tabIndex] == this && !_pause) _gui?.ScreenUpdate();
      }
      catch
      {
        // ignore
      }
      Thread.Sleep((int)3E3);
    }
  }
}