// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Forms.Form1
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms.Splash;
using M3D.GUI.Interfaces;
using M3D.GUI.OpenGL;
using M3D.GUI.Views;
using M3D.GUI.Views.Library_View;
using M3D.GUI.Views.Printer_View;
using M3D.Slicer.Cura15_04;
using M3D.Slicer.General;
using M3D.Spooling.Common.Utils;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace M3D.GUI.Forms
{
  public class Form1 : Form
  {
    public static DebugLogger debugLogger = new DebugLogger(Paths.StartupLogPath, 500U);
    public bool zoomed;
    public bool AskForUpdate;
    private SplashForm splashForm;
    private Updater SoftwareUpdater;
    private double elapsed;
    private double elapsed_frame;
    private Stopwatch fps_lock_watch;
    private Stopwatch fps_frame_counter;
    private string[] args;
    private System.Windows.Forms.Timer timer1;
    private int TEMPORARY_run_count;
    private IFileAssociations myFileAssociations;
    private IStopShutdown myStopShutdown;
    private Stopwatch keypress_stopwatch;
    private bool shift;
    private bool alt;
    private bool ctrl;
    private bool tab;
    private bool infocus;
    private SettingsManager settingsManager;
    private ModelLoadingManager model_loading_manager;
    private SpoolerConnection spooler_connection;
    private PrinterStatusDialogOrganizer printer_status_dialog_organizer;
    public GUIHost m_gui_host;
    private ControlBar controlbar;
    private PopupMessageBox messagebox;
    private MessagePopUp informationbox;
    public LibraryView libraryview;
    private OpenGLConnection OpenGLConnection;
    private bool resized;
    private PrinterView printerView;
    private bool force_close;
    private Queue<Form1.MainThreadTask> mainThreadTaskQueue;
    private IContainer components;
    public GLControl glControl1;
    private ContextMenuStrip contextMenuStrip1;

    public Form1(SplashForm splashForm, string[] args)
    {
      string str = (string) null;
      try
      {
        str = "InitializePlatformSpecificObjects";
        this.InitializePlatformSpecificObjects();
        this.args = args;
        this.AllowDrop = true;
        this.DragEnter += new DragEventHandler(this.Form1_DragEnter);
        this.DragDrop += new DragEventHandler(this.Form1_DragDrop);
        this.mainThreadTaskQueue = new Queue<Form1.MainThreadTask>();
        Form1.debugLogger.Add("Form1() Constructor", "Constructor for the main Form.", DebugLogger.LogType.Secondary);
        ExceptionForm.form1 = this;
        str = "splashForm.Show";
        this.splashForm = splashForm;
        splashForm.Show();
        str = "InitializeComponent";
        this.InitializeComponent();
        this.settingsManager = new SettingsManager(this.FileAssociations);
        Form1.debugLogger.Add("Form1() Constructor", "SettingsManager created.", DebugLogger.LogType.Secondary);
        this.timer1 = new System.Windows.Forms.Timer();
        this.timer1.Interval = 16;
        this.timer1.Tick += new EventHandler(this.on_timerTick);
        this.timer1.Start();
        this.MouseWheel += new MouseEventHandler(this.Form1_MouseWheel);
        this.fps_lock_watch = new Stopwatch();
        this.fps_frame_counter = new Stopwatch();
      }
      catch (Exception ex)
      {
        string extra_info = str;
        ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void Form1_DragEnter(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        return;
      e.Effect = DragDropEffects.Copy;
    }

    private void Form1_DragDrop(object sender, DragEventArgs e)
    {
      string[] data = (string[]) e.Data.GetData(DataFormats.FileDrop);
      if (data != null && data.Length != 0 && data[0] != null)
        this.model_loading_manager.LoadModelIntoPrinter(data[0]);
      else
        this.informationbox.AddMessageToQueue("Unable to find model.");
    }

    public void StopTimers()
    {
      if (this.timer1 == null)
        return;
      this.timer1.Stop();
    }

    private void on_timerTick(object sender, EventArgs e)
    {
      this.timer1.Stop();
      string str = (string) null;
      ++this.TEMPORARY_run_count;
      try
      {
        str = "on_timerTick::views.Process";
        if (this.TEMPORARY_run_count <= 3)
          Form1.debugLogger.Add(nameof (on_timerTick), str + (object) this.TEMPORARY_run_count, DebugLogger.LogType.Primary);
        str = "on_timerTick::Refresh";
        if (!this.contextMenuStrip1.Visible)
          this.Refresh();
        if (this.TEMPORARY_run_count <= 3)
          Form1.debugLogger.Add(nameof (on_timerTick), str + (object) this.TEMPORARY_run_count, DebugLogger.LogType.Primary);
        str = "on_timerTick::RefreshViews";
        if (this.m_gui_host != null)
        {
          if (this.resized)
          {
            this.m_gui_host.RefreshViews();
            this.resized = false;
          }
          str = "on_timerTick::m_gui_host.OnUpdate()";
          this.m_gui_host.OnUpdate();
        }
        if (this.TEMPORARY_run_count <= 3)
          Form1.debugLogger.Add(nameof (on_timerTick), str + (object) this.TEMPORARY_run_count, DebugLogger.LogType.Primary);
      }
      catch (Exception ex)
      {
        string extra_info = str;
        ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
      lock (this.mainThreadTaskQueue)
      {
        while (this.mainThreadTaskQueue.Count > 0)
        {
          Form1.MainThreadTask mainThreadTask = this.mainThreadTaskQueue.Dequeue();
          mainThreadTask.the_task(mainThreadTask.data);
        }
      }
      this.timer1.Start();
    }

    private void glControl1_Load(object sender, EventArgs e)
    {
      this.OpenGLConnection = new OpenGLConnection();
      try
      {
        this.splashForm.Hide();
        this.splashForm.Show();
        this.splashForm.TopMost = true;
        this.OpenGLConnection.OnLoad(this.glControl1, Form1.debugLogger);
        this.InitGUI();
        SlicerConnectionBase slicer_connection = (SlicerConnectionBase) new M3D.Slicer.Cura15_04.SlicerConnectionCura(Paths.WorkingFolder, Paths.ResourceFolder);
        this.model_loading_manager = new ModelLoadingManager();
        this.spooler_connection = new SpoolerConnection(this.messagebox, this.informationbox, this.settingsManager);
        this.SoftwareUpdater = new Updater(this, this.messagebox, this.spooler_connection, this.settingsManager);
        this.controlbar = new ControlBar(this, this.m_gui_host, this.settingsManager, this.messagebox, this.informationbox, this.spooler_connection, this.model_loading_manager, this.SoftwareUpdater);
        Frame frame = new Frame(24680);
        frame.SetPosition(0, 0);
        frame.RelativeWidth = 1f;
        frame.RelativeHeight = 1f;
        frame.BGColor = new Color4(0.913725f, 0.905882f, 0.9098f, 1f);
        this.m_gui_host.AddElement((Element2D) frame);
        this.libraryview = new LibraryView(10001, (Element2D) frame, this.glControl1, this.m_gui_host, this.informationbox, this.model_loading_manager);
        this.m_gui_host.SetFocus(1001);
        this.m_gui_host.Refresh();
        Form1.debugLogger.Add("glControl1_Load()", "LibraryView created.", DebugLogger.LogType.Secondary);
        this.printerView = new PrinterView(this, this.m_gui_host, this.OpenGLConnection, this.spooler_connection, slicer_connection, this.model_loading_manager, this.messagebox, this.informationbox, this.settingsManager, this.libraryview);
        this.printerView.SetViewPointPos(0.0f, 100f, 400f);
        Form1.debugLogger.Add("glControl1_Load()", "GLPrinterView created.", DebugLogger.LogType.Secondary);
        frame.AddChildElement((Element2D) this.printerView);
        frame.AddChildElement((Element2D) this.libraryview);
        Form1.debugLogger.Add("glControl1_Load()", "Views added to background view.", DebugLogger.LogType.Secondary);
        this.model_loading_manager.Init(this.settingsManager, this.libraryview, this.printerView, this.messagebox, this.informationbox);
        Form1.debugLogger.Add("glControl1_Load()", "Model Loading Manager Initialized.", DebugLogger.LogType.Secondary);
        this.printer_status_dialog_organizer = new PrinterStatusDialogOrganizer(this.spooler_connection, this.model_loading_manager, this.settingsManager, this, this.m_gui_host, this.printerView, this.messagebox);
        Form1.debugLogger.Add("glControl1_Load()", "PrinterStatusDialogOrganizer Initialized.", DebugLogger.LogType.Secondary);
        this.spooler_connection.SpoolerStartUp(Form1.debugLogger);
        Form1.debugLogger.Add("glControl1_Load()", "spooler_connection.SpoolerStartUp() completed.", DebugLogger.LogType.Secondary);
        this.controlbar.UpdateSettings();
        Form1.debugLogger.Add("glControl1_Load()", "controlbar.UpdateSettings() completed.", DebugLogger.LogType.Secondary);
        if (this.settingsManager.CurrentAppearanceSettings.StartFullScreen)
          this.WindowState = FormWindowState.Maximized;
        else
          this.WindowState = FormWindowState.Normal;
        this.splashForm.Close();
        Form1.debugLogger.Add("glControl1_Load()", "splash form closed.", DebugLogger.LogType.Secondary);
        this.glControl1.MakeCurrent();
        this.glControl1.VSync = false;
        Form1.debugLogger.Add("glControl1_Load()", "glcontrol sync", DebugLogger.LogType.Secondary);
        if (SplashFormFirstRun.WasRunForTheFirstTime)
        {
          WelcomeDialog welcomeDialog = new WelcomeDialog(1209, this.messagebox);
          welcomeDialog.Init(this.m_gui_host);
          this.m_gui_host.GlobalChildDialog += (Element2D) welcomeDialog;
        }
        else
          this.messagebox.AllowMessages = true;
        Form1.debugLogger.Add("glControl1_Load()", "Welcome Initialized", DebugLogger.LogType.Secondary);
        this.CheckFileAssociations();
        Form1.debugLogger.Add("glControl1_Load()", "File Associations Checked", DebugLogger.LogType.Secondary);
        int num = this.spooler_connection.PrintSpoolerClient.IsPrinting ? 0 : (!Program.runfirst_start ? 1 : 0);
        this.SoftwareUpdater.CheckForUpdate(false);
        Form1.debugLogger.Add("glControl1_Load()", "Checked for updates", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
      if (this.args.Length != 0)
        this.model_loading_manager.LoadModelIntoPrinter(this.args[0]);
      FileAssociationSingleInstance.OnNewInstance += new NewInstanceEvent(this.OnNewInstanceEvent);
    }

    public void SetToEditView()
    {
      if (this.printerView != null && this.printerView.TransitionViewState(ViewState.Active))
        Form1.debugLogger.Add("SetToEditView()", "Setting to edit view.", DebugLogger.LogType.Secondary);
      this.libraryview.TransitionViewState(ViewState.Hidden);
    }

    public void SetToLibraryView()
    {
      if (this.printerView.TransitionViewState(ViewState.Hidden))
        Form1.debugLogger.Add("SetToLibraryView()", "Setting to library view.", DebugLogger.LogType.Secondary);
      this.libraryview.TransitionViewState(ViewState.Active);
    }

    private void OnNewInstanceEvent(string[] args)
    {
      try
      {
        if (this.printerView.IsModelLoaded() && !this.settingsManager.CurrentAppearanceSettings.UseMultipleModels && !this.settingsManager.CurrentAppearanceSettings.ShowRemoveModelWarning)
        {
          if (new LoadNewModelForm().ShowDialog() != DialogResult.Yes || args.Length == 0)
            return;
          this.model_loading_manager.LoadModelIntoPrinter(args[0]);
        }
        else
          this.model_loading_manager.LoadModelIntoPrinter(args[0]);
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    private void CheckFileAssociations()
    {
      try
      {
        if (this.FileAssociations == null)
          return;
        string startupPath = Application.StartupPath;
        string str1 = this.FileAssociations.ExtensionOpenWith(".stl");
        string str2 = this.FileAssociations.ExtensionOpenWith(".obj");
        bool associationsDialog = this.settingsManager.Settings.miscSettings.FileAssociations.ShowFileAssociationsDialog;
        if (str1 == null || str1 != null && !str1.Contains(Application.ExecutablePath) || (str2 == null || str2 != null && !str2.Contains(Application.ExecutablePath)))
        {
          if (!associationsDialog && !SplashFormFirstRun.WasRunForTheFirstTime)
            return;
          AssociationsForm associationsForm = new AssociationsForm(this.settingsManager, this.messagebox, this.FileAssociations, Application.ExecutablePath, startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
        }
        else
        {
          this.FileAssociations.Set3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.stl)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
          this.FileAssociations.Set3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.obj)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
        }
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public IFileAssociations FileAssociations
    {
      get
      {
        return this.myFileAssociations;
      }
    }

    private void InitializePlatformSpecificObjects()
    {
      this.myFileAssociations = (IFileAssociations) null;
      this.myStopShutdown = (IStopShutdown) new WinStopShutdown(this.Handle);
      this.myFileAssociations = (IFileAssociations) new WinFileAssociations();
    }

    private void glControl1_Paint(object sender, PaintEventArgs e)
    {
      try
      {
        if (!this.fps_lock_watch.IsRunning)
        {
          this.fps_lock_watch.Start();
        }
        else
        {
          this.elapsed = (double) this.fps_lock_watch.ElapsedTicks / (double) Stopwatch.Frequency;
          if (this.elapsed + this.elapsed_frame >= 1.0 / 60.0)
          {
            this.fps_lock_watch.Reset();
            this.fps_lock_watch.Start();
            this.fps_frame_counter.Reset();
            this.fps_frame_counter.Start();
            this.OpenGLConnection.OnPaint((OpenGLConnection.RenderTaskDelegate) (() =>
            {
              if (this.resized)
                return;
              this.m_gui_host.Render();
            }));
            this.fps_frame_counter.Stop();
            this.elapsed_frame = (double) this.fps_frame_counter.ElapsedTicks / (double) Stopwatch.Frequency;
          }
          else
            Thread.Sleep(5);
        }
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    private void Form1_MouseDown(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Down;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      this.m_gui_host.OnMouseCommand(mouseevent);
    }

    private void Form1_MouseMove(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Move;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      try
      {
        this.m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseUp(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Up;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      try
      {
        this.m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseLeave(object sender, EventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Leave;
      mouseevent.pos.x = 0;
      mouseevent.pos.y = 0;
      mouseevent.button = MouseButton.None;
      mouseevent.num_clicks = 0;
      mouseevent.delta = 0;
      try
      {
        this.m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseWheel(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.MouseWheel;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.num_clicks = 0;
      mouseevent.button = MouseButton.None;
      mouseevent.delta = e.Delta;
      this.contextMenuStrip1.Visible = false;
      try
      {
        this.m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
      this.Refresh();
    }

    private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (!this.infocus)
        return;
      try
      {
        this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new InputKeyEvent(e.KeyChar, this.shift, this.alt, this.ctrl));
      }
      catch (Exception ex)
      {
      }
    }

    private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      switch (e.KeyData & Keys.KeyCode)
      {
        case Keys.ShiftKey:
        case Keys.ControlKey:
        case Keys.Menu:
        case Keys.End:
        case Keys.Home:
        case Keys.Left:
        case Keys.Up:
        case Keys.Right:
        case Keys.Down:
          e.IsInputKey = true;
          break;
      }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      try
      {
        if (keyData == Keys.Tab)
        {
          this.tab = true;
          this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new InputKeyEvent(' ', this.shift, this.alt, this.ctrl, this.tab));
        }
      }
      catch (Exception ex)
      {
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (!this.infocus)
        return;
      try
      {
        if (this.keypress_stopwatch == null)
          this.keypress_stopwatch = new Stopwatch();
        switch (e.KeyCode & Keys.KeyCode)
        {
          case Keys.ShiftKey:
            this.shift = true;
            break;
          case Keys.ControlKey:
            this.ctrl = true;
            break;
          case Keys.Menu:
            this.alt = true;
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Alt, this.shift, this.alt, this.ctrl));
            break;
          case Keys.End:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.End, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Home:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Home, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Left:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Left, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Up:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Up, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Right:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Right, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Down:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Down, this.shift, this.alt, this.ctrl));
            break;
          case Keys.Delete:
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new CommandKeyEvent(KeyboardCommandKey.Delete, this.shift, this.alt, this.ctrl));
            break;
          case Keys.C:
            if (!this.alt && !this.ctrl)
              break;
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new InputKeyEvent('C', this.shift, this.alt, this.ctrl));
            break;
          case Keys.V:
            if (!this.alt && !this.ctrl)
              break;
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new InputKeyEvent('V', this.shift, this.alt, this.ctrl));
            break;
          case Keys.X:
            if (!this.alt && !this.ctrl)
              break;
            this.m_gui_host.OnKeyboardEvent((KeyboardEvent) new InputKeyEvent('X', this.shift, this.alt, this.ctrl));
            break;
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (!this.infocus)
        return;
      switch (e.KeyCode & Keys.KeyCode)
      {
        case Keys.ShiftKey:
          this.shift = false;
          break;
        case Keys.ControlKey:
          this.ctrl = false;
          break;
        case Keys.Menu:
          this.alt = false;
          break;
      }
    }

    private void OnLeaveFocus(object sender, EventArgs e)
    {
      this.infocus = false;
      this.shift = false;
      this.alt = false;
      this.ctrl = false;
    }

    private void OnEnterFocus(object sender, EventArgs e)
    {
      this.infocus = true;
    }

    private void glControl1_Resize(object sender, EventArgs e)
    {
      if (this.m_gui_host != null)
        this.m_gui_host.OnResize(this.glControl1.Width, this.glControl1.Height);
      this.resized = true;
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
      FileAssociationSingleInstance.UnRegisterAsSingleInstance();
      if (this.controlbar != null)
        this.controlbar.SaveSettings();
      if (this.model_loading_manager != null)
        this.model_loading_manager.OnShutdown();
      if (this.settingsManager != null)
        this.settingsManager.OnShutdown();
      if (this.spooler_connection == null)
        return;
      this.spooler_connection.OnShutdown();
    }

    private void InitGUI()
    {
      Locale.GlobalLocale = new Locale(Path.Combine(Paths.ReadOnlyDataFolder, "locales", "English.locale.xml"));
      this.m_gui_host = new GUIHost(Locale.GlobalLocale, 11f, Paths.PublicDataFolder, this.glControl1);
      this.m_gui_host.OnResize(this.glControl1.Width, this.glControl1.Height);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      this.messagebox = new PopupMessageBox(0);
      this.messagebox.Init(this.m_gui_host);
      this.informationbox = new MessagePopUp(0, this.settingsManager);
      this.informationbox.Init(this.m_gui_host);
    }

    public void OpenSettingsDialogCalibrationPage()
    {
      if (this.controlbar == null)
        return;
      this.controlbar.OpenSettingsDialogCalibrationPage();
    }

    public void OpenSettingsDialogAdvancedCalibrate(string printer_serial_number)
    {
      if (this.controlbar == null)
        return;
      this.controlbar.OpenSettingsDialogAdvancedCalibrate(printer_serial_number);
    }

    public void OpenSettingsDialogFilamentManagement()
    {
      if (this.controlbar == null)
        return;
      this.controlbar.OpenSettingsDialogFilamentManagement();
    }

    public void OpenSettingsDialogChangeFilament()
    {
      if (this.controlbar == null)
        return;
      this.controlbar.OpenSettingsDialogChangeFilament();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (this.force_close || this.spooler_connection == null || (this.myStopShutdown == null || e.CloseReason != CloseReason.WindowsShutDown) || (this.spooler_connection.PrintSpoolerClient == null || !this.spooler_connection.PrintSpoolerClient.IsPrinting))
        return;
      this.myStopShutdown.CreateShutdownMessage("This app is preventing shutdown because this will cancel print jobs.");
      ConfirmShutdownForm confirmShutdownForm = new ConfirmShutdownForm();
      int num1 = (int) confirmShutdownForm.ShowDialog();
      if (confirmShutdownForm.shutdown)
      {
        this.force_close = true;
        int num2 = (int) this.spooler_connection.PrintSpoolerClient.ForceSpoolerShutdown();
      }
      else
        e.Cancel = true;
      this.myStopShutdown.DestroyShutdownMessage();
    }

    public void AddTask(Form1.Task task, object data)
    {
      lock (this.mainThreadTaskQueue)
        this.mainThreadTaskQueue.Enqueue(new Form1.MainThreadTask(task, data));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Form1));
      this.glControl1 = new GLControl(new GraphicsMode(new ColorFormat(32), 24, 0, 8), 3, 0, GraphicsContextFlags.ForwardCompatible);
      this.contextMenuStrip1 = new ContextMenuStrip(this.components);
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      this.glControl1.BackColor = Color.Black;
      componentResourceManager.ApplyResources((object) this.glControl1, "glControl1");
      this.glControl1.Name = "glControl1";
      this.glControl1.VSync = false;
      this.glControl1.Load += new EventHandler(this.glControl1_Load);
      this.glControl1.Paint += new PaintEventHandler(this.glControl1_Paint);
      this.glControl1.Enter += new EventHandler(this.OnEnterFocus);
      this.glControl1.KeyDown += new KeyEventHandler(this.OnKeyDown);
      this.glControl1.KeyPress += new KeyPressEventHandler(this.OnKeyPress);
      this.glControl1.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.glControl1.Leave += new EventHandler(this.OnLeaveFocus);
      this.glControl1.MouseDown += new MouseEventHandler(this.Form1_MouseDown);
      this.glControl1.MouseLeave += new EventHandler(this.Form1_MouseLeave);
      this.glControl1.MouseMove += new MouseEventHandler(this.Form1_MouseMove);
      this.glControl1.MouseUp += new MouseEventHandler(this.Form1_MouseUp);
      this.glControl1.PreviewKeyDown += new PreviewKeyDownEventHandler(this.OnPreviewKeyDown);
      this.glControl1.Resize += new EventHandler(this.glControl1_Resize);
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.glControl1);
      this.Name = nameof (Form1);
      this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
      this.FormClosed += new FormClosedEventHandler(this.Form1_FormClosed);
      this.Load += new EventHandler(this.Form1_Load);
      this.MouseDown += new MouseEventHandler(this.Form1_MouseDown);
      this.MouseLeave += new EventHandler(this.Form1_MouseLeave);
      this.MouseMove += new MouseEventHandler(this.Form1_MouseMove);
      this.MouseUp += new MouseEventHandler(this.Form1_MouseUp);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    public struct MainThreadTask
    {
      public Form1.Task the_task;
      public object data;

      public MainThreadTask(Form1.Task task, object data)
      {
        this.the_task = task;
        this.data = data;
      }
    }

    public delegate void Task(object data);
  }
}
