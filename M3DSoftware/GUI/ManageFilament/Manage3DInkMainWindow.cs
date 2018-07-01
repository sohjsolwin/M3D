// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Manage3DInkMainWindow
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.ManageFilament.Child_Frames;
using M3D.GUI.Views;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System.Collections.Generic;

namespace M3D.GUI.ManageFilament
{
  public class Manage3DInkMainWindow : BorderedImageFrame
  {
    public FilamentSpool previous_spool = new FilamentSpool();
    private List<Manage3DInkChildWindow> frames = new List<Manage3DInkChildWindow>();
    public const float DEFAULT_PRIME_AMOUNT = 50f;
    private Manage3DInkChildWindow current_frame;
    private int previously_requested_temperature;
    private int target_temp;
    private PrinterObject last_selected_printer;
    private SpoolerConnection spooler_connection;

    public Manage3DInkMainWindow(int ID, GUIHost host, SettingsManager main_controller, PopupMessageBox messagebox, MessagePopUp infobox, SpoolerConnection spooler_connection)
      : base(ID, (Element2D) null)
    {
      this.spooler_connection = spooler_connection;
      spooler_connection.OnPrinterMessage += new SpoolerConnection.PrinterMessageCallback(this.PrinterMessageCallback);
      this.Init(host, main_controller, messagebox, infobox);
    }

