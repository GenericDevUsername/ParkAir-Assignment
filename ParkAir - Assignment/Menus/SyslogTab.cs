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

    private string _ipfilter = "";
    private string _severityFilter = "";
    private int _resetter;

    private int _selectint = -1;
    

    private readonly SettingsTab _settings;

    private readonly Thread t_backgroundListenerThread;
    public SyslogServer sysServer;

    public SyslogTab(SettingsTab settings)
    {
      this.sysServer = new(IPAddress.Parse((string)settings.Settings.SelectToken("Network.ListeningIp.Selected")),
        (int)settings.Settings.SelectToken("Network.ListeningPort.Selected"),
        (string)settings.Settings.SelectToken("Network.ListeningType.Selected")
      );

      this._settings = settings;

      this.sysServer.Start();
      this.t_backgroundListenerThread = new(ScreenRefresh);
      this.t_backgroundListenerThread.Start();
    }

    public string TabName { get; set; } = "Syslog";
    public string ScreenString { get; private set; } = "";
    public bool Tabber { get; private set; } = true;
    public Gui? _gui { get; private set; }
    public bool Restart { get; } = true;

    public void RestartTab()
    {
      this.sysServer._listeningPort = (int)this._settings.Settings.SelectToken("Network.ListeningPort.Selected");
      this.sysServer._listeningIP =
        IPAddress.Parse((string)this._settings.Settings.SelectToken("Network.ListeningIp.Selected"));
      this.sysServer.Restart();

    }

    private void ResetFilter()
    {
      this._ipfilter = "";
      this._severityFilter = "";
      
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {
      if (this._selectint < 3-1 && this._resetter > 0) this._resetter = 0;
      switch (key.Key)
      {
        case ConsoleKey.DownArrow:
          this._selectint = this._selectint + 1 == 3-1 + 1 ? this._selectint : this._selectint + 1;
          break;

        case ConsoleKey.UpArrow:
          this._selectint = this._selectint - 1 == -2 ? this._selectint : this._selectint - 1;
          break;
        
        case ConsoleKey.Enter:
          if (this._selectint is >= 0 and < 3 - 1)
          {
            //ChangeSetting(this._selectionIndex[this._selectint], key);
          }
          if (this._selectint >= 3-1 && this._resetter > 0)
          {
            this._resetter++;
            if (this._resetter == 3)
            {
              ResetFilter();
              this._resetter = 0;
            }
          }
          else if (this._selectint >= 3-1)
          {
            this._resetter++;
          }

          break;
      }
      
      Tabber = this._selectint < 0;
    }

    public string Screen()
    {
      List<SysMessage> logs = this.sysServer.GetLogs();

      
      
      List<SysMessage> lastAmount = logs.Skip(Math.Max(0, logs.Count - 15)).ToList();
      var screenReturn = "";
      screenReturn += $"│[{(this._selectint == 0 ? ">" : " ")}] Filter IP{new(' ', 85 - 9 - this._ipfilter.Length)}{this._ipfilter} │\n";
      screenReturn += $"│[{(this._selectint == 1 ? ">" : " ")}] Filter Port{new(' ', 85 - 11 - this._ipfilter.Length)}{this._ipfilter} │\n";
      screenReturn += $"│{new string(' ', 90)}│\n";
      screenReturn +=
        $"│[{(this._selectint == 3 - 1 ? ">" : " ")}] Delete Local Store {(this._selectint == 3 - 1 ? this._resetter > 0 ? $"\x1b[31m(Press Enter {3 - this._resetter} More Time(s) to Confirm)\x1b[0m" : "(Press Enter)" : " ")}{new(' ', 85 - 19 - (this._selectint == 3 - 1 ? this._resetter > 0 ? 38 : 12 : 0))}│\n";
      screenReturn += $"├──────────┬─────────────────┬{new string('─', 61)}┤\n";

      int i = 0;
      foreach (SysMessage log in lastAmount)
      {
        var color = new Color().FromSetting((string)this._settings.Settings.SelectToken($"Colors.{this._color[log.severity]}.Selected"));
        screenReturn += $"│ {log.timestamp.ToLongTimeString()} │{color} {$"[{this._color[log.severity].ToUpper()}]",-15}\x1b[0m │ {log.sysString}\x1b[0m\n";
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
        if (_gui?.Tabs[_gui._tabIndex] == this) _gui?.ScreenUpdate();
        Thread.Sleep((int)3E3);
      }
    }
  }
}
