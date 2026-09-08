using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkAir___Assignment.Syslog
{
  public class SysMessage
  {
    public string sysString { get; private set; }

    public SysMessage(string message)
    {
      this.sysString = message;
    }
  }
}
