using Newtonsoft.Json.Linq;
using ParkAir___Assignment.Menus;
using System.Text;

namespace ParkAir___Assignment;

internal static class Program
{
  private static void Main()
  {
    Console.OutputEncoding = Encoding.UTF8;
    AnsiConsole.Initialize();

    Console.WriteLine("Initialising...");
    string dir = Directory.GetCurrentDirectory();

    Gui menuHandler = new("Park Air Syslog");
    SettingsTab settingsConfig = InitSettings($"{dir}/settings.json");
    menuHandler.AddTab(settingsConfig);
    menuHandler.AddTab(new SyslogTab(settingsConfig));

    menuHandler.Start();
    Console.Title = "Menu not blocking";
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
            Default = "BOTH",
            RequiresRestart = true
          },
          IpType = new
          {
            Name = "Ip Type",
            InputType = "SingleSelect",
            OptionGap = 1,
            Options = new List<string> { "IPV4", "IPV6", "BOTH" },
            Selected = "BOTH",
            Default = "BOTH",
            RequiresRestart = true
          },
          ListeningIp = new
          {
            Name = "Listening IP",
            InputType = "IP",
            Selected = "127.0.0.1",
            Default = "127.0.0.1",
            RequiresRestart = true
          },
          ListeningPort = new
          {
            Name = "Listening Port",
            InputType = "Port",
            Selected = "514",
            Default = "514",
            RequiresRestart = true
          }
        },
        Colors = new
        {
          Emergency = new
          {
            Name = "Emergency",
            InputType = "Color",
            Selected = "DARK_BLUE",
            Default = "DARK_BLUE",
            RequiresRestart = false
          },
          Alert = new
          {
            Name = "Alert",
            InputType = "Color",
            Selected = "DARK_BLUE",
            Default = "DARK_BLUE",
            RequiresRestart = false
          },
          Critical = new
          {
            Name = "Critical",
            InputType = "Color",
            Selected = "DARK_BLUE",
            Default = "DARK_BLUE",
            RequiresRestart = false
          },
          Error = new
          {
            Name = "Color",
            InputType = "Color",
            Selected = "DARK_RED",
            Default = "DARK_RED",
            RequiresRestart = false
          },
          Warning = new
          {
            Name = "Warning",
            InputType = "Color",
            Selected = "DARK_YELLOW",
            Default = "DARK_YELLOW",
            RequiresRestart = false
          },
          Notice = new
          {
            Name = "Notice",
            InputType = "Color",
            Selected = "DARK_YELLOW",
            Default = "DARK_YELLOW",
            RequiresRestart = false
          },
          Informational = new
          {
            Name = "Informational",
            InputType = "Color",
            Selected = "BLACK",
            Default = "BLACK",
            RequiresRestart = false
          },
          Debug = new
          {
            Name = "Debug",
            InputType = "Color",
            Selected = "DARK_BLUE",
            Default = "DARK_BLUE",
            RequiresRestart = false
          }
        }
      });

      //Console.WriteLine(settingsTemplate.ToString());

      try
      {
        // Create the file, or overwrite if the file exists.
        using (FileStream fs = File.Create(fp))
        {
          byte[] info = new UTF8Encoding(true).GetBytes(settingsTemplate.ToString());
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