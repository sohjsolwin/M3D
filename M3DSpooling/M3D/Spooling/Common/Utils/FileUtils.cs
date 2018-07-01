// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.FileUtils
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
        DirectorySecurity accessControl = directoryInfo.GetAccessControl();
        accessControl.AddAccessRule(new FileSystemAccessRule((IdentityReference) new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier) null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
        directoryInfo.SetAccessControl(accessControl);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
