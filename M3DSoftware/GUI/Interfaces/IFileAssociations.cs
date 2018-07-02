namespace M3D.GUI.Interfaces
{
  public interface IFileAssociations
  {
    string ExtensionOpenWith(string Extension);

    void Set3DFileAssociation(string Extension, string KeyName, string OpenWith, string FileDescription, string fileIcon);

    void Delete3DFileAssociation(string Extension, string KeyName);
  }
}
