using System.Net;
using System.Runtime.CompilerServices;

namespace ParkAir___Assignment.Syslog
{
  public class SyslogServer
  {
    internal IPAddress _listeningIP;
    internal int _listeningPort;
    internal List<SysMessage> _log = new();
    public bool Shutdown { get; private set; } = false;

    private readonly string _listeningType;

    private Thread t_UdpListenerThread;
    private UdpListener? _udpServerListener;
    
    private Thread t_TcpListenerThread;
    private TcpListener? _tcpServerListener;

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
      
      this.t_TcpListenerThread = new Thread(RunTcp);
      
      this.t_UdpListenerThread = new Thread(RunUdp);
    
  }

    public List<SysMessage> GetLogs()
    {
      return new List<SysMessage>(this._log);
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

    public void Restart()
    {
      this._udpServerListener?.Restart();
      this._tcpServerListener?.Stop();
    }

    private void RunTcp()
    {
      
    }
    private void RunUdp()
    { 
      this._udpServerListener = new(this);
    }


  }
}
