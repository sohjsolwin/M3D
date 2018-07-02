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
      return other.max.x >= (double)min.x && other.min.x <= (double)max.x && (other.max.y >= (double)min.y && other.min.y <= (double)max.y) && (other.max.z >= (double)max.z && other.min.z <= (double)max.z);
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
      if (dist2 > 0.0)
      {
        intercept = p2;
        return false;
      }
      var num = dist1 / dist2;
      if (num > -0.01 && num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if (num > 0.0 && num <= 1.0)
      {
        intercept = new Vector3D((p2.x - p1.x) * num + p1.x, (p2.y - p1.y) * num + p1.y, (p2.z - p1.z) * num + p1.z);
        return true;
      }
      if (num >= 0.0 && num < 1.40129846432482E-45 && dist2 < 0.0)
      {
        intercept = p1;
        return true;
      }
      intercept = p2;
      return false;
    }

    private bool LineInterceptsMaxSide(out Vector3D intercept, float dist1, float dist2, Vector3D p1, Vector3D p2)
    {
      if (dist2 < 0.0)
      {
        intercept = p2;
        return false;
      }
      var num = dist1 / dist2;
      if (num > -0.01 && num < 0.0)
      {
        intercept = p1;
        return true;
      }
      if (num > 0.0 && num <= 1.0)
      {
        intercept = new Vector3D((p2.x - p1.x) * num + p1.x, (p2.y - p1.y) * num + p1.y, (p2.z - p1.z) * num + p1.z);
        return true;
      }
      if (num >= 0.0 && num < 1.40129846432482E-45 && dist2 > 0.0)
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

    public bool InRegion(Vector3D p)
    {
      return p.x >= (double)min.x && p.x <= (double)max.x && (p.y >= (double)min.y && p.y <= (double)max.y) && (p.z >= (double)min.z && p.z <= (double)max.z);
    }

    public bool InRegionNaN(Vector3D p)
    {
      return (float.IsNaN(p.x) || p.x >= (double)min.x && p.x <= (double)max.x) && (float.IsNaN(p.y) || p.y >= (double)min.y && p.y <= (double)max.y) && (float.IsNaN(p.z) || p.z >= (double)min.z && p.z <= (double)max.z);
    }

    public int OutOfBoundsCheck(float X, float Y, float Z)
    {
      var num = 0;
      if (X < (double)min.x)
      {
        num |= 4;
      }
      else if (X > (double)max.x)
      {
        num |= 8;
      }

      if (Y < (double)min.y)
      {
        num |= 1;
      }
      else if (Y > (double)max.y)
      {
        num |= 2;
      }

      if (Z < (double)min.z)
      {
        num |= 16;
      }
      else if (Z > (double)max.z)
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
      return min.x == (double)other.min.x && min.y == (double)other.min.y && (min.z == (double)other.min.z && max.x == (double)other.max.x) && (max.y == (double)other.max.y && max.z == (double)other.max.z);
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
