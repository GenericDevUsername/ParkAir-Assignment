using System.Net;

namespace ParkAir___Assignment.Syslog
{
  internal class SyslogServer
  {
    private IPAddress _listeningIP;
    private int _listeningPort;

    private readonly Thread t_UdpListenerThread;
    private readonly Thread t_TcpListenerThread;

    public SyslogServer(IPAddress? ip = null, int port = 514)
    {
      ip ??= IPAddress.Parse("127.0.0.1");

      this._listeningIP = ip;
      this._listeningPort = port;
      
      this.t_TcpListenerThread = new(new ThreadStart(RunTcp));
      this.t_UdpListenerThread = new(new ThreadStart(RunUdp));
    }

    public void Start()
    {

    }
    private void RunTcp()
    {
      
    }
    private void RunUdp()
    {

    }


  }
}
