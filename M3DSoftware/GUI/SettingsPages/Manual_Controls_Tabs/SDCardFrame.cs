using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Views;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System;
using System.IO;

namespace M3D.GUI.SettingsPages.Manual_Controls_Tabs
{
  public class SDCardFrame : XMLFrame
  {
    private ListBoxWidget m_oSDFilesListbox;
    private ButtonWidget m_oAllowSavingtoSDOnlyCheckbox;
    private PrinterObject m_opLastPrinterSelected;
    private SettingsManager m_oSettings;
    private PopupMessageBox m_oMessagebox;
    private SpoolerConnection m_oSpoolerConnection;

    public SDCardFrame(int ID, GUIHost host, PopupMessageBox messagebox, SpoolerConnection spooler_connection, SettingsManager settings)
      : base(ID)
    {
      m_oMessagebox = messagebox;
      m_oSpoolerConnection = spooler_connection;
      m_oSettings = settings;
      var manualcontrolsframeSdcard = Resources.manualcontrolsframe_sdcard;
      Init(host, manualcontrolsframeSdcard, new ButtonCallback(MyButtonCallback));
      m_oSDFilesListbox = (ListBoxWidget)FindChildElement(1100);
      m_oAllowSavingtoSDOnlyCheckbox = (ButtonWidget)FindChildElement(1106);
      CenterHorizontallyInParent = true;
      RelativeY = 0.1f;
      RelativeWidth = 0.95f;
      RelativeHeight = 0.9f;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      SyncFromSettings();
      Visible = false;
      Enabled = false;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (bVisible)
      {
        PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
        if (selectedPrinter != null && selectedPrinter.SDCardExtension.Available)
        {
          DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, null);
          m_opLastPrinterSelected = selectedPrinter;
        }
        SyncFromSettings();
      }
      else
      {
        if (m_oSettings == null)
        {
          return;
        }

        m_oSettings.SaveSettings();
      }
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      VerifySelectedPrinter();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.IsConnected())
      {
        return;
      }

      switch (button.ID)
      {
        case 1000:
          var num = (int) selectedPrinter.SendEmergencyStop(null, null);
          break;
        case 1101:
          DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, null);
          break;
        case 1102:
          if (null == selectedPrinter.GetCurrentFilament())
          {
            m_oMessagebox.AddMessageToQueue("Unable to start print because the printer doesn't have any filament.");
            break;
          }
          var selectedFile1 = GetSelectedFile();
          if (string.IsNullOrEmpty(selectedFile1))
          {
            break;
          }

          DoPrinterOperation(SDCardFrame.Operation.Print, selectedPrinter, selectedFile1);
          break;
        case 1103:
          var selectedFile2 = GetSelectedFile();
          if (string.IsNullOrEmpty(selectedFile2))
          {
            break;
          }

