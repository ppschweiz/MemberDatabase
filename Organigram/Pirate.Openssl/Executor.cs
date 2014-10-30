using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Pirate.Openssl
{
  public class Executor
  {
    public string WorkingPath { get; set; }

    public string CaCertifcatePath { get; set; }

    public string OpensslBinaryPath { get; set; }

    public string ConfigPath { get; set; }

    public Executor()
    {
      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        OpensslBinaryPath = "openssl";
        WorkingPath = "/root/mdbca";
        CaCertifcatePath = "/root/mdbca/ca/ca.pem";
        ConfigPath = "openssl.cfg";
      }
      else
      {
        OpensslBinaryPath = @"C:\Program Files (x86)\OpenSSL-Win32\bin\openssl.exe";
        WorkingPath = @"D:\Downloads\SSL\mdbca";
        CaCertifcatePath = @"D:\Security\PPS\mdb-ca.pem";
        ConfigPath = "openssl.cfg";
      }
    }

    public X509Certificate2 CaCertficate
    {
      get
      {
        return new X509Certificate2(CaCertifcatePath);
      }
    }

    public byte[] ConvertPemToDer(string certificateDataPem)
    {
      var parts = certificateDataPem.Split(new[] { "-----BEGIN CERTIFICATE-----", "-----END CERTIFICATE-----" }, StringSplitOptions.None);

      if (parts.Length == 3)
      {
        return Convert.FromBase64String(parts[1].Replace("\r", string.Empty).Replace("\n", string.Empty));
      }
      else
      {
        throw new InvalidOperationException();
      }
    }

    public byte[] ConvertPemToDerOld(string certificateDataPem)
    {
      string filename = DateTime.Now.Ticks + ".der";
      string filepath = Path.Combine(WorkingPath, filename);

      var arguments = new List<string>();
      arguments.Add("x509");
      arguments.Add("-outform DER");
      arguments.Add("-out " + filepath);

      Execute(arguments, certificateDataPem);

      if (File.Exists(filepath))
      {
        var data = File.ReadAllBytes(filepath);
        File.Delete(filepath);
        return data;
      }
      else
      {
        throw new InvalidOperationException();
      }
    }

    public string SignSpkac(Spkac value)
    {
      string filename = DateTime.Now.Ticks + ".spkac";
      string filepath = Path.Combine(WorkingPath, filename);

      File.WriteAllText(filepath, value.Text, Encoding.ASCII);

      try
      {
        var arguments = new List<string>();
        arguments.Add("ca");
        arguments.Add("-config " + ConfigPath);
        arguments.Add("-batch");
        arguments.Add("-notext");
        arguments.Add("-spkac " + filename);

        return Execute(arguments);
      }
      finally
      {
        File.Delete(filepath);
      }
    }

    private string Execute(IList<string> arguments, string input = null)
    {
      return Execute(string.Join(" ", arguments.ToArray()), input);
    }

    private string Execute(string arguments, string input = null)
    {
      var start = new ProcessStartInfo(OpensslBinaryPath, arguments);
      start.UseShellExecute = false;
      start.WorkingDirectory = WorkingPath;
      start.RedirectStandardOutput = true;
      start.RedirectStandardError = true;
      start.RedirectStandardInput = input != null;

      var process = Process.Start(start);
      string error = string.Empty;
      string output = string.Empty;

      var errorReader = new Thread(() => error = process.StandardError.ReadToEnd());
      errorReader.Start();

      var outputReader = new Thread(() => output = process.StandardOutput.ReadToEnd());
      outputReader.Start();

      if (input != null)
      {
        process.StandardInput.WriteLine(input);
        process.StandardInput.Flush();
        process.StandardInput.Close();
      }

      process.WaitForExit();

      errorReader.Join();
      outputReader.Join();

      return output;
    }
  }
}
