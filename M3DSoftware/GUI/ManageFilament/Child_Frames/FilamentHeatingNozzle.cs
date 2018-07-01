// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentHeatingNozzle
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
        return;
      this.MainWindow.ResetToStartup();
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Warming up", "Please wait while the nozzle is heated.\nBe careful. The nozzle may be very hot.", true, false, true, false, false, false);
      this.progressBar = (ProgressBarWidget) this.FindChildElement(4);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      if (this.CurrentDetails.current_spool == (FilamentSpool) null)
        this.MainWindow.ResetToStartup();
      else if (!selectedPrinter.Info.extruder.Z_Valid)
      {
        this.messagebox.AddMessageToQueue("Sorry. The extruder can't move to a safe position for heating because the Z location has not be calibrated.", PopupMessageBox.MessageBoxButtons.OK);
        this.MainWindow.ResetToStartup();
      }
      else
      {
        FilamentConstants.ColorsEnum colors = (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), this.CurrentDetails.current_spool.filament_color_code);
        this.MainWindow.TurnOnHeater(new M3D.Spooling.Client.AsyncCallback(this.MainWindow.HeaterStartedSuccess), (object) selectedPrinter, this.settingsManager.GetFilamentTemperature(this.CurrentDetails.current_spool.filament_type, colors), this.CurrentDetails.current_spool.filament_type);
      }
    }

    public override void OnUpdate()
    {
      if (!this.Visible)
        return;
      base.OnUpdate();
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      int targetTemperature = this.MainWindow.TargetTemperature;
      if ((double) selectedPrinter.Info.extruder.Temperature < 50.0)
      {
        this.progressBar.PercentComplete = 0.0f;
      }
      else
      {
        if (this.progressBar != null)
        {
          double num = (double) selectedPrinter.Info.extruder.Temperature / (double) targetTemperature;
          if (num > 1.0)
            num = 1.0;
          this.progressBar.PercentComplete = (float) (Math.Exp((num * 100.0 + 23.1809997558594) / 24.2569999694824) / 166.0);
        }
        if ((double) selectedPrinter.Info.extruder.Temperature <= (double) (targetTemperature - 10) || (double) selectedPrinter.Info.extruder.Temperature >= (double) (targetTemperature + 10) || selectedPrinter.WaitingForCommandToComplete || selectedPrinter.Info.Status != PrinterStatus.Firmware_Idle && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused)
          return;
        if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.RemoveFilament)
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page10_PrimingNozzle, this.CurrentDetails);
        else
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page4_InsertNewFilament, this.CurrentDetails);
      }
    }
  }
}
