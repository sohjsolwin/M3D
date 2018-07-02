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
      var texture = 0;
      var bitmap = new Bitmap(texture_name);
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      plate = new FrontFacingPlaneNode(ID, (Element3D) this);
      logo = new FrontFacingPlaneNode(ID, (Element3D) this);
      plate.Create(position, size.x, size.z, 0);
      plate.Emission = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
      logo.Create(position, size.x - 1f, size.z - 1f, texture);
      logo.Emission = new Color4(1f, 1f, 1f, 1f);
    }

    public override void Render3D()
    {
      GL.DepthMask(true);
      plate.Render3D();
      GL.PushMatrix();
      GL.Translate(0.0, -0.1, 0.0);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
      logo.Render3D();
      GL.PopMatrix();
      base.Render3D();
    }

    public void Rescale(float x, float y, float z)
    {
      plate.Rescale(x, y, z);
      logo.Rescale(x, y, z);
    }

    public Color4 Ambient
    {
      get
      {
        return plate.Ambient;
      }
      set
      {
        plate.Ambient = value;
      }
    }

    public Color4 Diffuse
    {
      get
      {
        return plate.Diffuse;
      }
      set
      {
        plate.Diffuse = value;
      }
    }

    public Color4 Specular
    {
      get
      {
        return plate.Specular;
      }
      set
      {
        plate.Specular = value;
      }
    }

    public float Shininess
    {
      get
      {
        return plate.Shininess;
      }
      set
      {
        plate.Shininess = value;
      }
    }
  }
}
