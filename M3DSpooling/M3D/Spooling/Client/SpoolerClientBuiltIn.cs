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
      MyDebugLogger.Add("SpoolerClientInternal::StartSession", "Starting", DebugLogger.LogType.Secondary);
      SpoolerResult spoolerResult = SpoolerResult.Fail_Connect;
      try
      {
        var spoolerConnection = new InternalSpoolerConnection
        {
          XMLProcessor = new OnReceiveXMLFromSpooler((this).ProcessXMLFromServer)
        };
        if (!spoolerConnection.StartServer(42345))
        {
          return SpoolerResult.Fail_Connect;
        }

        spoolerConnection.OnReceivedSpoolerShutdownMessage = new EventHandler<EventArgs>(ReceivedSpoolerShutdownMessage);
        spoolerConnection.OnReceivedSpoolerShowMessage = new EventHandler<EventArgs>(ReceivedSpoolerShowMessage);
        spoolerConnection.OnReceivedSpoolerHideMessage = new EventHandler<EventArgs>(ReceivedSpoolerHideMessage);
        spooler_connection = spoolerConnection;
        spoolerResult = InitialConnect();
        Trace.WriteLine(string.Format("SpoolerClient2.StartSession Completed {0}", spoolerResult));
        MyDebugLogger.Add("SpoolerClientInternal::StartSession", "Connected to Spooler", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient2.StartSession " + ex.Message, ex);
        spooler_connection = null;
        CloseSession();
      }
      if (spooler_connection != null)
      {
        StartThreads();
      }

      MyDebugLogger.Add("SpoolerClientInternal::StartSession", "SpoolerClient threads created", DebugLogger.LogType.Secondary);
      return spoolerResult;
    }

    public PublicPrinterConnection UnsafeFindPrinterConnection(PrinterSerialNumber serialnumber)
    {
      if (spooler_connection == null || !(spooler_connection is InternalSpoolerConnection))
      {
        return null;
      }

      return ((InternalSpoolerConnection)spooler_connection).SpoolerServer.GetPrinterConnection(serialnumber);
    }

    public bool ConnectInternalSpoolerToWindow(IntPtr hwnd)
    {
      if (spooler_connection == null || !(spooler_connection is InternalSpoolerConnection))
      {
        return false;
      }

      return ((InternalSpoolerConnection)spooler_connection).ConnectToWindow(hwnd);
    }

    public bool CanShutdown()
    {
      if (spooler_connection == null || !(spooler_connection is InternalSpoolerConnection))
      {
        return true;
      }

      return !IsBusy;
    }

    public bool DisconnectAllPrinters()
    {
      if (spooler_connection != null && spooler_connection is InternalSpoolerConnection)
      {
        var spoolerConnection = (InternalSpoolerConnection)spooler_connection;
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
        if (spooler_connection != null && spooler_connection is InternalSpoolerConnection)
        {
          var spoolerConnection = (InternalSpoolerConnection)spooler_connection;
          if (spoolerConnection.SpoolerServer != null)
          {
            return spoolerConnection.SpoolerServer.ClientCount;
          }
        }
        return 0;
      }
    }

    public void BroadcastMessage(string message)
    {
      if (spooler_connection == null || !(spooler_connection is InternalSpoolerConnection))
      {
        return;
      }

      var spoolerConnection = (InternalSpoolerConnection)spooler_connection;
      if (spoolerConnection.SpoolerServer == null)
      {
        return;
      }

      spoolerConnection.SpoolerServer.BroadcastMessage(message);
    }

    public bool IsBusy
    {
      get
      {
        if (spooler_connection != null && spooler_connection is InternalSpoolerConnection)
        {
          var spoolerConnection = (InternalSpoolerConnection)spooler_connection;
          if (spoolerConnection.SpoolerServer != null)
          {
            SpoolerServer spoolerServer = spoolerConnection.SpoolerServer;
            if (spoolerServer.NumActiveAndQueuedJobs <= 0)
            {
              return spoolerServer.ArePrintersDoingWork;
            }

            return true;
          }
        }
        return false;
      }
    }

    private void ReceivedSpoolerShutdownMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerShutdownMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerShutdownMessage(sender, e);
    }

    private void ReceivedSpoolerShowMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerShowMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerShowMessage(sender, e);
    }

    private void ReceivedSpoolerHideMessage(object sender, EventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerHideMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerHideMessage(sender, e);
    }
  }
}
