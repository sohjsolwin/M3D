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
      thread_sync = new object();
      connected_printers = new List<PrinterObject>();
      PrinterCount = new ThreadSafeVariable<int>
      {
        Value = 0
      };
      printerIDList = new List<string>();
      selected_printer = new ThreadSafeVariable<PrinterObject>
      {
        Value = (PrinterObject)null
      };
      general_log = new CircularArray<string>(200);
    }

    public bool CopyPrinterList(ref List<PrinterInfo> printer_list)
    {
      var flag = true;
      lock (thread_sync)
      {
        if (printer_list.Count != PrintSpoolerClient.PrinterCount)
        {
          flag = false;
        }
        else
        {
          try
          {
            for (var index = 0; index < PrintSpoolerClient.PrinterCount & flag; ++index)
            {
              PrinterInfo info = PrintSpoolerClient.GetPrinter(index).Info;
              if (printer_list[index].hardware.com_port != info.hardware.com_port)
              {
                flag = false;
              }
              else if (printer_list[index].hardware.machine_type != info.hardware.machine_type)
              {
                flag = false;
              }
              else if (printer_list[index].serial_number != info.serial_number)
              {
                flag = false;
              }
              else if (printer_list[index].current_job != info.current_job)
              {
                flag = false;
              }
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
          for (var index = 0; index < PrintSpoolerClient.PrinterCount; ++index)
          {
            printer_list.Add(PrintSpoolerClient.GetPrinter(index).Info);
          }

          selected_printer_index = 0;
        }
        else if (selected_printer_index < 0)
        {
          selected_printer_index = 0;
        }
      }
      return flag;
    }

    public void PrintModel(string sn, JobParams jobParams)
    {
      var printerBySerialNumber = (IPrinter)GetPrinterBySerialNumber(sn);
      if (printerBySerialNumber == null || !printerBySerialNumber.isConnected())
      {
        return;
      }

      var num = (int) printerBySerialNumber.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
    }

    public void SpoolerStartUp(DebugLogger debugLogger)
    {
      try
      {
        print_spooler_client = new SpoolerClient(debugLogger)
        {
          IgnoreConnectingPrinters = false
        };
        print_spooler_client.OnReceivedPrinterList += new OnReceivedPrinterListDel(OnReceivedPrinterList);
        print_spooler_client.OnReceivedMessage += new OnReceivedMessageDel(OnReceivedMessage);
        print_spooler_client.OnGotNewPrinter += new SpoolerClient.OnGotNewPrinterDel(OnGotNewPrinterInternal);
        print_spooler_client.OnPrinterDisconnected += new SpoolerClient.OnPrinterDisconnectedDel(OnPrinterDisconnectedInternal);
        print_spooler_client.OnProcessFromServer += new SpoolerClient.OnPrintProcessDel(OnPrintProcessChangedInternal);
        print_spooler_client.OnPrintStopped += new SpoolerClient.OnPrintStoppedDel(OnPrintStoppedInternal);
        var directorySeparatorChar = Path.DirectorySeparatorChar;
        Form1.debugLogger.Add("SpoolerConnection.SpoolerStartUp", "Setup", DebugLogger.LogType.Secondary);
        var num = (int)print_spooler_client.StartSession(Paths.ResourceFolder + directorySeparatorChar.ToString() + "Spooler" + directorySeparatorChar.ToString() + "M3DSpooler.exe", Paths.ResourceFolder + directorySeparatorChar.ToString() + "Spooler", "H B A D PSM", 2000);
        Form1.debugLogger.Add("SpoolerConnection.SpoolerStartUp", "Session Started", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public void SendMessageToConnectedPrinter(PrinterSerialNumber sn, SpoolerMessage message)
    {
      var printerObject = (PrinterObject) null;
      lock (connected_printers)
      {
        foreach (PrinterObject connectedPrinter in connected_printers)
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
      lock (connected_printers)
      {
        connected_printers.Add(new PrinterObject(new_printer, messagebox, informationbox, this, settingsManager));
      }

      if (OnGotNewPrinter == null)
      {
        return;
      }

      OnGotNewPrinter(new_printer);
    }

    private void OnPrinterDisconnectedInternal(Printer the_printer)
    {
      lock (connected_printers)
      {
        try
        {
          for (var index = 0; index < connected_printers.Count; ++index)
          {
            if (connected_printers[index].Info.hardware.com_port == the_printer.Info.hardware.com_port)
            {
              connected_printers.RemoveAt(index);
              break;
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      if (OnPrinterDisconnected == null)
      {
        return;
      }

      OnPrinterDisconnected(the_printer);
    }

    private void OnPrintProcessChangedInternal()
    {
      if (OnPrintProcessChanged == null)
      {
        return;
      }

      OnPrintProcessChanged();
    }

    public void OnShutdown()
    {
      if (print_spooler_client == null)
      {
        return;
      }

      print_spooler_client.CloseSession();
    }

    private void OnReceivedPrinterList(List<PrinterInfo> connected_printers)
    {
      PrinterCount.Value = connected_printers.Count;
      lock (printerIDList)
      {
        printerIDList.Clear();
        foreach (PrinterInfo connectedPrinter in connected_printers)
        {
          printerIDList.Add(connectedPrinter.serial_number.ToString());
        }
      }
      lock (this.connected_printers)
      {
        foreach (PrinterObject connectedPrinter in this.connected_printers)
        {
          connectedPrinter.ProcessState();
        }
      }
    }

    public List<string> GetPrinterIDList()
    {
      lock (printerIDList)
      {
        return new List<string>((IEnumerable<string>)printerIDList);
      }
    }

    public void CheckUpdatedFilamentProfile(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions new_data)
    {
      try
      {
        if (key.type == FilamentSpool.TypeEnum.NoFilament)
        {
          return;
        }

        foreach (Printer printer in PrintSpoolerClient)
        {
          FilamentSpool currentFilament = printer.GetCurrentFilament();
          if (currentFilament != (FilamentSpool) null && currentFilament.filament_type != FilamentSpool.TypeEnum.NoFilament && (currentFilament.filament_type == key.type && (FilamentConstants.ColorsEnum) currentFilament.filament_color_code == key.color))
          {
            currentFilament.filament_temperature = new_data.temperature;
            var num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(OnFilamentUpdateLock), (object) new SpoolerConnection.UpdateFilamentData((IPrinter) printer, currentFilament));
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void OnFilamentUpdateLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as SpoolerConnection.UpdateFilamentData;
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        return;
      }

      var num = (int) asyncState.printer.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(OnFilamentUpdateDone), (object) asyncState.printer, asyncState.printer_filament);
    }

    private void OnFilamentUpdateDone(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as IPrinter;
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    public bool FilamentSpoolLoaded(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions new_data)
    {
      var flag = false;
      try
      {
        if (key.type != FilamentSpool.TypeEnum.NoFilament)
        {
          foreach (Printer printer in PrintSpoolerClient)
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
          informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message));
          break;
        case MessageType.JobComplete:
        case MessageType.JobCanceled:
        case MessageType.PrinterMessage:
        case MessageType.PrinterTimeout:
        case MessageType.SinglePointCalibrationNotSupported:
        case MessageType.MultiPointCalibrationNotSupported:
        case MessageType.PowerOutageRecovery:
          informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message));
          break;
        case MessageType.JobStarted:
          if (settingsManager.ShowAllWarnings)
          {
            messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "A new job has started. Please do not unplug your 3D printer while it is printing. If you unplug your it while printing, you will have to recalibrate."));
          }

          informationbox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(message) + string.Format(" - {0}", (object) message.Message));
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
          messagebox.AddMessageToQueue(message);
          break;
        case MessageType.UserDefined:
          if (settingsManager.ShowAllWarnings)
          {
            messagebox.AddMessageToQueue(message);
            break;
          }
          break;
        case MessageType.WarningABSPrintLarge:
          messagebox.AddMessageToQueue(message, PopupMessageBox.MessageBoxButtons.OK, new PopupMessageBox.OnUserSelectionDel(OnUserSelection));
          break;
        case MessageType.IncompatibleSpooler:
          if (PrintSpoolerClient != null)
          {
            var num = (int)PrintSpoolerClient.ForceSpoolerShutdown();
            break;
          }
          break;
        case MessageType.LoggingMessage:
        case MessageType.FullLoggingData:
          ProcessLoggerMessage(message);
          break;
      }
      if (OnPrinterMessage == null)
      {
        return;
      }

      try
      {
        OnPrinterMessage(message);
      }
      catch (Exception ex)
      {
      }
    }

    private void OnPrintStoppedInternal(string serial)
    {
      if (string.IsNullOrEmpty(serial))
      {
        return;
      }

      settingsManager.UpdateUsedFilamentSpool(GetPrinterBySerialNumber(serial).GetCurrentFilament());
      settingsManager.SaveSettings();
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object data)
    {
      PrinterObject printerBySerialNumber = GetPrinterBySerialNumber(sn.ToString());
      if (printerBySerialNumber == null || type != MessageType.WarningABSPrintLarge || (printerBySerialNumber == null || !printerBySerialNumber.isConnected()))
      {
        return;
      }

      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        var num1 = (int) printerBySerialNumber.ClearCurrentWarning((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      else
      {
        var num2 = (int) printerBySerialNumber.AbortPrint((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
    }

    public void ProcessLoggerMessage(SpoolerMessage message)
    {
      var str1 = Base64Convert.Base64Decode(message.Message);
      if (message.Type == MessageType.LoggingMessage)
      {
        lock (general_log)
        {
          general_log.Enqueue(message.SerialNumber.ToString() + "::" + str1);
          log_updated = true;
        }
      }
      else
      {
        if (message.Type != MessageType.FullLoggingData)
        {
          return;
        }

        string[] strArray = str1.Split('\n');
        lock (general_log)
        {
          general_log.Clear();
          foreach (var str2 in strArray)
          {
            general_log.Enqueue(message.SerialNumber.ToString() + "::" + str2);
          }

          log_updated = true;
        }
      }
    }

    public void ClearLog()
    {
      lock (general_log)
      {
        general_log.Clear();
        log_updated = true;
      }
    }

    public bool LogUpdated
    {
      get
      {
        lock (general_log)
        {
          return log_updated;
        }
      }
    }

    public List<string> GetLog()
    {
      var stringList = (List<string>) null;
      lock (general_log)
      {
        stringList = new List<string>((IEnumerable<string>)general_log);
        log_updated = false;
      }
      return stringList;
    }

    public bool SelectPrinterBySerialNumber(string serial_number)
    {
      PrinterObject printerBySerialNumber = GetPrinterBySerialNumber(serial_number);
      PrinterObject printerObject1 = selected_printer.Value;
      if (printerObject1 != null && printerBySerialNumber != printerObject1 && printerObject1.HasLock)
      {
        var num = (int) printerObject1.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      selected_printer.Value = printerBySerialNumber;
      if (OnSelectedPrinterChanged != null)
      {
        PrinterObject printerObject2 = selected_printer.Value;
        if (printerObject2 != null)
        {
          OnSelectedPrinterChanged(printerObject2.Info.serial_number);
        }
      }
      return selected_printer.Value != null;
    }

    public PrinterObject GetPrinterBySerialNumber(string serial_number)
    {
      var printerObject = (PrinterObject) null;
      if (string.IsNullOrEmpty(serial_number))
      {
        return (PrinterObject) null;
      }

      var printerSerialNumber = new PrinterSerialNumber(serial_number);
      lock (connected_printers)
      {
        foreach (PrinterObject connectedPrinter in connected_printers)
        {
          if (connectedPrinter.Info.serial_number == printerSerialNumber)
          {
            printerObject = connectedPrinter;
          }
        }
      }
      return printerObject;
    }

    private PrinterObject SelectConnectedPrinter()
    {
      if (selected_printer.Value != null)
      {
        return selected_printer.Value;
      }

      lock (connected_printers)
      {
        foreach (PrinterObject connectedPrinter in connected_printers)
        {
          if (connectedPrinter.isConnected() && !connectedPrinter.Info.InBootloaderMode)
          {
            selected_printer.Value = connectedPrinter;
            if (OnSelectedPrinterChanged != null)
            {
              OnSelectedPrinterChanged(connectedPrinter.Info.serial_number);
              break;
            }
            break;
          }
        }
      }
      return selected_printer.Value;
    }

    public PrinterObject SelectedPrinter
    {
      get
      {
        PrinterObject printerObject = selected_printer.Value;
        if (printerObject == null)
        {
          return SelectConnectedPrinter();
        }

        if (!printerObject.isConnected())
        {
          selected_printer.Value = (PrinterObject) null;
          printerObject = (PrinterObject) null;
          if (OnSelectedPrinterChanged != null)
          {
            OnSelectedPrinterChanged(PrinterSerialNumber.Undefined);
          }
        }
        return printerObject;
      }
    }

    public SpoolerClient PrintSpoolerClient
    {
      get
      {
        return print_spooler_client;
      }
    }

    public static string LocalizedSpoolerMessageString(SpoolerMessage message)
    {
      return SpoolerConnection.LocalizedSpoolerMessageString(message.Type);
    }

    public static string LocalizedSpoolerMessageString(MessageType message)
    {
      var key = string.Format("T_{0}", (object) message.ToString());
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
