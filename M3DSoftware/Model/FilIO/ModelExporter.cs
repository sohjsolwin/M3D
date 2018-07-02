namespace M3D.Model.FilIO
{
  internal interface ModelExporter
  {
    void Save(ModelData model, string fileName);
  }
}
