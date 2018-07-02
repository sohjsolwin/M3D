using System;

namespace M3D.Slicer.General
{
  public class Matrix4x4
  {
    public float[,] m;

    public Matrix4x4()
    {
      m = new float[4, 4];
    }

    public void RotationX(float angle)
    {
      Identity();
      m[1, 1] = (float) Math.Cos(angle);
      m[1, 2] = (float) -Math.Sin(angle);
      m[2, 1] = (float) Math.Sin(angle);
      m[2, 2] = (float) Math.Cos(angle);
    }

    public void RotationY(float angle)
    {
      Identity();
      m[0, 0] = (float) Math.Cos(angle);
      m[0, 2] = (float) Math.Sin(angle);
      m[2, 0] = (float) -Math.Sin(angle);
      m[2, 2] = (float) Math.Cos(angle);
    }

    public void RotationZ(float angle)
    {
      Identity();
      m[0, 0] = (float) Math.Cos(angle);
      m[0, 1] = (float) -Math.Sin(angle);
      m[1, 0] = (float) Math.Sin(angle);
      m[1, 1] = (float) Math.Cos(angle);
    }

    public void Identity()
    {
      for (var index1 = 0; index1 < 4; ++index1)
      {
        for (var index2 = 0; index2 < 4; ++index2)
        {
          m[index1, index2] = 0.0f;
        }
      }
      m[0, 0] = 1f;
      m[1, 1] = 1f;
      m[2, 2] = 1f;
      m[3, 3] = 1f;
    }

    public static Matrix4x4 Mul(Matrix4x4 lhs, Matrix4x4 rhs)
    {
      var matrix4x4 = new Matrix4x4();
      for (var index1 = 0; index1 < 4; ++index1)
      {
        for (var index2 = 0; index2 < 4; ++index2)
        {
          matrix4x4.m[index1, index2] = 0.0f;
          for (var index3 = 0; index3 < 4; ++index3)
          {
            matrix4x4.m[index1, index2] += lhs.m[index3, index2] * rhs.m[index1, index3];
          }
        }
      }
      return matrix4x4;
    }
  }
}
