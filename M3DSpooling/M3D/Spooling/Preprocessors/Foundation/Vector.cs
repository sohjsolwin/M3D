// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Preprocessors.Foundation.Vector
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Preprocessors.Foundation
{
  public class Vector
  {
    public float x;
    public float y;
    public float z;
    public float e;

    public Vector()
    {
    }

    public Vector(Vector rhs)
    {
      this.x = rhs.x;
      this.y = rhs.y;
      this.z = rhs.z;
      this.e = rhs.e;
    }

    public Vector(float x, float y, float z)
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    public Vector(float x, float y, float z, float e)
    {
      this.x = x;
      this.y = y;
      this.z = z;
      this.e = e;
    }

    public float GetLength()
    {
      return (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z + (double) this.e * (double) this.e);
    }

    public void Normalize()
    {
      Vector vector1 = new Vector();
      Vector vector2 = this / this.GetLength();
      for (int index = 0; index < 4; ++index)
        this[index] = vector2[index];
    }

    public float this[int key]
    {
      get
      {
        switch (key)
        {
          case 0:
            return this.x;
          case 1:
            return this.y;
          case 2:
            return this.z;
          case 3:
            return this.e;
          default:
            throw new Exception("unexpected index");
        }
      }
      set
      {
        switch (key)
        {
          case 0:
            this.x = value;
            break;
          case 1:
            this.y = value;
            break;
          case 2:
            this.z = value;
            break;
          case 3:
            this.e = value;
            break;
          default:
            throw new Exception("unexpected index");
        }
      }
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
      Vector vector = new Vector();
      for (int index = 0; index < 4; ++index)
        vector[index] = v1[index] - v2[index];
      return vector;
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
      Vector vector = new Vector();
      for (int index = 0; index < 4; ++index)
        vector[index] = v1[index] + v2[index];
      return vector;
    }

    public static Vector operator /(Vector v1, float divisor)
    {
      Vector vector = new Vector();
      for (int index = 0; index < 4; ++index)
        vector[index] = v1[index] / divisor;
      return vector;
    }

    public static Vector operator *(Vector v1, float multiplier)
    {
      Vector vector = new Vector();
      for (int index = 0; index < 4; ++index)
        vector[index] = v1[index] * multiplier;
      return vector;
    }
  }
}
