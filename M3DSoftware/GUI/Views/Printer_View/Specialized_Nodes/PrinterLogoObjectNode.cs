// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Specialized_Nodes.PrinterLogoObjectNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.Model.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace M3D.GUI.Views.Printer_View.Specialized_Nodes
{
  public class PrinterLogoObjectNode : Element3D
  {
    private FrontFacingPlaneNode plate;
    private FrontFacingPlaneNode logo;

    public PrinterLogoObjectNode(string texture_name, Vector3 position, Vector3 size)
      : this(texture_name, 0, (Element3D) null, position, size)
    {
    }

    public PrinterLogoObjectNode(string texture_name, int ID, Vector3 position, Vector3 size)
      : this(texture_name, ID, (Element3D) null, position, size)
    {
    }

    public PrinterLogoObjectNode(string texture_name, int ID, Element3D parent, Vector3 position, Vector3 size)
      : base(ID, parent)
    {
      int texture = 0;
      Bitmap bitmap = new Bitmap(texture_name);
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      this.plate = new FrontFacingPlaneNode(ID, (Element3D) this);
      this.logo = new FrontFacingPlaneNode(ID, (Element3D) this);
      this.plate.Create(position, size.x, size.z, 0);
      this.plate.Emission = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
      this.logo.Create(position, size.x - 1f, size.z - 1f, texture);
      this.logo.Emission = new Color4(1f, 1f, 1f, 1f);
    }

    public override void Render3D()
    {
      GL.DepthMask(true);
      this.plate.Render3D();
      GL.PushMatrix();
      GL.Translate(0.0, -0.1, 0.0);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
      this.logo.Render3D();
      GL.PopMatrix();
      base.Render3D();
    }

    public void Rescale(float x, float y, float z)
    {
      this.plate.Rescale(x, y, z);
      this.logo.Rescale(x, y, z);
    }

    public Color4 Ambient
    {
      get
      {
        return this.plate.Ambient;
      }
      set
      {
        this.plate.Ambient = value;
      }
    }

    public Color4 Diffuse
    {
      get
      {
        return this.plate.Diffuse;
      }
      set
      {
        this.plate.Diffuse = value;
      }
    }

    public Color4 Specular
    {
      get
      {
        return this.plate.Specular;
      }
      set
      {
        this.plate.Specular = value;
      }
    }

    public float Shininess
    {
      get
      {
        return this.plate.Shininess;
      }
      set
      {
        this.plate.Shininess = value;
      }
    }
  }
}
