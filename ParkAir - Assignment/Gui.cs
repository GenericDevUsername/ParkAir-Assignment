using ParkAir___Assignment.Menus;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ParkAir___Assignment
{
  public class Gui
  {
    private readonly List<ITab> _tabs = new();
    private int _tabIndex;
    private Task? _inputTaskHandler;
    public bool Cursor = false;

    private readonly List<string> _debug = new() { "", "" };


    public void AddTab(ITab tab)
    {
      tab.Register(this);
      this._tabs.Add(tab);
    }

    public void Start()
    {
      Console.Clear();
      this._inputTaskHandler = new(InputHandler);
      this._inputTaskHandler.Start();
      ScreenUpdate();

    }



    public string Input(int left = -1, int top = -1, string prefill = "", string prompt = "", char? spaceholder = null, int? min = null, int? max = null, int length = -1, Regex? regexCheck = null, string customError = "")
    {
      // Assign dynamic location defaults based on current cursor position
      bool currentCursorVisibility = Console.CursorVisible;
      if (top == -1 || left == -1) (left, top) = Console.GetCursorPosition();
      if (length == -1) length = Console.WindowWidth - (left >= 0 ? left : 0);

      // Pre define variables based on above information (if a prefill is provided we must start the function at a point where it appears this input has already been typed in
      string inputOutput = prefill == "" ? "" : prefill;
      int currentIndex = prefill == "" ? 0 : prefill.Length - 1;
      int maxIndex = prefill == "" ? 0 : prefill.Length - 1;

      // Check if the prefill already surpasses the provided max char count, If it does throw an error
      if (max is not null && max < prefill.Length)
      {
        Exception error = new Exception("Max is smaller than provided prefill");
        throw error;
      }

      // Position cursor at provided location
      bool intercept = true;
      string errorMsg = "";
      while (intercept)
      {
        // update indexes
        maxIndex = inputOutput.Length;

        Console.CursorVisible = false;
        Console.SetCursorPosition(left, top);
        Console.Write($"{prompt}{(errorMsg == "" ? $"{inputOutput}{(spaceholder is not null ? new string(Convert.ToChar(spaceholder), length - inputOutput.Length) : "")}" : $"\u001b[91m{errorMsg}\u001b[0m")}");

        Console.SetCursorPosition(left + prompt.Length + currentIndex + 1, top);
        if ((length >= 0 ? currentIndex + 1 != length : true) || (max is not null ? currentIndex + 1 != max : true))
        {
          Console.CursorVisible = true;
        }

        if (errorMsg != "")
        {
          Thread.Sleep(150);
          errorMsg = "";
          continue;
        }
        ConsoleKeyInfo key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter)
        {
          if ((min is not null ? inputOutput.Length > min : true) && (max is not null ? inputOutput.Length <= max : true) && (length >= 0 ? inputOutput.Length <= length : true) && (regexCheck is not null ? regexCheck.Matches(inputOutput).Count > 0 : true))
          {
            intercept = false;
            break;
          }
          if (!(min is not null ? inputOutput.Length > min : true))
          {
            errorMsg = "Input too short!";
          }
          else if (!(max is not null ? inputOutput.Length <= max : true) || !(length >= 0 ? inputOutput.Length <= length : true))
          {
            errorMsg = "Input too long!";
          }
          else if (regexCheck is not null && regexCheck.Matches(inputOutput).Count <= 0)
          {
            errorMsg = (customError == "" ? "Failed regex!" : customError);
          }
          
        }
        else if (key.Key == ConsoleKey.Backspace && currentIndex >= 0)
        {
          currentIndex--;
          inputOutput = inputOutput.Remove(currentIndex+1, 1);
        }
        else if (key.Key == ConsoleKey.Delete && currentIndex + 1 < maxIndex)
        {
          inputOutput = inputOutput.Remove(currentIndex+1, 1);
        }
        else if ((key.Key == ConsoleKey.LeftArrow && currentIndex >= 0) || (key.Key == ConsoleKey.RightArrow && currentIndex + 1 < maxIndex))
        {
          switch (key.Key)
          {
            case ConsoleKey.LeftArrow:
              currentIndex--;
              break;

            case ConsoleKey.RightArrow:
              currentIndex++;
              break;
          }
        }
        else if ((Char.IsLetterOrDigit(key.KeyChar) || Char.IsPunctuation(key.KeyChar) ) && (max is not null ? inputOutput.Length +1 <= max : true) && (length >= 0 ? inputOutput.Length + 1 <= length : true))
        {
          inputOutput = inputOutput.Insert(currentIndex+1, key.KeyChar.ToString());
          currentIndex++;
        }
      }


      Console.CursorVisible = currentCursorVisibility;
      return inputOutput;
    }

    private string GenerateTabs(Gui gui)
    {
      string tabTop = "";
      string tabMiddle = "";
      string tabBottom = "";

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
            int lineCount = 92 - tabBottom.Length < 0 ? 0 : 92 - tabBottom.Length;
            tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}{new string('─', lineCount)}┐";
            char[] lineThreeArray = tabBottom.ToCharArray();
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
            char[] lineThreeArray = tabBottom.ToCharArray();
            char[] lineTwoArray = tabMiddle.ToCharArray();
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
