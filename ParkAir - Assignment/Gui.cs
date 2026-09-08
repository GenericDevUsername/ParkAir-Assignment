using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment
{
  internal class Gui
  {
    private readonly List<ITab> _tabs = new();
    private int _tabIndex;
    private Task? _inputTaskHandler;
    public bool Cursor = false;

    private readonly List<string> _debug = new() { "", "" };


    public void AddTab(ITab tab)
    {
      this._tabs.Add(tab);
    }

    public void Start()
    {
      Console.Clear();
      this._inputTaskHandler = new(InputHandler);
      this._inputTaskHandler.Start();
      ScreenUpdate();
    }

    private string GenerateTabs(Gui gui)
    {
      var tabTop = "";
      var tabMiddle = "";
      var tabBottom = "";

      if (gui._tabs.Count > 1)
      {
        tabTop += "┌";
        tabMiddle += "│";
        tabBottom += "├";
        foreach (ITab tab in this._tabs)
        {
          tabTop += $"{new string('─', tab.TabName.Length + 4)}{(tab == this._tabs[^1] ? "┐" : "┬")}";
          tabMiddle +=
            $" {(this._tabs[this._tabIndex] == tab ? $"{(this._tabs[this._tabIndex].Tabber ? "[" : "\x1b[90m[\x1b[0m")}" : " ")}{tab.TabName}{(this._tabs[this._tabIndex] == tab ? $"{(this._tabs[this._tabIndex].Tabber ? "]" : "\x1b[90m]\x1b[0m")}" : " ")} │";
          tabBottom += $"{new string('─', tab.TabName.Length + 4)}┴";
          if (tab == this._tabs[this._tabs.Count - 1 < 0 ? 0 : this._tabs.Count - 1] &&
              tabBottom.Length - (this._tabs[this._tabIndex].Tabber ? 0 : 9) < 92)
          {
            var lineCount = 92 - tabBottom.Length < 0 ? 0 : 92 - tabBottom.Length;
            tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}{new string('─', lineCount)}┐";
            var lineThreeArray = tabBottom.ToCharArray();
            lineThreeArray[tabMiddle.Length - (this._tabs[this._tabIndex].Tabber ? 0 : 18) - 1] = '┴';
            tabBottom = new(lineThreeArray);
          }
          else if (tab == this._tabs[this._tabs.Count - 1 < 0 ? 0 : this._tabs.Count - 1] &&
                   tabBottom.Length - (this._tabs[this._tabIndex].Tabber ? 0 : 9) == 92)
          {
            tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}┤";
          }
          else if (tab == this._tabs[this._tabs.Count - 1 < 0 ? 0 : this._tabs.Count - 1] &&
                   tabBottom.Length - (this._tabs[this._tabIndex].Tabber ? 0 : 9) > 92)
          {
            tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}┘";
            var lineThreeArray = tabBottom.ToCharArray();
            var lineTwoArray = tabMiddle.ToCharArray();
            lineThreeArray[91] = lineTwoArray[this._tabs[this._tabIndex].Tabber ? 91 : 96] == '│' ? '┼' : '┬';
            tabBottom = new(lineThreeArray);
          }
        }
      }
      else
      {
        tabBottom = $"┌{new string('─', 90)}┐";
      }


      return
        $"{(tabTop.Length > 0 ? $"{tabTop}\n" : "")}{(tabMiddle.Length > 0 ? $"{tabMiddle}\n" : "")}{(tabBottom.Length > 0 ? $"{tabBottom}" : "")}";
    }

    private void ScreenUpdate()
    {
      var tabs = GenerateTabs(this);
      var screen = $"{tabs}\n{this._tabs[this._tabIndex].Screen()}";

      Console.SetCursorPosition(0, 0);
      Console.WriteLine(screen + $"\n{string.Join('\n', this._debug.ToArray())}");
    }

    private void InputHandler()
    {
      ConsoleKeyInfo key = Console.ReadKey(true);
      HandleKeypress(key);
    }

    private void HandleKeypress(ConsoleKeyInfo key)
    {
      if (this._tabs[this._tabIndex].Tabber)
        switch (key.Key)
        {
          case ConsoleKey.LeftArrow:
            this._tabIndex = this._tabIndex - 1 == -1 ? this._tabIndex : this._tabIndex - 1;
            break;

          case ConsoleKey.RightArrow:
            this._tabIndex = this._tabIndex + 1 == this._tabs.Count ? this._tabIndex : this._tabIndex + 1;
            break;
        }

      Console.CursorVisible = this.Cursor;
      if (this._tabs.Count > 0) this._tabs[this._tabIndex].HandleKeypress(key);

      // DEBUG STRING UPDATER - REMOVE LATER
      if (this._debug[0] == key.Key.ToString())
      {
        var temp = Convert.ToInt32(this._debug[1].Replace(" ", "").Remove(0, 1));
        temp++;
        this._debug[1] = $"x{temp}          ";
      }
      else
      {
        this._debug[0] = $"{key.Key.ToString()}          ";
        this._debug[1] = "x1          ";
      }

      ScreenUpdate();
      InputHandler();
    }
  }
}
