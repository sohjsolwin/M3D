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
      bUpdateWhenNotVisible = true;
      m_gui_host = host;
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      m_gui_host.SetFontProperty(FontSize.VeryLarge, 20f);
      m_gui_host.SetFontProperty(FontSize.Large, 14f);
      m_gui_host.SetFontProperty(FontSize.Medium, 11f);
      m_gui_host.SetFontProperty(FontSize.Small, 8f);
      RelativeX = 0.51f;
      RelativeY = 0.11f;
      RelativeWidth = 0.423f;
      RelativeHeight = 0.83f;
      var imageWidget1 = new ImageWidget(1008, (Element2D) null);
      imageWidget1.Init(host, "extendedcontrols3", 3f, 288f, 84f, 374f, 3f, 288f, 84f, 374f, 3f, 288f, 84f, 374f);
      imageWidget1.Text = "Remove From List";
      imageWidget1.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      imageWidget1.VAlignment = TextVerticalAlignment.Bottom;
      imageWidget1.TextAreaHeight = 32;
      imageWidget1.ImageAreaWidth = 80;
      imageWidget1.SetSize(80, 115);
      imageWidget1.Visible = false;
      AddChildElement((Element2D) imageWidget1);
      imageWidget1.SetPosition(-12, -115);
      var imageWidget2 = new ImageWidget(1009, (Element2D) null);
      imageWidget2.Init(host, "extendedcontrols3", 92f, 285f, 173f, 346f, 92f, 285f, 173f, 346f, 92f, 285f, 173f, 346f);
      imageWidget2.Text = "Save";
      imageWidget2.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      imageWidget2.VAlignment = TextVerticalAlignment.Bottom;
      imageWidget2.TextAreaHeight = 20;
      imageWidget2.ImageAreaWidth = 81;
      imageWidget2.SetSize(81, 85);
      imageWidget2.Visible = false;
      AddChildElement((Element2D) imageWidget2);
      imageWidget2.SetPosition(-12, -240);
      search_filter = "";
      var editBoxWidget = new EditBoxWidget(1001, (Element2D) null);
      editBoxWidget.Init(host, "guicontrols", 513f, 0.0f, 608f, 63f);
      editBoxWidget.SetGrowableWidth(40, 16, 64);
      editBoxWidget.Size = FontSize.Large;
      editBoxWidget.Color = new Color4(0.71f, 0.71f, 0.71f, 1f);
      editBoxWidget.SetTextWindowBorders(48, 16, 22, 16);
      editBoxWidget.SetToolTipRegion(0, 48, 0, 60);
      editBoxWidget.ToolTipMessage = host.Locale.T("T_TOOLTIP_SEARCH");
      editBoxWidget.Hint = m_gui_host.Locale.T("T_SEARCH");
      tabsFrame = new HorizontalLayout(0, (Element2D)null)
      {
        FixedColumnWidth = true,
        BorderWidth = 0,
        BorderHeight = 0,
        RelativeWidth = 1f
      };
      navigation = new Frame(0, (Element2D) null);
      navigation_left = new ButtonWidget(1005, (Element2D)null)
      {
        Text = "",
        X = 16,
        Y = 0,
        Width = 32,
        Height = 32
      };
      navigation_left.SetCallback(new ButtonCallback(MyButtonCallback));
      navigation_left.Init(host, "guicontrols", 608f, 0.0f, 639f, 31f, 640f, 0.0f, 671f, 31f, 672f, 0.0f, 703f, 31f, 704f, 0.0f, 735f, 31f);
      navigation_right = new ButtonWidget(1006, (Element2D)null)
      {
        Text = "",
        X = -48,
        Y = 0,
        Width = 32,
        Height = 32
      };
      navigation_right.SetCallback(new ButtonCallback(MyButtonCallback));
      navigation_right.Init(host, "guicontrols", 608f, 32f, 639f, 63f, 640f, 32f, 671f, 63f, 672f, 32f, 703f, 63f, 704f, 32f, 735f, 63f);
      pagebuttons = new ButtonWidget[32];
      for (var ID1 = 1032; ID1 <= 1063; ++ID1)
      {
        var index = ID1 - 1032;
        pagebuttons[index] = new ButtonWidget(ID1, (Element2D)null)
        {
          Text = "",
          X = 48 + (ID1 - 1032) * 24,
          Y = 8,
          Width = 16,
          Height = 16
        };
        pagebuttons[index].SetCallback(new ButtonCallback(MyButtonCallback));
        pagebuttons[index].Init(host, "guicontrols", 448f, 192f, 463f, 208f, 480f, 192f, 495f, 208f, 464f, 192f, 479f, 208f);
        pagebuttons[index].DontMove = true;
        pagebuttons[index].GroupID = 1;
        pagebuttons[index].ClickType = ButtonType.Checkable;
        pagebuttons[index].Visible = false;
        navigation.AddChildElement((Element2D)pagebuttons[index]);
      }
      navigation.AddChildElement((Element2D)navigation_left);
      navigation.AddChildElement((Element2D)navigation_right);
      LibraryGrid = new GridLayout(1)
      {
        ColumnWidth = 130,
        RowHeight = 150,
        BorderWidth = 0,
        BorderHeight = 0
      };
      var verticalLayout = new VerticalLayout(0)
      {
        RelativeHeight = 1f,
        RelativeWidth = 1f,
        BorderHeight = 10
      };
      verticalLayout.AddChildElement((Element2D) editBoxWidget, 0, 64 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D)tabsFrame, 1, 64 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D)navigation, 2, 32 + verticalLayout.BorderHeight);
      verticalLayout.AddChildElement((Element2D)LibraryGrid, 3, -1);
      AddChildElement((Element2D) verticalLayout);
      library_status = new TextWidget(1007)
      {
        Text = m_gui_host.Locale.T("T_NOMODELS"),
        Size = FontSize.VeryLarge,
        Alignment = QFontAlignment.Centre,
        RelativeHeight = 1f,
        RelativeWidth = 1f,
        X = 0,
        Y = 0,
        Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f)
      };
      AddChildElement((Element2D)library_status);
      recentModelsTab = new RecentModelTab(this, model_loading_manager, infobox, glControl);
      ButtonWidget buttonWidget = AddTabButton(host, (LibraryViewTab)recentModelsTab, LibraryView.TabButtonStyle.Left, m_gui_host.Locale.T("T_RECENT_MODELS"), 1002);
      recentPrintsTab = new RecentPrintsTab(this, model_loading_manager);
      AddTabButton(host, (LibraryViewTab)recentPrintsTab, LibraryView.TabButtonStyle.Right, m_gui_host.Locale.T("T_RECENT_PRINTS"), 1004);
      var num = 1;
      buttonWidget.SetChecked(num != 0);
      ShowView(true);
      viewstate = ViewState.Active;
    }

    private ButtonWidget AddTabButton(GUIHost host, LibraryViewTab tabFrame, LibraryView.TabButtonStyle style, string text, int ID)
    {
      var buttonWidget = new ButtonWidget(ID, (Element2D)null)
      {
        Text = text,
        TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f),
        TextOverColor = new Color4(1f, 1f, 1f, 1f),
        TextDownColor = new Color4(1f, 1f, 1f, 1f),
        Size = FontSize.Medium
      };
      buttonWidget.SetCallback(new ButtonCallback(TabButtonCallback));
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
      tabsFrame.AddChildElement((Element2D) buttonWidget);
      return buttonWidget;
    }

    public override void OnRender(GUIHost host)
    {
      if (LibraryGrid != null)
      {
        if (LibraryGrid.Count < 1)
        {
          library_status.Visible = true;
        }
        else
        {
          library_status.Visible = false;
        }

        if (LibraryGrid.CurPage == 0)
        {
          navigation_left.Enabled = false;
        }
        else
        {
          navigation_left.Enabled = true;
        }

        if (LibraryGrid.CurPage == LibraryGrid.PageCount - 1)
        {
          navigation_right.Enabled = false;
        }
        else
        {
          navigation_right.Enabled = true;
        }

        ResetPageButtons();
      }
      base.OnRender(host);
    }

    public void ScheduleRefresh()
    {
      refresh.Value = true;
    }

    private void RefreshTab()
    {
      if (currentTab == null)
      {
        return;
      }

      currentTab.Show(m_gui_host, LibraryGrid, search_filter);
    }

    private void ResetPageButtons()
    {
      var extra_info = "ResetPageButtons:";
      try
      {
        if (LibraryGrid == null)
        {
          return;
        }

        extra_info = "ResetPageButtons:1";
        if (lastpage_count != LibraryGrid.PageCount)
        {
          extra_info = "ResetPageButtons:2";
          lastpage_count = LibraryGrid.PageCount;
        }
        extra_info = "ResetPageButtons:3";
        var curPage = LibraryGrid.CurPage;
        extra_info = "ResetPageButtons:4";
        for (var index = 1032; index < 1063; ++index)
        {
          extra_info = "ResetPageButtons:5";
          pagebuttons[index - 1032].Visible = false;
          extra_info = "ResetPageButtons:6";
          pagebuttons[index - 1032].SetChecked(false);
        }
        extra_info = "ResetPageButtons:7";
        LibraryGrid.CurPage = curPage;
        extra_info = "ResetPageButtons:8";
        if (LibraryGrid.Count < 1)
        {
          return;
        }

        extra_info = "ResetPageButtons:9";
        var num1 = navigation.Width - 96;
        var num2 = num1 / 16;
        var num3 = lastpage_count;
        if (num3 > num2)
        {
          num3 = num2;
        }

        if (num3 > 32)
        {
          num3 = 32;
        }

        extra_info = "ResetPageButtons:10";
        var num4 = (num1 - num3 * 16) / 2 + 48;
        for (var index = 0; index < num3; ++index)
        {
          extra_info = "ResetPageButtons:11";
          pagebuttons[index].X = num4;
          extra_info = "ResetPageButtons:12";
          pagebuttons[index].Visible = true;
          num4 += 16;
        }
        extra_info = "ResetPageButtons:13";
        if (curPage >= num3)
        {
          return;
        }

        pagebuttons[curPage].SetChecked(true);
      }
      catch (Exception ex)
      {
        if (temp_exception_count < 100)
        {
          ++temp_exception_count;
        }
        else
        {
          ExceptionForm.ShowExceptionForm(ex, extra_info);
        }
      }
    }

    public void TransitionViewState(ViewState new_state)
    {
      if (new_state == viewstate || new_state == ViewState.ToActive || new_state == ViewState.ToHidden)
      {
        return;
      }

      var num1 = 0.5f;
      var num2 = 1f;
      if (new_state == ViewState.Active)
      {
        viewstate = ViewState.ToActive;
        target_x = num1;
        animationTime = 1500L;
      }
      else
      {
        viewstate = ViewState.ToHidden;
        target_x = num2;
        animationTime = 300L;
      }
      realanimtiontime = (int)animationTime;
      startTime = DateTime.Now.Ticks / 10000L;
    }

    private void ShowView(bool show)
    {
      if (Visible == show)
      {
        return;
      }

      Visible = show;
    }

    public override void OnUpdate()
    {
      if (recorddata_to_load != null && currentTab != null)
      {
        currentTab.LoadRecord(recorddata_to_load);
        recorddata_to_load = (LibraryRecord) null;
      }
      if (viewstate == ViewState.ToActive || viewstate == ViewState.ToHidden)
      {
        elapsed = DateTime.Now.Ticks / 10000L - startTime;
        if (elapsed >= (long)realanimtiontime || (double)X == (double)target_x)
        {
          elapsed = (long)realanimtiontime;
          viewstate = viewstate != ViewState.ToActive ? ViewState.Hidden : ViewState.Active;
        }
        var num = target_x - (target_x - RelativeX) * (float) (1.0 - (double)elapsed / (double)realanimtiontime);
        RelativeX = num;
        ShowView((double) num < 1.0);
      }
      if (refresh.Value)
      {
        RefreshTab();
        refresh.Value = false;
      }
      base.OnUpdate();
    }

    public void TabButtonCallback(ButtonWidget button)
    {
      if (!(button.Data is LibraryViewTab))
      {
        return;
      }

      currentTab = (LibraryViewTab) button.Data;
      ScheduleRefresh();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID >= 1032 && button.ID <= 1063)
      {
        LibraryGrid.CurPage = button.ID - 1032;
      }

      switch (button.ID)
      {
        case 1005:
          --LibraryGrid.CurPage;
          break;
        case 1006:
          ++LibraryGrid.CurPage;
          break;
      }
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      switch (msg)
      {
        case ControlMsg.MSG_MOVE:
          if (currentTab.CanRemoveRecords)
          {
            var childElement = (ImageWidget)FindChildElement(1008);
            if (childElement != null)
            {
              childElement.Visible = true;
            }
          }
          if (!currentTab.CanSaveRecords)
          {
            break;
          }

          var childElement1 = (ImageWidget)FindChildElement(1009);
          if (childElement1 == null)
          {
            break;
          }

          childElement1.Visible = true;
          break;
        case ControlMsg.ENTERHIT:
          if (the_control.GetElementType() != ElementType.EditBoxWidget)
          {
            break;
          }

          var editBoxWidget = (EditBoxWidget) the_control;
          if (editBoxWidget.ID != 1001)
          {
            break;
          }

          search_filter = editBoxWidget.Text;
          RefreshTab();
          break;
        case ControlMsg.MSG_DRAGSTOP:
          var childElement2 = (ImageWidget)FindChildElement(1008);
          var childElement3 = (ImageWidget)FindChildElement(1009);
          if (the_control.GetElementType() == ElementType.ButtonWidget)
          {
            var buttonWidget = (ButtonWidget) the_control;
            if (buttonWidget.Data != null && buttonWidget.Data is LibraryRecord)
            {
              if (currentTab.CanRemoveRecords && buttonWidget.Overlaps((Element2D) childElement2, (int) xparam - buttonWidget.X_Abs, (int) yparam - buttonWidget.Y_Abs))
              {
                currentTab.RemoveRecord((LibraryRecord) buttonWidget.Data);
                refresh.Value = true;
              }
              else if (currentTab.CanSaveRecords && buttonWidget.Overlaps((Element2D) childElement3, (int) xparam - buttonWidget.X_Abs, (int) yparam - buttonWidget.Y_Abs))
              {
                currentTab.SaveRecord((LibraryRecord) buttonWidget.Data);
                refresh.Value = true;
              }
              else
              {
                recorddata_to_load = (LibraryRecord) buttonWidget.Data;
              }
            }
          }
          if (childElement2 != null)
          {
            childElement2.Visible = false;
          }

          if (childElement3 == null)
          {
            break;
          }

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
        return recentModelsTab;
      }
    }

    public RecentPrintsTab RecentPrints
    {
      get
      {
        return recentPrintsTab;
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
