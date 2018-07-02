using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System.Diagnostics;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentWaitingPage : Manage3DInkChildWindow
  {
    public static string CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
    public const string WaitingDefaultText = "Please wait. The printer is busy perfoming the requested actions.";
    private TextWidget WaitingText;
    private PopupMessageBox messagebox;
    private bool finishedWaiting;
    private Stopwatch m_osIdleStopwatchTimer;

    public FilamentWaitingPage(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.messagebox = messagebox;
      m_osIdleStopwatchTimer = new Stopwatch();
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Please wait. The printer is busy perfoming the requested actions.", "", false, false, false, false, false, false);
      WaitingText = (TextWidget)FindChildElement(1);
      var childElement = (Frame)FindChildElement(2);
      if (childElement == null)
      {
        return;
      }

      var spriteAnimationWidget = new SpriteAnimationWidget(11);
      spriteAnimationWidget.Init(Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.CenterVerticallyInParent = true;
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      childElement.AddChildElement(spriteAnimationWidget);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      WaitingText.Text = FilamentWaitingPage.CurrentWaitingText;
      m_osIdleStopwatchTimer.Stop();
      finishedWaiting = false;
    }

    public override void OnUpdate()
    {
      if (Visible)
      {
        PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
        if (selectedPrinter == null)
        {
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          return;
        }
        if ((selectedPrinter.Info.Status == PrinterStatus.Firmware_Ready || selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused) && !selectedPrinter.WaitingForCommandToComplete && ((CurrentDetails.waitCondition != null ? (CurrentDetails.waitCondition() ? 1 : 0) : 1) != 0 && !finishedWaiting))
        {
          if (CurrentDetails.pageAfterWait == Manage3DInkMainWindow.PageID.Page0_StartupPage)
          {
            if (selectedPrinter.LockStatus == PrinterLockStatus.Unlocked)
            {
              finishedWaiting = true;
              var num = (int) selectedPrinter.AcquireLock(new AsyncCallback(DoStartUpSequenceCallsAfterLock), selectedPrinter);
            }
            else if (selectedPrinter.LockStatus == PrinterLockStatus.WeOwnLocked)
            {
              finishedWaiting = true;
              DoStartUpSequence(selectedPrinter);
            }
            else
            {
              MainWindow.ActivateFrame(CurrentDetails.pageAfterWait, CurrentDetails);
            }
          }
          else
          {
            finishedWaiting = true;
            MainWindow.ActivateFrame(CurrentDetails.pageAfterWait, CurrentDetails);
          }
        }
        else if (!selectedPrinter.WaitingForCommandToComplete && CurrentDetails.waitCondition == null)
        {
          MainWindow.ActivateFrame(CurrentDetails.pageAfterWait, CurrentDetails);
        }
        else if (selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle)
        {
          if (!m_osIdleStopwatchTimer.IsRunning)
          {
            m_osIdleStopwatchTimer.Restart();
          }
          else if (m_osIdleStopwatchTimer.ElapsedMilliseconds > 3000L)
          {
            var num = (int) selectedPrinter.BreakLock(null, null);
            finishedWaiting = true;
          }
        }
      }
      base.OnUpdate();
    }

    private void DoStartUpSequence(PrinterObject printer)
    {
      if (printer == null)
      {
        return;
      }

      var num = (int) printer.SendManualGCode(new AsyncCallback(OnStartUpSuccess), printer, "M117", "M114", "M576", "M104 S0");
    }

    private void DoStartUpSequenceCallsAfterLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      switch (ar.CallResult)
      {
        case CommandResult.Success_LockAcquired:
          DoStartUpSequence(asyncState);
          break;
        default:
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          break;
      }
    }

    private void OnStartUpSuccess(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState.LockStatus != PrinterLockStatus.WeOwnLocked)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock(new AsyncCallback(MainWindow.AfterRelease), asyncState);
    }

    public enum ControlIDs
    {
      Progress = 11, // 0x0000000B
    }
  }
}
