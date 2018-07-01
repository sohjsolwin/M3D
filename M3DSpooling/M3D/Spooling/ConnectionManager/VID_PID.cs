// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.ConnectionManager.VID_PID
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        return false;
      VID_PID other = obj as VID_PID;
      if (other == null)
        return false;
      return this.Equals(other);
    }

    public override int GetHashCode()
    {
      return this.GetHashCode(this);
    }

    public bool Equals(VID_PID other)
    {
      return this.Equals(this, other);
    }

    public int GetHashCode(VID_PID vidpidpair)
    {
      return vidpidpair.ToString().GetHashCode();
    }

    public bool Equals(VID_PID vidpidpair1, VID_PID vidpidpair2)
    {
      if ((int) vidpidpair1.PID == (int) vidpidpair2.PID)
        return (int) vidpidpair1.VID == (int) vidpidpair2.VID;
      return false;
    }

    public override string ToString()
    {
      return string.Format("{0}-{1}", (object) this.VID, (object) this.PID);
    }
  }
}
