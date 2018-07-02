// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Sockets.SocketPeer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace M3D.Spooling.Sockets
{
  public class SocketPeer
  {
    private Guid myguid;
    protected Socket listener;
    private Thread SocketListenerThread;
    private bool blocking_socket;
    private object thread_lock;
    protected IPEndPoint localEndPoint;

    public SocketPeer()
    {
      myguid = Guid.NewGuid();
      thread_lock = new object();
      blocking_socket = false;
    }

    ~SocketPeer()
    {
      Shutdown();
    }

    public bool StartListening()
    {
      SocketListenerThread = new Thread(new ThreadStart(SocketListeningLoop))
      {
        Name = "Listener",
        Priority = ThreadPriority.Lowest,
        IsBackground = true
      };
      SocketListenerThread.Start();
      SocketListenerThread.IsBackground = true;
      return true;
    }

    public virtual void Shutdown()
    {
      if (SocketListenerThread == null)
      {
        return;
      }

      Thread socketListenerThread = SocketListenerThread;
      SocketListenerThread = (Thread) null;
      CloseListener();
      socketListenerThread.Abort();
    }

    private void CloseListener()
    {
      Socket listener = this.listener;
      this.listener = (Socket) null;
      if (listener == null)
      {
        return;
      }

      try
      {
        listener.Close();
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketPeer.CloseListener " + ex.Message, ex);
      }
    }

    public virtual void OnRawMessage(string message, Socket handler)
    {
    }

    public virtual int StartSocketPeer(int myListenerPort)
    {
      var num = 0;
      var address = (IPAddress) null;
      try
      {
        IPAddress[] addressList = Dns.GetHostEntry("localhost").AddressList;
        if (addressList != null)
        {
          foreach (IPAddress ipAddress in addressList)
          {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
              address = ipAddress;
              break;
            }
          }
        }
      }
      catch (SocketException ex)
      {
        address = IPAddress.Parse("127.0.0.1");
      }
      if (address == null)
      {
        return -5;
      }

      localEndPoint = new IPEndPoint(address, myListenerPort);
      listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      {
        ReceiveTimeout = 2000,
        Blocking = blocking_socket
      };
      try
      {
        listener.Bind((EndPoint)localEndPoint);
        listener.Listen(10);
      }
      catch (SocketException ex)
      {
        if (ex.ErrorCode == 10048)
        {
          ;
        }

        num = -2;
        listener = (Socket) null;
      }
      catch (ObjectDisposedException ex)
      {
        num = -3;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketPeer.StartServer " + ex.Message, ex);
      }
      return num;
    }

    public void SocketListeningLoop()
    {
      try
      {
        while (listener != null)
        {
          try
          {
            Socket socket = listener.Accept();
            if (socket != null)
            {
              ThreadPool.QueueUserWorkItem(new WaitCallback(HandleIncomingConnection), (object) socket);
            }
          }
          catch (SocketException ex)
          {
          }
          catch (ThreadAbortException ex)
          {
            throw ex;
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in Socketpeer.SocketListeningLoop " + ex.Message, ex);
            break;
          }
          Thread.Sleep(1);
        }
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketPeer.CoketListeningLoop 2 " + ex.Message, ex);
      }
      CloseListener();
      Shutdown();
    }

    public void HandleIncomingConnection(object client)
    {
      var handler = (Socket) null;
      var message = (string) null;
      byte[] numArray = new byte[1024];
      try
      {
        handler = (Socket) client;
        handler.ReceiveTimeout = 2000;
        while (true)
        {
          var count = 0;
          try
          {
            count = handler.Receive(numArray);
          }
          catch (SocketException ex)
          {
            if (ex.ErrorCode != 10035)
            {
              throw ex;
            }
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in SocketPeer.HandleIncomingConnection" + ex.Message, ex);
          }
          message += Encoding.UTF8.GetString(numArray, 0, count);
          if (message.IndexOf("<EOF>") <= -1)
          {
            Thread.Sleep(1);
          }
          else
          {
            break;
          }
        }
        OnRawMessage(message, handler);
      }
      catch (Exception ex)
      {
      }
      try
      {
        handler.Shutdown(SocketShutdown.Both);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketPeer.HandleIncomingConnection " + ex.Message, ex);
      }
      try
      {
        handler.Close();
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SocketPeer.HandleIncomingConnection " + ex.Message, ex);
      }
    }

    public Guid MyGuid
    {
      get
      {
        return myguid;
      }
    }
  }
}
