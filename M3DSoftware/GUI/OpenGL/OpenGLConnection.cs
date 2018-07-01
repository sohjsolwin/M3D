// Decompiled with JetBrains decompiler
// Type: M3D.GUI.OpenGL.OpenGLConnection
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D.ModelRendering;
using M3D.GUI.Forms;
using M3D.Spooling.Common.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace M3D.GUI.OpenGL
{
  public class OpenGLConnection
  {
    public static int _TextureSize = 1024;
    private GLControl _glControl1;

    private void DetermineOpenGLRenderingMethod(DebugLogger debugLogger)
    {
      string str1 = (string) null;
      try
      {
        int data1 = 0;
        int data2 = 0;
        str1 = "DetermineOpenGLRenderingMethod::GL.GetInteger";
        GL.GetInteger(GetPName.MajorVersion, out data1);
        GL.GetInteger(GetPName.MinorVersion, out data2);
        Version version = new Version(data1, data2);
        debugLogger.Add("DetermineOpenGLRenderingMethod()", "OpenGL vendor: " + GL.GetString(StringName.Vendor) + ".", DebugLogger.LogType.Primary);
        debugLogger.Add("DetermineOpenGLRenderingMethod()", "OpenGL renderer: " + GL.GetString(StringName.Renderer) + ".", DebugLogger.LogType.Primary);
        debugLogger.Add("DetermineOpenGLRenderingMethod()", "OpenGL version: " + version.ToString() + ".", DebugLogger.LogType.Primary);
        debugLogger.Add("DetermineOpenGLRenderingMethod()", "OpenGL Shading Language Version: " + GL.GetString(StringName.ShadingLanguageVersion) + ".", DebugLogger.LogType.Primary);
        Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
        if (version.Major >= 3)
        {
          str1 = "DetermineOpenGLRenderingMethod::version.Major >= 3";
          int integer = GL.GetInteger(GetPName.NumExtensions);
          for (int index = 0; index < integer; ++index)
          {
            string key = GL.GetString(StringNameIndexed.Extensions, index);
            dictionary.Add(key, true);
          }
        }
        else
        {
          str1 = "DetermineOpenGLRenderingMethod::else";
          string str2 = GL.GetString(StringName.Extensions);
          char[] chArray = new char[1]{ ' ' };
          foreach (string key in str2.Split(chArray))
            dictionary.Add(key, true);
        }
        if (version.Major >= 2)
        {
          str1 = "DetermineOpenGLRenderingMethod::OpenGLRendererObject.OpenGLRenderMode.VBOs";
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.VBOs;
        }
        else
        {
          str1 = "DetermineOpenGLRenderingMethod::OpenGLRendererObject.OpenGLRenderMode.ImmediateMode";
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
        }
        debugLogger.Add("DetermineOpenGLRenderingMethod()", "OpenGL render mode: " + OpenGLRendererObject.openGLRenderMode.ToString() + ".", DebugLogger.LogType.Primary);
      }
      catch (Exception ex)
      {
        string extra_info = str1;
        ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
    }

    public void OnLoad(GLControl glControl1, DebugLogger debugLogger)
    {
      this._glControl1 = glControl1;
      this.DetermineOpenGLRenderingMethod(debugLogger);
      glControl1.Context.SwapInterval = 0;
      GraphicsContext.CurrentContext.SwapInterval = 0;
      debugLogger.Add("glControl1_Load()", "Frame buffer and textures created.", DebugLogger.LogType.Secondary);
      GL.Enable(EnableCap.Blend);
      GL.Enable(EnableCap.Lighting);
      GL.Enable(EnableCap.Light0);
      GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1f, 1f, 1f, 1f));
      GL.Light(LightName.Light0, LightParameter.Ambient, new Color4(0.2f, 0.2f, 0.2f, 1f));
      GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1f, 1f, 1f, 1f));
      GL.Light(LightName.Light0, LightParameter.Specular, new Color4(1f, 1f, 1f, 1f));
      GL.Enable(EnableCap.Light1);
      GL.Light(LightName.Light1, LightParameter.Position, new Vector4(1f, 1f, 1f, 1f));
      GL.Light(LightName.Light1, LightParameter.Ambient, new Color4(0.0f, 0.0f, 0.0f, 1f));
      GL.Light(LightName.Light1, LightParameter.Diffuse, new Vector4(1f, 1f, 1f, 1f));
      GL.Light(LightName.Light1, LightParameter.Specular, new Color4(0.3f, 0.3f, 0.3f, 1f));
      GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);
      GL.Enable(EnableCap.Normalize);
      GL.Enable(EnableCap.CullFace);
      GL.ShadeModel(ShadingModel.Smooth);
      GL.Enable(EnableCap.Texture2D);
      GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
    }

    public void OnPaint(OpenGLConnection.RenderTaskDelegate renderTask)
    {
      try
      {
        GL.ClearColor(0.913725f, 0.905882f, 0.9098f, 1f);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
        GL.Translate(0.0f, 0.0f, -500f);
        renderTask();
        this.GLControl1.SwapBuffers();
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public GLControl GLControl1
    {
      get
      {
        return this._glControl1;
      }
    }

    public delegate void RenderTaskDelegate();
  }
}
