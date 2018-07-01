// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.SoundManager
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Media;
using System.Reflection;

namespace M3D.GUI.Controller
{
  public static class SoundManager
  {
    public static void playSound(object sender)
    {
      new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("M3D.Resources.filename")).Play();
    }
  }
}
