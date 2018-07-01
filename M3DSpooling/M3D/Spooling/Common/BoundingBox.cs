// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.BoundingBox
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.min.x = min_x;
      this.max.x = max_x;
      this.min.y = min_y;
      this.max.y = max_y;
      this.min.z = min_z;
      this.max.z = max_z;
    }

    public BoundingBox(Vector3D min, Vector3D max)
    {
      this.min = min;
      this.max = max;
    }

    public bool OverLap(BoundingBox other)
    {
      return (double) other.max.x >= (double) this.min.x && (double) other.min.x <= (double) this.max.x && ((double) other.max.y >= (double) this.min.y && (double) other.min.y <= (double) this.max.y) && ((double) other.max.z >= (double) this.max.z && (double) other.min.z <= (double) this.max.z);
    }

    public bool LineIntercepts(out Vector3D intercept, Vector3D p1, Vector3D p2)
    {
      if (this.LineInterceptsMinSide(out intercept, this.min.x - p1.x, p2.x - p1.x, p1, p2) && this.InBoxCheckYZ(intercept) || this.LineInterceptsMaxSide(out intercept, this.max.x - p1.x, p2.x - p1.x, p1, p2) && this.InBoxCheckYZ(intercept) || (this.LineInterceptsMinSide(out intercept, this.min.y - p1.y, p2.y - p1.y, p1, p2) && this.InBoxCheckXZ(intercept) || this.LineInterceptsMaxSide(out intercept, this.max.y - p1.y, p2.y - p1.y, p1, p2) && this.InBoxCheckXZ(intercept)) || (this.LineInterceptsMinSide(out intercept, this.min.z - p1.z, p2.z - p1.z, p1, p2) && this.InBoxCheckXY(intercept) || this.LineInterceptsMaxSide(out intercept, this.max.z - p1.z, p2.z - p1.z, p1, p2) && this.InBoxCheckXY(intercept)))
        return true;
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
      float num = dist1 / dist2;
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
      float num = dist1 / dist2;
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
      return this.ELessThan(p.y, this.max.y) && this.EGreaterThan(p.y, this.min.y) && (this.ELessThan(p.z, this.max.z) && this.EGreaterThan(p.z, this.min.z));
    }

    private bool InBoxCheckXZ(Vector3D p)
    {
      return this.ELessThan(p.x, this.max.x) && this.EGreaterThan(p.x, this.min.x) && (this.ELessThan(p.z, this.max.z) && this.EGreaterThan(p.z, this.min.z));
    }

    private bool InBoxCheckXY(Vector3D p)
    {
      return this.ELessThan(p.x, this.max.x) && this.EGreaterThan(p.x, this.min.x) && (this.ELessThan(p.y, this.max.y) && this.EGreaterThan(p.y, this.min.y));
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

    public bool InRegion(Vector3D p)
    {
      return (double) p.x >= (double) this.min.x && (double) p.x <= (double) this.max.x && ((double) p.y >= (double) this.min.y && (double) p.y <= (double) this.max.y) && ((double) p.z >= (double) this.min.z && (double) p.z <= (double) this.max.z);
    }

    public bool InRegionNaN(Vector3D p)
    {
      return (float.IsNaN(p.x) || (double) p.x >= (double) this.min.x && (double) p.x <= (double) this.max.x) && (float.IsNaN(p.y) || (double) p.y >= (double) this.min.y && (double) p.y <= (double) this.max.y) && (float.IsNaN(p.z) || (double) p.z >= (double) this.min.z && (double) p.z <= (double) this.max.z);
    }

    public int outOfBoundsCheck(float X, float Y, float Z)
    {
      int num = 0;
      if ((double) X < (double) this.min.x)
        num |= 4;
      else if ((double) X > (double) this.max.x)
        num |= 8;
      if ((double) Y < (double) this.min.y)
        num |= 1;
      else if ((double) Y > (double) this.max.y)
        num |= 2;
      if ((double) Z < (double) this.min.z)
        num |= 16;
      else if ((double) Z > (double) this.max.z)
        num |= 32;
      return num;
    }

    public override bool Equals(object obj)
    {
      BoundingBox boundingBox = this;
      BoundingBox other = obj as BoundingBox;
      if ((object) boundingBox == null && (object) other == null)
        return true;
      if ((object) boundingBox == null || (object) other == null)
        return false;
      return boundingBox.Equals(other);
    }

    public bool Equals(BoundingBox other)
    {
      return (double) this.min.x == (double) other.min.x && (double) this.min.y == (double) other.min.y && ((double) this.min.z == (double) other.min.z && (double) this.max.x == (double) other.max.x) && ((double) this.max.y == (double) other.max.y && (double) this.max.z == (double) other.max.z);
    }

    public override int GetHashCode()
    {
      return this.min.x.GetHashCode() ^ this.min.y.GetHashCode() ^ this.min.z.GetHashCode() ^ this.max.x.GetHashCode() ^ this.max.y.GetHashCode() ^ this.max.z.GetHashCode();
    }

    public static bool operator ==(BoundingBox a, BoundingBox b)
    {
      if ((object) a == null && (object) b == null)
        return true;
      if ((object) a == null || (object) b == null)
        return false;
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
