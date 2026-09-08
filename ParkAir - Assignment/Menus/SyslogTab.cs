using ParkAir___Assignment.Menus.SettingsHandlers;
using ParkAir___Assignment.Syslog;
using System.Net;

namespace ParkAir___Assignment.Menus
{
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
      this._sysServer = new(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
        (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
        (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );

      this._settings = settings;

      this._sysServer.Start();
      this.t_backgroundListenerThread = new(ScreenRefresh);
      this.t_backgroundListenerThread.Start();
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
      this._sysServer._listeningPort = (int)this._settings.Settings.SelectToken("Network.ListeningPort.Selected");
      this._sysServer._listeningIP =
        IPAddress.Parse((string)this._settings.Settings.SelectToken("Network.ListeningIp.Selected"));
      this._sysServer.Restart();

    }

    private void DeleteLocalStore()
    {
      this._sysServer._queue.Clear();
      this._sysServer._clearLogs = true;
      this._sysServer._log = new(250);
    }

    private void ChangeFilter(int index, ConsoleKey action)
    {
      switch (index)
      {
        case 0:
          this._pause = true;
          List<SysMessage> logsList = this._sysServer.GetLogs();
          List<string> autocompleteList = (from log in logsList select log.hostname).Distinct().ToList();

          this._ipfilter = _gui.Input(top: 3, left: 64, prefill: this._ipfilter != "" ? this._ipfilter : "",
            spaceholder: '_', max: 26, length: 26, autocomplete: autocompleteList);
          this._pause = false;
          break;

        case 1:
          this._severityFilter = Mod(this._severityFilter + (action == ConsoleKey.LeftArrow ? 0 : 2),
            this._color.Count + 1) - 1;
          break;
      }

      if (_gui?.Tabs[_gui._tabIndex] == this && !this._pause) _gui?.ScreenUpdate();
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {
      if (this._selectint < 3 - 1 && this._resetter > 0) this._resetter = 0;
      switch (key.Key)
      {
        case ConsoleKey.DownArrow:
          this._selectint = this._selectint + 1 == 3 - 1 + 1 ? this._selectint : this._selectint + 1;
          break;

        case ConsoleKey.UpArrow:
          this._selectint = this._selectint - 1 == -2 ? this._selectint : this._selectint - 1;
          break;

        case ConsoleKey.Enter:
          if (this._selectint is >= 0 and < 3 - 1) ChangeFilter(this._selectint, key.Key);
          switch (this._selectint)
          {
            case >= 3 - 1 when this._resetter > 0:
            {
              this._resetter++;
              if (this._resetter == 3)
              {
                DeleteLocalStore();
                this._resetter = 0;
              }

              break;
            }
            case >= 3 - 1:
              this._resetter++;
              break;
          }

          break;

        case ConsoleKey.RightArrow:
          if (this._selectint is >= 0 and < 3 - 1)
            ChangeFilter(this._selectint, key.Key);
          break;

        case ConsoleKey.LeftArrow:
          if (this._selectint is >= 0 and < 3 - 1)
            ChangeFilter(this._selectint, key.Key);
          break;
      }

      Tabber = this._selectint < 0;
    }

    public string Screen()
    {
      List<SysMessage> logs = this._sysServer.GetLogs();
      if (this._severityFilter >= 0)
        logs = (from log in logs where log.severity == this._severityFilter select log).ToList();

      if (this._ipfilter != "")
        logs = (from log in logs where log.hostname.ToUpper().Contains(this._ipfilter.ToUpper()) select log).ToList();


      List<SysMessage> lastAmount = logs.Skip(Math.Max(0, logs.Count - 15)).ToList();
      string screenReturn = "";

      string ipFilterString = $"{this._ipfilter}{new string('_', 26 - this._ipfilter.Length)}";
      screenReturn +=
        $"│[{(this._selectint == 0 ? ">" : " ")}] Filter IP{new(' ', 85 - 9 - ipFilterString.Length)}{ipFilterString} │\n";


      string sevcolor =
        $"{(this._severityFilter >= 0 ? new Color().FromSetting((string)this._settings.Settings.SelectToken($"Colors.{this._color[this._severityFilter]}.Selected")) : "")}";
      int offset = this._severityFilter >= 0 ? sevcolor.Length + 4 : 0;
      string sevFilterString =
        $"{(this._severityFilter >= 0 ? $"{sevcolor}{this._color[this._severityFilter]}\x1b[0m" : "UNSET")}";
      screenReturn +=
        $"│[{(this._selectint == 1 ? ">" : " ")}] Filter Type{new(' ', 83 - 11 - sevFilterString.Length + offset)}[{sevFilterString}] │\n";
      screenReturn += $"│{new string(' ', 90)}│\n";
      screenReturn +=
        $"│[{(this._selectint == 3 - 1 ? ">" : " ")}] Delete Local Store {(this._selectint == 3 - 1 ? this._resetter > 0 ? $"\x1b[31m(Press Enter {3 - this._resetter} More Time(s) to Confirm)\x1b[0m" : "(Press Enter)" : " ")}{new(' ', 85 - 19 - (this._selectint == 3 - 1 ? this._resetter > 0 ? 38 : 12 : 0))}│\n";
      screenReturn += $"├──────────┬─────────────────┬{new string('─', 61)}┤\n";

      int i = 0;
      foreach (SysMessage log in lastAmount)
      {
        string? color = new Color().FromSetting(
          (string)this._settings.Settings.SelectToken($"Colors.{this._color[log.severity]}.Selected"));
        screenReturn +=
          $"│ {log.timestamp.ToLongTimeString()} │{color} {$"[{this._color[log.severity].ToUpper()}]",-15}\x1b[0m │ {log.sysString}\x1b[0m\n";
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
          if (_gui?.Tabs[_gui._tabIndex] == this && !this._pause) _gui?.ScreenUpdate();
        }
        catch
        {
          // ignore
        }

        Thread.Sleep((int)3E3);
      }
    }
  }
}
