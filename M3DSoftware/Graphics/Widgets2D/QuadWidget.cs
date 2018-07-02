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
