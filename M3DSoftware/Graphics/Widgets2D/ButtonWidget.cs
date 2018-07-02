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
      : this(0, null)
    {
    }

    public ButtonWidget(int ID)
      : this(ID, null)
    {
    }

    public ButtonWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      textcolor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
      textovercolor = new Color4(0.3686275f, 0.6588235f, 0.6901961f, 1f);
      textdowncolor = new Color4(0.25f, 0.25f, 0.25f, 1f);
      textdisabledcolor = new Color4(0.7f, 0.7f, 0.7f, 1f);
      white = new Color4(1f, 1f, 1f, 1f);
      imageHasFocusColor = white;
      Color = textcolor;
      down = false;
      state = State.Normal;
      buttoncallback = null;
      ClickType = ButtonType.Clickable;
      _can_click_off = false;
      draggable_fully = false;
      dragged_fully = false;
      dragged_x_fully = 0;
      dragged_y_fully = 0;
      IgnoreMouse = false;
    }

    public override ElementType GetElementType()
    {
      return ElementType.ButtonWidget;
    }

    public void SetDraggable(int xmin, int xmax, int ymin, int ymax)
    {
      ClickType = ButtonType.Draggable;
      draggable_xmin = xmin;
      draggable_xmax = xmax;
      draggable_ymin = ymin;
      draggable_ymax = ymax;
    }

    public void SetFullyDraggable()
    {
      draggable_fully = true;
      dragged_fully = false;
    }

    public override void SetOff()
    {
      down = false;
      state = State.Normal;
    }

    [XmlAttribute("ischecked")]
    public bool Checked
    {
      set
      {
        SetChecked(value);
      }
      get
      {
        if (ClickType == ButtonType.Checkable)
        {
          return state == State.Down;
        }

        return false;
      }
    }

    public void SetChecked(bool bChecked)
    {
      if (ClickType == ButtonType.Checkable)
      {
        if (bChecked)
        {
          down = true;
          state = State.Down;
          if (Parent != null)
          {
            Parent.TurnOffGroup(GroupID, this);
          }

          DoButtonCallback(true);
        }
        else
        {
          SetOff();
          DoButtonCallback(true);
        }
      }
      else
      {
        if (!bChecked)
        {
          return;
        }

        DoButtonCallback(true);
      }
    }

    [XmlIgnore]
    public new object Value
    {
      get
      {
        return Checked;
      }
      set
      {
        if (value is bool)
        {
          Checked = (bool) value;
        }
        else if (value is int)
        {
          Checked = (int) value > 0;
        }
        else
        {
          if (!(value is string))
          {
            throw new Exception("ButtonWidget.Value cannot have be set to a non-bool/int value");
          }

          var lowerInvariant = ((string) value).ToLowerInvariant();
          if (!(lowerInvariant == "true") && !(lowerInvariant == "1"))
          {
            if (!(lowerInvariant == "false") && !(lowerInvariant == "0"))
            {
              throw new Exception("ButtonWidget.Value cannot be set to a string that is not equal to true,false,1,0");
            }

            Checked = false;
          }
          else
          {
            Checked = true;
          }
        }
      }
    }

    public void SetCallback(ButtonCallback func)
    {
      buttoncallback = func;
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    public override void OnMouseLeave()
    {
      if (DisableMouseEvents)
      {
        return;
      }

      if (dragged_fully && draggable_fully)
      {
        var xparam = (float) (X_Abs - X + dragged_x_fully);
        var yparam = (float) (Y_Abs - Y + dragged_y_fully);
        dragged_fully = false;
        down = false;
        state = State.Normal;
        OnControlMsg(this, ControlMsg.MSG_DRAGSTOP, xparam, yparam);
      }
      else if (ClickType == ButtonType.Draggable && down)
      {
        down = false;
        state = State.Normal;
        OnControlMsg(this, ControlMsg.MSG_DRAGSTOP, 0.0f, 0.0f);
      }
      if (ClickType != ButtonType.Checkable)
      {
        down = false;
      }

      mouse_over = false;
      base.OnMouseLeave();
    }

    public override void OnMouseMove(int x, int y)
    {
      if (DisableMouseEvents || !Enabled)
      {
        return;
      }

      if (draggable_fully && down)
      {
        var num1 = x - lastx;
        var num2 = y - lasty;
        if (num1 != 0 || num2 != 0)
        {
          dragged_x_fully += num1;
          dragged_y_fully += num2;
          OnControlMsg(this, ControlMsg.MSG_MOVE, dragged_x_fully, dragged_y_fully);
          lastx = x;
          lasty = y;
          dragged_fully = true;
        }
      }
      else if (ClickType == ButtonType.Draggable && down)
      {
        lastx = x;
        lasty = y;
        var num1 = x - (X_Abs + Width / 2);
        var num2 = y - (Y_Abs + Height / 2);
        var x1 = X + num1;
        var y1 = Y + num2;
        if (x1 < draggable_xmin)
        {
          x1 = draggable_xmin;
        }

        if (y1 < draggable_ymin)
        {
          y1 = draggable_ymin;
        }

        if (x1 > draggable_xmax)
        {
          x1 = draggable_xmax;
        }

        if (y1 > draggable_ymax)
        {
          y1 = draggable_ymax;
        }

        OnControlMsg(this, ControlMsg.MSG_MOVE, x1, y1);
        SetPosition(x1, y1);
      }
      else if (!ContainsPoint(x, y) && !down)
      {
        state = State.Normal;
      }

      base.OnMouseMove(x, y);
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (DisableMouseEvents || !Enabled)
      {
        return false;
      }

      if (ClickType != ButtonType.Checkable)
      {
        state = State.Normal;
      }

      if (mouseevent.type == MouseEventType.Down)
      {
        if (ContainsPoint(mouseevent.pos.x, mouseevent.pos.y))
        {
          lastx = mouseevent.pos.x;
          lasty = mouseevent.pos.y;
          dragged_x_fully = X;
          dragged_y_fully = Y;
          down = ClickType != ButtonType.Checkable || !CanClickOff || !down;
          dragged_fully = false;
          state = State.Down;
          if (ClickType == ButtonType.Checkable)
          {
            SetChecked(down);
          }

          return true;
        }
        if (ClickType != ButtonType.Checkable)
        {
          down = false;
        }
      }
      else if (mouseevent.type == MouseEventType.Up)
      {
        if (mouseevent.button == MouseButton.Left && down && (ContainsPoint(mouseevent.pos.x, mouseevent.pos.y) && ClickType == ButtonType.Clickable) && !dragged_fully)
        {
          DoButtonCallback(false);
          OnControlMsg(this, ControlMsg.MSG_HIT, 0.0f, 0.0f);
          Color = textcolor;
          return true;
        }
      }
      else if (ContainsPoint(mouseevent.pos.x, mouseevent.pos.y))
      {
        mouse_over = true;
        if (ClickType != ButtonType.Checkable || !down)
        {
          Color = textovercolor;
          state = State.Highlighted;
        }
      }
      else
      {
        if (ClickType != ButtonType.Checkable || AlwaysHighlightOnMouseOver)
        {
          Color = textcolor;
        }

        mouse_over = false;
      }
      return false;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!Enabled || !HasFocus)
      {
        return false;
      }

      if (keyboardevent.type == KeyboardEventType.InputKey && ((InputKeyEvent) keyboardevent).Ch == '\r')
      {
        down = ClickType != ButtonType.Checkable || !CanClickOff || !down;
        dragged_fully = false;
        state = State.Down;
        if (ClickType == ButtonType.Checkable)
        {
          SetChecked(down);
        }

        DoButtonCallback(false);
        OnControlMsg(this, ControlMsg.MSG_HIT, 0.0f, 0.0f);
        Color = textcolor;
      }
      return true;
    }

    public override void OnRender(GUIHost host)
    {
      if (Enabled)
      {
        switch (state)
        {
          case State.Normal:
            TextElementColor = textcolor;
            off_x = 0;
            off_y = 0;
            break;
          case State.Highlighted:
            TextElementColor = textovercolor;
            if (!dontmove)
            {
              off_x = 0;
              off_y = 0;
              break;
            }
            break;
          case State.Down:
            TextElementColor = !mouse_over || !AlwaysHighlightOnMouseOver ? textdowncolor : textovercolor;
            if (!dontmove)
            {
              off_x = 2;
              off_y = 2;
              break;
            }
            break;
        }
        if (state != State.Highlighted && Flashing && ImageWidget.FlashOn)
        {
          TextElementColor = textovercolor;
        }
      }
      else
      {
        TextElementColor = textdisabledcolor;
        off_x = 0;
        off_y = 0;
      }
      if (ClickType == ButtonType.Checkable)
      {
        if (HasFocus)
        {
          ImageColor = imageHasFocusColor;
        }
        else
        {
          ImageColor = white;
        }
      }
      else
      {
        var num = HasFocus ? 1 : 0;
      }
      if (!Enabled && FadeWhenDisabled)
      {
        Color4 imageColor = ImageColor;
        imageColor.A = 0.5f;
        ImageColor = imageColor;
      }
      if (Checked && !Enabled)
      {
        base.OnRender(host);
      }
      else
      {
        base.OnRender(host);
      }

      if (!dragged_fully || !draggable_fully)
      {
        return;
      }

      var x = X;
      var y = Y;
      var wrapOnNegativeX = WrapOnNegativeX;
      var wrapOnNegativeY = WrapOnNegativeY;
      WrapOnNegativeX = false;
      WrapOnNegativeY = false;
      X = dragged_x_fully;
      Y = dragged_y_fully;
      base.OnRender(host);
      WrapOnNegativeX = wrapOnNegativeX;
      WrapOnNegativeY = wrapOnNegativeY;
      X = x;
      Y = y;
    }

    public void Init(GUIHost host, ButtonTemplate color_template)
    {
      SetToDefaultOptions(color_template);
      Init(host, ImageSrc, u0, v0, u1, v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, disabled_u0, disabled_v0, disabled_u1, disabled_v1);
      SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimum_width_pixels);
      SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimum_height_pixels);
    }

    private void SetToDefaultOptions(ButtonTemplate type_template)
    {
      switch (type_template)
      {
        case ButtonTemplate.Gray:
          ImageSrc = "extendedcontrols";
          u0 = 0.0f;
          v0 = 928f;
          u1 = 63f;
          v1 = 975f;
          down_u0 = 64f;
          down_v0 = 928f;
          down_u1 = sbyte.MaxValue;
          down_v1 = 975f;
          over_u0 = 128f;
          over_v0 = 928f;
          over_u1 = 191f;
          over_v1 = 975f;
          disabled_u0 = 0.0f;
          disabled_v0 = 928f;
          disabled_u1 = 63f;
          disabled_v1 = 975f;
          leftbordersize_pixels = 9;
          rightbordersize_pixels = 9;
          minimum_width_pixels = 26;
          topbordersize_pixels = 9;
          bottombordersize_pixels = 9;
          minimum_height_pixels = 24;
          break;
        case ButtonTemplate.Blue:
          ImageSrc = "extendedcontrols";
          u0 = 0.0f;
          v0 = 976f;
          u1 = 63f;
          v1 = 1023f;
          down_u0 = 64f;
          down_v0 = 976f;
          down_u1 = sbyte.MaxValue;
          down_v1 = 1023f;
          over_u0 = 128f;
          over_v0 = 976f;
          over_u1 = 191f;
          over_v1 = 1023f;
          disabled_u0 = 0.0f;
          disabled_v0 = 928f;
          disabled_u1 = 63f;
          disabled_v1 = 975f;
          leftbordersize_pixels = 9;
          rightbordersize_pixels = 9;
          minimum_width_pixels = 26;
          topbordersize_pixels = 9;
          bottombordersize_pixels = 9;
          minimum_height_pixels = 24;
          break;
        case ButtonTemplate.CheckBox:
          ImageSrc = "guicontrols";
          u0 = 640f;
          v0 = 448f;
          u1 = 671f;
          v1 = 479f;
          down_u0 = 640f;
          down_v0 = 480f;
          down_u1 = 671f;
          down_v1 = 511f;
          over_u0 = 672f;
          over_v0 = 448f;
          over_u1 = 703f;
          over_v1 = 479f;
          disabled_u0 = 672f;
          disabled_v0 = 480f;
          disabled_u1 = 703f;
          disabled_v1 = 511f;
          DontMove = true;
          ClickType = ButtonType.Checkable;
          if (!_can_click_off_set)
          {
            CanClickOff = true;
          }

          SetGrowableWidth(9, 9, 26);
          SetGrowableHeight(9, 9, 24);
          break;
        case ButtonTemplate.MenuItem:
          ImageSrc = "extendedcontrols";
          u0 = 961f;
          v0 = 65f;
          u1 = 1022f;
          v1 = sbyte.MaxValue;
          down_u0 = 897f;
          down_v0 = 65f;
          down_u1 = 959f;
          down_v1 = sbyte.MaxValue;
          over_u0 = 897f;
          over_v0 = 65f;
          over_u1 = 959f;
          over_v1 = sbyte.MaxValue;
          disabled_u0 = 672f;
          disabled_v0 = 480f;
          disabled_u1 = 703f;
          disabled_v1 = 511f;
          DontMove = true;
          ClickType = ButtonType.Clickable;
          CanClickOff = false;
          leftbordersize_pixels = 4;
          rightbordersize_pixels = 4;
          minimum_width_pixels = 16;
          topbordersize_pixels = 4;
          bottombordersize_pixels = 4;
          minimum_height_pixels = 16;
          VAlignment = TextVerticalAlignment.Middle;
          Alignment = QFontAlignment.Left;
          break;
        case ButtonTemplate.TextOnly:
        case ButtonTemplate.TextOnlyWhite:
        case ButtonTemplate.TextOnlyBlue:
          ImageSrc = "guicontrols";
          u0 = 200f;
          v0 = 705f;
          u1 = 220f;
          v1 = 725f;
          down_u0 = 200f;
          down_v0 = 705f;
          down_u1 = 220f;
          down_v1 = 725f;
          over_u0 = 200f;
          over_v0 = 705f;
          over_u1 = 220f;
          over_v1 = 725f;
          disabled_u0 = 200f;
          disabled_v0 = 705f;
          disabled_u1 = 220f;
          disabled_v1 = 725f;
          if (type_template == ButtonTemplate.TextOnlyWhite)
          {
            TextColor = new Color4(1f, 1f, 1f, 1f);
            break;
          }
          if (type_template != ButtonTemplate.TextOnlyBlue)
          {
            break;
          }

          TextColor = new Color4(0.3529412f, 0.7450981f, 0.8627451f, 1f);
          TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
          TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
          break;
        case ButtonTemplate.GrayCheckable:
          ImageSrc = "extendedcontrols";
          u0 = 0.0f;
          v0 = 928f;
          u1 = 63f;
          v1 = 975f;
          down_u0 = 64f;
          down_v0 = 928f;
          down_u1 = sbyte.MaxValue;
          down_v1 = 975f;
          over_u0 = 128f;
          over_v0 = 928f;
          over_u1 = 191f;
          over_v1 = 975f;
          disabled_u0 = 0.0f;
          disabled_v0 = 928f;
          disabled_u1 = 63f;
          disabled_v1 = 975f;
          leftbordersize_pixels = 9;
          rightbordersize_pixels = 9;
          minimum_width_pixels = 26;
          topbordersize_pixels = 9;
          bottombordersize_pixels = 9;
          minimum_height_pixels = 24;
          DontMove = true;
          ClickType = ButtonType.Checkable;
          if (_can_click_off_set)
          {
            break;
          }

          CanClickOff = true;
          break;
      }
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      if (!parent.IsComboBoxElement() && !parent.IsListBoxElement() && (u0 == 0.0 && v0 == 0.0) && (u1 == 0.0 && v1 == 0.0))
      {
        SetToDefaultOptions(type_template);
      }

      base.InitChildren(parent, host, MyButtonCallback);
      if (parent.GetElementType() != ElementType.ComboBoxWidget)
      {
        SetCallback(MyButtonCallback);
      }

      SetGrowableWidth(16, 16, 48);
    }

    private void DoButtonCallback(bool wasChecked)
    {
      buttoncallback?.Invoke(this);

      if (ButtonWidget.ButtonListenerHook == null)
      {
        return;
      }

      ButtonWidget.ButtonListenerHook(this);
    }

    [XmlAttribute("click-type")]
    public ButtonType ClickType
    {
      get
      {
        return type;
      }
      set
      {
        type = value;
      }
    }

    [XmlAttribute("font-color")]
    public new string HexColor
    {
      get
      {
        return hexColor;
      }
      set
      {
        hexColor = value;
        TextColor = IElement.GenerateColorFromHtml(value);
      }
    }

    public Color4 TextColor
    {
      get
      {
        return textcolor;
      }
      set
      {
        Color = value;
        textcolor = value;
      }
    }

    private Color4 TextElementColor
    {
      get
      {
        return Color;
      }
      set
      {
        Color = value;
      }
    }

    [XmlAttribute("font-over-color")]
    public string HexFontOverColor
    {
      get
      {
        return hexFontOverColor;
      }
      set
      {
        hexFontOverColor = value;
        TextOverColor = IElement.GenerateColorFromHtml(hexFontOverColor);
      }
    }

    public Color4 TextOverColor
    {
      get
      {
        return textovercolor;
      }
      set
      {
        textovercolor = value;
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

    [XmlAttribute("font-down-color")]
    public string HexfontdownColor
    {
      get
      {
        return hexFontDownColor;
      }
      set
      {
        hexFontDownColor = value;
        TextDownColor = IElement.GenerateColorFromHtml(hexFontDownColor);
      }
    }

    public Color4 TextDownColor
    {
      get
      {
        return textdowncolor;
      }
      set
      {
        textdowncolor = value;
      }
    }

    public Color4 TextDisabledColor
    {
      get
      {
        return textdisabledcolor;
      }
      set
      {
        textdisabledcolor = value;
      }
    }

    [XmlAttribute("dont-move")]
    public bool DontMove
    {
      get
      {
        return dontmove;
      }
      set
      {
        dontmove = value;
      }
    }

    private void StopDragging()
    {
      down = false;
      dragged_fully = false;
    }

    [XmlAttribute("can-click-off")]
    public bool CanClickOff
    {
      set
      {
        _can_click_off = value;
        _can_click_off_set = true;
      }
      get
      {
        return _can_click_off;
      }
    }
  }
}
