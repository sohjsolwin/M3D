// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.LibraryView
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Forms;
using M3D.Spooling.Common;
using OpenTK;
using OpenTK.Graphics;
using QuickFont;
using System;

namespace M3D.GUI.Views.Library_View
{
  public class LibraryView : Frame
  {
    protected long animationTime = 1500;
    private ThreadSafeVariable<bool> refresh = new ThreadSafeVariable<bool>(false);
    private int temp_exception_count;
    private GUIHost m_gui_host;
    protected long startTime;
    protected long elapsed;
    private int lastpage_count;
    private HorizontalLayout tabsFrame;
    private GridLayout LibraryGrid;
    private TextWidget library_status;
    private Frame navigation;
    private ButtonWidget navigation_left;
    private ButtonWidget navigation_right;
    private string search_filter;
    private ButtonWidget[] pagebuttons;
    private ViewState viewstate;
    private int realanimtiontime;
    private float target_x;
    public LibraryRecord recorddata_to_load;
    private RecentModelTab recentModelsTab;
    private RecentPrintsTab recentPrintsTab;
    private LibraryViewTab currentTab;

    public LibraryView(int ID, Element2D parent, GLControl glControl, GUIHost host, MessagePopUp infobox, ModelLoadingManager model_loading_manager)
      : base(ID, parent)
    {
      this.bUpdateWhenNotVisible = true;
      this.m_gui_host = host;
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      this.m_gui_host.SetFontProperty(FontSize.VeryLarge, 20f);
      this.m_gui_host.SetFontProperty(FontSize.Large, 14f);
      this.m_gui_host.SetFontProperty(FontSize.Medium, 11f);
      this.m_gui_host.SetFontProperty(FontSize.Small, 8f);
      this.RelativeX = 0.51f;
      this.RelativeY = 0.11f;
      this.RelativeWidth = 0.423f;
      this.RelativeHeight = 0.83f;
      ImageWidget imageWidget1 = new ImageWidget(1008, (Element2D) null);
      imageWidget1.Init(host, "extendedcontrols3", 3f, 288f, 84f, 374f, 3f, 288f, 84f, 374f, 3f, 288f, 84f, 374f);
      imageWidget1.Text = "Remove From List";
      imageWidget1.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      imageWidget1.VAlignment = TextVerticalAlignment.Bottom;
      imageWidget1.TextAreaHeight = 32;
      imageWidget1.ImageAreaWidth = 80;
      imageWidget1.SetSize(80, 115);
      imageWidget1.Visible = false;
      this.AddChildElement((Element2D) imageWidget1);
      imageWidget1.SetPosition(-12, -115);
      ImageWidget imageWidget2 = new ImageWidget(1009, (Element2D) null);
      imageWidget2.Init(host, "extendedcontrols3", 92f, 285f, 173f, 346f, 92f, 285f, 173f, 346f, 92f, 285f, 173f, 346f);
      imageWidget2.Text = "Save";
      imageWidget2.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      imageWidget2.VAlignment = TextVerticalAlignment.Bottom;
      imageWidget2.TextAreaHeight = 20;
      imageWidget2.ImageAreaWidth = 81;
      imageWidget2.SetSize(81, 85);
      imageWidget2.Visible = false;
      this.AddChildElement((Element2D) imageWidget2);
      imageWidget2.SetPosition(-12, -240);
      this.search_filter = "";
      EditBoxWidget editBoxWidget = new EditBoxWidget(1001, (Element2D) null);
      editBoxWidget.Init(host, "guicontrols", 513f, 0.0f, 608f, 63f);
      editBoxWidget.SetGrowableWidth(40, 16, 64);
      editBoxWidget.Size = FontSize.Large;
      editBoxWidget.Color = new Color4(0.71f, 0.71f, 0.71f, 1f);
      editBoxWidget.SetTextWindowBorders(48, 16, 22, 16);
      editBoxWidget.SetToolTipRegion(0, 48, 0, 60);
      editBoxWidget.ToolTipMessage = host.Locale.T("T_TOOLTIP_SEARCH");
      editBoxWidget.Hint = this.m_gui_host.Locale.T("T_SEARCH");
      this.tabsFrame = new HorizontalLayout(0, (Element2D) null);
      this.tabsFrame.FixedColumnWidth = true;
      this.tabsFrame.BorderWidth = 0;
      this.tabsFrame.BorderHeight = 0;
      this.tabsFrame.RelativeWidth = 1f;
      this.navigation = new Frame(0, (Element2D) null);
      this.navigation_left = new ButtonWidget(1005, (Element2D) null);
      this.navigation_left.Text = "";
      this.navigation_left.X = 16;
      this.navigation_left.Y = 0;
      this.navigation_left.Width = 32;
      this.navigation_left.Height = 32;
      this.navigation_left.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.navigation_left.Init(host, "guicontrols", 608f, 0.0f, 639f, 31f, 640f, 0.0f, 671f, 31f, 672f, 0.0f, 703f, 31f, 704f, 0.0f, 735f, 31f);
      this.navigation_right = new ButtonWidget(1006, (Element2D) null);
      this.navigation_right.Text = "";
      this.navigation_right.X = -48;
      this.navigation_right.Y = 0;
      this.navigation_right.Width = 32;
      this.navigation_right.Height = 32;
      this.navigation_right.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.navigation_right.Init(host, "guicontrols", 608f, 32f, 639f, 63f, 640f, 32f, 671f, 63f, 672f, 32f, 703f, 63f, 704f, 32f, 735f, 63f);
      this.pagebuttons = new ButtonWidget[32];
      for (int ID1 = 1032; ID1 <= 1063; ++ID1)
      {
        int index = ID1 - 1032;
        this.pagebuttons[index] = new ButtonWidget(ID1, (Element2D) null);
        this.pagebuttons[index].Text = "";
        this.pagebuttons[index].X = 48 + (ID1 - 1032) * 24;
        this.pagebuttons[index].Y = 8;
        this.pagebuttons[index].Width = 16;
        this.pagebuttons[index].Height = 16;
        this.pagebuttons[index].SetCallback(new ButtonCallback(this.MyButtonCallback));
        this.pagebuttons[index].Init(host, "guicontrols", 448f, 192f, 463f, 208f, 480f, 192f, 495f, 208f, 464f, 192f, 479f, 208f);
        this.pagebuttons[index].DontMove = true;
        this.pagebuttons[index].GroupID = 1;
        this.pagebuttons[index].ClickType = ButtonType.Checkable;
        this.pagebuttons[index].Visible = false;
        this.navigation.AddChildElement((Element2D) this.pagebuttons[index]);
      }
      this.navigation.AddChildElement((Element2D) this.navigation_left);
      this.navigation.AddChildElement((Element2D) this.navigation_right);
      this.LibraryGrid = new GridLayout(1);
      this.LibraryGrid.ColumnWidth = 130;
      this.LibraryGrid.RowHeight = 150;
      this.LibraryGrid.BorderWidth = 0;
      this.LibraryGrid.BorderHeight = 0;
      VerticalLayout verticalLayout = new VerticalLayout(0);
      verticalLayout.RelativeHeight = 1f;
      verticalLayout.RelativeWidth = 1f;
      verticalLayout.BorderHeight = 10;
      verticalLayout.AddChildElement((Element2D) editBoxWidget, 0, 64 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D) this.tabsFrame, 1, 64 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D) this.navigation, 2, 32 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D) this.LibraryGrid, 3, -1);
      this.AddChildElement((Element2D) verticalLayout);
      this.library_status = new TextWidget(1007);
      this.library_status.Text = this.m_gui_host.Locale.T("T_NOMODELS");
      this.library_status.Size = FontSize.VeryLarge;
      this.library_status.Alignment = QFontAlignment.Centre;
      this.library_status.RelativeHeight = 1f;
      this.library_status.RelativeWidth = 1f;
      this.library_status.X = 0;
      this.library_status.Y = 0;
      this.library_status.Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f);
      this.AddChildElement((Element2D) this.library_status);
      this.recentModelsTab = new RecentModelTab(this, model_loading_manager, infobox, glControl);
      ButtonWidget buttonWidget = this.AddTabButton(host, (LibraryViewTab) this.recentModelsTab, LibraryView.TabButtonStyle.Left, this.m_gui_host.Locale.T("T_RECENT_MODELS"), 1002);
      this.recentPrintsTab = new RecentPrintsTab(this, model_loading_manager);
      this.AddTabButton(host, (LibraryViewTab) this.recentPrintsTab, LibraryView.TabButtonStyle.Right, this.m_gui_host.Locale.T("T_RECENT_PRINTS"), 1004);
      int num = 1;
      buttonWidget.SetChecked(num != 0);
      this.ShowView(true);
      this.viewstate = ViewState.Active;
    }

    private ButtonWidget AddTabButton(GUIHost host, LibraryViewTab tabFrame, LibraryView.TabButtonStyle style, string text, int ID)
    {
      ButtonWidget buttonWidget = new ButtonWidget(ID, (Element2D) null);
      buttonWidget.Text = text;
      buttonWidget.TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f);
      buttonWidget.TextOverColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.SetCallback(new ButtonCallback(this.TabButtonCallback));
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 1;
      buttonWidget.Data = (object) tabFrame;
      switch (style)
      {
        case LibraryView.TabButtonStyle.Left:
          buttonWidget.Init(host, "guicontrols", 513f, 64f, 575f, (float) sbyte.MaxValue, 513f, 128f, 575f, 191f, 513f, 192f, 575f, (float) byte.MaxValue, 513f, 64f, 575f, (float) sbyte.MaxValue);
          break;
        case LibraryView.TabButtonStyle.Middle:
          buttonWidget.Init(host, "guicontrols", 576f, 64f, 639f, (float) sbyte.MaxValue, 576f, 128f, 639f, 191f, 576f, 192f, 639f, (float) byte.MaxValue, 576f, 64f, 639f, (float) sbyte.MaxValue);
          break;
        case LibraryView.TabButtonStyle.Right:
          buttonWidget.Init(host, "guicontrols", 640f, 64f, 703f, (float) sbyte.MaxValue, 640f, 128f, 703f, 191f, 640f, 192f, 703f, (float) byte.MaxValue, 640f, 64f, 703f, (float) sbyte.MaxValue);
          break;
      }
      buttonWidget.SetGrowableWidth(16, 16, 48);
      this.tabsFrame.AddChildElement((Element2D) buttonWidget);
      return buttonWidget;
    }

    public override void OnRender(GUIHost host)
    {
      if (this.LibraryGrid != null)
      {
        if (this.LibraryGrid.Count < 1)
          this.library_status.Visible = true;
        else
          this.library_status.Visible = false;
        if (this.LibraryGrid.CurPage == 0)
          this.navigation_left.Enabled = false;
        else
          this.navigation_left.Enabled = true;
        if (this.LibraryGrid.CurPage == this.LibraryGrid.PageCount - 1)
          this.navigation_right.Enabled = false;
        else
          this.navigation_right.Enabled = true;
        this.ResetPageButtons();
      }
      base.OnRender(host);
    }

    public void ScheduleRefresh()
    {
      this.refresh.Value = true;
    }

    private void RefreshTab()
    {
      if (this.currentTab == null)
        return;
      this.currentTab.Show(this.m_gui_host, this.LibraryGrid, this.search_filter);
    }

    private void ResetPageButtons()
    {
      string extra_info = "ResetPageButtons:";
      try
      {
        if (this.LibraryGrid == null)
          return;
        extra_info = "ResetPageButtons:1";
        if (this.lastpage_count != this.LibraryGrid.PageCount)
        {
          extra_info = "ResetPageButtons:2";
          this.lastpage_count = this.LibraryGrid.PageCount;
        }
        extra_info = "ResetPageButtons:3";
        int curPage = this.LibraryGrid.CurPage;
        extra_info = "ResetPageButtons:4";
        for (int index = 1032; index < 1063; ++index)
        {
          extra_info = "ResetPageButtons:5";
          this.pagebuttons[index - 1032].Visible = false;
          extra_info = "ResetPageButtons:6";
          this.pagebuttons[index - 1032].SetChecked(false);
        }
        extra_info = "ResetPageButtons:7";
        this.LibraryGrid.CurPage = curPage;
        extra_info = "ResetPageButtons:8";
        if (this.LibraryGrid.Count < 1)
          return;
        extra_info = "ResetPageButtons:9";
        int num1 = this.navigation.Width - 96;
        int num2 = num1 / 16;
        int num3 = this.lastpage_count;
        if (num3 > num2)
          num3 = num2;
        if (num3 > 32)
          num3 = 32;
        extra_info = "ResetPageButtons:10";
        int num4 = (num1 - num3 * 16) / 2 + 48;
        for (int index = 0; index < num3; ++index)
        {
          extra_info = "ResetPageButtons:11";
          this.pagebuttons[index].X = num4;
          extra_info = "ResetPageButtons:12";
          this.pagebuttons[index].Visible = true;
          num4 += 16;
        }
        extra_info = "ResetPageButtons:13";
        if (curPage >= num3)
          return;
        this.pagebuttons[curPage].SetChecked(true);
      }
      catch (Exception ex)
      {
        if (this.temp_exception_count < 100)
          ++this.temp_exception_count;
        else
          ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
    }

    public void TransitionViewState(ViewState new_state)
    {
      if (new_state == this.viewstate || new_state == ViewState.ToActive || new_state == ViewState.ToHidden)
        return;
      float num1 = 0.5f;
      float num2 = 1f;
      if (new_state == ViewState.Active)
      {
        this.viewstate = ViewState.ToActive;
        this.target_x = num1;
        this.animationTime = 1500L;
      }
      else
      {
        this.viewstate = ViewState.ToHidden;
        this.target_x = num2;
        this.animationTime = 300L;
      }
      this.realanimtiontime = (int) this.animationTime;
      this.startTime = DateTime.Now.Ticks / 10000L;
    }

    private void ShowView(bool show)
    {
      if (this.Visible == show)
        return;
      this.Visible = show;
    }

    public override void OnUpdate()
    {
      if (this.recorddata_to_load != null && this.currentTab != null)
      {
        this.currentTab.LoadRecord(this.recorddata_to_load);
        this.recorddata_to_load = (LibraryRecord) null;
      }
      if (this.viewstate == ViewState.ToActive || this.viewstate == ViewState.ToHidden)
      {
        this.elapsed = DateTime.Now.Ticks / 10000L - this.startTime;
        if (this.elapsed >= (long) this.realanimtiontime || (double) this.X == (double) this.target_x)
        {
          this.elapsed = (long) this.realanimtiontime;
          this.viewstate = this.viewstate != ViewState.ToActive ? ViewState.Hidden : ViewState.Active;
        }
        float num = this.target_x - (this.target_x - this.RelativeX) * (float) (1.0 - (double) this.elapsed / (double) this.realanimtiontime);
        this.RelativeX = num;
        this.ShowView((double) num < 1.0);
      }
      if (this.refresh.Value)
      {
        this.RefreshTab();
        this.refresh.Value = false;
      }
      base.OnUpdate();
    }

    public void TabButtonCallback(ButtonWidget button)
    {
      if (!(button.Data is LibraryViewTab))
        return;
      this.currentTab = (LibraryViewTab) button.Data;
      this.ScheduleRefresh();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID >= 1032 && button.ID <= 1063)
        this.LibraryGrid.CurPage = button.ID - 1032;
      switch (button.ID)
      {
        case 1005:
          --this.LibraryGrid.CurPage;
          break;
        case 1006:
          ++this.LibraryGrid.CurPage;
          break;
      }
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      switch (msg)
      {
        case ControlMsg.MSG_MOVE:
          if (this.currentTab.CanRemoveRecords)
          {
            ImageWidget childElement = (ImageWidget) this.FindChildElement(1008);
            if (childElement != null)
              childElement.Visible = true;
          }
          if (!this.currentTab.CanSaveRecords)
            break;
          ImageWidget childElement1 = (ImageWidget) this.FindChildElement(1009);
          if (childElement1 == null)
            break;
          childElement1.Visible = true;
          break;
        case ControlMsg.ENTERHIT:
          if (the_control.GetElementType() != ElementType.EditBoxWidget)
            break;
          EditBoxWidget editBoxWidget = (EditBoxWidget) the_control;
          if (editBoxWidget.ID != 1001)
            break;
          this.search_filter = editBoxWidget.Text;
          this.RefreshTab();
          break;
        case ControlMsg.MSG_DRAGSTOP:
          ImageWidget childElement2 = (ImageWidget) this.FindChildElement(1008);
          ImageWidget childElement3 = (ImageWidget) this.FindChildElement(1009);
          if (the_control.GetElementType() == ElementType.ButtonWidget)
          {
            ButtonWidget buttonWidget = (ButtonWidget) the_control;
            if (buttonWidget.Data != null && buttonWidget.Data is LibraryRecord)
            {
              if (this.currentTab.CanRemoveRecords && buttonWidget.Overlaps((Element2D) childElement2, (int) xparam - buttonWidget.X_Abs, (int) yparam - buttonWidget.Y_Abs))
              {
                this.currentTab.RemoveRecord((LibraryRecord) buttonWidget.Data);
                this.refresh.Value = true;
              }
              else if (this.currentTab.CanSaveRecords && buttonWidget.Overlaps((Element2D) childElement3, (int) xparam - buttonWidget.X_Abs, (int) yparam - buttonWidget.Y_Abs))
              {
                this.currentTab.SaveRecord((LibraryRecord) buttonWidget.Data);
                this.refresh.Value = true;
              }
              else
                this.recorddata_to_load = (LibraryRecord) buttonWidget.Data;
            }
          }
          if (childElement2 != null)
            childElement2.Visible = false;
          if (childElement3 == null)
            break;
          childElement3.Visible = false;
          break;
        default:
          base.OnControlMsg(the_control, msg, xparam, yparam);
          break;
      }
    }

    public RecentModelTab RecentModels
    {
      get
      {
        return this.recentModelsTab;
      }
    }

    public RecentPrintsTab RecentPrints
    {
      get
      {
        return this.recentPrintsTab;
      }
    }

    public enum LibraryControlID
    {
      Static = 0,
      SearchBar = 1001, // 0x000003E9
      RecentModelsTab = 1002, // 0x000003EA
      MiddleTab = 1003, // 0x000003EB
      RecentPrintsTab = 1004, // 0x000003EC
      PrevPageButton = 1005, // 0x000003ED
      NextPageButton = 1006, // 0x000003EE
      LibraryStatusText = 1007, // 0x000003EF
      TrashIcon = 1008, // 0x000003F0
      SaveIcon = 1009, // 0x000003F1
      FirstPageButton = 1032, // 0x00000408
      LastPageButton = 1063, // 0x00000427
      FirstModelCell = 1064, // 0x00000428
      LibraryView = 10001, // 0x00002711
    }

    public enum TabButtonStyle
    {
      Left,
      Middle,
      Right,
    }

    private enum LibraryControlGroups
    {
      TabsGroup = 1,
      PageButtonsGroup = 2,
    }
  }
}
