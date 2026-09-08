using System.Net;

namespace ParkAir___Assignment.Syslog
{
  public class SyslogServer
  {
    internal IPAddress _listeningIP;
    internal readonly int _listeningPort;
    internal List<SysMessage> _log = new();

    private readonly string _listeningType;

    private readonly Thread t_UdpListenerThread;
    private readonly Thread t_TcpListenerThread;

    public SyslogServer(IPAddress? ip = null, int port = 514, string type = "BOTH")
    {
      if (type != "UDP" && type != "TCP" && type != "BOTH")
      {
        Exception error = new Exception("Type must be UDP, TCP or BOTH");
        throw error;
      }
      ip ??= IPAddress.Parse("127.0.0.1");

      this._listeningIP = ip;
      this._listeningPort = port;
      this._listeningType = type;
      
      this.t_TcpListenerThread = new(new ThreadStart(RunTcp));
      this.t_UdpListenerThread = new(new ThreadStart(RunUdp));
    }

    public void Start()
    {
      switch (this._listeningType)
      {
        case "UDP":
          this.t_UdpListenerThread.Start();
          break;
        
        case "TCP":
          this.t_TcpListenerThread.Start();
          break;
        
        case "BOTH":
          this.t_UdpListenerThread.Start();
          this.t_TcpListenerThread.Start();
          break;
      }
    }
    private void RunTcp()
    {
      
    }
    private void RunUdp()
    {
      UdpListener udpServerListener = new UdpListener(this);
    }


  }
}
