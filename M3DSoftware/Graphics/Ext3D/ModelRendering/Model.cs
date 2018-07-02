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
