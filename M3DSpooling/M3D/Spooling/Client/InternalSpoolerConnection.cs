// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.InternalSpoolerConnection
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using M3D.Spooling.Sockets;
using System;

namespace M3D.Spooling.Client
{
  public class InternalSpoolerConnection : ISpoolerConnection, IBroadcastServer
  {
    private SpoolerServer spooler_server;

    public EventHandler<EventArgs> OnReceivedSpoolerShutdownMessage
    {
      set
      {
        this.spooler_server.OnReceivedSpoolerShutdownMessage += value;
      }
    }

    public EventHandler<EventArgs> OnReceivedSpoolerShowMessage
    {
      set
      {
        this.spooler_server.OnReceivedSpoolerShowMessage += value;
      }
    }

    public EventHandler<EventArgs> OnReceivedSpoolerHideMessage
    {
      set
      {
        this.spooler_server.OnReceivedSpoolerHideMessage += value;
      }
    }

    public InternalSpoolerConnection()
    {
      this.spooler_server = new SpoolerServer();
      this.spooler_server.SetBroadcastServer((IBroadcastServer) this);
    }

    public bool ConnectToWindow(IntPtr hwnd)
    {
      return this.spooler_server.ConnectToWindow(hwnd);
    }

    public bool StartServer(int port)
    {
      int num = this.spooler_server.StartSocketPeer(port) >= 0 ? 1 : 0;
      if (num != 0)
        return num != 0;
      this.spooler_server.Shutdown();
      return num != 0;
    }

    public override void ShutdownConnection()
    {
      this.spooler_server.CloseConnections();
      this.spooler_server.Shutdown();
    }

    public override SpoolerResult SendSpoolerMessageInternal(string message)
    {
      SpoolerResult spoolerResult = SpoolerResult.Fail_Connect;
      if (this.spooler_server != null)
      {
        string data = this.spooler_server.onClientMessage(message);
        if (!string.IsNullOrEmpty(data))
          this.OnRawMessage(data);
        spoolerResult = SpoolerResult.OK;
      }
      return spoolerResult;
    }

    public override bool UseNoSpoolerMode
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    public void BroadcastMessage(string message)
    {
      this.OnRawMessage("<SocketBroadcast>" + message + "</SocketBroadcast><EOF>");
      this.spooler_server.BroadcastMessage(message);
    }

    public void SendMessageToClient(Guid client_guid, string message)
    {
      if (client_guid == this.spooler_server.MyGuid)
        this.OnRawMessage("<SocketBroadcast>" + message + "</SocketBroadcast><EOF>");
      else
        this.spooler_server.SendMessageToClient(client_guid, message);
    }

    internal SpoolerServer SpoolerServer
    {
      get
      {
        return this.spooler_server;
      }
    }
  }
}
