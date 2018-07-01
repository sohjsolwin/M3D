// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Sockets.SocketClient
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace M3D.Spooling.Sockets
{
  public class SocketClient : SocketPeer
  {
    private ThreadSafeVariable<bool> dontTimeOut = new ThreadSafeVariable<bool>(false);
    public CallBackOnReceivedRawMessage OnReceivedRawMessage;
    private int myListenerPort;
    private IPEndPoint remoteEP;

    public SocketClient()
    {
      this.myListenerPort = -1;
      this.DontTimeOut = false;
    }

    public int StartUp(int serverport)
    {
      IPAddress ipAddress1 = (IPAddress) null;
      try
      {
        IPAddress[] addressList = Dns.GetHostEntry("localhost").AddressList;
        if (addressList != null)
        {
          foreach (IPAddress ipAddress2 in addressList)
          {
            if (ipAddress2.AddressFamily == AddressFamily.InterNetwork)
            {
              ipAddress1 = ipAddress2;
              break;
            }
          }
        }
      }
      catch (SocketException ex)
      {
        ipAddress1 = IPAddress.Parse("127.0.0.1");
      }
      if (ipAddress1 == null)
        return 0;
      return this.StartUp(ipAddress1, serverport);
    }

    public int StartUp(IPAddress ipAddress, int serverport)
    {
      this.remoteEP = new IPEndPoint(ipAddress, serverport);
      if (this.StartSocketPeer(0) >= 0)
      {
        IPEndPoint localEndPoint = this.listener.LocalEndPoint as IPEndPoint;
        if (localEndPoint != null)
        {
          this.myListenerPort = localEndPoint.Port;
          this.StartListening();
          return 1;
        }
      }
      return 0;
    }

    public string SendMessage(string message)
    {
      return this.SendMessage(message, this.DontTimeOut ? 0 : 4000);
    }

    public string SendMessage(string message, int timeout)
    {
      string s = this.MyGuid.ToString() + "::" + this.myListenerPort.ToString() + "::" + message + "<EOF>";
      string str = (string) null;
      Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      socket.SendTimeout = timeout;
      socket.ReceiveTimeout = timeout;
      byte[] numArray = new byte[1024];
      try
      {
        socket.Connect((EndPoint) this.remoteEP);
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        socket.Send(bytes);
        while (true)
        {
          int count = socket.Receive(numArray);
          str += Encoding.UTF8.GetString(numArray, 0, count);
          if (str.IndexOf("<EOF>") <= -1)
            Thread.Sleep(1);
          else
            break;
        }
        if (socket.Connected)
        {
          try
          {
            socket.Shutdown(SocketShutdown.Both);
          }
          catch (Exception ex)
          {
          }
          try
          {
            socket.Close();
          }
          catch (Exception ex)
          {
          }
        }
      }
      catch (TimeoutException ex)
      {
        return "<TIMEDOUT>";
      }
      catch (Exception ex1)
      {
        ErrorLogger.LogException("Exception in SocketClient.SendMessage " + ex1.Message, ex1);
        try
        {
          socket.Close();
        }
        catch (Exception ex2)
        {
          ErrorLogger.LogException("Exception in SocketClient.SendMessage " + ex2.Message, ex2);
        }
        if (ex1 is ArgumentNullException)
          return "FAIL:ArgumentNullException : " + ex1.ToString();
        if (ex1 is SocketException)
          return "FAIL:SocketException : " + ex1.ToString();
        return "FAIL:Unexpected exception : " + ex1.ToString();
      }
      return str.Substring(0, str.IndexOf("<EOF>"));
    }

    public bool DontTimeOut
    {
      get
      {
        return this.dontTimeOut.Value;
      }
      set
      {
        this.dontTimeOut.Value = value;
      }
    }

    public override void OnRawMessage(string message, Socket handler)
    {
      if (this.OnReceivedRawMessage == null)
        return;
      this.OnReceivedRawMessage(message);
    }
  }
}
