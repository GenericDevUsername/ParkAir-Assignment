namespace ParkAir___Assignment.Menus;

public class SyslogTab : ITab
{
    public string TabName { get; set; } = "Syslog";
    public string ScreenString => "";
    public bool Tabber => true;
    public Gui? _gui { get; private set; }
    public void HandleKeypress(ConsoleKeyInfo key)
    {
        
    }

    public string Screen()
    {
        return "";
    }

    public void Register(Gui gui)
    {
        this._gui = gui;
    }
}