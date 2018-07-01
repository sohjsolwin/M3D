// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.StackedBoundingBox
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class StackedBoundingBox
  {
    [XmlElement]
    public List<BoundingBox> bounds_list;
    [XmlAttribute]
    public float min_z;
    [XmlAttribute]
    public float max_z;

    public StackedBoundingBox()
    {
      this.bounds_list = new List<BoundingBox>();
    }

    public StackedBoundingBox(StackedBoundingBox other)
    {
      this.bounds_list = new List<BoundingBox>((IEnumerable<BoundingBox>) other.bounds_list);
      this.min_z = other.min_z;
      this.max_z = other.max_z;
    }

    public StackedBoundingBox(params BoundingBox[] regions)
    {
      if (regions == null || regions.Length < 1)
        throw new ArgumentException("Must include at least one region.");
      this.bounds_list = new List<BoundingBox>((IEnumerable<BoundingBox>) regions);
      this.min_z = this.bounds_list[0].min.z;
      this.max_z = this.bounds_list[0].max.z;
      foreach (BoundingBox bounds in this.bounds_list)
      {
        if ((double) bounds.max.z < (double) this.max_z)
          throw new ArgumentException("Bounding boxes must be in order from Z min to Z max.");
        this.max_z = bounds.max.z;
      }
    }

    public bool OverLap(BoundingBox other)
    {
      foreach (BoundingBox bounds in this.bounds_list)
      {
        if (other.OverLap(bounds))
          return true;
      }
      return false;
    }

    public bool InRegion(Vector3D p)
    {
      foreach (BoundingBox bounds in this.bounds_list)
      {
        if (bounds.InRegion(p))
          return true;
      }
      return false;
    }

    public bool InRegionNaN(Vector3D p)
    {
      foreach (BoundingBox bounds in this.bounds_list)
      {
        if (bounds.InRegionNaN(p))
          return true;
      }
      return false;
    }

    public bool LineIntercepts(out Vector3D intercept, Vector3D p1, Vector3D p2)
    {
      for (int index1 = 0; index1 < this.bounds_list.Count; ++index1)
      {
        if (this.bounds_list[index1].LineIntercepts(out intercept, p1, p2))
        {
          bool flag = false;
          for (int index2 = 0; index2 < this.bounds_list.Count; ++index2)
          {
            if (index2 != index1)
              flag |= this.bounds_list[index2].InRegion(intercept);
          }
          if (!flag)
            return true;
        }
      }
      intercept = p2;
      return false;
    }

    public int outOfBoundsCheck(float X, float Y, float Z)
    {
      if ((double) Z < (double) this.min_z)
        return 16;
      if ((double) Z > (double) this.max_z)
        return 32;
      foreach (BoundingBox bounds in this.bounds_list)
      {
        if ((double) Z <= (double) bounds.max.z && (double) Z >= (double) bounds.min.z)
          return bounds.outOfBoundsCheck(X, Y, Z);
      }
      return 0;
    }

    public override bool Equals(object obj)
    {
      StackedBoundingBox stackedBoundingBox = this;
      StackedBoundingBox other = obj as StackedBoundingBox;
      if ((object) stackedBoundingBox == null && (object) other == null)
        return true;
      if ((object) stackedBoundingBox == null || (object) other == null)
        return false;
      return stackedBoundingBox.Equals(other);
    }

    public bool Equals(StackedBoundingBox other)
    {
      if (this.bounds_list.Count != other.bounds_list.Count || (double) this.min_z != (double) other.min_z || (double) this.max_z != (double) other.max_z)
        return false;
      for (int index = 0; index < this.bounds_list.Count; ++index)
      {
        if (this.bounds_list[index] != other.bounds_list[index])
          return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      int num = this.min_z.GetHashCode() ^ this.max_z.GetHashCode();
      for (int index = 0; index < this.bounds_list.Count; ++index)
        num ^= this.bounds_list[index].GetHashCode();
      return num;
    }

    public static bool operator ==(StackedBoundingBox a, StackedBoundingBox b)
    {
      if ((object) a == null && (object) b == null)
        return true;
      if ((object) a == null || (object) b == null)
        return false;
      return a.Equals(b);
    }

    public static bool operator !=(StackedBoundingBox a, StackedBoundingBox b)
    {
      return !a.Equals(b);
    }
  }
}
