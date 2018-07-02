// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.SpoolerServer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.ConnectionManager;
using M3D.Spooling.Embedded_Firmware;
using M3D.Spooling.Printer_Profiles;
using M3D.Spooling.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace M3D.Spooling.Core
{
  internal class SpoolerServer : SocketServer
  {
    public static bool AUTO_UPDATE_FIRMWARE = false;
    public static bool CHECK_INCOMPATIBLE_FIRMWARE = true;
    public static bool CHECK_GANTRY_CLIPS = true;
    public static bool CHECK_BED_CALIBRATION = true;
    public static bool UsePreprocessors = true;
    public static bool CheckFirmware = false;
    public static bool StayInBootloader = false;
    internal static Random RandomGenerator = new Random();
    private List<string> LockExceptionList = new List<string>() { "StopJob", "SendEmergencyStop", "SetPrintLogFeedback", "SetPrintLogWaits", "PausePrint", "BreakLock", "SetTemperatureWhilePrinting", "AddUpdateKeyValuePair" };
    private ThreadSafeVariable<uint> current_processing_id = new ThreadSafeVariable<uint>(0U);
    private ThreadSafeVariable<bool> shared_shutdown;
    private ThreadBasedTimer network_logging_timer;
    private List<SpoolerMessage> logging_queue;
    private ThreadSafeVariable<Logger.OnLogDel> __onlog;
    private PrinterConnectionManager connectionManager;
    private PrinterProfileDictionary profile_dictionary;
    private object thread_sync;
    private object printer_handshake;
    private IBroadcastServer broadcastserver;
    private RPCInvoker rpc_invoker;

    internal Dictionary<string, List<FirmwareBoardVersionKVP>> GetEmbeddedFirmwareList()
    {
      var dictionary = new Dictionary<string, List<FirmwareBoardVersionKVP>>();
      foreach (VID_PID vidPid in profile_dictionary.GenerateVID_PID_List())
      {
        InternalPrinterProfile internalPrinterProfile = profile_dictionary.Get(vidPid);
        var firmwareBoardVersionKvpList = new List<FirmwareBoardVersionKVP>();
        foreach (KeyValuePair<char, FirmwareDetails> firmware in internalPrinterProfile.ProductConstants.FirmwareList)
        {
          firmwareBoardVersionKvpList.Add(new FirmwareBoardVersionKVP(firmware.Key, firmware.Value.firmware_version));
        }

        dictionary.Add(internalPrinterProfile.ProfileName, firmwareBoardVersionKvpList);
      }
      return dictionary;
    }

    public event EventHandler<EventArgs> OnReceivedSpoolerShutdownMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerShowMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerHideMessage;

    public SpoolerServer()
    {
      shared_shutdown = new ThreadSafeVariable<bool>(false);
      profile_dictionary = new PrinterProfileDictionary();
      broadcastserver = (IBroadcastServer) null;
      rpc_invoker = new RPCInvoker((object) this);
      thread_sync = new object();
      printer_handshake = new object();
      logging_queue = new List<SpoolerMessage>();
      network_logging_timer = new ThreadBasedTimer((IContainer)null, shared_shutdown)
      {
        Interval = 100,
        Tick = new EventHandler(onLogger_Tick)
      };
      network_logging_timer.Start();
    }

    ~SpoolerServer()
    {
      shared_shutdown.Value = true;
    }

    public override void Shutdown()
    {
      shared_shutdown.Value = true;
      base.Shutdown();
    }

    public bool ConnectToWindow(IntPtr hWnd)
    {
      if (!StartListening())
      {
        return false;
      }

      StartConnectionManager();
      return true;
    }

    private int CompareDateTime(KeyValuePair<string, DateTime> a, KeyValuePair<string, DateTime> b)
    {
      return -a.Value.CompareTo(b.Value);
    }

    private void appDataCleanUp()
    {
      var stringList = new List<string>();
      var dictionary1 = new Dictionary<string, List<string>>();
      var dictionary2 = new Dictionary<string, DateTime>();
      DateTime t2 = DateTime.Now.AddDays(-7.0);
      ulong num1 = 1000000000;
      try
      {
        stringList.AddRange((IEnumerable<string>) Directory.GetFiles(Paths.SpoolerFolder));
      }
      catch (Exception ex)
      {
      }
      try
      {
        stringList.AddRange((IEnumerable<string>) Directory.GetFiles(Paths.QueuePath));
      }
      catch (Exception ex)
      {
      }
      try
      {
        stringList.AddRange((IEnumerable<string>) Directory.GetFiles(Paths.LogPath));
      }
      catch (Exception ex)
      {
      }
      if (stringList.Count == 0)
      {
        return;
      }

      foreach (var path in stringList)
      {
        try
        {
          DateTime lastAccessTime = File.GetLastAccessTime(path);
          var withoutExtension = Path.GetFileNameWithoutExtension(path);
          if (!dictionary1.ContainsKey(withoutExtension))
          {
            dictionary1.Add(withoutExtension, new List<string>());
          }

          dictionary1[withoutExtension].Add(path);
          if (!dictionary2.ContainsKey(withoutExtension))
          {
            dictionary2.Add(withoutExtension, lastAccessTime);
          }
          else if (DateTime.Compare(dictionary2[withoutExtension], lastAccessTime) < 0)
          {
            dictionary2[withoutExtension] = lastAccessTime;
          }
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException(string.Format("Exception in SpoolerServer.appdatacleanup: {0}", (object) ex.Message), ex);
          ErrorLogger.LogErrorMsg(string.Format("SpoolServer.appdatacleanup path Clean up threw: {0}", (object) ex.Message));
        }
      }
      var keyValuePairList = new List<KeyValuePair<string, DateTime>>();
      foreach (var key in dictionary2.Keys)
      {
        keyValuePairList.Add(new KeyValuePair<string, DateTime>(key, dictionary2[key]));
      }

      dictionary2.Clear();
      keyValuePairList.Sort(new Comparison<KeyValuePair<string, DateTime>>(CompareDateTime));
      for (var index = 0; index < keyValuePairList.Count; ++index)
      {
        KeyValuePair<string, DateTime> keyValuePair = keyValuePairList[index];
        var key = keyValuePair.Key;
        keyValuePair = keyValuePairList[index];
        if (DateTime.Compare(keyValuePair.Value, t2) < 0)
        {
          foreach (var path in dictionary1[key])
          {
            try
            {
              File.Delete(path);
            }
            catch (IOException ex)
            {
            }
            catch (Exception ex)
            {
              ErrorLogger.LogException("Exception in SpoolerServer.appdatacleanup 1" + ex.Message, ex);
              ErrorLogger.LogErrorMsg(string.Format("SpoolServer.appdatacleanup path Clean up threw:{0}", (object) ex.Message));
            }
          }
          dictionary1.Remove(key);
          keyValuePairList.Remove(keyValuePairList[index]);
          --index;
        }
      }
      ulong num2 = 0;
      foreach (KeyValuePair<string, DateTime> keyValuePair in keyValuePairList)
      {
        var key = keyValuePair.Key;
        if (num2 < num1)
        {
          foreach (var fileName in dictionary1[key])
          {
            num2 += (ulong) new FileInfo(fileName).Length;
          }
        }
        else
        {
          foreach (var path in dictionary1[key])
          {
            try
            {
              File.Delete(path);
            }
            catch (Exception ex)
            {
              ErrorLogger.LogException("Exception in SpoolerServer.appdatacleanup 2" + ex.Message, ex);
              ErrorLogger.LogErrorMsg(string.Format("SpoolServer.appdatacleanup path Clean up threw:{0}", (object) ex.Message));
            }
          }
        }
      }
    }

    private void InitializeAppData()
    {
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      Directory.CreateDirectory(Paths.SpoolerFolder);
      Directory.CreateDirectory(Paths.QueuePath);
      Directory.CreateDirectory(Paths.LogPath);
      Directory.CreateDirectory(Paths.SpoolerStorage);
      appDataCleanUp();
    }

    public void SetBroadcastServer(IBroadcastServer brodcastserver)
    {
      broadcastserver = brodcastserver;
    }

    public void CloseConnections()
    {
      DisconnectAll();
    }

    public void DisconnectAll()
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
      {
        printer.Value.Shutdown();
      }
    }

    public override int StartSocketPeer(int port)
    {
      var num = base.StartSocketPeer(port);
      if (num < 0)
      {
        return num;
      }

      InitializeAppData();
      return num;
    }

    public override void onNewClientConnection(Guid guid)
    {
    }

    public override void OnClientRemoved(Guid guid)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
      {
        printer.Value.OnClientRemoved(guid);
      }
    }

    private void PrinterLoggerCallback(string message, string printer_serial)
    {
      if (printer_serial == "Detecting")
      {
        message = "Detecting>" + message;
        printer_serial = "00-00-00-00-00-000-000";
      }
      var spoolerMessage = new SpoolerMessage(MessageType.LoggingMessage, new PrinterSerialNumber(printer_serial), Base64Convert.Base64Encode(message));
      lock (logging_queue)
      {
        logging_queue.Add(spoolerMessage);
      }
    }

    private void onLogger_Tick(object sender, EventArgs e)
    {
      List<SpoolerMessage> spoolerMessageList;
      lock (logging_queue)
      {
        spoolerMessageList = new List<SpoolerMessage>((IEnumerable<SpoolerMessage>)logging_queue);
        logging_queue.Clear();
      }
      foreach (SpoolerMessage spoolerMessage in spoolerMessageList)
      {
        if (broadcastserver != null)
        {
          broadcastserver.BroadcastMessage(spoolerMessage.Serialize());
        }
        else
        {
          BroadcastMessage(spoolerMessage.Serialize());
        }
      }
    }

    public Logger.OnLogDel LoggerCallback
    {
      get
      {
        return __onlog.Value;
      }
      set
      {
        __onlog.Value = value;
      }
    }

    private void StartConnectionManager()
    {
      connectionManager = new PrinterConnectionManager(profile_dictionary.GenerateVID_PID_List());
      connectionManager.PrinterConnectedEventHandler += new EventHandler<PrinterConnEventArgs>(OnPrinterConnected);
      connectionManager.PrinterDisconnectedEventHandler += new EventHandler<PrinterConnEventArgs>(OnPrinterDisconnected);
      connectionManager.LogEventHandler += new EventHandler<LogMessageEventArgs>(OnLogEventHandler);
      connectionManager.Start(shared_shutdown);
    }

    private void OnPrinterConnected(object sender, PrinterConnEventArgs e)
    {
      if (e.printer == null)
      {
        return;
      }

      e.printer.SetPrinterProfile(profile_dictionary.Get(e.vid_pid));
      ConnectToPrinter(e.printer);
    }

    private void OnPrinterDisconnected(object sender, PrinterConnEventArgs e)
    {
      if (e.printer == null)
      {
        return;
      }

      RemovePrinter(e.printer);
    }

    private void OnLogEventHandler(object sender, LogMessageEventArgs e)
    {
      PrinterLoggerCallback(e.message, "Detecting");
    }

    public void RemovePrinter(PrinterConnection connection)
    {
      connection.Shutdown();
    }

    public int ConnectToPrinter(PrinterConnection serial_connection)
    {
      if (string.IsNullOrEmpty(serial_connection.ComPort))
      {
        return 0;
      }

      serial_connection.InternalLogger._onLog += new Logger.OnLogDel(PrinterLoggerCallback);
      serial_connection.InitializeController(broadcastserver);
      serial_connection.StartSerialProcessing();
      return 1;
    }

    public string Serialize()
    {
      var str1 = "";
      if (connectionManager != null && connectionManager.printers != null)
      {
        lock (connectionManager.printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
          {
            PrinterConnection printerConnection = printer.Value;
            var str2 = (string) null;
            try
            {
              if (printerConnection.PrinterInfo.serial_number != PrinterSerialNumber.Undefined)
              {
                if (printerConnection.PrinterInfo.Status != PrinterStatus.Uninitialized)
                {
                  if (printerConnection.PrinterInfo.Status != PrinterStatus.Connected)
                  {
                    if (printerConnection.PrinterInfo.Status != PrinterStatus.Error_PrinterNotAlive)
                    {
                      str2 = printerConnection.Serialize();
                    }
                  }
                }
              }
            }
            catch (InvalidOperationException ex)
            {
              str2 = (string) null;
            }
            if (!string.IsNullOrEmpty(str2))
            {
              str1 += str2;
            }
          }
        }
        str1 += string.Format("<DeviceInstallDetected count=\"{0}\" />", (object) WinUSBPrinterFinder.DeviceInstallDetected);
      }
      return "<SPOOLER__ALL/>" + str1;
    }

    internal PrinterConnection GetPrinterConnection(PrinterSerialNumber serialnumber)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
      {
        PrinterConnection printerConnection = printer.Value;
        if (printerConnection.PrinterInfo.serial_number == serialnumber)
        {
          return printerConnection;
        }
      }
      return (PrinterConnection) null;
    }

    private string LockVerifyResultToSpoolerMessage(CommandResult result, PrinterSerialNumber serialnumber, uint callID)
    {
      if (result == CommandResult.Success)
      {
        return (string) null;
      }

      return new SpoolerMessage(MessageType.LockResult, serialnumber, callID.ToString("D8") + result.ToString()).Serialize();
    }

    public string onClientMessage(string message)
    {
      return onClientMessage(MyGuid, message);
    }

    public override string onClientMessage(Guid guid, string message)
    {
      try
      {
        if (message == "<CloseConnection/>")
        {
          if (guid != MyGuid)
          {
            RemoveClient(guid);
          }
        }
        else if (message.StartsWith("<RPC"))
        {
          var call = new RPCInvoker.RPC();
          call.Deserialize(message);
          var str = (string) null;
          var obj = (object) null;
          try
          {
            if (call.serialnumber == (PrinterSerialNumber) null)
            {
              obj = rpc_invoker.CallMethod(call);
            }
            else
            {
              PrinterConnection printerConnection = GetPrinterConnection(call.serialnumber);
              if (printerConnection != null)
              {
                if (call.callID != 0U)
                {
                  if ((int) call.callID == (int)current_processing_id.Value)
                  {
                    return (string) null;
                  }

                  current_processing_id.Value = call.callID;
                }
                CommandResult result;
                if (call.name == "AcquireLock")
                {
                  result = printerConnection.AcquireLock(guid);
                }
                else if (call.name == "ReleaseLock")
                {
                  result = printerConnection.ReleaseLock(call.lockID);
                }
                else
                {
                  var flag = true;
                  if (LockExceptionList.Contains(call.name))
                  {
                    flag = false;
                  }

                  result = printerConnection.VerifyLock(call.lockID);
                  if (result == CommandResult.Failed_PrinterDoesNotHaveLock && call.name == "SendEmergencyStop" && printerConnection.IsWorking)
                  {
                    printerConnection.SendInterrupted(LockVerifyResultToSpoolerMessage(CommandResult.CommandInterruptedByM0, call.serialnumber, 0U));
                  }

                  if (result == CommandResult.Success || !flag)
                  {
                    obj = rpc_invoker.CallMethod((object) printerConnection, call);
                    if (obj is CommandResult)
                    {
                      result = (CommandResult) obj;
                      obj = (object) null;
                    }
                  }
                }
                if (call.callID != 0U)
                {
                  printerConnection.CurrentRPC_id.Value = call.callID;
                }

                str = LockVerifyResultToSpoolerMessage(result, call.serialnumber, call.callID);
              }
              else
              {
                str = new SpoolerMessage(MessageType.PrinterNotConnected, call.serialnumber, call.name).Serialize();
              }
            }
          }
          catch (Exception ex)
          {
            str = new SpoolerMessage(MessageType.RPCError, ex.Message + "::" + call.name).Serialize();
          }
          if (str != null)
          {
            return "<SocketBroadcast>" + str + "</SocketBroadcast>";
          }

          if (obj != null)
          {
            if (obj is string)
            {
              return (string) obj;
            }
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerServer.OnClientMessage " + ex.Message, ex);
        return "FAIL";
      }
      return (string) null;
    }

    public string InitialConnect(VersionNumber client_version)
    {
      if (client_version != M3D.Spooling.Version.Client_Version)
      {
        return "<SocketBroadcast>" + new SpoolerMessage(MessageType.IncompatibleSpooler, (string) null).Serialize() + "</SocketBroadcast>";
      }

      var spoolerInfo = new SpoolerInfo
      {
        Version = M3D.Spooling.Version.Client_Version
      };
      foreach (KeyValuePair<string, List<FirmwareBoardVersionKVP>> embeddedFirmware in GetEmbeddedFirmwareList())
      {
        var embeddedFirmwareSummary = new EmbeddedFirmwareSummary(embeddedFirmware.Key);
        foreach (FirmwareBoardVersionKVP firmwareBoardVersionKvp in embeddedFirmware.Value)
        {
          embeddedFirmwareSummary.FirmwareVersions.Add(firmwareBoardVersionKvp);
        }

        spoolerInfo.SupportPrinterProfiles.Add(embeddedFirmwareSummary);
      }
      spoolerInfo.PrinterProfileList = profile_dictionary.CreateProfileList();
      return "<SocketBroadcast>" + spoolerInfo.Serialize() + Serialize() + "</SocketBroadcast>";
    }

    public string UpdatePrinterList()
    {
      return "<SocketBroadcast>" + Serialize() + "</SocketBroadcast>";
    }

    public void ForceShutdownSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerShutdownMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerShutdownMessage((object) this, new EventArgs());
    }

    public void HideSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerHideMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerHideMessage((object) this, new EventArgs());
    }

    public void ShowSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (OnReceivedSpoolerShowMessage == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      OnReceivedSpoolerShowMessage((object) this, new EventArgs());
    }

    public int NumActiveAndQueuedJobs
    {
      get
      {
        var num = 0;
        foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
        {
          PrinterConnection printerConnection = printer.Value;
          if (printerConnection != null)
          {
            num += printerConnection.GetJobsCount();
          }
        }
        return num;
      }
    }

    public bool ArePrintersDoingWork
    {
      get
      {
        var flag = false;
        foreach (KeyValuePair<string, PrinterConnection> printer in connectionManager.printers)
        {
          PrinterConnection printerConnection = printer.Value;
          if (printerConnection != null)
          {
            flag |= printerConnection.IsWorking;
          }
        }
        return flag;
      }
    }
  }
}
