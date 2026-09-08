using System.Text.RegularExpressions;

namespace ParkAir___Assignment.Syslog
{
  public class SysMessage
  {
    public readonly int priority;
    public readonly int version;
    public readonly DateTime timestamp;
    public readonly string? hostname;
    public readonly string? application;
    public readonly string? message;
    public readonly string? messageId;
    public readonly string? processId;
    public readonly int severity;
    public readonly string? structuredData;

    public string sysString { get; private set; }
    public bool logReady = false;


    public SysMessage(string message)
    {
      sysString = message;


      /*/*/
      // Using Regex, We separate the string into an array for each section in the syslog message.
      // We also modify the regex to work with ParkAir's weird proprietary formatting:
      //
      // HOSTNAME - modified to allow a string as a valid input despite it not being valid in the RFC5424 Syslog Protocol Standards
      // PRIORITY - modified to allow for EMPTY as a value despite this being a required variable according to the RFC5424 Syslog Protocol Standards
      //
      // (https://www.rfc-editor.org/rfc/pdfrfc/rfc5424.txt.pdf)
      /*/*/
      const string pattern =
        @"^\<(?<Priority>[0-9]{0,3})\>(?<Version>\d{0,2}) (?<Timestamp>\-|[\S]+) (?<Hostname>\-|[\S\s]{1,255}) (?<AppName>\-|[\S]{1,48}) (?<ProcId>\-|[\S]{1,128}) (?<MsgId>\-|[\S]{1,32}) (?<StructuredData>\-|\[[\S\s]+\]) (?<Message>[\S\s]+){0,1}";
      Match regexMatch = Regex.Match(message, pattern, RegexOptions.Singleline);
      if (!((regexMatch.Length > 0) & (regexMatch.Groups.Count == 10))) return;
      int.TryParse(regexMatch.Groups[1].ToString(), out this.priority);
      int.TryParse(regexMatch.Groups[2].ToString(), out this.version);
      this.severity = this.priority - this.priority / 8 * 8;
      DateTime.TryParse(regexMatch.Groups[3].ToString(), out this.timestamp);
      this.hostname = regexMatch.Groups[4].ToString();
      this.application = regexMatch.Groups[5].ToString();
      this.processId = regexMatch.Groups[6].ToString();
      this.messageId = regexMatch.Groups[7].ToString();
      this.structuredData = regexMatch.Groups[8].ToString();
      this.message = regexMatch.Groups[9].ToString();

      this.logReady = true;
    }


  }
}
