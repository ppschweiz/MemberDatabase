using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ServiceControl
{
  public class Controller
  {
    private List<Service> _services;

    public Controller()
    {
      _services = new List<Service>();
    }

    public void Load()
    {
      var directory = new DirectoryInfo(Environment.CurrentDirectory);

      foreach (var file in directory.GetFiles("*.svc"))
      {
        var service = new Service(file.FullName);
        _services.Add(service);
      }
    }

    public void Check()
    {
      foreach (var service in _services)
      {
        service.Check();
      }
    }

    public void Terminate()
    {
      foreach (var service in _services)
      {
        service.Terminate();
      }
    }
  }
}
