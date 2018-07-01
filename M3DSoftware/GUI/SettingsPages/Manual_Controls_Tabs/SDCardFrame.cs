// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Manual_Controls_Tabs.SDCardFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.m_oMessagebox = messagebox;
      this.m_oSpoolerConnection = spooler_connection;
      this.m_oSettings = settings;
      string manualcontrolsframeSdcard = Resources.manualcontrolsframe_sdcard;
      this.Init(host, manualcontrolsframeSdcard, new ButtonCallback(this.MyButtonCallback));
      this.m_oSDFilesListbox = (ListBoxWidget) this.FindChildElement(1100);
      this.m_oAllowSavingtoSDOnlyCheckbox = (ButtonWidget) this.FindChildElement(1106);
      this.CenterHorizontallyInParent = true;
      this.RelativeY = 0.1f;
      this.RelativeWidth = 0.95f;
      this.RelativeHeight = 0.9f;
      this.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.SyncFromSettings();
      this.Visible = false;
      this.Enabled = false;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (bVisible)
      {
        PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
        if (selectedPrinter != null && selectedPrinter.SDCardExtension.Available)
        {
          this.DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, (string) null);
          this.m_opLastPrinterSelected = selectedPrinter;
        }
        this.SyncFromSettings();
      }
      else
      {
        if (this.m_oSettings == null)
          return;
        this.m_oSettings.SaveSettings();
      }
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      this.VerifySelectedPrinter();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected())
        return;
      switch (button.ID)
      {
        case 1000:
          int num = (int) selectedPrinter.SendEmergencyStop((M3D.Spooling.Client.AsyncCallback) null, (object) null);
          break;
        case 1101:
          this.DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, (string) null);
          break;
        case 1102:
          if ((FilamentSpool) null == selectedPrinter.GetCurrentFilament())
          {
            this.m_oMessagebox.AddMessageToQueue("Unable to start print because the printer doesn't have any filament.");
            break;
          }
          string selectedFile1 = this.GetSelectedFile();
          if (string.IsNullOrEmpty(selectedFile1))
            break;
          this.DoPrinterOperation(SDCardFrame.Operation.Print, selectedPrinter, selectedFile1);
          break;
        case 1103:
          string selectedFile2 = this.GetSelectedFile();
          if (string.IsNullOrEmpty(selectedFile2))
            break;
          this.DoPrinterOperation(SDCardFrame.Operation.Delete, selectedPrinter, selectedFile2);
          break;
        case 1105:
          this.SaveGCodeToSDCard();
          break;
        case 1106:
          this.m_oSettings.CurrentAppearanceSettings.AllowSDOnlyPrinting = button.Checked;
          break;
      }
    }

    private string GetSelectedFile()
    {
      int selected = this.m_oSDFilesListbox.Selected;
      if (selected >= 0)
        return this.m_oSDFilesListbox.Items[selected].ToString();
      return (string) null;
    }

    private void VerifySelectedPrinter()
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || this.m_opLastPrinterSelected == selectedPrinter)
        return;
      this.m_opLastPrinterSelected = selectedPrinter;
      if (!selectedPrinter.SDCardExtension.Available)
        return;
      this.DoPrinterOperation(SDCardFrame.Operation.Refresh, selectedPrinter, (string) null);
    }

    private void SaveGCodeToSDCard()
    {
      PrinterObject selectedPrinter1 = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter1 == null || !selectedPrinter1.isConnected())
        return;
      string sourceFileName = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.GCode);
      if (string.IsNullOrEmpty(sourceFileName))
        return;
      string str = Path.Combine(Paths.PublicDataFolder, "Working", "m3doutput.gcode");
      if (sourceFileName != str)
      {
        try
        {
          File.Copy(sourceFileName, str, true);
        }
        catch (Exception ex)
        {
          this.m_oMessagebox.AddMessageToQueue("Unable to send gcode file. " + ex.Message);
          return;
        }
      }
      JobParams UserJob = new JobParams(str, "User Job", "", selectedPrinter1.GetCurrentFilament().filament_type, 0.0f, 0.0f);
      UserJob.options.autostart_ignorewarnings = true;
      UserJob.options.turn_on_fan_before_print = true;
      UserJob.preprocessor = selectedPrinter1.MyFilamentProfile.preprocessor;
      UserJob.filament_temperature = selectedPrinter1.MyFilamentProfile.Temperature;
      UserJob.jobMode = JobParams.Mode.SavingToSDCard;
      PrinterObject selectedPrinter2 = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter2 == null || !selectedPrinter2.isConnected())
        this.m_oMessagebox.AddMessageToQueue("Printer Disconnected", PopupMessageBox.MessageBoxButtons.OK);
      else if (selectedPrinter2.PrinterState != PrinterObject.State.IsPrinting)
        selectedPrinter2.AutoLockAndPrint(UserJob);
      else
        this.m_oMessagebox.AddMessageToQueue("Printer Busy", PopupMessageBox.MessageBoxButtons.OK);
    }

    private void SyncFromSettings()
    {
      if (this.m_oAllowSavingtoSDOnlyCheckbox == null)
        return;
      this.m_oAllowSavingtoSDOnlyCheckbox.Checked = this.m_oSettings.CurrentAppearanceSettings.AllowSDOnlyPrinting;
    }

    private void DoPrinterOperation(SDCardFrame.Operation op, PrinterObject printer, string file)
    {
      int num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.DoOperationOnLock), (object) new SDCardFrame.OnLockOperationData(op, printer, file));
    }

    private void DoOperationOnLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success && ar.CallResult != CommandResult.Success_LockAcquired)
        return;
      SDCardFrame.OnLockOperationData asyncState = ar.AsyncState as SDCardFrame.OnLockOperationData;
      PrinterObject printer = asyncState.printer;
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter == printer && selectedPrinter.SDCardExtension.Available)
      {
        switch (asyncState.op)
        {
          case SDCardFrame.Operation.Refresh:
            int num1 = (int) selectedPrinter.SDCardExtension.RefreshSDCardList(new M3D.Spooling.Client.AsyncCallback(this.OnRefreshCallback), (object) selectedPrinter);
            return;
          case SDCardFrame.Operation.Print:
            string file1 = asyncState.file;
            JobParams jobParams = new JobParams(file1, file1, "null", selectedPrinter.GetCurrentFilament().filament_type, 0.0f, 0.0f);
            jobParams.jobMode = JobParams.Mode.FirmwarePrintingFromSDCard;
            jobParams.options.autostart_ignorewarnings = true;
            jobParams.options.dont_use_preprocessors = false;
            FilamentProfile filamentProfile = FilamentProfile.CreateFilamentProfile(selectedPrinter.GetCurrentFilament(), selectedPrinter.MyPrinterProfile);
            jobParams.preprocessor = filamentProfile.preprocessor;
            jobParams.filament_temperature = filamentProfile.Temperature;
            int num2 = (int) selectedPrinter.PrintModel(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, jobParams);
            return;
          case SDCardFrame.Operation.Delete:
            string file2 = asyncState.file;
            int num3 = (int) selectedPrinter.SDCardExtension.DeleteFileFromSDCard(new M3D.Spooling.Client.AsyncCallback(this.DoOperationOnLock), (object) new SDCardFrame.OnLockOperationData(SDCardFrame.Operation.Refresh, selectedPrinter, (string) null), file2);
            return;
        }
      }
      int num4 = (int) printer.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private void OnRefreshCallback(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
        return;
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter == asyncState && (selectedPrinter.SDCardExtension.Available && this.m_oSDFilesListbox != null))
      {
        this.m_oSDFilesListbox.Items.Clear();
        foreach (object sdCardFile in selectedPrinter.SDCardExtension.GetSDCardFileList())
          this.m_oSDFilesListbox.Items.Add(sdCardFile);
      }
      int num = (int) asyncState.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
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
