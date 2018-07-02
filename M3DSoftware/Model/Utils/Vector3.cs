using OpenTK;
using System;

namespace M3D.Model.Utils
{
  public class Vector3 : IEquatable<Vector3>
  {
    private double[] position = new double[3];

    public float X
    {
      get
      {
        return (float)position[0];
      }
      set
      {
        position[0] = value;
      }
    }

    public float Y
    {
      get
      {
        return (float)position[1];
      }
      set
      {
        position[1] = value;
      }
    }

    public float Z
    {
      get
      {
        return (float)position[2];
      }
      set
      {
        position[2] = value;
      }
    }

    public Vector3()
    {
      X = 0.0f;
      Y = 0.0f;
      Z = 0.0f;
    }

    public Vector3(float x, float y, float z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    public Vector3(Vector3 v)
    {
      X = v.X;
      Y = v.Y;
      Z = v.Z;
    }

    public static uint SizeInBytes
    {
      get
      {
        return 12;
      }
    }

    public override bool Equals(object obj)
    {
      Vector3 vector3_1 = this;
      var vector3_2 = obj as Vector3;
      return (object) vector3_1 == null && (object) vector3_2 == null || (object) vector3_1 != null && (object) vector3_2 != null && (vector3_1.X == (double)vector3_2.X && vector3_1.Y == (double)vector3_2.Y) && vector3_1.Z == (double)vector3_2.Z;
    }

    public bool Equals(Vector3 other)
    {
      return X == (double)other.X && Y == (double)other.Y && Z == (double)other.Z;
    }

    public override int GetHashCode()
    {
      var num1 = X;
      var hashCode1 = num1.GetHashCode();
      num1 = Y;
      var hashCode2 = num1.GetHashCode();
      var num2 = hashCode1 ^ hashCode2;
      num1 = Z;
      var hashCode3 = num1.GetHashCode();
      return num2 ^ hashCode3;
    }

    public static bool operator ==(Vector3 a, Vector3 b)
    {
      return (object) a == null && (object) b == null || (object) a != null && (object) b != null && (a.X == (double)b.X && a.Y == (double)b.Y) && a.Z == (double)b.Z;
    }

    public static bool operator !=(Vector3 a, Vector3 b)
    {
      return ((object) a != null || (object) b != null) && ((object) a == null || (object) b == null || (a.X != (double)b.X || a.Y != (double)b.Y) || a.Z != (double)b.Z);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
      return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
      return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3 operator -(Vector3 a)
    {
      return a.Inverse();
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
      return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
      return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    public static Vector3 operator +(Vector3 a, float b)
    {
      return new Vector3(a.X + b, a.Y + b, a.Z + b);
    }

    public static Vector3 operator -(Vector3 a, float b)
    {
      return new Vector3(a.X - b, a.Y - b, a.Z - b);
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
      return new Vector3(a.X * b, a.Y * b, a.Z * b);
    }

    public static Vector3 operator *(float b, Vector3 a)
    {
      return new Vector3(a.X * b, a.Y * b, a.Z * b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
      var num = 1f / b;
      return new Vector3(a.X * num, a.Y * num, a.Z * num);
    }

    public static Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
      return a.Cross(b);
    }

    public float SqLength()
    {
      return (float)(X * (double)X + Y * (double)Y + Z * (double)Z);
    }

    public float Length()
    {
      return (float) Math.Sqrt(X * (double)X + Y * (double)Y + Z * (double)Z);
    }

    public float Dot(Vector3 v)
    {
      return (float)(X * (double)v.X + Y * (double)v.Y + Z * (double)v.Z);
    }

    public Vector3 Normalize()
    {
      return this * (1f / Length());
    }

    public static Vector3 Normalize(Vector3 vector)
    {
      var num = 1f / vector.Length();
      return vector * num;
    }

    public Vector3 Cross(Vector3 v)
    {
      return new Vector3((float)(Y * (double)v.Z - Z * (double)v.Y), (float)(Z * (double)v.X - X * (double)v.Z), (float)(X * (double)v.Y - Y * (double)v.X));
    }

    public float Distance(Vector3 vector2)
    {
      return (vector2 - this).Length();
    }

    public Vector3 Inverse()
    {
      return new Vector3(-X, -Y, -Z);
    }

    public void RotateVector(float angle, bool x, bool y, bool z)
    {
      if (x)
      {
        var num = (float)(this.Z * Math.Cos(angle) - this.Y * Math.Sin(angle));
        this.Y = (float)(this.Z * Math.Sin(angle) + this.Y * Math.Cos(angle));
        this.Z = num;
      }
      else if (y)
      {
        var num1 = (float)(this.X * Math.Cos(angle) - this.Z * Math.Sin(angle));
        var num2 = (float)(this.X * Math.Sin(angle) + this.Z * Math.Cos(angle));
        this.X = num1;
        this.Z = num2;
      }
      else
      {
        if (!z)
        {
          return;
        }

        var num1 = (float)(this.X * Math.Cos(angle) - this.Y * Math.Sin(angle));
        var num2 = (float)(this.X * Math.Sin(angle) + this.Y * Math.Cos(angle));
        this.X = num1;
        this.Y = num2;
      }
    }

    public static Vector3 MatrixProduct(Matrix4 matrix, Vector3 vector)
    {
      var vector4 = Vector4.Transform(new Vector4(vector.X, vector.Y, vector.Z, 1f), matrix);
      return new Vector3(vector4.X, vector4.Y, vector4.Z);
    }

    public void MatrixProduct(Matrix4 matrix)
    {
      var vector4 = Vector4.Transform(new Vector4(X, Y, Z, 1f), matrix);
      X = vector4.X;
      Y = vector4.Y;
      Z = vector4.Z;
    }

    public bool isZero()
    {
      if (X == 0.0 && Y == 0.0)
      {
        return Z == 0.0;
      }

      return false;
    }

    public double[] Position
    {
      get
      {
        return position;
      }
      set
      {
        if (value.Length != 3)
        {
          throw new Exception("Must be a length 3 array");
        }

        position = value;
      }
    }
  }
}
