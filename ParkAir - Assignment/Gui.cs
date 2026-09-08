using System.Text;
using ParkAir___Assignment.Menus;
using System.Text.RegularExpressions;

namespace ParkAir___Assignment
{
  public class Gui
  {
    private const bool CURSOR = false;
    private readonly List<string> _debug = new() { "", "", "" };
    private Task? _inputTaskHandler;
    private int _lines;
    internal int _tabIndex;
    private readonly string? _menuTitle;
    private readonly Thread t_menuThread;


    public Gui(string? title = null)
    {
      this.t_menuThread = new(RunMenu);
      this._menuTitle = title;
    }

    public List<ITab> Tabs { get; } = new();

    public void AddTab(ITab tab)
    {
      tab.Register(this);
      Tabs.Add(tab);
    }

    /// <summary>
    /// Start the gui input handling thread
    /// </summary>
    public void Start()
    {
      this.t_menuThread.Start();
    }

    /// <summary>
    /// Thread function.
    /// Clears the startup messages and begins the loop for gathering user input.
    /// </summary>
    private void RunMenu()
    {
      this._inputTaskHandler = new(InputHandler);
      this._inputTaskHandler.Start();
      ScreenUpdate();
      Console.Clear();
      ScreenUpdate();
      while (true)
      {
        Thread.Sleep(1);
      }
    }

    /// <summary>
    /// Ran when a change is made to settings, it makes all the tabs with Restart set to True reload with the new settings.
    /// </summary>
    public void RefreshSettings()
    {
      foreach (ITab tab in Tabs.Where(tab => tab.Restart))
      {
        this._debug[2] = "reset";
        tab.RestartTab();
      }
    }

