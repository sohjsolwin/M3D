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
      threadsync = new object();
      this.spooler_client = spooler_client;
      queued_messages = new Queue<SpoolerMessage>();
    }

    public void OnGotNewPrinter(Printer new_printer)
    {
      if (!new_printer.InBootloaderMode || !new_printer.Info.FirmwareIsInvalid)
      {
        return;
      }

      OnSpoolerMessage(new SpoolerMessage(MessageType.UserDefined, new_printer.Info.serial_number, invalid_firmware_message));
    }

    public void OnSpoolerMessage(SpoolerMessage translated_message)
    {
      if (spooler_client.ClientCount != 0 || translated_message.Type != MessageType.FirmwareUpdateComplete && translated_message.Type != MessageType.FirmwareUpdateFailed && (translated_message.Type != MessageType.InvalidZ && translated_message.Type != MessageType.ModelOutOfPrintableBounds) && (translated_message.Type != MessageType.UserDefined && translated_message.Type != MessageType.FirmwareErrorCyclePower && (translated_message.Type != MessageType.WarningABSPrintLarge && translated_message.Type != MessageType.BacklashOutOfRange)) && (translated_message.Type != MessageType.BedLocationMustBeCalibrated && translated_message.Type != MessageType.BedOrientationMustBeCalibrated && (translated_message.Type != MessageType.CheckGantryClips && translated_message.Type != MessageType.UnexpectedDisconnect) && (translated_message.Type != MessageType.MultiPointCalibrationNotSupported && translated_message.Type != MessageType.SDPrintIncompatibleFilament && (translated_message.Type != MessageType.SinglePointCalibrationNotSupported && translated_message.Type != MessageType.PowerOutageWhilePrinting))))
      {
        return;
      }

      lock (threadsync)
      {
        if (queued_messages.Contains(translated_message) || mPrevMessageType == MessageType.BedOrientationMustBeCalibrated && mPrevMessageType == translated_message.Type)
        {
          return;
        }

        mPrevMessageType = translated_message.Type;
        queued_messages.Enqueue(translated_message);
      }
    }

    public void ShowMessage()
    {
      var spoolerMessage = (SpoolerMessage) null;
      lock (threadsync)
      {
        try
        {
          if (queued_messages.Count > 0)
          {
            spoolerMessage = queued_messages.Dequeue();
          }
        }
        catch (Exception ex)
        {
          var num = (int) MessageBox.Show("Exception in MyBroadcastServer.ShowMessage " + ex.Message, "Exception");
        }
      }
      if (spoolerMessage == null)
      {
        return;
      }

      Printer printer = spooler_client.GetPrinter(spoolerMessage.SerialNumber);
      if (printer != null)
      {
        if (spoolerMessage.Type == MessageType.InvalidZ)
        {
          if (MessageBox.Show(spoolerMessage.ToString() + "\n\nWould you like to calibrate your printer now?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.Yes)
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.CalibrateBedLocationG30, this, printer));
          }
        }
        else if (spoolerMessage.Type == MessageType.BedOrientationMustBeCalibrated)
        {
          if (printer.Info.Status != PrinterStatus.Firmware_Calibrating && MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler", MessageBoxButtons.OKCancel) == DialogResult.OK)
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.CalibrateGantryG32, this, printer));
          }
        }
        else if (spoolerMessage.Type == MessageType.WarningABSPrintLarge)
        {
          if (MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.AbortPrint, this, printer));
          }
          else
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.ClearWarning, this, printer));
          }
        }
        else if (spoolerMessage.Type == MessageType.UserDefined && spoolerMessage.Message == invalid_firmware_message)
        {
          var num = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
          if (MessageBox.Show("Update firmware now?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.Yes)
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.DoFirmwareUpdate, this, printer));
          }
        }
        else if (spoolerMessage.Type == MessageType.CheckGantryClips)
        {
          if (MessageBox.Show(spoolerMessage.ToString() + "\nHave you removed your gantry clips?", "M3D Spooler", MessageBoxButtons.YesNo) == DialogResult.No)
          {
            var num1 = (int) MessageBox.Show("You will not be able to use your printer until you remove the gantry clips.", "M3D Spooler");
          }
          else
          {
            DoTask(new HandlerTaskDesc(AfterLockTask.CheckGantryClips, this, printer));
          }
        }
        else if (spoolerMessage.Type == MessageType.SDPrintIncompatibleFilament)
        {
          var num2 = (int) MessageBox.Show("Unable to start saved print because the 3D ink used doesn't match what's currently in the printer.", "M3D Spooler");
        }
        else if (spoolerMessage.Type == MessageType.PowerOutageWhilePrinting)
        {
          if (DialogResult.Yes == MessageBox.Show("We detected that a power failure interrupted your print. Would you like to resume printing?", "M3D Spooler", MessageBoxButtons.YesNo))
          {
            DialogResult dialogResult = MessageBox.Show("Was the extruder head moved manually after power failure? If so, we must re-home the extruder and accuracy of the continued print may be somewhat degraded.", "M3D Spooler", MessageBoxButtons.YesNoCancel);
            if (DialogResult.Yes == dialogResult)
            {
              DoTask(new HandlerTaskDesc(AfterLockTask.RecoverFromPowerOutageG28, this, printer));
              return;
            }
            if (DialogResult.No == dialogResult)
            {
              DoTask(new HandlerTaskDesc(AfterLockTask.RecoverFromPowerOutage, this, printer));
              return;
            }
          }
          DoTask(new HandlerTaskDesc(AfterLockTask.ClearPowerRecoveryFault, this, printer));
        }
        else
        {
          var num3 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
        }
      }
      if (spoolerMessage.Type == MessageType.FirmwareErrorCyclePower)
      {
        var num4 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
      else if (spoolerMessage.Type == MessageType.UnexpectedDisconnect)
      {
        var num5 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
      else
      {
        if (printer != null)
        {
          return;
        }

        var num6 = (int) MessageBox.Show(spoolerMessage.ToString(), "M3D Spooler");
      }
    }

    private void DoTask(HandlerTaskDesc desc)
    {
      if (!desc.printer.HasLock)
      {
        desc.hadlockbeforecall = false;
        var num = (int) desc.printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) desc);
      }
      else
      {
        var handlerTaskDesc = (HandlerTaskDesc) null;
        if (!desc.hadlockbeforecall)
        {
          handlerTaskDesc = new HandlerTaskDesc(AfterLockTask.ReleaseLock, desc.handler, desc.printer, desc.task, 0)
          {
            hadlockbeforecall = true
          };
        }
        switch (desc.task)
        {
          case AfterLockTask.DoFirmwareUpdate:
            var num1 = (int) desc.printer.DoFirmwareUpdate(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.CalibrateBedLocationG30:
            if ("Pro" == desc.printer.Info.ProfileName)
            {
              var num2 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G28", "G30");
              break;
            }
            var num3 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G30");
            break;
          case AfterLockTask.CalibrateGantryG32:
            var num4 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, "M104 S150", "G32");
            break;
          case AfterLockTask.ReleaseLock:
            var num5 = (int) desc.printer.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) null);
            break;
          case AfterLockTask.AbortPrint:
            var num6 = (int) desc.printer.AbortPrint(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.ClearWarning:
            var num7 = (int) desc.printer.ClearCurrentWarning(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
          case AfterLockTask.CheckGantryClips:
            var num8 = (int) desc.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, "M583");
            break;
          case AfterLockTask.RecoverFromPowerOutage:
            var num9 = (int) desc.printer.RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, false);
            break;
          case AfterLockTask.RecoverFromPowerOutageG28:
            var num10 = (int) desc.printer.RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc, true);
            break;
          case AfterLockTask.ClearPowerRecoveryFault:
            var num11 = (int) desc.printer.ClearPowerRecoveryFault(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) handlerTaskDesc);
            break;
        }
      }
    }

    private void DoTaskAsyncCallback(IAsyncCallResult ar)
    {
      var desc = (HandlerTaskDesc) null;
      if (ar.AsyncState != null)
      {
        desc = ar.AsyncState as HandlerTaskDesc;
      }

      if (desc != null)
      {
        var num1 = desc.printer.Connected ? 1 : 0;
      }
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          if (desc == null || desc.task == AfterLockTask.None)
          {
            break;
          }

          DoTask(desc);
          break;
        case CommandResult.Failed_PrinterDoesNotHaveLock:
          if (desc == null || desc.printer == null || desc.attempts >= 2)
          {
            break;
          }

          var num2 = (int) desc.printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(DoTaskAsyncCallback), (object) new HandlerTaskDesc(desc.previous_task, desc.handler, desc.printer)
          {
            attempts = (desc.attempts + 1),
            hadlockbeforecall = false
          });
          break;
      }
    }
  }
}
