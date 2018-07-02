// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.ListBoxWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class ListBoxWidget : Element2D
  {
    private ListBoxWidget.ScrollBarState _showScrollbar;
    private Color4 white;
    private Color4 imageHasFocusColor;
    private FontSize size;
    public List<object> Items;
    private int selected;
    private int leftbordersize_pixels;
    private int rightbordersize_pixels;
    private int topbordersize_pixels;
    private int bottombordersize_pixels;
    private int rowheight;
    private int scrollbar_width;
    private int text_region_height;
    private int text_region_width;
    private int max_rows_on_screen;
    private int start_row;
    private bool highlightmouseover;
    private int mouse_over_row;
    private Color4 color;
    private Color4 color_highlighted;
    private Color4 color_selected;
    private ImageWidget image_widget;
    private VerticalSliderWidget scrollbar;
    public bool NoSelect;
    private int item_count;
    private ListBoxWidget.OnChangeCallback onchangecallback;

    public ListBoxWidget()
      : this(0, (Element2D) null)
    {
    }

    public ListBoxWidget(int ID)
      : this(ID, (Element2D) null)
    {
      Size = FontSize.Medium;
    }

    public ListBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      Items = new List<object>();
      image_widget = new ImageWidget(0)
      {
        IgnoreMouse = true
      };
      scrollbar = new VerticalSliderWidget(1)
      {
        Visible = false,
        RoundingPlace = -1
      };
      white = new Color4(1f, 1f, 1f, 1f);
      imageHasFocusColor = white;
      leftbordersize_pixels = 0;
      rightbordersize_pixels = 0;
      topbordersize_pixels = 0;
      bottombordersize_pixels = 0;
      rowheight = 0;
      start_row = 0;
      item_count = 0;
      Selected = 0;
      highlightmouseover = false;
      mouse_over_row = -1;
      MouseOverHighlight = true;
      color = new Color4(0.15f, 0.15f, 0.15f, 1f);
      color_selected = new Color4(0.7f, 0.7f, 1f, 1f);
      color_highlighted = new Color4(1f, 1f, 0.5f, 1f);
      ChildList = ChildList + (Element2D)image_widget;
      ChildList = ChildList + (Element2D)scrollbar;
      onchangecallback = (ListBoxWidget.OnChangeCallback) null;
    }

    public override ElementType GetElementType()
    {
      return ElementType.ListBoxWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      Refresh();
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.ID == 1)
      {
        if (msg != ControlMsg.SCROLL_MOVE)
        {
          return;
        }

        start_row = (int) xparam;
        if (start_row >= 0)
        {
          return;
        }

        start_row = 0;
      }
      else
      {
        base.OnControlMsg(the_control, msg, xparam, yparam);
      }
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth, int topbordersize_pixels, int bottombordersize_pixels, int minimumheight, int rowheight, int scrollbar_width)
    {
      image_widget.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, normal_u0, normal_v0, normal_u1, normal_v1);
      image_widget.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      image_widget.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      this.leftbordersize_pixels = leftbordersize_pixels;
      this.rightbordersize_pixels = rightbordersize_pixels;
      this.topbordersize_pixels = topbordersize_pixels;
      this.bottombordersize_pixels = bottombordersize_pixels;
      this.rowheight = rowheight;
      this.scrollbar_width = scrollbar_width;
    }

    public void Init(GUIHost host)
    {
      Init(host, "guicontrols", 944f, 96f, 959f, 143f, 944f, 96f, 959f, 143f, 4, 4, 16, 4, 4, 48, 24, 24);
      ScrollBar.InitTrack(host, "guicontrols", 809f, 80f, 831f, 87f, 2, 8);
      ScrollBar.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      ScrollBar.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      ScrollBar.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      ScrollBar.SetButtonSize(24f);
      ScrollBar.ShowPushButtons = true;
      Refresh();
      Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      tabIndex = 1;
      ImageHasFocusColor = new Color4((byte) 100, (byte) 230, byte.MaxValue, byte.MaxValue);
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (Items.Count - Selected < max_rows_on_screen)
      {
        start_row = Items.Count - max_rows_on_screen;
        if (start_row < 0)
        {
          start_row = 0;
        }
      }
      else
      {
        start_row = Selected;
      }

      mouse_over_row = Selected;
    }

    public override void OnRender(GUIHost host)
    {
      if (item_count != Items.Count)
      {
        Refresh();
      }

      base.OnRender(host);
      var y = (float) (topbordersize_pixels + Y_Abs);
      var x = (float) (leftbordersize_pixels + X_Abs);
      host.SetCurFontSize(Size);
      QFont currentFont = host.GetCurrentFont();
      QFont.Begin();
      var num = 0;
      for (var startRow = start_row; startRow < Items.Count && num < max_rows_on_screen; ++startRow)
      {
        if (startRow == mouse_over_row && !NoSelect)
        {
          QFont.End();
          Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
          Simple2DRenderer.Quad quad1;
          quad1.x0 = x;
          quad1.y0 = y;
          quad1.x1 = x + (float)text_region_width;
          quad1.y1 = y + (float)rowheight * 0.75f;
          quad1.color = color_highlighted;
          Simple2DRenderer.Quad quad2 = quad1;
          simpleRenderer.DrawQuad(quad2);
          QFont.Begin();
        }
        if (startRow == selected && !NoSelect)
        {
          QFont.End();
          Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
          Simple2DRenderer.Quad quad1;
          quad1.x0 = x;
          quad1.y0 = y;
          quad1.x1 = x + (float)text_region_width;
          quad1.y1 = y + (float)rowheight * 0.75f;
          quad1.color = color_selected;
          Simple2DRenderer.Quad quad2 = quad1;
          simpleRenderer.DrawQuad(quad2);
          QFont.Begin();
        }
        currentFont.Options.Colour = Color;
        currentFont.Print(Items[startRow].ToString(), (float)text_region_width, new Vector2(x, y));
        y += (float)rowheight;
        ++num;
      }
      QFont.End();
    }

    public override void OnMouseMove(int x, int y)
    {
      if (Enabled && MouseOverHighlight)
      {
        var xAbs = X_Abs;
        var yAbs = Y_Abs;
        var num1 = xAbs + text_region_width + rightbordersize_pixels;
        var num2 = yAbs + text_region_height;
        if (x >= xAbs && x <= num1 && (y >= yAbs && y <= num2))
        {
          mouse_over_row = (y - yAbs) / rowheight + start_row;
        }
      }
      else
      {
        base.OnMouseMove(x, y);
      }

      scrollbar.OnMouseMove(x, y);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (Enabled && mouseevent.type == MouseEventType.Down && mouseevent.button == MouseButton.Left)
      {
        var xAbs = X_Abs;
        var yAbs = Y_Abs;
        var num1 = xAbs + text_region_width + rightbordersize_pixels;
        var num2 = yAbs + text_region_height;
        if (mouseevent.pos.x >= xAbs && mouseevent.pos.x <= num1 && (mouseevent.pos.y >= yAbs && mouseevent.pos.y <= num2))
        {
          var num3 = (mouseevent.pos.y - yAbs) / rowheight + start_row;
          if (num3 >= Items.Count)
          {
            num3 = -1;
          }

          if (num3 >= 0)
          {
            Selected = num3;
            if (onchangecallback != null)
            {
              onchangecallback(this);
            }
          }
        }
      }
      return false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!Enabled || !HasFocus)
      {
        return false;
      }

      if (keyboardevent.type == KeyboardEventType.CommandKey)
      {
        var commandKeyEvent = (CommandKeyEvent) keyboardevent;
        if (commandKeyEvent.Key == KeyboardCommandKey.Down)
        {
          if (mouse_over_row < Items.Count - 1)
          {
            ++mouse_over_row;
          }

          if (mouse_over_row - start_row >= max_rows_on_screen)
          {
            ScrollBar.MoveSlider(1f);
          }
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Up)
        {
          if (mouse_over_row > 0)
          {
            --mouse_over_row;
          }

          if (mouse_over_row < start_row)
          {
            ScrollBar.MoveSlider(-1f);
          }
        }
      }
      else if (keyboardevent.type == KeyboardEventType.InputKey && ((InputKeyEvent) keyboardevent).Ch == '\r')
      {
        Selected = mouse_over_row;
      }

      return base.OnKeyboardEvent(keyboardevent);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      Init(host);
      lock (ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
        {
          child.InitChildren((Element2D) this, host, MyButtonCallback);
        }
      }
    }

    public void SetOnChangeCallback(ListBoxWidget.OnChangeCallback func)
    {
      onchangecallback = func;
    }

    public VerticalSliderWidget ScrollBar
    {
      get
      {
        return scrollbar;
      }
    }

    public override void Refresh()
    {
      var flag = false;
      text_region_height = Height - (topbordersize_pixels + bottombordersize_pixels);
      text_region_width = Width - (leftbordersize_pixels + rightbordersize_pixels);
      max_rows_on_screen = rowheight > 0 ? (int) Math.Ceiling((double)text_region_height / (double)rowheight) : 0;
      item_count = Items.Count;
      if (ShowScrollbar != ListBoxWidget.ScrollBarState.Off && (max_rows_on_screen < item_count || ShowScrollbar == ListBoxWidget.ScrollBarState.On))
      {
        image_widget.SetPosition(0, 0);
        image_widget.SetSize(Width - scrollbar_width, Height);
        scrollbar.SetPosition(image_widget.Width, 0);
        scrollbar.SetSize(scrollbar_width, Height);
        text_region_width = image_widget.Width - (leftbordersize_pixels + rightbordersize_pixels);
        if (item_count > max_rows_on_screen)
        {
          scrollbar.Enabled = true;
          scrollbar.SetRange(0.0f, (float) (item_count - max_rows_on_screen), (float)Selected);
          scrollbar.PushButtonStep = 1f;
        }
        else
        {
          scrollbar.Enabled = true;
          scrollbar.SetRange(0.0f, 0.0f, (float)Selected);
        }
        scrollbar.Visible = true;
        flag = true;
      }
      if (flag)
      {
        return;
      }

      scrollbar.Enabled = false;
      scrollbar.Visible = false;
      image_widget.SetPosition(0, 0);
      image_widget.SetSize(Width, Height);
    }

    public void SetTrackPositionToEnd()
    {
      max_rows_on_screen = (int) Math.Ceiling((double)text_region_height / (double)rowheight);
      if (max_rows_on_screen < item_count)
      {
        scrollbar.SetTrackPositionToEnd();
      }
      else
      {
        scrollbar.SetTrackPosition(0.0f);
      }
    }

    [XmlAttribute("ShowScrollbar")]
    public ListBoxWidget.ScrollBarState ShowScrollbar
    {
      set
      {
        _showScrollbar = value;
        Refresh();
      }
      get
      {
        return _showScrollbar;
      }
    }

    public int Selected
    {
      get
      {
        return selected;
      }
      set
      {
        selected = value;
        base.OnControlMsg((Element2D) this, ControlMsg.SELECTCHANGED, (float)selected, (float)selected);
      }
    }

    public Color4 Color
    {
      get
      {
        return color;
      }
      set
      {
        color = value;
      }
    }

    public Color4 HighlightColor
    {
      get
      {
        return color_highlighted;
      }
      set
      {
        color_highlighted = value;
      }
    }

    public Color4 SelectedColor
    {
      get
      {
        return color_selected;
      }
      set
      {
        color_selected = value;
      }
    }

    public bool MouseOverHighlight
    {
      get
      {
        return highlightmouseover;
      }
      set
      {
        highlightmouseover = value;
      }
    }

    public override bool HasFocus
    {
      get
      {
        return base.HasFocus;
      }
      set
      {
        base.HasFocus = value;
        image_widget.HasFocus = value;
        if (value)
        {
          image_widget.state = State.Highlighted;
        }
        else
        {
          image_widget.state = State.Normal;
        }
      }
    }

    public Color4 ImageHasFocusColor
    {
      get
      {
        return imageHasFocusColor;
      }
      set
      {
        imageHasFocusColor = value;
      }
    }

    [XmlAttribute("font-size")]
    public FontSize Size
    {
      get
      {
        return size;
      }
      set
      {
        size = value;
      }
    }

    public delegate void OnChangeCallback(ListBoxWidget listBox);

    private enum ListBoxControlID
    {
      image_widget,
      vertical_scrollbar,
    }

    public enum ScrollBarState
    {
      On,
      Off,
      Auto,
    }
  }
}
