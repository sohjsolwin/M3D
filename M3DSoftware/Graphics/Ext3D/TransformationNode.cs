using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace M3D.Graphics.Ext3D
{
  public class TransformationNode : Element3D
  {
    private TransformationNode.Transform _transform;

    public TransformationNode(int ID, Element3D parent)
      : base(ID, parent)
    {
      Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      Scale = new M3D.Model.Utils.Vector3(1f, 1f, 1f);
      Translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public TransformationNode(M3D.Model.Utils.Vector3 translate, M3D.Model.Utils.Vector3 scale, M3D.Model.Utils.Vector3 rotate)
      : base(0, null)
    {
      Rotation = new M3D.Model.Utils.Vector3(rotate);
      Scale = new M3D.Model.Utils.Vector3(scale);
      Translation = new M3D.Model.Utils.Vector3(translate);
    }

    public override void Render3D()
    {
      GL.PushMatrix();
      Matrix4 transformationMatrix = GetTransformationMatrix();
      GL.MultMatrix(ref transformationMatrix);
      base.Render3D();
      GL.PopMatrix();
    }

    public Matrix4 GetTransformationMatrix()
    {
      var scale = Matrix4.CreateScale(_transform.scaling.X, _transform.scaling.Y, _transform.scaling.Z);
      Matrix4 matrix4_1 = Matrix4.CreateRotationY(_transform.rotation.Y * ((float) Math.PI / 180f)) * Matrix4.CreateRotationX(_transform.rotation.X * ((float) Math.PI / 180f)) * Matrix4.CreateRotationZ(_transform.rotation.Z * ((float) Math.PI / 180f));
      var translation = Matrix4.CreateTranslation(_transform.translation.X, _transform.translation.Y, _transform.translation.Z);
      Matrix4 matrix4_2 = matrix4_1;
      return scale * matrix4_2 * translation;
    }

    public M3D.Model.Utils.Vector3 Rotation
    {
      get
      {
        return _transform.rotation;
      }
      set
      {
        _transform.rotation = value;
      }
    }

    public M3D.Model.Utils.Vector3 Scale
    {
      get
      {
        return _transform.scaling;
      }
      set
      {
        _transform.scaling = value;
      }
    }

    public M3D.Model.Utils.Vector3 Translation
    {
      get
      {
        return _transform.translation;
      }
      set
      {
        _transform.translation = value;
      }
    }

    public TransformationNode.Transform TotalTransform
    {
      get
      {
        return _transform;
      }
      set
      {
        _transform = value;
      }
    }

    public TransformationNode.Transform TransformData
    {
      get
      {
        return new TransformationNode.Transform(_transform);
      }
      set
      {
        _transform = value;
      }
    }

    public struct Transform
    {
      public M3D.Model.Utils.Vector3 rotation;
      public M3D.Model.Utils.Vector3 scaling;
      public M3D.Model.Utils.Vector3 translation;

      public Transform(M3D.Model.Utils.Vector3 rotation, M3D.Model.Utils.Vector3 scaling, M3D.Model.Utils.Vector3 translation)
      {
        this.rotation = new M3D.Model.Utils.Vector3(rotation);
        this.scaling = new M3D.Model.Utils.Vector3(scaling);
        this.translation = new M3D.Model.Utils.Vector3(translation);
      }

      public Transform(TransformationNode.Transform transform)
      {
        this = new TransformationNode.Transform(transform.rotation, transform.scaling, transform.translation);
      }

      public bool Equals(ref TransformationNode.Transform transform)
      {
        if (rotation == transform.rotation && scaling == transform.scaling && translation.X == (double)transform.translation.X)
        {
          return translation.Y == (double)transform.translation.Y;
        }

        return false;
      }
    }
  }
}
