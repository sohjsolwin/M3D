// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.TransformationNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : base(0, (Element3D) null)
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
      var scale = Matrix4.CreateScale(_transform.scaling.x, _transform.scaling.y, _transform.scaling.z);
      Matrix4 matrix4_1 = Matrix4.CreateRotationY(_transform.rotation.y * ((float) Math.PI / 180f)) * Matrix4.CreateRotationX(_transform.rotation.x * ((float) Math.PI / 180f)) * Matrix4.CreateRotationZ(_transform.rotation.z * ((float) Math.PI / 180f));
      var translation = Matrix4.CreateTranslation(_transform.translation.x, _transform.translation.y, _transform.translation.z);
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
        if (rotation == transform.rotation && scaling == transform.scaling && (double)translation.x == (double) transform.translation.x)
        {
          return (double)translation.y == (double) transform.translation.y;
        }

        return false;
      }
    }
  }
}