    /// <summary>
    /// Takes input from the user
    /// </summary>
    /// <param name="left">The row position of the cursor. Rows are numbered from top to bottom starting at 0.</param>
    /// <param name="top">The column position of the cursor. Columns are numbered from left to right starting at 0.</param>
    /// <param name="prefill">The prefilled value, as if it was already typed by the user.</param>
    /// <param name="prompt">The prompt placed before the input section.</param>
    /// <param name="spaceholder">The char to use instead of spaces within the input area.</param>
    /// <param name="min">Minimum length the input can be.</param>
    /// <param name="max">Max length the input can be.</param>
    /// <param name="length">The length of the users typing area. If -1 use Console.Width - left.</param>
    /// <param name="regexCheck">The regex check made before the user can submit their value.</param>
    /// <param name="customError">Custom error message displayed if the regex check is not successful.</param>
    /// <param name="autocomplete">Autocomplete list to display below the input.</param>
    /// <returns>The users input as a string.</returns>
    /// <exception cref="Exception">Thrown when max is smaller than the provided prefill.</exception>
    public string Input(int left = -1, int top = -1, string prefill = "", string prompt = "", char? spaceholder = null,
      int? min = null, int? max = null, int length = -1, Regex? regexCheck = null, string customError = "",
      List<string>? autocomplete = null)
    {
      // Assign dynamic location defaults based on current cursor position
      bool currentCursorVisibility = Console.CursorVisible;
      if (top == -1 || left == -1) (left, top) = Console.GetCursorPosition();
      if (length == -1) length = Console.WindowWidth - (left >= 0 ? left : 0);


      // Pre define variables based on above information
      // (if a prefill is provided we must start the function at a point where it appears this input has already been typed in)
      string inputOutput = prefill == "" ? "" : prefill;
      int currentIndex = prefill == "" ? -1 : prefill.Length - 1;

      // Check if the prefill already surpasses the provided max char count, If it does throw an error
      if (max is not null && max < prefill.Length)
      {
        Exception error = new("Max is smaller than provided prefill");
        throw error;
      }


      bool intercept = true;
      bool cancel = false;
      string errorMsg = "";
      while (intercept)
      {
        // refresh screen
        ScreenUpdate();
        if (autocomplete is not null)
        {
          Console.SetCursorPosition(left, top + 1);
          Console.WriteLine("\u001b[100;30mAutocomplete\u001b[0m");
          List<string> resultsList = new();
          resultsList.AddRange(autocomplete.Where(r => r.StartsWith(inputOutput)));
          if (resultsList.Count == 0)
          {
            Console.SetCursorPosition(left, top + 2);
            Console.WriteLine("\u001b[100;97mNo Recommendations Found\u001b[0m");
          }

          for (int i = 0; i < resultsList.Count && i < 11; i++)
          {
            Console.SetCursorPosition(left, top + 2 + i);
            switch (i)
            {
              case < 10:
                Console.WriteLine($"\u001b[100;97m{resultsList[i]}\u001b[0m");
                break;
              default:
                Console.WriteLine($"\u001b[100;97mAnd {resultsList.Count - i + 1} more...\u001b[0m");
                break;
            }
          }
        }

        // update indexes
        int maxIndex = inputOutput.Length;

        Console.CursorVisible = false;
        Console.SetCursorPosition(left, top); // Position cursor at provided location
        Console.Write(
          $"{prompt}{(errorMsg == "" ? $"{(spaceholder is not null ? inputOutput.Replace(' ', Convert.ToChar(spaceholder)) : inputOutput)}{(spaceholder is not null ? new(Convert.ToChar(spaceholder), length - inputOutput.Length) : "")}" : $"\u001b[91m{errorMsg}\u001b[0m")}");

        Console.SetCursorPosition(left + prompt.Length + currentIndex + 1, top);
        if (length < 0 || currentIndex + 1 != length || max is null || currentIndex + 1 != max)
          Console.CursorVisible = true;

        if (errorMsg != "")
        {
          Thread.Sleep(150);
          errorMsg = "";
          continue;
        }

        ConsoleKeyInfo key = Console.ReadKey(true);

        // Key handler 
        // Handles key presses while input is intercepting inputs from the user
        switch (key.Key)
        {
          case ConsoleKey.Escape:
            intercept = false;
            cancel = true;
            break;
          case ConsoleKey.Enter:
          {
            if ((min is null || inputOutput.Length > min) && (max is null || inputOutput.Length <= max) &&
                (length < 0 || inputOutput.Length <= length) &&
                (regexCheck is null || regexCheck.Matches(inputOutput).Count > 0))
              intercept = false;

            if (!(min is null || inputOutput.Length > min))
              errorMsg = "Input too short!";
            else if (!(max is null || inputOutput.Length <= max) || !(length < 0 || inputOutput.Length <= length))
              errorMsg = "Input too long!";
            else if (regexCheck is not null && regexCheck.Matches(inputOutput).Count <= 0)
              errorMsg = customError == "" ? "Failed regex!" : customError;
            break;
          }
          case ConsoleKey.Backspace when currentIndex >= 0:
            currentIndex--;
            inputOutput = inputOutput.Remove(currentIndex + 1, 1);
            break;
          case ConsoleKey.Delete when currentIndex + 1 < maxIndex:
            inputOutput = inputOutput.Remove(currentIndex + 1, 1);
            break;
          case ConsoleKey.LeftArrow when currentIndex >= 0:
          case ConsoleKey.RightArrow when currentIndex + 1 < maxIndex:
            switch (key.Key)
            {
              case ConsoleKey.LeftArrow:
                currentIndex--;
                break;

              case ConsoleKey.RightArrow:
                currentIndex++;
                break;
            }

            break;
          default:
          {
            if ((char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar)) &&
                (max is null || inputOutput.Length + 1 <= max) &&
                (length < 0 || inputOutput.Length + 1 <= length))
            {
              inputOutput = inputOutput.Insert(currentIndex + 1, key.KeyChar.ToString());
              currentIndex++;
            }
            else if (key.Key == ConsoleKey.Spacebar &&
                     (max is null || inputOutput.Length + 1 <= max) &&
                     (length < 0 || inputOutput.Length + 1 <= length))
            {
              inputOutput = inputOutput.Insert(currentIndex + 1, " ");
              currentIndex++;
            }

            break;
          }
        }
      }


      Console.CursorVisible = currentCursorVisibility;
      return cancel ? prefill : inputOutput;
    }

    /// <summary>
    /// Takes the tab list and generates the tablist navbar at the top of the screen
    /// </summary>
    /// <param name="gui">this class</param>
    /// <returns>string tab list</returns>
    private string GenerateTabs(Gui gui)
    {
      StringBuilder tabTop = new();
      StringBuilder tabMiddle = new();
      StringBuilder tabBottom = new();

      if (gui.Tabs.Count > 1)
      {
        tabTop.Append("┌");
        tabMiddle.Append("│");
        tabBottom.Append("├");
        foreach (ITab tab in Tabs)
        {
          tabTop.Append($"{new('─', tab.TabName.Length + 4)}{(tab == Tabs[^1] ? "┐" : "┬")}");
          tabMiddle.Append(
            $" {(Tabs[this._tabIndex] == tab ? $"{(Tabs[this._tabIndex].Tabber ? "[" : "\x1b[90m[\x1b[0m")}" : " ")}{tab.TabName}{(Tabs[this._tabIndex] == tab ? $"{(Tabs[this._tabIndex].Tabber ? "]" : "\x1b[90m]\x1b[0m")}" : " ")} │");
          tabBottom.Append($"{new('─', tab.TabName.Length + 4)}┴");

          if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
              tabBottom.Length - (Tabs[this._tabIndex].Tabber ? 0 : 9) < 92)
          {
            int lineCount = 92 - tabBottom.Length < 0 ? 0 : 92 - tabBottom.Length;
            string bottomString = tabBottom.ToString();
            tabBottom.Clear();
            tabBottom.Append($"{bottomString.Remove(bottomString.Length - 1, 1)}{new('─', lineCount)}┐");
            char[] lineThreeArray = tabBottom.ToString().ToCharArray();
            lineThreeArray[tabMiddle.Length - (Tabs[this._tabIndex].Tabber ? 0 : 18) - 1] = '┴';
            tabBottom.Clear();
            tabBottom.Append(string.Join("", lineThreeArray));
          }
          else if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
                   tabBottom.Length - (Tabs[this._tabIndex].Tabber ? 0 : 9) == 92)
          {
            StringBuilder tempBuilder = new();
            tempBuilder.Append(tabBottom.ToString());

            tabBottom.Clear();
            tabBottom.Append($"{tempBuilder.Remove(tempBuilder.Length - 1, 1)}┤");
          }
          else if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
                   tabBottom.Length - (Tabs[this._tabIndex].Tabber ? 0 : 9) > 92)
          {
            StringBuilder tempBuilder = new();
            tempBuilder.Append(tabBottom.ToString());

            tabBottom.Clear();
            tabBottom.Append($"{tempBuilder.Remove(tempBuilder.Length - 1, 1)}┘");
            char[] lineThreeArray = tabBottom.ToString().ToCharArray();
            char[] lineTwoArray = tabMiddle.ToString().ToCharArray();
            lineThreeArray[91] = lineTwoArray[Tabs[this._tabIndex].Tabber ? 91 : 96] == '│' ? '┼' : '┬';

            tabBottom.Clear();
            tabBottom.Append(string.Join("", lineThreeArray));
          }
        }
      }
      else
      {
        tabBottom.Clear();
        tabBottom.Append($"┌{new('─', 90)}┐");
      }


