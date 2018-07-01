// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.ButtonWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using QuickFont;
using System;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class ButtonWidget : ImageWidget, IDataAccessable
  {
    [XmlIgnore]
    private ButtonType type;
    private Color4 textcolor;
    private Color4 textovercolor;
    private Color4 imageHasFocusColor;
    private Color4 textdowncolor;
    private Color4 textdisabledcolor;
    private Color4 white;
    private string hexColor;
    private string hexFontOverColor;
    private string hexFontDownColor;
    private bool down;
    private bool dontmove;
    private bool _can_click_off;
    private bool _can_click_off_set;
    [XmlAttribute("disable-mouse")]
    public bool DisableMouseEvents;
    [XmlAttribute("template")]
    public ButtonTemplate type_template;
    public bool AlwaysHighlightOnMouseOver;
    private bool mouse_over;
    private int draggable_xmin;
    private int draggable_xmax;
    private int draggable_ymin;
    private int draggable_ymax;
    private bool draggable_fully;
    private bool dragged_fully;
    private int dragged_x_fully;
    private int dragged_y_fully;
    private int lastx;
    private int lasty;
    private ButtonCallback buttoncallback;
    public static ButtonCallback ButtonListenerHook;

    public ButtonWidget()
      : this(0, (Element2D) null)
    {
    }

    public ButtonWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public ButtonWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.textcolor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
      this.textovercolor = new Color4(0.3686275f, 0.6588235f, 0.6901961f, 1f);
      this.textdowncolor = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.textdisabledcolor = new Color4(0.7f, 0.7f, 0.7f, 1f);
      this.white = new Color4(1f, 1f, 1f, 1f);
      this.imageHasFocusColor = this.white;
      this.Color = this.textcolor;
      this.down = false;
      this.state = State.Normal;
      this.buttoncallback = (ButtonCallback) null;
      this.ClickType = ButtonType.Clickable;
      this._can_click_off = false;
      this.draggable_fully = false;
      this.dragged_fully = false;
      this.dragged_x_fully = 0;
      this.dragged_y_fully = 0;
      this.IgnoreMouse = false;
    }

    public override ElementType GetElementType()
    {
      return ElementType.ButtonWidget;
    }

    public void SetDraggable(int xmin, int xmax, int ymin, int ymax)
    {
      this.ClickType = ButtonType.Draggable;
      this.draggable_xmin = xmin;
      this.draggable_xmax = xmax;
      this.draggable_ymin = ymin;
      this.draggable_ymax = ymax;
    }

    public void SetFullyDraggable()
    {
      this.draggable_fully = true;
      this.dragged_fully = false;
    }

    public override void SetOff()
    {
      this.down = false;
      this.state = State.Normal;
    }

    [XmlAttribute("ischecked")]
    public bool Checked
    {
      set
      {
        this.SetChecked(value);
      }
      get
      {
        if (this.ClickType == ButtonType.Checkable)
          return this.state == State.Down;
        return false;
      }
    }

    public void SetChecked(bool bChecked)
    {
      if (this.ClickType == ButtonType.Checkable)
      {
        if (bChecked)
        {
          this.down = true;
          this.state = State.Down;
          if (this.Parent != null)
            this.Parent.TurnOffGroup(this.GroupID, (Element2D) this);
          this.DoButtonCallback(true);
        }
        else
        {
          this.SetOff();
          this.DoButtonCallback(true);
        }
      }
      else
      {
        if (!bChecked)
          return;
        this.DoButtonCallback(true);
      }
    }

    [XmlIgnore]
    public new object Value
    {
      get
      {
        return (object) this.Checked;
      }
      set
      {
        if (value is bool)
          this.Checked = (bool) value;
        else if (value is int)
        {
          this.Checked = (int) value > 0;
        }
        else
        {
          if (!(value is string))
            throw new Exception("ButtonWidget.Value cannot have be set to a non-bool/int value");
          string lowerInvariant = ((string) value).ToLowerInvariant();
          if (!(lowerInvariant == "true") && !(lowerInvariant == "1"))
          {
            if (!(lowerInvariant == "false") && !(lowerInvariant == "0"))
              throw new Exception("ButtonWidget.Value cannot be set to a string that is not equal to true,false,1,0");
            this.Checked = false;
          }
          else
            this.Checked = true;
        }
      }
    }

    public void SetCallback(ButtonCallback func)
    {
      this.buttoncallback = func;
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void OnMouseLeave()
    {
      if (this.DisableMouseEvents)
        return;
      if (this.dragged_fully && this.draggable_fully)
      {
        float xparam = (float) (this.X_Abs - this.X + this.dragged_x_fully);
        float yparam = (float) (this.Y_Abs - this.Y + this.dragged_y_fully);
        this.dragged_fully = false;
        this.down = false;
        this.state = State.Normal;
        this.OnControlMsg((Element2D) this, ControlMsg.MSG_DRAGSTOP, xparam, yparam);
      }
      else if (this.ClickType == ButtonType.Draggable && this.down)
      {
        this.down = false;
        this.state = State.Normal;
        this.OnControlMsg((Element2D) this, ControlMsg.MSG_DRAGSTOP, 0.0f, 0.0f);
      }
      if (this.ClickType != ButtonType.Checkable)
        this.down = false;
      this.mouse_over = false;
      base.OnMouseLeave();
    }

    public override void OnMouseMove(int x, int y)
    {
      if (this.DisableMouseEvents || !this.Enabled)
        return;
      if (this.draggable_fully && this.down)
      {
        int num1 = x - this.lastx;
        int num2 = y - this.lasty;
        if (num1 != 0 || num2 != 0)
        {
          this.dragged_x_fully += num1;
          this.dragged_y_fully += num2;
          this.OnControlMsg((Element2D) this, ControlMsg.MSG_MOVE, (float) this.dragged_x_fully, (float) this.dragged_y_fully);
          this.lastx = x;
          this.lasty = y;
          this.dragged_fully = true;
        }
      }
      else if (this.ClickType == ButtonType.Draggable && this.down)
      {
        this.lastx = x;
        this.lasty = y;
        int num1 = x - (this.X_Abs + this.Width / 2);
        int num2 = y - (this.Y_Abs + this.Height / 2);
        int x1 = this.X + num1;
        int y1 = this.Y + num2;
        if (x1 < this.draggable_xmin)
          x1 = this.draggable_xmin;
        if (y1 < this.draggable_ymin)
          y1 = this.draggable_ymin;
        if (x1 > this.draggable_xmax)
          x1 = this.draggable_xmax;
        if (y1 > this.draggable_ymax)
          y1 = this.draggable_ymax;
        this.OnControlMsg((Element2D) this, ControlMsg.MSG_MOVE, (float) x1, (float) y1);
        this.SetPosition(x1, y1);
      }
      else if (!this.ContainsPoint(x, y) && !this.down)
        this.state = State.Normal;
      base.OnMouseMove(x, y);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (this.DisableMouseEvents || !this.Enabled)
        return false;
      if (this.ClickType != ButtonType.Checkable)
        this.state = State.Normal;
      if (mouseevent.type == MouseEventType.Down)
      {
        if (this.ContainsPoint(mouseevent.pos.x, mouseevent.pos.y))
        {
          this.lastx = mouseevent.pos.x;
          this.lasty = mouseevent.pos.y;
          this.dragged_x_fully = this.X;
          this.dragged_y_fully = this.Y;
          this.down = this.ClickType != ButtonType.Checkable || !this.CanClickOff || !this.down;
          this.dragged_fully = false;
          this.state = State.Down;
          if (this.ClickType == ButtonType.Checkable)
            this.SetChecked(this.down);
          return true;
        }
        if (this.ClickType != ButtonType.Checkable)
          this.down = false;
      }
      else if (mouseevent.type == MouseEventType.Up)
      {
        if (mouseevent.button == MouseButton.Left && this.down && (this.ContainsPoint(mouseevent.pos.x, mouseevent.pos.y) && this.ClickType == ButtonType.Clickable) && !this.dragged_fully)
        {
          this.DoButtonCallback(false);
          this.OnControlMsg((Element2D) this, ControlMsg.MSG_HIT, 0.0f, 0.0f);
          this.Color = this.textcolor;
          return true;
        }
      }
      else if (this.ContainsPoint(mouseevent.pos.x, mouseevent.pos.y))
      {
        this.mouse_over = true;
        if (this.ClickType != ButtonType.Checkable || !this.down)
        {
          this.Color = this.textovercolor;
          this.state = State.Highlighted;
        }
      }
      else
      {
        if (this.ClickType != ButtonType.Checkable || this.AlwaysHighlightOnMouseOver)
          this.Color = this.textcolor;
        this.mouse_over = false;
      }
      return false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!this.Enabled || !this.HasFocus)
        return false;
      if (keyboardevent.type == KeyboardEventType.InputKey && ((InputKeyEvent) keyboardevent).Ch == '\r')
      {
        this.down = this.ClickType != ButtonType.Checkable || !this.CanClickOff || !this.down;
        this.dragged_fully = false;
        this.state = State.Down;
        if (this.ClickType == ButtonType.Checkable)
          this.SetChecked(this.down);
        this.DoButtonCallback(false);
        this.OnControlMsg((Element2D) this, ControlMsg.MSG_HIT, 0.0f, 0.0f);
        this.Color = this.textcolor;
      }
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      if (this.Enabled)
      {
        switch (this.state)
        {
          case State.Normal:
            this.TextElementColor = this.textcolor;
            this.off_x = 0;
            this.off_y = 0;
            break;
          case State.Highlighted:
            this.TextElementColor = this.textovercolor;
            if (!this.dontmove)
            {
              this.off_x = 0;
              this.off_y = 0;
              break;
            }
            break;
          case State.Down:
            this.TextElementColor = !this.mouse_over || !this.AlwaysHighlightOnMouseOver ? this.textdowncolor : this.textovercolor;
            if (!this.dontmove)
            {
              this.off_x = 2;
              this.off_y = 2;
              break;
            }
            break;
        }
        if (this.state != State.Highlighted && this.Flashing && ImageWidget.FlashOn)
          this.TextElementColor = this.textovercolor;
      }
      else
      {
        this.TextElementColor = this.textdisabledcolor;
        this.off_x = 0;
        this.off_y = 0;
      }
      if (this.ClickType == ButtonType.Checkable)
      {
        if (this.HasFocus)
          this.ImageColor = this.imageHasFocusColor;
        else
          this.ImageColor = this.white;
      }
      else
      {
        int num = this.HasFocus ? 1 : 0;
      }
      if (!this.Enabled && this.FadeWhenDisabled)
      {
        Color4 imageColor = this.ImageColor;
        imageColor.A = 0.5f;
        this.ImageColor = imageColor;
      }
      if (this.Checked && !this.Enabled)
        base.OnRender(host);
      else
        base.OnRender(host);
      if (!this.dragged_fully || !this.draggable_fully)
        return;
      int x = this.X;
      int y = this.Y;
      bool wrapOnNegativeX = this.WrapOnNegativeX;
      bool wrapOnNegativeY = this.WrapOnNegativeY;
      this.WrapOnNegativeX = false;
      this.WrapOnNegativeY = false;
      this.X = this.dragged_x_fully;
      this.Y = this.dragged_y_fully;
      base.OnRender(host);
      this.WrapOnNegativeX = wrapOnNegativeX;
      this.WrapOnNegativeY = wrapOnNegativeY;
      this.X = x;
      this.Y = y;
    }

    public void Init(GUIHost host, ButtonTemplate color_template)
    {
      this.SetToDefaultOptions(color_template);
      this.Init(host, this.ImageSrc, this.u0, this.v0, this.u1, this.v1, this.over_u0, this.over_v0, this.over_u1, this.over_v1, this.down_u0, this.down_v0, this.down_u1, this.down_v1, this.disabled_u0, this.disabled_v0, this.disabled_u1, this.disabled_v1);
      this.SetGrowableWidth(this.leftbordersize_pixels, this.rightbordersize_pixels, this.minimum_width_pixels);
      this.SetGrowableHeight(this.topbordersize_pixels, this.bottombordersize_pixels, this.minimum_height_pixels);
    }

    private void SetToDefaultOptions(ButtonTemplate type_template)
    {
      switch (type_template)
      {
        case ButtonTemplate.Gray:
          this.ImageSrc = "extendedcontrols";
          this.u0 = 0.0f;
          this.v0 = 928f;
          this.u1 = 63f;
          this.v1 = 975f;
          this.down_u0 = 64f;
          this.down_v0 = 928f;
          this.down_u1 = (float) sbyte.MaxValue;
          this.down_v1 = 975f;
          this.over_u0 = 128f;
          this.over_v0 = 928f;
          this.over_u1 = 191f;
          this.over_v1 = 975f;
          this.disabled_u0 = 0.0f;
          this.disabled_v0 = 928f;
          this.disabled_u1 = 63f;
          this.disabled_v1 = 975f;
          this.leftbordersize_pixels = 9;
          this.rightbordersize_pixels = 9;
          this.minimum_width_pixels = 26;
          this.topbordersize_pixels = 9;
          this.bottombordersize_pixels = 9;
          this.minimum_height_pixels = 24;
          break;
        case ButtonTemplate.Blue:
          this.ImageSrc = "extendedcontrols";
          this.u0 = 0.0f;
          this.v0 = 976f;
          this.u1 = 63f;
          this.v1 = 1023f;
          this.down_u0 = 64f;
          this.down_v0 = 976f;
          this.down_u1 = (float) sbyte.MaxValue;
          this.down_v1 = 1023f;
          this.over_u0 = 128f;
          this.over_v0 = 976f;
          this.over_u1 = 191f;
          this.over_v1 = 1023f;
          this.disabled_u0 = 0.0f;
          this.disabled_v0 = 928f;
          this.disabled_u1 = 63f;
          this.disabled_v1 = 975f;
          this.leftbordersize_pixels = 9;
          this.rightbordersize_pixels = 9;
          this.minimum_width_pixels = 26;
          this.topbordersize_pixels = 9;
          this.bottombordersize_pixels = 9;
          this.minimum_height_pixels = 24;
          break;
        case ButtonTemplate.CheckBox:
          this.ImageSrc = "guicontrols";
          this.u0 = 640f;
          this.v0 = 448f;
          this.u1 = 671f;
          this.v1 = 479f;
          this.down_u0 = 640f;
          this.down_v0 = 480f;
          this.down_u1 = 671f;
          this.down_v1 = 511f;
          this.over_u0 = 672f;
          this.over_v0 = 448f;
          this.over_u1 = 703f;
          this.over_v1 = 479f;
          this.disabled_u0 = 672f;
          this.disabled_v0 = 480f;
          this.disabled_u1 = 703f;
          this.disabled_v1 = 511f;
          this.DontMove = true;
          this.ClickType = ButtonType.Checkable;
          if (!this._can_click_off_set)
            this.CanClickOff = true;
          this.SetGrowableWidth(9, 9, 26);
          this.SetGrowableHeight(9, 9, 24);
          break;
        case ButtonTemplate.MenuItem:
          this.ImageSrc = "extendedcontrols";
          this.u0 = 961f;
          this.v0 = 65f;
          this.u1 = 1022f;
          this.v1 = (float) sbyte.MaxValue;
          this.down_u0 = 897f;
          this.down_v0 = 65f;
          this.down_u1 = 959f;
          this.down_v1 = (float) sbyte.MaxValue;
          this.over_u0 = 897f;
          this.over_v0 = 65f;
          this.over_u1 = 959f;
          this.over_v1 = (float) sbyte.MaxValue;
          this.disabled_u0 = 672f;
          this.disabled_v0 = 480f;
          this.disabled_u1 = 703f;
          this.disabled_v1 = 511f;
          this.DontMove = true;
          this.ClickType = ButtonType.Clickable;
          this.CanClickOff = false;
          this.leftbordersize_pixels = 4;
          this.rightbordersize_pixels = 4;
          this.minimum_width_pixels = 16;
          this.topbordersize_pixels = 4;
          this.bottombordersize_pixels = 4;
          this.minimum_height_pixels = 16;
          this.VAlignment = TextVerticalAlignment.Middle;
          this.Alignment = QFontAlignment.Left;
          break;
        case ButtonTemplate.TextOnly:
        case ButtonTemplate.TextOnlyWhite:
        case ButtonTemplate.TextOnlyBlue:
          this.ImageSrc = "guicontrols";
          this.u0 = 200f;
          this.v0 = 705f;
          this.u1 = 220f;
          this.v1 = 725f;
          this.down_u0 = 200f;
          this.down_v0 = 705f;
          this.down_u1 = 220f;
          this.down_v1 = 725f;
          this.over_u0 = 200f;
          this.over_v0 = 705f;
          this.over_u1 = 220f;
          this.over_v1 = 725f;
          this.disabled_u0 = 200f;
          this.disabled_v0 = 705f;
          this.disabled_u1 = 220f;
          this.disabled_v1 = 725f;
          if (type_template == ButtonTemplate.TextOnlyWhite)
          {
            this.TextColor = new Color4(1f, 1f, 1f, 1f);
            break;
          }
          if (type_template != ButtonTemplate.TextOnlyBlue)
            break;
          this.TextColor = new Color4(0.3529412f, 0.7450981f, 0.8627451f, 1f);
          this.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
          this.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
          break;
        case ButtonTemplate.GrayCheckable:
          this.ImageSrc = "extendedcontrols";
          this.u0 = 0.0f;
          this.v0 = 928f;
          this.u1 = 63f;
          this.v1 = 975f;
          this.down_u0 = 64f;
          this.down_v0 = 928f;
          this.down_u1 = (float) sbyte.MaxValue;
          this.down_v1 = 975f;
          this.over_u0 = 128f;
          this.over_v0 = 928f;
          this.over_u1 = 191f;
          this.over_v1 = 975f;
          this.disabled_u0 = 0.0f;
          this.disabled_v0 = 928f;
          this.disabled_u1 = 63f;
          this.disabled_v1 = 975f;
          this.leftbordersize_pixels = 9;
          this.rightbordersize_pixels = 9;
          this.minimum_width_pixels = 26;
          this.topbordersize_pixels = 9;
          this.bottombordersize_pixels = 9;
          this.minimum_height_pixels = 24;
          this.DontMove = true;
          this.ClickType = ButtonType.Checkable;
          if (this._can_click_off_set)
            break;
          this.CanClickOff = true;
          break;
      }
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      if (!parent.IsComboBoxElement() && !parent.IsListBoxElement() && ((double) this.u0 == 0.0 && (double) this.v0 == 0.0) && ((double) this.u1 == 0.0 && (double) this.v1 == 0.0))
        this.SetToDefaultOptions(this.type_template);
      base.InitChildren(parent, host, MyButtonCallback);
      if (parent.GetElementType() != ElementType.ComboBoxWidget)
        this.SetCallback(MyButtonCallback);
      this.SetGrowableWidth(16, 16, 48);
    }

    private void DoButtonCallback(bool wasChecked)
    {
      if (this.buttoncallback != null)
        this.buttoncallback(this);
      if (ButtonWidget.ButtonListenerHook == null)
        return;
      ButtonWidget.ButtonListenerHook(this);
    }

    [XmlAttribute("click-type")]
    public ButtonType ClickType
    {
      get
      {
        return this.type;
      }
      set
      {
        this.type = value;
      }
    }

    [XmlAttribute("font-color")]
    public new string HexColor
    {
      get
      {
        return this.hexColor;
      }
      set
      {
        this.hexColor = value;
        this.TextColor = IElement.GenerateColorFromHtml(value);
      }
    }

    public Color4 TextColor
    {
      get
      {
        return this.textcolor;
      }
      set
      {
        this.Color = value;
        this.textcolor = value;
      }
    }

    private Color4 TextElementColor
    {
      get
      {
        return this.Color;
      }
      set
      {
        this.Color = value;
      }
    }

    [XmlAttribute("font-over-color")]
    public string HexFontOverColor
    {
      get
      {
        return this.hexFontOverColor;
      }
      set
      {
        this.hexFontOverColor = value;
        this.TextOverColor = IElement.GenerateColorFromHtml(this.hexFontOverColor);
      }
    }

    public Color4 TextOverColor
    {
      get
      {
        return this.textovercolor;
      }
      set
      {
        this.textovercolor = value;
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

    [XmlAttribute("font-down-color")]
    public string HexfontdownColor
    {
      get
      {
        return this.hexFontDownColor;
      }
      set
      {
        this.hexFontDownColor = value;
        this.TextDownColor = IElement.GenerateColorFromHtml(this.hexFontDownColor);
      }
    }

    public Color4 TextDownColor
    {
      get
      {
        return this.textdowncolor;
      }
      set
      {
        this.textdowncolor = value;
      }
    }

    public Color4 TextDisabledColor
    {
      get
      {
        return this.textdisabledcolor;
      }
      set
      {
        this.textdisabledcolor = value;
      }
    }

    [XmlAttribute("dont-move")]
    public bool DontMove
    {
      get
      {
        return this.dontmove;
      }
      set
      {
        this.dontmove = value;
      }
    }

    private void StopDragging()
    {
      this.down = false;
      this.dragged_fully = false;
    }

    [XmlAttribute("can-click-off")]
    public bool CanClickOff
    {
      set
      {
        this._can_click_off = value;
        this._can_click_off_set = true;
      }
      get
      {
        return this._can_click_off;
      }
    }
  }
}
