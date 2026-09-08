using System.Text;
using ParkAir___Assignment.Menus.ExportHandlers;

namespace ParkAir___Assignment.Menus
{
  public class ExportTab : ITab
  {

    public string TabName { get; set; } = "Export";
    public string ScreenString { get; } = "";
    public bool Restart { get; } = false;
    public Gui? _gui { get; private set; }
    public bool Tabber { get; private set; } = true;

    private int _selectint;

    private readonly Dictionary<int, string> _fileTypes = new()
    {
      { 0, "TXT" },
      { 1, "CSV" }
    };

    private int _fileType;

    private readonly Dictionary<int, string> _exportTypes = new()
    {
      { 0, "MULTI" },
      { 1, "SINGLE" }
    };

    private int _exportType;
    private bool _exporting = false;

    private static int Mod(int k, int n)
    {
      return (k %= n) < 0 ? k + n : k;
    }

    private void ChangeFilter(int index, ConsoleKey action)
    {
      Console.WriteLine(index);
      switch (index)
      {
        case 2:
          this._fileType = Mod(this._fileType + (action == ConsoleKey.LeftArrow ? -1 : 1), this._fileTypes.Count);
          break;

        case 1:
          this._exportType = Mod(this._exportType + (action == ConsoleKey.LeftArrow ? -1 : 1), this._exportTypes.Count);
          break;
      }
    }

    private void ExportFiles()
    {
      switch (this._exportTypes[this._exportType])
      {
        case "SINGLE":
          this._exporting = true;
          _gui.ScreenUpdate();
          SysExport.ExportSingle(this._fileTypes[this._fileType]);
          Console.Clear();
          _gui.ScreenUpdate();

          this._exporting = false;
          break;

        case "MULTI":
          this._exporting = true;
          _gui.ScreenUpdate();
          SysExport.ExportMulti(this._fileTypes[this._fileType]);
          Console.Clear();
          _gui.ScreenUpdate();

          this._exporting = false;
          break;
      }
    }

    public void HandleKeypress(ConsoleKeyInfo key)
    {
      switch (key.Key)
      {
        case ConsoleKey.DownArrow:
          this._selectint = this._selectint + 1 == 4 - 1 + 1 ? this._selectint : this._selectint + 1;
          break;

        case ConsoleKey.UpArrow:
          this._selectint = this._selectint - 1 == -1 ? this._selectint : this._selectint - 1;
          break;

        case ConsoleKey.Enter:
          if (this._selectint is >= 0 and < 3)
            ChangeFilter(this._selectint, key.Key);
          else if (this._selectint == 3) ExportFiles();


          break;

        case ConsoleKey.RightArrow:
          if (this._selectint is > 0 and < 3)
            ChangeFilter(this._selectint, key.Key);
          break;

        case ConsoleKey.LeftArrow:
          if (this._selectint is > 0 and < 3)
            ChangeFilter(this._selectint, key.Key);
          break;
      }

      Tabber = this._selectint <= 0;
    }

    public string Screen()
    {
      StringBuilder screenReturn = new();

      screenReturn.Append(
        $"│[{(this._selectint == 1 ? ">" : " ")}] Export Type{new(' ', 83 - 12 - 18)}[{(this._exportType == 0 ? "X" : " ")}] MULTI  [{(this._exportType == 1 ? "X" : " ")}] SINGLE │\n");
      screenReturn.Append(
        $"│[{(this._selectint == 2 ? ">" : " ")}] File Type{new(' ', 83 - 10 - 18)}[{(this._fileType == 0 ? "X" : " ")}] TXT    [{(this._fileType == 1 ? "X" : " ")}] CSV    │\n");
      screenReturn.Append($"│{new(' ', 90)}│\n");
      screenReturn.Append(
        $"│[{(this._selectint == 3 ? ">" : " ")}] Export {(!this._exporting ? this._selectint == 3 ? "(Press Enter)" : "             " : "(Exporting..)")}{new(' ', 83 - 17)}│\n");
      screenReturn.Append($"└{new('─', 90)}┘");

      return screenReturn.ToString();
    }

    public void RestartTab()
    {

    }

    public void Register(Gui gui)
    {
      _gui = gui;
    }
  }
}
