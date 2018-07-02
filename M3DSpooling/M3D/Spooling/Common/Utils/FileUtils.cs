using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace M3D.Spooling.Common.Utils
{
  public static class FileUtils
  {
    private static void Unused(object o)
    {
    }

    public static void GrantAccess(string fullPath)
    {
      try
      {
        var directoryInfo = new DirectoryInfo(fullPath);
        DirectorySecurity accessControl = directoryInfo.GetAccessControl();
        accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier)null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
        directoryInfo.SetAccessControl(accessControl);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
