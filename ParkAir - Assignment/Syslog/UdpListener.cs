using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog
{
  internal class UdpListener
  {
    private UdpClient _udpClientListener;
    private IPEndPoint _RemoteIPEndpoint;

    public UdpListener(SyslogServer server)
    {
      this._udpClientListener = new(server._listeningPort);
      this._RemoteIPEndpoint = new IPEndPoint(IPAddress.Any, server._listeningPort);
      
      while (true)
      {

        byte[] receiveBytes = this._udpClientListener.Receive(ref this._RemoteIPEndpoint);
        string returnData = Encoding.ASCII.GetString(receiveBytes);


        server._log.Add(new(returnData));
        Thread.Sleep(1);
      }
    }
  }
}