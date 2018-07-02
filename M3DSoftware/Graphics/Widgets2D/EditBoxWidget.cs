// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.EditBoxWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.TextLocalization;
using OpenTK;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class EditBoxWidget : AbstractEditBoxWidget
  {
    public int MAX_CHARS = -1;
    private int highlight_start = -1;
    private int highlight_end = -1;
    private Color4 color;
    private FontSize size;
    protected EditBoxWidget.EditBoxCallback enterkeycallback;
    protected EditBoxWidget.EditBoxCallback clickkeycallback;
    protected EditBoxWidget.EditBoxCallback onbackspace;
    protected EditBoxWidget.EditBoxCallback onnewtext;
    [XmlIgnore]
    public EditBoxWidget.EditBoxCallback OnFocusLost;
    public bool CAPS;
    private int text_window_left_border;
    private int text_window_right_border;
    private int text_window_top_border;
    private int text_window_bottom_border;
    private int tooltipLeft;
    private int tooltipRight;
    private int tooltipTop;
    private int tooltipBottom;
    private long elasped;
    private Simple2DRenderer.Quad cursor_quad;
    private Stopwatch cursor_stopwatch;
    private TextLine textline;
    private Sprite drawable_sprite;
    private int cursor;
    private string hexColor;
    private string hint_text;
    private float _u0;
    private float _u1;
    private float _v0;
    private float _v1;
    private NumFormat _numFormat;
    private int processed_cursor_location;
    private int processed_start;
    private int highlight_start_offset;
    private int highlight_end_offset;
    private bool mouse_dragging;
    private QFont my_font;

    public EditBoxWidget()
      : this(0, (Element2D) null)
    {
    }

    public EditBoxWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public EditBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      drawable_sprite = new Sprite();
      textline = new TextLine();
      cursor_stopwatch = new Stopwatch();
      cursor_stopwatch.Start();
      textline.SetText("");
      hint_text = "";
      SetGrowableWidth(3, 3, 32);
      SetTextWindowBorders(4, 4, 4, 4);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1)
    {
      drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      cursor_quad.color = new Color4(0.7f, 0.7f, 0.7f, 1f);
      cursor_quad.x0 = 0.0f;
      cursor_quad.x1 = 2f;
      cursor_quad.y0 = 0.0f;
      cursor_quad.y1 = (float) (Height - (text_window_top_border + text_window_bottom_border));
      SetToolTipRegion(0, width, 0, height);
    }

    public void SetTextWindowBorders(int left, int right, int top, int bottom)
    {
      text_window_left_border = left;
      text_window_right_border = right;
      text_window_top_border = top;
      text_window_bottom_border = bottom;
    }

    public void SetToolTipRegion(int left, int right, int top, int bottom)
    {
      tooltipLeft = left;
      tooltipRight = right;
      tooltipTop = top;
      tooltipBottom = bottom;
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
    }

    public override void OnMouseLeave()
    {
      mouse_dragging = false;
      base.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (!Enabled || mouseevent.pos.y < TextRegionTop || (mouseevent.pos.y > TextRegionBottom || mouseevent.pos.x < TextRegionLeft) || mouseevent.pos.x > TextRegionRight)
      {
        return false;
      }

      var mouse_rel_x = mouseevent.pos.x - TextRegionLeft;
      var mouse_rel_y = mouseevent.pos.y - TextRegionBottom;
      if (mouseevent.type == MouseEventType.Down && mouseevent.button == MouseButton.Left)
      {
        CancelHighlight();
        cursor = GetCursorFromMousePosition(mouse_rel_x, mouse_rel_y);
        mouse_dragging = true;
        if (mouseevent.num_clicks == 2 && cursor < Text.Length)
        {
          char[] charArray = Text.ToCharArray();
          for (highlight_start = cursor; highlight_start > 0; --highlight_start)
          {
            if (charArray[highlight_start] == ' ')
            {
              ++highlight_start;
              break;
            }
          }
          highlight_end = cursor;
          while (highlight_end < charArray.Length && charArray[highlight_end] != ' ')
          {
            ++highlight_end;
          }

          if (highlight_end - highlight_start < 1)
          {
            CancelHighlight();
          }
        }
      }
      else if (mouseevent.type == MouseEventType.Up && mouseevent.button == MouseButton.Left)
      {
        mouse_dragging = false;
        if (clickkeycallback != null)
        {
          clickkeycallback(this);
        }
      }
      else if (mouse_dragging)
      {
        var fromMousePosition = GetCursorFromMousePosition(mouse_rel_x, mouse_rel_y);
        cursor = fromMousePosition;
        if (highlight_start < 0)
        {
          highlight_start = highlight_end = cursor;
        }

        if (fromMousePosition > highlight_start)
        {
          highlight_end = fromMousePosition;
        }
        else
        {
          highlight_start = fromMousePosition;
        }
      }
      return true;
    }

    private int GetCursorFromMousePosition(int mouse_rel_x, int mouse_rel_y)
    {
      if (my_font == null || Text.Length <= 0)
      {
        return 0;
      }

      var length = 0;
      var num = mouse_rel_x;
      SizeF sizeF = my_font.Measure(Text);
      var width = (int) sizeF.Width;
      if (num > width)
      {
        return Text.Length;
      }

      do
      {
        ++length;
        sizeF = my_font.Measure(Text.Substring(0, length));
      }
      while ((int) sizeF.Width < mouse_rel_x && length < Text.Length);
      return length - 1;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!Enabled || !HasFocus)
      {
        return false;
      }

      if (keyboardevent.type == KeyboardEventType.InputKey)
      {
        var inputKeyEvent = (InputKeyEvent) keyboardevent;
        if (inputKeyEvent.Ch == '\r' && enterkeycallback != null)
        {
          enterkeycallback(this);
        }

        if (inputKeyEvent.Ctrl)
        {
          if (inputKeyEvent.Ch == 'c' || inputKeyEvent.Ch == 'C')
          {
            CopyToClipboard(GetHighlightedRegion());
          }
          else if (inputKeyEvent.Ch == 'V' || inputKeyEvent.Ch == 'v')
          {
            var ch = CopyFromClipboard();
            if (!string.IsNullOrEmpty(ch))
            {
              if (highlight_start >= 0)
              {
                var highlightStart = highlight_start;
                DeleteHighlightedRegion();
                textline.AddStringAt(ch, highlightStart);
              }
              else
              {
                textline.AddStringAt(ch, cursor);
              }
            }
          }
          else if (inputKeyEvent.Ch == 'X' || inputKeyEvent.Ch == 'x')
          {
            CopyToClipboard(GetHighlightedRegion());
            DeleteHighlightedRegion();
          }
        }
        else
        {
          DeleteHighlightedRegion();
          if (inputKeyEvent.Ch == '\r' || inputKeyEvent.Ch == '\n')
          {
            OnControlMsg((Element2D) this, ControlMsg.ENTERHIT, 0.0f, 0.0f);
          }
          else if (inputKeyEvent.Ch == '\b')
          {
            if (cursor > 0)
            {
              textline.DeleteAt(cursor - 1);
            }

            EditBoxWidget.EditBoxCallback onbackspace = this.onbackspace;
            if (onbackspace != null)
            {
              onbackspace(this);
            }

            if (cursor > 0)
            {
              --cursor;
            }
          }
          else if (MAX_CHARS <= 0 || MAX_CHARS > textline.GetSize())
          {
            var ch = inputKeyEvent.Ch;
            if (CAPS)
            {
              ch = char.ToUpper(ch);
            }

            textline.AddCharAt(ch, cursor);
            EditBoxWidget.EditBoxCallback onnewtext = this.onnewtext;
            if (onnewtext != null)
            {
              onnewtext(this);
            }

            ++cursor;
          }
        }
        return true;
      }
      if (keyboardevent.type == KeyboardEventType.CommandKey)
      {
        var commandKeyEvent = (CommandKeyEvent) keyboardevent;
        if (commandKeyEvent.Key == KeyboardCommandKey.Left)
        {
          if (cursor > 0)
          {
            if (commandKeyEvent.Shift)
            {
              if (highlight_start < 0 || highlight_end == highlight_start)
              {
                highlight_end = cursor;
                highlight_start = --cursor;
              }
              else if (cursor == highlight_end)
              {
                highlight_end = --cursor;
              }
              else
              {
                highlight_start = --cursor;
              }
            }
            else
            {
              --cursor;
            }
          }
          if (!commandKeyEvent.Shift)
          {
            CancelHighlight();
          }
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Right)
        {
          if (cursor < Text.Length)
          {
            if (commandKeyEvent.Shift)
            {
              if (highlight_start < 0 || highlight_end == highlight_start)
              {
                highlight_start = cursor;
                highlight_end = ++cursor;
              }
              else if (cursor == highlight_start)
              {
                highlight_start = ++cursor;
              }
              else
              {
                highlight_end = ++cursor;
              }
            }
            else
            {
              ++cursor;
            }
          }
          if (!commandKeyEvent.Shift)
          {
            CancelHighlight();
          }
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Delete)
        {
          if (highlight_start >= 0)
          {
            DeleteHighlightedRegion();
          }
          else if (cursor < Text.Length)
          {
            textline.DeleteAt(cursor);
          }
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Home)
        {
          if (!commandKeyEvent.Shift)
          {
            CancelHighlight();
          }
          else if (Text.Length > 0 && cursor > 0)
          {
            if (highlight_start < 0)
            {
              highlight_end = cursor;
            }
            else if (cursor > highlight_start)
            {
              highlight_end = highlight_start;
            }

            highlight_start = 0;
          }
          cursor = 0;
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.End)
        {
          if (!commandKeyEvent.Shift)
          {
            CancelHighlight();
          }
          else if (Text.Length > 0 && cursor < Text.Length)
          {
            if (highlight_start < 0)
            {
              highlight_start = cursor;
            }
            else if (cursor < highlight_end)
            {
              highlight_start = highlight_end;
            }

            highlight_end = Text.Length;
          }
          cursor = Text.Length;
        }
      }
      return base.OnKeyboardEvent(keyboardevent);
    }

    public override void OnRender(GUIHost host)
    {
      drawable_sprite.Render(host, State.Normal, X_Abs, Y_Abs, Width, Height);
      RenderText(host);
      if (Enabled)
      {
        DrawCursor(host);
      }

      base.OnRender(host);
    }

    private void RenderText(GUIHost host)
    {
      if (cursor >= Text.Length)
      {
        cursor = Text.Length;
      }

      var str = textline.GetSize() <= 0 ? hint_text : textline.GetText();
      host.SetCurFontSize(Size);
      my_font = host.GetCurrentFont();
      if (my_font == null)
      {
        return;
      }

      GetStartCharacterIndex(GetCursorLocation(my_font), my_font);
      my_font.Options.LockToPixel = true;
      var width = Width;
      var y = (float) (Y_Abs + text_window_top_border);
      var x = (float) (X_Abs + text_window_left_border);
      DrawHighlight(host.GetSimpleRenderer(), my_font);
      QFont.Begin();
      my_font.Options.Colour = Enabled ? Color : new Color4(0.7f, 0.7f, 0.7f, 0.5f);
      my_font.Print(str.Substring(processed_start), (float) (Width - (text_window_left_border + text_window_right_border)), new Vector2(x, y));
      QFont.End();
    }

    public bool IsValid(bool charsAllowed, bool checkNumFormat)
    {
      var flag = false;
      if (!charsAllowed)
      {
        var source = Value as string;
        Func<char, bool> func = (Func<char, bool>) (x => char.IsLetter(x));
        if (!source.Any())
        {
          flag = true;
        }
        else
        {
          Value = (object) Regex.Replace((string)Value, "[^0-9.+-]", "");
        }
      }
      if (checkNumFormat & flag)
      {
        if (WithinNumericRange())
        {
          flag = true;
        }
        else
        {
          Value = (object)RoundNumberToFormat(Convert.ToDouble(Value.ToString()), GetPrecision(_numFormat));
        }
      }
      return flag;
    }

    private bool WithinNumericRange()
    {
      var flag = false;
      if (Value != null && double.TryParse(Value.ToString(), out var result))
      {
        if (_numFormat == NumFormat.Whole)
        {
          if (result % 1.0 == 0.0)
          {
            flag = true;
          }
        }
        else
        {
          var num = result % 1.0;
          if (num > 0.0 && num <= 0.999)
          {
            flag = true;
          }
        }
        if (flag)
        {
          Value = (object)RoundNumberToFormat(result, GetPrecision(_numFormat));
        }
      }
      return flag;
    }

    private int GetPrecision(NumFormat format)
    {
      int num;
      switch (format)
      {
        case NumFormat.Ten:
          num = 1;
          break;
        case NumFormat.Hundreds:
          num = 2;
          break;
        case NumFormat.Thousands:
          num = 3;
          break;
        default:
          num = 0;
          break;
      }
      return num;
    }

    private string RoundNumberToFormat(double numVal, int precision)
    {
      var format = "0.";
      for (var index = 0; index < precision; ++index)
      {
        format += "0";
      }

      return numVal.ToString(format);
    }

    private string GetHighlightedRegion()
    {
      if (highlight_start >= 0)
      {
        return Text.Substring(highlight_start, highlight_end - highlight_start);
      }

      return "";
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      if (parent.GetElementType() != ElementType.ComboBoxWidget)
      {
        Init(host, "guicontrols", u0, v0, u1, v1);
        SetGrowableWidth(3, 3, 32);
        SetTextWindowBorders(4, 4, 4, 4);
      }
      else
      {
        HexColor = ((ComboBoxWidget) parent).HexColor;
        Size = ((ComboBoxWidget) parent).Size;
        ToolTipMessage = parent.ToolTipMessage;
      }
      lock (ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>)ChildList)
        {
          child.InitChildren((Element2D) this, host, MyButtonCallback);
        }
      }
    }

    [XmlAttribute("texture-u0")]
    public float u0
    {
      get
      {
        return _u0;
      }
      set
      {
        _u0 = value;
      }
    }

    [XmlAttribute("texture-v0")]
    public float v0
    {
      get
      {
        return _v0;
      }
      set
      {
        _v0 = value;
      }
    }

    [XmlAttribute("texture-u1")]
    public float u1
    {
      get
      {
        return _u1;
      }
      set
      {
        _u1 = value;
      }
    }

    [XmlAttribute("texture-v1")]
    public float v1
    {
      get
      {
        return _v1;
      }
      set
      {
        _v1 = value;
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

    [XmlAttribute("font-color")]
    public string HexColor
    {
      get
      {
        return hexColor;
      }
      set
      {
        hexColor = value;
        Color = IElement.GenerateColorFromHtml(hexColor);
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

    [XmlAttribute("hint")]
    public string Hint
    {
      get
      {
        return hint_text;
      }
      set
      {
        if (value.StartsWith("T_"))
        {
          hint_text = Locale.GlobalLocale.T(value);
        }
        else
        {
          hint_text = value.Replace("\\n", "\n");
        }
      }
    }

    [XmlText]
    public override string Text
    {
      get
      {
        return textline.GetText();
      }
      set
      {
        CancelHighlight();
        if (string.IsNullOrEmpty(value))
        {
          textline.SetText("");
          cursor = 0;
        }
        else
        {
          textline.SetText((!value.StartsWith("T_") ? value.Replace('\n', ' ') : Locale.GlobalLocale.T(value)).Replace('\r', ' ').Replace('\t', ' '));
          cursor = value.Length;
        }
        OnControlMsg((Element2D) this, ControlMsg.TEXT_CHANGED, 0.0f, 0.0f);
      }
    }

    [XmlIgnore]
    public int Cursor
    {
      get
      {
        return cursor;
      }
    }

    [XmlIgnore]
    private int TextRegionTop
    {
      get
      {
        return Y_Abs + text_window_top_border;
      }
    }

    [XmlIgnore]
    private int TextRegionBottom
    {
      get
      {
        return TextRegionTop + (int)cursor_quad.y1;
      }
    }

    [XmlIgnore]
    private int TextRegionLeft
    {
      get
      {
        return X_Abs + text_window_left_border;
      }
    }

    [XmlIgnore]
    private int TextRegionRight
    {
      get
      {
        return TextRegionLeft + (Width - (text_window_left_border + text_window_right_border));
      }
    }

    public NumFormat NumFormat
    {
      set
      {
        _numFormat = value;
      }
    }

    private void DrawCursor(GUIHost host)
    {
      if (!HasFocus)
      {
        return;
      }

      var processedCursorLocation = processed_cursor_location;
      cursor_stopwatch.Stop();
      elasped += cursor_stopwatch.ElapsedMilliseconds;
      cursor_stopwatch.Reset();
      cursor_stopwatch.Start();
      if (elasped < 300L)
      {
        return;
      }

      if (elasped > 600L)
      {
        elasped = 0L;
      }

      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      cursor_quad.x0 = (float) (X_Abs + text_window_left_border + processedCursorLocation);
      cursor_quad.y0 = (float) (Y_Abs + text_window_top_border - 2);
      cursor_quad.x1 = (float) (int) ((double)cursor_quad.x0 + 2.0);
      cursor_quad.y1 = cursor_quad.y0 + (float)Height - (float) (text_window_top_border + text_window_bottom_border);
      Simple2DRenderer.Quad cursorQuad = cursor_quad;
      simpleRenderer.DrawQuad(cursorQuad);
    }

    private int GetCursorLocation(QFont font)
    {
      if (cursor <= 0)
      {
        return 0;
      }

      var text = Text.Substring(0, cursor);
      return (int) font.Measure(text).Width;
    }

    private int GetStartCharacterIndex(int cursor_location, QFont font)
    {
      var length = 0;
      var num1 = 0;
      int num2;
      string text1;
      for (num2 = Width - (text_window_left_border + text_window_right_border); cursor_location - num1 > num2 && length < Text.Length; num1 = (int) font.Measure(text1).Width)
      {
        ++length;
        text1 = Text.Substring(0, length);
      }
      processed_start = length;
      processed_cursor_location = cursor_location - num1;
      if (highlight_start >= 0 && highlight_start < Text.Length)
      {
        SizeF sizeF;
        if (highlight_start == 0)
        {
          highlight_start_offset = 0;
        }
        else
        {
          var text2 = Text.Substring(0, highlight_start);
          sizeF = font.Measure(text2);
          highlight_start_offset = (int) sizeF.Width;
        }
        if (highlight_end >= highlight_start)
        {
          if (highlight_end >= Text.Length)
          {
            sizeF = font.Measure(Text);
            highlight_end_offset = (int) sizeF.Width;
          }
          else
          {
            var text2 = Text.Substring(0, highlight_end);
            sizeF = font.Measure(text2);
            highlight_end_offset = (int) sizeF.Width;
          }
        }
      }
      highlight_start_offset -= num1;
      if (highlight_start_offset < 0)
      {
        highlight_start_offset = 0;
      }

      highlight_end_offset -= num1;
      if (highlight_end_offset < 0)
      {
        highlight_end_offset = 0;
      }

      if (highlight_end_offset > num2)
      {
        highlight_end_offset = num2;
      }

      return length;
    }

    private void DrawHighlight(Simple2DRenderer render, QFont font)
    {
      if (highlight_start < 0)
      {
        return;
      }

      var num1 = X_Abs + text_window_left_border;
      var num2 = Y_Abs + text_window_top_border;
      var num3 = Height - text_window_top_border * 2;
      Simple2DRenderer.Quad cursorQuad = cursor_quad;
      cursorQuad.x0 = (float) (num1 + highlight_start_offset);
      cursorQuad.x1 = (float) (num1 + highlight_end_offset);
      cursorQuad.y0 = (float) num2;
      cursorQuad.y1 = (float) (num2 + num3);
      cursorQuad.color = new Color4(0.854902f, 0.945098f, 0.972549f, 1f);
      render.DrawQuad(cursorQuad);
    }

    private void CancelHighlight()
    {
      highlight_start = -1;
      highlight_end = -1;
      highlight_start_offset = 0;
      highlight_end_offset = 0;
    }

    private void DeleteHighlightedRegion()
    {
      if (highlight_start >= 0 && highlight_end <= Text.Length)
      {
        textline.DeleteRegion(highlight_start, highlight_end);
      }

      CancelHighlight();
    }

    private void CopyToClipboard(string thetext)
    {
      if (string.IsNullOrEmpty(thetext))
      {
        return;
      }

      Clipboard.SetText(thetext);
    }

    private string CopyFromClipboard()
    {
      return Clipboard.GetText().Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
    }

    public void SetCallbackEnterKey(EditBoxWidget.EditBoxCallback func)
    {
      enterkeycallback = func;
    }

    public void SetCallbackOnClick(EditBoxWidget.EditBoxCallback func)
    {
      clickkeycallback = func;
    }

    public void SetCallbackOnTextAdded(EditBoxWidget.EditBoxCallback func)
    {
      onnewtext = func;
    }

    public void SetCallbackOnBackspace(EditBoxWidget.EditBoxCallback func)
    {
      onbackspace = func;
    }

    public override void OnFocusChanged()
    {
      if (HasFocus || OnFocusLost == null)
      {
        return;
      }

      OnFocusLost(this);
    }

    public delegate void EditBoxCallback(EditBoxWidget edit);
  }
}
