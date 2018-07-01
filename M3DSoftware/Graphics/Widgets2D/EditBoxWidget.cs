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
      this.drawable_sprite = new Sprite();
      this.textline = new TextLine();
      this.cursor_stopwatch = new Stopwatch();
      this.cursor_stopwatch.Start();
      this.textline.SetText("");
      this.hint_text = "";
      this.SetGrowableWidth(3, 3, 32);
      this.SetTextWindowBorders(4, 4, 4, 4);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1)
    {
      this.drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
    }

    public override void SetSize(int width, int height)
    {
      base.SetSize(width, height);
      this.cursor_quad.color = new Color4(0.7f, 0.7f, 0.7f, 1f);
      this.cursor_quad.x0 = 0.0f;
      this.cursor_quad.x1 = 2f;
      this.cursor_quad.y0 = 0.0f;
      this.cursor_quad.y1 = (float) (this.Height - (this.text_window_top_border + this.text_window_bottom_border));
      this.SetToolTipRegion(0, width, 0, height);
    }

    public void SetTextWindowBorders(int left, int right, int top, int bottom)
    {
      this.text_window_left_border = left;
      this.text_window_right_border = right;
      this.text_window_top_border = top;
      this.text_window_bottom_border = bottom;
    }

    public void SetToolTipRegion(int left, int right, int top, int bottom)
    {
      this.tooltipLeft = left;
      this.tooltipRight = right;
      this.tooltipTop = top;
      this.tooltipBottom = bottom;
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      this.drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
    }

    public override void OnMouseLeave()
    {
      this.mouse_dragging = false;
      base.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (!this.Enabled || mouseevent.pos.y < this.TextRegionTop || (mouseevent.pos.y > this.TextRegionBottom || mouseevent.pos.x < this.TextRegionLeft) || mouseevent.pos.x > this.TextRegionRight)
        return false;
      int mouse_rel_x = mouseevent.pos.x - this.TextRegionLeft;
      int mouse_rel_y = mouseevent.pos.y - this.TextRegionBottom;
      if (mouseevent.type == MouseEventType.Down && mouseevent.button == MouseButton.Left)
      {
        this.CancelHighlight();
        this.cursor = this.GetCursorFromMousePosition(mouse_rel_x, mouse_rel_y);
        this.mouse_dragging = true;
        if (mouseevent.num_clicks == 2 && this.cursor < this.Text.Length)
        {
          char[] charArray = this.Text.ToCharArray();
          for (this.highlight_start = this.cursor; this.highlight_start > 0; --this.highlight_start)
          {
            if (charArray[this.highlight_start] == ' ')
            {
              ++this.highlight_start;
              break;
            }
          }
          this.highlight_end = this.cursor;
          while (this.highlight_end < charArray.Length && charArray[this.highlight_end] != ' ')
            ++this.highlight_end;
          if (this.highlight_end - this.highlight_start < 1)
            this.CancelHighlight();
        }
      }
      else if (mouseevent.type == MouseEventType.Up && mouseevent.button == MouseButton.Left)
      {
        this.mouse_dragging = false;
        if (this.clickkeycallback != null)
          this.clickkeycallback(this);
      }
      else if (this.mouse_dragging)
      {
        int fromMousePosition = this.GetCursorFromMousePosition(mouse_rel_x, mouse_rel_y);
        this.cursor = fromMousePosition;
        if (this.highlight_start < 0)
          this.highlight_start = this.highlight_end = this.cursor;
        if (fromMousePosition > this.highlight_start)
          this.highlight_end = fromMousePosition;
        else
          this.highlight_start = fromMousePosition;
      }
      return true;
    }

    private int GetCursorFromMousePosition(int mouse_rel_x, int mouse_rel_y)
    {
      if (this.my_font == null || this.Text.Length <= 0)
        return 0;
      int length = 0;
      int num = mouse_rel_x;
      SizeF sizeF = this.my_font.Measure(this.Text);
      int width = (int) sizeF.Width;
      if (num > width)
        return this.Text.Length;
      do
      {
        ++length;
        sizeF = this.my_font.Measure(this.Text.Substring(0, length));
      }
      while ((int) sizeF.Width < mouse_rel_x && length < this.Text.Length);
      return length - 1;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (!this.Enabled || !this.HasFocus)
        return false;
      if (keyboardevent.type == KeyboardEventType.InputKey)
      {
        InputKeyEvent inputKeyEvent = (InputKeyEvent) keyboardevent;
        if (inputKeyEvent.Ch == '\r' && this.enterkeycallback != null)
          this.enterkeycallback(this);
        if (inputKeyEvent.Ctrl)
        {
          if (inputKeyEvent.Ch == 'c' || inputKeyEvent.Ch == 'C')
            this.CopyToClipboard(this.GetHighlightedRegion());
          else if (inputKeyEvent.Ch == 'V' || inputKeyEvent.Ch == 'v')
          {
            string ch = this.CopyFromClipboard();
            if (!string.IsNullOrEmpty(ch))
            {
              if (this.highlight_start >= 0)
              {
                int highlightStart = this.highlight_start;
                this.DeleteHighlightedRegion();
                this.textline.AddStringAt(ch, highlightStart);
              }
              else
                this.textline.AddStringAt(ch, this.cursor);
            }
          }
          else if (inputKeyEvent.Ch == 'X' || inputKeyEvent.Ch == 'x')
          {
            this.CopyToClipboard(this.GetHighlightedRegion());
            this.DeleteHighlightedRegion();
          }
        }
        else
        {
          this.DeleteHighlightedRegion();
          if (inputKeyEvent.Ch == '\r' || inputKeyEvent.Ch == '\n')
            this.OnControlMsg((Element2D) this, ControlMsg.ENTERHIT, 0.0f, 0.0f);
          else if (inputKeyEvent.Ch == '\b')
          {
            if (this.cursor > 0)
              this.textline.DeleteAt(this.cursor - 1);
            EditBoxWidget.EditBoxCallback onbackspace = this.onbackspace;
            if (onbackspace != null)
              onbackspace(this);
            if (this.cursor > 0)
              --this.cursor;
          }
          else if (this.MAX_CHARS <= 0 || this.MAX_CHARS > this.textline.GetSize())
          {
            char ch = inputKeyEvent.Ch;
            if (this.CAPS)
              ch = char.ToUpper(ch);
            this.textline.AddCharAt(ch, this.cursor);
            EditBoxWidget.EditBoxCallback onnewtext = this.onnewtext;
            if (onnewtext != null)
              onnewtext(this);
            ++this.cursor;
          }
        }
        return true;
      }
      if (keyboardevent.type == KeyboardEventType.CommandKey)
      {
        CommandKeyEvent commandKeyEvent = (CommandKeyEvent) keyboardevent;
        if (commandKeyEvent.Key == KeyboardCommandKey.Left)
        {
          if (this.cursor > 0)
          {
            if (commandKeyEvent.Shift)
            {
              if (this.highlight_start < 0 || this.highlight_end == this.highlight_start)
              {
                this.highlight_end = this.cursor;
                this.highlight_start = --this.cursor;
              }
              else if (this.cursor == this.highlight_end)
                this.highlight_end = --this.cursor;
              else
                this.highlight_start = --this.cursor;
            }
            else
              --this.cursor;
          }
          if (!commandKeyEvent.Shift)
            this.CancelHighlight();
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Right)
        {
          if (this.cursor < this.Text.Length)
          {
            if (commandKeyEvent.Shift)
            {
              if (this.highlight_start < 0 || this.highlight_end == this.highlight_start)
              {
                this.highlight_start = this.cursor;
                this.highlight_end = ++this.cursor;
              }
              else if (this.cursor == this.highlight_start)
                this.highlight_start = ++this.cursor;
              else
                this.highlight_end = ++this.cursor;
            }
            else
              ++this.cursor;
          }
          if (!commandKeyEvent.Shift)
            this.CancelHighlight();
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Delete)
        {
          if (this.highlight_start >= 0)
            this.DeleteHighlightedRegion();
          else if (this.cursor < this.Text.Length)
            this.textline.DeleteAt(this.cursor);
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.Home)
        {
          if (!commandKeyEvent.Shift)
            this.CancelHighlight();
          else if (this.Text.Length > 0 && this.cursor > 0)
          {
            if (this.highlight_start < 0)
              this.highlight_end = this.cursor;
            else if (this.cursor > this.highlight_start)
              this.highlight_end = this.highlight_start;
            this.highlight_start = 0;
          }
          this.cursor = 0;
        }
        else if (commandKeyEvent.Key == KeyboardCommandKey.End)
        {
          if (!commandKeyEvent.Shift)
            this.CancelHighlight();
          else if (this.Text.Length > 0 && this.cursor < this.Text.Length)
          {
            if (this.highlight_start < 0)
              this.highlight_start = this.cursor;
            else if (this.cursor < this.highlight_end)
              this.highlight_start = this.highlight_end;
            this.highlight_end = this.Text.Length;
          }
          this.cursor = this.Text.Length;
        }
      }
      return base.OnKeyboardEvent(keyboardevent);
    }

    public override void OnRender(GUIHost host)
    {
      this.drawable_sprite.Render(host, State.Normal, this.X_Abs, this.Y_Abs, this.Width, this.Height);
      this.RenderText(host);
      if (this.Enabled)
        this.DrawCursor(host);
      base.OnRender(host);
    }

    private void RenderText(GUIHost host)
    {
      if (this.cursor >= this.Text.Length)
        this.cursor = this.Text.Length;
      string str = this.textline.GetSize() <= 0 ? this.hint_text : this.textline.GetText();
      host.SetCurFontSize(this.Size);
      this.my_font = host.GetCurrentFont();
      if (this.my_font == null)
        return;
      this.GetStartCharacterIndex(this.GetCursorLocation(this.my_font), this.my_font);
      this.my_font.Options.LockToPixel = true;
      int width = this.Width;
      float y = (float) (this.Y_Abs + this.text_window_top_border);
      float x = (float) (this.X_Abs + this.text_window_left_border);
      this.DrawHighlight(host.GetSimpleRenderer(), this.my_font);
      QFont.Begin();
      this.my_font.Options.Colour = this.Enabled ? this.Color : new Color4(0.7f, 0.7f, 0.7f, 0.5f);
      this.my_font.Print(str.Substring(this.processed_start), (float) (this.Width - (this.text_window_left_border + this.text_window_right_border)), new Vector2(x, y));
      QFont.End();
    }

    public bool IsValid(bool charsAllowed, bool checkNumFormat)
    {
      bool flag = false;
      if (!charsAllowed)
      {
        string source = this.Value as string;
        Func<char, bool> func = (Func<char, bool>) (x => char.IsLetter(x));
        if (!source.Any())
          flag = true;
        else
          this.Value = (object) Regex.Replace((string) this.Value, "[^0-9.+-]", "");
      }
      if (checkNumFormat & flag)
      {
        if (this.WithinNumericRange())
          flag = true;
        else
          this.Value = (object) this.RoundNumberToFormat(Convert.ToDouble(this.Value.ToString()), this.GetPrecision(this._numFormat));
      }
      return flag;
    }

    private bool WithinNumericRange()
    {
      bool flag = false;
      double result;
      if (this.Value != null && double.TryParse(this.Value.ToString(), out result))
      {
        if (this._numFormat == NumFormat.Whole)
        {
          if (result % 1.0 == 0.0)
            flag = true;
        }
        else
        {
          double num = result % 1.0;
          if (num > 0.0 && num <= 0.999)
            flag = true;
        }
        if (flag)
          this.Value = (object) this.RoundNumberToFormat(result, this.GetPrecision(this._numFormat));
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
      string format = "0.";
      for (int index = 0; index < precision; ++index)
        format += "0";
      return numVal.ToString(format);
    }

    private string GetHighlightedRegion()
    {
      if (this.highlight_start >= 0)
        return this.Text.Substring(this.highlight_start, this.highlight_end - this.highlight_start);
      return "";
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      if (parent.GetElementType() != ElementType.ComboBoxWidget)
      {
        this.Init(host, "guicontrols", this.u0, this.v0, this.u1, this.v1);
        this.SetGrowableWidth(3, 3, 32);
        this.SetTextWindowBorders(4, 4, 4, 4);
      }
      else
      {
        this.HexColor = ((ComboBoxWidget) parent).HexColor;
        this.Size = ((ComboBoxWidget) parent).Size;
        this.ToolTipMessage = parent.ToolTipMessage;
      }
      lock (this.ChildList)
      {
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
          child.InitChildren((Element2D) this, host, MyButtonCallback);
      }
    }

    [XmlAttribute("texture-u0")]
    public float u0
    {
      get
      {
        return this._u0;
      }
      set
      {
        this._u0 = value;
      }
    }

    [XmlAttribute("texture-v0")]
    public float v0
    {
      get
      {
        return this._v0;
      }
      set
      {
        this._v0 = value;
      }
    }

    [XmlAttribute("texture-u1")]
    public float u1
    {
      get
      {
        return this._u1;
      }
      set
      {
        this._u1 = value;
      }
    }

    [XmlAttribute("texture-v1")]
    public float v1
    {
      get
      {
        return this._v1;
      }
      set
      {
        this._v1 = value;
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

    [XmlAttribute("font-color")]
    public string HexColor
    {
      get
      {
        return this.hexColor;
      }
      set
      {
        this.hexColor = value;
        this.Color = IElement.GenerateColorFromHtml(this.hexColor);
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

    [XmlAttribute("hint")]
    public string Hint
    {
      get
      {
        return this.hint_text;
      }
      set
      {
        if (value.StartsWith("T_"))
          this.hint_text = Locale.GlobalLocale.T(value);
        else
          this.hint_text = value.Replace("\\n", "\n");
      }
    }

    [XmlText]
    public override string Text
    {
      get
      {
        return this.textline.GetText();
      }
      set
      {
        this.CancelHighlight();
        if (string.IsNullOrEmpty(value))
        {
          this.textline.SetText("");
          this.cursor = 0;
        }
        else
        {
          this.textline.SetText((!value.StartsWith("T_") ? value.Replace('\n', ' ') : Locale.GlobalLocale.T(value)).Replace('\r', ' ').Replace('\t', ' '));
          this.cursor = value.Length;
        }
        this.OnControlMsg((Element2D) this, ControlMsg.TEXT_CHANGED, 0.0f, 0.0f);
      }
    }

    [XmlIgnore]
    public int Cursor
    {
      get
      {
        return this.cursor;
      }
    }

    [XmlIgnore]
    private int TextRegionTop
    {
      get
      {
        return this.Y_Abs + this.text_window_top_border;
      }
    }

    [XmlIgnore]
    private int TextRegionBottom
    {
      get
      {
        return this.TextRegionTop + (int) this.cursor_quad.y1;
      }
    }

    [XmlIgnore]
    private int TextRegionLeft
    {
      get
      {
        return this.X_Abs + this.text_window_left_border;
      }
    }

    [XmlIgnore]
    private int TextRegionRight
    {
      get
      {
        return this.TextRegionLeft + (this.Width - (this.text_window_left_border + this.text_window_right_border));
      }
    }

    public NumFormat NumFormat
    {
      set
      {
        this._numFormat = value;
      }
    }

    private void DrawCursor(GUIHost host)
    {
      if (!this.HasFocus)
        return;
      int processedCursorLocation = this.processed_cursor_location;
      this.cursor_stopwatch.Stop();
      this.elasped += this.cursor_stopwatch.ElapsedMilliseconds;
      this.cursor_stopwatch.Reset();
      this.cursor_stopwatch.Start();
      if (this.elasped < 300L)
        return;
      if (this.elasped > 600L)
        this.elasped = 0L;
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      this.cursor_quad.x0 = (float) (this.X_Abs + this.text_window_left_border + processedCursorLocation);
      this.cursor_quad.y0 = (float) (this.Y_Abs + this.text_window_top_border - 2);
      this.cursor_quad.x1 = (float) (int) ((double) this.cursor_quad.x0 + 2.0);
      this.cursor_quad.y1 = this.cursor_quad.y0 + (float) this.Height - (float) (this.text_window_top_border + this.text_window_bottom_border);
      Simple2DRenderer.Quad cursorQuad = this.cursor_quad;
      simpleRenderer.DrawQuad(cursorQuad);
    }

    private int GetCursorLocation(QFont font)
    {
      if (this.cursor <= 0)
        return 0;
      string text = this.Text.Substring(0, this.cursor);
      return (int) font.Measure(text).Width;
    }

    private int GetStartCharacterIndex(int cursor_location, QFont font)
    {
      int length = 0;
      int num1 = 0;
      int num2;
      string text1;
      for (num2 = this.Width - (this.text_window_left_border + this.text_window_right_border); cursor_location - num1 > num2 && length < this.Text.Length; num1 = (int) font.Measure(text1).Width)
      {
        ++length;
        text1 = this.Text.Substring(0, length);
      }
      this.processed_start = length;
      this.processed_cursor_location = cursor_location - num1;
      if (this.highlight_start >= 0 && this.highlight_start < this.Text.Length)
      {
        SizeF sizeF;
        if (this.highlight_start == 0)
        {
          this.highlight_start_offset = 0;
        }
        else
        {
          string text2 = this.Text.Substring(0, this.highlight_start);
          sizeF = font.Measure(text2);
          this.highlight_start_offset = (int) sizeF.Width;
        }
        if (this.highlight_end >= this.highlight_start)
        {
          if (this.highlight_end >= this.Text.Length)
          {
            sizeF = font.Measure(this.Text);
            this.highlight_end_offset = (int) sizeF.Width;
          }
          else
          {
            string text2 = this.Text.Substring(0, this.highlight_end);
            sizeF = font.Measure(text2);
            this.highlight_end_offset = (int) sizeF.Width;
          }
        }
      }
      this.highlight_start_offset -= num1;
      if (this.highlight_start_offset < 0)
        this.highlight_start_offset = 0;
      this.highlight_end_offset -= num1;
      if (this.highlight_end_offset < 0)
        this.highlight_end_offset = 0;
      if (this.highlight_end_offset > num2)
        this.highlight_end_offset = num2;
      return length;
    }

    private void DrawHighlight(Simple2DRenderer render, QFont font)
    {
      if (this.highlight_start < 0)
        return;
      int num1 = this.X_Abs + this.text_window_left_border;
      int num2 = this.Y_Abs + this.text_window_top_border;
      int num3 = this.Height - this.text_window_top_border * 2;
      Simple2DRenderer.Quad cursorQuad = this.cursor_quad;
      cursorQuad.x0 = (float) (num1 + this.highlight_start_offset);
      cursorQuad.x1 = (float) (num1 + this.highlight_end_offset);
      cursorQuad.y0 = (float) num2;
      cursorQuad.y1 = (float) (num2 + num3);
      cursorQuad.color = new Color4(0.854902f, 0.945098f, 0.972549f, 1f);
      render.DrawQuad(cursorQuad);
    }

    private void CancelHighlight()
    {
      this.highlight_start = -1;
      this.highlight_end = -1;
      this.highlight_start_offset = 0;
      this.highlight_end_offset = 0;
    }

    private void DeleteHighlightedRegion()
    {
      if (this.highlight_start >= 0 && this.highlight_end <= this.Text.Length)
        this.textline.DeleteRegion(this.highlight_start, this.highlight_end);
      this.CancelHighlight();
    }

    private void CopyToClipboard(string thetext)
    {
      if (string.IsNullOrEmpty(thetext))
        return;
      Clipboard.SetText(thetext);
    }

    private string CopyFromClipboard()
    {
      return Clipboard.GetText().Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
    }

    public void SetCallbackEnterKey(EditBoxWidget.EditBoxCallback func)
    {
      this.enterkeycallback = func;
    }

    public void SetCallbackOnClick(EditBoxWidget.EditBoxCallback func)
    {
      this.clickkeycallback = func;
    }

    public void SetCallbackOnTextAdded(EditBoxWidget.EditBoxCallback func)
    {
      this.onnewtext = func;
    }

    public void SetCallbackOnBackspace(EditBoxWidget.EditBoxCallback func)
    {
      this.onbackspace = func;
    }

    public override void OnFocusChanged()
    {
      if (this.HasFocus || this.OnFocusLost == null)
        return;
      this.OnFocusLost(this);
    }

    public delegate void EditBoxCallback(EditBoxWidget edit);
  }
}
