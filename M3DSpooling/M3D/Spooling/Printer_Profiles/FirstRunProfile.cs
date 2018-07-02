using M3D.Spooling.FirstRunUpdates;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class FirstRunProfile
  {
    public readonly List<IFirstRunUpdater> updater_list;

    public FirstRunProfile(params IFirstRunUpdater[] updater)
    {
      updater_list = new List<IFirstRunUpdater>(updater);
    }
  }
}
