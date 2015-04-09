using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceControl
{
  public class Program
  {
    private static bool _run;

    private static DateTime _lastCheck;

    public static void Main(string[] args)
    {
      Console.WriteLine("ServiceControl up and running.");

      Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

      var controller = new Controller();
      controller.Load();
      _run = true;
      _lastCheck = DateTime.Now;

      while (_run)
      {
        if (_lastCheck.Hour <= 3 && DateTime.Now.Hour >= 4)
        {
          Console.WriteLine("Autorestart of all services.");
          controller.Terminate();
        }

        controller.Check();
        _lastCheck = DateTime.Now;
        System.Threading.Thread.Sleep(100);
      }

      controller.Terminate();

      Console.WriteLine("ServiceControl terminating.");
    }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
      e.Cancel = true;
      _run = false;
    }
  }
}
