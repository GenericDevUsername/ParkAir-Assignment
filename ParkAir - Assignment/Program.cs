using System.Text;
using Newtonsoft.Json.Linq;
using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment
{
  internal class Program
  {
    private static void Main()
    {
      Console.WriteLine("Initialising...");
      var dir = Directory.GetCurrentDirectory();

      Gui menuHandler = new();
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));

      menuHandler.Start();
      while (true)
      {
        Thread.Sleep(1);
      }
    }

    private static SettingsTab InitSettings(string fp)
    {
      if (!File.Exists(fp))
      {
        Console.WriteLine("Generating Settings File...");

        JObject settingsTemplate = JObject.FromObject(new
        {
          FirstLaunch = true,
          Network = new
          {
            ListeningType = new
            {
              InputType = "SingleSelect",
              Options = new List<string> { "TCP", "UDP", "BOTH" },
              Selected = "BOTH",
              Default = "BOTH"
            },
            IpType = new
            {
              InputType = "SingleSelect",
              Options = new List<string> { "IPV4", "IPV6", "BOTH" },
              Selected = "BOTH",
              Default = "BOTH"
            },
            ListeningIp = new
            {
              InputType = "IP",
              Selected = "LocalHost",
              Default = "LocalHost"
            },
            ListeningPort = new
            {
              InputType = "Port",
              Selected = "514",
              Default = "514"
            }
          },
          Colors = new
          {
            Emergency = new
            {
              InputType = "Color",
              Selected = "DarkBlue",
              Default = "DarkBlue"
            },
            Alert = new
            {
              InputType = "Color",
              Selected = "DarkBlue",
              Default = "DarkBlue"
            },
            Critical = new
            {
              InputType = "Color",
              Selected = "DarkBlue",
              Default = "DarkBlue"
            },
            Error = new
            {
              InputType = "Color",
              Selected = "Red",
              Default = "Red"
            },
            Warning = new
            {
              InputType = "Color",
              Selected = "Yellow",
              Default = "Yellow"
            },
            Notice = new
            {
              InputType = "Color",
              Selected = "Yellow",
              Default = "Yellow"
            },
            Informational = new
            {
              InputType = "Color",
              Selected = "Black",
              Default = "Black"
            },
            Debug = new
            {
              InputType = "Color",
              Selected = "DarkBlue",
              Default = "DarkBlue"
            }
          }
        });

        //Console.WriteLine(settingsTemplate.ToString());

        try
        {
          // Create the file, or overwrite if the file exists.
          using (FileStream fs = File.Create(fp))
          {
            var info = new UTF8Encoding(true).GetBytes(settingsTemplate.ToString());
            // Add some information to the file.
            fs.Write(info, 0, info.Length);
          }

          Console.WriteLine("Generated Settings File...");
        }

        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
      SettingsTab settingsTab = new(fp);
      return settingsTab;
    }
  }
}
