// Decompiled with JetBrains decompiler
// Type: M3D.GUI.AccessoriesDialog.NozzlePage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.GUI.SettingsPages;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.GUI.AccessoriesDialog
{
  internal class NozzlePage : SettingsPage
  {
    private Frame m_oframeNozzleSelect;
    private Frame m_oframeIncompatiblePrinter;
    private Frame m_oframePrinterNotConnected;
    private Frame m_oframeUpdating;
    private ButtonWidget m_obuttonSelectedNozzleButton;
    private SpoolerConnection m_oSpoolerConnection;
    private PopupMessageBox m_oMessagebox;
    private bool m_bUpdating;
    private bool m_busy;

    public NozzlePage(int ID, GUIHost host, SpoolerConnection spooler_connection, PopupMessageBox messagebox)
      : base(ID)
    {
      this.m_oSpoolerConnection = spooler_connection;
      this.m_oMessagebox = messagebox;
      this.m_oSpoolerConnection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(this.OnSelectedPrinterChanged);
      string accessoriesNozzle = Resources.accessories_nozzle;
      this.Init(host, accessoriesNozzle, new ButtonCallback(this.ButtonCallback));
      this.m_oframeNozzleSelect = (Frame) this.FindChildElement(10101);
      this.m_oframeIncompatiblePrinter = (Frame) this.FindChildElement(10102);
      this.m_oframePrinterNotConnected = (Frame) this.FindChildElement(10103);
      this.m_oframeUpdating = (Frame) this.FindChildElement(10104);
      this.Visible = false;
      this.Enabled = false;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
    }

    public void ButtonCallback(ButtonWidget button)
    {
      if (this.m_bUpdating || button.ID > 10000)
        return;
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
        return;
      if (button.ID == 10000)
      {
        AccessoriesProfile.NozzleProfile nozzleConstants = selectedPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants;
        CustomNozzleSizeDialog.Show(this.Host, this.m_oMessagebox, new EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult>(this.OnCustomNozzleSizeDialogClosed), nozzleConstants.iMinimumNozzleSizeMicrons, nozzleConstants.iMaximumNozzleSizeMicrons, selectedPrinter);
      }
      else
      {
        this.SelectAppropriateButton(button.ID);
        this.SetNozzleSize(selectedPrinter, button.ID);
      }
    }

    private void OnCustomNozzleSizeDialogClosed(object sender, CustomNozzleSizeDialog.CustomNozzleSizeResult result)
    {
      if (result.bCanceled)
        return;
      this.SelectAppropriateButton(10000);
      this.SetNozzleSize(result.Printer, result.iCustomNozzleSizeMicrons);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter == null && (this.m_busy || !selectedPrinter.isBusy) && (!this.m_busy || selectedPrinter.isBusy))
        return;
      this.CheckPrinterState(selectedPrinter);
    }

    public override void OnOpen()
    {
      this.CheckPrinterState(this.m_oSpoolerConnection.SelectedPrinter);
    }

    private void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      if (!this.Enabled || !this.Visible)
        return;
      this.CheckPrinterState(this.m_oSpoolerConnection.SelectedPrinter);
    }

    private void CheckPrinterState(PrinterObject printer)
    {
      this.m_oframeNozzleSelect.Visible = false;
      this.m_oframeIncompatiblePrinter.Visible = false;
      this.m_oframePrinterNotConnected.Visible = false;
      this.m_oframeUpdating.Visible = false;
      if (printer == null || PrinterObject.State.IsNotHealthy == printer.PrinterState)
        this.m_oframePrinterNotConnected.Visible = true;
      else if (!printer.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
        this.m_oframeIncompatiblePrinter.Visible = true;
      else if (printer.isBusy)
      {
        this.m_busy = true;
        this.m_oframeUpdating.Visible = true;
      }
      else
      {
        this.m_oframeNozzleSelect.Visible = true;
        this.SelectAppropriateButton(printer.Info.extruder.iNozzleSizeMicrons);
      }
    }

    private void SelectAppropriateButton(int iNozzleSizeMicrons)
    {
      ButtonWidget childElement = (ButtonWidget) this.m_oframeNozzleSelect.FindChildElement(iNozzleSizeMicrons);
      if (childElement != null)
        this.ChangeSelectedNozzleButton(childElement);
      else
        this.ChangeSelectedNozzleButton((ButtonWidget) this.m_oframeNozzleSelect.FindChildElement(10000));
    }

    private void ChangeSelectedNozzleButton(ButtonWidget new_selection)
    {
      string str = " (Current)";
      if (this.m_obuttonSelectedNozzleButton != null && this.m_obuttonSelectedNozzleButton.Text.StartsWith(str))
        this.m_obuttonSelectedNozzleButton.Text = this.m_obuttonSelectedNozzleButton.Text.Substring(str.Length);
      this.m_obuttonSelectedNozzleButton = new_selection;
      if (this.m_obuttonSelectedNozzleButton == null)
        return;
      this.m_obuttonSelectedNozzleButton.Text = str + this.m_obuttonSelectedNozzleButton.Text;
      this.m_bUpdating = true;
      this.m_obuttonSelectedNozzleButton.Checked = true;
      this.m_bUpdating = false;
    }

    private void SetNozzleSize(PrinterObject printer, int iNozzleSizeMicrons)
    {
      if (printer == null || !printer.isConnected())
        return;
      int num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.ProcessCommandsToPrinter), (object) new NozzlePage.SetNozzleParameters(printer, iNozzleSizeMicrons, false));
    }

    private void ProcessCommandsToPrinter(IAsyncCallResult ar)
    {
      NozzlePage.SetNozzleParameters asyncState = ar.AsyncState as NozzlePage.SetNozzleParameters;
      if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived)
      {
        if (asyncState.bReleasePrinterOnly)
        {
          int num1 = (int) asyncState.printer.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        }
        else
        {
          int num2 = (int) asyncState.printer.SetNozzleWidth(new M3D.Spooling.Client.AsyncCallback(this.ProcessCommandsToPrinter), (object) new NozzlePage.SetNozzleParameters(asyncState.printer, 0, true), asyncState.iNozzleSizeMicrons);
        }
      }
      else
        asyncState.printer.ShowLockError(ar);
    }

    public override void OnClose()
    {
    }

    private enum ControlIDs
    {
      CustomSize = 10000, // 0x00002710
      NozzleSelectFrame = 10101, // 0x00002775
      IncompatiblePrinterFrame = 10102, // 0x00002776
      PrinterNotConnectedFrame = 10103, // 0x00002777
      UpdatingFrame = 10104, // 0x00002778
    }

    private class SetNozzleParameters
    {
      public PrinterObject printer;
      public int iNozzleSizeMicrons;
      public bool bReleasePrinterOnly;

      public SetNozzleParameters(PrinterObject printer, int iNozzleSizeMicrons, bool bReleasePrinterOnly)
      {
        this.printer = printer;
        this.iNozzleSizeMicrons = iNozzleSizeMicrons;
        this.bReleasePrinterOnly = bReleasePrinterOnly;
      }
    }
  }
}
