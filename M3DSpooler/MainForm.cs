// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.MainForm
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooler.Core;
using M3D.Spooler.Dialogs;
using M3D.Spooler.Forms;
using M3D.Spooler.Properties;
using M3D.Spooling;
using M3D.Spooling.Client;
using M3D.Spooling.Client.Extensions;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class MainForm : Form
  {
    private string lastManualCommand = string.Empty;
    private ThreadSafeVariable<bool> PrinterLocked = new ThreadSafeVariable<bool>(false);
    private SampleSet m_sampleSetTemperatureSamples = new SampleSet(100);
    private ThreadSafeVariable<MainForm.PrinterLockChanging> LockChanging = new ThreadSafeVariable<MainForm.PrinterLockChanging>(MainForm.PrinterLockChanging.No);
    private bool printer_controls_open = true;
    private object _myCurrentPrinterObjectSync = new object();
    private static MainForm global_form;
    private const int DefaultLockTimeOut = 120;
    private SpoolerSettings settings;
    private ThreadSafeVariable<bool> shared_shutdown;
    private SpoolerClientBuiltIn spooler_client;
    private AdvancedStatistics advancedStatisticsDialog;
    private string Selected_Printer_Serial;
    public static bool AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT;
    private bool has_had_a_connected_client;
    private bool m_bSDTabSelected;
    private Stopwatch restart_counter;
    private bool in_check_printers;
    private ConcurrentDictionary<PrinterSerialNumber, MainForm.PrinterData> m_oPrinterData;
    private ThreadSafeVariable<RequestedFormTask> form_task;
    private bool m_bDontHideSpoolerJustShutDown;
    public bool m_bShutdownByUser;
    private bool force_shutdown;
    private bool job_stopped;
    private Timer timerLogger;
    private int firmwareUpdateStatusIndex;
    private Queue<string> logqueue;
    private ThreadSafeVariable<bool> logToScreen;
    private StayAwakeAndShutdown stay_awake;
    private SpoolerMessageHandler message_handler;
    private Printer MyCurrentPrinterObject;
    private const float XY_SPEED = 3000f;
    private const float Z_SPEED = 90f;
    private const float E_SPEED = 345f;
    private IContainer components;
    private Timer timer1;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStripMenuItem viewToolStripMenuItem;
    private GroupBox groupBoxPrinterList;
    private Button buttonStandAlone;
    private CheckBox checkBoxAutoCheckFirmware;
    private ListView listViewPrinterInfo;
    private ColumnHeader columnSerialNumber;
    private ColumnHeader columnStatus;
    private ColumnHeader columnTemp;
    public ColumnHeader columnZValid;
    private ColumnHeader columnJobStatus;
    private ColumnHeader columnFile;
    private ColumnHeader columnPerComplete;
    private ColumnHeader columnUser;
    private Label labelSpoolerVersion;
    private GroupBox groupBoxPrinterControls;
    private Button buttonLock;
    private GroupBox groupBoxControls;
    private TabControl tabControl1;
    private TabPage tabPageBasicOptions;
    private Button buttonHeaterOff;
    private Button buttonFanOff;
    private Button buttonFanOn;
    private TextBox textBoxEVal;
    private Button buttonPLATemp;
    private Button buttonDownE;
    private Button buttonEmergencyStop;
    private Button buttonUpE;
    private Button buttonABSTemp;
    private Button buttonAddJob;
    private Button buttonMotorsOff;
    private Button buttonMotorsOn;
    private TextBox textBoxYVal;
    private TextBox textBoxXVal;
    private Button buttonPrintToFile;
    private TextBox textBoxZVal;
    private Button button2;
    private Button buttonUpX;
    private Button button1;
    private Button buttonDownX;
    private Button buttonFilamentInfo;
    private Button buttonDownZ;
    private Button buttonDownY;
    private Button buttonUpZ;
    private Button buttonUpY;
    private TabPage tabPageDiagnostics;
    private Button buttonUpdateFirmware;
    private Button buttonPreprocess;
    private GroupBox groupBox4;
    private Button buttonBedPointTest;
    private ComboBox comboBoxBedTestPoint;
    private Button buttonBacklashPrint;
    private Button buttonGetGCode;
    private Button buttonReCenter;
    private Button buttonHome;
    private Button buttonSpeedTest;
    private Button buttonXSpeedTest;
    private Button buttonYSkipTestBack;
    private Button buttonXSkipTestRight;
    private Button buttonYSkipTest;
    private Button buttonXSkipTest;
    private Button buttonRepeatLast;
    private Label label3;
    private TextBox textBoxManualGCode;
    private Button buttonSendGCode;
    private Button buttonClearLog;
    private CheckBox checkBoxLogToScreen;
    private RichTextBox richTextBoxLoggedItems;
    private Label label5;
    private ListBox listBoxManualHistory;
    private Label label4;
    private CheckBox checkBoxAutoScroll;
    private ToolStripMenuItem showAdvancedToolStripMenuItem;
    private Label label1;
    private ComboBox selectedPrinterComboBox;
    private ToolStripMenuItem optionsToolStripMenuItem;
    private ToolStripMenuItem showAdvancedOptionsAtStartupToolStripMenuItem;
    private Button buttonTGHTemp;
    private Button buttonResumePrint;
    private Button buttonPausePrint;
    private ToolStripMenuItem showAdvancedStatisticsToolStripMenuItem;
    private Button buttonLoadEepromData;
    private Button buttonSaveEepromData;
    private ColumnHeader columnType;
    private Button buttonAbortPrint;
    private CheckBox logWaitsCheckBox;
    private GroupBox groupBoxBootloaderOptions;
    private GroupBox groupBoxFirmwareControls;
    private Button button3;
    private Label label2;
    private Button buttonQuitBootloader;
    private ColumnHeader columnBedTemp;
    private TabPage tabPageHeatedBedControl;
    private Button buttonTurnOfHeatedbed;
    private Button buttonBedHeatToABR;
    private Button buttonBedHeatToABS;
    private Button buttonBedHeatToPLA;
    private TabPage tabPageSDCard;
    private Button buttonSDDeleteFile;
    private Button buttonSDPrint;
    private Button buttonSDSaveGcode;
    private Button buttonSDRefresh;
    private ListBox listBoxSDFiles;
    private Label label6;
    private CheckBox checkBoxCalibrateBeforeSDPrint;
    private GroupBox groupBoxPrinting;
    private GroupBox groupBox1;
    private Label textBoxMeanTemperature;
    private Label label7;
    private GroupBox groupBox2;
    private Button buttonSetTemp;
    private TextBox textBoxTempEdit;
    private Label label8;
    private GroupBox groupBox3;

    public MainForm(SpoolerClientBuiltIn spooler_client)
    {
      this.m_oPrinterData = new ConcurrentDictionary<PrinterSerialNumber, MainForm.PrinterData>();
      this.form_task = new ThreadSafeVariable<RequestedFormTask>(RequestedFormTask.None);
      spooler_client.OnReceivedSpoolerShutdownMessage += new EventHandler<EventArgs>(this.OnReceivedSpoolerShutdownMessage);
      spooler_client.OnReceivedSpoolerShowMessage += new EventHandler<EventArgs>(this.OnReceivedSpoolerShowMessage);
      spooler_client.OnReceivedSpoolerHideMessage += new EventHandler<EventArgs>(this.OnReceivedSpoolerHideMessage);
      SpoolerClientBuiltIn spoolerClientBuiltIn1 = spooler_client;
      spoolerClientBuiltIn1.OnGotNewPrinter = spoolerClientBuiltIn1.OnGotNewPrinter + new SpoolerClient.OnGotNewPrinterDel(this.OnGotNewPrinter);
      SpoolerClientBuiltIn spoolerClientBuiltIn2 = spooler_client;
      spoolerClientBuiltIn2.OnPrinterDisconnected = spoolerClientBuiltIn2.OnPrinterDisconnected + new SpoolerClient.OnPrinterDisconnectedDel(this.OnPrinterDisconnected);
      this.stay_awake = new StayAwakeAndShutdown();
      this.stay_awake.StartUp(this.Handle);
      this.shared_shutdown = new ThreadSafeVariable<bool>(false);
      MainForm.global_form = this;
      this.InitializeComponent();
      this.InitializeTimers();
      this.logToScreen = new ThreadSafeVariable<bool>(false);
      this.logqueue = new Queue<string>();
      this.advancedStatisticsDialog = new AdvancedStatistics();
      this.buttonStandAlone.Visible = false;
      this.restart_counter = new Stopwatch();
      this.restart_counter.Reset();
      this.message_handler = new SpoolerMessageHandler(spooler_client);
      if (!MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT)
        this.m_bDontHideSpoolerJustShutDown = true;
      this.spooler_client = spooler_client;
    }

    private void OnLoad(object sender, EventArgs e)
    {
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(MainForm.SystemEvents_PowerModeChanged);
      this.settings = SpoolerSettings.LoadSettings();
      if (this.settings == null)
        this.settings = new SpoolerSettings();
      SpoolerClientBuiltIn spoolerClient1 = this.spooler_client;
      spoolerClient1.OnGotNewPrinter = spoolerClient1.OnGotNewPrinter + new SpoolerClient.OnGotNewPrinterDel(this.message_handler.OnGotNewPrinter);
      SpoolerClientBuiltIn spoolerClient2 = this.spooler_client;
      spoolerClient2.OnReceivedMessage = spoolerClient2.OnReceivedMessage + new OnReceivedMessageDel(this.message_handler.OnSpoolerMessage);
      this.spooler_client.ConnectInternalSpoolerToWindow(this.Handle);
      this.StartTimers();
      this.checkBoxAutoCheckFirmware.Checked = true;
      this.checkBoxAutoCheckFirmware.Enabled = false;
      this.OnAutoCheckFirmwareChanged((object) null, new EventArgs());
      this.buttonLoadEepromData.Visible = false;
      this.viewToolStripMenuItem.DropDownItems.Remove((ToolStripItem) this.showAdvancedStatisticsToolStripMenuItem);
      this.labelSpoolerVersion.Text = "Spooler Version: " + M3D.Spooling.Version.VersionText;
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Checked = this.settings.StartAdvanced;
      if (this.settings.StartAdvanced)
        return;
      this.ClosePrinterControls();
    }

    private void ClosePrinterControls()
    {
      if (!this.printer_controls_open)
        return;
      this.groupBoxPrinterControls.Visible = false;
      this.groupBoxPrinterList.Location = new Point(1, 22);
      this.Height = 190;
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject != null && this.MyCurrentPrinterObject.HasLock)
        {
          int num = (int) this.MyCurrentPrinterObject.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        }
        this.MyCurrentPrinterObject = (Printer) null;
      }
      this.LockChanging.Value = MainForm.PrinterLockChanging.No;
      this.PrinterLocked.Value = false;
      this.printer_controls_open = false;
      this.showAdvancedToolStripMenuItem.Text = "Show Advanced Options";
    }

    private void OpenPrinterControls()
    {
      if (this.printer_controls_open)
        return;
      this.Height = 585;
      this.groupBoxPrinterControls.Visible = true;
      this.buttonPausePrint.Enabled = false;
      this.buttonResumePrint.Enabled = false;
      this.buttonAbortPrint.Enabled = false;
      this.groupBoxControls.Enabled = false;
      this.checkBoxLogToScreen.Checked = false;
      this.groupBoxPrinterList.Location = new Point(1, 420);
      this.printer_controls_open = true;
      this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      this.showAdvancedToolStripMenuItem.Text = "Hide Advanced Options";
    }

    private void CheckLockState()
    {
      if (this.LockChanging.Value == MainForm.PrinterLockChanging.YesToLocked)
      {
        this.PrinterLocked.Value = true;
        this.groupBoxControls.Enabled = true;
        this.buttonLock.Text = "Release Printer";
        this.buttonLock.Enabled = true;
        this.LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      else if (this.LockChanging.Value == MainForm.PrinterLockChanging.YesToUnlocked)
      {
        this.PrinterLocked.Value = false;
        this.groupBoxControls.Enabled = false;
        this.buttonLock.Text = "Control This Printer";
        this.buttonLock.Enabled = true;
        this.LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      else if (this.LockChanging.Value == MainForm.PrinterLockChanging.NoFailed)
      {
        this.buttonLock.Enabled = true;
        this.LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      lock (this._myCurrentPrinterObjectSync)
      {
        try
        {
          if (this.MyCurrentPrinterObject != null)
          {
            if (this.MyCurrentPrinterObject.InBootloaderMode)
            {
              this.groupBoxFirmwareControls.Visible = false;
              this.groupBoxPrinting.Visible = false;
              this.groupBoxBootloaderOptions.Visible = true;
            }
            else
            {
              if (this.MyCurrentPrinterObject.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed && this.MyCurrentPrinterObject.Info.supportedFeatures.Available("Heated Bed Control", this.MyCurrentPrinterObject.MyPrinterProfile.SupportedFeaturesConstants))
              {
                if (!this.tabControl1.TabPages.Contains(this.tabPageHeatedBedControl))
                  this.tabControl1.TabPages.Add(this.tabPageHeatedBedControl);
              }
              else if (this.tabControl1.TabPages.Contains(this.tabPageHeatedBedControl))
                this.tabControl1.TabPages.Remove(this.tabPageHeatedBedControl);
              if (this.MyCurrentPrinterObject.Info.supportedFeatures.UsesSupportedFeatures && this.MyCurrentPrinterObject.Info.supportedFeatures.Available("Untethered Printing", this.MyCurrentPrinterObject.MyPrinterProfile.SupportedFeaturesConstants))
              {
                if (!this.tabControl1.TabPages.Contains(this.tabPageSDCard))
                  this.tabControl1.TabPages.Add(this.tabPageSDCard);
              }
              else if (this.tabControl1.TabPages.Contains(this.tabPageSDCard))
                this.tabControl1.TabPages.Remove(this.tabPageSDCard);
              this.groupBoxFirmwareControls.Visible = true;
              this.groupBoxBootloaderOptions.Visible = false;
              this.groupBoxPrinting.Visible = false;
            }
          }
          else
            this.groupBoxControls.Enabled = false;
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void OnClosing(object sender, FormClosingEventArgs e)
    {
      if (this.spooler_client == null)
        return;
      if (this.m_bShutdownByUser || this.force_shutdown || (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) || !this.printer_controls_open && this.m_bDontHideSpoolerJustShutDown)
      {
        if (!this.CanShutdownImmediately() && !this.force_shutdown && e.CloseReason != CloseReason.TaskManagerClosing)
        {
          if ((!this.m_bShutdownByUser || e.CloseReason == CloseReason.WindowsShutDown) && this.spooler_client.ClientCount > 0)
          {
            e.Cancel = true;
            return;
          }
          int num1 = (int) MessageBox.Show("Shutting down now will stop all print jobs and may cause you printer to lose calibration. Shut down anyway.", "M3D Print Spooler", MessageBoxButtons.YesNo);
          this.ClearShutdownMessage();
          int num2 = 7;
          if (num1 == num2)
          {
            e.Cancel = true;
            return;
          }
        }
        SpoolerSettings.SaveSettings(this.settings, true);
        this.StopTimers();
        MainForm.global_form = (MainForm) null;
        this.spooler_client.CloseSession();
        this.shared_shutdown.Value = true;
        if (this.stay_awake == null)
          return;
        this.stay_awake.Shutdown();
      }
      else if (this.printer_controls_open)
      {
        this.ClosePrinterControls();
        e.Cancel = true;
      }
      else
      {
        this.TopMost = false;
        this.Visible = false;
        this.ShowInTaskbar = false;
        this.Hide();
        e.Cancel = true;
      }
    }

    public bool CanShutdownImmediately()
    {
      if (this.spooler_client.CanShutdown())
        return true;
      this.StayAwakeMethods.CreateShutdownMessage("This app is preventing shutdown because this will cancel print jobs.");
      return false;
    }

    public void ClearShutdownMessage()
    {
      this.StayAwakeMethods.DestroyShutdownMessage();
    }

    private void RefreshPrinters(bool redo_list = false)
    {
      ++this.firmwareUpdateStatusIndex;
      this.firmwareUpdateStatusIndex %= 5;
      try
      {
        if (this.spooler_client.IsBusy && !this.StayAwakeMethods.InStayAwakeMode())
          this.StayAwakeMethods.NeverSleep();
        else if (!this.spooler_client.IsBusy && this.StayAwakeMethods.InStayAwakeMode())
          this.StayAwakeMethods.AllowSleep();
        List<PrinterSerialNumber> serialNumbers = this.spooler_client.GetSerialNumbers();
        bool flag = false;
        string str = (string) null;
        int driverInstallCount = this.spooler_client.PrinterDriverInstallCount;
        int num = driverInstallCount;
        foreach (ListViewItem listViewItem in this.listViewPrinterInfo.Items)
        {
          foreach (PrinterSerialNumber printerSerialNumber in serialNumbers)
          {
            if (listViewItem.Text == printerSerialNumber.ToString())
            {
              ++num;
              break;
            }
          }
        }
        if (this.listViewPrinterInfo.SelectedItems.Count > 0)
          str = this.listViewPrinterInfo.SelectedItems[0].Text;
        if (num != this.listViewPrinterInfo.Items.Count | redo_list)
        {
          this.listViewPrinterInfo.Items.Clear();
          this.selectedPrinterComboBox.Items.Clear();
          this.advancedStatisticsDialog.ClearList();
          flag = true;
        }
        List<PrinterInfo> printerInfo = this.spooler_client.GetPrinterInfo();
        this.AddPrinterInfoToListView(printerInfo, driverInstallCount);
        this.advancedStatisticsDialog.RefreshList(printerInfo);
        this.CheckForPrintFailureOnPowerOutage(printerInfo);
        if (!string.IsNullOrEmpty(str) & flag)
        {
          ListViewItem listViewItem = this.FindItem(this.listViewPrinterInfo, this.Selected_Printer_Serial);
          if (listViewItem != null && !listViewItem.Selected)
            listViewItem.Selected = true;
        }
        lock (this._myCurrentPrinterObjectSync)
        {
          if (string.IsNullOrEmpty(this.Selected_Printer_Serial) && this.selectedPrinterComboBox.Items.Count > 0)
            this.SetNewPrinter(this.selectedPrinterComboBox.Items[0].ToString());
          else if (this.MyCurrentPrinterObject == null)
          {
            string selectedPrinterSerial = this.Selected_Printer_Serial;
            this.Selected_Printer_Serial = (string) null;
            this.SetNewPrinter(selectedPrinterSerial);
          }
        }
        if (this.MyCurrentPrinterObject != null)
        {
          if (this.MyCurrentPrinterObject.Info.current_job != null)
            this.RefreshPrintingTemperatureStats(this.MyCurrentPrinterObject.Info);
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("MainForm.UpdatePrinters" + ex.Message, "Exception");
        this.Selected_Printer_Serial = (string) null;
      }
      this.RefreshCurrentPrinter();
      if (MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT && this.has_had_a_connected_client)
      {
        if (this.spooler_client.ClientCount == 0 && !this.spooler_client.IsBusy)
        {
          this.force_shutdown = true;
          this.Close();
        }
      }
      else if (!MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT && this.has_had_a_connected_client && this.spooler_client.ClientCount == 0)
      {
        this.has_had_a_connected_client = false;
        this.m_bDontHideSpoolerJustShutDown = true;
      }
      if (this.has_had_a_connected_client || this.spooler_client.ClientCount <= 0)
        return;
      this.m_bDontHideSpoolerJustShutDown = false;
      this.has_had_a_connected_client = true;
    }

    private void AddPrinterInfoToListView(List<PrinterInfo> connected_printers, int numPrintersInstalling)
    {
      foreach (PrinterInfo connectedPrinter in connected_printers)
      {
        PrinterSerialNumber serialNumber = connectedPrinter.serial_number;
        PrinterProfile printerProfile = this.spooler_client.GetPrinterProfile(connectedPrinter.ProfileName);
        if (!(serialNumber == PrinterSerialNumber.Undefined))
        {
          if (!this.selectedPrinterComboBox.Items.Contains((object) serialNumber.ToString()))
            this.selectedPrinterComboBox.Items.Add((object) serialNumber.ToString());
          if (connectedPrinter != null)
          {
            ListViewItem listViewItem = this.FindItem(this.listViewPrinterInfo, connectedPrinter.serial_number.ToString());
            if (listViewItem == null)
            {
              listViewItem = this.listViewPrinterInfo.Items.Add(connectedPrinter.serial_number.ToString());
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("None");
              listViewItem.SubItems.Add(" ");
              listViewItem.SubItems.Add(" ");
              listViewItem.SubItems.Add(" ");
            }
            string profileName = connectedPrinter.ProfileName;
            if (listViewItem.SubItems[1].Text != profileName)
              listViewItem.SubItems[1].Text = profileName;
            string str1 = connectedPrinter.Status.ToString();
            if (connectedPrinter.Status == PrinterStatus.Bootloader_UpdatingFirmware)
            {
              str1 += " ";
              for (int index = 0; index < this.firmwareUpdateStatusIndex; ++index)
                str1 += "-";
            }
            if (listViewItem.SubItems[2].Text != str1)
              listViewItem.SubItems[2].Text = str1;
            string str2 = (double) connectedPrinter.extruder.Temperature != -1.0 ? ((double) connectedPrinter.extruder.Temperature >= 1.0 ? connectedPrinter.extruder.Temperature.ToString() : "OFF") : "ON";
            if (str2 != listViewItem.SubItems[3].Text)
              listViewItem.SubItems[3].Text = str2;
            string str3 = !printerProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed ? "N/A" : ((double) connectedPrinter.accessories.BedStatus.BedTemperature != -1.0 ? ((double) connectedPrinter.accessories.BedStatus.BedTemperature >= 1.0 ? connectedPrinter.accessories.BedStatus.BedTemperature.ToString() : "OFF") : "ON");
            if (str3 != listViewItem.SubItems[4].Text)
              listViewItem.SubItems[4].Text = str3;
            string str4 = !connectedPrinter.extruder.Z_Valid ? "Invalid" : "Valid";
            if (str4 != listViewItem.SubItems[5].Text)
              listViewItem.SubItems[5].Text = str4;
            JobInfo currentJob = connectedPrinter.current_job;
            string str5;
            string str6;
            string str7;
            string str8;
            if (currentJob != null)
            {
              str5 = currentJob.Status.ToString();
              str6 = currentJob.JobName;
              float f = currentJob.PercentComplete * 100f;
              str7 = float.IsNaN(f) || (double) f < 0.0 || (double) f > 100.0 ? "Processing" : f.ToString();
              str8 = currentJob.User;
            }
            else
            {
              str5 = "None";
              str6 = " ";
              str7 = " ";
              str8 = " ";
            }
            if (str5 != listViewItem.SubItems[6].Text)
              listViewItem.SubItems[6].Text = str5;
            if (listViewItem.SubItems[7].Text != str6)
              listViewItem.SubItems[7].Text = str6;
            if (listViewItem.SubItems[8].Text != str7)
              listViewItem.SubItems[8].Text = str7;
            if (listViewItem.SubItems[9].Text != str8)
              listViewItem.SubItems[9].Text = str8;
          }
        }
      }
      for (int index = 0; index < numPrintersInstalling; ++index)
      {
        PrinterSerialNumber printerSerialNumber = new PrinterSerialNumber(index.ToString("X16"));
        if (this.FindItem(this.listViewPrinterInfo, printerSerialNumber.ToString()) == null)
        {
          ListViewItem listViewItem = this.listViewPrinterInfo.Items.Add(printerSerialNumber.ToString());
          listViewItem.SubItems.Add("Driver Installing");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("None");
          listViewItem.SubItems.Add(" ");
          listViewItem.SubItems.Add(" ");
          listViewItem.SubItems.Add(" ");
        }
      }
    }

    private void CheckForPrintFailureOnPowerOutage(List<PrinterInfo> connected_printers)
    {
      foreach (PrinterInfo connectedPrinter in connected_printers)
      {
        if (connectedPrinter.powerRecovery.bPowerOutageWhilePrinting)
        {
          bool flag = false;
          if (this.m_oPrinterData.ContainsKey(connectedPrinter.serial_number))
            flag = this.m_oPrinterData[connectedPrinter.serial_number].bPowerOutageHandled;
          else
            this.m_oPrinterData.TryAdd(connectedPrinter.serial_number, new MainForm.PrinterData());
          this.m_oPrinterData[connectedPrinter.serial_number].bPowerOutageHandled = true;
          if (!flag && this.message_handler != null)
            this.message_handler.OnSpoolerMessage(new SpoolerMessage(MessageType.PowerOutageWhilePrinting, connectedPrinter.serial_number, (string) null));
        }
      }
    }

    private void RefreshCurrentPrinter()
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject != null)
        {
          if (!this.MyCurrentPrinterObject.HasLock && this.PrinterLocked.Value)
            this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
          this.CheckLockState();
          bool flag1 = true;
          if (!this.MyCurrentPrinterObject.Connected && !this.MyCurrentPrinterObject.Switching)
            this.SetNewPrinter((string) null);
          try
          {
            if (this.MyCurrentPrinterObject.Info != null)
            {
              bool flag2 = this.MyCurrentPrinterObject.Info.current_job != null;
              bool flag3 = this.MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_PrintingPaused;
              if (flag2)
              {
                if (this.MyCurrentPrinterObject.Info.current_job.Status != JobStatus.SavingToSD)
                {
                  if (this.MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_Printing)
                  {
                    this.buttonResumePrint.Enabled = false;
                    this.buttonResumePrint.Visible = false;
                    this.buttonPausePrint.Enabled = true;
                    this.buttonPausePrint.Visible = true;
                    flag1 = false;
                  }
                  else if (this.MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_PrintingPaused)
                  {
                    if (this.groupBoxControls.Enabled)
                      this.buttonResumePrint.Enabled = true;
                    this.buttonResumePrint.Visible = true;
                    this.buttonPausePrint.Enabled = false;
                    this.buttonPausePrint.Visible = false;
                    flag1 = false;
                  }
                }
                this.buttonAbortPrint.Enabled = true;
              }
              else
                this.buttonAbortPrint.Enabled = false;
              bool flag4 = this.MyCurrentPrinterObject.Info.Status == PrinterStatus.Bootloader_UpdatingFirmware;
              this.ControlsEnable(this.MyCurrentPrinterObject.Connected && !(flag2 | flag4) | flag3);
            }
          }
          catch (Exception ex)
          {
          }
          if (!flag1)
            return;
          this.buttonResumePrint.Enabled = false;
          this.buttonResumePrint.Visible = false;
          this.buttonPausePrint.Enabled = false;
          this.buttonPausePrint.Visible = true;
        }
        else
          this.groupBoxControls.Enabled = false;
      }
    }

    private void RefreshPrintingTemperatureStats(PrinterInfo printerInfo)
    {
      double num1 = (double) printerInfo.extruder.Temperature > 0.0 ? (double) printerInfo.extruder.Temperature : 0.0;
      this.m_sampleSetTemperatureSamples.Add(num1);
      double sampleMean = (double) this.m_sampleSetTemperatureSamples.SampleMean;
      double num2 = num1 - sampleMean;
      byte num3 = 0;
      byte num4 = 0;
      byte num5 = 0;
      if (num2 < 0.0)
      {
        if (num2 < -5.0)
          num2 = -5.0;
        num4 = (byte) (-num2 * (double) byte.MaxValue / 5.0);
      }
      else if (num2 > 0.0)
      {
        if (num2 > 5.0)
          num2 = 5.0;
        num3 = (byte) (num2 * (double) byte.MaxValue / 5.0);
      }
      if (num2 > 2.0 || num2 < -2.0 || num1 < 150.0)
      {
        this.textBoxTempEdit.Enabled = false;
        this.buttonSetTemp.Enabled = false;
      }
      else
      {
        this.textBoxTempEdit.Enabled = true;
        this.buttonSetTemp.Enabled = true;
      }
      this.textBoxMeanTemperature.ForeColor = System.Drawing.Color.FromArgb((int) num3, (int) num5, (int) num4);
      this.textBoxMeanTemperature.Text = num1.ToString("0.00");
    }

    private void ControlsEnable(bool flag)
    {
      GroupBox boxPrinterControls = this.groupBoxPrinterControls;
      foreach (Control control in this.GetAll((Control) boxPrinterControls, new HashSet<System.Type>()
      {
        typeof (TextBox),
        typeof (Button)
      }))
      {
        if (!(control.Name == "buttonEmergencyStop") && !(control.Name == "buttonStandAlone") && (!(control.Name == "buttonClearLog") && !(control.Name == "buttonFanOff")) && (!(control.Name == "buttonFanOn") && !(control.Name == "buttonSetClear") && (!(control.Name == "buttonLock") && !(control.Name == "buttonResumePrint"))) && (!(control.Name == "buttonPausePrint") && !(control.Name == "buttonAbortPrint") && (!(control.Name == "textBoxTempEdit") && !(control.Name == "buttonSetTemp"))))
          control.Enabled = flag;
      }
    }

    public IEnumerable<Control> GetAll(Control control, HashSet<System.Type> types)
    {
      IEnumerable<Control> controls = control.Controls.Cast<Control>();
      return controls.SelectMany<Control, Control>((Func<Control, IEnumerable<Control>>) (ctrl => this.GetAll(ctrl, types))).Concat<Control>(controls).Where<Control>((Func<Control, bool>) (c => types.Contains(c.GetType())));
    }

    private Printer SelectedPrinter
    {
      get
      {
        if (this.spooler_client != null && !string.IsNullOrEmpty(this.Selected_Printer_Serial))
          return this.spooler_client.GetPrinter(new PrinterSerialNumber(this.Selected_Printer_Serial));
        return (Printer) null;
      }
    }

    private void DoUpdates(object sender, EventArgs e)
    {
      if (this.in_check_printers)
        return;
      this.in_check_printers = true;
      if (this.restart_counter.IsRunning)
      {
        if (this.restart_counter.ElapsedMilliseconds > 10000L)
        {
          this.restart_counter.Stop();
          if (this.job_stopped)
          {
            this.job_stopped = false;
            if (this.message_handler != null)
              this.spooler_client.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, "Print jobs cancelled because your computer went into sleep mode. Please restart your printer by disconnecting it from power and then reconnecting it to power.").Serialize());
          }
          else if (this.message_handler != null)
            this.spooler_client.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, "Your computer went into sleep mode. Please restart your printer by disconnecting it from power and then reconnecting it to power.").Serialize());
        }
        else if (this.restart_counter.ElapsedMilliseconds > 5000L)
          this.spooler_client.DisconnectAllPrinters();
      }
      else
      {
        this.DoFormTask();
        this.RefreshPrinters(false);
      }
      this.in_check_printers = false;
    }

    private ListViewItem FindItem(ListView view, string text)
    {
      for (int index = 0; index < view.Items.Count; ++index)
      {
        if (view.Items[index].Text == text)
          return view.Items[index];
      }
      return (ListViewItem) null;
    }

    private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      if (MainForm.global_form == null)
        return;
      SpoolerClientBuiltIn spoolerClient = MainForm.global_form.spooler_client;
      if (spoolerClient != null)
        return;
      if (e.Mode == PowerModes.Suspend)
      {
        MainForm.global_form.job_stopped = spoolerClient.IsBusy;
        spoolerClient.DisconnectAllPrinters();
      }
      else
      {
        if (e.Mode != PowerModes.Resume)
          return;
        spoolerClient.DisconnectAllPrinters();
        MainForm.global_form.restart_counter.Reset();
        MainForm.global_form.restart_counter.Start();
      }
    }

    protected void ShowSpooler()
    {
      this.ShowInTaskbar = true;
      this.WindowState = FormWindowState.Normal;
      this.ShowInTaskbar = true;
      this.Show();
      this.BringToFront();
      this.Show();
    }

    private void DoFormTask()
    {
      RequestedFormTask requestedFormTask = this.form_task.Value;
      if (requestedFormTask == RequestedFormTask.Shutdown)
      {
        this.force_shutdown = true;
        this.Close();
      }
      else if (requestedFormTask == RequestedFormTask.Hide)
      {
        MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT = true;
        this.m_bShutdownByUser = false;
        this.Close();
      }
      else if (requestedFormTask == RequestedFormTask.Show)
      {
        MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT = false;
        this.ShowSpooler();
      }
      if (requestedFormTask == RequestedFormTask.None)
        return;
      this.form_task.Value = RequestedFormTask.None;
    }

    private void OnReceivedSpoolerShutdownMessage(object sender, EventArgs e)
    {
      this.form_task.Value = RequestedFormTask.Shutdown;
    }

    private void OnReceivedSpoolerShowMessage(object sender, EventArgs e)
    {
      this.form_task.Value = RequestedFormTask.Show;
    }

    private void OnReceivedSpoolerHideMessage(object sender, EventArgs e)
    {
      this.form_task.Value = RequestedFormTask.Hide;
    }

    private void OnGotNewPrinter(Printer new_printer)
    {
      new_printer.RegisterPlugin(SDCardExtensions.ID, (IPrinterPlugin) new SDCardExtensions((IPrinter) new_printer));
      if (this.m_oPrinterData.ContainsKey(new_printer.Info.serial_number))
        return;
      this.m_oPrinterData.TryAdd(new_printer.Info.serial_number, new MainForm.PrinterData());
    }

    private void OnPrinterDisconnected(Printer new_printer)
    {
      if (!this.m_oPrinterData.ContainsKey(new_printer.Info.serial_number))
        return;
      MainForm.PrinterData printerData;
      this.m_oPrinterData.TryRemove(new_printer.Info.serial_number, out printerData);
    }

    private void InitializeTimers()
    {
      this.timerLogger = new Timer(this.components);
      this.timerLogger.Tick += new EventHandler(this.timerLogger_tick);
    }

    private void StartTimers()
    {
      this.timer1.Start();
      this.timerLogger.Start();
    }

    private void StopTimers()
    {
      this.timer1.Stop();
      this.timerLogger.Stop();
    }

    private void OnLog(string message, string printer_serial)
    {
      lock (this.logqueue)
        this.logqueue.Enqueue(string.Format("{0}::{1}", (object) printer_serial, (object) message));
    }

    private void timerLogger_tick(object sender, EventArgs e)
    {
      this.timerLogger.Stop();
      lock (this._myCurrentPrinterObjectSync)
      {
        lock (this.logqueue)
        {
          while (this.logqueue.Count > 0)
            this.richTextBoxLoggedItems.AppendText(this.logqueue.Dequeue() + "\r\n");
        }
        if (this.MyCurrentPrinterObject != null)
        {
          if (this.MyCurrentPrinterObject.LogUpdated)
          {
            if (this.logToScreen.Value)
            {
              List<string> nextLogItems = this.MyCurrentPrinterObject.GetNextLogItems();
              int num = 0;
              foreach (string str in nextLogItems)
              {
                if (this.logWaitsCheckBox.Checked || !(str == ">> wait"))
                {
                  this.richTextBoxLoggedItems.AppendText(str + "\r\n");
                  ++num;
                }
              }
              if (num > 0)
              {
                if (this.checkBoxAutoScroll.Checked)
                {
                  this.richTextBoxLoggedItems.SelectionStart = this.richTextBoxLoggedItems.Text.Length;
                  this.richTextBoxLoggedItems.ScrollToCaret();
                }
              }
            }
          }
        }
      }
      if (this.message_handler != null)
        this.message_handler.ShowMessage();
      this.timerLogger.Start();
    }

    private void richTextBoxLoggedItems_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
        return;
      ContextMenu contextMenu = new ContextMenu();
      MenuItem menuItem1 = new MenuItem("Select All");
      menuItem1.Click += new EventHandler(this.SelectAllAction);
      contextMenu.MenuItems.Add(menuItem1);
      MenuItem menuItem2 = new MenuItem("Copy");
      menuItem2.Click += new EventHandler(this.CopyAction);
      contextMenu.MenuItems.Add(menuItem2);
      this.richTextBoxLoggedItems.ContextMenu = contextMenu;
    }

    private void SelectAllAction(object sender, EventArgs e)
    {
      this.richTextBoxLoggedItems.SelectAll();
    }

    private void CopyAction(object sender, EventArgs e)
    {
      Clipboard.SetText(this.richTextBoxLoggedItems.SelectedText);
    }

    public StayAwakeAndShutdown StayAwakeMethods
    {
      get
      {
        return this.stay_awake;
      }
    }

    private void listViewPrinterInfo_DoubleClick(object sender, EventArgs e)
    {
      if (this.listViewPrinterInfo.SelectedItems.Count <= 0)
        return;
      string text = this.listViewPrinterInfo.SelectedItems[0].Text;
      if (text.StartsWith("00"))
        return;
      this.SetNewPrinter(text);
      this.OpenPrinterControls();
    }

    private void OnSelectedPrintComboBoxChanged(object sender, EventArgs e)
    {
      this.SetNewPrinter(this.selectedPrinterComboBox.Text);
    }

    private void showAdvancedStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!this.printer_controls_open)
        this.OpenPrinterControls();
      else
        this.ClosePrinterControls();
    }

    private void SetNewPrinter(string serial_number)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.Selected_Printer_Serial != serial_number)
        {
          Printer selectedPrinter1 = this.SelectedPrinter;
          if (selectedPrinter1 != null)
          {
            int num1 = (int) selectedPrinter1.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
          }
          this.ResetSDFileList();
          this.Selected_Printer_Serial = serial_number;
          Printer selectedPrinter2 = this.SelectedPrinter;
          if (selectedPrinter2 != null)
          {
            if (this.MyCurrentPrinterObject == null || selectedPrinter2.Info.MySerialNumber != this.MyCurrentPrinterObject.Info.MySerialNumber)
            {
              this.groupBoxPrinterControls.Text = selectedPrinter2.Info.MySerialNumber;
              int num2 = (int) selectedPrinter2.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
              this.MyCurrentPrinterObject = selectedPrinter2;
              this.MyCurrentPrinterObject.LockStepMode = false;
              this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
            }
            this.selectedPrinterComboBox.SelectedIndex = this.selectedPrinterComboBox.Items.IndexOf((object) serial_number);
            this.buttonLock.Enabled = true;
            this.buttonEmergencyStop.Enabled = true;
          }
          else
          {
            this.MyCurrentPrinterObject = (Printer) null;
            this.groupBoxPrinterControls.Text = "00-00-00-00-00-000-000";
            this.selectedPrinterComboBox.SelectedIndex = -1;
            this.buttonLock.Enabled = false;
            this.buttonEmergencyStop.Enabled = false;
          }
        }
        this.m_sampleSetTemperatureSamples.Clear();
      }
    }

    private void buttonStandAlone_Click(object sender, EventArgs e)
    {
    }

    private void OnAutoCheckFirmwareChanged(object sender, EventArgs e)
    {
      this.spooler_client.CheckFirmware = this.checkBoxAutoCheckFirmware.Checked;
    }

    private void buttonSetTemp_Click(object sender, EventArgs e)
    {
      Printer currentPrinterObject = this.MyCurrentPrinterObject;
      if (currentPrinterObject == null)
        return;
      int result;
      if (int.TryParse(this.textBoxTempEdit.Text, out result))
      {
        if ((double) Math.Abs((float) result - this.m_sampleSetTemperatureSamples.SampleMean) > 15.0)
        {
          int num1 = (int) MessageBox.Show("The new temperature must be with 15 degrees of the current temperature. Please try again");
        }
        else if (result < 180)
        {
          int num2 = (int) MessageBox.Show("The new temperature must be over 180 degrees. Please try again");
        }
        else
        {
          int num3 = (int) currentPrinterObject.SetTemperatureWhilePrinting(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) currentPrinterObject, result);
        }
      }
      else
      {
        int num4 = (int) MessageBox.Show("The temperature entered is not an integer. Please try again");
      }
    }

    private void buttonAddJob_Click(object sender, EventArgs e)
    {
      if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
        return;
      this.AddJobFromGCodeFile(false, JobParams.Mode.DirectPrinting);
    }

    private void AddJobFromGCodeFile(bool allow_untethered, JobParams.Mode jobMode)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "G-Code (*.gcode)|*.gcode|Binary G-Code (*.bgcode)|*.bgcode|Text Files (.txt)|*.txt|All Files (*.*)|*.*";
      openFileDialog.FilterIndex = 1;
      openFileDialog.Multiselect = false;
      openFileDialog.CheckFileExists = true;
      if (openFileDialog.ShowDialog() != DialogResult.OK)
        return;
      string fileName = openFileDialog.FileName;
      PrintOptions options = ManualPrintOptions.GetOptions(this.MyCurrentPrinterObject.GetCurrentFilament(), allow_untethered, jobMode);
      bool flag = !options.use_preprocessors;
      bool calibrateZ = options.calibrateZ;
      if (options.type == FilamentSpool.TypeEnum.OtherOrUnknown)
        return;
      Printer currentPrinterObject = this.MyCurrentPrinterObject;
      JobParams jobParams = new JobParams(fileName, Path.GetFileNameWithoutExtension(fileName), "null", options.type, 0.0f, 0.0f);
      jobParams.jobMode = options.jobMode;
      jobParams.options.autostart_ignorewarnings = true;
      jobParams.options.dont_use_preprocessors = flag;
      jobParams.options.calibrate_before_print = calibrateZ;
      if (calibrateZ)
      {
        jobParams.options.calibrate_before_print_z = 0.4f;
        if (this.MyCurrentPrinterObject.Info.calibration.UsesCalibrationOffset)
          jobParams.options.calibrate_before_print_z += this.MyCurrentPrinterObject.Info.calibration.CALIBRATION_OFFSET;
      }
      FilamentProfile filamentProfile = FilamentProfile.CreateFilamentProfile(new FilamentSpool()
      {
        filament_type = options.type,
        filament_temperature = options.temperature
      }, currentPrinterObject.MyPrinterProfile);
      jobParams.preprocessor = filamentProfile.preprocessor;
      jobParams.filament_temperature = filamentProfile.Temperature;
      if (jobMode == JobParams.Mode.SaveToBinaryGCodeFile)
      {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "G-Code (*.gcode)|*.gcode|Binary G-Code (*.bgcode)|*.bgcode";
        saveFileDialog.FilterIndex = 1;
        if (saveFileDialog.ShowDialog() != DialogResult.OK)
          return;
        jobParams.jobMode = jobMode;
        jobParams.outputfile = saveFileDialog.FileName;
      }
      int num = (int) this.MyCurrentPrinterObject.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
    }

    private void comboBoxActivePrinter_TextUpdate(object sender, EventArgs e)
    {
    }

    private void OnLogToScreenChanged(object sender, EventArgs e)
    {
      this.logToScreen.Value = this.checkBoxLogToScreen.Checked;
    }

    private void buttonClearLog_Click(object sender, EventArgs e)
    {
      this.richTextBoxLoggedItems.Clear();
    }

    private void OnEnterInsertGCode(object sender, EventArgs e)
    {
      this.AcceptButton = (IButtonControl) this.buttonSendGCode;
    }

    private void OnLeaveInsertGCode(object sender, EventArgs e)
    {
      this.AcceptButton = (IButtonControl) null;
    }

    private void buttonPreprocess_Click(object sender, EventArgs e)
    {
      float initial_x = 0.0f;
      float initial_y = 0.0f;
      float initial_speed = 0.0f;
      float default_speed = 0.0f;
      float max_speed = 0.0f;
      bool flag = false;
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject != null)
        {
          if (this.MyCurrentPrinterObject.Connected)
          {
            initial_x = this.MyCurrentPrinterObject.Info.calibration.BACKLASH_X;
            initial_y = this.MyCurrentPrinterObject.Info.calibration.BACKLASH_Y;
            initial_speed = this.MyCurrentPrinterObject.Info.calibration.BACKLASH_SPEED;
            default_speed = this.MyCurrentPrinterObject.MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed;
            max_speed = this.MyCurrentPrinterObject.MyPrinterProfile.SpeedLimitConstants.FastestPossible;
            flag = true;
          }
        }
      }
      if (!flag)
        return;
      BacklashSettingsDialog backlashSettingsDialog = new BacklashSettingsDialog(initial_x, initial_y, initial_speed, default_speed, max_speed);
      int num1 = (int) backlashSettingsDialog.ShowDialog();
      if (!backlashSettingsDialog.ok)
        return;
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null)
          return;
        if (!this.MyCurrentPrinterObject.HasLock)
        {
          int num2 = (int) MessageBox.Show("Unable to send command to the printer because the lock on the printer has timed out.");
        }
        else if (this.MyCurrentPrinterObject.Connected)
        {
          int num3 = (int) this.MyCurrentPrinterObject.SetBacklash((M3D.Spooling.Client.AsyncCallback) null, (object) null, new BacklashSettings(backlashSettingsDialog.X_BACKLASH, backlashSettingsDialog.Y_BACKLASH, backlashSettingsDialog.BACKLASH_SPEED));
        }
        else
        {
          int num4 = (int) MessageBox.Show("Unable to send command to the printer because the printer has disconnected.");
        }
      }
    }

    private void buttonPrintToFile_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        this.AddJobFromGCodeFile(false, JobParams.Mode.SaveToBinaryGCodeFile);
      }
    }

    private void buttonBacklashPrint_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.PrintBacklashPrint(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
      }
    }

    private void buttonGoToBootloader_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null)
          return;
        int num = this.MyCurrentPrinterObject.Connected ? 1 : 0;
      }
    }

    private void buttonQuitBootloader_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "Q");
      }
    }

    private void buttonUpdateFirmware_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
        int num = (int) this.MyCurrentPrinterObject.DoFirmwareUpdate((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        this.SetNewPrinter((string) null);
      }
    }

    private void ProcessCallErrors(IAsyncCallResult ar)
    {
      Printer asyncState = ar.AsyncState as Printer;
      if (asyncState == null)
        return;
      switch (ar.CallResult)
      {
        case CommandResult.Success:
          break;
        case CommandResult.SuccessfullyReceived:
          break;
        case CommandResult.OverridedByNonLockStepCall:
          if (this.spooler_client.ClientCount != 0)
            break;
          this.OnLog("The previous command is still running, so your request has been queued.", asyncState.Info.MySerialNumber);
          break;
        case CommandResult.Failed_CannotPauseSavingToSD:
          int num1 = (int) MessageBox.Show("Sorry, but saving to the printer can't be paused.");
          break;
        case CommandResult.Failed_G28_G30_G32_NotAllowedWhilePaused:
          if (this.spooler_client.ClientCount != 0)
            break;
          int num2 = (int) MessageBox.Show("Homing and calibration are disabled while paused.");
          break;
        case CommandResult.CommandInterruptedByM0:
          if (this.spooler_client.ClientCount != 0)
            break;
          this.OnLog("The previous command was interrupted.", asyncState.Info.MySerialNumber);
          break;
        default:
          this.OnLog(ar.CallResult.ToString(), asyncState.Info.MySerialNumber);
          break;
      }
    }

    private void buttonSendGCode_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected || string.IsNullOrEmpty(this.textBoxManualGCode.Text))
          return;
        if (this.textBoxManualGCode.Text.IndexOf("backlash") == 0)
        {
          int num1 = (int) this.MyCurrentPrinterObject.PrintBacklashPrint(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
        }
        else
        {
          this.checkBoxLogToScreen.Checked = true;
          this.lastManualCommand = this.textBoxManualGCode.Text;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, this.textBoxManualGCode.Text);
          this.listBoxManualHistory.Items.Add((object) ("(" + this.MyCurrentPrinterObject.Info.MySerialNumber + ")->" + this.textBoxManualGCode.Text));
          this.textBoxManualGCode.Text = "";
        }
      }
    }

    private void buttonEmergencyStop_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendEmergencyStop(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
        this.listBoxManualHistory.Items.Add((object) ("(" + this.MyCurrentPrinterObject.Info.MySerialNumber + ")->Emergency Stop"));
      }
    }

    private void buttonPausePrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
        return;
      int num = (int) selectedPrinter.PausePrint(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
      this.listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Pause Print"));
    }

    private void buttonResumePrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
        return;
      int num = (int) selectedPrinter.ContinuePrint(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
      this.listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Resume Print"));
    }

    private void buttonAbortPrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected || MessageBox.Show("Are you sure you want to abort this print?", "M3D Print Spooler", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      int num = (int) selectedPrinter.AbortPrint(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject);
      this.listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Abort Print"));
    }

    private void buttonUpZ_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxZVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the Z value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxZVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Z{1}", (object) 90f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpY_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxYVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the Y value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxYVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Y{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownX_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxXVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the X value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxXVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} X-{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpX_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxXVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the X value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxXVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} X{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownY_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxYVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the Y value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxYVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Y-{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownZ_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxZVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the Z value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxZVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Z-{1}", (object) 90f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownE_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxEVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the E value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxEVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} E-{1}", (object) 345f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpE_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(this.textBoxEVal.Text))
      {
        int num1 = (int) MessageBox.Show("Sorry, but the E value is not a number");
      }
      else
      {
        float floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(this.textBoxEVal.Text);
        lock (this._myCurrentPrinterObjectSync)
        {
          if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
            return;
          int num2 = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} E{1}", (object) 345f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonABSTemp_Click(object sender, EventArgs e)
    {
      this.SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.ABS));
    }

    private void buttonTGHTemp_Click(object sender, EventArgs e)
    {
      this.SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.TGH));
    }

    private void buttonPLATemp_Click(object sender, EventArgs e)
    {
      this.SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.PLA));
    }

    private void buttonHeaterOff_Click(object sender, EventArgs e)
    {
      this.SetTemperature(0);
    }

    private void SetTemperature(int temperature)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, PrinterCompatibleString.Format("M109 S{0}", (object) temperature));
      }
    }

    private void buttonBedHeatToPLA_Click(object sender, EventArgs e)
    {
      this.SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.PLA));
    }

    private void buttonBedHeatToABS_Click(object sender, EventArgs e)
    {
      this.SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.ABS));
    }

    private void buttonBedHeatToABR_Click(object sender, EventArgs e)
    {
      this.SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.ABS_R));
    }

    private void buttonTurnOfHeatedbed_Click(object sender, EventArgs e)
    {
      this.SetBedTemperature(0);
    }

    private void SetBedTemperature(int temperature)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, PrinterCompatibleString.Format("M190 S{0}", (object) temperature));
      }
    }

    private void buttonMotorsOn_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "M17");
      }
    }

    private void buttonMotorsOff_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "M18");
      }
    }

    private void buttonFanOn_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "M106 S255");
      }
    }

    private void buttonFanOff_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "M106 S0");
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G90");
      }
    }

    private void buttonRepeatLast_Click(object sender, EventArgs e)
    {
      this.checkBoxLogToScreen.Checked = true;
      this.textBoxManualGCode.Text = this.lastManualCommand;
      this.buttonSendGCode_Click(sender, e);
    }

    private void buttonChangeFilamentInfo_Click(object sender, EventArgs e)
    {
    }

    private void buttonLoadFilament_Click(object sender, EventArgs e)
    {
    }

    private void buttonEjectFilament_Click(object sender, EventArgs e)
    {
    }

    private void button2_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "G91");
      }
    }

    private void buttonXSkipTest_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateXSkipTestMinus(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSkipTest_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateYSkipTestMinus(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonXSkipTestRight_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateXSkipTestPlus(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSkipTestBack_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateYSkipTestPlus(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonXSpeedTest_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateXSpeedTest(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSpeedTest_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.CreateYSpeedTest(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonHome_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "G28");
      }
    }

    private void buttonReCenter_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, TestGeneration.FastRecenter(this.MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void OnPrinterLockCallback(IAsyncCallResult ar)
    {
      bool flag = this.PrinterLocked.Value;
      if (ar.CallResult == CommandResult.Success_LockAcquired && !flag)
      {
        this.LockChanging.Value = MainForm.PrinterLockChanging.YesToLocked;
        SDCardExtensions sdPluginExtension = this.GetCurrentSDPluginExtension();
        if (sdPluginExtension == null || !sdPluginExtension.Available)
          return;
        sdPluginExtension.OnReceivedFileList = new EventHandler(this.ReceivedUpdatedSDCardList);
        if (!this.m_bSDTabSelected)
          return;
        int num = (int) sdPluginExtension.RefreshSDCardList((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      else if (ar.CallResult == CommandResult.Success_LockReleased & flag)
        this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      else if (ar.CallResult == CommandResult.Failed_PrinterDoesNotHaveLock)
      {
        if (!flag)
          return;
        this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      }
      else if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        int num = (int) MessageBox.Show("Can not lock this printer because it is being used by another program.");
        this.LockChanging.Value = MainForm.PrinterLockChanging.NoFailed;
      }
      else
      {
        if (ar.CallResult != CommandResult.Failed_CannotLockWhilePrinting)
          return;
        int num = (int) MessageBox.Show("Can not lock this printer because it is printing.");
        this.LockChanging.Value = MainForm.PrinterLockChanging.NoFailed;
      }
    }

    private void buttonGetGCode_Click(object sender, EventArgs e)
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "M3D", "Spooler", "queue");
      string destFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "M3D - Last used G-code.gcode");
      try
      {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo fileInfo = (FileInfo) null;
        string searchPattern = "*_processed.gcode";
        foreach (FileInfo file in directoryInfo.GetFiles(searchPattern))
        {
          if (fileInfo == null)
            fileInfo = file;
          else if (fileInfo.CreationTimeUtc < file.CreationTimeUtc)
            fileInfo = file;
        }
        if (fileInfo != null)
        {
          File.Copy(fileInfo.FullName, destFileName, true);
          int num = (int) MessageBox.Show("Successfully moved \"M3D - Last used G-code.gcode\" to the Desktop", "Info");
        }
        else
        {
          int num1 = (int) MessageBox.Show("Sorry, no gcode files could be found", "Info");
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Sorry, there was a problem copying the gcode to the Desktop", "Info");
      }
    }

    private void buttonBedPointTest_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        PrinterInfo info = this.MyCurrentPrinterObject.Info;
        if (info == null)
          return;
        BedCompensationPreprocessor compensationPreprocessor = new BedCompensationPreprocessor();
        compensationPreprocessor.UpdateConfigurations(info.calibration, this.MyCurrentPrinterObject.MyPrinterProfile.PrinterSizeConstants);
        string selectedItem = (string) this.comboBoxBedTestPoint.SelectedItem;
        Vector vector;
        if (!(selectedItem == "Center"))
        {
          if (!(selectedItem == "Front Right"))
          {
            if (!(selectedItem == "Front Left"))
            {
              if (!(selectedItem == "Back Right"))
              {
                if (!(selectedItem == "Back Left"))
                  return;
                vector = compensationPreprocessor.BackLeft;
              }
              else
                vector = compensationPreprocessor.BackRight;
            }
            else
              vector = compensationPreprocessor.FrontLeft;
          }
          else
            vector = compensationPreprocessor.FrontRight;
        }
        else
          vector = compensationPreprocessor.Center;
        vector.z = 0.1f + compensationPreprocessor.GetHeightAdjustmentRequired(vector.x, vector.y) + compensationPreprocessor.entire_z_height_offset;
        int num = (int) this.MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.ProcessCallErrors), (object) this.MyCurrentPrinterObject, "M1012", "G91", PrinterCompatibleString.Format("G0 Z{0} F100", (object) 2), "G90", PrinterCompatibleString.Format("G0 X{0} Y{1} Z{2} F3000", (object) vector.x, (object) vector.y, (object) 5f), PrinterCompatibleString.Format("G0 Z{0} F100", (object) vector.z), "M1011");
      }
    }

    private void buttonLock_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        this.buttonLock.Enabled = false;
        if (!this.PrinterLocked.Value)
        {
          this.MyCurrentPrinterObject.LockStepMode = false;
          int num1 = (int) this.MyCurrentPrinterObject.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.OnPrinterLockCallback), (object) this.MyCurrentPrinterObject, new EventLockTimeOutCallBack(this.OnLockTimedOut), 120);
          if (this.settings.DoNotShowPrinterLockOutWarning)
            return;
          PrinterLockWarning printerLockWarning = new PrinterLockWarning();
          int num2 = (int) printerLockWarning.ShowDialog((IWin32Window) this);
          this.settings.DoNotShowPrinterLockOutWarning = printerLockWarning.DoNotShowAgain;
          if (!printerLockWarning.DoNotShowAgain)
            return;
          SpoolerSettings.SaveSettings(this.settings, this.shared_shutdown.Value);
        }
        else
        {
          this.MyCurrentPrinterObject.LockStepMode = true;
          int num = (int) this.MyCurrentPrinterObject.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(this.OnPrinterLockCallback), (object) this.MyCurrentPrinterObject);
        }
      }
    }

    private void OnLockTimedOut(IPrinter printer)
    {
      this.OnLog(" >> The lock on the printer has timed out", printer.Info.MySerialNumber);
      if (!this.PrinterLocked.Value)
        return;
      this.LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
    }

    private void buttonSaveEepromData_Click(object sender, EventArgs e)
    {
      lock (this._myCurrentPrinterObjectSync)
      {
        if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
          return;
        PrinterInfo info = this.MyCurrentPrinterObject.Info;
        if (info == null)
          return;
        PublicPrinterConnection printerConnection = this.spooler_client.UnsafeFindPrinterConnection(info.serial_number);
        if (printerConnection == null)
        {
          int num1 = (int) MessageBox.Show((IWin32Window) this, "Unable to modify PrinterConnection", "M3D Print Spooler");
        }
        else
        {
          SaveFileDialog saveFileDialog = new SaveFileDialog();
          saveFileDialog.Filter = "EEPROM XML Files (.EEPROM.xml)|*.EEPROM.xml|All Files (*.*)|*.*";
          saveFileDialog.FilterIndex = 1;
          saveFileDialog.FileName = info.serial_number.ToString() + ".EEPROM.xml";
          if (saveFileDialog.ShowDialog() != DialogResult.OK)
            return;
          switch (printerConnection.SaveEEPROMDataToXMLFile(saveFileDialog.FileName))
          {
            case CommandResult.Success:
              int num2 = (int) MessageBox.Show("EEPROM Data Saved to XML file.", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
              break;
            case CommandResult.Failed_NotInFirmware:
              int num3 = (int) MessageBox.Show("Printer must be in firmware mode", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Hand);
              break;
            case CommandResult.Failed_Argument:
              int num4 = (int) MessageBox.Show("Unable to save xml file.", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Hand);
              break;
          }
        }
      }
    }

    private void buttonLoadEepromData_Click(object sender, EventArgs e)
    {
    }

    private void buttonSDRefresh_Click(object sender, EventArgs e)
    {
      SDCardExtensions sdPluginExtension = this.GetCurrentSDPluginExtension();
      if (sdPluginExtension == null || !sdPluginExtension.Available)
        return;
      sdPluginExtension.OnReceivedFileList = new EventHandler(this.ReceivedUpdatedSDCardList);
      int num = (int) sdPluginExtension.RefreshSDCardList((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private void buttonSDSaveGcode_Click(object sender, EventArgs e)
    {
      if (this.MyCurrentPrinterObject == null || !this.MyCurrentPrinterObject.Connected)
        return;
      this.AddJobFromGCodeFile(true, JobParams.Mode.SavingToSDCard);
    }

    private void buttonSDPrint_Click(object sender, EventArgs e)
    {
      if (this.listBoxSDFiles.SelectedIndex < 0)
      {
        int num1 = (int) MessageBox.Show("Please select a file on your 3D printer to print.");
      }
      else
      {
        try
        {
          string gcodefile = this.listBoxSDFiles.Items[this.listBoxSDFiles.SelectedIndex].ToString();
          if (this.MyCurrentPrinterObject != null && this.MyCurrentPrinterObject.Connected)
          {
            Printer currentPrinterObject = this.MyCurrentPrinterObject;
            JobParams jobParams = new JobParams(gcodefile, "Spooler Inserted Job", "null", FilamentSpool.TypeEnum.OtherOrUnknown, 0.0f, 0.0f);
            jobParams.jobMode = JobParams.Mode.FirmwarePrintingFromSDCard;
            jobParams.options.autostart_ignorewarnings = true;
            jobParams.options.dont_use_preprocessors = true;
            if (this.checkBoxCalibrateBeforeSDPrint.Checked)
            {
              jobParams.options.calibrate_before_print = true;
              jobParams.options.calibrate_before_print_z = 0.4f;
              if (this.MyCurrentPrinterObject.Info.calibration.UsesCalibrationOffset)
                jobParams.options.calibrate_before_print_z += this.MyCurrentPrinterObject.Info.calibration.CALIBRATION_OFFSET;
            }
            FilamentSpool filamentSpool = new FilamentSpool()
            {
              filament_type = FilamentSpool.TypeEnum.OtherOrUnknown,
              filament_temperature = 0
            };
            jobParams.preprocessor = (FilamentPreprocessorData) null;
            jobParams.filament_temperature = 0;
            int num2 = (int) this.MyCurrentPrinterObject.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
          }
          else
          {
            int num3 = (int) MessageBox.Show("Sorry but the printer has disconnected.");
          }
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show("There was an error selecting the file. Please try again.");
        }
      }
    }

    private void buttonSDDelete_Click(object sender, EventArgs e)
    {
      if (this.listBoxSDFiles.SelectedIndex < 0)
      {
        int num1 = (int) MessageBox.Show("Please select a file on your 3D printer to print.");
      }
      else
      {
        try
        {
          string filename = this.listBoxSDFiles.Items[this.listBoxSDFiles.SelectedIndex].ToString();
          SDCardExtensions sdPluginExtension = this.GetCurrentSDPluginExtension();
          if (sdPluginExtension == null || !sdPluginExtension.Available)
            return;
          int num2 = (int) sdPluginExtension.DeleteFileFromSDCard(new M3D.Spooling.Client.AsyncCallback(this.RefreshAfterCommand), (object) sdPluginExtension, filename);
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void RefreshAfterCommand(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
        return;
      this.buttonSDRefresh_Click((object) null, (EventArgs) null);
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      int num = (int) new About(this.spooler_client.ConnectedSpoolerInfo).ShowDialog((IWin32Window) this);
    }

    private void AdvancedOptionsAtStartup(object sender, EventArgs e)
    {
      bool flag = !this.showAdvancedOptionsAtStartupToolStripMenuItem.Checked;
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Checked = flag;
      this.settings.StartAdvanced = flag;
      SpoolerSettings.SaveSettings(this.settings, this.shared_shutdown.Value);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.tabPageSDCard == this.tabControl1.SelectedTab)
      {
        this.m_bSDTabSelected = true;
        Printer currentPrinterObject = this.MyCurrentPrinterObject;
        if (currentPrinterObject == null)
          return;
        (currentPrinterObject.GetPrinterPlugin(SDCardExtensions.ID) as SDCardExtensions).OnReceivedFileList = new EventHandler(this.ReceivedUpdatedSDCardList);
        int num = (int) currentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "M20");
      }
      else
        this.m_bSDTabSelected = false;
    }

    private void ResetSDFileList()
    {
      this.listBoxSDFiles.Items.Clear();
    }

    private void ReceivedUpdatedSDCardList(object sender, EventArgs args)
    {
      SDCardExtensions sdplugin = this.GetCurrentSDPluginExtension();
      if (sdplugin == null || sdplugin != sender as SDCardExtensions)
        return;
      this.BeginInvoke((Action) (() =>
      {
        this.listBoxSDFiles.Items.Clear();
        foreach (object sdCardFile in sdplugin.GetSDCardFileList())
          this.listBoxSDFiles.Items.Add(sdCardFile);
      }));
    }

    private SDCardExtensions GetCurrentSDPluginExtension()
    {
      Printer currentPrinterObject = this.MyCurrentPrinterObject;
      if (currentPrinterObject != null)
        return currentPrinterObject.GetPrinterPlugin(SDCardExtensions.ID) as SDCardExtensions;
      return (SDCardExtensions) null;
    }

    private void showAdvancedStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      bool flag = !this.showAdvancedStatisticsToolStripMenuItem.Checked;
      this.showAdvancedStatisticsToolStripMenuItem.Checked = flag;
      if (flag)
        this.advancedStatisticsDialog.Show((IWin32Window) this);
      else
        this.advancedStatisticsDialog.Hide();
    }

    protected override void Dispose(bool bIsDisposing)
    {
      if (bIsDisposing && this.components != null)
        this.components.Dispose();
      base.Dispose(bIsDisposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (MainForm));
      this.timer1 = new Timer(this.components);
      this.menuStrip1 = new MenuStrip();
      this.viewToolStripMenuItem = new ToolStripMenuItem();
      this.showAdvancedToolStripMenuItem = new ToolStripMenuItem();
      this.showAdvancedStatisticsToolStripMenuItem = new ToolStripMenuItem();
      this.optionsToolStripMenuItem = new ToolStripMenuItem();
      this.showAdvancedOptionsAtStartupToolStripMenuItem = new ToolStripMenuItem();
      this.helpToolStripMenuItem = new ToolStripMenuItem();
      this.aboutToolStripMenuItem = new ToolStripMenuItem();
      this.groupBoxPrinterList = new GroupBox();
      this.buttonStandAlone = new Button();
      this.checkBoxAutoCheckFirmware = new CheckBox();
      this.listViewPrinterInfo = new ListView();
      this.columnSerialNumber = new ColumnHeader();
      this.columnType = new ColumnHeader();
      this.columnStatus = new ColumnHeader();
      this.columnTemp = new ColumnHeader();
      this.columnBedTemp = new ColumnHeader();
      this.columnZValid = new ColumnHeader();
      this.columnJobStatus = new ColumnHeader();
      this.columnFile = new ColumnHeader();
      this.columnPerComplete = new ColumnHeader();
      this.columnUser = new ColumnHeader();
      this.labelSpoolerVersion = new Label();
      this.groupBoxPrinterControls = new GroupBox();
      this.logWaitsCheckBox = new CheckBox();
      this.buttonAbortPrint = new Button();
      this.buttonResumePrint = new Button();
      this.buttonPausePrint = new Button();
      this.label1 = new Label();
      this.selectedPrinterComboBox = new ComboBox();
      this.buttonLock = new Button();
      this.groupBoxPrinting = new GroupBox();
      this.groupBox2 = new GroupBox();
      this.buttonSetTemp = new Button();
      this.textBoxTempEdit = new TextBox();
      this.label8 = new Label();
      this.label7 = new Label();
      this.groupBox1 = new GroupBox();
      this.textBoxMeanTemperature = new Label();
      this.groupBoxControls = new GroupBox();
      this.groupBoxBootloaderOptions = new GroupBox();
      this.label2 = new Label();
      this.buttonQuitBootloader = new Button();
      this.button3 = new Button();
      this.groupBoxFirmwareControls = new GroupBox();
      this.tabControl1 = new TabControl();
      this.tabPageBasicOptions = new TabPage();
      this.buttonTGHTemp = new Button();
      this.buttonHeaterOff = new Button();
      this.buttonFanOff = new Button();
      this.buttonFanOn = new Button();
      this.textBoxEVal = new TextBox();
      this.buttonPLATemp = new Button();
      this.buttonDownE = new Button();
      this.buttonUpE = new Button();
      this.buttonABSTemp = new Button();
      this.buttonAddJob = new Button();
      this.buttonMotorsOff = new Button();
      this.buttonMotorsOn = new Button();
      this.textBoxYVal = new TextBox();
      this.textBoxXVal = new TextBox();
      this.buttonPrintToFile = new Button();
      this.textBoxZVal = new TextBox();
      this.button2 = new Button();
      this.buttonUpX = new Button();
      this.button1 = new Button();
      this.buttonDownX = new Button();
      this.buttonFilamentInfo = new Button();
      this.buttonDownZ = new Button();
      this.buttonDownY = new Button();
      this.buttonUpZ = new Button();
      this.buttonUpY = new Button();
      this.tabPageDiagnostics = new TabPage();
      this.buttonLoadEepromData = new Button();
      this.buttonSaveEepromData = new Button();
      this.buttonUpdateFirmware = new Button();
      this.buttonPreprocess = new Button();
      this.groupBox4 = new GroupBox();
      this.buttonBedPointTest = new Button();
      this.comboBoxBedTestPoint = new ComboBox();
      this.buttonBacklashPrint = new Button();
      this.buttonGetGCode = new Button();
      this.buttonReCenter = new Button();
      this.buttonHome = new Button();
      this.buttonSpeedTest = new Button();
      this.buttonXSpeedTest = new Button();
      this.buttonYSkipTestBack = new Button();
      this.buttonXSkipTestRight = new Button();
      this.buttonYSkipTest = new Button();
      this.buttonXSkipTest = new Button();
      this.tabPageSDCard = new TabPage();
      this.checkBoxCalibrateBeforeSDPrint = new CheckBox();
      this.buttonSDDeleteFile = new Button();
      this.buttonSDPrint = new Button();
      this.buttonSDSaveGcode = new Button();
      this.buttonSDRefresh = new Button();
      this.listBoxSDFiles = new ListBox();
      this.label6 = new Label();
      this.tabPageHeatedBedControl = new TabPage();
      this.buttonTurnOfHeatedbed = new Button();
      this.buttonBedHeatToABR = new Button();
      this.buttonBedHeatToABS = new Button();
      this.buttonBedHeatToPLA = new Button();
      this.buttonRepeatLast = new Button();
      this.label3 = new Label();
      this.textBoxManualGCode = new TextBox();
      this.buttonSendGCode = new Button();
      this.buttonClearLog = new Button();
      this.checkBoxLogToScreen = new CheckBox();
      this.richTextBoxLoggedItems = new RichTextBox();
      this.buttonEmergencyStop = new Button();
      this.label5 = new Label();
      this.listBoxManualHistory = new ListBox();
      this.label4 = new Label();
      this.checkBoxAutoScroll = new CheckBox();
      this.groupBox3 = new GroupBox();
      this.menuStrip1.SuspendLayout();
      this.groupBoxPrinterList.SuspendLayout();
      this.groupBoxPrinterControls.SuspendLayout();
      this.groupBoxPrinting.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBoxControls.SuspendLayout();
      this.groupBoxBootloaderOptions.SuspendLayout();
      this.groupBoxFirmwareControls.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPageBasicOptions.SuspendLayout();
      this.tabPageDiagnostics.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.tabPageSDCard.SuspendLayout();
      this.tabPageHeatedBedControl.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      this.timer1.Interval = 300;
      this.timer1.Tick += new EventHandler(this.DoUpdates);
      this.menuStrip1.Items.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.viewToolStripMenuItem,
        (ToolStripItem) this.optionsToolStripMenuItem,
        (ToolStripItem) this.helpToolStripMenuItem
      });
      this.menuStrip1.Location = new Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new Size(1040, 24);
      this.menuStrip1.TabIndex = 49;
      this.menuStrip1.Text = "menuStrip1";
      this.viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.showAdvancedToolStripMenuItem,
        (ToolStripItem) this.showAdvancedStatisticsToolStripMenuItem
      });
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new Size(45, 20);
      this.viewToolStripMenuItem.Text = "View";
      this.showAdvancedToolStripMenuItem.Name = "showAdvancedToolStripMenuItem";
      this.showAdvancedToolStripMenuItem.Size = new Size(212, 22);
      this.showAdvancedToolStripMenuItem.Text = "Show Advanced Options";
      this.showAdvancedToolStripMenuItem.Click += new EventHandler(this.showAdvancedStripMenuItem_Click);
      this.showAdvancedStatisticsToolStripMenuItem.Name = "showAdvancedStatisticsToolStripMenuItem";
      this.showAdvancedStatisticsToolStripMenuItem.Size = new Size(212, 22);
      this.showAdvancedStatisticsToolStripMenuItem.Text = "Show Advanced Statistics";
      this.showAdvancedStatisticsToolStripMenuItem.Click += new EventHandler(this.showAdvancedStatisticsToolStripMenuItem_Click);
      this.optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.showAdvancedOptionsAtStartupToolStripMenuItem
      });
      this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
      this.optionsToolStripMenuItem.Size = new Size(61, 20);
      this.optionsToolStripMenuItem.Text = "Options";
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Name = "showAdvancedOptionsAtStartupToolStripMenuItem";
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Size = new Size(264, 22);
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Text = "Show Advanced Options at Startup";
      this.showAdvancedOptionsAtStartupToolStripMenuItem.Click += new EventHandler(this.AdvancedOptionsAtStartup);
      this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.aboutToolStripMenuItem
      });
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new Size(44, 20);
      this.helpToolStripMenuItem.Text = "Help";
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Size = new Size(107, 22);
      this.aboutToolStripMenuItem.Text = "About";
      this.aboutToolStripMenuItem.Click += new EventHandler(this.aboutToolStripMenuItem_Click);
      this.groupBoxPrinterList.Controls.Add((Control) this.buttonStandAlone);
      this.groupBoxPrinterList.Controls.Add((Control) this.checkBoxAutoCheckFirmware);
      this.groupBoxPrinterList.Controls.Add((Control) this.listViewPrinterInfo);
      this.groupBoxPrinterList.Controls.Add((Control) this.labelSpoolerVersion);
      this.groupBoxPrinterList.Location = new Point(1, 420);
      this.groupBoxPrinterList.Name = "groupBoxPrinterList";
      this.groupBoxPrinterList.Size = new Size(1039, 124);
      this.groupBoxPrinterList.TabIndex = 50;
      this.groupBoxPrinterList.TabStop = false;
      this.buttonStandAlone.Location = new Point(208, 98);
      this.buttonStandAlone.Name = "buttonStandAlone";
      this.buttonStandAlone.Size = new Size(153, 18);
      this.buttonStandAlone.TabIndex = 51;
      this.buttonStandAlone.Text = "Make Stand-alone";
      this.buttonStandAlone.UseVisualStyleBackColor = true;
      this.buttonStandAlone.Click += new EventHandler(this.buttonStandAlone_Click);
      this.checkBoxAutoCheckFirmware.AutoSize = true;
      this.checkBoxAutoCheckFirmware.Checked = true;
      this.checkBoxAutoCheckFirmware.CheckState = CheckState.Checked;
      this.checkBoxAutoCheckFirmware.Location = new Point(15, 101);
      this.checkBoxAutoCheckFirmware.Name = "checkBoxAutoCheckFirmware";
      this.checkBoxAutoCheckFirmware.Size = new Size(146, 16);
      this.checkBoxAutoCheckFirmware.TabIndex = 50;
      this.checkBoxAutoCheckFirmware.Text = "Auto-check Firmware";
      this.checkBoxAutoCheckFirmware.UseVisualStyleBackColor = true;
      this.checkBoxAutoCheckFirmware.CheckedChanged += new EventHandler(this.OnAutoCheckFirmwareChanged);
      this.listViewPrinterInfo.Activation = ItemActivation.OneClick;
      this.listViewPrinterInfo.Columns.AddRange(new ColumnHeader[10]
      {
        this.columnSerialNumber,
        this.columnType,
        this.columnStatus,
        this.columnTemp,
        this.columnBedTemp,
        this.columnZValid,
        this.columnJobStatus,
        this.columnFile,
        this.columnPerComplete,
        this.columnUser
      });
      this.listViewPrinterInfo.FullRowSelect = true;
      this.listViewPrinterInfo.HideSelection = false;
      this.listViewPrinterInfo.LabelWrap = false;
      this.listViewPrinterInfo.Location = new Point(9, 12);
      this.listViewPrinterInfo.MultiSelect = false;
      this.listViewPrinterInfo.Name = "listViewPrinterInfo";
      this.listViewPrinterInfo.Size = new Size(1018, 80);
      this.listViewPrinterInfo.TabIndex = 49;
      this.listViewPrinterInfo.UseCompatibleStateImageBehavior = false;
      this.listViewPrinterInfo.View = View.Details;
      this.listViewPrinterInfo.DoubleClick += new EventHandler(this.listViewPrinterInfo_DoubleClick);
      this.columnSerialNumber.Text = "Serial Number";
      this.columnSerialNumber.Width = 149;
      this.columnType.Text = "Type";
      this.columnStatus.Text = "Status";
      this.columnStatus.Width = 213;
      this.columnTemp.Text = "Temp. (C)";
      this.columnTemp.Width = 72;
      this.columnBedTemp.Text = "Bed Temp. (C)";
      this.columnBedTemp.Width = 99;
      this.columnZValid.Text = "Z Valid";
      this.columnZValid.Width = 53;
      this.columnJobStatus.Text = "Job Status";
      this.columnJobStatus.Width = 81;
      this.columnFile.Text = "File";
      this.columnFile.Width = 109;
      this.columnPerComplete.Text = "% Done";
      this.columnPerComplete.Width = 58;
      this.columnUser.Text = "User";
      this.columnUser.Width = 117;
      this.labelSpoolerVersion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.labelSpoolerVersion.AutoSize = true;
      this.labelSpoolerVersion.Location = new Point(731, 96);
      this.labelSpoolerVersion.Name = "labelSpoolerVersion";
      this.labelSpoolerVersion.Size = new Size(181, 12);
      this.labelSpoolerVersion.TabIndex = 53;
      this.labelSpoolerVersion.Text = "Spooler Version: ????-??-??-??";
      this.groupBoxPrinterControls.Controls.Add((Control) this.logWaitsCheckBox);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonAbortPrint);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonResumePrint);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonPausePrint);
      this.groupBoxPrinterControls.Controls.Add((Control) this.label1);
      this.groupBoxPrinterControls.Controls.Add((Control) this.selectedPrinterComboBox);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonLock);
      this.groupBoxPrinterControls.Controls.Add((Control) this.groupBoxPrinting);
      this.groupBoxPrinterControls.Controls.Add((Control) this.groupBoxControls);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonClearLog);
      this.groupBoxPrinterControls.Controls.Add((Control) this.checkBoxLogToScreen);
      this.groupBoxPrinterControls.Controls.Add((Control) this.richTextBoxLoggedItems);
      this.groupBoxPrinterControls.Controls.Add((Control) this.buttonEmergencyStop);
      this.groupBoxPrinterControls.Controls.Add((Control) this.label5);
      this.groupBoxPrinterControls.Controls.Add((Control) this.listBoxManualHistory);
      this.groupBoxPrinterControls.Controls.Add((Control) this.label4);
      this.groupBoxPrinterControls.Controls.Add((Control) this.checkBoxAutoScroll);
      this.groupBoxPrinterControls.Location = new Point(10, 25);
      this.groupBoxPrinterControls.Name = "groupBoxPrinterControls";
      this.groupBoxPrinterControls.Size = new Size(1018, 393);
      this.groupBoxPrinterControls.TabIndex = 51;
      this.groupBoxPrinterControls.TabStop = false;
      this.groupBoxPrinterControls.Text = "00-00-00-00-00-000-000";
      this.logWaitsCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.logWaitsCheckBox.AutoSize = true;
      this.logWaitsCheckBox.Location = new Point(129, 278);
      this.logWaitsCheckBox.Name = "logWaitsCheckBox";
      this.logWaitsCheckBox.Size = new Size(79, 16);
      this.logWaitsCheckBox.TabIndex = 51;
      this.logWaitsCheckBox.Text = "Log Waits";
      this.logWaitsCheckBox.UseVisualStyleBackColor = true;
      this.buttonAbortPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonAbortPrint.BackColor = SystemColors.Control;
      this.buttonAbortPrint.ForeColor = System.Drawing.Color.Black;
      this.buttonAbortPrint.Location = new Point(6, 85);
      this.buttonAbortPrint.Name = "buttonAbortPrint";
      this.buttonAbortPrint.Size = new Size(184, 30);
      this.buttonAbortPrint.TabIndex = 50;
      this.buttonAbortPrint.Text = "Abort Print";
      this.buttonAbortPrint.UseVisualStyleBackColor = false;
      this.buttonAbortPrint.Click += new EventHandler(this.buttonAbortPrint_Click);
      this.buttonResumePrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonResumePrint.BackColor = SystemColors.Control;
      this.buttonResumePrint.ForeColor = SystemColors.ControlText;
      this.buttonResumePrint.Location = new Point(199, 85);
      this.buttonResumePrint.Name = "buttonResumePrint";
      this.buttonResumePrint.Size = new Size(184, 30);
      this.buttonResumePrint.TabIndex = 49;
      this.buttonResumePrint.Text = "Resume";
      this.buttonResumePrint.UseVisualStyleBackColor = false;
      this.buttonResumePrint.Visible = false;
      this.buttonResumePrint.Click += new EventHandler(this.buttonResumePrint_Click);
      this.buttonPausePrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonPausePrint.BackColor = SystemColors.Control;
      this.buttonPausePrint.ForeColor = SystemColors.ControlText;
      this.buttonPausePrint.Location = new Point(199, 85);
      this.buttonPausePrint.Name = "buttonPausePrint";
      this.buttonPausePrint.Size = new Size(184, 30);
      this.buttonPausePrint.TabIndex = 48;
      this.buttonPausePrint.Text = "Pause";
      this.buttonPausePrint.UseVisualStyleBackColor = false;
      this.buttonPausePrint.Click += new EventHandler(this.buttonPausePrint_Click);
      this.label1.AutoSize = true;
      this.label1.Location = new Point(10, 21);
      this.label1.Name = "label1";
      this.label1.Size = new Size(90, 12);
      this.label1.TabIndex = 47;
      this.label1.Text = "Current Printer:";
      this.selectedPrinterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      this.selectedPrinterComboBox.FormattingEnabled = true;
      this.selectedPrinterComboBox.Location = new Point(103, 18);
      this.selectedPrinterComboBox.Name = "selectedPrinterComboBox";
      this.selectedPrinterComboBox.Size = new Size(280, 20);
      this.selectedPrinterComboBox.TabIndex = 46;
      this.selectedPrinterComboBox.SelectedIndexChanged += new EventHandler(this.OnSelectedPrintComboBoxChanged);
      this.buttonLock.Location = new Point(8, 49);
      this.buttonLock.Name = "buttonLock";
      this.buttonLock.Size = new Size(184, 30);
      this.buttonLock.TabIndex = 45;
      this.buttonLock.Text = "Control This Printer";
      this.buttonLock.UseVisualStyleBackColor = true;
      this.buttonLock.Click += new EventHandler(this.buttonLock_Click);
      this.groupBoxPrinting.Controls.Add((Control) this.groupBox3);
      this.groupBoxPrinting.Location = new Point(389, 11);
      this.groupBoxPrinting.Name = "groupBoxPrinting";
      this.groupBoxPrinting.Size = new Size(619, 382);
      this.groupBoxPrinting.TabIndex = 52;
      this.groupBoxPrinting.TabStop = false;
      this.groupBoxPrinting.Text = "Now Printing";
      this.groupBoxPrinting.Visible = false;
      this.groupBox2.Controls.Add((Control) this.buttonSetTemp);
      this.groupBox2.Controls.Add((Control) this.textBoxTempEdit);
      this.groupBox2.Controls.Add((Control) this.label8);
      this.groupBox2.Location = new Point(4, 90);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(435, 109);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Tempeature Adjustments";
      this.buttonSetTemp.Location = new Point(158, 71);
      this.buttonSetTemp.Name = "buttonSetTemp";
      this.buttonSetTemp.Size = new Size(124, 24);
      this.buttonSetTemp.TabIndex = 4;
      this.buttonSetTemp.Text = "Set Temperature";
      this.buttonSetTemp.UseVisualStyleBackColor = true;
      this.buttonSetTemp.Click += new EventHandler(this.buttonSetTemp_Click);
      this.textBoxTempEdit.Location = new Point(14, 72);
      this.textBoxTempEdit.Name = "textBoxTempEdit";
      this.textBoxTempEdit.Size = new Size(131, 21);
      this.textBoxTempEdit.TabIndex = 3;
      this.label8.Location = new Point(7, 23);
      this.label8.Name = "label8";
      this.label8.Size = new Size(401, 40);
      this.label8.TabIndex = 2;
      this.label8.Text = "The current temperature can be set while printing to improve print results. Only temperature values within 15 degees of the current average temperature are allowed.";
      this.label7.AutoSize = true;
      this.label7.Location = new Point(95, 40);
      this.label7.Name = "label7";
      this.label7.Size = new Size(178, 12);
      this.label7.TabIndex = 1;
      this.label7.Text = "Average Extruder Temperature";
      this.groupBox1.Controls.Add((Control) this.textBoxMeanTemperature);
      this.groupBox1.Location = new Point(3, 20);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(83, 49);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.textBoxMeanTemperature.AutoSize = true;
      this.textBoxMeanTemperature.Font = new Font("Gulim", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 129);
      this.textBoxMeanTemperature.Location = new Point(8, 18);
      this.textBoxMeanTemperature.Name = "textBoxMeanTemperature";
      this.textBoxMeanTemperature.Size = new Size(65, 19);
      this.textBoxMeanTemperature.TabIndex = 0;
      this.textBoxMeanTemperature.Text = "000.00";
      this.groupBoxControls.BackgroundImageLayout = ImageLayout.Center;
      this.groupBoxControls.Controls.Add((Control) this.groupBoxBootloaderOptions);
      this.groupBoxControls.Controls.Add((Control) this.groupBoxFirmwareControls);
      this.groupBoxControls.Location = new Point(389, 8);
      this.groupBoxControls.Name = "groupBoxControls";
      this.groupBoxControls.Size = new Size(624, 396);
      this.groupBoxControls.TabIndex = 43;
      this.groupBoxControls.TabStop = false;
      this.groupBoxBootloaderOptions.Controls.Add((Control) this.label2);
      this.groupBoxBootloaderOptions.Controls.Add((Control) this.buttonQuitBootloader);
      this.groupBoxBootloaderOptions.Controls.Add((Control) this.button3);
      this.groupBoxBootloaderOptions.Location = new Point(8, 11);
      this.groupBoxBootloaderOptions.Name = "groupBoxBootloaderOptions";
      this.groupBoxBootloaderOptions.Size = new Size(499, 162);
      this.groupBoxBootloaderOptions.TabIndex = 51;
      this.groupBoxBootloaderOptions.TabStop = false;
      this.groupBoxBootloaderOptions.Text = "Printer Boot Options";
      this.groupBoxBootloaderOptions.Visible = false;
      this.label2.AutoSize = true;
      this.label2.Location = new Point(19, 74);
      this.label2.Name = "label2";
      this.label2.Size = new Size(241, 12);
      this.label2.TabIndex = 2;
      this.label2.Text = "The main printer program has not started.";
      this.buttonQuitBootloader.Location = new Point(11, 20);
      this.buttonQuitBootloader.Name = "buttonQuitBootloader";
      this.buttonQuitBootloader.Size = new Size(200, 36);
      this.buttonQuitBootloader.TabIndex = 1;
      this.buttonQuitBootloader.Text = "Start Printer Firmware";
      this.buttonQuitBootloader.UseVisualStyleBackColor = true;
      this.buttonQuitBootloader.UseWaitCursor = true;
      this.buttonQuitBootloader.Click += new EventHandler(this.buttonQuitBootloader_Click);
      this.button3.Location = new Point(229, 20);
      this.button3.Name = "button3";
      this.button3.Size = new Size(200, 36);
      this.button3.TabIndex = 0;
      this.button3.Text = "Update Firmware";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new EventHandler(this.buttonUpdateFirmware_Click);
      this.groupBoxFirmwareControls.Controls.Add((Control) this.tabControl1);
      this.groupBoxFirmwareControls.Controls.Add((Control) this.buttonRepeatLast);
      this.groupBoxFirmwareControls.Controls.Add((Control) this.label3);
      this.groupBoxFirmwareControls.Controls.Add((Control) this.textBoxManualGCode);
      this.groupBoxFirmwareControls.Controls.Add((Control) this.buttonSendGCode);
      this.groupBoxFirmwareControls.Location = new Point(8, 11);
      this.groupBoxFirmwareControls.Name = "groupBoxFirmwareControls";
      this.groupBoxFirmwareControls.Size = new Size(617, 387);
      this.groupBoxFirmwareControls.TabIndex = 0;
      this.groupBoxFirmwareControls.TabStop = false;
      this.tabControl1.Controls.Add((Control) this.tabPageBasicOptions);
      this.tabControl1.Controls.Add((Control) this.tabPageDiagnostics);
      this.tabControl1.Controls.Add((Control) this.tabPageSDCard);
      this.tabControl1.Controls.Add((Control) this.tabPageHeatedBedControl);
      this.tabControl1.Location = new Point(4, 49);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new Size(609, 329);
      this.tabControl1.TabIndex = 50;
      this.tabControl1.SelectedIndexChanged += new EventHandler(this.tabControl1_SelectedIndexChanged);
      this.tabPageBasicOptions.BackColor = SystemColors.Control;
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonTGHTemp);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonHeaterOff);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonFanOff);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonFanOn);
      this.tabPageBasicOptions.Controls.Add((Control) this.textBoxEVal);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonPLATemp);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonDownE);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonUpE);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonABSTemp);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonAddJob);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonMotorsOff);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonMotorsOn);
      this.tabPageBasicOptions.Controls.Add((Control) this.textBoxYVal);
      this.tabPageBasicOptions.Controls.Add((Control) this.textBoxXVal);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonPrintToFile);
      this.tabPageBasicOptions.Controls.Add((Control) this.textBoxZVal);
      this.tabPageBasicOptions.Controls.Add((Control) this.button2);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonUpX);
      this.tabPageBasicOptions.Controls.Add((Control) this.button1);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonDownX);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonFilamentInfo);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonDownZ);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonDownY);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonUpZ);
      this.tabPageBasicOptions.Controls.Add((Control) this.buttonUpY);
      this.tabPageBasicOptions.Location = new Point(4, 22);
      this.tabPageBasicOptions.Name = "tabPageBasicOptions";
      this.tabPageBasicOptions.Padding = new Padding(3);
      this.tabPageBasicOptions.Size = new Size(601, 303);
      this.tabPageBasicOptions.TabIndex = 0;
      this.tabPageBasicOptions.Text = "Basic Options";
      this.buttonTGHTemp.Location = new Point(13, 177);
      this.buttonTGHTemp.Name = "buttonTGHTemp";
      this.buttonTGHTemp.Size = new Size(129, 32);
      this.buttonTGHTemp.TabIndex = 59;
      this.buttonTGHTemp.Text = "Heat to TGH Temp";
      this.buttonTGHTemp.UseVisualStyleBackColor = true;
      this.buttonTGHTemp.Click += new EventHandler(this.buttonTGHTemp_Click);
      this.buttonHeaterOff.Location = new Point(149, 178);
      this.buttonHeaterOff.Name = "buttonHeaterOff";
      this.buttonHeaterOff.Size = new Size(129, 32);
      this.buttonHeaterOff.TabIndex = 58;
      this.buttonHeaterOff.Text = "Heater Off";
      this.buttonHeaterOff.UseVisualStyleBackColor = true;
      this.buttonHeaterOff.Click += new EventHandler(this.buttonHeaterOff_Click);
      this.buttonFanOff.Location = new Point(149, 109);
      this.buttonFanOff.Name = "buttonFanOff";
      this.buttonFanOff.Size = new Size(129, 32);
      this.buttonFanOff.TabIndex = 57;
      this.buttonFanOff.Text = "Fan Off";
      this.buttonFanOff.UseVisualStyleBackColor = true;
      this.buttonFanOff.Click += new EventHandler(this.buttonFanOff_Click);
      this.buttonFanOn.Location = new Point(13, 109);
      this.buttonFanOn.Name = "buttonFanOn";
      this.buttonFanOn.Size = new Size(129, 32);
      this.buttonFanOn.TabIndex = 56;
      this.buttonFanOn.Text = "Fan On";
      this.buttonFanOn.UseVisualStyleBackColor = true;
      this.buttonFanOn.Click += new EventHandler(this.buttonFanOn_Click);
      this.textBoxEVal.Location = new Point(408, 251);
      this.textBoxEVal.Name = "textBoxEVal";
      this.textBoxEVal.Size = new Size(49, 21);
      this.textBoxEVal.TabIndex = 53;
      this.textBoxEVal.Text = "1.0";
      this.buttonPLATemp.Location = new Point(13, 143);
      this.buttonPLATemp.Name = "buttonPLATemp";
      this.buttonPLATemp.Size = new Size(129, 32);
      this.buttonPLATemp.TabIndex = 55;
      this.buttonPLATemp.Text = "Heat to PLA Temp";
      this.buttonPLATemp.UseVisualStyleBackColor = true;
      this.buttonPLATemp.Click += new EventHandler(this.buttonPLATemp_Click);
      this.buttonDownE.BackgroundImage = (Image) Resources.arrowsEMinus;
      this.buttonDownE.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonDownE.Location = new Point(285, 223);
      this.buttonDownE.Name = "buttonDownE";
      this.buttonDownE.Size = new Size(101, 73);
      this.buttonDownE.TabIndex = 52;
      this.buttonDownE.UseVisualStyleBackColor = true;
      this.buttonDownE.Click += new EventHandler(this.buttonDownE_Click);
      this.buttonUpE.BackgroundImage = (Image) Resources.arrowsEPlus;
      this.buttonUpE.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonUpE.Location = new Point(477, 223);
      this.buttonUpE.Name = "buttonUpE";
      this.buttonUpE.Size = new Size(101, 73);
      this.buttonUpE.TabIndex = 51;
      this.buttonUpE.UseVisualStyleBackColor = true;
      this.buttonUpE.Click += new EventHandler(this.buttonUpE_Click);
      this.buttonABSTemp.Location = new Point(149, 143);
      this.buttonABSTemp.Name = "buttonABSTemp";
      this.buttonABSTemp.Size = new Size(129, 32);
      this.buttonABSTemp.TabIndex = 54;
      this.buttonABSTemp.Text = "Heat to ABS Temp";
      this.buttonABSTemp.UseVisualStyleBackColor = true;
      this.buttonABSTemp.Click += new EventHandler(this.buttonABSTemp_Click);
      this.buttonAddJob.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonAddJob.ForeColor = SystemColors.ControlText;
      this.buttonAddJob.Location = new Point(13, 75);
      this.buttonAddJob.Name = "buttonAddJob";
      this.buttonAddJob.Size = new Size(129, 32);
      this.buttonAddJob.TabIndex = 5;
      this.buttonAddJob.Text = "Add Print Job";
      this.buttonAddJob.UseVisualStyleBackColor = true;
      this.buttonAddJob.Click += new EventHandler(this.buttonAddJob_Click);
      this.buttonMotorsOff.Location = new Point(149, 6);
      this.buttonMotorsOff.Name = "buttonMotorsOff";
      this.buttonMotorsOff.Size = new Size(129, 32);
      this.buttonMotorsOff.TabIndex = 41;
      this.buttonMotorsOff.Text = "Motors Off";
      this.buttonMotorsOff.UseVisualStyleBackColor = true;
      this.buttonMotorsOff.Click += new EventHandler(this.buttonMotorsOff_Click);
      this.buttonMotorsOn.Location = new Point(13, 6);
      this.buttonMotorsOn.Name = "buttonMotorsOn";
      this.buttonMotorsOn.Size = new Size(129, 32);
      this.buttonMotorsOn.TabIndex = 40;
      this.buttonMotorsOn.Text = "Motors On";
      this.buttonMotorsOn.UseVisualStyleBackColor = true;
      this.buttonMotorsOn.Click += new EventHandler(this.buttonMotorsOn_Click);
      this.textBoxYVal.Location = new Point(408, 178);
      this.textBoxYVal.Name = "textBoxYVal";
      this.textBoxYVal.Size = new Size(49, 21);
      this.textBoxYVal.TabIndex = 39;
      this.textBoxYVal.Text = "10.0";
      this.textBoxXVal.Location = new Point(408, 105);
      this.textBoxXVal.Name = "textBoxXVal";
      this.textBoxXVal.Size = new Size(49, 21);
      this.textBoxXVal.TabIndex = 38;
      this.textBoxXVal.Text = "10.0";
      this.buttonPrintToFile.Location = new Point(149, 75);
      this.buttonPrintToFile.Name = "buttonPrintToFile";
      this.buttonPrintToFile.Size = new Size(129, 32);
      this.buttonPrintToFile.TabIndex = 28;
      this.buttonPrintToFile.Text = "Print to Binary File";
      this.buttonPrintToFile.UseVisualStyleBackColor = true;
      this.buttonPrintToFile.Click += new EventHandler(this.buttonPrintToFile_Click);
      this.textBoxZVal.Location = new Point(408, 34);
      this.textBoxZVal.Name = "textBoxZVal";
      this.textBoxZVal.Size = new Size(49, 21);
      this.textBoxZVal.TabIndex = 37;
      this.textBoxZVal.Text = "1.0";
      this.button2.Location = new Point(13, 41);
      this.button2.Name = "button2";
      this.button2.Size = new Size(129, 32);
      this.button2.TabIndex = 50;
      this.button2.Text = "Relative Mode";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new EventHandler(this.button2_Click);
      this.buttonUpX.BackgroundImage = (Image) Resources.arrowsXPlus;
      this.buttonUpX.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonUpX.Location = new Point(477, 78);
      this.buttonUpX.Name = "buttonUpX";
      this.buttonUpX.Size = new Size(101, 73);
      this.buttonUpX.TabIndex = 36;
      this.buttonUpX.UseVisualStyleBackColor = true;
      this.buttonUpX.Click += new EventHandler(this.buttonUpX_Click);
      this.button1.Location = new Point(149, 41);
      this.button1.Name = "button1";
      this.button1.Size = new Size(129, 32);
      this.button1.TabIndex = 42;
      this.button1.Text = "Absolute Mode";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.buttonDownX.BackgroundImage = (Image) Resources.arrowsXMinus;
      this.buttonDownX.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonDownX.Location = new Point(285, 78);
      this.buttonDownX.Name = "buttonDownX";
      this.buttonDownX.Size = new Size(101, 73);
      this.buttonDownX.TabIndex = 35;
      this.buttonDownX.UseVisualStyleBackColor = true;
      this.buttonDownX.Click += new EventHandler(this.buttonDownX_Click);
      this.buttonFilamentInfo.ForeColor = SystemColors.ControlText;
      this.buttonFilamentInfo.Location = new Point(13, 211);
      this.buttonFilamentInfo.Name = "buttonFilamentInfo";
      this.buttonFilamentInfo.Size = new Size(129, 32);
      this.buttonFilamentInfo.TabIndex = 44;
      this.buttonFilamentInfo.Text = "Set Filament Info";
      this.buttonFilamentInfo.UseVisualStyleBackColor = true;
      this.buttonFilamentInfo.Visible = false;
      this.buttonFilamentInfo.Click += new EventHandler(this.buttonChangeFilamentInfo_Click);
      this.buttonDownZ.BackgroundImage = (Image) Resources.arrowsZMinus;
      this.buttonDownZ.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonDownZ.Location = new Point(285, 5);
      this.buttonDownZ.Name = "buttonDownZ";
      this.buttonDownZ.Size = new Size(101, 73);
      this.buttonDownZ.TabIndex = 34;
      this.buttonDownZ.UseVisualStyleBackColor = true;
      this.buttonDownZ.Click += new EventHandler(this.buttonDownZ_Click);
      this.buttonDownY.BackgroundImage = (Image) Resources.arrowsYMinus;
      this.buttonDownY.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonDownY.Location = new Point(285, 150);
      this.buttonDownY.Name = "buttonDownY";
      this.buttonDownY.Size = new Size(101, 73);
      this.buttonDownY.TabIndex = 32;
      this.buttonDownY.UseVisualStyleBackColor = true;
      this.buttonDownY.Click += new EventHandler(this.buttonDownY_Click);
      this.buttonUpZ.BackgroundImage = (Image) Resources.arrowsZPlus;
      this.buttonUpZ.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonUpZ.Location = new Point(477, 5);
      this.buttonUpZ.Name = "buttonUpZ";
      this.buttonUpZ.Size = new Size(101, 73);
      this.buttonUpZ.TabIndex = 33;
      this.buttonUpZ.UseVisualStyleBackColor = true;
      this.buttonUpZ.Click += new EventHandler(this.buttonUpZ_Click);
      this.buttonUpY.BackgroundImage = (Image) Resources.arrowsYPlus;
      this.buttonUpY.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonUpY.Location = new Point(477, 150);
      this.buttonUpY.Name = "buttonUpY";
      this.buttonUpY.Size = new Size(101, 73);
      this.buttonUpY.TabIndex = 31;
      this.buttonUpY.UseVisualStyleBackColor = true;
      this.buttonUpY.Click += new EventHandler(this.buttonUpY_Click);
      this.tabPageDiagnostics.BackColor = SystemColors.Control;
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonLoadEepromData);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonSaveEepromData);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonUpdateFirmware);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonPreprocess);
      this.tabPageDiagnostics.Controls.Add((Control) this.groupBox4);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonBacklashPrint);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonGetGCode);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonReCenter);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonHome);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonSpeedTest);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonXSpeedTest);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonYSkipTestBack);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonXSkipTestRight);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonYSkipTest);
      this.tabPageDiagnostics.Controls.Add((Control) this.buttonXSkipTest);
      this.tabPageDiagnostics.Location = new Point(4, 22);
      this.tabPageDiagnostics.Name = "tabPageDiagnostics";
      this.tabPageDiagnostics.Padding = new Padding(3);
      this.tabPageDiagnostics.Size = new Size(601, 303);
      this.tabPageDiagnostics.TabIndex = 1;
      this.tabPageDiagnostics.Text = "Diagnostics";
      this.buttonLoadEepromData.Location = new Point(448, 58);
      this.buttonLoadEepromData.Name = "buttonLoadEepromData";
      this.buttonLoadEepromData.Size = new Size(138, 36);
      this.buttonLoadEepromData.TabIndex = 29;
      this.buttonLoadEepromData.Text = "Load EEPROM Data";
      this.buttonLoadEepromData.UseVisualStyleBackColor = true;
      this.buttonLoadEepromData.Click += new EventHandler(this.buttonLoadEepromData_Click);
      this.buttonSaveEepromData.Location = new Point(448, 11);
      this.buttonSaveEepromData.Name = "buttonSaveEepromData";
      this.buttonSaveEepromData.Size = new Size(138, 36);
      this.buttonSaveEepromData.TabIndex = 28;
      this.buttonSaveEepromData.Text = "Save EEPROM Data";
      this.buttonSaveEepromData.UseVisualStyleBackColor = true;
      this.buttonSaveEepromData.Click += new EventHandler(this.buttonSaveEepromData_Click);
      this.buttonUpdateFirmware.Location = new Point(301, 157);
      this.buttonUpdateFirmware.Name = "buttonUpdateFirmware";
      this.buttonUpdateFirmware.Size = new Size(138, 36);
      this.buttonUpdateFirmware.TabIndex = 26;
      this.buttonUpdateFirmware.Text = "Update Firmware";
      this.buttonUpdateFirmware.UseVisualStyleBackColor = true;
      this.buttonUpdateFirmware.Click += new EventHandler(this.buttonUpdateFirmware_Click);
      this.buttonPreprocess.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonPreprocess.Location = new Point(155, 157);
      this.buttonPreprocess.Name = "buttonPreprocess";
      this.buttonPreprocess.Size = new Size(138, 36);
      this.buttonPreprocess.TabIndex = 27;
      this.buttonPreprocess.Text = "Backlash Settings";
      this.buttonPreprocess.UseVisualStyleBackColor = true;
      this.buttonPreprocess.Click += new EventHandler(this.buttonPreprocess_Click);
      this.groupBox4.Controls.Add((Control) this.buttonBedPointTest);
      this.groupBox4.Controls.Add((Control) this.comboBoxBedTestPoint);
      this.groupBox4.Location = new Point(9, 203);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new Size(297, 45);
      this.groupBox4.TabIndex = 10;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Bed Point Test";
      this.buttonBedPointTest.Location = new Point(7, 17);
      this.buttonBedPointTest.Name = "buttonBedPointTest";
      this.buttonBedPointTest.Size = new Size(131, 21);
      this.buttonBedPointTest.TabIndex = 1;
      this.buttonBedPointTest.Text = "Move to Point";
      this.buttonBedPointTest.UseVisualStyleBackColor = true;
      this.buttonBedPointTest.Click += new EventHandler(this.buttonBedPointTest_Click);
      this.comboBoxBedTestPoint.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxBedTestPoint.FormattingEnabled = true;
      this.comboBoxBedTestPoint.Items.AddRange(new object[5]
      {
        (object) "Center",
        (object) "Front Right",
        (object) "Front Left",
        (object) "Back Left",
        (object) "Back Right"
      });
      this.comboBoxBedTestPoint.Location = new Point(160, 18);
      this.comboBoxBedTestPoint.Name = "comboBoxBedTestPoint";
      this.comboBoxBedTestPoint.Size = new Size(129, 20);
      this.comboBoxBedTestPoint.TabIndex = 0;
      this.buttonBacklashPrint.Location = new Point(9, 157);
      this.buttonBacklashPrint.Name = "buttonBacklashPrint";
      this.buttonBacklashPrint.Size = new Size(138, 36);
      this.buttonBacklashPrint.TabIndex = 9;
      this.buttonBacklashPrint.Text = "Start Backlash Print";
      this.buttonBacklashPrint.UseVisualStyleBackColor = true;
      this.buttonBacklashPrint.Click += new EventHandler(this.buttonBacklashPrint_Click);
      this.buttonGetGCode.Location = new Point(301, 106);
      this.buttonGetGCode.Name = "buttonGetGCode";
      this.buttonGetGCode.Size = new Size(138, 36);
      this.buttonGetGCode.TabIndex = 8;
      this.buttonGetGCode.Text = "Get G-Code";
      this.buttonGetGCode.UseVisualStyleBackColor = true;
      this.buttonGetGCode.Click += new EventHandler(this.buttonGetGCode_Click);
      this.buttonReCenter.Location = new Point(301, 58);
      this.buttonReCenter.Name = "buttonReCenter";
      this.buttonReCenter.Size = new Size(138, 36);
      this.buttonReCenter.TabIndex = 7;
      this.buttonReCenter.Text = "Fast Re-Center";
      this.buttonReCenter.UseVisualStyleBackColor = true;
      this.buttonReCenter.Click += new EventHandler(this.buttonReCenter_Click);
      this.buttonHome.Location = new Point(301, 11);
      this.buttonHome.Name = "buttonHome";
      this.buttonHome.Size = new Size(138, 36);
      this.buttonHome.TabIndex = 6;
      this.buttonHome.Text = "Home";
      this.buttonHome.UseVisualStyleBackColor = true;
      this.buttonHome.Click += new EventHandler(this.buttonHome_Click);
      this.buttonSpeedTest.Location = new Point(155, 106);
      this.buttonSpeedTest.Name = "buttonSpeedTest";
      this.buttonSpeedTest.Size = new Size(138, 36);
      this.buttonSpeedTest.TabIndex = 5;
      this.buttonSpeedTest.Text = "Y Speed Test";
      this.buttonSpeedTest.UseVisualStyleBackColor = true;
      this.buttonSpeedTest.Click += new EventHandler(this.buttonYSpeedTest_Click);
      this.buttonXSpeedTest.Location = new Point(9, 106);
      this.buttonXSpeedTest.Name = "buttonXSpeedTest";
      this.buttonXSpeedTest.Size = new Size(138, 36);
      this.buttonXSpeedTest.TabIndex = 4;
      this.buttonXSpeedTest.Text = "X Speed Test";
      this.buttonXSpeedTest.UseVisualStyleBackColor = true;
      this.buttonXSpeedTest.Click += new EventHandler(this.buttonXSpeedTest_Click);
      this.buttonYSkipTestBack.Location = new Point(155, 58);
      this.buttonYSkipTestBack.Name = "buttonYSkipTestBack";
      this.buttonYSkipTestBack.Size = new Size(138, 36);
      this.buttonYSkipTestBack.TabIndex = 3;
      this.buttonYSkipTestBack.Text = "Y Skip Test /\\ (Back)";
      this.buttonYSkipTestBack.UseVisualStyleBackColor = true;
      this.buttonYSkipTestBack.Click += new EventHandler(this.buttonYSkipTestBack_Click);
      this.buttonXSkipTestRight.Location = new Point(9, 58);
      this.buttonXSkipTestRight.Name = "buttonXSkipTestRight";
      this.buttonXSkipTestRight.Size = new Size(138, 36);
      this.buttonXSkipTestRight.TabIndex = 2;
      this.buttonXSkipTestRight.Text = "X Skip Test -->(Right)";
      this.buttonXSkipTestRight.UseVisualStyleBackColor = true;
      this.buttonXSkipTestRight.Click += new EventHandler(this.buttonXSkipTestRight_Click);
      this.buttonYSkipTest.Location = new Point(155, 10);
      this.buttonYSkipTest.Name = "buttonYSkipTest";
      this.buttonYSkipTest.Size = new Size(138, 36);
      this.buttonYSkipTest.TabIndex = 1;
      this.buttonYSkipTest.Text = "Y Skip Test \\/ (Front)";
      this.buttonYSkipTest.UseVisualStyleBackColor = true;
      this.buttonYSkipTest.Click += new EventHandler(this.buttonYSkipTest_Click);
      this.buttonXSkipTest.Location = new Point(9, 10);
      this.buttonXSkipTest.Name = "buttonXSkipTest";
      this.buttonXSkipTest.Size = new Size(138, 36);
      this.buttonXSkipTest.TabIndex = 0;
      this.buttonXSkipTest.Text = "X Skip Test <--  (Left)";
      this.buttonXSkipTest.UseVisualStyleBackColor = true;
      this.buttonXSkipTest.Click += new EventHandler(this.buttonXSkipTest_Click);
      this.tabPageSDCard.BackColor = SystemColors.Control;
      this.tabPageSDCard.Controls.Add((Control) this.checkBoxCalibrateBeforeSDPrint);
      this.tabPageSDCard.Controls.Add((Control) this.buttonSDDeleteFile);
      this.tabPageSDCard.Controls.Add((Control) this.buttonSDPrint);
      this.tabPageSDCard.Controls.Add((Control) this.buttonSDSaveGcode);
      this.tabPageSDCard.Controls.Add((Control) this.buttonSDRefresh);
      this.tabPageSDCard.Controls.Add((Control) this.listBoxSDFiles);
      this.tabPageSDCard.Controls.Add((Control) this.label6);
      this.tabPageSDCard.Location = new Point(4, 22);
      this.tabPageSDCard.Name = "tabPageSDCard";
      this.tabPageSDCard.Size = new Size(601, 303);
      this.tabPageSDCard.TabIndex = 3;
      this.tabPageSDCard.Text = "Untethered Printing";
      this.checkBoxCalibrateBeforeSDPrint.AutoSize = true;
      this.checkBoxCalibrateBeforeSDPrint.Location = new Point(20, 217);
      this.checkBoxCalibrateBeforeSDPrint.Name = "checkBoxCalibrateBeforeSDPrint";
      this.checkBoxCalibrateBeforeSDPrint.Size = new Size(267, 16);
      this.checkBoxCalibrateBeforeSDPrint.TabIndex = 11;
      this.checkBoxCalibrateBeforeSDPrint.Text = "Calibrate printer before printing saved jobs.";
      this.checkBoxCalibrateBeforeSDPrint.UseVisualStyleBackColor = true;
      this.buttonSDDeleteFile.Location = new Point(359, 173);
      this.buttonSDDeleteFile.Name = "buttonSDDeleteFile";
      this.buttonSDDeleteFile.Size = new Size(138, 36);
      this.buttonSDDeleteFile.TabIndex = 10;
      this.buttonSDDeleteFile.Text = "Delete Saved Job";
      this.buttonSDDeleteFile.UseVisualStyleBackColor = true;
      this.buttonSDDeleteFile.Click += new EventHandler(this.buttonSDDelete_Click);
      this.buttonSDPrint.Location = new Point(359, (int) sbyte.MaxValue);
      this.buttonSDPrint.Name = "buttonSDPrint";
      this.buttonSDPrint.Size = new Size(138, 36);
      this.buttonSDPrint.TabIndex = 9;
      this.buttonSDPrint.Text = "Print Saved Job";
      this.buttonSDPrint.UseVisualStyleBackColor = true;
      this.buttonSDPrint.Click += new EventHandler(this.buttonSDPrint_Click);
      this.buttonSDSaveGcode.Location = new Point(359, 81);
      this.buttonSDSaveGcode.Name = "buttonSDSaveGcode";
      this.buttonSDSaveGcode.Size = new Size(138, 36);
      this.buttonSDSaveGcode.TabIndex = 8;
      this.buttonSDSaveGcode.Text = "Save Job to Printer";
      this.buttonSDSaveGcode.UseVisualStyleBackColor = true;
      this.buttonSDSaveGcode.Click += new EventHandler(this.buttonSDSaveGcode_Click);
      this.buttonSDRefresh.Location = new Point(359, 35);
      this.buttonSDRefresh.Name = "buttonSDRefresh";
      this.buttonSDRefresh.Size = new Size(138, 36);
      this.buttonSDRefresh.TabIndex = 7;
      this.buttonSDRefresh.Text = "Refresh";
      this.buttonSDRefresh.UseVisualStyleBackColor = true;
      this.buttonSDRefresh.Click += new EventHandler(this.buttonSDRefresh_Click);
      this.listBoxSDFiles.FormattingEnabled = true;
      this.listBoxSDFiles.ItemHeight = 12;
      this.listBoxSDFiles.Location = new Point(15, 35);
      this.listBoxSDFiles.Name = "listBoxSDFiles";
      this.listBoxSDFiles.Size = new Size(330, 172);
      this.listBoxSDFiles.TabIndex = 1;
      this.label6.AutoSize = true;
      this.label6.Location = new Point(10, 18);
      this.label6.Name = "label6";
      this.label6.Size = new Size(214, 12);
      this.label6.TabIndex = 0;
      this.label6.Text = "Print Jobs Saved to Internal Memory:";
      this.tabPageHeatedBedControl.BackColor = SystemColors.Control;
      this.tabPageHeatedBedControl.Controls.Add((Control) this.buttonTurnOfHeatedbed);
      this.tabPageHeatedBedControl.Controls.Add((Control) this.buttonBedHeatToABR);
      this.tabPageHeatedBedControl.Controls.Add((Control) this.buttonBedHeatToABS);
      this.tabPageHeatedBedControl.Controls.Add((Control) this.buttonBedHeatToPLA);
      this.tabPageHeatedBedControl.Location = new Point(4, 22);
      this.tabPageHeatedBedControl.Name = "tabPageHeatedBedControl";
      this.tabPageHeatedBedControl.Padding = new Padding(3);
      this.tabPageHeatedBedControl.Size = new Size(601, 303);
      this.tabPageHeatedBedControl.TabIndex = 2;
      this.tabPageHeatedBedControl.Text = "Heated Bed Control";
      this.buttonTurnOfHeatedbed.Location = new Point(6, 52);
      this.buttonTurnOfHeatedbed.Name = "buttonTurnOfHeatedbed";
      this.buttonTurnOfHeatedbed.Size = new Size(141, 32);
      this.buttonTurnOfHeatedbed.TabIndex = 63;
      this.buttonTurnOfHeatedbed.Text = "Turn off Heated Bed";
      this.buttonTurnOfHeatedbed.UseVisualStyleBackColor = true;
      this.buttonTurnOfHeatedbed.Click += new EventHandler(this.buttonTurnOfHeatedbed_Click);
      this.buttonBedHeatToABR.Location = new Point(304, 6);
      this.buttonBedHeatToABR.Name = "buttonBedHeatToABR";
      this.buttonBedHeatToABR.Size = new Size(141, 32);
      this.buttonBedHeatToABR.TabIndex = 61;
      this.buttonBedHeatToABR.Text = "Heat to ABS-R Temp";
      this.buttonBedHeatToABR.UseVisualStyleBackColor = true;
      this.buttonBedHeatToABR.Click += new EventHandler(this.buttonBedHeatToABR_Click);
      this.buttonBedHeatToABS.Location = new Point(155, 6);
      this.buttonBedHeatToABS.Name = "buttonBedHeatToABS";
      this.buttonBedHeatToABS.Size = new Size(141, 32);
      this.buttonBedHeatToABS.TabIndex = 60;
      this.buttonBedHeatToABS.Text = "Heat to ABS Temp";
      this.buttonBedHeatToABS.UseVisualStyleBackColor = true;
      this.buttonBedHeatToABS.Click += new EventHandler(this.buttonBedHeatToABS_Click);
      this.buttonBedHeatToPLA.Location = new Point(6, 6);
      this.buttonBedHeatToPLA.Name = "buttonBedHeatToPLA";
      this.buttonBedHeatToPLA.Size = new Size(141, 32);
      this.buttonBedHeatToPLA.TabIndex = 59;
      this.buttonBedHeatToPLA.Text = "Heat to PLA Temp";
      this.buttonBedHeatToPLA.UseVisualStyleBackColor = true;
      this.buttonBedHeatToPLA.Click += new EventHandler(this.buttonBedHeatToPLA_Click);
      this.buttonRepeatLast.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonRepeatLast.Location = new Point(383, 24);
      this.buttonRepeatLast.Name = "buttonRepeatLast";
      this.buttonRepeatLast.Size = new Size(78, 18);
      this.buttonRepeatLast.TabIndex = 43;
      this.buttonRepeatLast.Text = "Repeat";
      this.buttonRepeatLast.UseVisualStyleBackColor = true;
      this.buttonRepeatLast.Click += new EventHandler(this.buttonRepeatLast_Click);
      this.label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label3.AutoSize = true;
      this.label3.Location = new Point(4, 8);
      this.label3.Name = "label3";
      this.label3.Size = new Size(131, 12);
      this.label3.TabIndex = 7;
      this.label3.Text = "Manual Insert G-Code";
      this.textBoxManualGCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.textBoxManualGCode.Location = new Point(8, 24);
      this.textBoxManualGCode.Name = "textBoxManualGCode";
      this.textBoxManualGCode.Size = new Size(278, 21);
      this.textBoxManualGCode.TabIndex = 8;
      this.textBoxManualGCode.Enter += new EventHandler(this.OnEnterInsertGCode);
      this.textBoxManualGCode.Leave += new EventHandler(this.OnLeaveInsertGCode);
      this.buttonSendGCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonSendGCode.Location = new Point(297, 24);
      this.buttonSendGCode.Name = "buttonSendGCode";
      this.buttonSendGCode.Size = new Size(78, 18);
      this.buttonSendGCode.TabIndex = 9;
      this.buttonSendGCode.Text = "Send";
      this.buttonSendGCode.UseVisualStyleBackColor = true;
      this.buttonSendGCode.Click += new EventHandler(this.buttonSendGCode_Click);
      this.buttonClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.buttonClearLog.Location = new Point(9, 252);
      this.buttonClearLog.Name = "buttonClearLog";
      this.buttonClearLog.Size = new Size(87, 21);
      this.buttonClearLog.TabIndex = 41;
      this.buttonClearLog.Text = "Clear Log";
      this.buttonClearLog.UseVisualStyleBackColor = true;
      this.buttonClearLog.Click += new EventHandler(this.buttonClearLog_Click);
      this.checkBoxLogToScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.checkBoxLogToScreen.AutoSize = true;
      this.checkBoxLogToScreen.Location = new Point(240, 254);
      this.checkBoxLogToScreen.Name = "checkBoxLogToScreen";
      this.checkBoxLogToScreen.Size = new Size(103, 16);
      this.checkBoxLogToScreen.TabIndex = 40;
      this.checkBoxLogToScreen.Text = "Log to Screen";
      this.checkBoxLogToScreen.UseVisualStyleBackColor = true;
      this.checkBoxLogToScreen.CheckedChanged += new EventHandler(this.OnLogToScreenChanged);
      this.richTextBoxLoggedItems.Location = new Point(8, 144);
      this.richTextBoxLoggedItems.Name = "richTextBoxLoggedItems";
      this.richTextBoxLoggedItems.Size = new Size(375, 104);
      this.richTextBoxLoggedItems.TabIndex = 42;
      this.richTextBoxLoggedItems.Text = "";
      this.richTextBoxLoggedItems.MouseUp += new MouseEventHandler(this.richTextBoxLoggedItems_MouseUp);
      this.buttonEmergencyStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonEmergencyStop.BackColor = SystemColors.Control;
      this.buttonEmergencyStop.ForeColor = System.Drawing.Color.Red;
      this.buttonEmergencyStop.Location = new Point(200, 49);
      this.buttonEmergencyStop.Name = "buttonEmergencyStop";
      this.buttonEmergencyStop.Size = new Size(184, 30);
      this.buttonEmergencyStop.TabIndex = 0;
      this.buttonEmergencyStop.Text = "EMERGENCY STOP";
      this.buttonEmergencyStop.UseVisualStyleBackColor = false;
      this.buttonEmergencyStop.Click += new EventHandler(this.buttonEmergencyStop_Click);
      this.label5.AutoSize = true;
      this.label5.Location = new Point(7, 126);
      this.label5.Name = "label5";
      this.label5.Size = new Size(89, 12);
      this.label5.TabIndex = 39;
      this.label5.Tag = (object) "";
      this.label5.Text = "Command Log";
      this.listBoxManualHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.listBoxManualHistory.FormattingEnabled = true;
      this.listBoxManualHistory.ItemHeight = 12;
      this.listBoxManualHistory.Location = new Point(13, 320);
      this.listBoxManualHistory.Name = "listBoxManualHistory";
      this.listBoxManualHistory.Size = new Size(370, 64);
      this.listBoxManualHistory.TabIndex = 38;
      this.label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label4.AutoSize = true;
      this.label4.Location = new Point(10, 304);
      this.label4.Name = "label4";
      this.label4.Size = new Size(103, 12);
      this.label4.TabIndex = 37;
      this.label4.Text = "Manual G-Codes";
      this.checkBoxAutoScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.checkBoxAutoScroll.AutoSize = true;
      this.checkBoxAutoScroll.Checked = true;
      this.checkBoxAutoScroll.CheckState = CheckState.Checked;
      this.checkBoxAutoScroll.Location = new Point(129, 254);
      this.checkBoxAutoScroll.Name = "checkBoxAutoScroll";
      this.checkBoxAutoScroll.Size = new Size(85, 16);
      this.checkBoxAutoScroll.TabIndex = 44;
      this.checkBoxAutoScroll.Text = "Auto Scroll";
      this.checkBoxAutoScroll.UseVisualStyleBackColor = true;
      this.groupBox3.Controls.Add((Control) this.groupBox1);
      this.groupBox3.Controls.Add((Control) this.groupBox2);
      this.groupBox3.Controls.Add((Control) this.label7);
      this.groupBox3.Location = new Point(18, 28);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(461, 226);
      this.groupBox3.TabIndex = 4;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Temperature Controls";
      this.AutoScaleDimensions = new SizeF(7f, 12f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.ClientSize = new Size(1040, 547);
      this.Controls.Add((Control) this.groupBoxPrinterControls);
      this.Controls.Add((Control) this.groupBoxPrinterList);
      this.Controls.Add((Control) this.menuStrip1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MainMenuStrip = this.menuStrip1;
      this.MaximizeBox = false;
      this.Name = nameof (MainForm);
      this.Text = "M3D Print Spooler";
      this.FormClosing += new FormClosingEventHandler(this.OnClosing);
      this.Load += new EventHandler(this.OnLoad);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.groupBoxPrinterList.ResumeLayout(false);
      this.groupBoxPrinterList.PerformLayout();
      this.groupBoxPrinterControls.ResumeLayout(false);
      this.groupBoxPrinterControls.PerformLayout();
      this.groupBoxPrinting.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBoxControls.ResumeLayout(false);
      this.groupBoxBootloaderOptions.ResumeLayout(false);
      this.groupBoxBootloaderOptions.PerformLayout();
      this.groupBoxFirmwareControls.ResumeLayout(false);
      this.groupBoxFirmwareControls.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPageBasicOptions.ResumeLayout(false);
      this.tabPageBasicOptions.PerformLayout();
      this.tabPageDiagnostics.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.tabPageSDCard.ResumeLayout(false);
      this.tabPageSDCard.PerformLayout();
      this.tabPageHeatedBedControl.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private enum PrinterLockChanging
    {
      No,
      NoFailed,
      YesToLocked,
      YesToUnlocked,
    }

    private class PrinterData
    {
      public bool bPowerOutageHandled;

      public PrinterData()
      {
        this.bPowerOutageHandled = false;
      }
    }
  }
}
