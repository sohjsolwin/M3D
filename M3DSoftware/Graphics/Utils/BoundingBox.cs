// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Utils.BoundingBox
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model.Utils;

namespace M3D.Graphics.Utils
{
  public class BoundingBox
  {
    public Vector3 min;
    public Vector3 max;

    public BoundingBox()
    {
      this.min = new Vector3();
      this.max = new Vector3();
    }

    public BoundingBox(float min_x, float max_x, float min_y, float max_y, float min_z, float max_z)
    {
      this.min = new Vector3(min_x, min_y, min_z);
      this.max = new Vector3(max_x, max_y, max_z);
    }

    public BoundingBox(Vector3 min, Vector3 max)
    {
      this.min = new Vector3(min);
      this.max = new Vector3(max);
    }

    public bool OverLap(BoundingBox other)
    {
      return (double) other.max.x >= (double) this.min.x && (double) other.min.x <= (double) this.max.x && ((double) other.max.y >= (double) this.min.y && (double) other.min.y <= (double) this.max.y) && ((double) other.max.z >= (double) this.max.z && (double) other.min.z <= (double) this.max.z);
    }

    public bool LineIntercepts(out Vector3 intercept, Vector3 p1, Vector3 p2)
    {
      if (this.LineIntercepts(out intercept, this.min.x - p1.x, p2.x - p1.x, p1, p2) && this.InBox(intercept, 0) || this.LineIntercepts(out intercept, this.max.x - p1.x, p2.x - p1.x, p1, p2) && this.InBox(intercept, 0) || (this.LineIntercepts(out intercept, this.min.y - p1.y, p2.y - p1.y, p1, p2) && this.InBox(intercept, 1) || this.LineIntercepts(out intercept, this.max.y - p1.y, p2.y - p1.y, p1, p2) && this.InBox(intercept, 1)) || (this.LineIntercepts(out intercept, this.min.z - p1.z, p2.z - p1.z, p1, p2) && this.InBox(intercept, 2) || this.LineIntercepts(out intercept, this.max.z - p1.z, p2.z - p1.z, p1, p2) && this.InBox(intercept, 2)))
        return true;
      intercept = p2;
      return false;
    }

    private bool LineIntercepts(out Vector3 intercept, float dist1, float dist2, Vector3 p1, Vector3 p2)
    {
      float num = dist1 / dist2;
      if ((double) num > -0.01 && (double) num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if ((double) num >= 0.0 && (double) num <= 1.0)
      {
        intercept = new Vector3((p2.x - p1.x) * num + p1.x, (p2.y - p1.y) * num + p1.y, (p2.z - p1.z) * num + p1.z);
        return true;
      }
      intercept = p2;
      return false;
    }

    private bool InBox(Vector3 p, int axis)
    {
      return axis == 0 && this.ELessThan(p.y, this.max.y) && (this.EGreaterThan(p.y, this.min.y) && this.ELessThan(p.z, this.max.z)) && this.EGreaterThan(p.z, this.min.z) || axis == 1 && this.ELessThan(p.x, this.max.x) && (this.EGreaterThan(p.x, this.min.x) && this.ELessThan(p.z, this.max.z)) && this.EGreaterThan(p.z, this.min.z) || axis == 2 && this.ELessThan(p.y, this.max.y) && (this.EGreaterThan(p.y, this.min.y) && this.ELessThan(p.x, this.max.x)) && this.EGreaterThan(p.x, this.min.x);
    }

    private bool ELessThan(float rhs, float lhs)
    {
      if ((double) rhs >= (double) lhs + 0.00999999977648258)
        return (double) rhs < (double) lhs - 0.00999999977648258;
      return true;
    }

    private bool EGreaterThan(float rhs, float lhs)
    {
      if ((double) rhs <= (double) lhs + 0.00999999977648258)
        return (double) rhs > (double) lhs - 0.00999999977648258;
      return true;
    }

    public bool InRegion(Vector3 p)
    {
      return (double) p.x >= (double) this.min.x && (double) p.x <= (double) this.max.x && ((double) p.y >= (double) this.min.y && (double) p.y <= (double) this.max.y) && ((double) p.z >= (double) this.min.z && (double) p.z <= (double) this.max.z);
    }

    public bool InRegionNaN(Vector3 p)
    {
      return (float.IsNaN(p.x) || (double) p.x >= (double) this.min.x && (double) p.x <= (double) this.max.x) && (float.IsNaN(p.y) || (double) p.y >= (double) this.min.y && (double) p.y <= (double) this.max.y) && (float.IsNaN(p.z) || (double) p.z >= (double) this.min.z && (double) p.z <= (double) this.max.z);
    }
  }
}
