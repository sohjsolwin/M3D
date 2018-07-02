namespace M3D.Graphics
{
  internal class Texture
  {
    public string name;
    public uint handle;

    public Texture(string name, uint handle)
    {
      this.name = name;
      this.handle = handle;
    }
  }
}
