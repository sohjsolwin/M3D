// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentRaisingExtruder
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentRaisingExtruder : Manage3DInkChildWindow
  {
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;

    public FilamentRaisingExtruder(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
      this.messagebox = messagebox;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Positioning Print Head", "Please wait.\n\nMoving the print head to the proper place.\n\nBe careful. The nozzle might be hot.", false, false, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      if (this.CurrentDetails.current_spool == (FilamentSpool) null)
        this.MainWindow.ResetToStartup();
      int num = (int) selectedPrinter.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.CheckPrinterAndHeater), (object) selectedPrinter, "M117", "M114");
    }

    private void CheckPrinterAndHeater(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        this.MainWindow.ResetToStartup();
      if (ar.CallResult != CommandResult.Success)
      {
        this.messagebox.AddMessageToQueue("There was a problem sending commands to the printer. Please try again.");
        this.MainWindow.ResetToStartup();
      }
      else if (!asyncState.Info.extruder.Z_Valid)
      {
        this.messagebox.AddMessageToQueue("Sorry. The extruder can't move to a safe position for heating because the Z location has not be calibrated.", PopupMessageBox.MessageBoxButtons.OK);
        this.MainWindow.ResetToStartup();
      }
      else if (this.CurrentDetails.current_spool.filament_location == FilamentSpool.Location.External)
      {
        FilamentConstants.ColorsEnum colors = (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), this.CurrentDetails.current_spool.filament_color_code);
        this.MainWindow.TurnOnHeater(new M3D.Spooling.Client.AsyncCallback(this.RaiseExtruder), (object) asyncState, this.settingsManager.GetFilamentTemperature(this.CurrentDetails.current_spool.filament_type, colors), this.CurrentDetails.current_spool.filament_type);
      }
      else
        this.MainWindow.TurnOffHeater(new M3D.Spooling.Client.AsyncCallback(this.RaiseExtruder), (object) asyncState);
    }

    private void RaiseExtruder(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        this.MainWindow.ResetToStartup();
      if (ar.CallResult != CommandResult.Success)
      {
        this.messagebox.AddMessageToQueue("There was a problem sending commands to the printer. Please try again.");
        this.MainWindow.ResetToStartup();
      }
      else if (!asyncState.Info.extruder.Z_Valid)
      {
        this.messagebox.AddMessageToQueue("Sorry. The extruder can't move to a safe position for heating because the Z location has not be calibrated.", PopupMessageBox.MessageBoxButtons.OK);
        this.MainWindow.ResetToStartup();
      }
      else
      {
        float fastestPossible = asyncState.MyPrinterProfile.SpeedLimitConstants.FastestPossible;
        PrinterSizeProfile printerSizeConstants = asyncState.MyPrinterProfile.PrinterSizeConstants;
        int num1 = asyncState.Info.extruder.ishomed == Trilean.True ? 1 : 0;
        float num2 = asyncState.Info.extruder.position.pos.z;
        List<string> stringList = new List<string>();
        if (num1 == 0)
        {
          if ((double) num2 > (double) printerSizeConstants.BoxTopLimitZ)
          {
            stringList.Add("G90");
            stringList.Add(PrinterCompatibleString.Format("G0 Z{0} F{1}", (object) printerSizeConstants.BoxTopLimitZ, (object) fastestPossible));
            num2 = printerSizeConstants.BoxTopLimitZ;
          }
          stringList.Add("G28");
          stringList.Add("M114");
        }
        Manage3DInkMainWindow.PageID nextPage;
        if (this.CurrentDetails.current_spool.filament_location == FilamentSpool.Location.Internal)
        {
          float boxTopLimitZ = printerSizeConstants.BoxTopLimitZ;
          stringList.Add("G90");
          stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2} Z{3}", (object) printerSizeConstants.BackCornerPosition.x, (object) printerSizeConstants.BackCornerPosition.y, (object) fastestPossible, (object) boxTopLimitZ));
          nextPage = this.CurrentDetails.mode != Manage3DInkMainWindow.Mode.RemoveFilament ? Manage3DInkMainWindow.PageID.Page14_InternalSpoolInstructions : Manage3DInkMainWindow.PageID.Page16_RemoveInternalSpoolInstructions;
        }
        else
        {
          float num3 = (double) num2 > 15.0 ? num2 : 15f;
          float x;
          float y;
          if (asyncState.IsPausedorPausing)
          {
            if ((double) num3 > (double) printerSizeConstants.BoxTopLimitZ)
            {
              x = printerSizeConstants.BackCornerPositionBoxTop.x;
              y = printerSizeConstants.BackCornerPositionBoxTop.y;
            }
            else
            {
              x = printerSizeConstants.BackCornerPosition.x;
              y = printerSizeConstants.BackCornerPosition.y;
            }
          }
          else
          {
            x = printerSizeConstants.HomeLocation.x;
            y = printerSizeConstants.HomeLocation.y;
          }
          stringList.Add("G90");
          stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2} Z{3}", (object) x, (object) y, (object) fastestPossible, (object) num3));
          nextPage = Manage3DInkMainWindow.PageID.Page1_HeatingNozzle;
        }
        int num4 = (int) asyncState.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(asyncState, nextPage, this.CurrentDetails), stringList.ToArray());
      }
    }
  }
}
