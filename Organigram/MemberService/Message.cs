using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Pirate.Util;

namespace MemberService
{
  public class Message
  {
    public string TextId { get; private set; }

    public string ContinueTo { get; private set; }

    public Message(string textId, string continueTo)
    {
      TextId = textId;
      ContinueTo = continueTo;
    }

    private Message()
    { 
    }
  }
}