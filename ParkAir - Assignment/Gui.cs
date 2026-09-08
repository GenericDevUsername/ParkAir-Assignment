using System.Text.RegularExpressions;
using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment;

public class Gui
{
    private readonly List<string> _debug = new() { "", "" };
    private Task? _inputTaskHandler;
    private int _tabIndex;
    private const bool Cursor = false;
    public List<ITab> Tabs { get; } = new();


    public void AddTab(ITab tab)
    {
        tab.Register(this);
        Tabs.Add(tab);
    }

    public void Start()
    {
        Console.Clear();
        _inputTaskHandler = new Task(InputHandler);
        _inputTaskHandler.Start();
        ScreenUpdate();
    }


    public string Input(int left = -1, int top = -1, string prefill = "", string prompt = "", char? spaceholder = null,
        int? min = null, int? max = null, int length = -1, Regex? regexCheck = null, string customError = "")
    {
        // Assign dynamic location defaults based on current cursor position
        var currentCursorVisibility = Console.CursorVisible;
        if (top == -1 || left == -1) (left, top) = Console.GetCursorPosition();
        if (length == -1) length = Console.WindowWidth - (left >= 0 ? left : 0);

        // Pre define variables based on above information (if a prefill is provided we must start the function at a point where it appears this input has already been typed in
        var inputOutput = prefill == "" ? "" : prefill;
        var currentIndex = prefill == "" ? 0 : prefill.Length - 1;

        // Check if the prefill already surpasses the provided max char count, If it does throw an error
        if (max is not null && max < prefill.Length)
        {
            var error = new Exception("Max is smaller than provided prefill");
            throw error;
        }

        // Position cursor at provided location
        var intercept = true;
        var cancel = false;
        var errorMsg = "";
        while (intercept)
        {
            // update indexes
            var maxIndex = inputOutput.Length;

            Console.CursorVisible = false;
            Console.SetCursorPosition(left, top);
            Console.Write(
                $"{prompt}{(errorMsg == "" ? $"{inputOutput}{(spaceholder is not null ? new string(Convert.ToChar(spaceholder), length - inputOutput.Length) : "")}" : $"\u001b[91m{errorMsg}\u001b[0m")}");

            Console.SetCursorPosition(left + prompt.Length + currentIndex + 1, top);
            if ((length < 0 || currentIndex + 1 != length) || (max is null || currentIndex + 1 != max))
                Console.CursorVisible = true;

            if (errorMsg != "")
            {
                Thread.Sleep(150);
                errorMsg = "";
                continue;
            }

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
            {
                intercept = false;
                cancel = true;
            }
            else if (key.Key == ConsoleKey.Enter)
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
            }
            else if (key.Key == ConsoleKey.Backspace && currentIndex >= 0)
            {
                currentIndex--;
                inputOutput = inputOutput.Remove(currentIndex + 1, 1);
            }
            else if (key.Key == ConsoleKey.Delete && currentIndex + 1 < maxIndex)
            {
                inputOutput = inputOutput.Remove(currentIndex + 1, 1);
            }
            else if ((key.Key == ConsoleKey.LeftArrow && currentIndex >= 0) ||
                     (key.Key == ConsoleKey.RightArrow && currentIndex + 1 < maxIndex))
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
            else if ((char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar)) &&
                     (max is null || inputOutput.Length + 1 <= max) &&
                     (length < 0 || inputOutput.Length + 1 <= length))
            {
                inputOutput = inputOutput.Insert(currentIndex + 1, key.KeyChar.ToString());
                currentIndex++;
            }
        }


        Console.CursorVisible = currentCursorVisibility;
        return cancel ? prefill : inputOutput;
    }

    private string GenerateTabs(Gui gui)
    {
        var tabTop = "";
        var tabMiddle = "";
        var tabBottom = "";

        if (gui.Tabs.Count > 1)
        {
            tabTop += "┌";
            tabMiddle += "│";
            tabBottom += "├";
            foreach (var tab in Tabs)
            {
                tabTop += $"{new string('─', tab.TabName.Length + 4)}{(tab == Tabs[^1] ? "┐" : "┬")}";
                tabMiddle +=
                    $" {(Tabs[_tabIndex] == tab ? $"{(Tabs[_tabIndex].Tabber ? "[" : "\x1b[90m[\x1b[0m")}" : " ")}{tab.TabName}{(Tabs[_tabIndex] == tab ? $"{(Tabs[_tabIndex].Tabber ? "]" : "\x1b[90m]\x1b[0m")}" : " ")} │";
                tabBottom += $"{new string('─', tab.TabName.Length + 4)}┴";
                if (tab == Tabs[Tabs.Count - 1 < 0 ? 0 : Tabs.Count - 1] &&
                    tabBottom.Length - (Tabs[_tabIndex].Tabber ? 0 : 9) < 92)
                {
                    var lineCount = 92 - tabBottom.Length < 0 ? 0 : 92 - tabBottom.Length;
                    tabBottom = $"{tabBottom.Remove(tabBottom.Length - 1, 1)}{new string('─', lineCount)}┐";
                    var lineThreeArray = tabBottom.ToCharArray();
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
                    var lineThreeArray = tabBottom.ToCharArray();
                    var lineTwoArray = tabMiddle.ToCharArray();
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

    private void ScreenUpdate()
    {
        var tabs = GenerateTabs(this);
        var screen = $"{tabs}\n{Tabs[_tabIndex].Screen()}";
        var screenLines = screen.Split("\n").ToList();
        screenLines.AddRange(_debug);
        for (var i = 0; i < screenLines.Count; i++)
            screenLines[i] = $"{screenLines[i]}{new string(' ', Console.BufferWidth - screenLines[i].Length)}";

        for (var i = 0; i < Console.WindowHeight - screenLines.Count; i++)
            screenLines.Add($"{new string(' ', Console.BufferWidth)}");

        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"{string.Join('\n', screenLines)}");
    }

    private void InputHandler()
    {
        var key = Console.ReadKey(true);
        HandleKeypress(key);
    }

    private void HandleKeypress(ConsoleKeyInfo key)
    {
        if (Tabs[_tabIndex].Tabber)
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    _tabIndex = _tabIndex - 1 == -1 ? _tabIndex : _tabIndex - 1;
                    break;

                case ConsoleKey.RightArrow:
                    _tabIndex = _tabIndex + 1 == Tabs.Count ? _tabIndex : _tabIndex + 1;
                    break;
            }

        Console.CursorVisible = Cursor;
        if (Tabs.Count > 0) Tabs[_tabIndex].HandleKeypress(key);

        // DEBUG STRING UPDATER - REMOVE LATER
        if (_debug[0] == key.Key.ToString())
        {
            var temp = Convert.ToInt32(_debug[1].Replace(" ", "").Remove(0, 1));
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