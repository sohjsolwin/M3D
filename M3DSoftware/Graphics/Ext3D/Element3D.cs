// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.Element3D
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;

namespace M3D.Graphics.Ext3D
{
  public class Element3D : IElement
  {
    [XmlElement("Model3D", Type = typeof (Model3DNode))]
    public List<Element3D> child_list3D;
    private Element3D parent;

    public Element3D()
      : this(0, (Element3D) null)
    {
    }

    public Element3D(int ID)
      : this(ID, (Element3D) null)
    {
    }

    public Element3D(int ID, Element3D parent)
      : base(ID, (IElement) parent)
    {
      this.parent = parent;
      child_list3D = new List<Element3D>();
    }

    public virtual void Render3D()
    {
      lock (child_list3D)
      {
        foreach (Element3D element3D in child_list3D)
        {
          if (element3D.Visible)
          {
            element3D.Render3D();
          }
        }
      }
      OnRender();
    }

    public virtual void AddChildElement(Element3D child)
    {
      child.parent = this;
      child.SetBaseParent((IElement) this);
      lock (child_list3D)
      {
        child_list3D.Add(child);
      }
    }

    public virtual void RemoveChildElement(Element3D child)
    {
      if (child == null)
      {
        return;
      }

      child.parent = (Element3D) null;
      if (child_list3D == null)
      {
        return;
      }

      lock (child_list3D)
      {
        child_list3D.Remove(child);
      }
    }

    public Element3D Parent
    {
      get
      {
        return parent;
      }
    }

    public static void CreateTexture(ref int texture, Bitmap bitmap)
    {
      if (texture == 0)
      {
        GL.GenTextures(1, out texture);
      }

      GL.BindTexture(TextureTarget.Texture2D, texture);
      BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapdata.Width, bitmapdata.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
      bitmap.UnlockBits(bitmapdata);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9729);
      GL.BindTexture(TextureTarget.Texture2D, 0);
    }
  }
}
