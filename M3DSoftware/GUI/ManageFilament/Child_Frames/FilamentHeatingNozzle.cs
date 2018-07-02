using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Common;
using System;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentHeatingNozzle : Manage3DInkChildWindow
  {
    private PopupMessageBox messagebox;
    private SettingsManager settingsManager;
    private ProgressBarWidget progressBar;

    public FilamentHeatingNozzle(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.messagebox = messagebox;
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 9)
      {
        return;
      }

      MainWindow.ResetToStartup();
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Warming up", "Please wait while the nozzle is heated.\nBe careful. The nozzle may be very hot.", true, false, true, false, false, false);
      progressBar = (ProgressBarWidget)FindChildElement(4);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      if (CurrentDetails.current_spool == null)
      {
        MainWindow.ResetToStartup();
      }
      else if (!selectedPrinter.Info.extruder.Z_Valid)
      {
        messagebox.AddMessageToQueue("Sorry. The extruder can't move to a safe position for heating because the Z location has not be calibrated.", PopupMessageBox.MessageBoxButtons.OK);
        MainWindow.ResetToStartup();
      }
      else
      {
        var colors = (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), CurrentDetails.current_spool.filament_color_code);
        MainWindow.TurnOnHeater(new M3D.Spooling.Client.AsyncCallback(MainWindow.HeaterStartedSuccess), selectedPrinter, settingsManager.GetFilamentTemperature(CurrentDetails.current_spool.filament_type, colors), CurrentDetails.current_spool.filament_type);
      }
    }

    public override void OnUpdate()
    {
      if (!Visible)
      {
        return;
      }

      base.OnUpdate();
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      var targetTemperature = MainWindow.TargetTemperature;
      if (selectedPrinter.Info.extruder.Temperature < 50.0)
      {
        progressBar.PercentComplete = 0.0f;
      }
      else
      {
        if (progressBar != null)
        {
          var num = selectedPrinter.Info.extruder.Temperature / (double)targetTemperature;
          if (num > 1.0)
          {
            num = 1.0;
          }

          progressBar.PercentComplete = (float) (Math.Exp((num * 100.0 + 23.1809997558594) / 24.2569999694824) / 166.0);
        }
        if (selectedPrinter.Info.extruder.Temperature <= (double)(targetTemperature - 10) || selectedPrinter.Info.extruder.Temperature >= (double)(targetTemperature + 10) || selectedPrinter.WaitingForCommandToComplete || selectedPrinter.Info.Status != PrinterStatus.Firmware_Idle && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused)
        {
          return;
        }

        if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.RemoveFilament)
        {
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page10_PrimingNozzle, CurrentDetails);
        }
        else
        {
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page4_InsertNewFilament, CurrentDetails);
        }
      }
    }
  }
}
