using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog
{
  internal class SysUdpListener
  {
    private IPEndPoint _remoteIpEndpoint;
    private readonly SyslogServer _server;
    private UdpClient _udpClientListener;

    public SysUdpListener(SyslogServer server)
    {
      this._server = server;
      this._udpClientListener = new(server._listeningPort);
      this._remoteIpEndpoint = new(IPAddress.Any, server._listeningPort);

      this._udpClientListener.BeginReceive(Recv, null);
    }

    public void Restart()
    {
      this._udpClientListener.Dispose();
      this._udpClientListener.Close();

      this._udpClientListener = new(this._server._listeningPort);
      this._remoteIpEndpoint = new(IPAddress.Any, this._server._listeningPort);

      this._udpClientListener.BeginReceive(Recv, null);
    }

    private void Recv(IAsyncResult res)
    {
      try
      {
        byte[] received = this._udpClientListener.EndReceive(res, ref this._remoteIpEndpoint);
        string returnData = Encoding.ASCII.GetString(received);


        this._server.LogAppend(new(returnData));

        this._udpClientListener.BeginReceive(Recv, null);
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
}
