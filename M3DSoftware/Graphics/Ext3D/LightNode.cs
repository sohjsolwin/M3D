// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.LightNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : this(0, (Element3D) null)
    {
    }

    public LightNode(int ID)
      : this(ID, (Element3D) null)
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
      if (this.LightIndex >= 0 && this.LightIndex <= 4 && this.Enabled)
      {
        GL.Enable((EnableCap) (16384 + this.LightIndex));
        GL.Light((LightName) (16384 + this.LightIndex), LightParameter.Position, this.Position);
        GL.Light((LightName) (16384 + this.LightIndex), LightParameter.Ambient, this.Ambient);
        GL.Light((LightName) (16384 + this.LightIndex), LightParameter.Diffuse, this.Diffuse);
        GL.Light((LightName) (16384 + this.LightIndex), LightParameter.Specular, this.Specular);
      }
      base.Render3D();
      if (!this.LightOnlyChildren || this.LightIndex < 0 || (this.LightIndex > 4 || !this.Enabled))
        return;
      GL.Disable((EnableCap) (16384 + this.LightIndex));
    }
  }
}
