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
      settingsManager = main_controller;
      this.messagebox = messagebox;
      this.host = host;
      Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 37, 8, 64);
      SetSize(792, 356);
      var textWidget = new TextWidget(0);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "T_ACCESSORIES";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      AddChildElement(textWidget);
      var buttonWidget = new ButtonWidget(1)
      {
        X = -40,
        Y = 4
      };
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      AddChildElement(buttonWidget);
      tab_frame = new Frame(2)
      {
        X = 210,
        Y = 45,
        RelativeWidth = 1f,
        RelativeWidthAdj = -220,
        RelativeHeight = 1f,
        RelativeHeightAdj = -55
      };
      AddChildElement(tab_frame);
      m_oTabButtonsVerticalLayout = new ScrollableVerticalLayout();
      m_oTabButtonsVerticalLayout.Init(host);
      m_oTabButtonsVerticalLayout.SetSize(208, 200);
      m_oTabButtonsVerticalLayout.RelativeHeight = 1f;
      m_oTabButtonsVerticalLayout.RelativeHeightAdj = -40;
      m_oTabButtonsVerticalLayout.Y = 35;
      m_oTabButtonsVerticalLayout.X = 0;
      m_oTabButtonsVerticalLayout.BorderWidth = 0;
      m_oTabButtonsVerticalLayout.BorderHeight = 0;
      m_oTabButtonsVerticalLayout.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
      AddChildElement(m_oTabButtonsVerticalLayout);
      Sprite.pixel_perfect = true;
      ButtonWidget tabButton = CreateTabButton(3, "T_AccessoriesTab_Nozzle");
      m_oTabButtonsVerticalLayout.Refresh();
      Sprite.pixel_perfect = false;
      Visible = false;
      CreateNozzlePage(host, spooler_connection, messagebox);
      var num = 1;
      tabButton.Checked = num != 0;
    }

    private ButtonWidget CreateTabButton(int ID, string text)
    {
      var buttonWidget = new ButtonWidget(ID);
      buttonWidget.SetPosition(0, 0);
      buttonWidget.SetSize(181, 64);
      buttonWidget.Text = text;
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 18303;
      buttonWidget.Checked = false;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      m_oTabButtonsVerticalLayout.AddChildElement(buttonWidget);
      return buttonWidget;
    }

    private void CreateNozzlePage(GUIHost host, SpoolerConnection spooler_connection, PopupMessageBox messagebox)
    {
      m_pageNozzle = new NozzlePage(3, host, spooler_connection, messagebox);
      tab_frame.AddChildElement(m_pageNozzle);
      m_pageNozzle.Refresh();
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
          Close();
          break;
        case 3:
          TurnOffActiveFrame();
          active_frame = m_pageNozzle;
          break;
      }
      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
      active_frame.Refresh();
      active_frame.OnOpen();
    }

    private void TurnOffActiveFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.OnClose();
      active_frame.Visible = false;
      active_frame.Enabled = false;
      active_frame = null;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible || active_frame == null)
      {
        return;
      }

      active_frame.OnOpen();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      Close();
      return true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      Visible = false;
      if (host.HasChildDialog)
      {
        host.GlobalChildDialog -= (this);
      }

      if (active_frame == null)
      {
        return;
      }

      active_frame.OnClose();
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
