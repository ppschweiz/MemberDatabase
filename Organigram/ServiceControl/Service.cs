using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ServiceControl
{
  public class Service
  {
    private Process _process;

    public string Name { get; private set; }

    public string Executable { get; private set; }

    public string Arguments { get; private set; }

    public Service(string filename)
    {
      var lines = File.ReadAllLines(filename);
      Name = lines[0];
      Executable = lines[1];
      Arguments = lines[2];

      Console.WriteLine(Name + ": Service loaded.");
      Console.WriteLine(Name + ": Executable " + Executable);
      Console.WriteLine(Name + ": Arguments " + Arguments);

      _process = Process
        .GetProcessesByName(Executable)
        .Where(p => p.StartInfo.Arguments == Arguments)
        .SingleOrDefault();

      if (_process == null ||
          _process.HasExited)
      {
        Console.WriteLine(Name + ": Process not started yet.");
      }
      else
      {
        Console.WriteLine(Name + ": Process already started.");
      }
    }

    public void Check()
    {
      if (_process == null ||
          _process.HasExited)
      {
        Console.Write(Name + ": Process not running. Restarting... ");
        var startInfo = new ProcessStartInfo(Executable, Arguments);
        _process = Process.Start(startInfo);
        Console.WriteLine("Done.");
      }
    }

    public void Terminate()
    {
      if (_process != null &&
          !_process.HasExited)
      {
        Console.Write(Name + ": Terminating... ");
        _process.Kill();
        _process.WaitForExit();
        Console.WriteLine("Done.");
      }
    }
  }
}
