// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.WelcomeDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Dialogs;
using M3D.Spooling;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.Views
{
  public class WelcomeDialog : BorderedImageFrame
  {
    private GUIHost m_host;
    private PopupMessageBox messagebox;
    private ButtonWidget continue_button;

    public WelcomeDialog(int ID, PopupMessageBox messagebox)
      : base(ID, (Element2D) null)
    {
      this.messagebox = messagebox;
    }

    public void Init(GUIHost host)
    {
      Sprite.pixel_perfect = false;
      this.m_host = host;
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      this.Init(host, "guicontrols", 640f, 256f, 703f, 319f, 12, 12, 64, 12, 12, 64);
      this.CenterHorizontallyInParent = true;
      this.CenterVerticallyInParent = true;
      this.SetSize(750, 450);
      this.MinHeight = 450;
      this.MaxHeight = 600;
      this.MinWidth = 750;
      this.MaxWidth = 900;
      Frame frame = new Frame(0);
      frame.CenterHorizontallyInParent = true;
      frame.CenterVerticallyInParent = true;
      frame.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      frame.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      frame.RelativeWidth = 0.95f;
      frame.RelativeHeight = 0.8f;
      this.AddChildElement((Element2D) frame);
      TextWidget textWidget1 = new TextWidget(0);
      textWidget1.Size = FontSize.VeryLarge;
      textWidget1.X = 0;
      textWidget1.Y = 30;
      textWidget1.SetSize(frame.Width, 50);
      textWidget1.RelativeWidth = 0.95f;
      textWidget1.CenterHorizontallyInParent = true;
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.VAlignment = TextVerticalAlignment.Top;
      textWidget1.Text = "Thank you for choosing M3D.";
      textWidget1.Color = new Color4(byte.MaxValue, (byte) 147, (byte) 0, byte.MaxValue);
      frame.AddChildElement((Element2D) textWidget1);
      TextWidget textWidget2 = new TextWidget(0);
      textWidget2.Size = FontSize.Medium;
      textWidget2.X = 0;
      textWidget2.Y = 80;
      textWidget2.SetSize(frame.Width, 30);
      textWidget2.RelativeWidth = 0.95f;
      textWidget2.CenterHorizontallyInParent = true;
      textWidget2.Alignment = QFontAlignment.Centre;
      textWidget2.VAlignment = TextVerticalAlignment.Top;
      textWidget2.Text = "Here are some features new to this release.";
      textWidget2.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      frame.AddChildElement((Element2D) textWidget2);
      ListBoxWidget listBoxWidget = new ListBoxWidget(0);
      listBoxWidget.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 944f, 96f, 960f, 144f, 4, 4, 16, 4, 4, 48, 24, 24);
      listBoxWidget.ScrollBar.InitTrack(host, "guicontrols", 809f, 80f, 831f, 87f, 2, 8);
      listBoxWidget.ScrollBar.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      listBoxWidget.ScrollBar.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      listBoxWidget.ScrollBar.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      listBoxWidget.ScrollBar.SetButtonSize(24f);
      listBoxWidget.ScrollBar.ShowPushButtons = true;
      listBoxWidget.ShowScrollbar = ListBoxWidget.ScrollBarState.On;
      listBoxWidget.Y = 110;
      listBoxWidget.CenterHorizontallyInParent = true;
      listBoxWidget.SetSize(650, 190);
      listBoxWidget.Items.Add((object) ("Release " + Version.VersionTextNoDate));
      listBoxWidget.Items.Add((object) "- Performance improvements for untethered printing (Micro+, Pro)");
      listBoxWidget.Items.Add((object) "- Power outage recovery (Micro+, Pro)");
      listBoxWidget.Items.Add((object) "- Improved \"on the fly\" backlash processing (Micro+, Pro)");
      listBoxWidget.Items.Add((object) "- New \"Accessories\" menu helps automate print settings (Pro)");
      listBoxWidget.Items.Add((object) "- Improved \"cat screen\" for quicker calibration adjustments (all models)");
      listBoxWidget.Items.Add((object) "- Remedied false alarms for \"Error 1006\" heater panics (Pro)");
      listBoxWidget.Items.Add((object) "- Numerous additional user interface and printer behavior improvements");
      listBoxWidget.Items.Add((object) "  (all models)");
      listBoxWidget.Items.Add((object) "- For full release notes, please see website");
      listBoxWidget.NoSelect = true;
      frame.AddChildElement((Element2D) listBoxWidget);
      ImageWidget imageWidget = new ImageWidget(0);
      imageWidget.Init(host, "guicontrols", 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f);
      imageWidget.Width = 129;
      imageWidget.Height = 31;
      imageWidget.Y = -40;
      imageWidget.CenterHorizontallyInParent = true;
      imageWidget.SandBoxing = false;
      frame.AddChildElement((Element2D) imageWidget);
      this.continue_button = new ButtonWidget(0);
      this.continue_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.continue_button.Size = FontSize.Medium;
      this.continue_button.Text = "Continue";
      this.continue_button.SetGrowableWidth(4, 4, 32);
      this.continue_button.SetGrowableHeight(4, 4, 32);
      this.continue_button.SetSize(100, 32);
      this.continue_button.SetPosition(-150, -40);
      this.continue_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.continue_button.CenterHorizontallyInParent = true;
      this.AddChildElement((Element2D) this.continue_button);
      frame.SetSize(frame.Width, frame.Height);
      frame.SetSize(660, 360);
      this.SetSize(750, 450);
      this.m_host.Refresh();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 0)
        return;
      this.Visible = false;
      this.m_host.GlobalChildDialog -= (Element2D) this;
      this.messagebox.AllowMessages = true;
    }

    private enum WelcomControlID
    {
      ContinueButton,
    }
  }
}
