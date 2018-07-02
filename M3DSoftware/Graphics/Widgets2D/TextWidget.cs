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
      : this(0, null)
    {
    }

    public TextWidget(int ID)
      : this(ID, null)
    {
    }

    public TextWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      color = new Color4(0.0f, 0.0f, 0.0f, 1f);
      off_x = 0;
      off_y = 0;
      text = "";
      size = FontSize.Medium;
      alignment = QFontAlignment.Centre;
      vertical_alignment = TextVerticalAlignment.Middle;
      IgnoreMouse = true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.TextWidget;
    }

    public override void OnRender(GUIHost host)
    {
      host.SetCurFontSize(Size);
      QFont currentFont = host.GetCurrentFont();
      if (currentFont != null)
      {
        currentFont.Options.LockToPixel = true;
        var width = (float)Width;
        var height = currentFont.Measure(text, width, alignment).Height;
        var num1 = VAlignment != TextVerticalAlignment.Top ? (VAlignment != TextVerticalAlignment.Bottom ? (float)(Height * 0.5 - height * 0.5) + Y_Abs : Y_Abs + (Height - height)) : Y_Abs;
        var num2 = alignment != QFontAlignment.Centre ? X_Abs : Width * 0.5f + X_Abs;
        Color4 color = Color;
        if (!Enabled && FadeWhenDisabled)
        {
          color.A = 0.5f;
        }

        QFont.Begin();
        currentFont.Options.Colour = Color;
        currentFont.Print(text, width, alignment, new Vector2(num2 + off_x, num1 + off_y));
        QFont.End();
      }
      base.OnRender(host);
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

    [XmlText]
    public string Text
    {
      get
      {
        return text;
      }
      set
      {
        if (value.StartsWith("T_"))
        {
          text = Locale.GlobalLocale.T(value);
        }
        else
        {
          text = value.Replace("\\n", "\n");
        }
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
        Color = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
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

    [XmlAttribute("alignment")]
    public QFontAlignment Alignment
    {
      get
      {
        return alignment;
      }
      set
      {
        alignment = value;
      }
    }

    [XmlAttribute("vertical-alignment")]
    public TextVerticalAlignment VAlignment
    {
      get
      {
        return vertical_alignment;
      }
      set
      {
        vertical_alignment = value;
      }
    }

    [XmlIgnore]
    public object Value
    {
      set
      {
        if (value == null)
        {
          Text = string.Empty;
        }
        else if (value is string)
        {
          Text = (string) value;
        }
        else
        {
          Text = value.ToString();
        }
      }
      get
      {
        return Text;
      }
    }
  }
}
