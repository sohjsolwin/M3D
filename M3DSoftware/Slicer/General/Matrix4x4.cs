// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.Matrix4x4
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;

namespace M3D.Slicer.General
{
  public class Matrix4x4
  {
    public float[,] m;

    public Matrix4x4()
    {
      this.m = new float[4, 4];
    }

    public void RotationX(float angle)
    {
      this.Identity();
      this.m[1, 1] = (float) Math.Cos((double) angle);
      this.m[1, 2] = (float) -Math.Sin((double) angle);
      this.m[2, 1] = (float) Math.Sin((double) angle);
      this.m[2, 2] = (float) Math.Cos((double) angle);
    }

    public void RotationY(float angle)
    {
      this.Identity();
      this.m[0, 0] = (float) Math.Cos((double) angle);
      this.m[0, 2] = (float) Math.Sin((double) angle);
      this.m[2, 0] = (float) -Math.Sin((double) angle);
      this.m[2, 2] = (float) Math.Cos((double) angle);
    }

    public void RotationZ(float angle)
    {
      this.Identity();
      this.m[0, 0] = (float) Math.Cos((double) angle);
      this.m[0, 1] = (float) -Math.Sin((double) angle);
      this.m[1, 0] = (float) Math.Sin((double) angle);
      this.m[1, 1] = (float) Math.Cos((double) angle);
    }

    public void Identity()
    {
      for (int index1 = 0; index1 < 4; ++index1)
      {
        for (int index2 = 0; index2 < 4; ++index2)
          this.m[index1, index2] = 0.0f;
      }
      this.m[0, 0] = 1f;
      this.m[1, 1] = 1f;
      this.m[2, 2] = 1f;
      this.m[3, 3] = 1f;
    }

    public static Matrix4x4 mul(Matrix4x4 lhs, Matrix4x4 rhs)
    {
      Matrix4x4 matrix4x4 = new Matrix4x4();
      for (int index1 = 0; index1 < 4; ++index1)
      {
        for (int index2 = 0; index2 < 4; ++index2)
        {
          matrix4x4.m[index1, index2] = 0.0f;
          for (int index3 = 0; index3 < 4; ++index3)
            matrix4x4.m[index1, index2] += lhs.m[index3, index2] * rhs.m[index1, index3];
        }
      }
      return matrix4x4;
    }
  }
}
