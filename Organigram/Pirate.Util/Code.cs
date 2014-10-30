using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Util
{
  public static class Code
  {
    public static string To(long key)
    {
      var code = string.Empty;

      for (int s = 0; s < 40; s += 5)
      {
        var c = (key >> s) & 0x1F;
        code += ToChar(c);
      }

      return code;
    }

    public static long From(string code)
    {
      long key = 0;

      for (var i = 0; i < code.Length; i++)
      {
        var c = ToInt(code.Substring(i, 1));
        key |= c << (5 * i);
      }

      return key;
    }

    private static long ToInt(string value)
    {
      switch (value)
      {
        case "0":
          return 0;
        case "1":
          return 1;
        case "2":
          return 2;
        case "3":
          return 3;
        case "4":
          return 4;
        case "5":
          return 5;
        case "6":
          return 6;
        case "7":
          return 7;
        case "8":
          return 8;
        case "9":
          return 9;
        case "A":
          return 10;
        case "B":
          return 11;
        case "C":
          return 12;
        case "D":
          return 13;
        case "E":
          return 14;
        case "F":
          return 15;
        case "G":
          return 16;
        case "H":
          return 17;
        case "I":
          return 18;
        case "K":
          return 19;
        case "L":
          return 20;
        case "M":
          return 21;
        case "N":
          return 22;
        case "P":
          return 23;
        case "Q":
          return 24;
        case "R":
          return 25;
        case "S":
          return 26;
        case "T":
          return 27;
        case "U":
          return 28;
        case "W":
          return 29;
        case "X":
          return 30;
        case "Z":
          return 31;
        default:
          return 0;
      }
    }

    private static string ToChar(long value)
    {
      switch (value)
      {
        case 0:
          return "0";
        case 1:
          return "1";
        case 2:
          return "2";
        case 3:
          return "3";
        case 4:
          return "4";
        case 5:
          return "5";
        case 6:
          return "6";
        case 7:
          return "7";
        case 8:
          return "8";
        case 9:
          return "9";
        case 10:
          return "A";
        case 11:
          return "B";
        case 12:
          return "C";
        case 13:
          return "D";
        case 14:
          return "E";
        case 15:
          return "F";
        case 16:
          return "G";
        case 17:
          return "H";
        case 18:
          return "I";
        case 19:
          return "K";
        case 20:
          return "L";
        case 21:
          return "M";
        case 22:
          return "N";
        case 23:
          return "P";
        case 24:
          return "Q";
        case 25:
          return "R";
        case 26:
          return "S";
        case 27:
          return "T";
        case 28:
          return "U";
        case 29:
          return "W";
        case 30:
          return "X";
        case 31:
          return "Z";
        default:
          return string.Empty;
      }
    }
  }
}
