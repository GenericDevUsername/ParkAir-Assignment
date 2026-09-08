using System.Diagnostics;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using ParkAir___Assignment.Syslog;

namespace ParkAir___Assignment.Menus.ExportHandlers
{
  public static class SysExport
  {
    private static IEnumerable<SysMessage> LoadLogs()
    {
      string dir = Directory.GetCurrentDirectory();
      DirectoryInfo d = new($"{dir}/local.store/");

      FileInfo[] files = d.GetFiles("logs-??.??.*.csv");
      Console.WriteLine($"Reading {files.Length} local log files...");
      foreach (FileInfo file in files)
      {
        TextFieldParser preLoad = new(file.FullName);
        preLoad.TextFieldType = FieldType.Delimited;
        preLoad.SetDelimiters(",");

        int i = 0;
        List<string> logs = new();
        while (!preLoad.EndOfData)
        {
          //Processing row
          string[] fields = preLoad.ReadFields();
          if (i == 0)
          {
            i++;
            continue;
          }

          logs.Add(fields.Last());
          i++;
        }

        preLoad.Close();
        foreach (string log in logs) yield return new(log);
      }
    }


    public static void ExportSingle(string filetype = "TXT")
    {
      Console.WriteLine($"Exporting with: {filetype}");

      string dir = Directory.GetCurrentDirectory();
      string filename = $"export_{DateTime.Now:yyyMMddHHmmsss}";
      string tempDirectory = $"{dir}/export.tmp/{DateTime.Now:yyyMMddHHmmsss}";
      string tempFile = $"{tempDirectory}/{filename}.{filetype.ToLower()}";
      FileInfo file = new(tempFile);
      file.Directory.Create();

      StreamWriter writer = file.AppendText();
      switch (filetype)
      {
        case "TXT":
          foreach (SysMessage log in LoadLogs().OrderBy(s => s.timestamp)) writer.WriteLine(log.sysString);
          break;

        case "CSV":
          writer.WriteLine(
            "priority,version,timestamp,hostname,application,processId,messageId,structuredData,message,severity,messageString");
          foreach (SysMessage logMessage in LoadLogs().OrderBy(s => s.timestamp))
            writer.WriteLine(
              $"{logMessage.priority},{logMessage.version},{logMessage.timestamp.ToUniversalTime().ToString("u").Replace(" ", "T")},\"{logMessage.hostname}\",{logMessage.application},{logMessage.processId},{logMessage.messageId},{logMessage.structuredData},{logMessage.message},{logMessage.severity},\"{logMessage.sysString}\"");
          break;
      }

      writer.Close();


      string moveDirectoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/parkair_exports/";
      FileInfo moveDirectory = new(moveDirectoryPath);
      moveDirectory.Directory.Create();
      Console.WriteLine($"Saving to: {$"{moveDirectoryPath}{filename}.{filetype.ToLower()}"}");

      int i = 0;
      while (true)
      {
        try
        {
          File.Move($"{tempFile}", $"{moveDirectoryPath}{filename}{(i > 0 ? $" {i}" : "")}.{filetype.ToLower()}");
          break;
        }
        catch
        {
          i++;
        }

      }

      Directory.Delete(tempDirectory);
      Directory.Delete($"{dir}/export.tmp/");
      Process.Start("explorer.exe",
        @$"{moveDirectoryPath}{Path.DirectorySeparatorChar}{filename}{(i > 0 ? $" {i}" : "")}.{filetype.ToLower()}");

    }

    public static void ExportMulti(string filetype = "CSV")
    {
      Console.WriteLine($"Exporting with: {filetype}");

      string dir = Directory.GetCurrentDirectory();
      string tempDirectory = $"{dir}/export.tmp/{DateTime.Now:yyyMMddHHmmsss}/";
      FileInfo file = new(tempDirectory);
      file.Directory.Create();

      Dictionary<string, List<SysMessage>> sortedDictionary = new();
      foreach (SysMessage logMessage in LoadLogs().OrderBy(s => s.timestamp))
      {
        string dictionaryKey = logMessage.hostname.ToLower().Replace(" ", "_");
        if (!sortedDictionary.ContainsKey(dictionaryKey)) sortedDictionary.Add(dictionaryKey, new());
        sortedDictionary[dictionaryKey].Add(logMessage);
      }

      string timestamp = $"{DateTime.Now:yyyMMddHHmmsss}";
      foreach (KeyValuePair<string, List<SysMessage>> sortedItem in sortedDictionary)
      {
        FileInfo tempfile = new($"{tempDirectory}{sortedItem.Key}_{timestamp}.{filetype.ToLower()}");
        StreamWriter writer = tempfile.AppendText();
        switch (filetype)
        {
          case "TXT":
            foreach (SysMessage log in sortedItem.Value.OrderBy(s => s.timestamp)) writer.WriteLine(log.sysString);
            break;

          case "CSV":
            writer.WriteLine(
              "priority,version,timestamp,hostname,application,processId,messageId,structuredData,message,severity,messageString");
            foreach (SysMessage logMessage in sortedItem.Value.OrderBy(s => s.timestamp))
              writer.WriteLine(
                $"{logMessage.priority},{logMessage.version},{logMessage.timestamp.ToUniversalTime().ToString("u").Replace(" ", "T")},\"{logMessage.hostname}\",{logMessage.application},{logMessage.processId},{logMessage.messageId},{logMessage.structuredData},{logMessage.message},{logMessage.severity},\"{logMessage.sysString}\"");
            break;
        }

        writer.Close();
      }

      string moveDirectoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/parkair_exports/";
      FileInfo moveDirectory = new(moveDirectoryPath);
      moveDirectory.Directory.Create();
      Console.WriteLine($"Saving to: {moveDirectoryPath}export_{timestamp}.zip");
      ZipFile.CreateFromDirectory(tempDirectory, $"{moveDirectoryPath}export_{timestamp}.zip", CompressionLevel.Fastest,
        false);

      DirectoryInfo d = new(tempDirectory);
      FileInfo[] files = d.GetFiles($"*");
      foreach (FileInfo tempFile in files) tempFile.Delete();
      Directory.Delete(tempDirectory);
      Directory.Delete($"{dir}/export.tmp/");
      Process.Start("explorer.exe",
        @$"{moveDirectoryPath}{Path.DirectorySeparatorChar}{moveDirectoryPath}export_{timestamp}.zip");

    }
  }
}
