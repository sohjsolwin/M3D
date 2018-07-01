// Decompiled with JetBrains decompiler
// Type: M3D.GUI.AccessoriesDialog.MainWindow
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.SettingsPages;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.AccessoriesDialog
{
  public class MainWindow : BorderedImageFrame
  {
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;
    private GUIHost host;
    private ScrollableVerticalLayout m_oTabButtonsVerticalLayout;
    public int window_width;
    public int window_height;
    private Frame tab_frame;
    private SettingsPage active_frame;
    private NozzlePage m_pageNozzle;

    public MainWindow(int ID, GUIHost host, SettingsManager main_controller, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.settingsManager = main_controller;
      this.messagebox = messagebox;
      this.host = host;
      this.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 37, 8, 64);
      this.SetSize(792, 356);
      TextWidget textWidget = new TextWidget(0);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "T_ACCESSORIES";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget);
      ButtonWidget buttonWidget = new ButtonWidget(1);
      buttonWidget.X = -40;
      buttonWidget.Y = 4;
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      this.tab_frame = new Frame(2);
      this.tab_frame.X = 210;
      this.tab_frame.Y = 45;
      this.tab_frame.RelativeWidth = 1f;
      this.tab_frame.RelativeWidthAdj = -220;
      this.tab_frame.RelativeHeight = 1f;
      this.tab_frame.RelativeHeightAdj = -55;
      this.AddChildElement((Element2D) this.tab_frame);
      this.m_oTabButtonsVerticalLayout = new ScrollableVerticalLayout();
      this.m_oTabButtonsVerticalLayout.Init(host);
      this.m_oTabButtonsVerticalLayout.SetSize(208, 200);
      this.m_oTabButtonsVerticalLayout.RelativeHeight = 1f;
      this.m_oTabButtonsVerticalLayout.RelativeHeightAdj = -40;
      this.m_oTabButtonsVerticalLayout.Y = 35;
      this.m_oTabButtonsVerticalLayout.X = 0;
      this.m_oTabButtonsVerticalLayout.BorderWidth = 0;
      this.m_oTabButtonsVerticalLayout.BorderHeight = 0;
      this.m_oTabButtonsVerticalLayout.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
      this.AddChildElement((Element2D) this.m_oTabButtonsVerticalLayout);
      Sprite.pixel_perfect = true;
      ButtonWidget tabButton = this.CreateTabButton(3, "T_AccessoriesTab_Nozzle");
      this.m_oTabButtonsVerticalLayout.Refresh();
      Sprite.pixel_perfect = false;
      this.Visible = false;
      this.CreateNozzlePage(host, spooler_connection, messagebox);
      int num = 1;
      tabButton.Checked = num != 0;
    }

    private ButtonWidget CreateTabButton(int ID, string text)
    {
      ButtonWidget buttonWidget = new ButtonWidget(ID);
      buttonWidget.SetPosition(0, 0);
      buttonWidget.SetSize(181, 64);
      buttonWidget.Text = text;
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(this.host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 18303;
      buttonWidget.Checked = false;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_oTabButtonsVerticalLayout.AddChildElement((Element2D) buttonWidget);
      return buttonWidget;
    }

    private void CreateNozzlePage(GUIHost host, SpoolerConnection spooler_connection, PopupMessageBox messagebox)
    {
      this.m_pageNozzle = new NozzlePage(3, host, spooler_connection, messagebox);
      this.tab_frame.AddChildElement((Element2D) this.m_pageNozzle);
      this.m_pageNozzle.Refresh();
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          this.Close();
          break;
        case 3:
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.m_pageNozzle;
          break;
      }
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
      this.active_frame.Refresh();
      this.active_frame.OnOpen();
    }

    private void TurnOffActiveFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.OnClose();
      this.active_frame.Visible = false;
      this.active_frame.Enabled = false;
      this.active_frame = (SettingsPage) null;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible || this.active_frame == null)
        return;
      this.active_frame.OnOpen();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      this.Close();
      return true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      this.Visible = false;
      if (this.host.HasChildDialog)
        this.host.GlobalChildDialog -= (Element2D) this;
      if (this.active_frame == null)
        return;
      this.active_frame.OnClose();
    }

    private enum AccessoriesID
    {
      Title,
      Close,
      TabFrame,
      NozzleSettings,
    }
  }
}
