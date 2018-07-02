// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Manual_Controls_Tabs.DiagnosticsFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Client.Extensions;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace M3D.GUI.SettingsPages.Manual_Controls_Tabs
{
  public class DiagnosticsFrame : XMLFrame
  {
    private SpoolerConnection spooler_connection;

    public DiagnosticsFrame(int ID, GUIHost host, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.spooler_connection = spooler_connection;
      var manualcontrolsframeDiagnostics = Resources.manualcontrolsframe_diagnostics;
      Init(host, manualcontrolsframeDiagnostics, new ButtonCallback(diagnosticsFrameButtonCallback));
      CenterHorizontallyInParent = true;
      RelativeY = 0.1f;
      RelativeWidth = 0.95f;
      RelativeHeight = 0.9f;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      Visible = false;
      Enabled = false;
    }

    public void diagnosticsFrameButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected())
      {
        return;
      }

      switch (button.ID)
      {
        case 1000:
          var num1 = (int) selectedPrinter.SendEmergencyStop((M3D.Spooling.Client.AsyncCallback) null, (object) null);
          break;
        case 1020:
          var num2 = (int) selectedPrinter.DoFirmwareUpdate();
          break;
        case 1021:
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "G28");
          break;
        case 1022:
          var num3 = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(FastRecenterOnLock), (object) selectedPrinter);
          break;
        case 1023:
          List<string> xspeedTest = TestGeneration.CreateXSpeedTest(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, xspeedTest.ToArray());
          break;
        case 1024:
          List<string> yspeedTest = TestGeneration.CreateYSpeedTest(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, yspeedTest.ToArray());
          break;
        case 1025:
          var num4 = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(DoBacklashPrintOnLock), (object) selectedPrinter);
          break;
        case 1026:
          List<string> xskipTestMinus = TestGeneration.CreateXSkipTestMinus(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, xskipTestMinus.ToArray());
          break;
        case 1027:
          List<string> xskipTestPlus = TestGeneration.CreateXSkipTestPlus(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, xskipTestPlus.ToArray());
          break;
        case 1028:
          List<string> yskipTestMinus = TestGeneration.CreateYSkipTestMinus(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, yskipTestMinus.ToArray());
          break;
        case 1029:
          List<string> yskipTestPlus = TestGeneration.CreateYSkipTestPlus(selectedPrinter.MyPrinterProfile);
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, yskipTestPlus.ToArray());
          break;
      }
    }

    private void DoBacklashPrintOnLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        throw new Exception("Big bad C# exception");
      }

      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num = (int) asyncState.PrintBacklashPrint(new M3D.Spooling.Client.AsyncCallback(asyncState.ShowLockError), (object) asyncState);
      }
      else
      {
        asyncState.ShowLockError(ar);
      }
    }

    private void FastRecenterOnLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        throw new Exception("Big bad C# exception");
      }

      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        if (asyncState.Info.extruder.ishomed == Trilean.True)
        {
          List<string> stringList = TestGeneration.FastRecenter(asyncState.MyPrinterProfile);
          var num = (int) asyncState.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ReleaseAfterCommand), (object) asyncState, stringList.ToArray());
        }
        else
        {
          var num1 = (int) asyncState.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ReleaseAfterCommand), (object) asyncState, "G28");
        }
      }
      else
      {
        asyncState.ShowLockError(ar);
      }
    }

    private void ReleaseAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        throw new Exception("Big bad C# exception");
      }

      asyncState.ShowLockError(ar);
      var num = (int) asyncState.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private enum DiagnosticsID
    {
      Button_EmergencyStop = 1000, // 0x000003E8
      Button_UpdateFirmware = 1020, // 0x000003FC
      Button_Home = 1021, // 0x000003FD
      Button_FastReCenter = 1022, // 0x000003FE
      Button_XSpeedTest = 1023, // 0x000003FF
      Button_YSpeedTest = 1024, // 0x00000400
      Button_BacklashPrint = 1025, // 0x00000401
      Button_XMinusSkipTest = 1026, // 0x00000402
      Button_XPlusSkipTest = 1027, // 0x00000403
      Button_YMinusSkipTest = 1028, // 0x00000404
      Button_YPlusSkipTest = 1029, // 0x00000405
      HorizontalLayout = 2000, // 0x000007D0
    }
  }
}
