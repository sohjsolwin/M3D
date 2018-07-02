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
      myListenerPort = -1;
      DontTimeOut = false;
    }

    public int StartUp(int serverport)
    {
      var ipAddress1 = (IPAddress) null;
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
      {
        return 0;
      }

      return StartUp(ipAddress1, serverport);
    }

    public int StartUp(IPAddress ipAddress, int serverport)
    {
      remoteEP = new IPEndPoint(ipAddress, serverport);
      if (StartSocketPeer(0) >= 0)
      {
        var localEndPoint = listener.LocalEndPoint as IPEndPoint;
        if (localEndPoint != null)
        {
          myListenerPort = localEndPoint.Port;
          StartListening();
          return 1;
        }
      }
      return 0;
    }

    public string SendMessage(string message)
    {
      return SendMessage(message, DontTimeOut ? 0 : 4000);
    }

    public string SendMessage(string message, int timeout)
    {
      var s = MyGuid.ToString() + "::" + myListenerPort.ToString() + "::" + message + "<EOF>";
      var str = (string) null;
      var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      {
        SendTimeout = timeout,
        ReceiveTimeout = timeout
      };
      byte[] numArray = new byte[1024];
      try
      {
        socket.Connect((EndPoint)remoteEP);
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        socket.Send(bytes);
        while (true)
        {
          var count = socket.Receive(numArray);
          str += Encoding.UTF8.GetString(numArray, 0, count);
          if (str.IndexOf("<EOF>") <= -1)
          {
            Thread.Sleep(1);
          }
          else
          {
            break;
          }
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
        {
          return "FAIL:ArgumentNullException : " + ex1.ToString();
        }

        if (ex1 is SocketException)
        {
          return "FAIL:SocketException : " + ex1.ToString();
        }

        return "FAIL:Unexpected exception : " + ex1.ToString();
      }
      return str.Substring(0, str.IndexOf("<EOF>"));
    }

    public bool DontTimeOut
    {
      get
      {
        return dontTimeOut.Value;
      }
      set
      {
        dontTimeOut.Value = value;
      }
    }

    public override void OnRawMessage(string message, Socket handler)
    {
      if (OnReceivedRawMessage == null)
      {
        return;
      }

      OnReceivedRawMessage(message);
    }
  }
}
