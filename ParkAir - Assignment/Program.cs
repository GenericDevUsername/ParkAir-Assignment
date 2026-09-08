using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParkAir___Assignment.Menus;
using System.Text;
using ParkAir___Assignment.Menus.ExportHandlers;

namespace ParkAir___Assignment;

internal static class Program
{
  /// <summary>
  /// The main function, runs on start and sets up the program to run
  /// </summary>
  private static void Main()
  {
    
    Console.WriteLine("Initialising...");

    ///// INITIALIZE CONSOLE FOR UTF-8 & ANSI SUPPORT /////
    Console.OutputEncoding = Encoding.UTF8;
    External.AnsiConsole.Initialize();

    // get current working directory
    string dir = Directory.GetCurrentDirectory();

    ///// SETUP UI CLASS /////
    Gui menuHandler = new("Park Air Syslog");
    SettingsTab settingsConfig = InitSettings($"{dir}/settings.json");
    // add menus (tabs)
    menuHandler.AddTab(new SyslogTab(settingsConfig));
    menuHandler.AddTab(new ExportTab());
    menuHandler.AddTab(settingsConfig);

    // start the menu background thread
    menuHandler.Start();
    Console.Clear();
  }

  /// <summary>
  /// Generates a settings tab based on a provided file path and returns a settingsTab Object.
  /// </summary>
  /// <param name="fp">The file path to settings.json.</param>
  /// <returns>settingsTab - a settingsTab Object.</returns>
  private static SettingsTab InitSettings(string fp)
  {
    // SETUP SETTINGS MENU REQUIREMENTS.

    // if the settings file doesn't exist create it with default values.
    if (!File.Exists(fp)) 
    {
      Console.WriteLine("Generating Settings File...");
      DefaultSettingsFile(fp);
    }
    
    //check settings file is valid
    StreamReader file = File.OpenText(fp);
    try
    {
      JsonTextReader reader = new(file);
      JObject test = (JObject)JToken.ReadFrom(reader);
      file.Close();
    }
    catch
    {
      Console.WriteLine("Settings file is corrupt, using defaults and backing up settings");
      if (File.Exists($"{fp}-{DateTime.Now:M.d.yyyy}.backup")) File.Delete($"{fp}-{DateTime.Now:M.d.yyyy}.backup");
      File.Copy(fp, $"{fp}-{DateTime.Now:M.d.yyyy}.backup");
      file.Close();
      DefaultSettingsFile(fp);
    }
    

    // create a settings tab with the settings file and return it
    SettingsTab settingsTab = new(fp);
    return settingsTab;
  }
  private static void DefaultSettingsFile(string fp) {
    // settings json
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

    try
    {
      // Create the file, or overwrite if the file exists.
      using (FileStream fs = File.Create(fp))
      {
        // Convert the file to byes and save to the settings file
        byte[] info = new UTF8Encoding(true).GetBytes(settingsTemplate.ToString());
        fs.Write(info, 0, info.Length);
      }

      Console.WriteLine("Generated Settings File...");
    }

    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
    }
      
  }
}

