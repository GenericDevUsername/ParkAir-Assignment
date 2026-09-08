using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog
{
  internal class UdpListener
  {
    private UdpClient _udpClientListener;
    private IPEndPoint _RemoteIPEndpoint;
    private SyslogServer _server;

    public UdpListener(SyslogServer server)
    {
      this._server = server;
      this._udpClientListener = new UdpClient(server._listeningPort);
      this._RemoteIPEndpoint = new IPEndPoint(IPAddress.Any, server._listeningPort);

      this._udpClientListener.BeginReceive(new AsyncCallback(Recv), null);
    }

    public void Restart()
    {
      this._udpClientListener.Dispose();
      this._udpClientListener.Close();

      this._udpClientListener = new UdpClient(this._server._listeningPort);
      this._RemoteIPEndpoint = new IPEndPoint(IPAddress.Any, this._server._listeningPort);
      
      this._udpClientListener.BeginReceive(new AsyncCallback(Recv), null);
    }
    
    private void Recv(IAsyncResult res)
    {
      try
      {
        byte[] received = this._udpClientListener.EndReceive(res, ref this._RemoteIPEndpoint);
        string returnData = Encoding.ASCII.GetString(received);


        this._server._log.Add(new SysMessage(returnData));

        this._udpClientListener.BeginReceive(new AsyncCallback(Recv), null);
      }
      catch (System.Net.Sockets.SocketException)
      {
        // ignore
      }
      catch (System.ObjectDisposedException)
      {
        //ignore
      }

    }
  }
}