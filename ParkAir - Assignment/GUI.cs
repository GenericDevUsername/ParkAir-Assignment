using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParkAir___Assignment
{
  internal class GUI
  {
    private string Menu = "";
    private Task inputTaskHandler;

    public GUI()
    {
      
    }

    public void Start()
    {
      this.inputTaskHandler = new(Tick);
      this.inputTaskHandler.Start();
    }

     void Tick()
    {
      Console.WriteLine("Waiting for input");
      ConsoleKeyInfo key = Console.ReadKey(true);
      Console.WriteLine(key.Key.ToString());

      Tick();
    }
  }
}
