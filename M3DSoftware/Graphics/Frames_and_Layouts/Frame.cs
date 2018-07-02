using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class Frame : Element2D
  {
    private string hexBackgroundColor = "#FF808080";
    private string hexBorderColor = "#FF808080";
    private Color4 bgcolor;
    private Color4 bordercolor;
    [XmlAttribute("Clipping")]
    public bool Clipping;
    [XmlIgnore]
    public bool always_contains_point;

    public Frame()
      : this(0)
    {
    }

    public Frame(int ID)
      : this(ID, null)
    {
    }

    public Frame(int ID, Element2D parent)
      : base(ID, parent)
    {
      bordercolor = new Color4(0, 0, 0, 0);
      bgcolor = new Color4(0, 0, 0, 0);
      Clipping = false;
    }

    public virtual void Close()
    {
    }

    public override bool ContainsPoint(int x, int y)
    {
      if (always_contains_point)
      {
        return true;
      }

      return base.ContainsPoint(x, y);
    }

    public override ElementType GetElementType()
    {
      return ElementType.Frame;
    }

    public override void OnRender(GUIHost host)
    {
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      if (DarkenBackground)
      {
        Simple2DRenderer.Quad quad;
        quad.x0 = 0.0f;
        quad.x1 = simpleRenderer.WindowWidth;
        quad.y0 = 0.0f;
        quad.y1 = simpleRenderer.WindowHeight;
        quad.color = new Color4(0.0f, 0.0f, 0.0f, 0.5f);
        simpleRenderer.DrawQuad(quad);
      }
      if (simpleRenderer == null)
      {
        return;
      }

      if (Clipping)
      {
        simpleRenderer.PushClipping(new Simple2DRenderer.Region(X_Abs, Y_Abs, X_Abs + Width, Y_Abs + Height));
      }

      Simple2DRenderer.Quad quad1;
      quad1.x0 = X_Abs;
      quad1.y0 = Y_Abs;
      quad1.x1 = X_Abs + Width;
      quad1.y1 = Y_Abs + Height;
      quad1.color = bgcolor;
      if (bordercolor.A > 0.0)
      {
        quad1.x0 = X_Abs + 1;
        quad1.y0 = Y_Abs + 1;
        quad1.x1 = X_Abs + Width - 1;
        quad1.y1 = Y_Abs + Height - 1;
        quad1.color = bgcolor;
      }
      if (bgcolor.A > 0.0)
      {
        quad1.color = bgcolor;
        simpleRenderer.DrawQuad(quad1);
      }
      if (bordercolor.A > 0.0)
      {
        quad1.color = bordercolor;
        simpleRenderer.DrawQuadLine(quad1);
      }
      base.OnRender(host);
      if (!Clipping)
      {
        return;
      }

      simpleRenderer.PopClipping();
    }

    [XmlAttribute("background-color")]
    public string HexBackgroundColor
    {
      get
      {
        return hexBackgroundColor;
      }
      set
      {
        hexBackgroundColor = value;
        BGColor = IElement.GenerateColorFromHtml(hexBackgroundColor);
      }
    }

    public Color4 BGColor
    {
      get
      {
        return bgcolor;
      }
      set
      {
        bgcolor = value;
      }
    }

    [XmlAttribute("border-color")]
    public string HexBorderColor
    {
      get
      {
        return hexBorderColor;
      }
      set
      {
        hexBorderColor = value;
        BorderColor = IElement.GenerateColorFromHtml(hexBorderColor);
      }
    }

    [XmlIgnore]
    public Color4 BorderColor
    {
      get
      {
        return bordercolor;
      }
      set
      {
        bordercolor = value;
      }
    }

    [XmlAttribute("darken-background")]
    public bool DarkenBackground { get; set; }
  }
}
