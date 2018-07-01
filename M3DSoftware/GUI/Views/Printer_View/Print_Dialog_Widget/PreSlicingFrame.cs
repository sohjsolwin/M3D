// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.PreSlicingFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Model;
using M3D.Model.FilIO;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class PreSlicingFrame : IPrintDialogFrame
  {
    private ThreadSafeVariable<bool> canceled = new ThreadSafeVariable<bool>(false);
    private PrinterObject myPrinter;

    public PreSlicingFrame(int ID, GUIHost host, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      this.Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.PrintDialogWindow.SetSize(480, 340);
      this.PrintDialogWindow.Refresh();
      this.myPrinter = details.printer;
      this.canceled.Value = false;
      int num = (int) details.printer.AcquireLock(new AsyncCallback(this.OnLockedBeforeSlicing), (object) details);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      this.SetSize(480, 340);
      BorderedImageFrame borderedImageFrame = new BorderedImageFrame(this.ID, (Element2D) null);
      borderedImageFrame.Init(host, "guicontrols", 640f, 256f, 703f, 319f, 8, 8, 64, 8, 8, 64);
      borderedImageFrame.SetSize(480, 340);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      this.AddChildElement((Element2D) borderedImageFrame);
      TextWidget textWidget = new TextWidget(0);
      textWidget.Size = FontSize.Medium;
      textWidget.Alignment = QFontAlignment.Centre;
      textWidget.VAlignment = TextVerticalAlignment.Middle;
      textWidget.Text = "T_PrintDialog_PreparingModel";
      textWidget.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) textWidget);
      SpriteAnimationWidget spriteAnimationWidget = new SpriteAnimationWidget(1);
      spriteAnimationWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.SetPosition(238, 100);
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) spriteAnimationWidget);
      ButtonWidget buttonWidget = new ButtonWidget(101);
      buttonWidget.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "T_Cancel";
      buttonWidget.SetGrowableWidth(4, 4, 32);
      buttonWidget.SetGrowableHeight(4, 4, 32);
      buttonWidget.SetSize(100, 32);
      buttonWidget.SetPosition(0, -46);
      buttonWidget.CenterHorizontallyInParent = true;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
    }

    private void OnLockedBeforeSlicing(IAsyncCallResult ar)
    {
      PrintJobDetails asyncState = (PrintJobDetails) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        if (!asyncState.print_to_file)
        {
          int num = (int) asyncState.printer.SendManualGCode(new AsyncCallback(this.StartSlicingOnSuccess), (object) asyncState, "M106 S1");
        }
        else
          this.StitchAndGotoSlicingFrame(asyncState);
      }
      else
      {
        asyncState.printer.ShowLockError(ar);
        this.PrintDialogWindow.ActivatePrevious(asyncState);
      }
    }

    private void StartSlicingOnSuccess(IAsyncCallResult ar)
    {
      PrintJobDetails asyncState = (PrintJobDetails) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success)
      {
        this.StitchAndGotoSlicingFrame(asyncState);
      }
      else
      {
        asyncState.printer.ShowLockError(ar);
        this.PrintDialogWindow.ActivatePrevious(asyncState);
      }
    }

    private void StitchAndGotoSlicingFrame(PrintJobDetails CurrentJobDetails)
    {
      this.StitchModels(CurrentJobDetails);
      if (this.canceled.Value)
        return;
      if (CurrentJobDetails.print_to_file)
        this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintingToFileFrame, CurrentJobDetails);
      else
        this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.SlicingFrame, CurrentJobDetails);
    }

    private void StitchModels(PrintJobDetails details)
    {
      ModelData modelData1 = ModelData.Stitch(details.slicer_objects);
      ModelSTLExporter modelStlExporter = new ModelSTLExporter();
      string combinedStlPath = Paths.CombinedSTLPath;
      ModelData modelData2 = modelData1;
      string filename = combinedStlPath;
      modelStlExporter.Save(modelData2, filename);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 101)
        return;
      this.canceled.Value = true;
      this.myPrinter.ClearAsyncCallbacks();
      int num = (int) this.myPrinter.ReleaseLock((AsyncCallback) null, (object) null);
      this.PrintDialogWindow.CloseWindow();
    }

    private enum ControlIDs
    {
      CancelButton = 101, // 0x00000065
    }
  }
}
