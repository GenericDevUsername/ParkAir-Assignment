using System.Text.RegularExpressions;

namespace ParkAir___Assignment.Syslog
{
  public class SysMessage
  {
    public string sysString { get; private set; }

    public readonly int priority;
    public readonly int severity;
    public readonly DateTime timestamp;
    public readonly string? hostname;
    public readonly string? application;
    public readonly string? processId;
    public readonly string? messageId;
    public readonly string? structuredData;
    public readonly string? message;
    
    

    public SysMessage(string message)
    {
      this.sysString = message;
      
      // Using Regex, We separate the string into an array for each section in the 
      const string pattern = @"^\<([0-9]{1,3})\>(\d{0,2}) (\-|[\S]+) (\-|[\S\s]{1,255}) (\-|[\S]{1,48}) (\-|[\S]{1,128}) (\-|[\S]{1,32}) (\-|\[[\S\s]+\]) ([\S\s]+){0,1}";
      Match regexMatch = Regex.Match(message, pattern, RegexOptions.Singleline);
      if (!(regexMatch.Length > 0 & regexMatch.Groups.Count == 10)) return;
      int.TryParse(regexMatch.Groups[1].ToString(), out this.priority);
      int.TryParse(regexMatch.Groups[2].ToString(), out this.severity);
      DateTime.TryParse(regexMatch.Groups[3].ToString(), out this.timestamp);
      this.hostname = regexMatch.Groups[4].ToString();
      this.application = regexMatch.Groups[5].ToString();
      this.processId = regexMatch.Groups[6].ToString();
      this.messageId = regexMatch.Groups[7].ToString();
      this.structuredData = regexMatch.Groups[8].ToString();
      this.message = regexMatch.Groups[9].ToString();
    }
  }
}
