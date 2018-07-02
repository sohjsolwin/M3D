using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace M3D.Graphics.Ext3D
{
  public class LightNode : Element3D
  {
    public Vector4 Position = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    public Color4 Ambient = new Color4(0.2f, 0.2f, 0.2f, 1f);
    public Color4 Diffuse = new Color4(1f, 1f, 1f, 1f);
    public Color4 Specular = new Color4(1f, 1f, 1f, 1f);
    public int LightIndex;
    public bool LightOnlyChildren;

    public LightNode()
      : this(0, null)
    {
    }

    public LightNode(int ID)
      : this(ID, null)
    {
    }

    public LightNode(int ID, Element3D parent)
      : base(ID, parent)
    {
    }

    public LightNode(int ID, int LightIndex, Vector4 Position, Color4 Ambient, Color4 Diffuse, Color4 Specular)
      : this(ID)
    {
      this.Position = Position;
      this.Ambient = Ambient;
      this.Diffuse = Diffuse;
      this.Specular = Specular;
      this.LightIndex = LightIndex;
    }

    public override void Render3D()
    {
      if (LightIndex >= 0 && LightIndex <= 4 && Enabled)
      {
        GL.Enable((EnableCap) (16384 + LightIndex));
        GL.Light((LightName) (16384 + LightIndex), LightParameter.Position, Position);
        GL.Light((LightName) (16384 + LightIndex), LightParameter.Ambient, Ambient);
        GL.Light((LightName) (16384 + LightIndex), LightParameter.Diffuse, Diffuse);
        GL.Light((LightName) (16384 + LightIndex), LightParameter.Specular, Specular);
      }
      base.Render3D();
      if (!LightOnlyChildren || LightIndex < 0 || (LightIndex > 4 || !Enabled))
      {
        return;
      }

      GL.Disable((EnableCap) (16384 + LightIndex));
    }
  }
}
