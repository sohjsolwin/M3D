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
