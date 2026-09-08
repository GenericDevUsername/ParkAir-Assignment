namespace ParkAir___Assignment.Menus;

public interface ITab
{
  public string TabName { get; set; }
  public string Screen { get; set; }
  internal void HandleKeypress(ConsoleKeyInfo key);
}