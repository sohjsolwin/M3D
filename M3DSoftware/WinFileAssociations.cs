// Decompiled with JetBrains decompiler
// Type: M3D.WinFileAssociations
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI.Interfaces;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace M3D
{
  public class WinFileAssociations : IFileAssociations
  {
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    public string ExtensionOpenWith(string Extension)
    {
      RegistryKey registryKey1 = (RegistryKey) null;
      RegistryKey registryKey2 = (RegistryKey) null;
      string str1 = (string) null;
      try
      {
        registryKey1 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + Extension);
        object obj = registryKey1.GetValue("");
        if (obj != null)
        {
          string str2 = obj.ToString();
          registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + str2);
          if (registryKey2 != null)
            str1 = registryKey2.OpenSubKey("Shell").OpenSubKey("open").OpenSubKey("command").GetValue("").ToString();
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        registryKey1?.Close();
        registryKey2?.Close();
      }
      return str1;
    }

    public void Set3DFileAssociation(string Extension, string KeyName, string OpenWith, string FileDescription, string fileIcon)
    {
      RegistryKey registryKey1 = (RegistryKey) null;
      RegistryKey registryKey2 = (RegistryKey) null;
      RegistryKey registryKey3 = (RegistryKey) null;
      RegistryKey registryKey4 = (RegistryKey) null;
      bool flag;
      try
      {
        registryKey1 = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + Extension);
        registryKey1.SetValue("", (object) KeyName);
        registryKey1.SetValue("DefaultIcon", (object) fileIcon, RegistryValueKind.String);
        registryKey2 = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + KeyName);
        registryKey2.SetValue("", (object) FileDescription);
        registryKey2.CreateSubKey("DefaultIcon").SetValue("", (object) ("\"" + fileIcon + "\",0"));
        registryKey3 = registryKey2.CreateSubKey("Shell");
        registryKey3.CreateSubKey("open").CreateSubKey("command").SetValue("", (object) ("\"" + OpenWith + "\" \"%1\""));
        registryKey4 = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
        registryKey4?.DeleteSubKey("UserChoice", false);
        WinFileAssociations.SHChangeNotify(134217728U, 0U, IntPtr.Zero, IntPtr.Zero);
        flag = true;
      }
      catch (Exception ex)
      {
        flag = false;
      }
      finally
      {
        registryKey1?.Close();
        registryKey2?.Close();
        registryKey3?.Close();
        registryKey4?.Close();
      }
      if (flag)
        return;
      this.Delete3DFileAssociation(Extension, KeyName);
    }

    public void Delete3DFileAssociation(string Extension, string KeyName)
    {
      this.Delete3DFileAssociation("Software\\Classes\\" + Extension);
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\Shell\\edit\\command");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\Shell\\open\\command");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\Shell\\edit");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\Shell\\open");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\Shell");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName + "\\DefaultIcon");
      this.Delete3DFileAssociation("Software\\Classes\\" + KeyName);
    }

    private void Delete3DFileAssociation(string subKey)
    {
      try
      {
        Registry.CurrentUser.DeleteSubKey(subKey);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
