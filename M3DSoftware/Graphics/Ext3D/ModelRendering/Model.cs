// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.Model
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model;
using System.Diagnostics;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class Model
  {
    public string fileName;
    public string zipFileName;
    public OpenGLRendererObject geometryData;
    public ModelData modelData;

    public Model(ModelData modelData, string fileName)
    {
      this.modelData = modelData;
      this.fileName = fileName;
      modelData.Translate(-modelData.Center);
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      geometryData = new OpenGLRendererObject(modelData);
      stopwatch.Stop();
    }

    public Model(ModelData modelData, OpenGLRendererObject geometryData, string fileName, string zipFileName)
    {
      this.modelData = modelData;
      this.geometryData = geometryData;
      this.fileName = fileName;
      this.zipFileName = zipFileName;
    }
  }
}
