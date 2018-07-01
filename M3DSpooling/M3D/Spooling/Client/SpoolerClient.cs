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
      this.MyDebugLogger = logger;
      this.thread_sync = new object();
      this.tick_sync = new object();
      this.new_printer_list_sync = new object();
      this.HasNewPrinterList = new ThreadSafeVariable<bool>();
      this.HasNewPrinterList.Value = false;
      this.connected_printers = new List<Printer>();
      this.connectedSpoolerInfo = new ThreadSafeVariable<SpoolerInfo>((SpoolerInfo) null);
      this.message_queue = new ConcurrentQueue<RPCInvoker.RPC>();
      this.message_thread = (Thread) null;
      this.queue_lock = new object();
      this.isprinting_lock = new object();
    }

    ~SpoolerClient()
    {
      this.CloseSession();
    }

    public List<PrinterInfo> GetPrinterInfo()
    {
      List<PrinterInfo> printerInfoList = new List<PrinterInfo>();
      lock (this.connected_printers)
      {
        foreach (Printer connectedPrinter in this.connected_printers)
          printerInfoList.Add(new PrinterInfo(connectedPrinter.Info));
      }
      return printerInfoList;
    }

    public IEnumerator<Printer> GetEnumerator()
    {
      return (IEnumerator<Printer>) this.connected_printers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    public void Lock(SpoolerClient.Del d)
    {
      lock (this.connected_printers)
        d();
    }

    public SpoolerResult StartSession(string spooler_app_and_path, string working_dir, string spooler_arguments, int start_delay)
    {
      Trace.WriteLine("SpoolerClient2.StartSession");
      this.MyDebugLogger.Add("SpoolerClient::StartSession", "Starting", DebugLogger.LogType.Secondary);
      SpoolerResult spoolerResult = SpoolerResult.Fail_Connect;
      try
      {
        RemoteSpoolerConnection spoolerConnection = new RemoteSpoolerConnection(spooler_app_and_path, working_dir, spooler_arguments);
        this.MyDebugLogger.Add("SpoolerClient::StartSession", "Remote Connection Object Created", DebugLogger.LogType.Secondary);
        spoolerConnection.StartUpDelay = start_delay;
        spoolerConnection.UseNoSpoolerMode = this.__use_no_spooler_mode;
        spoolerConnection.XMLProcessor = new OnReceiveXMLFromSpooler(this.ProcessXMLFromServer);
        spoolerConnection.StartUp(42345);
        this.MyDebugLogger.Add("SpoolerClient::StartSession", "Socket Client Initialized", DebugLogger.LogType.Secondary);
        this.spooler_connection = (ISpoolerConnection) spoolerConnection;
        spoolerResult = this.InitialConnect();
        Trace.WriteLine(string.Format("SpoolerClient2.StartSession Completed {0}", (object) spoolerResult));
        this.MyDebugLogger.Add("SpoolerClient::StartSession", "Connected to Spooler", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient2.StartSession " + ex.Message, ex);
        this.spooler_connection = (ISpoolerConnection) null;
        this.CloseSession();
      }
      if (this.spooler_connection != null)
        this.StartThreads();
      this.MyDebugLogger.Add("SpoolerClient::StartSession", "SpoolerClient threads created", DebugLogger.LogType.Secondary);
      return spoolerResult;
    }

    public void CloseSession()
    {
      this.MyDebugLogger.Add("SpoolerClient::CloseSession", "Closing Session", DebugLogger.LogType.Secondary);
      this.threadsAborted.Value = true;
      this.MyDebugLogger.Add("SpoolerClient::CloseSession", "Stopping Threads", DebugLogger.LogType.Secondary);
      this.processing_thread = (Thread) null;
      this.message_thread = (Thread) null;
      if (this.spooler_connection != null)
      {
        this.spooler_connection.ShutdownConnection();
        this.MyDebugLogger.Add("SpoolerClient::CloseSession", "Connection Shutdown", DebugLogger.LogType.Secondary);
      }
      if (this.connected_printers != null)
        this.connected_printers.Clear();
      if (this.new_connected_printers != null)
        this.new_connected_printers.Clear();
      this.spooler_connection = (ISpoolerConnection) null;
      this.MyDebugLogger.Add("SpoolerClient::CloseSession", "Data Cleared", DebugLogger.LogType.Secondary);
    }

    public SpoolerResult InitialConnect()
    {
      return this.SendSpoolerMessageRPC(nameof (InitialConnect), (object) M3D.Spooling.Version.Client_Version);
    }

    public SpoolerResult UpdatePrinterList()
    {
      bool flag = true;
      if (!this.AlwaysBlock)
      {
        lock (this.queue_lock)
        {
          flag = !this.update_in_queue || this.message_queue.Count == 0;
          this.update_in_queue = true;
        }
      }
      if (flag)
        return this.SendSpoolerMessageRPCAsync(nameof (UpdatePrinterList), new object[0]);
      return SpoolerResult.OK;
    }

    public SpoolerResult ForceSpoolerShutdown()
    {
      return this.SendSpoolerMessageRPCAsync("ForceShutdownSpooler", new object[0]);
    }

    public SpoolerResult HideSpooler()
    {
      return this.SendSpoolerMessageRPCAsync(nameof (HideSpooler), new object[0]);
    }

    public SpoolerResult ShowSpooler()
    {
      return this.SendSpoolerMessageRPCAsync(nameof (ShowSpooler), new object[0]);
    }

    public List<PrinterSerialNumber> GetSerialNumbers()
    {
      List<PrinterSerialNumber> printerSerialNumberList = new List<PrinterSerialNumber>();
      lock (this.connected_printers)
      {
        foreach (Printer connectedPrinter in this.connected_printers)
          printerSerialNumberList.Add(connectedPrinter.Info.serial_number);
      }
      return printerSerialNumberList;
    }

    public Printer GetPrinter(int index)
    {
      Printer printer = (Printer) null;
      lock (this.connected_printers)
      {
        if (index >= 0)
        {
          if (index < this.connected_printers.Count)
            printer = this.connected_printers[index];
        }
      }
      return printer;
    }

    public Printer GetPrinter(PrinterSerialNumber sn)
    {
      Printer printer = (Printer) null;
      lock (this.connected_printers)
      {
        for (int index = 0; index < this.connected_printers.Count; ++index)
        {
          if (this.connected_printers[index].Info.serial_number == sn)
          {
            printer = this.connected_printers[index];
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
        if (this.connected_printers.Count > 0)
          return this.connected_printers[0].Info.serial_number.ToString();
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SppolerClient.GetDefaultPrinter " + ex.Message, ex);
      }
      return "";
    }

    protected void StartThreads()
    {
      if (this.spooler_connection == null)
        return;
      this.message_thread = new Thread(new ThreadStart(this.MessageThread));
      this.message_thread.Priority = ThreadPriority.Normal;
      this.message_thread.IsBackground = true;
      this.message_thread.Name = "Message";
      this.message_thread.Start();
      this.processing_thread = new Thread(new ThreadStart(this.ProcessingThread));
      this.processing_thread.Name = "Processing";
      this.processing_thread.IsBackground = true;
      this.processing_thread.Start();
    }

    private int GetPrinterIndexFromSerialNumber(PrinterSerialNumber sn)
    {
      lock (this.connected_printers)
      {
        try
        {
          for (int index = 0; index < this.connected_printers.Count; ++index)
          {
            if (this.connected_printers[index].Info.serial_number == sn)
              return index;
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
      return this.GetPrinterIndexFromSerialNumber(new PrinterSerialNumber(printer_serial_number)) >= 0;
    }

    public bool IsPrinting
    {
      get
      {
        lock (this.isprinting_lock)
          return this._is_printing;
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
        if (this.spooler_connection != null)
          return this.spooler_connection.UseNoSpoolerMode;
        return this.__use_no_spooler_mode;
      }
      set
      {
        if (this.spooler_connection != null)
          this.spooler_connection.UseNoSpoolerMode = value;
        else
          this.__use_no_spooler_mode = value;
      }
    }

    public int PrinterDriverInstallCount
    {
      get
      {
        return this._printerDriverInstallCount.Value;
      }
    }

    public int PrinterCount
    {
      get
      {
        lock (this.connected_printers)
          return this.connected_printers.Count;
      }
    }

    public int CleanUpDisconnectedPrinterTime
    {
      set
      {
        lock (this.thread_sync)
          this._cleanupPrinterTime = value;
      }
      get
      {
        lock (this.thread_sync)
          return this._cleanupPrinterTime;
      }
    }

    public int UpdateTick
    {
      get
      {
        lock (this.tick_sync)
          return this._updateTick;
      }
      set
      {
        lock (this.tick_sync)
          this._updateTick = value;
      }
    }

    public bool IgnoreConnectingPrinters
    {
      get
      {
        return this._ignoreConnectingPrinters.Value;
      }
      set
      {
        this._ignoreConnectingPrinters.Value = value;
      }
    }

    private void ProcessingThread()
    {
      try
      {
        while (!this.threadsAborted.Value)
        {
          try
          {
            int num = (int) this.UpdatePrinterList();
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in ProcessingThread.UpdatePrinterList " + ex.Message, ex);
          }
          Thread.Sleep(this.UpdateTick);
          try
          {
            this.ProcessNewPrinterList();
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in ProcessingThread.ProcessNewPrinter " + ex.Message, ex);
          }
          try
          {
            this.CleanUpDisconnectedPrinters();
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
      List<Printer> printerList = new List<Printer>();
      lock (this.connected_printers)
      {
        try
        {
          int index = 0;
          while (index < this.connected_printers.Count)
          {
            if (!this.connected_printers[index].Connected)
            {
              this.connected_printers[index].time_since_found += this.UpdateTick;
              if (this.CleanUpDisconnectedPrinterTime > 0 && this.connected_printers[index].time_since_found >= this._cleanupPrinterTime)
              {
                printerList.Add(this.connected_printers[index]);
                this.connected_printers.RemoveAt(index);
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
      if (this.OnPrinterDisconnected != null)
      {
        foreach (Printer new_printer in printerList)
          this.OnPrinterDisconnected(new_printer);
      }
      printerList.Clear();
    }

    private void InternalOnReceivedMessage(SpoolerMessage message)
    {
      Printer printer = (Printer) null;
      if (this.connected_printers != null)
      {
        lock (this.connected_printers)
        {
          try
          {
            foreach (Printer connectedPrinter in this.connected_printers)
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
      if (this.OnReceivedMessage == null)
        return;
      this.OnReceivedMessage(message);
    }

    private void ProcessDeviceInstallDetected(string xmlmessage)
    {
      XmlReader xmlReader = XmlReader.Create((TextReader) new StringReader(xmlmessage));
      if (!xmlReader.Read() || !xmlReader.HasAttributes)
        return;
      int result = 0;
      if (!int.TryParse(xmlReader.GetAttribute("count"), out result))
        return;
      this._printerDriverInstallCount.Value = result;
    }

    private void ProcessNewPrinterList()
    {
      List<PrinterInfo> connected_printers = (List<PrinterInfo>) null;
      bool flag = false;
      lock (this.new_printer_list_sync)
      {
        if (this.HasNewPrinterList.Value)
        {
          connected_printers = new List<PrinterInfo>((IEnumerable<PrinterInfo>) this.new_connected_printers);
          this.HasNewPrinterList.Value = false;
          flag = true;
        }
      }
      if (!flag)
        return;
      lock (this.connected_printers)
      {
        try
        {
          if (this.connected_printers != null)
          {
            foreach (Printer connectedPrinter in this.connected_printers)
              connectedPrinter.Found.Value = false;
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
        if (info != null && !(info.serial_number == PrinterSerialNumber.Undefined) && (!this.IgnoreConnectingPrinters || info.Status != PrinterStatus.Connecting))
        {
          Printer printer = (Printer) null;
          lock (this.connected_printers)
          {
            foreach (Printer connectedPrinter in this.connected_printers)
            {
              if (info.serial_number == connectedPrinter.printer_info.serial_number)
                printer = connectedPrinter;
            }
          }
          if (printer != null)
          {
            printer.UpdateData(info);
            if (printer.time_since_found != 0)
              printer.switching_modes = false;
            else if (printer.Info.Status == PrinterStatus.Bootloader_UpdatingFirmware)
              printer.switching_modes = true;
            printer.time_since_found = 0;
            printer.Found.Value = true;
            printer._connected.Value = true;
          }
          else
          {
            Printer new_printer = new Printer(info, this.GetPrinterProfile(info.ProfileName), this);
            new_printer.Found.Value = true;
            new_printer._connected.Value = true;
            lock (this.connected_printers)
              this.connected_printers.Add(new_printer);
            if (this.OnGotNewPrinter != null)
              this.OnGotNewPrinter(new_printer);
          }
        }
      }
      int index = 0;
      List<Printer> printerList = new List<Printer>();
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
      if (this.OnPrinterDisconnected != null && printerList.Count > 0)
      {
        foreach (Printer new_printer in printerList)
          this.OnPrinterDisconnected(new_printer);
      }
      printerList.Clear();
      if (this.OnReceivedPrinterList == null)
        return;
      this.OnReceivedPrinterList(connected_printers);
    }

    protected internal void ProcessXMLFromServer(string message)
    {
      List<PrinterInfo> printerInfoList = new List<PrinterInfo>();
      bool flag1 = false;
      bool flag2 = false;
      try
      {
        if (message != "OK")
        {
          XmlDocument xmlDocument = new XmlDocument();
          xmlDocument.LoadXml(message);
          foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("SocketBroadcast"))
          {
            if (xmlNode is XmlElement)
            {
              XmlElement xmlElement = (XmlElement) xmlNode;
              StringWriter stringWriter = new StringWriter();
              xmlElement.WriteTo((XmlWriter) new XmlTextWriter((TextWriter) stringWriter)
              {
                Formatting = Formatting.Indented
              });
              string str = stringWriter.ToString();
              if (xmlElement.Name == "PrinterInfo")
              {
                PrinterInfo printerInfo = new PrinterInfo(str);
                printerInfoList.Add(printerInfo);
                if (printerInfo.current_job != null)
                {
                  flag2 = true;
                  this.serial_number = printerInfo.MySerialNumber;
                }
                if (this.OnProcessFromServer != null)
                  this.OnProcessFromServer();
              }
              else if (xmlElement.Name == "SpoolerInfo")
                this.connectedSpoolerInfo.Value = new SpoolerInfo(str);
              else if (xmlElement.Name == "SpoolerMessage")
                this.InternalOnReceivedMessage(new SpoolerMessage(str));
              else if (xmlElement.Name == "SPOOLER__ALL")
                flag1 = true;
              else if (xmlElement.Name == "DeviceInstallDetected")
                this.ProcessDeviceInstallDetected(str);
            }
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerClient.ProcessFormServer " + ex.Message, ex);
        int num = (int) this.ForceSpoolerShutdown();
      }
      if (!flag1)
        return;
      if (!flag2 && this._is_printing && this.OnPrintStopped != null)
        this.OnPrintStopped(this.serial_number);
      lock (this.isprinting_lock)
        this._is_printing = flag2;
      lock (this.new_printer_list_sync)
      {
        this.HasNewPrinterList.Value = true;
        this.new_connected_printers = new List<PrinterInfo>((IEnumerable<PrinterInfo>) printerInfoList);
      }
    }

    public PrinterProfile GetPrinterProfile(string name)
    {
      IEnumerable<PrinterProfile> source = this.ConnectedSpoolerInfo.PrinterProfileList.Where<PrinterProfile>((Func<PrinterProfile, bool>) (p => p.ProfileName == name));
      if (source.Count<PrinterProfile>() > 0)
        return new PrinterProfile(source.First<PrinterProfile>());
      return (PrinterProfile) null;
    }

    public SpoolerResult SendSpoolerMessageRPC(string function_name, params object[] options)
    {
      return this.SendSpoolerMessageRPC(new RPCInvoker.RPC(function_name, options));
    }

    public SpoolerResult SendSpoolerMessageRPC(RPCInvoker.RPC rpc_call)
    {
      return this.SendSpoolerMessage(rpc_call.Serialize());
    }

    public SpoolerResult SendSpoolerMessageRPCAsync(RPCInvoker.RPC rpc_call)
    {
      if (this.AlwaysBlock)
        return this.SendSpoolerMessageRPC(rpc_call);
      if (this.MessagesInQueue > 128)
        return SpoolerResult.Fail;
      lock (this.queue_lock)
        this.message_queue.Enqueue(rpc_call);
      return SpoolerResult.OK;
    }

    public SpoolerResult SendSpoolerMessageRPCAsync(string function_name, params object[] options)
    {
      return this.SendSpoolerMessageRPCAsync(new RPCInvoker.RPC(function_name, options));
    }

    private void MessageThread()
    {
      try
      {
        while (!this.threadsAborted.Value)
        {
          RPCInvoker.RPC result = new RPCInvoker.RPC();
          bool flag = false;
          lock (this.queue_lock)
          {
            try
            {
              if (this.message_queue.Count > 0)
                flag = this.message_queue.TryDequeue(out result);
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
              lock (this.queue_lock)
              {
                try
                {
                  this.update_in_queue = false;
                }
                catch (Exception ex)
                {
                  ErrorLogger.LogException("Exception in SpoolerClient.MessageThread 1 " + ex.Message, ex);
                }
              }
            }
            int num = (int) this.spooler_connection.SendSpoolerMessageInternal(result.Serialize());
          }
          else
            Thread.Sleep(50);
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
        lock (this.queue_lock)
          return this.message_queue.Count;
      }
    }

    private SpoolerResult SendSpoolerMessage(string message)
    {
      if (this.spooler_connection != null)
        return this.spooler_connection.SendSpoolerMessageInternal(message);
      return SpoolerResult.Fail;
    }

    public DebugLogger MyDebugLogger { get; private set; }

    public SpoolerInfo ConnectedSpoolerInfo
    {
      get
      {
        return this.connectedSpoolerInfo.Value;
      }
    }

    public delegate void OnGotNewPrinterDel(Printer new_printer);

    public delegate void OnPrinterDisconnectedDel(Printer new_printer);

    public delegate void OnPrintProcessDel();

    public delegate void OnPrintStoppedDel(string serial_number);

    public delegate void Del();
  }
}
