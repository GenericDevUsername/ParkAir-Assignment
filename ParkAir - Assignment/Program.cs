using System.Text;
using Newtonsoft.Json.Linq;
using ParkAir___Assignment.Menus;

namespace ParkAir___Assignment
{
  internal class Program
  {
    private static void Main()
    {
      Console.OutputEncoding = Encoding.UTF8;

      Console.WriteLine("Initialising...");
      var dir = Directory.GetCurrentDirectory();

      Gui menuHandler = new();
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));
      menuHandler.AddTab(InitSettings($"{dir}/settings.json"));

      Console.


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
              Name = "Listening Type",
              InputType = "SingleSelect",
              OptionGap = 2,
              Options = new List<string> { "TCP", "UDP", "BOTH" },
              Selected = "BOTH",
              Default = "BOTH"
            },
            IpType = new
            {
              Name = "Ip Type",
              InputType = "SingleSelect",
              OptionGap = 1,
              Options = new List<string> { "IPV4", "IPV6", "BOTH" },
              Selected = "BOTH",
              Default = "BOTH"
            },
            ListeningIp = new
            {
              Name = "Listening IP",
              InputType = "IP",
              Selected = "LocalHost",
              Default = "LocalHost"
            },
            ListeningPort = new
            {
              Name = "Listening Port",
              InputType = "Port",
              Selected = "514",
              Default = "514"
            }
          },
          Colors = new
          {
            Emergency = new
            {
              Name = "Emergency",
              InputType = "Color",
              Selected = "DARK_BLUE",
              Default = "DARK_BLUE"
            },
            Alert = new
            {
              Name = "Alert",
              InputType = "Color",
              Selected = "DARK_BLUE",
              Default = "DARK_BLUE"
            },
            Critical = new
            {
              Name = "Critical",
              InputType = "Color",
              Selected = "DARK_BLUE",
              Default = "DARK_BLUE"
            },
            Error = new
            {
              Name = "Color",
              InputType = "Color",
              Selected = "DARK_RED",
              Default = "DARK_RED"
            },
            Warning = new
            {
              Name = "Warning",
              InputType = "Color",
              Selected = "DARK_YELLOW",
              Default = "DARK_YELLOW"
            },
            Notice = new
            {
              Name = "Notice",
              InputType = "Color",
              Selected = "DARK_YELLOW",
              Default = "DARK_YELLOW"
            },
            Informational = new
            {
              Name = "Informational",
              InputType = "Color",
              Selected = "BLACK",
              Default = "BLACK"
            },
            Debug = new
            {
              Name = "Debug",
              InputType = "Color",
              Selected = "DARK_BLUE",
              Default = "DARK_BLUE"
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
