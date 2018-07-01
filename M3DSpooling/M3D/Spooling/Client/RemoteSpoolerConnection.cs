// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.RemoteSpoolerConnection
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using M3D.Spooling.Sockets;
using System;
using System.Diagnostics;
using System.Threading;

namespace M3D.Spooling.Client
{
  public class RemoteSpoolerConnection : ISpoolerConnection
  {
    private SocketClient socket_client;
    public int StartUpDelay;
    private string Executable;
    private string WorkingPath;
    private string SpoolerArguments;
    private bool __use_no_spooler_mode;
    private bool __started;
    private object available_lock;

    public RemoteSpoolerConnection(string executable, string working_path)
      : this(executable, working_path, "")
    {
    }

    public RemoteSpoolerConnection(string executable, string working_path, string spooler_arguments)
    {
      this.StartUpDelay = 2000;
      this.Executable = executable;
      this.WorkingPath = working_path;
      this.SpoolerArguments = spooler_arguments;
      this.available_lock = new object();
      this.socket_client = new SocketClient();
      this.socket_client.OnReceivedRawMessage = new CallBackOnReceivedRawMessage(((ISpoolerConnection) this).OnRawMessage);
    }

    ~RemoteSpoolerConnection()
    {
      this.socket_client.Shutdown();
    }

    public override void ShutdownConnection()
    {
      if (!this.UseNoSpoolerMode && this.__started)
        this.socket_client.SendMessage("<CloseConnection/>", 500);
      this.socket_client.Shutdown();
    }

    public override SpoolerResult SendSpoolerMessageInternal(string message)
    {
      if (this.UseNoSpoolerMode)
        return SpoolerResult.OK;
      this.__started = true;
      string xml_message = (string) null;
      lock (this.available_lock)
        xml_message = this.socket_client.SendMessage(message);
      if ((string.IsNullOrEmpty(xml_message) || xml_message.StartsWith("FAIL:")) && !string.IsNullOrEmpty(this.Executable))
      {
        if (!this.StartSpooler(message))
          return SpoolerResult.Fail;
      }
      else
      {
        if (xml_message.StartsWith("FAIL"))
          return SpoolerResult.Fail;
        if (xml_message == "ERROR")
          return SpoolerResult.Error;
        if (this.XMLProcessor != null)
          this.XMLProcessor(xml_message);
      }
      return SpoolerResult.OK;
    }

    private bool StartSpooler(string message)
    {
      this.__started = true;
      if (this.UseNoSpoolerMode)
        return true;
      bool flag;
      try
      {
        Process process = new Process();
        process.StartInfo.FileName = this.Executable;
        process.StartInfo.Arguments = this.SpoolerArguments;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = false;
        if (!string.IsNullOrEmpty(this.WorkingPath))
          process.StartInfo.WorkingDirectory = this.WorkingPath;
        flag = process.Start();
        if (flag && process.HasExited)
          flag = false;
        if (!flag)
          flag = process.Start();
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("SpoolerConnection", ex);
        throw ex;
      }
      if (flag)
      {
        Thread.Sleep(this.StartUpDelay);
        if (this.SendSpoolerMessageInternal(message) == SpoolerResult.OK)
          return true;
      }
      return false;
    }

    public int StartUp(int serverport)
    {
      return this.socket_client.StartUp(serverport);
    }

    public override bool UseNoSpoolerMode
    {
      get
      {
        return this.__use_no_spooler_mode;
      }
      set
      {
        if (this.__started)
          throw new Exception("Spooler Already Started");
        this.__use_no_spooler_mode = value;
      }
    }
  }
}
