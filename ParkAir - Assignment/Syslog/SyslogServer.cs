using System.Net;
using Microsoft.VisualBasic.FileIO;

namespace ParkAir___Assignment.Syslog
{
  public class SyslogServer
  {

    private readonly string _listeningType;
    internal IPAddress _listeningIP;
    internal int _listeningPort;
    internal SlidingBuffer<SysMessage> _log = new(250);
    private SysTcpListener? _tcpServerListener;
    private SysUdpListener? _udpServerListener;

    private readonly Thread t_TcpListenerThread;

    private readonly Thread t_UdpListenerThread;

    private readonly Thread t_fileLogger;
    internal Queue<SysMessage> _queue = new();
    internal bool _clearLogs = false;

    public void LogAppend(SysMessage message)
    {
      this._queue.Enqueue(message);
      this._log.Add(message);
    }

    private void FileLogger()
    {
      while (true)
      {
        string dir = Directory.GetCurrentDirectory();
        string filePath = $"{dir}/local.store/logs-{DateTime.Now:M.d.yyyy}.csv";
        if (this._clearLogs)
          try
          {
            DirectoryInfo d = new($"{dir}/local.store/");

            FileInfo[] files = d.GetFiles("logs-??.??.*.csv");
            foreach (FileInfo file in files)
            {
              file.Delete();
            }
            this._clearLogs = false;
          }
          catch
          {
            // ignore
          }

        if (!File.Exists(filePath))
        {
          // if the settings file doesn't exist create it with default values
          const string values =
            "priority,version,timestamp,hostname,application,processId,messageId,structuredData,message,severity,messageString\n";
          FileInfo file = new(filePath);
          file.Directory.Create(); // If the directory already exists, this method does nothing.
          File.WriteAllText(file.FullName, values);
        }

        StreamWriter writer = File.AppendText(filePath);
        try
        {
          SysMessage logMessage = this._queue.Dequeue();
          if (!logMessage.logReady)
          {
            this._queue.Enqueue(logMessage);
            continue;
          }

          writer.WriteLine(
            $"{logMessage.priority},{logMessage.version},{logMessage.timestamp.ToUniversalTime().ToString("u").Replace(" ", "T")},\"{logMessage.hostname}\",{logMessage.application},{logMessage.processId},{logMessage.messageId},{logMessage.structuredData},{logMessage.message},{logMessage.severity},\"{logMessage.sysString}\"");

        }
        catch (InvalidOperationException)
        {
          Thread.Sleep((int)1E3);
        }

        writer.Close();
      }
    }

    public SyslogServer(IPAddress? ip = null, int port = 514, string type = "BOTH")
    {
      string dir = Directory.GetCurrentDirectory();
      string filePath = $"{dir}/local.store/logs-{DateTime.Now:M.d.yyyy}.csv";

      if (File.Exists(filePath))
      {
        TextFieldParser preLoad = new(filePath);
        preLoad.TextFieldType = FieldType.Delimited;
        preLoad.SetDelimiters(",");

        int i = 0;
        while (!preLoad.EndOfData)
        {
          //Processing row
          string[] fields = preLoad.ReadFields();
          if (i == 0)
          {
            i++;
            continue;
          }

          this._log.Add(new(fields.Last()));
          i++;
        }

        preLoad.Close();
      }


      this.t_fileLogger = new(FileLogger);
      this.t_fileLogger.Start();

      if (type != "UDP" && type != "TCP" && type != "BOTH")
      {
        Exception error = new("Type must be UDP, TCP or BOTH");
        throw error;
      }

      ip ??= IPAddress.Parse("127.0.0.1");

      this._listeningIP = ip;
      this._listeningPort = port;
      this._listeningType = type;

      this.t_TcpListenerThread = new(RunTcp);

      this.t_UdpListenerThread = new(RunUdp);

    }

    public bool Shutdown { get; private set; } = false;

    public List<SysMessage> GetLogs()
    {
      return new(this._log);
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
      this._tcpServerListener?.Restart();
    }

    private void RunTcp()
    {
      this._tcpServerListener = new(this);
    }

    private void RunUdp()
    {
      this._udpServerListener = new(this);
    }
  }
}
