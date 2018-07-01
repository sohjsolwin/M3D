// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.Frame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(ID, (Element2D) null)
    {
    }

    public Frame(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.bordercolor = new Color4((byte) 0, (byte) 0, (byte) 0, (byte) 0);
      this.bgcolor = new Color4((byte) 0, (byte) 0, (byte) 0, (byte) 0);
      this.Clipping = false;
    }

    public virtual void Close()
    {
    }

    public override bool ContainsPoint(int x, int y)
    {
      if (this.always_contains_point)
        return true;
      return base.ContainsPoint(x, y);
    }

    public override ElementType GetElementType()
    {
      return ElementType.Frame;
    }

    public override void OnRender(GUIHost host)
    {
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      if (this.DarkenBackground)
      {
        Simple2DRenderer.Quad quad;
        quad.x0 = 0.0f;
        quad.x1 = (float) simpleRenderer.WindowWidth;
        quad.y0 = 0.0f;
        quad.y1 = (float) simpleRenderer.WindowHeight;
        quad.color = new Color4(0.0f, 0.0f, 0.0f, 0.5f);
        simpleRenderer.DrawQuad(quad);
      }
      if (simpleRenderer == null)
        return;
      if (this.Clipping)
        simpleRenderer.PushClipping(new Simple2DRenderer.Region(this.X_Abs, this.Y_Abs, this.X_Abs + this.Width, this.Y_Abs + this.Height));
      Simple2DRenderer.Quad quad1;
      quad1.x0 = (float) this.X_Abs;
      quad1.y0 = (float) this.Y_Abs;
      quad1.x1 = (float) (this.X_Abs + this.Width);
      quad1.y1 = (float) (this.Y_Abs + this.Height);
      quad1.color = this.bgcolor;
      if ((double) this.bordercolor.A > 0.0)
      {
        quad1.x0 = (float) (this.X_Abs + 1);
        quad1.y0 = (float) (this.Y_Abs + 1);
        quad1.x1 = (float) (this.X_Abs + this.Width - 1);
        quad1.y1 = (float) (this.Y_Abs + this.Height - 1);
        quad1.color = this.bgcolor;
      }
      if ((double) this.bgcolor.A > 0.0)
      {
        quad1.color = this.bgcolor;
        simpleRenderer.DrawQuad(quad1);
      }
      if ((double) this.bordercolor.A > 0.0)
      {
        quad1.color = this.bordercolor;
        simpleRenderer.DrawQuadLine(quad1);
      }
      base.OnRender(host);
      if (!this.Clipping)
        return;
      simpleRenderer.PopClipping();
    }

    [XmlAttribute("background-color")]
    public string HexBackgroundColor
    {
      get
      {
        return this.hexBackgroundColor;
      }
      set
      {
        this.hexBackgroundColor = value;
        this.BGColor = IElement.GenerateColorFromHtml(this.hexBackgroundColor);
      }
    }

    public Color4 BGColor
    {
      get
      {
        return this.bgcolor;
      }
      set
      {
        this.bgcolor = value;
      }
    }

    [XmlAttribute("border-color")]
    public string HexBorderColor
    {
      get
      {
        return this.hexBorderColor;
      }
      set
      {
        this.hexBorderColor = value;
        this.BorderColor = IElement.GenerateColorFromHtml(this.hexBorderColor);
      }
    }

    [XmlIgnore]
    public Color4 BorderColor
    {
      get
      {
        return this.bordercolor;
      }
      set
      {
        this.bordercolor = value;
      }
    }

    [XmlAttribute("darken-background")]
    public bool DarkenBackground { get; set; }
  }
}
