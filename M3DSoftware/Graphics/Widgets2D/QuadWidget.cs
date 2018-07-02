// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.QuadWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;

namespace M3D.Graphics.Widgets2D
{
  public class QuadWidget : Element2D
  {
    public Color4 Color;

    public QuadWidget()
      : this(0)
    {
    }

    public QuadWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public QuadWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      IgnoreMouse = true;
    }

    public override void OnRender(GUIHost host)
    {
      Simple2DRenderer.Quad quad;
      quad.x0 = (float)X_Abs;
      quad.y0 = (float)Y_Abs;
      quad.x1 = (float) (X_Abs + Width);
      quad.y1 = (float) (Y_Abs + Height);
      quad.color = Color;
      host.GetSimpleRenderer().DrawQuad(quad);
    }
  }
}
