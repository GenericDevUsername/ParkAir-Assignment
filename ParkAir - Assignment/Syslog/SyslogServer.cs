using System.Net;

namespace ParkAir___Assignment.Syslog;

public class SyslogServer
{

  private readonly string _listeningType;
  internal IPAddress _listeningIP;
  internal int _listeningPort;
  internal List<SysMessage> _log = new();
  private TcpListener? _tcpServerListener;
  private UdpListener? _udpServerListener;

  private readonly Thread t_TcpListenerThread;

  private readonly Thread t_UdpListenerThread;

  public SyslogServer(IPAddress? ip = null, int port = 514, string type = "BOTH")
  {
    if (type != "UDP" && type != "TCP" && type != "BOTH")
    {
      Exception error = new("Type must be UDP, TCP or BOTH");
      throw error;
    }
    ip ??= IPAddress.Parse("127.0.0.1");

    _listeningIP = ip;
    _listeningPort = port;
    _listeningType = type;

    t_TcpListenerThread = new Thread(RunTcp);

    t_UdpListenerThread = new Thread(RunUdp);

  }
  public bool Shutdown { get; private set; } = false;

  public List<SysMessage> GetLogs()
  {
    return new List<SysMessage>(_log);
  }

  public void Start()
  {
    switch (_listeningType)
    {
      case "UDP":
        t_UdpListenerThread.Start();
        break;

      case "TCP":
        t_TcpListenerThread.Start();
        break;

      case "BOTH":
        t_UdpListenerThread.Start();
        t_TcpListenerThread.Start();
        break;
    }
  }

  public void Restart()
  {
    _udpServerListener?.Restart();
    _tcpServerListener?.Stop();
  }

  private void RunTcp()
  {

  }
  private void RunUdp()
  {
    _udpServerListener = new UdpListener(this);
  }
}