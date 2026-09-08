using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment
{
  internal class Gui
  {
    private string _menu = "";
    public List<ITab> Tabs = new();
    public int TabIndex = 0;
    
    
    internal static Action? keyOveride;

    private Task? _inputTaskHandler;


    public void AddTab(ITab tab)
    {
      this.Tabs.Add(tab);
    }

    public void Start()
    {
      this._inputTaskHandler = new(InputHandler);
      this._inputTaskHandler.Start();
    }


    private void ScreenUpdate()
    {
      string temp = @"
┌──────────┬──────────┬────────────┬────────────┬────────────┬────────────┬────────────┬────────────┬────────────┬
│  Syslog  │  Export  │ [Settings] │
├──────────┴──────────┴────────────┴────────────┴────────────┴────────────┴────────────┴───┐
";
      string lineOne = ""; 
      string lineTwo = "";
      string lineThree = "";
      string screen = @$"";
      
      Console.SetCursorPosition(0,0);
      
    }

     private void InputHandler()
    {
      Console.WriteLine("Waiting for input");
      ConsoleKeyInfo key = Console.ReadKey(true);
      Console.WriteLine(key.Key.ToString());
      HandleKeypress(key);
    }

    private void HandleKeypress(ConsoleKeyInfo key)
    {
      if (keyOveride is null)
      {
        if (this.Tabs.Count > 0)
        { 
          this.Tabs[this.TabIndex].HandleKeypress(key);
        }
      }
      else keyOveride();

      ScreenUpdate();
      InputHandler();
    } 
  }
}
