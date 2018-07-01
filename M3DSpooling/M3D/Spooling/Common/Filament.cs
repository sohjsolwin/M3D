// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Filament
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common
{
  public class Filament
  {
    private FilamentSpool.TypeEnum type;
    private string codeStr;
    private FilamentConstants.ColorsEnum color;
    private FilamentConstants.Branding brand;

    public Filament(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color, string codeStr, FilamentConstants.Branding brand)
    {
      this.type = type;
      this.color = color;
      this.codeStr = codeStr;
      this.brand = brand;
    }

    public Filament(Filament other)
    {
      this.type = other.type;
      this.codeStr = other.codeStr;
      this.color = other.color;
    }

    public FilamentSpool ToSpool()
    {
      return new FilamentSpool() { filament_color_code = (uint) this.color, filament_type = this.type, filament_temperature = FilamentConstants.Temperature.Default(this.type), filament_location = FilamentSpool.Location.External };
    }

    public FilamentSpool.TypeEnum Type
    {
      get
      {
        return this.type;
      }
    }

    public string TypeStr
    {
      get
      {
        return FilamentConstants.TypesToString(this.type);
      }
    }

    public string CodeStr
    {
      get
      {
        return this.codeStr;
      }
    }

    public FilamentConstants.ColorsEnum Color
    {
      get
      {
        return this.color;
      }
    }

    public string ColorStr
    {
      get
      {
        return FilamentConstants.ColorsToString(this.color);
      }
    }

    public FilamentConstants.Branding Brand
    {
      get
      {
        return this.brand;
      }
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      return this.Equals(obj as Filament);
    }

    public bool Equals(Filament b)
    {
      if (b == null || this.type != b.type || this.color != b.color)
        return false;
      return this.brand == b.brand;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
