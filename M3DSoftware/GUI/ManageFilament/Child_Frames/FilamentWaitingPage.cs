// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentWaitingPage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.m_osIdleStopwatchTimer = new Stopwatch();
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Please wait. The printer is busy perfoming the requested actions.", "", false, false, false, false, false, false);
      this.WaitingText = (TextWidget) this.FindChildElement(1);
      Frame childElement = (Frame) this.FindChildElement(2);
      if (childElement == null)
        return;
      SpriteAnimationWidget spriteAnimationWidget = new SpriteAnimationWidget(11);
      spriteAnimationWidget.Init(this.Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.CenterVerticallyInParent = true;
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      childElement.AddChildElement((Element2D) spriteAnimationWidget);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.WaitingText.Text = FilamentWaitingPage.CurrentWaitingText;
      this.m_osIdleStopwatchTimer.Stop();
      this.finishedWaiting = false;
    }

    public override void OnUpdate()
    {
      if (this.Visible)
      {
        PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
        if (selectedPrinter == null)
        {
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          return;
        }
        if ((selectedPrinter.Info.Status == PrinterStatus.Firmware_Ready || selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused) && !selectedPrinter.WaitingForCommandToComplete && ((this.CurrentDetails.waitCondition != null ? (this.CurrentDetails.waitCondition() ? 1 : 0) : 1) != 0 && !this.finishedWaiting))
        {
          if (this.CurrentDetails.pageAfterWait == Manage3DInkMainWindow.PageID.Page0_StartupPage)
          {
            if (selectedPrinter.LockStatus == PrinterLockStatus.Unlocked)
            {
              this.finishedWaiting = true;
              int num = (int) selectedPrinter.AcquireLock(new AsyncCallback(this.DoStartUpSequenceCallsAfterLock), (object) selectedPrinter);
            }
            else if (selectedPrinter.LockStatus == PrinterLockStatus.WeOwnLocked)
            {
              this.finishedWaiting = true;
              this.DoStartUpSequence(selectedPrinter);
            }
            else
              this.MainWindow.ActivateFrame(this.CurrentDetails.pageAfterWait, this.CurrentDetails);
          }
          else
          {
            this.finishedWaiting = true;
            this.MainWindow.ActivateFrame(this.CurrentDetails.pageAfterWait, this.CurrentDetails);
          }
        }
        else if (!selectedPrinter.WaitingForCommandToComplete && this.CurrentDetails.waitCondition == null)
          this.MainWindow.ActivateFrame(this.CurrentDetails.pageAfterWait, this.CurrentDetails);
        else if (selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle)
        {
          if (!this.m_osIdleStopwatchTimer.IsRunning)
            this.m_osIdleStopwatchTimer.Restart();
          else if (this.m_osIdleStopwatchTimer.ElapsedMilliseconds > 3000L)
          {
            int num = (int) selectedPrinter.BreakLock((AsyncCallback) null, (object) null);
            this.finishedWaiting = true;
          }
        }
      }
      base.OnUpdate();
    }

    private void DoStartUpSequence(PrinterObject printer)
    {
      if (printer == null)
        return;
      int num = (int) printer.SendManualGCode(new AsyncCallback(this.OnStartUpSuccess), (object) printer, "M117", "M114", "M576", "M104 S0");
    }

    private void DoStartUpSequenceCallsAfterLock(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      switch (ar.CallResult)
      {
        case CommandResult.Success_LockAcquired:
          this.DoStartUpSequence(asyncState);
          break;
        default:
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          break;
      }
    }

    private void OnStartUpSuccess(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState.LockStatus != PrinterLockStatus.WeOwnLocked)
        return;
      int num = (int) asyncState.ReleaseLock(new AsyncCallback(this.MainWindow.AfterRelease), (object) asyncState);
    }

    public enum ControlIDs
    {
      Progress = 11, // 0x0000000B
    }
  }
}
