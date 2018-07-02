// Decompiled with JetBrains decompiler
// Type: M3D.GUI.FileAssociationSingleInstance
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Spooling.Sockets;
using System;
using System.Net.Sockets;
using System.Text;

namespace M3D.GUI
{
  public class FileAssociationSingleInstance
  {
    private const int Single_Instance_Server_Port = 43350;
    public static NewInstanceEvent OnNewInstance;
    private static FileAssociationSingleInstance.SingleInstanceSocket _myServer;

    public static bool RegisterAsSingleInstance()
    {
      if (FileAssociationSingleInstance._myServer == null)
      {
        FileAssociationSingleInstance._myServer = new FileAssociationSingleInstance.SingleInstanceSocket(new NewInstanceEvent(FileAssociationSingleInstance.NewInstanceEventCallback));
        if (FileAssociationSingleInstance._myServer.StartSocketPeer(43350) < 0)
        {
          return false;
        }

        FileAssociationSingleInstance._myServer.StartListening();
      }
      return true;
    }

    public static void UnRegisterAsSingleInstance()
    {
      if (FileAssociationSingleInstance._myServer == null)
      {
        return;
      }

      FileAssociationSingleInstance._myServer.Shutdown();
      FileAssociationSingleInstance._myServer = (FileAssociationSingleInstance.SingleInstanceSocket) null;
    }

    public static bool SendParametersToSingleInstance(string[] args)
    {
      var flag = false;
      if (args.Length != 0)
      {
        var socketClient = new SocketClient();
        if (socketClient.StartUp(43350) > 0)
        {
          var message = args[0];
          for (var index = 1; index < args.Length; ++index)
          {
            message = message + "\t" + args[index];
          }

          if (socketClient.SendMessage(message) == "OK")
          {
            flag = true;
          }
        }
        socketClient.Shutdown();
      }
      return flag;
    }

    private static void NewInstanceEventCallback(string[] arg)
    {
      if (arg == null || arg.Length != 1 || FileAssociationSingleInstance.OnNewInstance == null)
      {
        return;
      }

      FileAssociationSingleInstance.OnNewInstance(arg);
    }

    private class SingleInstanceSocket : SocketServer
    {
      private NewInstanceEvent callback;

      public SingleInstanceSocket(NewInstanceEvent callback)
      {
        this.callback = callback;
      }

      public override void OnRawMessage(string data, Socket handler)
      {
        base.OnRawMessage(data, handler);
        var message = new FileAssociationSingleInstance.SingleInstanceSocket.ParsedMessage(data).message;
        var str = "OK";
        if (!string.IsNullOrEmpty(message))
        {
          var length = 1;
          for (var startIndex = 0; message.IndexOf('\t', startIndex) > 0; startIndex = message.IndexOf('\t', startIndex) + 1)
          {
            ++length;
          }

          string[] strArray = new string[length];
          if (length == 1)
          {
            strArray[0] = message;
          }
          else
          {
            var startIndex = 0;
            for (var index = 0; index < length; ++index)
            {
              var num = message.IndexOf('\t', startIndex);
              if (num < 0)
              {
                num = message.Length - 1;
              }

              strArray[index] = message.Substring(startIndex, num - startIndex);
              startIndex = num + 1;
            }
          }
          try
          {
            if (callback != null)
            {
              callback(strArray);
            }
          }
          catch (Exception ex)
          {
            str = "FAIL";
          }
        }
        try
        {
          byte[] bytes = Encoding.UTF8.GetBytes(str + "<EOF>");
          handler.Send(bytes);
        }
        catch (Exception ex)
        {
          str = "FAIL";
        }
        if (!(str == "FAIL"))
        {
          return;
        }

        FileAssociationSingleInstance.UnRegisterAsSingleInstance();
      }

      private struct ParsedMessage
      {
        public Guid guid;
        public int port;
        public string message;

        public ParsedMessage(string message)
        {
          var length = message.IndexOf("::");
          guid = new Guid(message.Substring(0, length));
          var num = message.IndexOf("::", length + 2);
          port = int.Parse(message.Substring(length + 2, num - (length + 2)));
          this.message = message.Substring(num + 2, message.IndexOf("<EOF>") - (num + 2));
        }
      }
    }
  }
}
