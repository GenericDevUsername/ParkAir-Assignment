namespace ParkAir___Assignment.Menus.SettingsHandlers;

public class Color
{
    private readonly Dictionary<string, string> _colors = new()
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

    private readonly List<string> _order = new()
    {
        "BLACK", "DARK_RED", "DARK_GREEN", "DARK_YELLOW", "DARK_BLUE", "DARK_MAGENTA", "DARK_CYAN", "DARK_WHITE",
        "BRIGHT_BLACK", "BRIGHT_RED", "BRIGHT_GREEN", "BRIGHT_YELLOW", "BRIGHT_BLUE", "BRIGHT_MAGENTA", "BRIGHT_CYAN",
        "WHITE"
    };

    private static int Mod(int k, int n)
    {
        return (k %= n) < 0 ? k + n : k;
    }

    public string Next(string current)
    {
        var response = "Default";
        var currentIndex = _order.IndexOf(current);
        if (currentIndex > -1) response = _order[Mod(currentIndex + 1, _order.Count)];
        return response;
    }

    public string Previous(string current)
    {
        var response = "Default";
        var currentIndex = _order.IndexOf(current);
        if (currentIndex > -1) response = _order[Mod(currentIndex - 1, _order.Count)];
        return response;
    }

    public string? FromSetting(string value)
    {
        string? response = null;
        if (_colors.TryGetValue(value, out var color)) response = color;
        return response;
    }
}