// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Sockets.SocketServer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace M3D.Spooling.Sockets
{
  public class SocketServer : SocketPeer, IBroadcastServer
  {
    private Dictionary<Guid, SocketServer.IPAddressInfo> client_addresses;

    public SocketServer()
    {
      this.client_addresses = new Dictionary<Guid, SocketServer.IPAddressInfo>();
    }

    public void BroadcastMessage(string message)
    {
      message = "<SocketBroadcast>" + message + "</SocketBroadcast><EOF>";
      List<Guid> guidList = new List<Guid>();
      lock (this.client_addresses)
      {
        foreach (KeyValuePair<Guid, SocketServer.IPAddressInfo> clientAddress in this.client_addresses)
          guidList.Add(clientAddress.Key);
      }
      foreach (Guid client_guid in guidList)
      {
        try
        {
          this.SendMessageToClient(client_guid, message);
        }
        catch (InvalidOperationException ex)
        {
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in SockerServer.OnRawMessage " + ex.Message, ex);
        }
      }
    }

    public void RemoveClient(Guid guid)
    {
      lock (this.client_addresses)
      {
        if (this.client_addresses.ContainsKey(guid))
          this.client_addresses.Remove(guid);
      }
      this.OnClientRemoved(guid);
    }

    public void SendMessageToClient(Guid client_guid, string message)
    {
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.SendMessageToClientInternal), (object) new SocketServer.SendMessageData(client_guid, message));
    }

    public int ClientCount
    {
      get
      {
        int num = 0;
        if (this.client_addresses != null)
        {
          lock (this.client_addresses)
            num = this.client_addresses.Count;
        }
        return num;
      }
    }

    public override void OnRawMessage(string data, Socket handler)
    {
      try
      {
        SocketServer.ParsedMessage parsedMessage = new SocketServer.ParsedMessage(data);
        if (!parsedMessage.message.Contains("UpdatePrinterList"))
          parsedMessage.message.Contains("InitialConnect");
        SocketServer.IPAddressInfo remoteIP = new SocketServer.IPAddressInfo((IPAddress) null, parsedMessage.port);
        IPEndPoint remoteEndPoint = handler.RemoteEndPoint as IPEndPoint;
        if (remoteEndPoint != null)
          remoteIP.ip = remoteEndPoint.Address;
        this.ProcessGUIDPortPair(parsedMessage.guid, remoteIP);
        string str = this.onClientMessage(parsedMessage.guid, parsedMessage.message);
        if (string.IsNullOrEmpty(str))
          str = "OK";
        byte[] bytes = Encoding.UTF8.GetBytes(str + "<EOF>");
        handler.Send(bytes);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketServer.OnRawMessage " + ex.Message, ex);
      }
    }

    public virtual void onNewClientConnection(Guid guid)
    {
    }

    public virtual string onClientMessage(Guid guid, string message)
    {
      return (string) null;
    }

    public virtual void OnClientRemoved(Guid guid)
    {
    }

    private void ProcessGUIDPortPair(Guid guid, SocketServer.IPAddressInfo remoteIP)
    {
      lock (this.client_addresses)
      {
        if (!this.client_addresses.ContainsKey(guid))
        {
          this.client_addresses.Add(guid, remoteIP);
          this.onNewClientConnection(guid);
        }
        else
          this.client_addresses[guid] = remoteIP;
      }
    }

    private void SendMessageToClientInternal(object state)
    {
      SocketServer.SendMessageData sendMessageData = (SocketServer.SendMessageData) state;
      Guid clientGuid = sendMessageData.client_guid;
      string message = sendMessageData.message;
      for (int index = 0; index < 10; ++index)
      {
        if (this.SendMessageToClientInternal(clientGuid, message) == 1)
          return;
        Thread.Sleep((index + 1) * 200);
      }
      this.RemoveClient(clientGuid);
    }

    private int SendMessageToClientInternal(Guid client_guid, string message)
    {
      int num = 1;
      if (!message.StartsWith("<SocketBroadcast>"))
        message = "<SocketBroadcast>" + message + "</SocketBroadcast><EOF>";
      SocketServer.IPAddressInfo ipAddressInfo = (SocketServer.IPAddressInfo) null;
      lock (this.client_addresses)
      {
        if (this.client_addresses.ContainsKey(client_guid))
          ipAddressInfo = this.client_addresses[client_guid];
      }
      if (ipAddressInfo == null)
        return 0;
      IPAddress ipAddress = ipAddressInfo.ip;
      if (ipAddress == null)
      {
        foreach (IPAddress address in Dns.GetHostEntry("localhost").AddressList)
        {
          if (address.AddressFamily == AddressFamily.InterNetwork)
          {
            ipAddress = address;
            break;
          }
        }
        if (ipAddress == null)
          return 0;
      }
      IPEndPoint ipEndPoint = new IPEndPoint(ipAddressInfo.ip, ipAddressInfo.port);
      Socket socket = (Socket) null;
      try
      {
        if (socket == null)
          socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SendTimeout = 500;
        socket.ReceiveTimeout = 500;
        socket.Connect((EndPoint) ipEndPoint);
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        socket.Send(bytes);
        int length = bytes.Length;
        Thread.Sleep(100);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketServer.SendMessageToClient " + ex.Message, ex);
        num = 0;
      }
      try
      {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketServer.SendMessageToClient " + ex.Message, ex);
      }
      return num;
    }

    private struct ParsedMessage
    {
      public Guid guid;
      public int port;
      public string message;

      public ParsedMessage(string message)
      {
        int length = message.IndexOf("::");
        this.guid = new Guid(message.Substring(0, length));
        int num = message.IndexOf("::", length + 2);
        this.port = int.Parse(message.Substring(length + 2, num - (length + 2)));
        this.message = message.Substring(num + 2, message.IndexOf("<EOF>") - (num + 2));
      }
    }

    private struct SendMessageData
    {
      public Guid client_guid;
      public string message;

      public SendMessageData(Guid client_guid, string message)
      {
        this.client_guid = client_guid;
        this.message = message;
      }
    }

    private class IPAddressInfo
    {
      public IPAddress ip;
      public int port;

      public IPAddressInfo(IPAddress ip, int port)
      {
        this.ip = ip;
        this.port = port;
      }
    }
  }
}
