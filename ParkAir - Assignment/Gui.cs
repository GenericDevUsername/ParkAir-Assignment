using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment
{
  internal class Gui
  {
    public List<ITab> Tabs = new();
    public int TabIndex = 0;
    private Task? _inputTaskHandler;


    public void AddTab(ITab tab)
    {
      this.Tabs.Add(tab);
    }

    public void Start()
    {
      Console.Clear();
      this._inputTaskHandler = new(InputHandler);
      this._inputTaskHandler.Start();
      ScreenUpdate();
    }

    private static string PadBoth(string source, int length)
    {
      int spaces = length - source.Length;
      int padLeft = spaces/2 + source.Length;
      return source.PadLeft(padLeft).PadRight(length);
    }
    
    private string GenerateTabs(Gui gui)
    {
      string lineOne = ""; 
      string lineTwo = "";
      string lineThree = "";

      if (gui.Tabs.Count > 1)
      {
        lineOne += "┌";
        lineTwo += "│";
        lineThree += "├";
        foreach (ITab tab in this.Tabs)
        {
          lineOne += $"{new string('─', tab.TabName.Length + 4)}{(tab == this.Tabs[this.Tabs.Count -1] ? "┐" : "┬")}";
          lineTwo += $" {(this.Tabs[this.TabIndex] == tab ? "[" : " ")}{tab.TabName}{(this.Tabs[this.TabIndex] == tab ? "]" : " ")} │";
          lineThree += $"{new string('─', tab.TabName.Length + 4)}┴";
          if (tab == this.Tabs[(this.Tabs.Count - 1 < 0 ? 0 : this.Tabs.Count - 1)] && lineThree.Length < 92)
          {
            int lineCount = (92 - lineThree.Length < 0 ? 0 : 92 - lineThree.Length);
            lineThree = $"{lineThree}{new string('─', lineCount)}┐";
          }
          else if (tab == this.Tabs[(this.Tabs.Count - 1 < 0 ? 0 : this.Tabs.Count - 1)] && lineThree.Length == 92)
          {
            lineThree = $"{lineThree.Remove(lineThree.Length - 1, 1)}┤";
          }
          else if (tab == this.Tabs[(this.Tabs.Count - 1 < 0 ? 0 : this.Tabs.Count - 1)] && lineThree.Length > 92)
          {
            lineThree = $"{lineThree.Remove(lineThree.Length - 1, 1)}┘";
            char[] lineThreeArray = lineThree.ToCharArray();
            char[] lineTwoArray = lineTwo.ToCharArray();
            lineThreeArray[91] = lineTwoArray[91] == '│' ? '┼' : '┬';
            lineThree = new string (lineThreeArray);
          }
        }
      }
      else lineThree = "┌──────────────────────────────────────────────────────────────────────────────────────────┐";


      return $"{(lineOne.Length > 0 ? $"{lineOne}\n" : "")}{(lineTwo.Length > 0 ? $"{lineTwo}\n" : "")}{(lineThree.Length > 0 ? $"{lineThree}" : "")}";
    }

    private void ScreenUpdate()
    {
      string tabs = GenerateTabs(this);
      string screen = $"{tabs}\n{this.Tabs[this.TabIndex].Screen}";
      
      Console.SetCursorPosition(0,0);
      Console.WriteLine(screen);
    }

     private void InputHandler()
    {
      ConsoleKeyInfo key = Console.ReadKey(true);
      HandleKeypress(key);
    }

    private void HandleKeypress(ConsoleKeyInfo key)
    {
      if (this.Tabs.Count > 0)
      { 
        this.Tabs[this.TabIndex].HandleKeypress(key);
      }
      ScreenUpdate();
      InputHandler();
    } 
  }
}
