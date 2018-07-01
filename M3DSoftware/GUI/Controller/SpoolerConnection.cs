// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.SpoolerConnection
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.TextLocalization;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.GUI.Views;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.GUI.Controller
{
  public class SpoolerConnection
  {
    public SpoolerClient.OnGotNewPrinterDel OnGotNewPrinter;
    public SpoolerClient.OnPrinterDisconnectedDel OnPrinterDisconnected;
    public SpoolerConnection.SelectedPrinterChangedCallback OnSelectedPrinterChanged;
    public SpoolerClient.OnPrintProcessDel OnPrintProcessChanged;
    private const int MAX_PRINTERS = 12;
    private object thread_sync;
    private SpoolerClient print_spooler_client;
    private int selected_printer_index;
    public SpoolerConnection.PrinterMessageCallback OnPrinterMessage;
    private PopupMessageBox messagebox;
    private MessagePopUp informationbox;
    private SettingsManager settingsManager;
    private List<PrinterObject> connected_printers;
    private List<string> printerIDList;
    public ThreadSafeVariable<int> PrinterCount;
    private ThreadSafeVariable<PrinterObject> selected_printer;
    private CircularArray<string> general_log;
    private bool log_updated;

    public SpoolerConnection(PopupMessageBox messagebox, MessagePopUp informationbox, SettingsManager settingsManager)
    {
      this.messagebox = messagebox;
      this.informationbox = informationbox;
      this.settingsManager = settingsManager;
      this.thread_sync = new object();
      this.connected_printers = new List<PrinterObject>();
      this.PrinterCount = new ThreadSafeVariable<int>();
      this.PrinterCount.Value = 0;
      this.printerIDList = new List<string>();
      this.selected_printer = new ThreadSafeVariable<PrinterObject>();
      this.selected_printer.Value = (PrinterObject) null;
      this.general_log = new CircularArray<string>(200);
    }

    public bool CopyPrinterList(ref List<PrinterInfo> printer_list)
    {
      bool flag = true;
      lock (this.thread_sync)
      {
        if (printer_list.Count != this.PrintSpoolerClient.PrinterCount)
        {
          flag = false;
        }
        else
        {
          try
          {
            for (int index = 0; index < this.PrintSpoolerClient.PrinterCount & flag; ++index)
            {
              PrinterInfo info = this.PrintSpoolerClient.GetPrinter(index).Info;
              if (printer_list[index].hardware.com_port != info.hardware.com_port)
                flag = false;
              else if (printer_list[index].hardware.machine_type != info.hardware.machine_type)
                flag = false;
              else if (printer_list[index].serial_number != info.serial_number)
                flag = false;
              else if (printer_list[index].current_job != info.current_job)
                flag = false;
            }
          }
          catch (Exception ex)
          {
            flag = false;
          }
        }
        if (!flag)
        {
          printer_list.Clear();
          for (int index = 0; index < this.PrintSpoolerClient.PrinterCount; ++index)
            printer_list.Add(this.PrintSpoolerClient.GetPrinter(index).Info);
          this.selected_printer_index = 0;
        }
        else if (this.selected_printer_index < 0)
          this.selected_printer_index = 0;
      }
      return flag;
    }

    public void PrintModel(string sn, JobParams jobParams)
    {
      IPrinter printerBySerialNumber = (IPrinter) this.GetPrinterBySerialNumber(sn);
      if (printerBySerialNumber == null || !printerBySerialNumber.isConnected())
        return;
      int num = (int) printerBySerialNumber.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
    }

    public void SpoolerStartUp(DebugLogger debugLogger)
    {
      try
      {
        this.print_spooler_client = new SpoolerClient(debugLogger);
        this.print_spooler_client.IgnoreConnectingPrinters = false;
        this.print_spooler_client.OnReceivedPrinterList += new OnReceivedPrinterListDel(this.OnReceivedPrinterList);
        this.print_spooler_client.OnReceivedMessage += new OnReceivedMessageDel(this.OnReceivedMessage);
        this.print_spooler_client.OnGotNewPrinter += new SpoolerClient.OnGotNewPrinterDel(this.OnGotNewPrinterInternal);
        this.print_spooler_client.OnPrinterDisconnected += new SpoolerClient.OnPrinterDisconnectedDel(this.OnPrinterDisconnectedInternal);
        this.print_spooler_client.OnProcessFromServer += new SpoolerClient.OnPrintProcessDel(this.OnPrintProcessChangedInternal);
        this.print_spooler_client.OnPrintStopped += new SpoolerClient.OnPrintStoppedDel(this.OnPrintStoppedInternal);
        char directorySeparatorChar = Path.DirectorySeparatorChar;
        Form1.debugLogger.Add("SpoolerConnection.SpoolerStartUp", "Setup", DebugLogger.LogType.Secondary);
        int num = (int) this.print_spooler_client.StartSession(Paths.ResourceFolder + directorySeparatorChar.ToString() + "Spooler" + directorySeparatorChar.ToString() + "M3DSpooler.exe", Paths.ResourceFolder + directorySeparatorChar.ToString() + "Spooler", "H B A D PSM", 2000);
        Form1.debugLogger.Add("SpoolerConnection.SpoolerStartUp", "Session Started", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public void SendMessageToConnectedPrinter(PrinterSerialNumber sn, SpoolerMessage message)
    {
      PrinterObject printerObject = (PrinterObject) null;
      lock (this.connected_printers)
      {
        foreach (PrinterObject connectedPrinter in this.connected_printers)
        {
          if (connectedPrinter.Info.serial_number == sn)
          {
            printerObject = connectedPrinter;
            break;
          }
        }
      }
      printerObject?.ProcessSpoolerMessage(message);
    }

    private void OnGotNewPrinterInternal(Printer new_printer)
    {
      lock (this.connected_printers)
        this.connected_printers.Add(new PrinterObject(new_printer, this.messagebox, this.informationbox, this, this.settingsManager));
      if (this.OnGotNewPrinter == null)
        return;
      this.OnGotNewPrinter(new_printer);
    }

    private void OnPrinterDisconnectedInternal(Printer the_printer)
    {
      lock (this.connected_printers)
      {
        try
        {
          for (int index = 0; index < this.connected_printers.Count; ++index)
          {
            if (this.connected_printers[index].Info.hardware.com_port == the_printer.Info.hardware.com_port)
            {
              this.connected_printers.RemoveAt(index);
              break;
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      if (this.OnPrinterDisconnected == null)
        return;
      this.OnPrinterDisconnected(the_printer);
    }

    private void OnPrintProcessChangedInternal()
    {
      if (this.OnPrintProcessChanged == null)
        return;
      this.OnPrintProcessChanged();
    }

    public void OnShutdown()
    {
      if (this.print_spooler_client == null)
        return;
      this.print_spooler_client.CloseSession();
    }

    private void OnReceivedPrinterList(List<PrinterInfo> connected_printers)
    {
      this.PrinterCount.Value = connected_printers.Count;
      lock (this.printerIDList)
      {
        this.printerIDList.Clear();
        foreach (PrinterInfo connectedPrinter in connected_printers)
          this.printerIDList.Add(connectedPrinter.serial_number.ToString());
      }
      lock (this.connected_printers)
      {
        foreach (PrinterObject connectedPrinter in this.connected_printers)
          connectedPrinter.ProcessState();
      }
    }

    public List<string> GetPrinterIDList()
    {
      lock (this.printerIDList)
        return new List<string>((IEnumerable<string>) this.printerIDList);
    }

    public void CheckUpdatedFilamentProfile(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions new_data)
    {
      try
      {
        if (key.type == FilamentSpool.TypeEnum.NoFilament)
          return;
        foreach (Printer printer in this.PrintSpoolerClient)
        {
          FilamentSpool currentFilament = printer.GetCurrentFilament();
          if (currentFilament != (FilamentSpool) null && currentFilament.filament_type != FilamentSpool.TypeEnum.NoFilament && (currentFilament.filament_type == key.type && (FilamentConstants.ColorsEnum) currentFilament.filament_color_code == key.color))
          {
            currentFilament.filament_temperature = new_data.temperature;
            int num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.OnFilamentUpdateLock), (object) new SpoolerConnection.UpdateFilamentData((IPrinter) printer, currentFilament));
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void OnFilamentUpdateLock(IAsyncCallResult ar)
    {
      SpoolerConnection.UpdateFilamentData asyncState = ar.AsyncState as SpoolerConnection.UpdateFilamentData;
      if (ar.CallResult != CommandResult.Success_LockAcquired)
        return;
      int num = (int) asyncState.printer.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(this.OnFilamentUpdateDone), (object) asyncState.printer, asyncState.printer_filament);
    }

    private void OnFilamentUpdateDone(IAsyncCallResult ar)
    {
      IPrinter asyncState = ar.AsyncState as IPrinter;
      if (ar.CallResult != CommandResult.Success)
        return;
      int num = (int) asyncState.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    public bool FilamentSpoolLoaded(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions new_data)
    {
      bool flag = false;
      try
      {
        if (key.type != FilamentSpool.TypeEnum.NoFilament)
        {
          foreach (Printer printer in this.PrintSpoolerClient)
          {
            FilamentSpool currentFilament = printer.GetCurrentFilament();
            if (currentFilament != (FilamentSpool) null && currentFilament.filament_type != FilamentSpool.TypeEnum.NoFilament && (currentFilament.filament_type == key.type && (FilamentConstants.ColorsEnum) currentFilament.filament_color_code == key.color))
            {
              flag = true;
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      return flag;
    }

    private void OnReceivedMessage(SpoolerMessage message)
    {
      switch (message.Type)
      {
        case MessageType.PrinterConnected:
          this.informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message));
          break;
        case MessageType.JobComplete:
        case MessageType.JobCanceled:
        case MessageType.PrinterMessage:
        case MessageType.PrinterTimeout:
        case MessageType.SinglePointCalibrationNotSupported:
        case MessageType.MultiPointCalibrationNotSupported:
        case MessageType.PowerOutageRecovery:
          this.informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message));
          break;
        case MessageType.JobStarted:
          if (this.settingsManager.ShowAllWarnings)
            this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "A new job has started. Please do not unplug your 3D printer while it is printing. If you unplug your it while printing, you will have to recalibrate."));
          this.informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message) + string.Format(" - {0}", (object) message.Message));
          break;
        case MessageType.PrinterError:
        case MessageType.FirmwareUpdateFailed:
        case MessageType.ResetPrinterConnection:
        case MessageType.MicroMotionControllerFailed:
        case MessageType.ModelOutOfPrintableBounds:
        case MessageType.UnexpectedDisconnect:
        case MessageType.CantStartJobPrinterBusy:
        case MessageType.FirmwareErrorCyclePower:
        case MessageType.RPCError:
        case MessageType.SDPrintIncompatibleFilament:
          this.messagebox.AddMessageToQueue(message);
          break;
        case MessageType.UserDefined:
          if (this.settingsManager.ShowAllWarnings)
          {
            this.messagebox.AddMessageToQueue(message);
            break;
          }
          break;
        case MessageType.WarningABSPrintLarge:
          this.messagebox.AddMessageToQueue(message, PopupMessageBox.MessageBoxButtons.OK, new PopupMessageBox.OnUserSelectionDel(this.OnUserSelection));
          break;
        case MessageType.IncompatibleSpooler:
          if (this.PrintSpoolerClient != null)
          {
            int num = (int) this.PrintSpoolerClient.ForceSpoolerShutdown();
            break;
          }
          break;
        case MessageType.LoggingMessage:
        case MessageType.FullLoggingData:
          this.ProcessLoggerMessage(message);
          break;
      }
      if (this.OnPrinterMessage == null)
        return;
      try
      {
        this.OnPrinterMessage(message);
      }
      catch (Exception ex)
      {
      }
    }

    private void OnPrintStoppedInternal(string serial)
    {
      if (string.IsNullOrEmpty(serial))
        return;
      this.settingsManager.UpdateUsedFilamentSpool(this.GetPrinterBySerialNumber(serial).GetCurrentFilament());
      this.settingsManager.SaveSettings();
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object data)
    {
      PrinterObject printerBySerialNumber = this.GetPrinterBySerialNumber(sn.ToString());
      if (printerBySerialNumber == null || type != MessageType.WarningABSPrintLarge || (printerBySerialNumber == null || !printerBySerialNumber.isConnected()))
        return;
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        int num1 = (int) printerBySerialNumber.ClearCurrentWarning((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      else
      {
        int num2 = (int) printerBySerialNumber.AbortPrint((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
    }

    public void ProcessLoggerMessage(SpoolerMessage message)
    {
      string str1 = Base64Convert.Base64Decode(message.Message);
      if (message.Type == MessageType.LoggingMessage)
      {
        lock (this.general_log)
        {
          this.general_log.Enqueue(message.SerialNumber.ToString() + "::" + str1);
          this.log_updated = true;
        }
      }
      else
      {
        if (message.Type != MessageType.FullLoggingData)
          return;
        string[] strArray = str1.Split('\n');
        lock (this.general_log)
        {
          this.general_log.Clear();
          foreach (string str2 in strArray)
            this.general_log.Enqueue(message.SerialNumber.ToString() + "::" + str2);
          this.log_updated = true;
        }
      }
    }

    public void ClearLog()
    {
      lock (this.general_log)
      {
        this.general_log.Clear();
        this.log_updated = true;
      }
    }

    public bool LogUpdated
    {
      get
      {
        lock (this.general_log)
          return this.log_updated;
      }
    }

    public List<string> GetLog()
    {
      List<string> stringList = (List<string>) null;
      lock (this.general_log)
      {
        stringList = new List<string>((IEnumerable<string>) this.general_log);
        this.log_updated = false;
      }
      return stringList;
    }

    public bool SelectPrinterBySerialNumber(string serial_number)
    {
      PrinterObject printerBySerialNumber = this.GetPrinterBySerialNumber(serial_number);
      PrinterObject printerObject1 = this.selected_printer.Value;
      if (printerObject1 != null && printerBySerialNumber != printerObject1 && printerObject1.HasLock)
      {
        int num = (int) printerObject1.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      this.selected_printer.Value = printerBySerialNumber;
      if (this.OnSelectedPrinterChanged != null)
      {
        PrinterObject printerObject2 = this.selected_printer.Value;
        if (printerObject2 != null)
          this.OnSelectedPrinterChanged(printerObject2.Info.serial_number);
      }
      return this.selected_printer.Value != null;
    }

    public PrinterObject GetPrinterBySerialNumber(string serial_number)
    {
      PrinterObject printerObject = (PrinterObject) null;
      if (string.IsNullOrEmpty(serial_number))
        return (PrinterObject) null;
      PrinterSerialNumber printerSerialNumber = new PrinterSerialNumber(serial_number);
      lock (this.connected_printers)
      {
        foreach (PrinterObject connectedPrinter in this.connected_printers)
        {
          if (connectedPrinter.Info.serial_number == printerSerialNumber)
            printerObject = connectedPrinter;
        }
      }
      return printerObject;
    }

    private PrinterObject SelectConnectedPrinter()
    {
      if (this.selected_printer.Value != null)
        return this.selected_printer.Value;
      lock (this.connected_printers)
      {
        foreach (PrinterObject connectedPrinter in this.connected_printers)
        {
          if (connectedPrinter.isConnected() && !connectedPrinter.Info.InBootloaderMode)
          {
            this.selected_printer.Value = connectedPrinter;
            if (this.OnSelectedPrinterChanged != null)
            {
              this.OnSelectedPrinterChanged(connectedPrinter.Info.serial_number);
              break;
            }
            break;
          }
        }
      }
      return this.selected_printer.Value;
    }

    public PrinterObject SelectedPrinter
    {
      get
      {
        PrinterObject printerObject = this.selected_printer.Value;
        if (printerObject == null)
          return this.SelectConnectedPrinter();
        if (!printerObject.isConnected())
        {
          this.selected_printer.Value = (PrinterObject) null;
          printerObject = (PrinterObject) null;
          if (this.OnSelectedPrinterChanged != null)
            this.OnSelectedPrinterChanged(PrinterSerialNumber.Undefined);
        }
        return printerObject;
      }
    }

    public SpoolerClient PrintSpoolerClient
    {
      get
      {
        return this.print_spooler_client;
      }
    }

    public static string LocalizedSpoolerMessageString(SpoolerMessage message)
    {
      return SpoolerConnection.LocalizedSpoolerMessageString(message.Type);
    }

    public static string LocalizedSpoolerMessageString(MessageType message)
    {
      string key = string.Format("T_{0}", (object) message.ToString());
      return Locale.GlobalLocale.T(key);
    }

    public delegate void PrinterMessageCallback(SpoolerMessage message);

    public delegate void SelectedPrinterChangedCallback(PrinterSerialNumber serial_number);

    private class UpdateFilamentData
    {
      public IPrinter printer;
      public FilamentSpool printer_filament;

      public UpdateFilamentData(IPrinter printer, FilamentSpool printer_filament)
      {
        this.printer = printer;
        this.printer_filament = printer_filament;
      }
    }
  }
}
