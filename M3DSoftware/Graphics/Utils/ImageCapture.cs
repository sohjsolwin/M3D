// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Utils.ImageCapture
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.Model;
using M3D.Spooling.Common;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace M3D.Graphics.Utils
{
  public static class ImageCapture
  {
    public static string IconColor = "Standard";
    private static Random internalRandomGenerator = new Random();

    public static bool GenerateAndSaveIcon(ModelTransformPair model_transform, string icon_file, Vector2 icon_size, Color4 bgColor, GLControl glcontrol, ImageCapture.PreviewImageCallback callback)
    {
      Bitmap icon = ImageCapture.GenerateIcon(model_transform.modelNode, glcontrol, icon_size, new M3D.Model.Utils.Vector3(model_transform.transformNode.Rotation), new Color4(0.8431373f, 0.8901961f, 0.9921569f, 1f));
      if (icon == null)
      {
        return false;
      }

      ImageCapture.SaveIcon(icon, icon_file);
      if (callback != null)
      {
        var fileName = string.IsNullOrEmpty(model_transform.modelNode.zipFileName) ? model_transform.modelNode.fileName : model_transform.modelNode.zipFileName;
        callback(fileName, icon_file);
      }
      return true;
    }

    public static bool GenerateMultiModelPreview(List<ModelTransformPair> model_list, string icon_file, Vector2 icon_size, Color4 bgColor, GLControl glcontrol, M3D.Model.Utils.Vector3 center)
    {
      ImageCapture.SetViewPoint(glcontrol);
      ImageCapture.SetupForIconRender(new OpenTK.Vector3(center.x, center.y, center.z + 200f), new OpenTK.Vector3(center.x, center.y, center.z), bgColor);
      var minMax = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
      foreach (ModelTransformPair model in model_list)
      {
        ImageCapture.RenderModelGetScreenMinMax(model.modelNode, model.transformNode.Rotation, model.transformNode.Translation, model.transformNode.Scale, out Rectangle screen_rec);
        if (minMax.X == int.MaxValue)
        {
          minMax = screen_rec;
        }
        else
        {
          var x1 = minMax.X;
          var y1 = minMax.Y;
          var num1 = minMax.X + minMax.Width;
          var num2 = minMax.Y + minMax.Height;
          var x2 = screen_rec.X;
          var y2 = screen_rec.Y;
          var num3 = screen_rec.X + screen_rec.Width;
          var num4 = screen_rec.Y + screen_rec.Height;
          var num5 = x1 < x2 ? x1 : x2;
          var num6 = y1 < y2 ? y1 : y2;
          var num7 = num1 > num3 ? num1 : num3;
          var num8 = num2 > num4 ? num2 : num4;
          minMax.X = num5;
          minMax.Y = num6;
          minMax.Width = num7 - num5;
          minMax.Height = num8 - num6;
        }
      }
      Bitmap image = ImageCapture.GrabIconFromRender(glcontrol, ref minMax, icon_size);
      if (image == null)
      {
        return false;
      }

      ImageCapture.SaveIcon(image, icon_file);
      return true;
    }

    private static void SetViewPoint(GLControl glcontrol)
    {
      GL.Viewport(0, 0, glcontrol.Width, glcontrol.Height);
      GL.MatrixMode(MatrixMode.Projection);
      var perspectiveFieldOfView = Matrix4.CreatePerspectiveFieldOfView(0.7853982f, (float) glcontrol.Width / (float) glcontrol.Height, 100f, 1000f);
      GL.LoadMatrix(ref perspectiveFieldOfView);
    }

    private static Bitmap GenerateIcon(Model3DNode model, GLControl glcontrol, Vector2 icon_size, M3D.Model.Utils.Vector3 orientation, Color4 bgColor)
    {
      ImageCapture.SetViewPoint(glcontrol);
      var rotation = new M3D.Model.Utils.Vector3(orientation);
      var translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      M3D.Model.Utils.Vector3 ext = model.CalculateMinMax().Ext;
      var num = (double) ext.x <= (double) ext.z ? ((double) ext.z <= (double) ext.y ? 100f / ext.y : 100f / ext.z) : ((double) ext.y <= (double) ext.x ? 100f / ext.x : 100f / ext.y);
      var scale = new M3D.Model.Utils.Vector3(num, num, num);
      ImageCapture.SetupForIconRender(new OpenTK.Vector3(100f, 100f, 250f), new OpenTK.Vector3(0.0f, 0.0f, 0.0f), bgColor);
      ImageCapture.RenderModelGetScreenMinMax(model, rotation, translation, scale, out Rectangle screen_rec);
      return ImageCapture.GrabIconFromRender(glcontrol, ref screen_rec, icon_size);
    }

    private static void SetupForIconRender(OpenTK.Vector3 eye, OpenTK.Vector3 target, Color4 bgColor)
    {
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PushMatrix();
      GL.LoadIdentity();
      var mat = Matrix4.LookAt(eye, target, OpenTK.Vector3.UnitY);
      GL.LoadMatrix(ref mat);
      GL.BindTexture(TextureTarget.Texture2D, 0);
      GL.ClearColor(bgColor);
      GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
    }

    private static Bitmap GrabIconFromRender(GLControl glcontrol, ref Rectangle minMax, Vector2 icon_size)
    {
      ImageCapture.AdjustRectangle(ref minMax, glcontrol.Width, glcontrol.Height);
      Bitmap bitmap1 = ImageCapture.GrabScreenshot(minMax, glcontrol);
      var bitmap2 = new Bitmap((Image) bitmap1, new Size((int) icon_size.X, (int) icon_size.Y));
      bitmap1.Dispose();
      GL.PopMatrix();
      return bitmap2;
    }

    private static void SaveIcon(Bitmap image, string icon_file)
    {
      image.Save(icon_file);
      image.Dispose();
      GL.ClearColor(new Color4(0.913725f, 0.905882f, 0.9098f, 1f));
      GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
    }

    private static void RenderModelGetScreenMinMax(Model3DNode model, M3D.Model.Utils.Vector3 rotation, M3D.Model.Utils.Vector3 translation, M3D.Model.Utils.Vector3 scale, out Rectangle screen_rec)
    {
      var scale1 = Matrix4.CreateScale(scale.x, scale.y, scale.z);
      Matrix4 matrix4_1 = Matrix4.CreateRotationY(rotation.y * ((float) Math.PI / 180f)) * Matrix4.CreateRotationX(rotation.x * ((float) Math.PI / 180f)) * Matrix4.CreateRotationZ(rotation.z * ((float) Math.PI / 180f));
      var translation1 = Matrix4.CreateTranslation(translation.x, translation.y, translation.z);
      Matrix4 matrix4_2 = matrix4_1;
      Matrix4 mat = scale1 * matrix4_2 * translation1;
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PushMatrix();
      GL.Rotate(-90f, new OpenTK.Vector3(1f, 0.0f, 0.0f));
      GL.MultMatrix(ref mat);
      Color4 diffuse = model.Diffuse;
      var brightness = model.Brightness;
      var highlight = model.Highlight;
      if (ImageCapture.IconColor == "Standard")
      {
        model.Diffuse = new Color4((byte) 98, (byte) 181, (byte) 233, byte.MaxValue);
      }
      else if (ImageCapture.IconColor == "Random")
      {
        FilamentConstants.HexToRGB(FilamentConstants.generateHEXFromColor((FilamentConstants.ColorsEnum)(ImageCapture.internalRandomGenerator.Next(10) + 5)), out var R, out var G, out var B, out var A);
        model.Diffuse = new Color4(R, G, B, A);
      }
      model.Brightness = 1f;
      model.Highlight = false;
      model.Render3D();
      model.Diffuse = diffuse;
      model.Brightness = brightness;
      model.Highlight = highlight;
      screen_rec = ImageCapture.GetScreenMinMax(model);
      GL.PopMatrix();
    }

    public static Bitmap GrabScreenshot(GLControl glcontrol)
    {
      if (GraphicsContext.CurrentContext == null)
      {
        throw new GraphicsContextMissingException();
      }

      var width = glcontrol.Width;
      var height = glcontrol.Height;
      var bitmap = new Bitmap(width, height);
      BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
      bitmap.UnlockBits(bitmapdata);
      bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
      return bitmap;
    }

    public static unsafe Bitmap GrabScreenshot(Rectangle minMax, GLControl glcontrol)
    {
      var width1 = glcontrol.Width;
      var height1 = glcontrol.Height;
      if (GraphicsContext.CurrentContext == null)
      {
        throw new GraphicsContextMissingException();
      }

      var height2 = minMax.Height;
      var width2 = minMax.Width;
      var bitmap = new Bitmap(width2, height2);
      var x = minMax.X;
      var num1 = minMax.Y;
      if (minMax.Width > minMax.Height)
      {
        var num2 = minMax.Y + minMax.Height / 2 - height2 / 2;
        if (num2 + height2 > height1)
        {
          num2 -= num2 + height2 - height1;
        }

        if (num2 < 0)
        {
          num1 = 0;
        }
      }
      else if (minMax.Width < minMax.Height)
      {
        x = minMax.X + minMax.Width / 2 - width2 / 2;
        if (x + width2 > width1)
        {
          x -= x + width2 - width1;
        }

        if (x < 0)
        {
          x = 0;
        }
      }
      var rect = new Rectangle(0, 0, width2, height2);
      BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      var num3 = 4;
      for (var index1 = 0; index1 < bitmapdata.Height; ++index1)
      {
        var numPtr = (byte*) ((IntPtr) (void*) bitmapdata.Scan0 + index1 * bitmapdata.Stride);
        for (var index2 = 0; index2 < bitmapdata.Width; ++index2)
        {
          numPtr[index2 * num3] = (byte) 253;
          numPtr[index2 * num3 + 1] = (byte) 227;
          numPtr[index2 * num3 + 2] = (byte) 215;
          numPtr[index2 * num3 + 3] = byte.MaxValue;
        }
      }
      GL.ReadPixels(x, minMax.Y, width2, height2, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
      bitmap.UnlockBits(bitmapdata);
      bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
      return bitmap;
    }

    private static void AdjustRectangle(ref Rectangle minMax, int textureWidth, int textureHeight)
    {
      var num1 = 10;
      var num2 = 10;
      if (minMax.Width > minMax.Height)
      {
        num2 += (minMax.Width - minMax.Height) / 2;
      }
      else if (minMax.Height > minMax.Width)
      {
        num1 += (minMax.Height - minMax.Width) / 2;
      }

      minMax.X -= num1;
      minMax.Y -= num2;
      minMax.Width += num1 * 2;
      minMax.Height += num2 * 2;
      if (minMax.X < 0)
      {
        minMax.Width += minMax.X;
        minMax.X = 0;
      }
      if (minMax.Y < 0)
      {
        minMax.Height += minMax.Y;
        minMax.Y = 0;
      }
      if (minMax.X + minMax.Width > textureWidth)
      {
        minMax.Width = textureWidth - minMax.X;
      }

      if (minMax.Y + minMax.Height <= textureHeight)
      {
        return;
      }

      minMax.Height = textureHeight - minMax.Y;
    }

    private static Rectangle GetScreenMinMax(Model3DNode model)
    {
      int[] numArray = new int[4];
      double[] data1 = new double[16];
      double[] data2 = new double[16];
      GL.GetInteger(GetPName.Viewport, numArray);
      GL.GetDouble(GetPName.Modelview0MatrixExt, data1);
      GL.GetDouble(GetPName.ProjectionMatrix, data2);
      var right = new Matrix4((float) data2[0], (float) data2[1], (float) data2[2], (float) data2[3], (float) data2[4], (float) data2[5], (float) data2[6], (float) data2[7], (float) data2[8], (float) data2[9], (float) data2[10], (float) data2[11], (float) data2[12], (float) data2[13], (float) data2[14], (float) data2[15]);
      var left = new Matrix4((float) data1[0], (float) data1[1], (float) data1[2], (float) data1[3], (float) data1[4], (float) data1[5], (float) data1[6], (float) data1[7], (float) data1[8], (float) data1[9], (float) data1[10], (float) data1[11], (float) data1[12], (float) data1[13], (float) data1[14], (float) data1[15]);
      var screen_max = new M3D.Model.Utils.Vector3(float.MinValue, float.MinValue, float.MinValue);
      var screen_min = new M3D.Model.Utils.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      ModelSize minMax = model.CalculateMinMax();
      M3D.Model.Utils.Vector3 min = minMax.Min;
      M3D.Model.Utils.Vector3 max = minMax.Max;
      M3D.Model.Utils.Vector3 ext = minMax.Ext;
      var coord1 = new M3D.Model.Utils.Vector3(min);
      ImageCapture.UpdateScreenMinMax(coord1, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord1.x = max.x;
      ImageCapture.UpdateScreenMinMax(coord1, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord1.z = max.z;
      ImageCapture.UpdateScreenMinMax(coord1, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord1.x = min.x;
      ImageCapture.UpdateScreenMinMax(coord1, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      var coord2 = new M3D.Model.Utils.Vector3(max);
      ImageCapture.UpdateScreenMinMax(coord2, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord2.x = min.x;
      ImageCapture.UpdateScreenMinMax(coord2, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord2.z = min.z;
      ImageCapture.UpdateScreenMinMax(coord2, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      coord2.x = max.x;
      ImageCapture.UpdateScreenMinMax(coord2, ref screen_min, ref screen_max, Matrix4.Mult(left, right), numArray);
      return new Rectangle((int) screen_min.x, (int) screen_min.y, (int) ((double) screen_max.x - (double) screen_min.x), (int) ((double) screen_max.y - (double) screen_min.y));
    }

    private static void UpdateScreenMinMax(M3D.Model.Utils.Vector3 coord, ref M3D.Model.Utils.Vector3 screen_min, ref M3D.Model.Utils.Vector3 screen_max, Matrix4 matWorldViewProjection, int[] viewport)
    {
      OpenTK.Vector3 vector3 = ImageCapture.GluProject(coord, matWorldViewProjection, viewport);
      if ((double) vector3.X < (double) screen_min.x)
      {
        screen_min.x = vector3.X;
      }

      if ((double) vector3.Y < (double) screen_min.y)
      {
        screen_min.y = vector3.Y;
      }

      if ((double) vector3.Z < (double) screen_min.z)
      {
        screen_min.z = vector3.Z;
      }

      if ((double) vector3.X > (double) screen_max.x)
      {
        screen_max.x = vector3.X;
      }

      if ((double) vector3.Y > (double) screen_max.y)
      {
        screen_max.y = vector3.Y;
      }

      if ((double) vector3.Z <= (double) screen_max.z)
      {
        return;
      }

      screen_max.z = vector3.Z;
    }

    private static OpenTK.Vector3 GluProject(M3D.Model.Utils.Vector3 objPos, Matrix4 matWorldViewProjection, int[] viewport)
    {
      Vector4 vec;
      vec.X = objPos.x;
      vec.Y = objPos.y;
      vec.Z = objPos.z;
      vec.W = 1f;
      var vector4 = Vector4.Transform(vec, matWorldViewProjection);
      if ((double) vector4.W <= 0.0)
      {
        return OpenTK.Vector3.Zero;
      }

      vector4.X /= vector4.W;
      vector4.Y /= vector4.W;
      vector4.Z /= vector4.W;
      vector4.X = (float) ((double) vector4.X * 0.5 + 0.5);
      vector4.Y = (float) (-(double) vector4.Y * 0.5 + 0.5);
      vector4.Z = (float) ((double) vector4.Z * 0.5 + 0.5);
      vector4.X = vector4.X * (float) viewport[2] + (float) viewport[0];
      vector4.Y = vector4.Y * (float) viewport[3] + (float) viewport[1];
      OpenTK.Vector3 vector3;
      vector3.X = vector4.X;
      vector3.Y = (float) viewport[3] - vector4.Y;
      vector3.Z = vector4.Z;
      return vector3;
    }

    public delegate void PreviewImageCallback(string fileName, string imageName);
  }
}
