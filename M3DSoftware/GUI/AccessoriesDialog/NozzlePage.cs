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
      m_oSpoolerConnection = spooler_connection;
      m_oMessagebox = messagebox;
      m_oSpoolerConnection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(OnSelectedPrinterChanged);
      var accessoriesNozzle = Resources.accessories_nozzle;
      Init(host, accessoriesNozzle, new ButtonCallback(ButtonCallback));
      m_oframeNozzleSelect = (Frame)FindChildElement(10101);
      m_oframeIncompatiblePrinter = (Frame)FindChildElement(10102);
      m_oframePrinterNotConnected = (Frame)FindChildElement(10103);
      m_oframeUpdating = (Frame)FindChildElement(10104);
      Visible = false;
      Enabled = false;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
    }

    public void ButtonCallback(ButtonWidget button)
    {
      if (m_bUpdating || button.ID > 10000)
      {
        return;
      }

      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        return;
      }

      if (button.ID == 10000)
      {
        AccessoriesProfile.NozzleProfile nozzleConstants = selectedPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants;
        CustomNozzleSizeDialog.Show(Host, m_oMessagebox, new EventHandler<CustomNozzleSizeDialog.CustomNozzleSizeResult>(OnCustomNozzleSizeDialogClosed), nozzleConstants.iMinimumNozzleSizeMicrons, nozzleConstants.iMaximumNozzleSizeMicrons, selectedPrinter);
      }
      else
      {
        SelectAppropriateButton(button.ID);
        SetNozzleSize(selectedPrinter, button.ID);
      }
    }

    private void OnCustomNozzleSizeDialogClosed(object sender, CustomNozzleSizeDialog.CustomNozzleSizeResult result)
    {
      if (result.bCanceled)
      {
        return;
      }

      SelectAppropriateButton(10000);
      SetNozzleSize(result.Printer, result.iCustomNozzleSizeMicrons);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter == null && (m_busy || !selectedPrinter.isBusy) && (!m_busy || selectedPrinter.isBusy))
      {
        return;
      }

      CheckPrinterState(selectedPrinter);
    }

    public override void OnOpen()
    {
      CheckPrinterState(m_oSpoolerConnection.SelectedPrinter);
    }

    private void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      if (!Enabled || !Visible)
      {
        return;
      }

      CheckPrinterState(m_oSpoolerConnection.SelectedPrinter);
    }

    private void CheckPrinterState(PrinterObject printer)
    {
      m_oframeNozzleSelect.Visible = false;
      m_oframeIncompatiblePrinter.Visible = false;
      m_oframePrinterNotConnected.Visible = false;
      m_oframeUpdating.Visible = false;
      if (printer == null || PrinterObject.State.IsNotHealthy == printer.PrinterState)
      {
        m_oframePrinterNotConnected.Visible = true;
      }
      else if (!printer.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
      {
        m_oframeIncompatiblePrinter.Visible = true;
      }
      else if (printer.isBusy)
      {
        m_busy = true;
        m_oframeUpdating.Visible = true;
      }
      else
      {
        m_oframeNozzleSelect.Visible = true;
        SelectAppropriateButton(printer.Info.extruder.iNozzleSizeMicrons);
      }
    }

    private void SelectAppropriateButton(int iNozzleSizeMicrons)
    {
      var childElement = (ButtonWidget)m_oframeNozzleSelect.FindChildElement(iNozzleSizeMicrons);
      if (childElement != null)
      {
        ChangeSelectedNozzleButton(childElement);
      }
      else
      {
        ChangeSelectedNozzleButton((ButtonWidget)m_oframeNozzleSelect.FindChildElement(10000));
      }
    }

    private void ChangeSelectedNozzleButton(ButtonWidget new_selection)
    {
      var str = " (Current)";
      if (m_obuttonSelectedNozzleButton != null && m_obuttonSelectedNozzleButton.Text.StartsWith(str))
      {
        m_obuttonSelectedNozzleButton.Text = m_obuttonSelectedNozzleButton.Text.Substring(str.Length);
      }

      m_obuttonSelectedNozzleButton = new_selection;
      if (m_obuttonSelectedNozzleButton == null)
      {
        return;
      }

      m_obuttonSelectedNozzleButton.Text = str + m_obuttonSelectedNozzleButton.Text;
      m_bUpdating = true;
      m_obuttonSelectedNozzleButton.Checked = true;
      m_bUpdating = false;
    }

    private void SetNozzleSize(PrinterObject printer, int iNozzleSizeMicrons)
    {
      if (printer == null || !printer.isConnected())
      {
        return;
      }

      var num = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(ProcessCommandsToPrinter), (object) new NozzlePage.SetNozzleParameters(printer, iNozzleSizeMicrons, false));
    }

    private void ProcessCommandsToPrinter(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as NozzlePage.SetNozzleParameters;
      if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived)
      {
        if (asyncState.bReleasePrinterOnly)
        {
          var num1 = (int) asyncState.printer.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        }
        else
        {
          var num2 = (int) asyncState.printer.SetNozzleWidth(new M3D.Spooling.Client.AsyncCallback(ProcessCommandsToPrinter), (object) new NozzlePage.SetNozzleParameters(asyncState.printer, 0, true), asyncState.iNozzleSizeMicrons);
        }
      }
      else
      {
        asyncState.printer.ShowLockError(ar);
      }
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
