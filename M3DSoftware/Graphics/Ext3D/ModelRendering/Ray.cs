// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.Ray
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model.Utils;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class Ray
  {
    private Vector3 position;
    private Vector3 direction;

    public Ray(Vector3 position, Vector3 direction)
    {
      this.position = new Vector3(position);
      this.direction = new Vector3(direction);
    }

    public Vector3 Position
    {
      get
      {
        return position;
      }
    }

    public Vector3 Direction
    {
      get
      {
        return direction;
      }
    }
  }
}
