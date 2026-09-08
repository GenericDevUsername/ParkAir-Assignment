using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog;

internal class SysUdpListener
{
  private IPEndPoint _remoteIpEndpoint;
  private readonly SyslogServer _server;
  private UdpClient _udpClientListener;

  public SysUdpListener(SyslogServer server)
  {
    _server = server;
    _udpClientListener = new UdpClient(server._listeningPort);
    _remoteIpEndpoint = new IPEndPoint(IPAddress.Any, server._listeningPort);

    _udpClientListener.BeginReceive(Recv, null);
  }

  public void Restart()
  {
    _udpClientListener.Dispose();
    _udpClientListener.Close();

    _udpClientListener = new UdpClient(_server._listeningPort);
    _remoteIpEndpoint = new IPEndPoint(IPAddress.Any, _server._listeningPort);

    _udpClientListener.BeginReceive(Recv, null);
  }

  private void Recv(IAsyncResult res)
  {
    try
    {
      byte[] received = _udpClientListener.EndReceive(res, ref _remoteIpEndpoint);
      string returnData = Encoding.ASCII.GetString(received);


      _server.LogAppend(new SysMessage(returnData));

      _udpClientListener.BeginReceive(Recv, null);
    }
    catch (SocketException)
    {
      // ignore
    }
    catch (ObjectDisposedException)
    {
      //ignore
    }

  }
}