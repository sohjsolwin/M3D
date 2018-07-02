using System;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class BoundingBox
  {
    [XmlElement]
    public Vector3D min;
    [XmlElement]
    public Vector3D max;

    public BoundingBox()
    {
    }

    public BoundingBox(float min_x, float max_x, float min_y, float max_y, float min_z, float max_z)
    {
      min.x = min_x;
      max.x = max_x;
      min.y = min_y;
      max.y = max_y;
      min.z = min_z;
      max.z = max_z;
    }

    public BoundingBox(Vector3D min, Vector3D max)
    {
      this.min = min;
      this.max = max;
    }

    public bool OverLap(BoundingBox other)
    {
      return (double) other.max.x >= (double)min.x && (double) other.min.x <= (double)max.x && ((double) other.max.y >= (double)min.y && (double) other.min.y <= (double)max.y) && ((double) other.max.z >= (double)max.z && (double) other.min.z <= (double)max.z);
    }

    public bool LineIntercepts(out Vector3D intercept, Vector3D p1, Vector3D p2)
    {
      if (LineInterceptsMinSide(out intercept, min.x - p1.x, p2.x - p1.x, p1, p2) && InBoxCheckYZ(intercept) || LineInterceptsMaxSide(out intercept, max.x - p1.x, p2.x - p1.x, p1, p2) && InBoxCheckYZ(intercept) || (LineInterceptsMinSide(out intercept, min.y - p1.y, p2.y - p1.y, p1, p2) && InBoxCheckXZ(intercept) || LineInterceptsMaxSide(out intercept, max.y - p1.y, p2.y - p1.y, p1, p2) && InBoxCheckXZ(intercept)) || (LineInterceptsMinSide(out intercept, min.z - p1.z, p2.z - p1.z, p1, p2) && InBoxCheckXY(intercept) || LineInterceptsMaxSide(out intercept, max.z - p1.z, p2.z - p1.z, p1, p2) && InBoxCheckXY(intercept)))
      {
        return true;
      }

      intercept = p2;
      return false;
    }

    private bool LineInterceptsMinSide(out Vector3D intercept, float dist1, float dist2, Vector3D p1, Vector3D p2)
    {
      if ((double) dist2 > 0.0)
      {
        intercept = p2;
        return false;
      }
      var num = dist1 / dist2;
      if ((double) num > -0.01 && (double) num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if ((double) num > 0.0 && (double) num <= 1.0)
      {
        intercept = new Vector3D((p2.x - p1.x) * num + p1.x, (p2.y - p1.y) * num + p1.y, (p2.z - p1.z) * num + p1.z);
        return true;
      }
      if ((double) num >= 0.0 && (double) num < 1.40129846432482E-45 && (double) dist2 < 0.0)
      {
        intercept = p1;
        return true;
      }
      intercept = p2;
      return false;
    }

    private bool LineInterceptsMaxSide(out Vector3D intercept, float dist1, float dist2, Vector3D p1, Vector3D p2)
    {
      if ((double) dist2 < 0.0)
      {
        intercept = p2;
        return false;
      }
      var num = dist1 / dist2;
      if ((double) num > -0.01 && (double) num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if ((double) num > 0.0 && (double) num <= 1.0)
      {
        intercept = new Vector3D((p2.x - p1.x) * num + p1.x, (p2.y - p1.y) * num + p1.y, (p2.z - p1.z) * num + p1.z);
        return true;
      }
      if ((double) num >= 0.0 && (double) num < 1.40129846432482E-45 && (double) dist2 > 0.0)
      {
        intercept = p1;
        return true;
      }
      intercept = p2;
      return false;
    }

    private bool InBoxCheckYZ(Vector3D p)
    {
      return ELessThan(p.y, max.y) && EGreaterThan(p.y, min.y) && (ELessThan(p.z, max.z) && EGreaterThan(p.z, min.z));
    }

    private bool InBoxCheckXZ(Vector3D p)
    {
      return ELessThan(p.x, max.x) && EGreaterThan(p.x, min.x) && (ELessThan(p.z, max.z) && EGreaterThan(p.z, min.z));
    }

    private bool InBoxCheckXY(Vector3D p)
    {
      return ELessThan(p.x, max.x) && EGreaterThan(p.x, min.x) && (ELessThan(p.y, max.y) && EGreaterThan(p.y, min.y));
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

    public bool InRegion(Vector3D p)
    {
      return (double) p.x >= (double)min.x && (double) p.x <= (double)max.x && ((double) p.y >= (double)min.y && (double) p.y <= (double)max.y) && ((double) p.z >= (double)min.z && (double) p.z <= (double)max.z);
    }

    public bool InRegionNaN(Vector3D p)
    {
      return (float.IsNaN(p.x) || (double) p.x >= (double)min.x && (double) p.x <= (double)max.x) && (float.IsNaN(p.y) || (double) p.y >= (double)min.y && (double) p.y <= (double)max.y) && (float.IsNaN(p.z) || (double) p.z >= (double)min.z && (double) p.z <= (double)max.z);
    }

    public int outOfBoundsCheck(float X, float Y, float Z)
    {
      var num = 0;
      if ((double) X < (double)min.x)
      {
        num |= 4;
      }
      else if ((double) X > (double)max.x)
      {
        num |= 8;
      }

      if ((double) Y < (double)min.y)
      {
        num |= 1;
      }
      else if ((double) Y > (double)max.y)
      {
        num |= 2;
      }

      if ((double) Z < (double)min.z)
      {
        num |= 16;
      }
      else if ((double) Z > (double)max.z)
      {
        num |= 32;
      }

      return num;
    }

    public override bool Equals(object obj)
    {
      BoundingBox boundingBox = this;
      var other = obj as BoundingBox;
      if ((object) boundingBox == null && (object) other == null)
      {
        return true;
      }

      if ((object) boundingBox == null || (object) other == null)
      {
        return false;
      }

      return boundingBox.Equals(other);
    }

    public bool Equals(BoundingBox other)
    {
      return (double)min.x == (double) other.min.x && (double)min.y == (double) other.min.y && ((double)min.z == (double) other.min.z && (double)max.x == (double) other.max.x) && ((double)max.y == (double) other.max.y && (double)max.z == (double) other.max.z);
    }

    public override int GetHashCode()
    {
      return min.x.GetHashCode() ^ min.y.GetHashCode() ^ min.z.GetHashCode() ^ max.x.GetHashCode() ^ max.y.GetHashCode() ^ max.z.GetHashCode();
    }

    public static bool operator ==(BoundingBox a, BoundingBox b)
    {
      if ((object) a == null && (object) b == null)
      {
        return true;
      }

      if ((object) a == null || (object) b == null)
      {
        return false;
      }

      return a.Equals(b);
    }

    public static bool operator !=(BoundingBox a, BoundingBox b)
    {
      return !a.Equals(b);
    }

    [Flags]
    public enum BoundarySide
    {
      None = 0,
      Front = 1,
      Back = 2,
      Left = 4,
      Right = 8,
      Bottom = 16, // 0x00000010
      Top = 32, // 0x00000020
    }
  }
}
