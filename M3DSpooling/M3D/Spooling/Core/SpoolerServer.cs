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
      Dictionary<string, List<FirmwareBoardVersionKVP>> dictionary = new Dictionary<string, List<FirmwareBoardVersionKVP>>();
      foreach (VID_PID vidPid in this.profile_dictionary.GenerateVID_PID_List())
      {
        InternalPrinterProfile internalPrinterProfile = this.profile_dictionary.Get(vidPid);
        List<FirmwareBoardVersionKVP> firmwareBoardVersionKvpList = new List<FirmwareBoardVersionKVP>();
        foreach (KeyValuePair<char, FirmwareDetails> firmware in internalPrinterProfile.ProductConstants.FirmwareList)
          firmwareBoardVersionKvpList.Add(new FirmwareBoardVersionKVP(firmware.Key, firmware.Value.firmware_version));
        dictionary.Add(internalPrinterProfile.ProfileName, firmwareBoardVersionKvpList);
      }
      return dictionary;
    }

    public event EventHandler<EventArgs> OnReceivedSpoolerShutdownMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerShowMessage;

    public event EventHandler<EventArgs> OnReceivedSpoolerHideMessage;

    public SpoolerServer()
    {
      this.shared_shutdown = new ThreadSafeVariable<bool>(false);
      this.profile_dictionary = new PrinterProfileDictionary();
      this.broadcastserver = (IBroadcastServer) null;
      this.rpc_invoker = new RPCInvoker((object) this);
      this.thread_sync = new object();
      this.printer_handshake = new object();
      this.logging_queue = new List<SpoolerMessage>();
      this.network_logging_timer = new ThreadBasedTimer((IContainer) null, this.shared_shutdown);
      this.network_logging_timer.Interval = 100;
      this.network_logging_timer.Tick = new EventHandler(this.onLogger_Tick);
      this.network_logging_timer.Start();
    }

    ~SpoolerServer()
    {
      this.shared_shutdown.Value = true;
    }

    public override void Shutdown()
    {
      this.shared_shutdown.Value = true;
      base.Shutdown();
    }

    public bool ConnectToWindow(IntPtr hWnd)
    {
      if (!this.StartListening())
        return false;
      this.StartConnectionManager();
      return true;
    }

    private int CompareDateTime(KeyValuePair<string, DateTime> a, KeyValuePair<string, DateTime> b)
    {
      return -a.Value.CompareTo(b.Value);
    }

    private void appDataCleanUp()
    {
      List<string> stringList = new List<string>();
      Dictionary<string, List<string>> dictionary1 = new Dictionary<string, List<string>>();
      Dictionary<string, DateTime> dictionary2 = new Dictionary<string, DateTime>();
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
        return;
      foreach (string path in stringList)
      {
        try
        {
          DateTime lastAccessTime = File.GetLastAccessTime(path);
          string withoutExtension = Path.GetFileNameWithoutExtension(path);
          if (!dictionary1.ContainsKey(withoutExtension))
            dictionary1.Add(withoutExtension, new List<string>());
          dictionary1[withoutExtension].Add(path);
          if (!dictionary2.ContainsKey(withoutExtension))
            dictionary2.Add(withoutExtension, lastAccessTime);
          else if (DateTime.Compare(dictionary2[withoutExtension], lastAccessTime) < 0)
            dictionary2[withoutExtension] = lastAccessTime;
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException(string.Format("Exception in SpoolerServer.appdatacleanup: {0}", (object) ex.Message), ex);
          ErrorLogger.LogErrorMsg(string.Format("SpoolServer.appdatacleanup path Clean up threw: {0}", (object) ex.Message));
        }
      }
      List<KeyValuePair<string, DateTime>> keyValuePairList = new List<KeyValuePair<string, DateTime>>();
      foreach (string key in dictionary2.Keys)
        keyValuePairList.Add(new KeyValuePair<string, DateTime>(key, dictionary2[key]));
      dictionary2.Clear();
      keyValuePairList.Sort(new Comparison<KeyValuePair<string, DateTime>>(this.CompareDateTime));
      for (int index = 0; index < keyValuePairList.Count; ++index)
      {
        KeyValuePair<string, DateTime> keyValuePair = keyValuePairList[index];
        string key = keyValuePair.Key;
        keyValuePair = keyValuePairList[index];
        if (DateTime.Compare(keyValuePair.Value, t2) < 0)
        {
          foreach (string path in dictionary1[key])
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
        string key = keyValuePair.Key;
        if (num2 < num1)
        {
          foreach (string fileName in dictionary1[key])
            num2 += (ulong) new FileInfo(fileName).Length;
        }
        else
        {
          foreach (string path in dictionary1[key])
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
      this.appDataCleanUp();
    }

    public void SetBroadcastServer(IBroadcastServer brodcastserver)
    {
      this.broadcastserver = brodcastserver;
    }

    public void CloseConnections()
    {
      this.DisconnectAll();
    }

    public void DisconnectAll()
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
        printer.Value.Shutdown();
    }

    public override int StartSocketPeer(int port)
    {
      int num = base.StartSocketPeer(port);
      if (num < 0)
        return num;
      this.InitializeAppData();
      return num;
    }

    public override void onNewClientConnection(Guid guid)
    {
    }

    public override void OnClientRemoved(Guid guid)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
        printer.Value.OnClientRemoved(guid);
    }

    private void PrinterLoggerCallback(string message, string printer_serial)
    {
      if (printer_serial == "Detecting")
      {
        message = "Detecting>" + message;
        printer_serial = "00-00-00-00-00-000-000";
      }
      SpoolerMessage spoolerMessage = new SpoolerMessage(MessageType.LoggingMessage, new PrinterSerialNumber(printer_serial), Base64Convert.Base64Encode(message));
      lock (this.logging_queue)
        this.logging_queue.Add(spoolerMessage);
    }

    private void onLogger_Tick(object sender, EventArgs e)
    {
      List<SpoolerMessage> spoolerMessageList;
      lock (this.logging_queue)
      {
        spoolerMessageList = new List<SpoolerMessage>((IEnumerable<SpoolerMessage>) this.logging_queue);
        this.logging_queue.Clear();
      }
      foreach (SpoolerMessage spoolerMessage in spoolerMessageList)
      {
        if (this.broadcastserver != null)
          this.broadcastserver.BroadcastMessage(spoolerMessage.Serialize());
        else
          this.BroadcastMessage(spoolerMessage.Serialize());
      }
    }

    public Logger.OnLogDel LoggerCallback
    {
      get
      {
        return this.__onlog.Value;
      }
      set
      {
        this.__onlog.Value = value;
      }
    }

    private void StartConnectionManager()
    {
      this.connectionManager = new PrinterConnectionManager(this.profile_dictionary.GenerateVID_PID_List());
      this.connectionManager.PrinterConnectedEventHandler += new EventHandler<PrinterConnEventArgs>(this.OnPrinterConnected);
      this.connectionManager.PrinterDisconnectedEventHandler += new EventHandler<PrinterConnEventArgs>(this.OnPrinterDisconnected);
      this.connectionManager.LogEventHandler += new EventHandler<LogMessageEventArgs>(this.OnLogEventHandler);
      this.connectionManager.Start(this.shared_shutdown);
    }

    private void OnPrinterConnected(object sender, PrinterConnEventArgs e)
    {
      if (e.printer == null)
        return;
      e.printer.SetPrinterProfile(this.profile_dictionary.Get(e.vid_pid));
      this.ConnectToPrinter(e.printer);
    }

    private void OnPrinterDisconnected(object sender, PrinterConnEventArgs e)
    {
      if (e.printer == null)
        return;
      this.RemovePrinter(e.printer);
    }

    private void OnLogEventHandler(object sender, LogMessageEventArgs e)
    {
      this.PrinterLoggerCallback(e.message, "Detecting");
    }

    public void RemovePrinter(PrinterConnection connection)
    {
      connection.Shutdown();
    }

    public int ConnectToPrinter(PrinterConnection serial_connection)
    {
      if (string.IsNullOrEmpty(serial_connection.ComPort))
        return 0;
      serial_connection.InternalLogger._onLog += new Logger.OnLogDel(this.PrinterLoggerCallback);
      serial_connection.InitializeController(this.broadcastserver);
      serial_connection.StartSerialProcessing();
      return 1;
    }

    public string Serialize()
    {
      string str1 = "";
      if (this.connectionManager != null && this.connectionManager.printers != null)
      {
        lock (this.connectionManager.printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
          {
            PrinterConnection printerConnection = printer.Value;
            string str2 = (string) null;
            try
            {
              if (printerConnection.PrinterInfo.serial_number != PrinterSerialNumber.Undefined)
              {
                if (printerConnection.PrinterInfo.Status != PrinterStatus.Uninitialized)
                {
                  if (printerConnection.PrinterInfo.Status != PrinterStatus.Connected)
                  {
                    if (printerConnection.PrinterInfo.Status != PrinterStatus.Error_PrinterNotAlive)
                      str2 = printerConnection.Serialize();
                  }
                }
              }
            }
            catch (InvalidOperationException ex)
            {
              str2 = (string) null;
            }
            if (!string.IsNullOrEmpty(str2))
              str1 += str2;
          }
        }
        str1 += string.Format("<DeviceInstallDetected count=\"{0}\" />", (object) WinUSBPrinterFinder.DeviceInstallDetected);
      }
      return "<SPOOLER__ALL/>" + str1;
    }

    internal PrinterConnection GetPrinterConnection(PrinterSerialNumber serialnumber)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
      {
        PrinterConnection printerConnection = printer.Value;
        if (printerConnection.PrinterInfo.serial_number == serialnumber)
          return printerConnection;
      }
      return (PrinterConnection) null;
    }

    private string LockVerifyResultToSpoolerMessage(CommandResult result, PrinterSerialNumber serialnumber, uint callID)
    {
      if (result == CommandResult.Success)
        return (string) null;
      return new SpoolerMessage(MessageType.LockResult, serialnumber, callID.ToString("D8") + result.ToString()).Serialize();
    }

    public string onClientMessage(string message)
    {
      return this.onClientMessage(this.MyGuid, message);
    }

    public override string onClientMessage(Guid guid, string message)
    {
      try
      {
        if (message == "<CloseConnection/>")
        {
          if (guid != this.MyGuid)
            this.RemoveClient(guid);
        }
        else if (message.StartsWith("<RPC"))
        {
          RPCInvoker.RPC call = new RPCInvoker.RPC();
          call.Deserialize(message);
          string str = (string) null;
          object obj = (object) null;
          try
          {
            if (call.serialnumber == (PrinterSerialNumber) null)
            {
              obj = this.rpc_invoker.CallMethod(call);
            }
            else
            {
              PrinterConnection printerConnection = this.GetPrinterConnection(call.serialnumber);
              if (printerConnection != null)
              {
                if (call.callID != 0U)
                {
                  if ((int) call.callID == (int) this.current_processing_id.Value)
                    return (string) null;
                  this.current_processing_id.Value = call.callID;
                }
                CommandResult result;
                if (call.name == "AcquireLock")
                  result = printerConnection.AcquireLock(guid);
                else if (call.name == "ReleaseLock")
                {
                  result = printerConnection.ReleaseLock(call.lockID);
                }
                else
                {
                  bool flag = true;
                  if (this.LockExceptionList.Contains(call.name))
                    flag = false;
                  result = printerConnection.VerifyLock(call.lockID);
                  if (result == CommandResult.Failed_PrinterDoesNotHaveLock && call.name == "SendEmergencyStop" && printerConnection.IsWorking)
                    printerConnection.SendInterrupted(this.LockVerifyResultToSpoolerMessage(CommandResult.CommandInterruptedByM0, call.serialnumber, 0U));
                  if (result == CommandResult.Success || !flag)
                  {
                    obj = this.rpc_invoker.CallMethod((object) printerConnection, call);
                    if (obj is CommandResult)
                    {
                      result = (CommandResult) obj;
                      obj = (object) null;
                    }
                  }
                }
                if (call.callID != 0U)
                  printerConnection.CurrentRPC_id.Value = call.callID;
                str = this.LockVerifyResultToSpoolerMessage(result, call.serialnumber, call.callID);
              }
              else
                str = new SpoolerMessage(MessageType.PrinterNotConnected, call.serialnumber, call.name).Serialize();
            }
          }
          catch (Exception ex)
          {
            str = new SpoolerMessage(MessageType.RPCError, ex.Message + "::" + call.name).Serialize();
          }
          if (str != null)
            return "<SocketBroadcast>" + str + "</SocketBroadcast>";
          if (obj != null)
          {
            if (obj is string)
              return (string) obj;
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
        return "<SocketBroadcast>" + new SpoolerMessage(MessageType.IncompatibleSpooler, (string) null).Serialize() + "</SocketBroadcast>";
      SpoolerInfo spoolerInfo = new SpoolerInfo();
      spoolerInfo.Version = M3D.Spooling.Version.Client_Version;
      foreach (KeyValuePair<string, List<FirmwareBoardVersionKVP>> embeddedFirmware in this.GetEmbeddedFirmwareList())
      {
        EmbeddedFirmwareSummary embeddedFirmwareSummary = new EmbeddedFirmwareSummary(embeddedFirmware.Key);
        foreach (FirmwareBoardVersionKVP firmwareBoardVersionKvp in embeddedFirmware.Value)
          embeddedFirmwareSummary.FirmwareVersions.Add(firmwareBoardVersionKvp);
        spoolerInfo.SupportPrinterProfiles.Add(embeddedFirmwareSummary);
      }
      spoolerInfo.PrinterProfileList = this.profile_dictionary.CreateProfileList();
      return "<SocketBroadcast>" + spoolerInfo.Serialize() + this.Serialize() + "</SocketBroadcast>";
    }

    public string UpdatePrinterList()
    {
      return "<SocketBroadcast>" + this.Serialize() + "</SocketBroadcast>";
    }

    public void ForceShutdownSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerShutdownMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerShutdownMessage((object) this, new EventArgs());
    }

    public void HideSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerHideMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerHideMessage((object) this, new EventArgs());
    }

    public void ShowSpooler()
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnReceivedSpoolerShowMessage == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OnReceivedSpoolerShowMessage((object) this, new EventArgs());
    }

    public int NumActiveAndQueuedJobs
    {
      get
      {
        int num = 0;
        foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
        {
          PrinterConnection printerConnection = printer.Value;
          if (printerConnection != null)
            num += printerConnection.GetJobsCount();
        }
        return num;
      }
    }

    public bool ArePrintersDoingWork
    {
      get
      {
        bool flag = false;
        foreach (KeyValuePair<string, PrinterConnection> printer in this.connectionManager.printers)
        {
          PrinterConnection printerConnection = printer.Value;
          if (printerConnection != null)
            flag |= printerConnection.IsWorking;
        }
        return flag;
      }
    }
  }
}
