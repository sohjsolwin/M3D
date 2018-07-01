// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.TextWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.TextLocalization;
using OpenTK;
using OpenTK.Graphics;
using QuickFont;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class TextWidget : Element2D, IDataAccessable
  {
    private string hexColor;
    private Color4 color;
    private string text;
    private FontSize size;
    private QFontAlignment alignment;
    protected int off_x;
    protected int off_y;
    private TextVerticalAlignment vertical_alignment;

    public TextWidget()
      : this(0, (Element2D) null)
    {
    }

    public TextWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public TextWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.color = new Color4(0.0f, 0.0f, 0.0f, 1f);
      this.off_x = 0;
      this.off_y = 0;
      this.text = "";
      this.size = FontSize.Medium;
      this.alignment = QFontAlignment.Centre;
      this.vertical_alignment = TextVerticalAlignment.Middle;
      this.IgnoreMouse = true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.TextWidget;
    }

    public override void OnRender(GUIHost host)
    {
      host.SetCurFontSize(this.Size);
      QFont currentFont = host.GetCurrentFont();
      if (currentFont != null)
      {
        currentFont.Options.LockToPixel = true;
        float width = (float) this.Width;
        float height = currentFont.Measure(this.text, width, this.alignment).Height;
        float num1 = this.VAlignment != TextVerticalAlignment.Top ? (this.VAlignment != TextVerticalAlignment.Bottom ? (float) ((double) this.Height * 0.5 - (double) height * 0.5) + (float) this.Y_Abs : (float) this.Y_Abs + ((float) this.Height - height)) : (float) this.Y_Abs;
        float num2 = this.alignment != QFontAlignment.Centre ? (float) this.X_Abs : (float) this.Width * 0.5f + (float) this.X_Abs;
        Color4 color = this.Color;
        if (!this.Enabled && this.FadeWhenDisabled)
          color.A = 0.5f;
        QFont.Begin();
        currentFont.Options.Colour = this.Color;
        currentFont.Print(this.text, width, this.alignment, new Vector2(num2 + (float) this.off_x, num1 + (float) this.off_y));
        QFont.End();
      }
      base.OnRender(host);
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

    [XmlText]
    public string Text
    {
      get
      {
        return this.text;
      }
      set
      {
        if (value.StartsWith("T_"))
          this.text = Locale.GlobalLocale.T(value);
        else
          this.text = value.Replace("\\n", "\n");
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
        this.Color = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
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

    [XmlAttribute("alignment")]
    public QFontAlignment Alignment
    {
      get
      {
        return this.alignment;
      }
      set
      {
        this.alignment = value;
      }
    }

    [XmlAttribute("vertical-alignment")]
    public TextVerticalAlignment VAlignment
    {
      get
      {
        return this.vertical_alignment;
      }
      set
      {
        this.vertical_alignment = value;
      }
    }

    [XmlIgnore]
    public object Value
    {
      set
      {
        if (value == null)
          this.Text = string.Empty;
        else if (value is string)
          this.Text = (string) value;
        else
          this.Text = value.ToString();
      }
      get
      {
        return (object) this.Text;
      }
    }
  }
}
