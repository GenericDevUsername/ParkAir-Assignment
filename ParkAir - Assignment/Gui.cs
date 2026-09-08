using ParkAir___Assignment.Menus;
using System.Text.RegularExpressions;

namespace ParkAir___Assignment;

public class Gui
{
  public string? menuTitle = null;
  internal readonly List<string> _debug = new() { "", "", "" };
  private Task? _inputTaskHandler;
  internal int _tabIndex;
  private int _lines = 0;
  private const bool CURSOR = false;
  public List<ITab> Tabs { get; } = new();
  private Thread t_menuThread;


  public Gui(string? title = null)
  {
    this.t_menuThread = new(new ThreadStart(RunMenu));
    menuTitle = title;
  }

  public void AddTab(ITab tab)
  {
    tab.Register(this);
    Tabs.Add(tab);
  }

  public void Start()
  {
    this.t_menuThread.Start();
  }

  private void RunMenu()
  {
    Console.Clear();
    _inputTaskHandler = new Task(InputHandler);
    _inputTaskHandler.Start();
    ScreenUpdate();
    while (true) Thread.Sleep(1);
  }


  public string Input(int left = -1, int top = -1, string prefill = "", string prompt = "", char? spaceholder = null,
      int? min = null, int? max = null, int length = -1, Regex? regexCheck = null, string customError = "", List<string>? autocomplete = null)
  {
    // Assign dynamic location defaults based on current cursor position
    bool currentCursorVisibility = Console.CursorVisible;
    if (top == -1 || left == -1) (left, top) = Console.GetCursorPosition();
    if (length == -1) length = Console.WindowWidth - (left >= 0 ? left : 0);

    // Pre define variables based on above information
    // (if a prefill is provided we must start the function at a point where it appears this input has already been typed in)
    string inputOutput = prefill == "" ? "" : prefill;
    int currentIndex = prefill == "" ? 0 : prefill.Length - 1;

    // Check if the prefill already surpasses the provided max char count, If it does throw an error
    if (max is not null && max < prefill.Length)
    {
      Exception error = new Exception("Max is smaller than provided prefill");
      throw error;
    }

    
    bool intercept = true;
    bool cancel = false;
    string errorMsg = "";
    while (intercept)
    {
      // refresh screen
      this.ScreenUpdate();
      if (autocomplete is not null)
      {
        Console.SetCursorPosition(left, top+1);
        Console.WriteLine($"\u001b[100;30mAutocomplete\u001b[0m");
        List<string> resultsList = new List<string>();
        resultsList.AddRange(autocomplete.Where(r => r.StartsWith(inputOutput)));
        if (resultsList.Count == 0)
        {
          Console.SetCursorPosition(left, top+2);
          Console.WriteLine($"\u001b[100;97mNo Recommendations Found\u001b[0m");
        }
        for (int i = 0; i < resultsList.Count; i++)
        {
          Console.SetCursorPosition(left, top+2+i);
          Console.WriteLine($"\u001b[100;97m{resultsList[i]}\u001b[0m");
        }
      }
      // update indexes
      int maxIndex = inputOutput.Length;

      Console.CursorVisible = false;
      Console.SetCursorPosition(left, top); // Position cursor at provided location
      Console.Write(
          $"{prompt}{(errorMsg == "" ? $"{(spaceholder is not null ? inputOutput.Replace(' ', Convert.ToChar(spaceholder)) : inputOutput)}{(spaceholder is not null ? new string(Convert.ToChar(spaceholder), length - inputOutput.Length) : "")}" : $"\u001b[91m{errorMsg}\u001b[0m")}");
      
      Console.SetCursorPosition(left + prompt.Length + currentIndex + 1, top);
      if ((length < 0 || currentIndex + 1 != length) || (max is null || currentIndex + 1 != max))
        Console.CursorVisible = true;

      if (errorMsg != "")
      {
        Thread.Sleep(150);
        errorMsg = "";
        continue;
      }

      ConsoleKeyInfo key = Console.ReadKey(true);
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
          {
            intercept = false;
          }

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
          else if (key.Key == ConsoleKey.Spacebar&&
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

  private string GenerateTabs(Gui gui)
  {
    string tabTop = "";
    string tabMiddle = "";
    string tabBottom = "";

    if (gui.Tabs.Count > 1)
    {
      tabTop += "┌";
      tabMiddle += "│";
      tabBottom += "├";
      foreach (ITab tab in Tabs)
      {
        tabTop += $"{new string('─', tab.TabName.Length + 4)}{(tab == Tabs[^1] ? "┐" : "┬")}";
        tabMiddle +=
            $" {(Tabs[_tabIndex] == tab ? $"{(Tabs[_tabIndex].Tabber ? "[" : "\x1b[90m[\x1b[0m")}" : " ")}{tab.TabName}{(Tabs[_tabIndex] == tab ? $"{(Tabs[_tabIndex].Tabber ? "]" : "\x1b[90m]\x1b[0m")}" : " ")} │";
        tabBottom += $"{new string('─', tab.TabName.Length + 4)}┴";
        if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
            tabBottom.Length - (Tabs[_tabIndex].Tabber ? 0 : 9) < 92)
        {
          int lineCount = 92 - tabBottom.Length < 0 ? 0 : 92 - tabBottom.Length;
          tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}{new string('─', lineCount)}┐";
          char[] lineThreeArray = tabBottom.ToCharArray();
          lineThreeArray[tabMiddle.Length - (Tabs[_tabIndex].Tabber ? 0 : 18) - 1] = '┴';
          tabBottom = new string(lineThreeArray);
        }
        else if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
                  tabBottom.Length - (Tabs[_tabIndex].Tabber ? 0 : 9) == 92)
        {
          tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}┤";
        }
        else if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
                  tabBottom.Length - (Tabs[_tabIndex].Tabber ? 0 : 9) > 92)
        {
          tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}┘";
          char[] lineThreeArray = tabBottom.ToCharArray();
          char[] lineTwoArray = tabMiddle.ToCharArray();
          lineThreeArray[91] = lineTwoArray[Tabs[_tabIndex].Tabber ? 91 : 96] == '│' ? '┼' : '┬';
          tabBottom = new string(lineThreeArray);
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

  internal void ScreenUpdate()
  {
    Console.Title = $"{(menuTitle is not null ? $"{menuTitle} - " : "")}{Tabs[_tabIndex].TabName}";
    string tabs = GenerateTabs(this);
    string screen = $"{tabs}\n{Tabs[_tabIndex].Screen()}";
    List<string> screenLines = screen.Split("\n").ToList();
    screenLines.AddRange(_debug);
    int newLines = screenLines.Count;
    for (int i = 0; i < screenLines.Count; i++)
      screenLines[i] = $"{screenLines[i]}{new string(' ', (Console.BufferWidth - screenLines[i].Length < 0 ? 0 : Console.BufferWidth - screenLines[i].Length ))}";
      
    for (int i = 0; i < this._debug.Count + this._lines - newLines; i++)
      screenLines.Add($"{new string(' ', Console.BufferWidth)}");

    this._lines = newLines;

    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"{string.Join('\n', screenLines)}");
    Console.SetCursorPosition(0, screenLines.Count + this._debug.Count);
  }

  private void InputHandler()
  {
    ConsoleKeyInfo key = Console.ReadKey(true);
    HandleKeypress(key);
  }

  private void HandleKeypress(ConsoleKeyInfo key)
  {
    if (Tabs[_tabIndex].Tabber)
      _tabIndex = key.Key switch
      {
        ConsoleKey.LeftArrow => _tabIndex - 1 == -1 ? _tabIndex : _tabIndex - 1,
        ConsoleKey.RightArrow => _tabIndex + 1 == Tabs.Count ? _tabIndex : _tabIndex + 1,
        _ => _tabIndex
      };

    Console.CursorVisible = CURSOR;
    if (Tabs.Count > 0) Tabs[_tabIndex].HandleKeypress(key);

    // DEBUG STRING UPDATER - REMOVE LATER
    if (_debug[0] == key.Key.ToString())
    {
      int temp = Convert.ToInt32(_debug[1].Replace(" ", "").Remove(0, 1));
      temp++;
      _debug[1] = $"x{temp}          ";
    }
    else
    {
      _debug[0] = $"{key.Key.ToString()}          ";
      _debug[1] = "x1          ";
    }

    ScreenUpdate();
    InputHandler();
  }
}