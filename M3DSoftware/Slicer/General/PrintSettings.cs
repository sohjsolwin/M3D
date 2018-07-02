// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.PrintSettings
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Spooling.Common;

namespace M3D.Slicer.General
{
  public class PrintSettings
  {
    public Matrix4x4 transformation;
    public FilamentSpool filament_info;

    public PrintSettings()
    {
      transformation = new Matrix4x4();
      SetPrintDefaults();
    }

    public void SetPrintDefaults()
    {
      transformation.Identity();
      filament_info = new FilamentSpool();
    }
  }
}
