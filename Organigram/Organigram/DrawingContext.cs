using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Organigram
{
  public class DrawingContext
  {
    public StringBuilder Css { get; private set; }

    public StringBuilder Body { get; private set; }

    private int _idCounter;

    public string NewId
    {
      get
      {
        _idCounter++;
        return "id" + _idCounter.ToString();
      }
    }
    
    public DrawingContext()
    {
      Css = new StringBuilder();
      Body = new StringBuilder();
    }
  }
}
