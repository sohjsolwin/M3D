// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.SpoolerClient
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace M3D.Spooling.Client
{
  public class SpoolerClient : IEnumerable<Printer>, IEnumerable
  {
    private ThreadSafeVariable<int> _printerDriverInstallCount = new ThreadSafeVariable<int>();
    private int _cleanupPrinterTime = -1;
    private int _updateTick = 100;
    private ThreadSafeVariable<bool> _ignoreConnectingPrinters = new ThreadSafeVariable<bool>(true);
    private ThreadSafeVariable<bool> threadsAborted = new ThreadSafeVariable<bool>(false);
    public bool AlwaysBlock;
    public OnReceivedPrinterListDel OnReceivedPrinterList;
    public OnReceivedMessageDel OnReceivedMessage;
    public SpoolerClient.OnGotNewPrinterDel OnGotNewPrinter;
    public SpoolerClient.OnPrinterDisconnectedDel OnPrinterDisconnected;
    public SpoolerClient.OnPrintProcessDel OnProcessFromServer;
    public SpoolerClient.OnPrintStoppedDel OnPrintStopped;
    protected ISpoolerConnection spooler_connection;
    private Thread processing_thread;
    private object thread_sync;
    private object tick_sync;
    private object new_printer_list_sync;
    private ThreadSafeVariable<bool> HasNewPrinterList;
    private bool __use_no_spooler_mode;
    private List<Printer> connected_printers;
    private ConcurrentQueue<RPCInvoker.RPC> message_queue;
    private object queue_lock;
    private object isprinting_lock;
    private Thread message_thread;
    private bool update_in_queue;
    private List<PrinterInfo> new_connected_printers;
    private bool _is_printing;
    private string serial_number;
    private ThreadSafeVariable<SpoolerInfo> connectedSpoolerInfo;

    public SpoolerClient(DebugLogger logger)
    {
      MyDebugLogger = logger;
      thread_sync = new object();
      tick_sync = new object();
      new_printer_list_sync = new object();
      HasNewPrinterList = new ThreadSafeVariable<bool>
      {
        Value = false
      };
      connected_printers = new List<Printer>();
      connectedSpoolerInfo = new ThreadSafeVariable<SpoolerInfo>((SpoolerInfo) null);
      message_queue = new ConcurrentQueue<RPCInvoker.RPC>();
      message_thread = (Thread) null;
      queue_lock = new object();
      isprinting_lock = new object();
    }

    ~SpoolerClient()
    {
      CloseSession();
    }

    public List<PrinterInfo> GetPrinterInfo()
    {
      var printerInfoList = new List<PrinterInfo>();
      lock (connected_printers)
      {
        foreach (Printer connectedPrinter in connected_printers)
        {
          printerInfoList.Add(new PrinterInfo(connectedPrinter.Info));
        }
      }
      return printerInfoList;
    }

    public IEnumerator<Printer> GetEnumerator()
    {
      return (IEnumerator<Printer>)connected_printers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }

    public void Lock(SpoolerClient.Del d)
    {
      lock (connected_printers)
      {
        d();
      }
    }

    public SpoolerResult StartSession(string spooler_app_and_path, string working_dir, string spooler_arguments, int start_delay)
    {
      Trace.WriteLine("SpoolerClient2.StartSession");
      MyDebugLogger.Add("SpoolerClient::StartSession", "Starting", DebugLogger.LogType.Secondary);
      SpoolerResult spoolerResult = SpoolerResult.Fail_Connect;
      try
      {
        var spoolerConnection = new RemoteSpoolerConnection(spooler_app_and_path, working_dir, spooler_arguments);
        MyDebugLogger.Add("SpoolerClient::StartSession", "Remote Connection Object Created", DebugLogger.LogType.Secondary);
        spoolerConnection.StartUpDelay = start_delay;
        spoolerConnection.UseNoSpoolerMode = __use_no_spooler_mode;
        spoolerConnection.XMLProcessor = new OnReceiveXMLFromSpooler(ProcessXMLFromServer);
        spoolerConnection.StartUp(42345);
        MyDebugLogger.Add("SpoolerClient::StartSession", "Socket Client Initialized", DebugLogger.LogType.Secondary);
        spooler_connection = (ISpoolerConnection) spoolerConnection;
        spoolerResult = InitialConnect();
        Trace.WriteLine(string.Format("SpoolerClient2.StartSession Completed {0}", (object) spoolerResult));
        MyDebugLogger.Add("SpoolerClient::StartSession", "Connected to Spooler", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient2.StartSession " + ex.Message, ex);
        spooler_connection = (ISpoolerConnection) null;
        CloseSession();
      }
      if (spooler_connection != null)
      {
        StartThreads();
      }

      MyDebugLogger.Add("SpoolerClient::StartSession", "SpoolerClient threads created", DebugLogger.LogType.Secondary);
      return spoolerResult;
    }

    public void CloseSession()
    {
      MyDebugLogger.Add("SpoolerClient::CloseSession", "Closing Session", DebugLogger.LogType.Secondary);
      threadsAborted.Value = true;
      MyDebugLogger.Add("SpoolerClient::CloseSession", "Stopping Threads", DebugLogger.LogType.Secondary);
      processing_thread = (Thread) null;
      message_thread = (Thread) null;
      if (spooler_connection != null)
      {
        spooler_connection.ShutdownConnection();
        MyDebugLogger.Add("SpoolerClient::CloseSession", "Connection Shutdown", DebugLogger.LogType.Secondary);
      }
      if (connected_printers != null)
      {
        connected_printers.Clear();
      }

      if (new_connected_printers != null)
      {
        new_connected_printers.Clear();
      }

      spooler_connection = (ISpoolerConnection) null;
      MyDebugLogger.Add("SpoolerClient::CloseSession", "Data Cleared", DebugLogger.LogType.Secondary);
    }

    public SpoolerResult InitialConnect()
    {
      return SendSpoolerMessageRPC(nameof (InitialConnect), (object) M3D.Spooling.Version.Client_Version);
    }

    public SpoolerResult UpdatePrinterList()
    {
      var flag = true;
      if (!AlwaysBlock)
      {
        lock (queue_lock)
        {
          flag = !update_in_queue || message_queue.Count == 0;
          update_in_queue = true;
        }
      }
      if (flag)
      {
        return SendSpoolerMessageRPCAsync(nameof (UpdatePrinterList), new object[0]);
      }

      return SpoolerResult.OK;
    }

    public SpoolerResult ForceSpoolerShutdown()
    {
      return SendSpoolerMessageRPCAsync("ForceShutdownSpooler", new object[0]);
    }

    public SpoolerResult HideSpooler()
    {
      return SendSpoolerMessageRPCAsync(nameof (HideSpooler), new object[0]);
    }

    public SpoolerResult ShowSpooler()
    {
      return SendSpoolerMessageRPCAsync(nameof (ShowSpooler), new object[0]);
    }

    public List<PrinterSerialNumber> GetSerialNumbers()
    {
      var printerSerialNumberList = new List<PrinterSerialNumber>();
      lock (connected_printers)
      {
        foreach (Printer connectedPrinter in connected_printers)
        {
          printerSerialNumberList.Add(connectedPrinter.Info.serial_number);
        }
      }
      return printerSerialNumberList;
    }

    public Printer GetPrinter(int index)
    {
      var printer = (Printer) null;
      lock (connected_printers)
      {
        if (index >= 0)
        {
          if (index < connected_printers.Count)
          {
            printer = connected_printers[index];
          }
        }
      }
      return printer;
    }

    public Printer GetPrinter(PrinterSerialNumber sn)
    {
      var printer = (Printer) null;
      lock (connected_printers)
      {
        for (var index = 0; index < connected_printers.Count; ++index)
        {
          if (connected_printers[index].Info.serial_number == sn)
          {
            printer = connected_printers[index];
            break;
          }
        }
      }
      return printer;
    }

    public string GetDefaultPrinter()
    {
      try
      {
        if (connected_printers.Count > 0)
        {
          return connected_printers[0].Info.serial_number.ToString();
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SppolerClient.GetDefaultPrinter " + ex.Message, ex);
      }
      return "";
    }

    protected void StartThreads()
    {
      if (spooler_connection == null)
      {
        return;
      }

      message_thread = new Thread(new ThreadStart(MessageThread))
      {
        Priority = ThreadPriority.Normal,
        IsBackground = true,
        Name = "Message"
      };
      message_thread.Start();
      processing_thread = new Thread(new ThreadStart(ProcessingThread))
      {
        Name = "Processing",
        IsBackground = true
      };
      processing_thread.Start();
    }

    private int GetPrinterIndexFromSerialNumber(PrinterSerialNumber sn)
    {
      lock (connected_printers)
      {
        try
        {
          for (var index = 0; index < connected_printers.Count; ++index)
          {
            if (connected_printers[index].Info.serial_number == sn)
            {
              return index;
            }
          }
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in SpoolerClient.GetPrinterIndexFromSerialNumber " + ex.Message, ex);
        }
      }
      return -1;
    }

    public bool IsPrinterConnected(string printer_serial_number)
    {
      return GetPrinterIndexFromSerialNumber(new PrinterSerialNumber(printer_serial_number)) >= 0;
    }

    public bool IsPrinting
    {
      get
      {
        lock (isprinting_lock)
        {
          return _is_printing;
        }
      }
    }

    public bool IsDebugVersion
    {
      get
      {
        return false;
      }
    }

    public bool UseNoSpoolerMode
    {
      get
      {
        if (spooler_connection != null)
        {
          return spooler_connection.UseNoSpoolerMode;
        }

        return __use_no_spooler_mode;
      }
      set
      {
        if (spooler_connection != null)
        {
          spooler_connection.UseNoSpoolerMode = value;
        }
        else
        {
          __use_no_spooler_mode = value;
        }
      }
    }

    public int PrinterDriverInstallCount
    {
      get
      {
        return _printerDriverInstallCount.Value;
      }
    }

    public int PrinterCount
    {
      get
      {
        lock (connected_printers)
        {
          return connected_printers.Count;
        }
      }
    }

    public int CleanUpDisconnectedPrinterTime
    {
      set
      {
        lock (thread_sync)
        {
          _cleanupPrinterTime = value;
        }
      }
      get
      {
        lock (thread_sync)
        {
          return _cleanupPrinterTime;
        }
      }
    }

    public int UpdateTick
    {
      get
      {
        lock (tick_sync)
        {
          return _updateTick;
        }
      }
      set
      {
        lock (tick_sync)
        {
          _updateTick = value;
        }
      }
    }

    public bool IgnoreConnectingPrinters
    {
      get
      {
        return _ignoreConnectingPrinters.Value;
      }
      set
      {
        _ignoreConnectingPrinters.Value = value;
      }
    }

    private void ProcessingThread()
    {
      try
      {
        while (!threadsAborted.Value)
        {
          try
          {
            var num = (int)UpdatePrinterList();
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in ProcessingThread.UpdatePrinterList " + ex.Message, ex);
          }
          Thread.Sleep(UpdateTick);
          try
          {
            ProcessNewPrinterList();
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in ProcessingThread.ProcessNewPrinter " + ex.Message, ex);
          }
          try
          {
            CleanUpDisconnectedPrinters();
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in ProcessingThread.CleanedUpPrinter " + ex.Message, ex);
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient2.ProcessingThread " + ex.Message, ex);
      }
    }

    private void CleanUpDisconnectedPrinters()
    {
      var printerList = new List<Printer>();
      lock (connected_printers)
      {
        try
        {
          var index = 0;
          while (index < connected_printers.Count)
          {
            if (!connected_printers[index].Connected)
            {
              connected_printers[index].time_since_found += UpdateTick;
              if (CleanUpDisconnectedPrinterTime > 0 && connected_printers[index].time_since_found >= _cleanupPrinterTime)
              {
                printerList.Add(connected_printers[index]);
                connected_printers.RemoveAt(index);
                continue;
              }
            }
            ++index;
          }
        }
        catch (InvalidOperationException ex)
        {
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in SpoolerClient2.CleanUpDisconnectedPrinters " + ex.Message, ex);
        }
      }
      if (OnPrinterDisconnected != null)
      {
        foreach (Printer new_printer in printerList)
        {
          OnPrinterDisconnected(new_printer);
        }
      }
      printerList.Clear();
    }

    private void InternalOnReceivedMessage(SpoolerMessage message)
    {
      var printer = (Printer) null;
      if (connected_printers != null)
      {
        lock (connected_printers)
        {
          try
          {
            foreach (Printer connectedPrinter in connected_printers)
            {
              if (connectedPrinter.printer_info.serial_number == message.SerialNumber || (message.Type == MessageType.FullLoggingData || message.Type == MessageType.LoggingMessage) && message.SerialNumber.ToString() == "00-00-00-00-00-000-000")
              {
                printer = connectedPrinter;
                break;
              }
            }
          }
          catch (InvalidOperationException ex)
          {
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in SpoolerClient2.InternalOnReceivedMessage " + ex.Message, ex);
          }
        }
      }
      printer?.ProcessSpoolerMessage(message);
      if (OnReceivedMessage == null)
      {
        return;
      }

      OnReceivedMessage(message);
    }

    private void ProcessDeviceInstallDetected(string xmlmessage)
    {
      var xmlReader = XmlReader.Create((TextReader) new StringReader(xmlmessage));
      if (!xmlReader.Read() || !xmlReader.HasAttributes)
      {
        return;
      }

      if (!int.TryParse(xmlReader.GetAttribute("count"), out var result))
      {
        return;
      }

      _printerDriverInstallCount.Value = result;
    }

    private void ProcessNewPrinterList()
    {
      var connected_printers = (List<PrinterInfo>) null;
      var flag = false;
      lock (new_printer_list_sync)
      {
        if (HasNewPrinterList.Value)
        {
          connected_printers = new List<PrinterInfo>((IEnumerable<PrinterInfo>)new_connected_printers);
          HasNewPrinterList.Value = false;
          flag = true;
        }
      }
      if (!flag)
      {
        return;
      }

      lock (this.connected_printers)
      {
        try
        {
          if (this.connected_printers != null)
          {
            foreach (Printer connectedPrinter in this.connected_printers)
            {
              connectedPrinter.Found.Value = false;
            }
          }
        }
        catch (InvalidOperationException ex)
        {
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in SpoolerClient2.ProcessNePrinterList " + ex.Message, ex);
        }
      }
      foreach (PrinterInfo info in connected_printers)
      {
        if (info != null && !(info.serial_number == PrinterSerialNumber.Undefined) && (!IgnoreConnectingPrinters || info.Status != PrinterStatus.Connecting))
        {
          var printer = (Printer) null;
          lock (this.connected_printers)
          {
            foreach (Printer connectedPrinter in this.connected_printers)
            {
              if (info.serial_number == connectedPrinter.printer_info.serial_number)
              {
                printer = connectedPrinter;
              }
            }
          }
          if (printer != null)
          {
            printer.UpdateData(info);
            if (printer.time_since_found != 0)
            {
              printer.switching_modes = false;
            }
            else if (printer.Info.Status == PrinterStatus.Bootloader_UpdatingFirmware)
            {
              printer.switching_modes = true;
            }

            printer.time_since_found = 0;
            printer.Found.Value = true;
            printer._connected.Value = true;
          }
          else
          {
            var new_printer = new Printer(info, GetPrinterProfile(info.ProfileName), this);
            new_printer.Found.Value = true;
            new_printer._connected.Value = true;
            lock (this.connected_printers)
            {
              this.connected_printers.Add(new_printer);
            }

            if (OnGotNewPrinter != null)
            {
              OnGotNewPrinter(new_printer);
            }
          }
        }
      }
      var index = 0;
      var printerList = new List<Printer>();
      lock (this.connected_printers)
      {
        while (index < this.connected_printers.Count)
        {
          if (!this.connected_printers[index].Found.Value)
          {
            this.connected_printers[index]._connected.Value = false;
            if (!this.connected_printers[index].switching_modes)
            {
              printerList.Add(this.connected_printers[index]);
              this.connected_printers.RemoveAt(index);
              continue;
            }
          }
          ++index;
        }
      }
      if (OnPrinterDisconnected != null && printerList.Count > 0)
      {
        foreach (Printer new_printer in printerList)
        {
          OnPrinterDisconnected(new_printer);
        }
      }
      printerList.Clear();
      if (OnReceivedPrinterList == null)
      {
        return;
      }

      OnReceivedPrinterList(connected_printers);
    }

    protected internal void ProcessXMLFromServer(string message)
    {
      var printerInfoList = new List<PrinterInfo>();
      var flag1 = false;
      var flag2 = false;
      try
      {
        if (message != "OK")
        {
          var xmlDocument = new XmlDocument();
          xmlDocument.LoadXml(message);
          foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("SocketBroadcast"))
          {
            if (xmlNode is XmlElement)
            {
              var xmlElement = (XmlElement) xmlNode;
              var stringWriter = new StringWriter();
              xmlElement.WriteTo((XmlWriter) new XmlTextWriter((TextWriter) stringWriter)
              {
                Formatting = Formatting.Indented
              });
              var str = stringWriter.ToString();
              if (xmlElement.Name == "PrinterInfo")
              {
                var printerInfo = new PrinterInfo(str);
                printerInfoList.Add(printerInfo);
                if (printerInfo.current_job != null)
                {
                  flag2 = true;
                  serial_number = printerInfo.MySerialNumber;
                }
                if (OnProcessFromServer != null)
                {
                  OnProcessFromServer();
                }
              }
              else if (xmlElement.Name == "SpoolerInfo")
              {
                connectedSpoolerInfo.Value = new SpoolerInfo(str);
              }
              else if (xmlElement.Name == "SpoolerMessage")
              {
                InternalOnReceivedMessage(new SpoolerMessage(str));
              }
              else if (xmlElement.Name == "SPOOLER__ALL")
              {
                flag1 = true;
              }
              else if (xmlElement.Name == "DeviceInstallDetected")
              {
                ProcessDeviceInstallDetected(str);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient.ProcessFormServer " + ex.Message, ex);
        var num = (int)ForceSpoolerShutdown();
      }
      if (!flag1)
      {
        return;
      }

      if (!flag2 && _is_printing && OnPrintStopped != null)
      {
        OnPrintStopped(serial_number);
      }

      lock (isprinting_lock)
      {
        _is_printing = flag2;
      }

      lock (new_printer_list_sync)
      {
        HasNewPrinterList.Value = true;
        new_connected_printers = new List<PrinterInfo>((IEnumerable<PrinterInfo>) printerInfoList);
      }
    }

    public PrinterProfile GetPrinterProfile(string name)
    {
      IEnumerable<PrinterProfile> source = ConnectedSpoolerInfo.PrinterProfileList.Where<PrinterProfile>((Func<PrinterProfile, bool>) (p => p.ProfileName == name));
      if (source.Count<PrinterProfile>() > 0)
      {
        return new PrinterProfile(source.First<PrinterProfile>());
      }

      return (PrinterProfile) null;
    }

    public SpoolerResult SendSpoolerMessageRPC(string function_name, params object[] options)
    {
      return SendSpoolerMessageRPC(new RPCInvoker.RPC(function_name, options));
    }

    public SpoolerResult SendSpoolerMessageRPC(RPCInvoker.RPC rpc_call)
    {
      return SendSpoolerMessage(rpc_call.Serialize());
    }

    public SpoolerResult SendSpoolerMessageRPCAsync(RPCInvoker.RPC rpc_call)
    {
      if (AlwaysBlock)
      {
        return SendSpoolerMessageRPC(rpc_call);
      }

      if (MessagesInQueue > 128)
      {
        return SpoolerResult.Fail;
      }

      lock (queue_lock)
      {
        message_queue.Enqueue(rpc_call);
      }

      return SpoolerResult.OK;
    }

    public SpoolerResult SendSpoolerMessageRPCAsync(string function_name, params object[] options)
    {
      return SendSpoolerMessageRPCAsync(new RPCInvoker.RPC(function_name, options));
    }

    private void MessageThread()
    {
      try
      {
        while (!threadsAborted.Value)
        {
          var result = new RPCInvoker.RPC();
          var flag = false;
          lock (queue_lock)
          {
            try
            {
              if (message_queue.Count > 0)
              {
                flag = message_queue.TryDequeue(out result);
              }
            }
            catch (Exception ex)
            {
              ErrorLogger.LogException("Exception in SpoolerClient.MessageThread " + ex.Message, ex);
            }
          }
          if (flag)
          {
            if (result.name.Contains("UpdatePrinterList"))
            {
              lock (queue_lock)
              {
                try
                {
                  update_in_queue = false;
                }
                catch (Exception ex)
                {
                  ErrorLogger.LogException("Exception in SpoolerClient.MessageThread 1 " + ex.Message, ex);
                }
              }
            }
            var num = (int)spooler_connection.SendSpoolerMessageInternal(result.Serialize());
          }
          else
          {
            Thread.Sleep(50);
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient.MessageThread 2 " + ex.Message, ex);
      }
    }

    private int MessagesInQueue
    {
      get
      {
        lock (queue_lock)
        {
          return message_queue.Count;
        }
      }
    }

    private SpoolerResult SendSpoolerMessage(string message)
    {
      if (spooler_connection != null)
      {
        return spooler_connection.SendSpoolerMessageInternal(message);
      }

      return SpoolerResult.Fail;
    }

    public DebugLogger MyDebugLogger { get; private set; }

    public SpoolerInfo ConnectedSpoolerInfo
    {
      get
      {
        return connectedSpoolerInfo.Value;
      }
    }

    public delegate void OnGotNewPrinterDel(Printer new_printer);

    public delegate void OnPrinterDisconnectedDel(Printer new_printer);

    public delegate void OnPrintProcessDel();

    public delegate void OnPrintStoppedDel(string serial_number);

    public delegate void Del();
  }
}
