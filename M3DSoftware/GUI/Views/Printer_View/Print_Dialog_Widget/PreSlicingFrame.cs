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
      Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      PrintDialogWindow.SetSize(480, 340);
      PrintDialogWindow.Refresh();
      myPrinter = details.printer;
      canceled.Value = false;
      var num = (int) details.printer.AcquireLock(new AsyncCallback(OnLockedBeforeSlicing), (object) details);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      SetSize(480, 340);
      var borderedImageFrame = new BorderedImageFrame(ID, (Element2D) null);
      borderedImageFrame.Init(host, "guicontrols", 640f, 256f, 703f, 319f, 8, 8, 64, 8, 8, 64);
      borderedImageFrame.SetSize(480, 340);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      AddChildElement((Element2D) borderedImageFrame);
      var textWidget = new TextWidget(0)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "T_PrintDialog_PreparingModel",
        Color = new Color4((byte)100, (byte)100, (byte)100, byte.MaxValue)
      };
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) textWidget);
      var spriteAnimationWidget = new SpriteAnimationWidget(1);
      spriteAnimationWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.SetPosition(238, 100);
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) spriteAnimationWidget);
      var buttonWidget = new ButtonWidget(101);
      buttonWidget.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "T_Cancel";
      buttonWidget.SetGrowableWidth(4, 4, 32);
      buttonWidget.SetGrowableHeight(4, 4, 32);
      buttonWidget.SetSize(100, 32);
      buttonWidget.SetPosition(0, -46);
      buttonWidget.CenterHorizontallyInParent = true;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      AddChildElement((Element2D) buttonWidget);
    }

    private void OnLockedBeforeSlicing(IAsyncCallResult ar)
    {
      var asyncState = (PrintJobDetails) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        if (!asyncState.print_to_file)
        {
          var num = (int) asyncState.printer.SendManualGCode(new AsyncCallback(StartSlicingOnSuccess), (object) asyncState, "M106 S1");
        }
        else
        {
          StitchAndGotoSlicingFrame(asyncState);
        }
      }
      else
      {
        asyncState.printer.ShowLockError(ar);
        PrintDialogWindow.ActivatePrevious(asyncState);
      }
    }

    private void StartSlicingOnSuccess(IAsyncCallResult ar)
    {
      var asyncState = (PrintJobDetails) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success)
      {
        StitchAndGotoSlicingFrame(asyncState);
      }
      else
      {
        asyncState.printer.ShowLockError(ar);
        PrintDialogWindow.ActivatePrevious(asyncState);
      }
    }

    private void StitchAndGotoSlicingFrame(PrintJobDetails CurrentJobDetails)
    {
      StitchModels(CurrentJobDetails);
      if (canceled.Value)
      {
        return;
      }

      if (CurrentJobDetails.print_to_file)
      {
        PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintingToFileFrame, CurrentJobDetails);
      }
      else
      {
        PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.SlicingFrame, CurrentJobDetails);
      }
    }

    private void StitchModels(PrintJobDetails details)
    {
      var modelData1 = ModelData.Stitch(details.slicer_objects);
      var modelStlExporter = new ModelSTLExporter();
      var combinedStlPath = Paths.CombinedSTLPath;
      ModelData modelData2 = modelData1;
      var filename = combinedStlPath;
      modelStlExporter.Save(modelData2, filename);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 101)
      {
        return;
      }

      canceled.Value = true;
      myPrinter.ClearAsyncCallbacks();
      var num = (int)myPrinter.ReleaseLock((AsyncCallback) null, (object) null);
      PrintDialogWindow.CloseWindow();
    }

    private enum ControlIDs
    {
      CancelButton = 101, // 0x00000065
    }
  }
}