          DoPrinterOperation(SDCardFrame.Operation.Delete, selectedPrinter, selectedFile2);
          break;
        case 1105:
          SaveGCodeToSDCard();
          break;
        case 1106:
          m_oSettings.CurrentAppearanceSettings.AllowSDOnlyPrinting = button.Checked;
          break;
      }
    }

    private string GetSelectedFile()
    {
      var selected = m_oSDFilesListbox.Selected;
      if (selected >= 0)
      {
        return m_oSDFilesListbox.Items[selected].ToString();
      }

      return null;
    }

    private void VerifySelectedPrinter()
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || m_opLastPrinterSelected == selectedPrinter)
      {
        return;
      }

      m_opLastPrinterSelected = selectedPrinter;
      if (!selectedPrinter.SDCardExtension.Available)
      {
        return;
      }

      DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, null);
    }

    private void SaveGCodeToSDCard()
    {
      PrinterObject selectedPrinter1 = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter1 == null || !selectedPrinter1.IsConnected())
      {
        return;
      }

      var sourceFileName = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.GCode);
      if (string.IsNullOrEmpty(sourceFileName))
      {
        return;
      }

      var str = Path.Combine(Paths.PublicDataFolder, "Working", "m3doutput.gcode");
      if (sourceFileName != str)
      {
        try
        {
          File.Copy(sourceFileName, str, true);
        }
        catch (Exception ex)
        {
          m_oMessagebox.AddMessageToQueue("Unable to send gcode file. " + ex.Message);
          return;
        }
      }
      var UserJob = new JobParams(str, "User Job", "", selectedPrinter1.GetCurrentFilament().filament_type, 0.0f, 0.0f);
      UserJob.options.autostart_ignorewarnings = true;
      UserJob.options.turn_on_fan_before_print = true;
      UserJob.preprocessor = selectedPrinter1.MyFilamentProfile.preprocessor;
      UserJob.filament_temperature = selectedPrinter1.MyFilamentProfile.Temperature;
      UserJob.jobMode = JobParams.Mode.SavingToSDCard;
      PrinterObject selectedPrinter2 = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter2 == null || !selectedPrinter2.IsConnected())
      {
        m_oMessagebox.AddMessageToQueue("Printer Disconnected", PopupMessageBox.MessageBoxButtons.OK);
      }
      else if (selectedPrinter2.PrinterState != PrinterObject.State.IsPrinting)
      {
        selectedPrinter2.AutoLockAndPrint(UserJob);
      }
      else
      {
        m_oMessagebox.AddMessageToQueue("Printer Busy", PopupMessageBox.MessageBoxButtons.OK);
      }
    }

    private void SyncFromSettings()
    {
      if (m_oAllowSavingtoSDOnlyCheckbox == null)
      {
        return;
      }

      m_oAllowSavingtoSDOnlyCheckbox.Checked = m_oSettings.CurrentAppearanceSettings.AllowSDOnlyPrinting;
    }

    private void DoPrinterOperation(SDCardFrame.Operation op, PrinterObject printer, string file)
    {
      var num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(DoOperationOnLock), new SDCardFrame.OnLockOperationData(op, printer, file));
    }

    private void DoOperationOnLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success && ar.CallResult != CommandResult.Success_LockAcquired)
      {
        return;
      }

      var asyncState = ar.AsyncState as SDCardFrame.OnLockOperationData;
      PrinterObject printer = asyncState.printer;
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter == printer && selectedPrinter.SDCardExtension.Available)
      {
        switch (asyncState.op)
        {
          case SDCardFrame.Operation.Refresh:
            var num1 = (int) selectedPrinter.SDCardExtension.RefreshSDCardList(new M3D.Spooling.Client.AsyncCallback(OnRefreshCallback), selectedPrinter);
            return;
          case SDCardFrame.Operation.Print:
            var file1 = asyncState.file;
            var jobParams = new JobParams(file1, file1, "null", selectedPrinter.GetCurrentFilament().filament_type, 0.0f, 0.0f)
            {
              jobMode = JobParams.Mode.FirmwarePrintingFromSDCard
            };
            jobParams.options.autostart_ignorewarnings = true;
            jobParams.options.dont_use_preprocessors = false;
            var filamentProfile = FilamentProfile.CreateFilamentProfile(selectedPrinter.GetCurrentFilament(), selectedPrinter.MyPrinterProfile);
            jobParams.preprocessor = filamentProfile.preprocessor;
            jobParams.filament_temperature = filamentProfile.Temperature;
            var num2 = (int) selectedPrinter.PrintModel(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), selectedPrinter, jobParams);
            return;
          case SDCardFrame.Operation.Delete:
            var file2 = asyncState.file;
            var num3 = (int) selectedPrinter.SDCardExtension.DeleteFileFromSDCard(new M3D.Spooling.Client.AsyncCallback(DoOperationOnLock), new SDCardFrame.OnLockOperationData(SDCardFrame.Operation.Refresh, selectedPrinter, (string)null), file2);
            return;
        }
      }
      var num4 = (int) printer.ReleaseLock(null, null);
    }

    private void OnRefreshCallback(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      var asyncState = ar.AsyncState as PrinterObject;
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter == asyncState && (selectedPrinter.SDCardExtension.Available && m_oSDFilesListbox != null))
      {
        m_oSDFilesListbox.Items.Clear();
        foreach (var sdCardFile in selectedPrinter.SDCardExtension.GetSDCardFileList())
        {
          m_oSDFilesListbox.Items.Add(sdCardFile);
        }
      }
      var num = (int) asyncState.ReleaseLock(null, null);
    }

    private enum AdvancedControlsID
    {
      Button_EmergencyStop = 1000, // 0x000003E8
      ListBox_SDFileList = 1100, // 0x0000044C
      Button_RefreshList = 1101, // 0x0000044D
      Button_PrintSavedJob = 1102, // 0x0000044E
      Button_DeleteSavedJob = 1103, // 0x0000044F
      Button_SaveGcodeToSD = 1105, // 0x00000451
      Checkbox_AllowSavingtoSDOnly = 1106, // 0x00000452
    }

    private enum Operation
    {
      Refresh,
      Print,
      Delete,
    }

    private class OnLockOperationData
    {
      public SDCardFrame.Operation op;
      public PrinterObject printer;
      public string file;

      public OnLockOperationData(SDCardFrame.Operation op, PrinterObject printer, string file)
      {
        this.op = op;
        this.printer = printer;
        this.file = file;
      }
    }
  }
}
