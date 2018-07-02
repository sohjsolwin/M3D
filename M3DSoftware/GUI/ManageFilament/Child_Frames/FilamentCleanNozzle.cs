using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;
using System.Diagnostics;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentCleanNozzle : Manage3DInkChildWindow
  {
    private Stopwatch _continueTimer = new Stopwatch();
    private TextWidget clean_nozzle_timer_text;

    public FilamentCleanNozzle(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 14)
      {
        return;
      }

      Continue();
    }

    public override void Init()
    {
      FontSize fontSize = FontSize.Medium;
      var color4_1 = new Color4(0.35f, 0.35f, 0.35f, 1f);
      var color4_2 = new Color4(0.35f, 0.35f, 0.35f, 1f);
      RelativeX = 0.0f;
      RelativeY = 0.0f;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      var textWidget1 = new TextWidget(11)
      {
        Color = color4_1,
        Text = "Clean Nozzle Head",
        RelativeWidth = 1f,
        Size = fontSize,
        Alignment = QFontAlignment.Centre
      };
      textWidget1.SetPosition(0, 50);
      AddChildElement((Element2D) textWidget1);
      var textWidget2 = new TextWidget(12)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "The nozzle may contain unwanted filament.\nPlease remove any excess material from the nozzle.\nBe careful. The nozzle might be hot.",
        Color = color4_2
      };
      textWidget2.SetPosition(0, 75);
      textWidget2.SetSize(480, 80);
      textWidget2.CenterHorizontallyInParent = true;
      textWidget2.Visible = true;
      AddChildElement((Element2D) textWidget2);
      var textWidget3 = new TextWidget(0)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "Continuing in",
        Color = new Color4((byte)100, (byte)100, (byte)100, byte.MaxValue)
      };
      textWidget3.SetSize(140, 30);
      textWidget3.SetPosition(327, 193);
      AddChildElement((Element2D) textWidget3);
      var spriteAnimationWidget = new SpriteAnimationWidget(1);
      spriteAnimationWidget.Init(Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.SetPosition(488, 150);
      AddChildElement((Element2D) spriteAnimationWidget);
      clean_nozzle_timer_text = new TextWidget(13)
      {
        Size = FontSize.VeryLarge,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "30"
      };
      clean_nozzle_timer_text.SetSize(128, 108);
      clean_nozzle_timer_text.SetPosition(488, 150);
      clean_nozzle_timer_text.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      AddChildElement((Element2D)clean_nozzle_timer_text);
      var buttonWidget = new ButtonWidget(14);
      buttonWidget.Init(Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "Continue";
      buttonWidget.SetGrowableWidth(4, 4, 32);
      buttonWidget.SetGrowableHeight(4, 4, 32);
      buttonWidget.SetSize(110, 40);
      buttonWidget.SetPosition(550, -50);
      buttonWidget.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      AddChildElement((Element2D) buttonWidget);
      var imageWidget = new ImageWidget(0);
      imageWidget.Init(Host, "extendedcontrols2", 696f, 100f, 1020f, 350f, 696f, 100f, 1020f, 350f, 696f, 100f, 1020f, 350f);
      imageWidget.SetSize(320, 256);
      imageWidget.SetPosition(0, 140);
      imageWidget.Visible = true;
      AddChildElement((Element2D) imageWidget);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      _continueTimer.Restart();
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!Visible || !_continueTimer.IsRunning)
      {
        return;
      }

      var num = 30 - _continueTimer.Elapsed.Seconds;
      if (num < 0)
      {
        _continueTimer.Stop();
        num = 0;
        Continue();
      }
      clean_nozzle_timer_text.Text = num.ToString();
    }

    private void Continue()
    {
      FilamentWaitingPage.CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
      MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      _continueTimer.Stop();
    }

    public enum ControlIDs
    {
      TitleText = 11, // 0x0000000B
      DirectionsText = 12, // 0x0000000C
      CleanNozzleTimerText = 13, // 0x0000000D
      ContinueButton = 14, // 0x0000000E
    }
  }
}
