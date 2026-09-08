using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog
{
  internal class UdpListener
  {
    private IPEndPoint _RemoteIPEndpoint;
    private readonly SyslogServer _server;
    private UdpClient _udpClientListener;

    public UdpListener(SyslogServer server)
    {
      this._server = server;
      this._udpClientListener = new(server._listeningPort);
      this._RemoteIPEndpoint = new(IPAddress.Any, server._listeningPort);

      this._udpClientListener.BeginReceive(Recv, null);
    }

    public void Restart()
    {
      this._udpClientListener.Dispose();
      this._udpClientListener.Close();

      this._udpClientListener = new(this._server._listeningPort);
      this._RemoteIPEndpoint = new(IPAddress.Any, this._server._listeningPort);

      this._udpClientListener.BeginReceive(Recv, null);
    }

    private void Recv(IAsyncResult res)
    {
      try
      {
        var received = this._udpClientListener.EndReceive(res, ref this._RemoteIPEndpoint);
        var returnData = Encoding.ASCII.GetString(received);


        this._server._log.Add(new(returnData));

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
