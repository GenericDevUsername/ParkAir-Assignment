using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ParkAir___Assignment.Syslog
{
  internal class SysTcpListener
  {

    private bool _exit = false;
    private TcpListener _listener;
    private readonly SyslogServer _server;

    public SysTcpListener(SyslogServer server)
    {
      this._server = server;

      string message;
      Socket client;
      string returnData;

      this._listener = new(IPAddress.Any, this._server._listeningPort);
      this._listener.Start();

      HandleListener(this._listener);
    }

    public void Restart()
    {
      this._listener.Stop();
      this._exit = true;

      this._listener = new(IPAddress.Any, this._server._listeningPort);
      this._listener.Start();

      this._exit = false;
      HandleListener(this._listener);
    }

    private void HandleListener(TcpListener listener)
    {
      Socket client;
      while (!this._exit)
      {
        client = this._listener.AcceptSocket();
        ThreadPool.QueueUserWorkItem(ThreadProc, client);
      }
    }

    private void ThreadProc(object obj)
    {
      Socket? client = (Socket)obj;

      while (true)
      {
        try
        {
          //("Reading data...");
          var data = new byte[200];
          var size = client.Receive(data);
          var returnData = Encoding.ASCII.GetString(data);
          this._server.LogAppend(new(returnData.Replace("\n", "")));
        }
        catch (Exception e)
        {
          // ignored
        }
      }
    }
  }
}
