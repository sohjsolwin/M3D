using M3D.Model.Utils;

namespace M3D.Graphics.Utils
{
  public class BoundingBox
  {
    public Vector3 min;
    public Vector3 max;

    public BoundingBox()
    {
      min = new Vector3();
      max = new Vector3();
    }

    public BoundingBox(float min_x, float max_x, float min_y, float max_y, float min_z, float max_z)
    {
      min = new Vector3(min_x, min_y, min_z);
      max = new Vector3(max_x, max_y, max_z);
    }

    public BoundingBox(Vector3 min, Vector3 max)
    {
      this.min = new Vector3(min);
      this.max = new Vector3(max);
    }

    public bool OverLap(BoundingBox other)
    {
      return other.max.X >= (double)min.X && other.min.X <= (double)max.X && (other.max.Y >= (double)min.Y && other.min.Y <= (double)max.Y) && (other.max.Z >= (double)max.Z && other.min.Z <= (double)max.Z);
    }

    public bool LineIntercepts(out Vector3 intercept, Vector3 p1, Vector3 p2)
    {
      if (LineIntercepts(out intercept, min.X - p1.X, p2.X - p1.X, p1, p2) && InBox(intercept, 0) || LineIntercepts(out intercept, max.X - p1.X, p2.X - p1.X, p1, p2) && InBox(intercept, 0) || (LineIntercepts(out intercept, min.Y - p1.Y, p2.Y - p1.Y, p1, p2) && InBox(intercept, 1) || LineIntercepts(out intercept, max.Y - p1.Y, p2.Y - p1.Y, p1, p2) && InBox(intercept, 1)) || (LineIntercepts(out intercept, min.Z - p1.Z, p2.Z - p1.Z, p1, p2) && InBox(intercept, 2) || LineIntercepts(out intercept, max.Z - p1.Z, p2.Z - p1.Z, p1, p2) && InBox(intercept, 2)))
      {
        return true;
      }

      intercept = p2;
      return false;
    }

    private bool LineIntercepts(out Vector3 intercept, float dist1, float dist2, Vector3 p1, Vector3 p2)
    {
      var num = dist1 / dist2;
      if (num > -0.01 && num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if (num >= 0.0 && num <= 1.0)
      {
        intercept = new Vector3((p2.X - p1.X) * num + p1.X, (p2.Y - p1.Y) * num + p1.Y, (p2.Z - p1.Z) * num + p1.Z);
        return true;
      }
      intercept = p2;
      return false;
    }

    private bool InBox(Vector3 p, int axis)
    {
      return axis == 0 && ELessThan(p.Y, max.Y) && (EGreaterThan(p.Y, min.Y) && ELessThan(p.Z, max.Z)) && EGreaterThan(p.Z, min.Z) || axis == 1 && ELessThan(p.X, max.X) && (EGreaterThan(p.X, min.X) && ELessThan(p.Z, max.Z)) && EGreaterThan(p.Z, min.Z) || axis == 2 && ELessThan(p.Y, max.Y) && (EGreaterThan(p.Y, min.Y) && ELessThan(p.X, max.X)) && EGreaterThan(p.X, min.X);
    }

    private bool ELessThan(float rhs, float lhs)
    {
      if (rhs >= lhs + 0.00999999977648258)
      {
        return rhs < lhs - 0.00999999977648258;
      }

      return true;
    }

    private bool EGreaterThan(float rhs, float lhs)
    {
      if (rhs <= lhs + 0.00999999977648258)
      {
        return rhs > lhs - 0.00999999977648258;
      }

      return true;
    }

    public bool InRegion(Vector3 p)
    {
      return p.X >= (double)min.X && p.X <= (double)max.X && (p.Y >= (double)min.Y && p.Y <= (double)max.Y) && (p.Z >= (double)min.Z && p.Z <= (double)max.Z);
    }

    public bool InRegionNaN(Vector3 p)
    {
      return (float.IsNaN(p.X) || p.X >= (double)min.X && p.X <= (double)max.X) && (float.IsNaN(p.Y) || p.Y >= (double)min.Y && p.Y <= (double)max.Y) && (float.IsNaN(p.Z) || p.Z >= (double)min.Z && p.Z <= (double)max.Z);
    }
  }
}
