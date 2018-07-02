namespace M3D.Model.FilIO
{
  internal interface IModelExporter
  {
    void Save(ModelData model, string fileName);
  }
}
