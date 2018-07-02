using M3D.Graphics.Ext3D.ModelRendering;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace M3D.Graphics.Ext3D
{
  public class Frame3DView : Frame
  {
    public bool UseLookAt = true;
    public Element3D origin = new Element3D();
    private float fov;
    private float minZ;
    private float maxZ;
    private M3D.Model.Utils.Vector3 viewPointPos;
    private M3D.Model.Utils.Vector3 cameraLookAtPos;
    private Matrix4 ProjMatrix;
    private Matrix4 ViewMatrix;

    public Frame3DView()
      : this(0)
    {
    }

    public Frame3DView(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public Frame3DView(int ID, Element2D parent)
      : base(ID, parent)
    {
      viewPointPos = new M3D.Model.Utils.Vector3(0.0f, 100f, 400f);
      cameraLookAtPos = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      SetCameraPerspective(45f, 100f, 1000f);
    }

    public override void OnRender(GUIHost host)
    {
      host.GetSimpleRenderer().End2D();
      GL.Enable(EnableCap.DepthTest);
      GL.PushMatrix();
      Render3D();
      GL.PopMatrix();
      host.GetSimpleRenderer().Begin2D();
      base.OnRender(host);
    }

    public virtual void Render3D()
    {
      GL.Viewport(0, 0, Width, Height);
      GL.MatrixMode(MatrixMode.Projection);
      ProjMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, (float)Width / (float)Height, minZ, maxZ);
      GL.LoadMatrix(ref ProjMatrix);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.Clear(ClearBufferMask.DepthBufferBit);
      if (UseLookAt)
      {
        GL.PushMatrix();
        ViewMatrix = Matrix4.LookAt(new OpenTK.Vector3(viewPointPos.x, viewPointPos.y, viewPointPos.z), new OpenTK.Vector3(cameraLookAtPos.x, cameraLookAtPos.y, cameraLookAtPos.z), OpenTK.Vector3.UnitY);
        GL.LoadMatrix(ref ViewMatrix);
      }
      origin.Render3D();
      if (!UseLookAt)
      {
        return;
      }

      GL.PopMatrix();
    }

    public virtual void AddChildElement3D(Element3D child)
    {
      origin.AddChildElement(child);
    }

    public virtual void RemoveChildElement3D(Element3D child)
    {
      origin.RemoveChildElement(child);
    }

    public void SetCameraPerspective(float fov, float minZ, float maxZ)
    {
      this.fov = (float) ((double) fov * 3.14159274101257 / 180.0);
      this.minZ = minZ;
      this.maxZ = maxZ;
    }

    public Ray UnProjectMouseCoordinates(float mouse_x, float mouse_y, Matrix4 worldMatrix)
    {
      var w1 = new Vector4(mouse_x, mouse_y, 0.0f, 0.0f);
      var w2 = new Vector4(mouse_x, mouse_y, 1f, 0.0f);
      Vector4 vector4_1 = UnProject(w1, worldMatrix);
      Vector4 vector4_2 = UnProject(w2, worldMatrix);
      var direction = new M3D.Model.Utils.Vector3(vector4_2.X - vector4_1.X, vector4_2.Y - vector4_1.Y, vector4_2.Z - vector4_1.Z);
      direction.Normalize();
      return new Ray(new M3D.Model.Utils.Vector3(vector4_1.X, vector4_1.Y, vector4_1.Z), direction);
    }

    private Vector4 UnProject(Vector4 w, Matrix4 worldMatrix)
    {
      var mat = Matrix4.Invert(worldMatrix * ViewMatrix * ProjMatrix);
      Vector4 vec = w;
      vec.X = (float) (2.0 * (double) vec.X / (double)Width - 1.0);
      vec.Y = (float) (1.0 - 2.0 * (double) vec.Y / (double)Height);
      vec.W = 1f;
      var vector4 = Vector4.Transform(vec, mat);
      if ((double) vector4.W == 0.0)
      {
        return vector4;
      }

      vector4.X /= vector4.W;
      vector4.Y /= vector4.W;
      vector4.Z /= vector4.W;
      return vector4;
    }

    public M3D.Model.Utils.Vector3 ViewPointPos
    {
      get
      {
        return viewPointPos;
      }
      set
      {
        viewPointPos = value;
      }
    }

    public M3D.Model.Utils.Vector3 CameraLookAtPos
    {
      get
      {
        return cameraLookAtPos;
      }
      set
      {
        cameraLookAtPos = value;
      }
    }
  }
}
