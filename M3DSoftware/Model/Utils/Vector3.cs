using OpenTK;
using System;

namespace M3D.Model.Utils
{
  public class Vector3 : IEquatable<Vector3>
  {
    private double[] position = new double[3];

    public float x
    {
      get
      {
        return (float)position[0];
      }
      set
      {
        position[0] = (double) value;
      }
    }

    public float y
    {
      get
      {
        return (float)position[1];
      }
      set
      {
        position[1] = (double) value;
      }
    }

    public float z
    {
      get
      {
        return (float)position[2];
      }
      set
      {
        position[2] = (double) value;
      }
    }

    public Vector3()
    {
      x = 0.0f;
      y = 0.0f;
      z = 0.0f;
    }

    public Vector3(float x, float y, float z)
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    public Vector3(Vector3 v)
    {
      x = v.x;
      y = v.y;
      z = v.z;
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
      return (object) vector3_1 == null && (object) vector3_2 == null || (object) vector3_1 != null && (object) vector3_2 != null && ((double) vector3_1.x == (double) vector3_2.x && (double) vector3_1.y == (double) vector3_2.y) && (double) vector3_1.z == (double) vector3_2.z;
    }

    public bool Equals(Vector3 other)
    {
      return (double)x == (double) other.x && (double)y == (double) other.y && (double)z == (double) other.z;
    }

    public override int GetHashCode()
    {
      var num1 = x;
      var hashCode1 = num1.GetHashCode();
      num1 = y;
      var hashCode2 = num1.GetHashCode();
      var num2 = hashCode1 ^ hashCode2;
      num1 = z;
      var hashCode3 = num1.GetHashCode();
      return num2 ^ hashCode3;
    }

    public static bool operator ==(Vector3 a, Vector3 b)
    {
      return (object) a == null && (object) b == null || (object) a != null && (object) b != null && ((double) a.x == (double) b.x && (double) a.y == (double) b.y) && (double) a.z == (double) b.z;
    }

    public static bool operator !=(Vector3 a, Vector3 b)
    {
      return ((object) a != null || (object) b != null) && ((object) a == null || (object) b == null || ((double) a.x != (double) b.x || (double) a.y != (double) b.y) || (double) a.z != (double) b.z);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
      return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
      return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator -(Vector3 a)
    {
      return a.Inverse();
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
      return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
      return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static Vector3 operator +(Vector3 a, float b)
    {
      return new Vector3(a.x + b, a.y + b, a.z + b);
    }

    public static Vector3 operator -(Vector3 a, float b)
    {
      return new Vector3(a.x - b, a.y - b, a.z - b);
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
      return new Vector3(a.x * b, a.y * b, a.z * b);
    }

    public static Vector3 operator *(float b, Vector3 a)
    {
      return new Vector3(a.x * b, a.y * b, a.z * b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
      var num = 1f / b;
      return new Vector3(a.x * num, a.y * num, a.z * num);
    }

    public static Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
      return a.Cross(b);
    }

    public float SqLength()
    {
      return (float) ((double)x * (double)x + (double)y * (double)y + (double)z * (double)z);
    }

    public float Length()
    {
      return (float) Math.Sqrt((double)x * (double)x + (double)y * (double)y + (double)z * (double)z);
    }

    public float Dot(Vector3 v)
    {
      return (float) ((double)x * (double) v.x + (double)y * (double) v.y + (double)z * (double) v.z);
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
      return new Vector3((float) ((double)y * (double) v.z - (double)z * (double) v.y), (float) ((double)z * (double) v.x - (double)x * (double) v.z), (float) ((double)x * (double) v.y - (double)y * (double) v.x));
    }

    public float Distance(Vector3 vector2)
    {
      return (vector2 - this).Length();
    }

    public Vector3 Inverse()
    {
      return new Vector3(-x, -y, -z);
    }

    public void RotateVector(float angle, bool x, bool y, bool z)
    {
      if (x)
      {
        var num = (float) ((double) this.z * Math.Cos((double) angle) - (double) this.y * Math.Sin((double) angle));
        this.y = (float) ((double) this.z * Math.Sin((double) angle) + (double) this.y * Math.Cos((double) angle));
        this.z = num;
      }
      else if (y)
      {
        var num1 = (float) ((double) this.x * Math.Cos((double) angle) - (double) this.z * Math.Sin((double) angle));
        var num2 = (float) ((double) this.x * Math.Sin((double) angle) + (double) this.z * Math.Cos((double) angle));
        this.x = num1;
        this.z = num2;
      }
      else
      {
        if (!z)
        {
          return;
        }

        var num1 = (float) ((double) this.x * Math.Cos((double) angle) - (double) this.y * Math.Sin((double) angle));
        var num2 = (float) ((double) this.x * Math.Sin((double) angle) + (double) this.y * Math.Cos((double) angle));
        this.x = num1;
        this.y = num2;
      }
    }

    public static Vector3 MatrixProduct(Matrix4 matrix, Vector3 vector)
    {
      var vector4 = Vector4.Transform(new Vector4(vector.x, vector.y, vector.z, 1f), matrix);
      return new Vector3(vector4.X, vector4.Y, vector4.Z);
    }

    public void MatrixProduct(Matrix4 matrix)
    {
      var vector4 = Vector4.Transform(new Vector4(x, y, z, 1f), matrix);
      x = vector4.X;
      y = vector4.Y;
      z = vector4.Z;
    }

    public bool isZero()
    {
      if ((double)x == 0.0 && (double)y == 0.0)
      {
        return (double)z == 0.0;
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
