namespace ParkAir___Assignment.Menus.SettingsHandlers
{
  public class Color
  {
    private Dictionary<string, string> _colors = new()
    {
      ["BLACK"] = "\x1b[30m",
      ["DARK_RED"] = "\x1b[31m",
      ["DARK_GREEN"] = "\x1b[32m",
      ["DARK_YELLOW"] = "\x1b[33m",
      ["DARK_BLUE"] = "\x1b[34m",
      ["DARK_MAGENTA"] = "\x1b[35m",
      ["DARK_CYAN"] = "\x1b[36m",
      ["DARK_WHITE"] = "\x1b[37m",
      ["BRIGHT_BLACK"] = "\x1b[90m",
      ["BRIGHT_RED"] = "\x1b[91m",
      ["BRIGHT_GREEN"] = "\x1b[92m",
      ["BRIGHT_YELLOW"] = "\x1b[93m",
      ["BRIGHT_BLUE"] = "\x1b[94m",
      ["BRIGHT_MAGENTA"] = "\x1b[95m",
      ["BRIGHT_CYAN"] = "\x1b[96m",
      ["WHITE"] = "\x1b[97m",
      ["DEFAULT"] = "\x1b[0m"
    };


    public string? FromSetting(string value)
    {
      string? response = null;
      if (this._colors.ContainsKey(value)) response = this._colors[value];

      return response;
    }
  }
}
