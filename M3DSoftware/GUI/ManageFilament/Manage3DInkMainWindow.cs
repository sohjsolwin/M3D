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
      spooler_connection.OnPrinterMessage += new SpoolerConnection.PrinterMessageCallback(PrinterMessageCallback);
      Init(host, main_controller, messagebox, infobox);
    }

    private void Init(GUIHost host, SettingsManager settingsManager, PopupMessageBox messagebox, MessagePopUp infobox)
    {
      base.SetVisible(false);
      Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      SetSize(792, 356);
      var textWidget = new TextWidget(9);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Manage Filament Page";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      AddChildElement((Element2D) textWidget);
      var buttonWidget = new ButtonWidget(4)
      {
        X = -40,
        Y = 4
      };
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(StartupPageButtonCallback));
      AddChildElement((Element2D) buttonWidget);
      var frame1 = new Frame(0)
      {
        CenterHorizontallyInParent = true,
        CenterVerticallyInParent = true,
        RelativeHeight = 0.9f,
        RelativeWidth = 0.9f
      };
      AddChildElement((Element2D) frame1);
      frames.Add((Manage3DInkChildWindow) new FilamentStartupPage(0, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentHeatingNozzle(1, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentRetractingFilament(2, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentHasRetractedFilament(3, host, this, settingsManager));
      frames.Add((Manage3DInkChildWindow) new FilamentInsertNewFilament(4, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentHasFilamentExited(5, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentIsThereFilament(6, host, this, settingsManager));
      frames.Add((Manage3DInkChildWindow) new FilamentFilamentColor(7, host, this, settingsManager));
      frames.Add((Manage3DInkChildWindow) new FilamentWaitingPage(8, host, this, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentChangeFilamentDetails(9, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentPrimingNozzle(10, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentCheatCodePage(11, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentRaisingExtruder(12, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentFilamentLocation(13, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentInternalSpoolInstructions(14, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentCloseBedInstructions(15, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentRemoveInternalSpoolInstructions(16, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentCleanNozzle(17, host, this));
      frames.Add((Manage3DInkChildWindow) new FilamentFilamentSpoolSize(18, host, this, settingsManager, messagebox));
      frames.Add((Manage3DInkChildWindow) new FilamentIsNewSpoolPage(19, host, this, settingsManager, infobox));
      foreach (Manage3DInkChildWindow frame2 in frames)
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
      PrinterObject selectedPrinter = GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        if (button.ID != 4)
        {
          return;
        }

        Close();
      }
      else
      {
        if (button.ID != 4)
        {
          return;
        }

        ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
        selectedPrinter.MarkedAsBusy = false;
        Close();
      }
    }

    private void PrinterMessageCallback(SpoolerMessage message)
    {
      if (current_frame == null || message.Type != MessageType.PrinterTimeout && message.Type != MessageType.BedLocationMustBeCalibrated && (message.Type != MessageType.BedOrientationMustBeCalibrated && message.Type != MessageType.CheckGantryClips) && message.Type != MessageType.PrinterError)
      {
        return;
      }

      switch ((Manage3DInkMainWindow.PageID)current_frame.ID)
      {
        case Manage3DInkMainWindow.PageID.Page0_StartupPage:
        case Manage3DInkMainWindow.PageID.Page1_HeatingNozzle:
        case Manage3DInkMainWindow.PageID.Page2_RetractingFilament:
        case Manage3DInkMainWindow.PageID.Page3_HasRetractedFilament:
        case Manage3DInkMainWindow.PageID.Page4_InsertNewFilament:
        case Manage3DInkMainWindow.PageID.Page5_HasFilamentExited:
          PrinterObject selectedPrinter = GetSelectedPrinter();
          if (selectedPrinter == null || !(selectedPrinter.Info.serial_number == message.SerialNumber))
          {
            break;
          }

          ResetToStartup();
          break;
      }
    }

    public override void SetVisible(bool bVisible)
    {
      PrinterObject selectedPrinter = GetSelectedPrinter();
      if (!Visible & bVisible)
      {
        ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      }
      else if (Visible && !bVisible && (selectedPrinter != null && selectedPrinter.HasLock) && !selectedPrinter.isBusy)
      {
        var num = (int) selectedPrinter.ReleaseLock(new AsyncCallback(AfterRelease), (object) selectedPrinter);
      }
      base.SetVisible(bVisible);
    }

    public void AfterRelease(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == GetSelectedPrinter())
      {
        asyncState.MarkedAsBusy = false;
      }

      ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
    }

    public override void Close()
    {
      if (current_frame.ID != 0)
      {
        return;
      }

      Visible = false;
      GUIHost host = current_frame.Host;
      if (!host.HasChildDialog)
      {
        return;
      }

      host.GlobalChildDialog -= (Element2D) this;
    }

    public void LockPrinterAndGotoPage(PrinterObject printer, Manage3DInkMainWindow.PageID page, Mangage3DInkStageDetails details)
    {
      var num = (int) printer.AcquireLock(new AsyncCallback(GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(printer, page, details));
    }

    public void ResetToStartup()
    {
      PrinterObject selectedPrinter = GetSelectedPrinter();
      if (selectedPrinter != null && selectedPrinter.Info.Status != PrinterStatus.Firmware_Printing && (selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPausedProcessing))
      {
        var num = (int) selectedPrinter.SendEmergencyStop((AsyncCallback) null, (object) null);
      }
      FilamentWaitingPage.CurrentWaitingText = "Please Wait";
      ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
    }

    public void ActivateFrame(Manage3DInkMainWindow.PageID frame, Mangage3DInkStageDetails details)
    {
      if (current_frame != null)
      {
        current_frame.Visible = false;
        current_frame = (Manage3DInkChildWindow) null;
      }
      current_frame = frames[(int) frame];
      current_frame.Visible = true;
      current_frame.OnActivate(details);
    }

    public PrinterObject GetSelectedPrinter()
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      var flag = false;
      if (last_selected_printer != selectedPrinter)
      {
        if (last_selected_printer != null && last_selected_printer.LockStatus == PrinterLockStatus.WeOwnLocked)
        {
          var num = (int)last_selected_printer.ReleaseLock((AsyncCallback) null, (object) null);
        }
        last_selected_printer = selectedPrinter;
        flag = true;
      }
      if (selectedPrinter == null)
      {
        if (current_frame == null || current_frame.ID != 0)
        {
          ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
        }
      }
      else if (((!selectedPrinter.isConnected() || selectedPrinter.PrinterState == PrinterObject.State.IsUpdatingFirmware ? 1 : (selectedPrinter.Info.InBootloaderMode ? 1 : 0)) | (flag ? 1 : 0)) != 0 && (current_frame == null || current_frame.ID != 8))
      {
        ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      }

      if (flag)
      {
        return (PrinterObject) null;
      }

      return selectedPrinter;
    }

    public void GotoPageAfterOperation(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as Manage3DInkMainWindow.PageAfterLockDetails;
      if (asyncState != null && asyncState.printer.CheckForLockError(ar))
      {
        ActivateFrame(asyncState.nextPage, asyncState.details);
      }
      else
      {
        ResetToStartup();
      }
    }

    public void TurnOffHeater(AsyncCallback callback, object state)
    {
      PrinterObject selectedPrinter = GetSelectedPrinter();
      if (selectedPrinter == null || selectedPrinter == null)
      {
        return;
      }

      selectedPrinter.TurnOffHeater(new AsyncCallback(AfterHeaterTurnedOff), (object) new Manage3DInkMainWindow.heaterData(callback, state, selectedPrinter, 0));
    }

    private void CallInternalHeaterMethod(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as Manage3DInkMainWindow.heaterData;
      if (ar.CallResult != CommandResult.Success)
      {
        asyncState.callback(ar);
      }
      else if (asyncState.temperature > 0)
      {
        asyncState.printer.TurnOnHeater(asyncState.callback, asyncState.state, asyncState.temperature);
      }
      else
      {
        asyncState.printer.TurnOffHeater(asyncState.callback, asyncState.state);
      }
    }

    private void AfterHeaterTurnedOff(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as Manage3DInkMainWindow.heaterData;
      if (ar.CallResult != CommandResult.Success)
      {
        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
      {
        var num = (int) asyncState.printer.SendManualGCode(asyncState.callback, asyncState.state, "G4 S2", "M107");
      }
    }

    public void TurnOnHeater(AsyncCallback callback, object state, int temperature, FilamentSpool.TypeEnum filament_type)
    {
      PrinterObject selectedPrinter = GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      var stringList = new List<string>();
      if (previously_requested_temperature > temperature)
      {
        if (previous_spool.filament_type == FilamentSpool.TypeEnum.ABS && filament_type == FilamentSpool.TypeEnum.PLA)
        {
          stringList.Add("M106");
          stringList.Add("G4 S30");
          stringList.Add("M107");
        }
        stringList.Add("M104 S0");
        stringList.Add("G4 S1");
      }
      previously_requested_temperature = temperature;
      target_temp = temperature + 5;
      if (filament_type == FilamentSpool.TypeEnum.PLA)
      {
        stringList.Add("M106");
      }

      if (target_temp > 285)
      {
        target_temp = 285;
      }
      else if (target_temp < 150)
      {
        target_temp = 150;
      }

      if (stringList.Count > 0)
      {
        var num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(CallInternalHeaterMethod), (object) new Manage3DInkMainWindow.heaterData(callback, state, selectedPrinter, target_temp), stringList.ToArray());
      }
      else
      {
        selectedPrinter.TurnOnHeater(callback, state, target_temp);
      }
    }

    public void HeaterStartedSuccess(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Success)
      {
        return;
      }

      ResetToStartup();
    }

    public int TargetTemperature
    {
      get
      {
        return target_temp;
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
