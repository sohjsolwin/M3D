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
      this.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      this.Scale = new M3D.Model.Utils.Vector3(1f, 1f, 1f);
      this.Translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public TransformationNode(M3D.Model.Utils.Vector3 translate, M3D.Model.Utils.Vector3 scale, M3D.Model.Utils.Vector3 rotate)
      : base(0, (Element3D) null)
    {
      this.Rotation = new M3D.Model.Utils.Vector3(rotate);
      this.Scale = new M3D.Model.Utils.Vector3(scale);
      this.Translation = new M3D.Model.Utils.Vector3(translate);
    }

    public override void Render3D()
    {
      GL.PushMatrix();
      Matrix4 transformationMatrix = this.GetTransformationMatrix();
      GL.MultMatrix(ref transformationMatrix);
      base.Render3D();
      GL.PopMatrix();
    }

    public Matrix4 GetTransformationMatrix()
    {
      Matrix4 scale = Matrix4.CreateScale(this._transform.scaling.x, this._transform.scaling.y, this._transform.scaling.z);
      Matrix4 matrix4_1 = Matrix4.CreateRotationY(this._transform.rotation.y * ((float) Math.PI / 180f)) * Matrix4.CreateRotationX(this._transform.rotation.x * ((float) Math.PI / 180f)) * Matrix4.CreateRotationZ(this._transform.rotation.z * ((float) Math.PI / 180f));
      Matrix4 translation = Matrix4.CreateTranslation(this._transform.translation.x, this._transform.translation.y, this._transform.translation.z);
      Matrix4 matrix4_2 = matrix4_1;
      return scale * matrix4_2 * translation;
    }

    public M3D.Model.Utils.Vector3 Rotation
    {
      get
      {
        return this._transform.rotation;
      }
      set
      {
        this._transform.rotation = value;
      }
    }

    public M3D.Model.Utils.Vector3 Scale
    {
      get
      {
        return this._transform.scaling;
      }
      set
      {
        this._transform.scaling = value;
      }
    }

    public M3D.Model.Utils.Vector3 Translation
    {
      get
      {
        return this._transform.translation;
      }
      set
      {
        this._transform.translation = value;
      }
    }

    public TransformationNode.Transform TotalTransform
    {
      get
      {
        return this._transform;
      }
      set
      {
        this._transform = value;
      }
    }

    public TransformationNode.Transform TransformData
    {
      get
      {
        return new TransformationNode.Transform(this._transform);
      }
      set
      {
        this._transform = value;
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
        if (this.rotation == transform.rotation && this.scaling == transform.scaling && (double) this.translation.x == (double) transform.translation.x)
          return (double) this.translation.y == (double) transform.translation.y;
        return false;
      }
    }
  }
}
