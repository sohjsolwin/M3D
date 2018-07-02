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
      StartUpDelay = 2000;
      Executable = executable;
      WorkingPath = working_path;
      SpoolerArguments = spooler_arguments;
      available_lock = new object();
      socket_client = new SocketClient
      {
        OnReceivedRawMessage = new CallBackOnReceivedRawMessage(((ISpoolerConnection)this).OnRawMessage)
      };
    }

    ~RemoteSpoolerConnection()
    {
      socket_client.Shutdown();
    }

    public override void ShutdownConnection()
    {
      if (!UseNoSpoolerMode && __started)
      {
        socket_client.SendMessage("<CloseConnection/>", 500);
      }

      socket_client.Shutdown();
    }

    public override SpoolerResult SendSpoolerMessageInternal(string message)
    {
      if (UseNoSpoolerMode)
      {
        return SpoolerResult.OK;
      }

      __started = true;
      var xml_message = (string) null;
      lock (available_lock)
      {
        xml_message = socket_client.SendMessage(message);
      }

      if ((string.IsNullOrEmpty(xml_message) || xml_message.StartsWith("FAIL:")) && !string.IsNullOrEmpty(Executable))
      {
        if (!StartSpooler(message))
        {
          return SpoolerResult.Fail;
        }
      }
      else
      {
        if (xml_message.StartsWith("FAIL"))
        {
          return SpoolerResult.Fail;
        }

        if (xml_message == "ERROR")
        {
          return SpoolerResult.Error;
        }

        if (XMLProcessor != null)
        {
          XMLProcessor(xml_message);
        }
      }
      return SpoolerResult.OK;
    }

    private bool StartSpooler(string message)
    {
      __started = true;
      if (UseNoSpoolerMode)
      {
        return true;
      }

      bool flag;
      try
      {
        var process = new Process();
        process.StartInfo.FileName = Executable;
        process.StartInfo.Arguments = SpoolerArguments;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = false;
        if (!string.IsNullOrEmpty(WorkingPath))
        {
          process.StartInfo.WorkingDirectory = WorkingPath;
        }

        flag = process.Start();
        if (flag && process.HasExited)
        {
          flag = false;
        }

        if (!flag)
        {
          flag = process.Start();
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("SpoolerConnection", ex);
        throw ex;
      }
      if (flag)
      {
        Thread.Sleep(StartUpDelay);
        if (SendSpoolerMessageInternal(message) == SpoolerResult.OK)
        {
          return true;
        }
      }
      return false;
    }

    public int StartUp(int serverport)
    {
      return socket_client.StartUp(serverport);
    }

    public override bool UseNoSpoolerMode
    {
      get
      {
        return __use_no_spooler_mode;
      }
      set
      {
        if (__started)
        {
          throw new Exception("Spooler Already Started");
        }

        __use_no_spooler_mode = value;
      }
    }
  }
}