      return
        $"{(tabTop.Length > 0 ? $"{tabTop}\n" : "")}{(tabMiddle.Length > 0 ? $"{tabMiddle}\n" : "")}{(tabBottom.Length > 0 ? $"{tabBottom}" : "")}";
    }

    /// <summary>
    /// Updates the screen for the user with new information.
    /// </summary>
    internal void ScreenUpdate()
    {
      try
      {
        Console.Title = $"{(this._menuTitle is not null ? $"{this._menuTitle} - " : "")}{Tabs[this._tabIndex].TabName}";
      }
      catch
      {
        return;
      }

      string tabs = GenerateTabs(this);
      string screen = $"{tabs}\n{Tabs[this._tabIndex].Screen()}";
      List<string> screenLines = screen.Split("\n").ToList();
      screenLines.AddRange(this._debug);
      int newLines = screenLines.Count;

      for (int i = 0; i < screenLines.Count; i++)
        screenLines[i] =
          $"{screenLines[i]}{new string(' ', Console.BufferWidth - screenLines[i].Length < 0 ? 0 : Console.BufferWidth - screenLines[i].Length)}";

      for (int i = 0; i < this._debug.Count + this._lines - newLines; i++)
        screenLines.Add($"{new string(' ', Console.BufferWidth)}");

      this._lines = newLines;

      Console.SetCursorPosition(0, 0);
      Console.WriteLine($"{string.Join('\n', screenLines)}");
      try
      {
        Console.SetCursorPosition(0, screenLines.Count + this._debug.Count);
      }
      catch
      {

        Console.WindowHeight = screenLines.Count + this._debug.Count;

        Console.Clear();
        Console.WriteLine("Window is too small to contain the UI, please expand it.");
        while (true)
        {
          Thread.Sleep(100);
          try
          {
            Console.SetCursorPosition(0, screenLines.Count + this._debug.Count);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{string.Join('\n', screenLines)}");
            Console.SetCursorPosition(0, screenLines.Count + this._debug.Count);
            break;
          }
          catch
          {
            // ignored
          }
        }
      }
    }

    /// <summary>
    /// Takes a key input from the user and executes HandleKeypress
    /// </summary>
    private void InputHandler()
    {
      ConsoleKeyInfo key = Console.ReadKey(true);
      HandleKeypress(key);
    }

    /// <summary>
    /// Handles a users keypress, sending it to the correct tab class to be handled.
    /// </summary>
    /// <param name="key">the key pressed</param>
    private void HandleKeypress(ConsoleKeyInfo key)
    {
      if (Tabs[this._tabIndex].Tabber)
        this._tabIndex = key.Key switch
        {
          ConsoleKey.LeftArrow => this._tabIndex - 1 == -1 ? this._tabIndex : this._tabIndex - 1,
          ConsoleKey.RightArrow => this._tabIndex + 1 == Tabs.Count ? this._tabIndex : this._tabIndex + 1,
          _ => this._tabIndex
        };

      Console.CursorVisible = CURSOR;
      if (Tabs.Count > 0) Tabs[this._tabIndex].HandleKeypress(key);

      // DEBUG STRING UPDATER - REMOVE LATER
      // (Keeping this in my assignment so you can see the keys being handled)
      if (this._debug[0] == $"{key.Key.ToString()}          ")
      {
        int temp = Convert.ToInt32(this._debug[1].Replace(" ", "").Remove(0, 1));
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
