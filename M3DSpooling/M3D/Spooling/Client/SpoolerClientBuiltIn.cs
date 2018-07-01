// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.SpoolerClientBuiltIn
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using System;
using System.Diagnostics;

namespace M3D.Spooling.Client
{
  public class SpoolerClientBuiltIn : SpoolerClient
  {
    public SpoolerClientBuiltIn(DebugLogger logger)
      : base(logger)
    {
    }

    public bool AUTO_UPDATE_FIRMWARE
    {
      get
      {
        return SpoolerServer.AUTO_UPDATE_FIRMWARE;
      }
      set
      {
        SpoolerServer.AUTO_UPDATE_FIRMWARE = value;
      }
    }

    public bool CHECK_INCOMPATIBLE_FIRMWARE
    {
      get
      {
        return SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE;
      }
      set
      {
        SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE = value;
      }
    }

    public bool CHECK_GANTRY_CLIPS
    {
      get
      {
        return SpoolerServer.CHECK_GANTRY_CLIPS;
      }
      set
      {
        SpoolerServer.CHECK_GANTRY_CLIPS = value;
      }
    }

    public bool CHECK_BED_CALIBRATION
    {
      get
      {
        return SpoolerServer.CHECK_BED_CALIBRATION;
      }
      set
      {
        SpoolerServer.CHECK_BED_CALIBRATION = value;
      }
    }

    public bool UsePreprocessors
    {
      get
      {
        return SpoolerServer.UsePreprocessors;
      }
      set
      {
        SpoolerServer.UsePreprocessors = value;
      }
    }

    public bool CheckFirmware
    {
      get
      {
        return SpoolerServer.CheckFirmware;
      }
      set
      {
        SpoolerServer.CheckFirmware = value;
      }
    }

    public bool StayInBootloader
    {
      get
      {
        return SpoolerServer.StayInBootloader;
      }
      set
      {
        SpoolerServer.StayInBootloader = value;
      }
    }

    public event EventHandler<EventArgs> OnReceivedSpoolerShutdownMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerShowMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerHideMessage;

    public SpoolerResult StartInternalSpoolerSession()
    {
      Trace.WriteLine("SpoolerClient2.StartSession");
      this.MyDebugLogger.Add("SpoolerClientInternal::StartSession", "Starting", DebugLogger.LogType.Secondary);
      SpoolerResult spoolerResult = SpoolerResult.Fail_Connect;
      try
      {
        InternalSpoolerConnection spoolerConnection = new InternalSpoolerConnection();
        spoolerConnection.XMLProcessor = new OnReceiveXMLFromSpooler(((SpoolerClient) this).ProcessXMLFromServer);
        if (!spoolerConnection.StartServer(42345))
          return SpoolerResult.Fail_Connect;
        spoolerConnection.OnReceivedSpoolerShutdownMessage = new EventHandler<EventArgs>(this.ReceivedSpoolerShutdownMessage);
        spoolerConnection.OnReceivedSpoolerShowMessage = new EventHandler<EventArgs>(this.ReceivedSpoolerShowMessage);
        spoolerConnection.OnReceivedSpoolerHideMessage = new EventHandler<EventArgs>(this.ReceivedSpoolerHideMessage);
        this.spooler_connection = (ISpoolerConnection) spoolerConnection;
        spoolerResult = this.InitialConnect();
        Trace.WriteLine(string.Format("SpoolerClient2.StartSession Completed {0}", (object) spoolerResult));
        this.MyDebugLogger.Add("SpoolerClientInternal::StartSession", "Connected to Spooler", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient2.StartSession " + ex.Message, ex);
        this.spooler_connection = (ISpoolerConnection) null;
        this.CloseSession();
      }
      if (this.spooler_connection != null)
        this.StartThreads();
      this.MyDebugLogger.Add("SpoolerClientInternal::StartSession", "SpoolerClient threads created", DebugLogger.LogType.Secondary);
      return spoolerResult;
    }

    public PublicPrinterConnection UnsafeFindPrinterConnection(PrinterSerialNumber serialnumber)
    {
      if (this.spooler_connection == null || !(this.spooler_connection is InternalSpoolerConnection))
        return (PublicPrinterConnection) null;
      return (PublicPrinterConnection) ((InternalSpoolerConnection) this.spooler_connection).SpoolerServer.GetPrinterConnection(serialnumber);
    }

    public bool ConnectInternalSpoolerToWindow(IntPtr hwnd)
    {
      if (this.spooler_connection == null || !(this.spooler_connection is InternalSpoolerConnection))
        return false;
      return ((InternalSpoolerConnection) this.spooler_connection).ConnectToWindow(hwnd);
    }

    public bool CanShutdown()
    {
      if (this.spooler_connection == null || !(this.spooler_connection is InternalSpoolerConnection))
        return true;
      return !this.IsBusy;
    }

    public bool DisconnectAllPrinters()
    {
      if (this.spooler_connection != null && this.spooler_connection is InternalSpoolerConnection)
      {
        InternalSpoolerConnection spoolerConnection = (InternalSpoolerConnection) this.spooler_connection;
        if (spoolerConnection.SpoolerServer != null)
        {
          spoolerConnection.SpoolerServer.DisconnectAll();
          return true;
        }
      }
      return false;
    }

    public int ClientCount
    {
      get
      {
        if (this.spooler_connection != null && this.spooler_connection is InternalSpoolerConnection)
        {
          InternalSpoolerConnection spoolerConnection = (InternalSpoolerConnection) this.spooler_connection;
          if (spoolerConnection.SpoolerServer != null)
            return spoolerConnection.SpoolerServer.ClientCount;
        }
        return 0;
      }
    }

    public void BroadcastMessage(string message)
    {
      if (this.spooler_connection == null || !(this.spooler_connection is InternalSpoolerConnection))
        return;
      InternalSpoolerConnection spoolerConnection = (InternalSpoolerConnection) this.spooler_connection;
      if (spoolerConnection.SpoolerServer == null)
        return;
      spoolerConnection.SpoolerServer.BroadcastMessage(message);
    }

    public bool IsBusy
    {
      get
      {
        if (this.spooler_connection != null && this.spooler_connection is InternalSpoolerConnection)
        {
          InternalSpoolerConnection spoolerConnection = (InternalSpoolerConnection) this.spooler_connection;
          if (spoolerConnection.SpoolerServer != null)
          {
            SpoolerServer spoolerServer = spoolerConnection.SpoolerServer;
            if (spoolerServer.NumActiveAndQueuedJobs <= 0)
              return spoolerServer.ArePrintersDoingWork;
            return true;
          }
        }
        return false;
      }
    }

    private void ReceivedSpoolerShutdownMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerShutdownMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerShutdownMessage(sender, e);
    }

    private void ReceivedSpoolerShowMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerShowMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerShowMessage(sender, e);
    }

    private void ReceivedSpoolerHideMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerHideMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerHideMessage(sender, e);
    }
  }
}