    private void Init(GUIHost host, SettingsManager settingsManager, PopupMessageBox messagebox, MessagePopUp infobox)
    {
      base.SetVisible(false);
      this.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      this.SetSize(792, 356);
      TextWidget textWidget = new TextWidget(9);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Manage Filament Page";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget);
      ButtonWidget buttonWidget = new ButtonWidget(4);
      buttonWidget.X = -40;
      buttonWidget.Y = 4;
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(this.StartupPageButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      Frame frame1 = new Frame(0);
      frame1.CenterHorizontallyInParent = true;
      frame1.CenterVerticallyInParent = true;
      frame1.RelativeHeight = 0.9f;
      frame1.RelativeWidth = 0.9f;
      this.AddChildElement((Element2D) frame1);
      this.frames.Add((Manage3DInkChildWindow) new FilamentStartupPage(0, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentHeatingNozzle(1, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentRetractingFilament(2, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentHasRetractedFilament(3, host, this, settingsManager));
      this.frames.Add((Manage3DInkChildWindow) new FilamentInsertNewFilament(4, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentHasFilamentExited(5, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentIsThereFilament(6, host, this, settingsManager));
      this.frames.Add((Manage3DInkChildWindow) new FilamentFilamentColor(7, host, this, settingsManager));
      this.frames.Add((Manage3DInkChildWindow) new FilamentWaitingPage(8, host, this, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentChangeFilamentDetails(9, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentPrimingNozzle(10, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentCheatCodePage(11, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentRaisingExtruder(12, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentFilamentLocation(13, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentInternalSpoolInstructions(14, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentCloseBedInstructions(15, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentRemoveInternalSpoolInstructions(16, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentCleanNozzle(17, host, this));
      this.frames.Add((Manage3DInkChildWindow) new FilamentFilamentSpoolSize(18, host, this, settingsManager, messagebox));
      this.frames.Add((Manage3DInkChildWindow) new FilamentIsNewSpoolPage(19, host, this, settingsManager, infobox));
      foreach (Manage3DInkChildWindow frame2 in this.frames)
      {
        frame2.Init();
        frame2.Visible = false;
        frame1.AddChildElement((Element2D) frame2);
      }
    }

    public override ElementType GetElementType()
    {
      return ElementType.ManageFilamentDialog;
    }

    public void StartupPageButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        if (button.ID != 4)
          return;
        this.Close();
      }
      else
      {
        if (button.ID != 4)
          return;
        this.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
        selectedPrinter.MarkedAsBusy = false;
        this.Close();
      }
    }

    private void PrinterMessageCallback(SpoolerMessage message)
    {
      if (this.current_frame == null || message.Type != MessageType.PrinterTimeout && message.Type != MessageType.BedLocationMustBeCalibrated && (message.Type != MessageType.BedOrientationMustBeCalibrated && message.Type != MessageType.CheckGantryClips) && message.Type != MessageType.PrinterError)
        return;
      switch ((Manage3DInkMainWindow.PageID) this.current_frame.ID)
      {
        case Manage3DInkMainWindow.PageID.Page0_StartupPage:
        case Manage3DInkMainWindow.PageID.Page1_HeatingNozzle:
        case Manage3DInkMainWindow.PageID.Page2_RetractingFilament:
        case Manage3DInkMainWindow.PageID.Page3_HasRetractedFilament:
        case Manage3DInkMainWindow.PageID.Page4_InsertNewFilament:
        case Manage3DInkMainWindow.PageID.Page5_HasFilamentExited:
          PrinterObject selectedPrinter = this.GetSelectedPrinter();
          if (selectedPrinter == null || !(selectedPrinter.Info.serial_number == message.SerialNumber))
            break;
          this.ResetToStartup();
          break;
      }
    }

    public override void SetVisible(bool bVisible)
    {
      PrinterObject selectedPrinter = this.GetSelectedPrinter();
      if (!this.Visible & bVisible)
        this.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      else if (this.Visible && !bVisible && (selectedPrinter != null && selectedPrinter.HasLock) && !selectedPrinter.isBusy)
      {
        int num = (int) selectedPrinter.ReleaseLock(new AsyncCallback(this.AfterRelease), (object) selectedPrinter);
      }
      base.SetVisible(bVisible);
    }

    public void AfterRelease(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == this.GetSelectedPrinter())
        asyncState.MarkedAsBusy = false;
      this.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
    }

    public override void Close()
    {
      if (this.current_frame.ID != 0)
        return;
      this.Visible = false;
      GUIHost host = this.current_frame.Host;
      if (!host.HasChildDialog)
        return;
      host.GlobalChildDialog -= (Element2D) this;
    }

    public void LockPrinterAndGotoPage(PrinterObject printer, Manage3DInkMainWindow.PageID page, Mangage3DInkStageDetails details)
    {
      int num = (int) printer.AcquireLock(new AsyncCallback(this.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(printer, page, details));
    }

    public void ResetToStartup()
    {
      PrinterObject selectedPrinter = this.GetSelectedPrinter();
      if (selectedPrinter != null && selectedPrinter.Info.Status != PrinterStatus.Firmware_Printing && (selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPausedProcessing))
      {
        int num = (int) selectedPrinter.SendEmergencyStop((AsyncCallback) null, (object) null);
      }
      FilamentWaitingPage.CurrentWaitingText = "Please Wait";
      this.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
    }

    public void ActivateFrame(Manage3DInkMainWindow.PageID frame, Mangage3DInkStageDetails details)
    {
      if (this.current_frame != null)
      {
        this.current_frame.Visible = false;
        this.current_frame = (Manage3DInkChildWindow) null;
      }
      this.current_frame = this.frames[(int) frame];
      this.current_frame.Visible = true;
      this.current_frame.OnActivate(details);
    }

    public PrinterObject GetSelectedPrinter()
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      bool flag = false;
      if (this.last_selected_printer != selectedPrinter)
      {
        if (this.last_selected_printer != null && this.last_selected_printer.LockStatus == PrinterLockStatus.WeOwnLocked)
        {
          int num = (int) this.last_selected_printer.ReleaseLock((AsyncCallback) null, (object) null);
        }
        this.last_selected_printer = selectedPrinter;
        flag = true;
      }
      if (selectedPrinter == null)
      {
        if (this.current_frame == null || this.current_frame.ID != 0)
          this.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
      }
      else if (((!selectedPrinter.isConnected() || selectedPrinter.PrinterState == PrinterObject.State.IsUpdatingFirmware ? 1 : (selectedPrinter.Info.InBootloaderMode ? 1 : 0)) | (flag ? 1 : 0)) != 0 && (this.current_frame == null || this.current_frame.ID != 8))
        this.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      if (flag)
        return (PrinterObject) null;
      return selectedPrinter;
    }

    public void GotoPageAfterOperation(IAsyncCallResult ar)
    {
      Manage3DInkMainWindow.PageAfterLockDetails asyncState = ar.AsyncState as Manage3DInkMainWindow.PageAfterLockDetails;
      if (asyncState != null && asyncState.printer.CheckForLockError(ar))
        this.ActivateFrame(asyncState.nextPage, asyncState.details);
      else
        this.ResetToStartup();
    }

    public void TurnOffHeater(AsyncCallback callback, object state)
    {
      PrinterObject selectedPrinter = this.GetSelectedPrinter();
      if (selectedPrinter == null || selectedPrinter == null)
        return;
      selectedPrinter.TurnOffHeater(new AsyncCallback(this.AfterHeaterTurnedOff), (object) new Manage3DInkMainWindow.heaterData(callback, state, selectedPrinter, 0));
    }

    private void CallInternalHeaterMethod(IAsyncCallResult ar)
    {
      Manage3DInkMainWindow.heaterData asyncState = ar.AsyncState as Manage3DInkMainWindow.heaterData;
      if (ar.CallResult != CommandResult.Success)
        asyncState.callback(ar);
      else if (asyncState.temperature > 0)
        asyncState.printer.TurnOnHeater(asyncState.callback, asyncState.state, asyncState.temperature);
      else
        asyncState.printer.TurnOffHeater(asyncState.callback, asyncState.state);
    }

    private void AfterHeaterTurnedOff(IAsyncCallResult ar)
    {
      Manage3DInkMainWindow.heaterData asyncState = ar.AsyncState as Manage3DInkMainWindow.heaterData;
      if (ar.CallResult != CommandResult.Success)
      {
        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
      {
        int num = (int) asyncState.printer.SendManualGCode(asyncState.callback, asyncState.state, "G4 S2", "M107");
      }
    }

    public void TurnOnHeater(AsyncCallback callback, object state, int temperature, FilamentSpool.TypeEnum filament_type)
    {
      PrinterObject selectedPrinter = this.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      List<string> stringList = new List<string>();
      if (this.previously_requested_temperature > temperature)
      {
        if (this.previous_spool.filament_type == FilamentSpool.TypeEnum.ABS && filament_type == FilamentSpool.TypeEnum.PLA)
        {
          stringList.Add("M106");
          stringList.Add("G4 S30");
          stringList.Add("M107");
        }
        stringList.Add("M104 S0");
        stringList.Add("G4 S1");
      }
      this.previously_requested_temperature = temperature;
      this.target_temp = temperature + 5;
      if (filament_type == FilamentSpool.TypeEnum.PLA)
        stringList.Add("M106");
      if (this.target_temp > 285)
        this.target_temp = 285;
      else if (this.target_temp < 150)
        this.target_temp = 150;
      if (stringList.Count > 0)
      {
        int num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(this.CallInternalHeaterMethod), (object) new Manage3DInkMainWindow.heaterData(callback, state, selectedPrinter, this.target_temp), stringList.ToArray());
      }
      else
        selectedPrinter.TurnOnHeater(callback, state, this.target_temp);
    }

    public void HeaterStartedSuccess(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Success)
        return;
      this.ResetToStartup();
    }

    public int TargetTemperature
    {
      get
      {
        return this.target_temp;
      }
    }

    public enum ControlIDs
    {
      Close,
      LastIDInList,
    }

    public enum Mode
    {
      None,
      RemoveFilament,
      AddFilament,
      SetFilamentLocationInsertingNew,
      SetFilamentLocationAlreadyInserted,
      SetDetails,
    }

    public enum PageID
    {
      Page0_StartupPage,
      Page1_HeatingNozzle,
      Page2_RetractingFilament,
      Page3_HasRetractedFilament,
      Page4_InsertNewFilament,
      Page5_HasFilamentExited,
      Page6_IsThereFilament,
      Page7_FilamentColor,
      Page8_WaitingPage,
      Page9_ChangeFilamentDetails,
      Page10_PrimingNozzle,
      Page11_CheatCodePage,
      Page12_RaisingExtruder,
      Page13_FilamentLocation,
      Page14_InternalSpoolInstructions,
      Page15_CloseBedInstructions,
      Page16_RemoveInternalSpoolInstructions,
      Page17_CleanNozzle,
      Page18_FilamentSpoolSize,
      Page19_FilamentIsNewSpoolPage,
    }

    public class PageAfterLockDetails
    {
      public PrinterObject printer;
      public Manage3DInkMainWindow.PageID nextPage;
      public Mangage3DInkStageDetails details;

      public PageAfterLockDetails(PrinterObject printer, Manage3DInkMainWindow.PageID nextPage, Mangage3DInkStageDetails details)
      {
        this.printer = printer;
        this.nextPage = nextPage;
        this.details = details;
      }
    }

    public class heaterData
    {
      public AsyncCallback callback;
      public object state;
      public int temperature;
      public PrinterObject printer;

      public heaterData(AsyncCallback callback, object state, PrinterObject printer, int temperature)
      {
        this.callback = callback;
        this.state = state;
        this.temperature = temperature;
        this.printer = printer;
      }
    }
  }
}
