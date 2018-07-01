// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Manage3DInkChildWindow
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.ManageFilament
{
  public abstract class Manage3DInkChildWindow : Frame
  {
    private Mangage3DInkStageDetails details;
    private GUIHost host;
    private Manage3DInkMainWindow mainWindow;

    public Manage3DInkChildWindow(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID)
    {
      this.host = host;
      this.mainWindow = mainWindow;
    }

    public abstract void MyButtonCallback(ButtonWidget button);

    public abstract void Init();

    public virtual void OnActivate(Mangage3DInkStageDetails details)
    {
      this.details = details;
    }

    protected void CreateManageFilamentFrame(string mainText, string subText, bool cancelButton, bool yesNoButton, bool createProgressBar, bool createFinishButton, bool createBack, bool createNext)
    {
      this.CreateManageFilamentFrame(mainText, FontSize.Medium, new Color4(0.35f, 0.35f, 0.35f, 1f), subText, FontSize.Medium, new Color4(0.35f, 0.35f, 0.35f, 1f), cancelButton, yesNoButton, createProgressBar, createFinishButton, createBack, createNext, new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue), new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue));
    }

    protected void CreateManageFilamentFrame(string mainText, FontSize mainTextSize, Color4 mainTextColor, string subText, FontSize subTextSize, Color4 subTextColor, bool cancelButton, bool yesNoButton, bool createProgressBar, bool createFinishButton, bool createBack, bool createNext, Color4 middleFrameColor, Color4 middleFrameBorderColor)
    {
      this.RelativeX = 0.0f;
      this.RelativeY = 0.0f;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      TextWidget textWidget1 = new TextWidget(1);
      textWidget1.Color = mainTextColor;
      textWidget1.Text = mainText;
      textWidget1.RelativeWidth = 1f;
      textWidget1.Size = mainTextSize;
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.SetPosition(0, 50);
      this.AddChildElement((Element2D) textWidget1);
      Frame frame = new Frame(2);
      frame.SetPosition(0, 100);
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.5f;
      frame.BGColor = middleFrameColor;
      frame.BorderColor = middleFrameBorderColor;
      TextWidget textWidget2 = new TextWidget(3);
      textWidget2.Color = subTextColor;
      textWidget2.Text = subText;
      textWidget2.RelativeWidth = 1f;
      textWidget2.Size = subTextSize;
      textWidget2.Alignment = QFontAlignment.Centre;
      textWidget2.SetPosition(0, 75);
      frame.AddChildElement((Element2D) textWidget2);
      if (createProgressBar)
      {
        ProgressBarWidget progressBarWidget = new ProgressBarWidget(4);
        progressBarWidget.Init(this.Host, "guicontrols", 944f, 96f, 960f, 144f, 2, 2, 16, 2, 2, 16);
        progressBarWidget.RelativeX = 0.15f;
        progressBarWidget.RelativeY = 0.65f;
        progressBarWidget.SetSize(375, 24);
        progressBarWidget.RelativeWidth = 0.7f;
        progressBarWidget.PercentComplete = 0.5f;
        progressBarWidget.BarColor = new Color4((byte) 37, (byte) 170, (byte) 225, byte.MaxValue);
        frame.AddChildElement((Element2D) progressBarWidget);
      }
      this.AddChildElement((Element2D) frame);
      if (yesNoButton)
      {
        ButtonWidget buttonWidget1 = new ButtonWidget(5);
        buttonWidget1.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget1.Size = FontSize.Medium;
        buttonWidget1.Text = "Yes";
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetSize(110, 40);
        buttonWidget1.SetPosition(110, -75);
        buttonWidget1.SetCallback(new ButtonCallback(this.MyButtonCallback));
        frame.AddChildElement((Element2D) buttonWidget1);
        ButtonWidget buttonWidget2 = new ButtonWidget(6);
        buttonWidget2.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "No";
        buttonWidget2.SetGrowableWidth(4, 4, 32);
        buttonWidget2.SetGrowableHeight(4, 4, 32);
        buttonWidget2.SetSize(110, 40);
        buttonWidget2.SetPosition(-220, -75);
        buttonWidget2.SetCallback(new ButtonCallback(this.MyButtonCallback));
        frame.AddChildElement((Element2D) buttonWidget2);
      }
      if (createBack)
      {
        ButtonWidget buttonWidget = new ButtonWidget(7);
        buttonWidget.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget.Size = FontSize.Medium;
        buttonWidget.Text = "Back";
        buttonWidget.SetGrowableWidth(4, 4, 32);
        buttonWidget.SetGrowableHeight(4, 4, 32);
        buttonWidget.SetSize(110, 40);
        buttonWidget.SetPosition(20, -50);
        buttonWidget.RelativeX = 0.025f;
        buttonWidget.RelativeY = -1000f;
        buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
        this.AddChildElement((Element2D) buttonWidget);
      }
      if (createNext)
      {
        ButtonWidget buttonWidget = new ButtonWidget(8);
        buttonWidget.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget.Size = FontSize.Medium;
        buttonWidget.Text = "Next";
        buttonWidget.SetGrowableWidth(4, 4, 32);
        buttonWidget.SetGrowableHeight(4, 4, 32);
        buttonWidget.SetSize(100, 32);
        buttonWidget.SetPosition(400, -50);
        buttonWidget.RelativeX = 0.8f;
        buttonWidget.RelativeY = -1000f;
        buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
        this.AddChildElement((Element2D) buttonWidget);
      }
      if (cancelButton)
      {
        ButtonWidget buttonWidget = new ButtonWidget(9);
        buttonWidget.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget.Size = FontSize.Medium;
        buttonWidget.Text = "Cancel";
        buttonWidget.SetGrowableWidth(4, 4, 32);
        buttonWidget.SetGrowableHeight(4, 4, 32);
        buttonWidget.SetSize(110, 40);
        buttonWidget.SetPosition(20, -50);
        buttonWidget.RelativeX = 0.025f;
        buttonWidget.RelativeY = -1000f;
        buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
        this.AddChildElement((Element2D) buttonWidget);
      }
      if (!createFinishButton)
        return;
      ButtonWidget buttonWidget3 = new ButtonWidget(10);
      buttonWidget3.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget3.Size = FontSize.Medium;
      buttonWidget3.Text = "Finish";
      buttonWidget3.SetGrowableWidth(4, 4, 32);
      buttonWidget3.SetGrowableHeight(4, 4, 32);
      buttonWidget3.SetSize(100, 32);
      buttonWidget3.SetPosition(400, -50);
      buttonWidget3.RelativeX = 0.8f;
      buttonWidget3.RelativeY = -1000f;
      buttonWidget3.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget3);
    }

    public GUIHost Host
    {
      get
      {
        return this.host;
      }
    }

    public Manage3DInkMainWindow MainWindow
    {
      get
      {
        return this.mainWindow;
      }
    }

    public Mangage3DInkStageDetails CurrentDetails
    {
      get
      {
        return this.details;
      }
    }

    public enum ControlIDs
    {
      TitleText = 1,
      MiddleFrame = 2,
      TextMain = 3,
      ProgressBar = 4,
      YesButton = 5,
      NoButton = 6,
      Back = 7,
      Next = 8,
      Cancel = 9,
      Finish = 10, // 0x0000000A
      LastIDInList = 11, // 0x0000000B
    }
  }
}
