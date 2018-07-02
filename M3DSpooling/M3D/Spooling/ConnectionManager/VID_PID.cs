using System;
using System.Collections.Generic;

namespace M3D.Spooling.ConnectionManager
{
  internal class VID_PID : IEqualityComparer<VID_PID>, IEquatable<VID_PID>
  {
    public readonly uint VID;
    public readonly uint PID;

    public VID_PID(uint VID, uint PID)
    {
      this.VID = VID;
      this.PID = PID;
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }

      var other = obj as VID_PID;
      if (other == null)
      {
        return false;
      }

      return Equals(other);
    }

    public override int GetHashCode()
    {
      return GetHashCode(this);
    }

    public bool Equals(VID_PID other)
    {
      return Equals(this, other);
    }

    public int GetHashCode(VID_PID vidpidpair)
    {
      return vidpidpair.ToString().GetHashCode();
    }

    public bool Equals(VID_PID vidpidpair1, VID_PID vidpidpair2)
    {
      if ((int) vidpidpair1.PID == (int) vidpidpair2.PID)
      {
        return (int) vidpidpair1.VID == (int) vidpidpair2.VID;
      }

      return false;
    }

    public override string ToString()
    {
      return string.Format("{0}-{1}", VID, PID);
    }
  }
}
