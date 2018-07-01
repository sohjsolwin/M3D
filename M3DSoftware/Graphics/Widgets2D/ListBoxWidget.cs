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
      this.Size = FontSize.Medium;
    }

    public ListBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.Items = new List<object>();
      this.image_widget = new ImageWidget(0);
      this.image_widget.IgnoreMouse = true;
      this.scrollbar = new VerticalSliderWidget(1);
      this.scrollbar.Visible = false;
      this.scrollbar.RoundingPlace = -1;
      this.white = new Color4(1f, 1f, 1f, 1f);
      this.imageHasFocusColor = this.white;
      this.leftbordersize_pixels = 0;
      this.rightbordersize_pixels = 0;
      this.topbordersize_pixels = 0;
      this.bottombordersize_pixels = 0;
      this.rowheight = 0;
      this.start_row = 0;
      this.item_count = 0;
      this.Selected = 0;
      this.highlightmouseover = false;
      this.mouse_over_row = -1;
      this.MouseOverHighlight = true;
      this.color = new Color4(0.15f, 0.15f, 0.15f, 1f);
      this.color_selected = new Color4(0.7f, 0.7f, 1f, 1f);
      this.color_highlighted = new Color4(1f, 1f, 0.5f, 1f);
      this.ChildList = this.ChildList + (Element2D) this.image_widget;
      this.ChildList = this.ChildList + (Element2D) this.scrollbar;
      this.onchangecallback = (ListBoxWidget.OnChangeCallback) null;
    }

    public override ElementType GetElementType()
    {
      return ElementType.ListBoxWidget;
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.Refresh();
    }

    public override void OnControlMsg(Element2D the_control, ControlMsg msg, float xparam, float yparam)
    {
      if (the_control.ID == 1)
      {
        if (msg != ControlMsg.SCROLL_MOVE)
          return;
        this.start_row = (int) xparam;
        if (this.start_row >= 0)
          return;
        this.start_row = 0;
      }
      else
        base.OnControlMsg(the_control, msg, xparam, yparam);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth, int topbordersize_pixels, int bottombordersize_pixels, int minimumheight, int rowheight, int scrollbar_width)
    {
      this.image_widget.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, normal_u0, normal_v0, normal_u1, normal_v1);
      this.image_widget.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      this.image_widget.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      this.leftbordersize_pixels = leftbordersize_pixels;
      this.rightbordersize_pixels = rightbordersize_pixels;
      this.topbordersize_pixels = topbordersize_pixels;
      this.bottombordersize_pixels = bottombordersize_pixels;
      this.rowheight = rowheight;
      this.scrollbar_width = scrollbar_width;
    }

    public void Init(GUIHost host)
    {
      this.Init(host, "guicontrols", 944f, 96f, 959f, 143f, 944f, 96f, 959f, 143f, 4, 4, 16, 4, 4, 48, 24, 24);
      this.ScrollBar.InitTrack(host, "guicontrols", 809f, 80f, 831f, 87f, 2, 8);
      this.ScrollBar.InitButton(host, "guicontrols", 1000f, 0.0f, 1023f, 23f, 1000f, 24f, 1023f, 47f, 1000f, 48f, 1023f, 71f, 4, 4, 24);
      this.ScrollBar.InitMinus(host, "guicontrols", 928f, 48f, 951f, 71f, 952f, 48f, 975f, 71f, 976f, 48f, 999f, 71f);
      this.ScrollBar.InitPlus(host, "guicontrols", 928f, 72f, 951f, 95f, 952f, 72f, 975f, 95f, 976f, 72f, 999f, 95f);
      this.ScrollBar.SetButtonSize(24f);
      this.ScrollBar.ShowPushButtons = true;
      this.Refresh();
      this.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.tabIndex = 1;
      this.ImageHasFocusColor = new Color4((byte) 100, (byte) 230, byte.MaxValue, byte.MaxValue);
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (this.Items.Count - this.Selected < this.max_rows_on_screen)
      {
        this.start_row = this.Items.Count - this.max_rows_on_screen;
        if (this.start_row < 0)
          this.start_row = 0;
      }
      else
        this.start_row = this.Selected;
      this.mouse_over_row = this.Selected;
    }

    public override void OnRender(GUIHost host)
    {
      if (this.item_count != this.Items.Count)
        this.Refresh();
      base.OnRender(host);
      float y = (float) (this.topbordersize_pixels + this.Y_Abs);
      float x = (float) (this.leftbordersize_pixels + this.X_Abs);
      host.SetCurFontSize(this.Size);
      QFont currentFont = host.GetCurrentFont();
      QFont.Begin();
      int num = 0;
      for (int startRow = this.start_row; startRow < this.Items.Count && num < this.max_rows_on_screen; ++startRow)
      {
        if (startRow == this.mouse_over_row && !this.NoSelect)
        {
          QFont.End();
          Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
          Simple2DRenderer.Quad quad1;
          quad1.x0 = x;
          quad1.y0 = y;
          quad1.x1 = x + (float) this.text_region_width;
          quad1.y1 = y + (float) this.rowheight * 0.75f;
          quad1.color = this.color_highlighted;
          Simple2DRenderer.Quad quad2 = quad1;
          simpleRenderer.DrawQuad(quad2);
          QFont.Begin();
        }
        if (startRow == this.selected && !this.NoSelect)
        {
          QFont.End();
          Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
          Simple2DRenderer.Quad quad1;
          quad1.x0 = x;
          quad1.y0 = y;
          quad1.x1 = x + (float) this.text_region_width;
          quad1.y1 = y + (float) this.rowheight * 0.75f;
          quad1.color = this.color_selected;
          Simple2DRenderer.Quad quad2 = quad1;
          simpleRenderer.DrawQuad(quad2);
          QFont.Begin();
        }
        currentFont.Options.Colour = this.Color;
        currentFont.Print(this.Items[startRow].ToString(), (float) this.text_region_width, new Vector2(x, y));
        y += (float) this.rowheight;
        ++num;
      }
      QFont.End();
    }

    public override void OnMouseMove(int x, int y)
    {
      if (this.Enabled && this.MouseOverHighlight)
      {
        int xAbs = this.X_Abs;
        int yAbs = this.Y_Abs;
        int num1 = xAbs + this.text_region_width + this.rightbordersize_pixels;
        int num2 = yAbs + this.text_region_height;
        if (x >= xAbs && x <= num1 && (y >= yAbs && y <= num2))
          this.mouse_over_row = (y - yAbs) / this.rowheight + this.start_row;
      }
      else
        base.OnMouseMove(x, y);
      this.scrollbar.OnMouseMove(x, y);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (this.Enabled && mouseevent.type == MouseEventType.Down && mouseevent.button == MouseButton.Left)
      {
        int xAbs = this.X_Abs;
        int yAbs = this.Y_Abs;
        int num1 = xAbs + this.text_region_width + this.rightbordersize_pixels;
        int num2 = yAbs + this.text_region_height;
        if (mouseevent.pos.x >= xAbs && mouseevent.pos.x <= num1 && (mouseevent.pos.y >= yAbs && mouseevent.pos.y <= num2))
        {
          int num3 = (mouseevent.pos.y - yAbs) / this.rowheight + this.start_row;
          if (num3 >= this.Items.Count)
            num3 = -1;
          if (num3 >= 0)
          {
            this.Selected = num3;
            if (this.onchangecallback != null)
              this.onchangecallback(this);
          }
        }
      }
      return false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!this.Enabled || !this.HasFocus)
        return false;
      if (keyboardevent.type == KeyboardEventType.CommandKey)
      {
        CommandKeyEvent commandKeyEvent = (CommandKeyEvent) keyboardevent;
        if (commandKeyEvent.Key == KeyboardCommandKey.Down)
        {
          if (this.mouse_over_row < this.Items.Count - 1)
            ++this.mouse_over_row;
          if (this.mouse_over_row - this.start_row >= this.max_rows_on_screen)
            this.ScrollBar.MoveSlider(1f);
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Up)
        {
          if (this.mouse_over_row > 0)
            --this.mouse_over_row;
          if (this.mouse_over_row < this.start_row)
            this.ScrollBar.MoveSlider(-1f);
        }
      }
      else if (keyboardevent.type == KeyboardEventType.InputKey && ((InputKeyEvent) keyboardevent).Ch == '\r')
        this.Selected = this.mouse_over_row;
      return base.OnKeyboardEvent(keyboardevent);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      this.Init(host);
      lock (this.ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
          child.InitChildren((Element2D) this, host, MyButtonCallback);
      }
    }

    public void SetOnChangeCallback(ListBoxWidget.OnChangeCallback func)
    {
      this.onchangecallback = func;
    }

    public VerticalSliderWidget ScrollBar
    {
      get
      {
        return this.scrollbar;
      }
    }

    public override void Refresh()
    {
      bool flag = false;
      this.text_region_height = this.Height - (this.topbordersize_pixels + this.bottombordersize_pixels);
      this.text_region_width = this.Width - (this.leftbordersize_pixels + this.rightbordersize_pixels);
      this.max_rows_on_screen = this.rowheight > 0 ? (int) Math.Ceiling((double) this.text_region_height / (double) this.rowheight) : 0;
      this.item_count = this.Items.Count;
      if (this.ShowScrollbar != ListBoxWidget.ScrollBarState.Off && (this.max_rows_on_screen < this.item_count || this.ShowScrollbar == ListBoxWidget.ScrollBarState.On))
      {
        this.image_widget.SetPosition(0, 0);
        this.image_widget.SetSize(this.Width - this.scrollbar_width, this.Height);
        this.scrollbar.SetPosition(this.image_widget.Width, 0);
        this.scrollbar.SetSize(this.scrollbar_width, this.Height);
        this.text_region_width = this.image_widget.Width - (this.leftbordersize_pixels + this.rightbordersize_pixels);
        if (this.item_count > this.max_rows_on_screen)
        {
          this.scrollbar.Enabled = true;
          this.scrollbar.SetRange(0.0f, (float) (this.item_count - this.max_rows_on_screen), (float) this.Selected);
          this.scrollbar.PushButtonStep = 1f;
        }
        else
        {
          this.scrollbar.Enabled = true;
          this.scrollbar.SetRange(0.0f, 0.0f, (float) this.Selected);
        }
        this.scrollbar.Visible = true;
        flag = true;
      }
      if (flag)
        return;
      this.scrollbar.Enabled = false;
      this.scrollbar.Visible = false;
      this.image_widget.SetPosition(0, 0);
      this.image_widget.SetSize(this.Width, this.Height);
    }

    public void SetTrackPositionToEnd()
    {
      this.max_rows_on_screen = (int) Math.Ceiling((double) this.text_region_height / (double) this.rowheight);
      if (this.max_rows_on_screen < this.item_count)
        this.scrollbar.SetTrackPositionToEnd();
      else
        this.scrollbar.SetTrackPosition(0.0f);
    }

    [XmlAttribute("ShowScrollbar")]
    public ListBoxWidget.ScrollBarState ShowScrollbar
    {
      set
      {
        this._showScrollbar = value;
        this.Refresh();
      }
      get
      {
        return this._showScrollbar;
      }
    }

    public int Selected
    {
      get
      {
        return this.selected;
      }
      set
      {
        this.selected = value;
        base.OnControlMsg((Element2D) this, ControlMsg.SELECTCHANGED, (float) this.selected, (float) this.selected);
      }
    }

    public Color4 Color
    {
      get
      {
        return this.color;
      }
      set
      {
        this.color = value;
      }
    }

    public Color4 HighlightColor
    {
      get
      {
        return this.color_highlighted;
      }
      set
      {
        this.color_highlighted = value;
      }
    }

    public Color4 SelectedColor
    {
      get
      {
        return this.color_selected;
      }
      set
      {
        this.color_selected = value;
      }
    }

    public bool MouseOverHighlight
    {
      get
      {
        return this.highlightmouseover;
      }
      set
      {
        this.highlightmouseover = value;
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
        this.image_widget.HasFocus = value;
        if (value)
          this.image_widget.state = State.Highlighted;
        else
          this.image_widget.state = State.Normal;
      }
    }

    public Color4 ImageHasFocusColor
    {
      get
      {
        return this.imageHasFocusColor;
      }
      set
      {
        this.imageHasFocusColor = value;
      }
    }

    [XmlAttribute("font-size")]
    public FontSize Size
    {
      get
      {
        return this.size;
      }
      set
      {
        this.size = value;
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
