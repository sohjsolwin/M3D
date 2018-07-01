// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.SpoolerMessageHandler
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace M3D.Spooler
{
  internal class SpoolerMessageHandler
  {
    private MessageType mPrevMessageType = MessageType.UserDefined;
    private string invalid_firmware_message = "Sorry but the firmware on this printer is invalid.";
    private object threadsync;
    private Queue<SpoolerMessage> queued_messages;
    private SpoolerClientBuiltIn spooler_client;

    public SpoolerMessageHandler(SpoolerClientBuiltIn spooler_client)
    {
      this.threadsync = new object();
      this.spooler_client = spooler_client;
      this.queued_messages = new Queue<SpoolerMessage>();
    }

    public void OnGotNewPrinter(Printer new_printer)
    {
      if (!new_printer.InBootloaderMode || !new_printer.Info.FirmwareIsInvalid)
        return;
      this.OnSpoolerMessage(new SpoolerMessage(MessageType.UserDefined, new_printer.Info.serial_number, this.invalid_firmware_message));
    }

    public void OnSpoolerMessage(SpoolerMessage translated_message)
    {
      if (this.spooler_client.ClientCount != 0 || translated_message.Type != MessageType.FirmwareUpdateComplete && translated_message.Type != MessageType.FirmwareUpdateFailed && (translated_message.Type != MessageType.InvalidZ && translated_message.Type != MessageType.ModelOutOfPrintableBounds) && (translated_message.Type != MessageType.UserDefined && translated_message.Type != MessageType.FirmwareErrorCyclePower && (translated_message.Type != MessageType.WarningABSPrintLarge && translated_message.Type != MessageType.BacklashOutOfRange)) && (translated_message.Type != MessageType.BedLocationMustBeCalibrated && translated_message.Type != MessageType.BedOrientationMustBeCalibrated && (translated_message.Type != MessageType.CheckGantryClips && translated_message.Type != MessageType.UnexpectedDisconnect) && (translated_message.Type != MessageType.MultiPointCalibrationNotSupported && translated_message.Type != MessageType.SDPrintIncompatibleFilament && (translated_message.Type != MessageType.SinglePointCalibrationNotSupported && translated_message.Type != MessageType.PowerOutageWhilePrinting))))
        return;
      lock (this.threadsync)
      {
        if (this.queued_messages.Contains(translated_message) || this.mPrevMessageType == MessageType.BedOrientationMustBeCalibrated && this.mPrevMessageType == translated_message.Type)
          return;
        this.mPrevMessageType = translated_message.Type;
        this.queued_messages.Enqueue(translated_message);
      }
    }

    public void ShowMessage()
    {
      SpoolerMessage spoolerMessage = (SpoolerMessage) null;
      lock (this.threadsync)
      {
        try
        {
          if (this.queued_messages.Count > 0)
            spoolerMessage = this.queued_messages.Dequeue();
        }
        catch (Exception ex)
        {
          int num = (int) MessageBox.Show("Exception in MyBroadcastServer.ShowMessage " + ex.Message, "Exception");
        }
      }
      if (spoolerMessage == null)
        return;
      Printer printer = this.spooler_client.GetPrinter(spoolerMessage.SerialNumber);
      if (printer != null)
      {
        if (spoolerMessage.Type == MessageType.InvalidZ)
        {
          if (MessageBox.Show(spoolerMessage.ToString() + "\n\nWould you like to calibrate your printer now?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.Yes)
            this.DoTask(new HandlerTaskDesc(AfterLockTask.CalibrateBedLocationG30, this, printer));
        }
        else if (spoolerMessage.Type == MessageType.BedOrientationMustBeCalibrated)
        {
          if (printer.Info.Status != PrinterStatus.Firmware_Calibrating && MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler", MessageBoxButtons.OKCancel) == DialogResult.OK)
            this.DoTask(new HandlerTaskDesc(AfterLockTask.CalibrateGantryG32, this, printer));
        }
        else if (spoolerMessage.Type == MessageType.WarningABSPrintLarge)
        {
          if (MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
            this.DoTask(new HandlerTaskDesc(AfterLockTask.AbortPrint, this, printer));
          else
            this.DoTask(new HandlerTaskDesc(AfterLockTask.ClearWarning, this, printer));
        }
        else if (spoolerMessage.Type == MessageType.UserDefined && spoolerMessage.Message == this.invalid_firmware_message)
        {
          int num = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
          if (MessageBox.Show("Update firmware now?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.Yes)
            this.DoTask(new HandlerTaskDesc(AfterLockTask.DoFirmwareUpdate, this, printer));
        }
        else if (spoolerMessage.Type == MessageType.CheckGantryClips)
        {
          if (MessageBox.Show(spoolerMessage.ToString() + "\nHave you removed your gantry clips?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            int num1 = (int) MessageBox.Show("You will not be able to use your printer until you remove the gantry clips.", "M3D Spooler");
          }
          else
            this.DoTask(new HandlerTaskDesc(AfterLockTask.CheckGantryClips, this, printer));
        }
        else if (spoolerMessage.Type == MessageType.SDPrintIncompatibleFilament)
        {
          int num2 = (int) MessageBox.Show("Unable to start saved print because the 3D ink used doesn't match what's currently in the printer.", "M3D Spooler");
        }
        else if (spoolerMessage.Type == MessageType.PowerOutageWhilePrinting)
        {
          if (DialogResult.Yes == MessageBox.Show("We detected that a power failure interrupted your print. Would you like to resume printing?", "M3D Spooler", MessageBoxButtons.YesNo))
          {
            DialogResult dialogResult = MessageBox.Show("Was the extruder head moved manually after power failure? If so, we must re-home the extruder and accuracy of the continued print may be somewhat degraded.", "M3D Spooler", MessageBoxButtons.YesNoCancel);
            if (DialogResult.Yes == dialogResult)
            {
              this.DoTask(new HandlerTaskDesc(AfterLockTask.RecoverFromPowerOutageG28, this, printer));
              return;
            }
            if (DialogResult.No == dialogResult)
            {
              this.DoTask(new HandlerTaskDesc(AfterLockTask.RecoverFromPowerOutage, this, printer));
              return;
            }
          }
          this.DoTask(new HandlerTaskDesc(AfterLockTask.ClearPowerRecoveryFault, this, printer));
        }
        else
        {
          int num3 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
        }
      }
      if (spoolerMessage.Type == MessageType.FirmwareErrorCyclePower)
      {
        int num4 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
      else if (spoolerMessage.Type == MessageType.UnexpectedDisconnect)
      {
        int num5 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
      else
      {
        if (printer != null)
          return;
        int num6 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
    }

    private void DoTask(HandlerTaskDesc desc)
    {
      if (!desc.printer.HasLock)
      {
        desc.hadlockbeforecall = false;
        int num = (int) desc.printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) desc);
      }
      else
      {
        HandlerTaskDesc handlerTaskDesc = (HandlerTaskDesc) null;
        if (!desc.hadlockbeforecall)
        {
          handlerTaskDesc = new HandlerTaskDesc(AfterLockTask.ReleaseLock, desc.handler, desc.printer, desc.task, 0);
          handlerTaskDesc.hadlockbeforecall = true;
        }
        switch (desc.task)
        {
          case AfterLockTask.DoFirmwareUpdate:
            int num1 = (int) desc.printer.DoFirmwareUpdate(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.CalibrateBedLocationG30:
            if ("Pro" == desc.printer.Info.ProfileName)
            {
              int num2 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G28", "G30");
              break;
            }
            int num3 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G30");
            break;
          case AfterLockTask.CalibrateGantryG32:
            int num4 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G32");
            break;
          case AfterLockTask.ReleaseLock:
            int num5 = (int) desc.printer.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) null);
            break;
          case AfterLockTask.AbortPrint:
            int num6 = (int) desc.printer.AbortPrint(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.ClearWarning:
            int num7 = (int) desc.printer.ClearCurrentWarning(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.CheckGantryClips:
            int num8 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, "M583");
            break;
          case AfterLockTask.RecoverFromPowerOutage:
            int num9 = (int) desc.printer.RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, false);
            break;
          case AfterLockTask.RecoverFromPowerOutageG28:
            int num10 = (int) desc.printer.RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc, true);
            break;
          case AfterLockTask.ClearPowerRecoveryFault:
            int num11 = (int) desc.printer.ClearPowerRecoveryFault(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
        }
      }
    }

    private void DoTaskAsyncCallback(IAsyncCallResult ar)
    {
      HandlerTaskDesc desc = (HandlerTaskDesc) null;
      if (ar.AsyncState != null)
        desc = ar.AsyncState as HandlerTaskDesc;
      if (desc != null)
      {
        int num1 = desc.printer.Connected ? 1 : 0;
      }
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          if (desc == null || desc.task == AfterLockTask.None)
            break;
          this.DoTask(desc);
          break;
        case CommandResult.Failed_PrinterDoesNotHaveLock:
          if (desc == null || desc.printer == null || desc.attempts >= 2)
            break;
          int num2 = (int) desc.printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.DoTaskAsyncCallback), (object) new HandlerTaskDesc(desc.previous_task, desc.handler, desc.printer)
          {
            attempts = (desc.attempts + 1),
            hadlockbeforecall = false
          });
          break;
      }
    }
  }
}
