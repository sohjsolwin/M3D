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
      return (double) other.max.x >= (double)min.x && (double) other.min.x <= (double)max.x && ((double) other.max.y >= (double)min.y && (double) other.min.y <= (double)max.y) && ((double) other.max.z >= (double)max.z && (double) other.min.z <= (double)max.z);
    }

    public bool LineIntercepts(out Vector3 intercept, Vector3 p1, Vector3 p2)
    {
      if (LineIntercepts(out intercept, min.x - p1.x, p2.x - p1.x, p1, p2) && InBox(intercept, 0) || LineIntercepts(out intercept, max.x - p1.x, p2.x - p1.x, p1, p2) && InBox(intercept, 0) || (LineIntercepts(out intercept, min.y - p1.y, p2.y - p1.y, p1, p2) && InBox(intercept, 1) || LineIntercepts(out intercept, max.y - p1.y, p2.y - p1.y, p1, p2) && InBox(intercept, 1)) || (LineIntercepts(out intercept, min.z - p1.z, p2.z - p1.z, p1, p2) && InBox(intercept, 2) || LineIntercepts(out intercept, max.z - p1.z, p2.z - p1.z, p1, p2) && InBox(intercept, 2)))
      {
        return true;
      }

      intercept = p2;
      return false;
    }

    private bool LineIntercepts(out Vector3 intercept, float dist1, float dist2, Vector3 p1, Vector3 p2)
    {
      var num = dist1 / dist2;
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
      return axis == 0 && ELessThan(p.y, max.y) && (EGreaterThan(p.y, min.y) && ELessThan(p.z, max.z)) && EGreaterThan(p.z, min.z) || axis == 1 && ELessThan(p.x, max.x) && (EGreaterThan(p.x, min.x) && ELessThan(p.z, max.z)) && EGreaterThan(p.z, min.z) || axis == 2 && ELessThan(p.y, max.y) && (EGreaterThan(p.y, min.y) && ELessThan(p.x, max.x)) && EGreaterThan(p.x, min.x);
    }

    private bool ELessThan(float rhs, float lhs)
    {
      if ((double) rhs >= (double) lhs + 0.00999999977648258)
      {
        return (double) rhs < (double) lhs - 0.00999999977648258;
      }

      return true;
    }

    private bool EGreaterThan(float rhs, float lhs)
    {
      if ((double) rhs <= (double) lhs + 0.00999999977648258)
      {
        return (double) rhs > (double) lhs - 0.00999999977648258;
      }

      return true;
    }

    public bool InRegion(Vector3 p)
    {
      return (double) p.x >= (double)min.x && (double) p.x <= (double)max.x && ((double) p.y >= (double)min.y && (double) p.y <= (double)max.y) && ((double) p.z >= (double)min.z && (double) p.z <= (double)max.z);
    }

    public bool InRegionNaN(Vector3 p)
    {
      return (float.IsNaN(p.x) || (double) p.x >= (double)min.x && (double) p.x <= (double)max.x) && (float.IsNaN(p.y) || (double) p.y >= (double)min.y && (double) p.y <= (double)max.y) && (float.IsNaN(p.z) || (double) p.z >= (double)min.z && (double) p.z <= (double)max.z);
    }
  }
}
