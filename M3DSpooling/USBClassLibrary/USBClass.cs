// Decompiled with JetBrains decompiler
// Type: USBClassLibrary.USBClass
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace USBClassLibrary
{
  public class USBClass
  {
    private const long INVALID_HANDLE_VALUE = -1;
    private const int BUFFER_SIZE = 1024;
    private IntPtr deviceEventHandle;

    public event USBClass.USBDeviceEventHandler USBDeviceAttached;

    public event USBClass.USBDeviceEventHandler USBDeviceRemoved;

    public event USBClass.USBDeviceEventHandler USBDeviceQueryRemove;

    public bool IsQueryHooked
    {
      get
      {
        return !(deviceEventHandle == IntPtr.Zero);
      }
    }

    public bool RegisterForDeviceChange(bool Register, IntPtr WindowsHandle)
    {
      var flag = false;
      if (Register)
      {
        var structure = new USBClass.Win32Wrapper.DEV_BROADCAST_DEVICEINTERFACE();
        var cb = Marshal.SizeOf<USBClass.Win32Wrapper.DEV_BROADCAST_DEVICEINTERFACE>(structure);
        structure.dbcc_size = cb;
        structure.dbcc_devicetype = 5;
        var num1 = new IntPtr();
        IntPtr num2 = Marshal.AllocHGlobal(cb);
        Marshal.StructureToPtr<USBClass.Win32Wrapper.DEV_BROADCAST_DEVICEINTERFACE>(structure, num2, true);
        deviceEventHandle = Win32Wrapper.RegisterDeviceNotification(WindowsHandle, num2, 4);
        flag = deviceEventHandle != IntPtr.Zero;
        if (!flag)
        {
          Marshal.GetLastWin32Error();
        }

        Marshal.FreeHGlobal(num2);
      }
      else
      {
        if (deviceEventHandle != IntPtr.Zero)
        {
          flag = Win32Wrapper.UnregisterDeviceNotification(deviceEventHandle);
        }

        deviceEventHandle = IntPtr.Zero;
      }
      return flag;
    }

    public void ProcessWindowsMessage(int Msg, IntPtr WParam, IntPtr LParam, ref bool handled)
    {
      if (Msg != 537)
      {
        return;
      }

      switch (WParam.ToInt32())
      {
        case 32768:
          if (Marshal.ReadInt32(LParam, 4) != 5)
          {
            break;
          }

          handled = true;
          // ISSUE: reference to a compiler-generated field
          USBDeviceAttached((object) this, new USBClass.USBDeviceEventArgs());
          break;
        case 32769:
          if (Marshal.ReadInt32(LParam, 4) != 5)
          {
            break;
          }

          handled = true;
          // ISSUE: reference to a compiler-generated field
          USBDeviceQueryRemove((object) this, new USBClass.USBDeviceEventArgs());
          break;
        case 32772:
          handled = true;
          if (Marshal.ReadInt32(LParam, 4) != 5)
          {
            break;
          }
          // ISSUE: reference to a compiler-generated field
          USBDeviceRemoved((object) this, new USBClass.USBDeviceEventArgs());
          break;
      }
    }

    public static bool GetUSBDevice(uint VID, uint PID, List<USBClass.DeviceProperties> DPList, bool GetCOMPort, uint? MI = null)
    {
      IntPtr num1 = Marshal.AllocHGlobal(1024);
      IntPtr num2 = IntPtr.Zero;
      DPList.Clear();
      try
      {
        var Enumerator = "USB";
        var empty = string.Empty;
        var str = string.Empty;
        var lowerInvariant1 = ("VID_" + VID.ToString("X4") + "&PID_" + PID.ToString("X4")).ToLowerInvariant();
        if (MI.HasValue)
        {
          str = ("MI_" + MI.Value.ToString("X2")).ToLowerInvariant();
        }

        num2 = Win32Wrapper.SetupDiGetClassDevs(IntPtr.Zero, Enumerator, IntPtr.Zero, 6);
        if (num2.ToInt64() != -1L)
        {
          var flag = true;
          uint MemberIndex = 0;
          while (flag)
          {
            if (flag)
            {
              uint RequiredSize = 0;
              uint PropertyRegDataType = 0;
              var DeviceInfoData = new USBClass.Win32Wrapper.SP_DEVINFO_DATA();
              DeviceInfoData.cbSize = (uint) Marshal.SizeOf<USBClass.Win32Wrapper.SP_DEVINFO_DATA>(DeviceInfoData);
              flag = Win32Wrapper.SetupDiEnumDeviceInfo(num2, MemberIndex, ref DeviceInfoData);
              if (flag)
              {
                Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 1U, ref PropertyRegDataType, IntPtr.Zero, 0U, ref RequiredSize);
                if (Marshal.GetLastWin32Error() == 122 && RequiredSize <= 1024U && Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 1U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                {
                  var lowerInvariant2 = Marshal.PtrToStringAuto(num1).ToLowerInvariant();
                  if (lowerInvariant2.Contains(lowerInvariant1) && (MI.HasValue && lowerInvariant2.Contains(str) || !MI.HasValue && !lowerInvariant2.Contains("mi")))
                  {
                    var deviceProperties = new DeviceProperties
                    {
                      FriendlyName = string.Empty
                    };
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 12U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.FriendlyName = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DeviceType = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 25U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DeviceType = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DeviceClass = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 7U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DeviceClass = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DeviceManufacturer = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 11U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DeviceManufacturer = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DeviceLocation = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 13U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DeviceLocation = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DevicePath = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 35U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DevicePath = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DevicePhysicalObjectName = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 14U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DevicePhysicalObjectName = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.DeviceDescription = string.Empty;
                    if (Win32Wrapper.SetupDiGetDeviceRegistryProperty(num2, ref DeviceInfoData, 0U, ref PropertyRegDataType, num1, 1024U, ref RequiredSize))
                    {
                      deviceProperties.DeviceDescription = Marshal.PtrToStringAuto(num1);
                    }

                    deviceProperties.COMPort = string.Empty;
                    if (GetCOMPort)
                    {
                      IntPtr hKey = Win32Wrapper.SetupDiOpenDevRegKey(num2, ref DeviceInfoData, 1U, 0U, 1U, 131097U);
                      if (hKey.ToInt32() == -1)
                      {
                        deviceProperties.COMPort = (string) null;
                      }
                      else
                      {
                        var lpData = new StringBuilder(1024);
                        var capacity = (uint) lpData.Capacity;
                        if (Win32Wrapper.RegQueryValueEx(hKey, "PortName", 0U, out var lpType, lpData, ref capacity) == 0)
                        {
                          deviceProperties.COMPort = lpData.ToString();
                        }

                        Win32Wrapper.RegCloseKey(hKey);
                      }
                    }
                    DPList.Add(deviceProperties);
                  }
                }
              }
            }
            else
            {
              var lastWin32Error = (long) Marshal.GetLastWin32Error();
            }
            ++MemberIndex;
          }
        }
        return DPList.Count > 0;
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        Win32Wrapper.SetupDiDestroyDeviceInfoList(num2);
        Marshal.FreeHGlobal(num1);
      }
    }

    ~USBClass()
    {
      RegisterForDeviceChange(false, IntPtr.Zero);
    }

    private class Win32Wrapper
    {
      public const int WM_DEVICECHANGE = 537;

      [DllImport("user32.dll", SetLastError = true)]
      public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool UnregisterDeviceNotification(IntPtr hHandle);

      [DllImport("setupapi.dll", SetLastError = true)]
      public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref USBClass.Win32Wrapper.SP_DEVINFO_DATA DeviceInfoData);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref USBClass.Win32Wrapper.SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref USBClass.Win32Wrapper.SP_DEVICE_INTERFACE_DATA deviceInterfaceData, ref USBClass.Win32Wrapper.SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref USBClass.Win32Wrapper.SP_DEVINFO_DATA deviceInfoData);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, int Flags);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref USBClass.Win32Wrapper.SP_DEVINFO_DATA DeviceInfoData, uint Property, ref uint PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, ref uint RequiredSize);

      [DllImport("setupapi.dll")]
      public static extern int CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, int ulFlags);

      [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
      public static extern int CM_Get_Device_ID(uint dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);

      [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref USBClass.Win32Wrapper.SP_DEVINFO_DATA DeviceInfoData, uint Scope, uint HwProfile, uint KeyType, uint samDesired);

      [DllImport("advapi32.dll", EntryPoint = "RegQueryValueExW", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, uint lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern int RegCloseKey(IntPtr hKey);

      [StructLayout(LayoutKind.Sequential, Size = 1)]
      public struct GUID_DEVINTERFACE
      {
        public const string DISK = "53f56307-b6bf-11d0-94f2-00a0c91efb8b";
        public const string HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
        public const string MODEM = "2C7089AA-2E0E-11D1-B114-00C04FC2AAE4";
        public const string SERENUM_BUS_ENUMERATOR = "4D36E978-E325-11CE-BFC1-08002BE10318";
        public const string COMPORT = "86E0D1E0-8089-11D0-9CE4-08003E301F73";
        public const string PARALLEL = "97F76EF0-F883-11D0-AF1F-0000F800845C";
      }

      [Flags]
      public enum DEVICE_NOTIFY : uint
      {
        DEVICE_NOTIFY_WINDOW_HANDLE = 0,
        DEVICE_NOTIFY_SERVICE_HANDLE = 1,
        DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4,
      }

      public enum DBTDEVICE : uint
      {
        DBT_DEVICEARRIVAL = 32768, // 0x00008000
        DBT_DEVICEQUERYREMOVE = 32769, // 0x00008001
        DBT_DEVICEQUERYREMOVEFAILED = 32770, // 0x00008002
        DBT_DEVICEREMOVEPENDING = 32771, // 0x00008003
        DBT_DEVICEREMOVECOMPLETE = 32772, // 0x00008004
        DBT_DEVICETYPESPECIFIC = 32773, // 0x00008005
        DBT_CUSTOMEVENT = 32774, // 0x00008006
      }

      public enum DBTDEVTYP : uint
      {
        DBT_DEVTYP_OEM,
        DBT_DEVTYP_DEVNODE,
        DBT_DEVTYP_VOLUME,
        DBT_DEVTYP_PORT,
        DBT_DEVTYP_NET,
        DBT_DEVTYP_DEVICEINTERFACE,
        DBT_DEVTYP_HANDLE,
      }

      public enum REGKEYSECURITY : uint
      {
        KEY_QUERY_VALUE = 1,
        KEY_SET_VALUE = 2,
        KEY_CREATE_SUB_KEY = 4,
        KEY_ENUMERATE_SUB_KEYS = 8,
        KEY_NOTIFY = 16, // 0x00000010
        KEY_CREATE_LINK = 32, // 0x00000020
        KEY_WOW64_64KEY = 256, // 0x00000100
        KEY_WOW64_32KEY = 512, // 0x00000200
        KEY_WRITE = 131078, // 0x00020006
        KEY_EXECUTE = 131097, // 0x00020019
        KEY_READ = 131097, // 0x00020019
        KEY_ALL_ACCESS = 983103, // 0x000F003F
      }

      [Flags]
      public enum DIGCF
      {
        DIGCF_DEFAULT = 1,
        DIGCF_PRESENT = 2,
        DIGCF_ALLCLASSES = 4,
        DIGCF_PROFILE = 8,
        DIGCF_DEVICEINTERFACE = 16, // 0x00000010
      }

      public enum DICS_FLAG : uint
      {
        DICS_FLAG_GLOBAL = 1,
        DICS_FLAG_CONFIGSPECIFIC = 2,
        DICS_FLAG_CONFIGGENERAL = 4,
      }

      public enum DIREG : uint
      {
        DIREG_DEV = 1,
        DIREG_DRV = 2,
        DIREG_BOTH = 4,
      }

      public enum WinErrors : long
      {
        ERROR_SUCCESS = 0,
        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_PATH_NOT_FOUND = 3,
        ERROR_TOO_MANY_OPEN_FILES = 4,
        ERROR_ACCESS_DENIED = 5,
        ERROR_INVALID_HANDLE = 6,
        ERROR_ARENA_TRASHED = 7,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_INVALID_BLOCK = 9,
        ERROR_BAD_ENVIRONMENT = 10, // 0x000000000000000A
        ERROR_BAD_FORMAT = 11, // 0x000000000000000B
        ERROR_INVALID_ACCESS = 12, // 0x000000000000000C
        ERROR_INVALID_DATA = 13, // 0x000000000000000D
        ERROR_OUTOFMEMORY = 14, // 0x000000000000000E
        ERROR_INSUFFICIENT_BUFFER = 122, // 0x000000000000007A
        ERROR_MORE_DATA = 234, // 0x00000000000000EA
        ERROR_NO_MORE_ITEMS = 259, // 0x0000000000000103
        ERROR_SERVICE_SPECIFIC_ERROR = 1066, // 0x000000000000042A
        ERROR_INVALID_USER_BUFFER = 1784, // 0x00000000000006F8
      }

      public enum CRErrorCodes
      {
        CR_SUCCESS,
        CR_DEFAULT,
        CR_OUT_OF_MEMORY,
        CR_INVALID_POINTER,
        CR_INVALID_FLAG,
        CR_INVALID_DEVNODE,
        CR_INVALID_RES_DES,
        CR_INVALID_LOG_CONF,
        CR_INVALID_ARBITRATOR,
        CR_INVALID_NODELIST,
        CR_DEVNODE_HAS_REQS,
        CR_INVALID_RESOURCEID,
        CR_DLVXD_NOT_FOUND,
        CR_NO_SUCH_DEVNODE,
        CR_NO_MORE_LOG_CONF,
        CR_NO_MORE_RES_DES,
        CR_ALREADY_SUCH_DEVNODE,
        CR_INVALID_RANGE_LIST,
        CR_INVALID_RANGE,
        CR_FAILURE,
        CR_NO_SUCH_LOGICAL_DEV,
        CR_CREATE_BLOCKED,
        CR_NOT_SYSTEM_VM,
        CR_REMOVE_VETOED,
        CR_APM_VETOED,
        CR_INVALID_LOAD_TYPE,
        CR_BUFFER_SMALL,
        CR_NO_ARBITRATOR,
        CR_NO_REGISTRY_HANDLE,
        CR_REGISTRY_ERROR,
        CR_INVALID_DEVICE_ID,
        CR_INVALID_DATA,
        CR_INVALID_API,
        CR_DEVLOADER_NOT_READY,
        CR_NEED_RESTART,
        CR_NO_MORE_HW_PROFILES,
        CR_DEVICE_NOT_THERE,
        CR_NO_SUCH_VALUE,
        CR_WRONG_TYPE,
        CR_INVALID_PRIORITY,
        CR_NOT_DISABLEABLE,
        CR_FREE_RESOURCES,
        CR_QUERY_VETOED,
        CR_CANT_SHARE_IRQ,
        CR_NO_DEPENDENT,
        CR_SAME_RESOURCES,
        CR_NO_SUCH_REGISTRY_KEY,
        CR_INVALID_MACHINENAME,
        CR_REMOTE_COMM_FAILURE,
        CR_MACHINE_UNAVAILABLE,
        CR_NO_CM_SERVICES,
        CR_ACCESS_DENIED,
        CR_CALL_NOT_IMPLEMENTED,
        CR_INVALID_PROPERTY,
        CR_DEVICE_INTERFACE_ACTIVE,
        CR_NO_SUCH_DEVICE_INTERFACE,
        CR_INVALID_REFERENCE_STRING,
        CR_INVALID_CONFLICT_LIST,
        CR_INVALID_INDEX,
        CR_INVALID_STRUCTURE_SIZE,
        NUM_CR_RESULTS,
      }

      public enum SPDRP
      {
        SPDRP_DEVICEDESC,
        SPDRP_HARDWAREID,
        SPDRP_COMPATIBLEIDS,
        SPDRP_UNUSED0,
        SPDRP_SERVICE,
        SPDRP_UNUSED1,
        SPDRP_UNUSED2,
        SPDRP_CLASS,
        SPDRP_CLASSGUID,
        SPDRP_DRIVER,
        SPDRP_CONFIGFLAGS,
        SPDRP_MFG,
        SPDRP_FRIENDLYNAME,
        SPDRP_LOCATION_INFORMATION,
        SPDRP_PHYSICAL_DEVICE_OBJECT_NAME,
        SPDRP_CAPABILITIES,
        SPDRP_UI_NUMBER,
        SPDRP_UPPERFILTERS,
        SPDRP_LOWERFILTERS,
        SPDRP_BUSTYPEGUID,
        SPDRP_LEGACYBUSTYPE,
        SPDRP_BUSNUMBER,
        SPDRP_ENUMERATOR_NAME,
        SPDRP_SECURITY,
        SPDRP_SECURITY_SDS,
        SPDRP_DEVTYPE,
        SPDRP_EXCLUSIVE,
        SPDRP_CHARACTERISTICS,
        SPDRP_ADDRESS,
        SPDRP_UI_NUMBER_DESC_FORMAT,
        SPDRP_DEVICE_POWER_DATA,
        SPDRP_REMOVAL_POLICY,
        SPDRP_REMOVAL_POLICY_HW_DEFAULT,
        SPDRP_REMOVAL_POLICY_OVERRIDE,
        SPDRP_INSTALL_STATE,
        SPDRP_LOCATION_PATHS,
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct SP_DEVINFO_DATA
      {
        public uint cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct SP_DEVICE_INTERFACE_DATA
      {
        public uint cbSize;
        public Guid interfaceClassGuid;
        public uint flags;
        private IntPtr reserved;
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct SP_DEVICE_INTERFACE_DETAIL_DATA
      {
        public uint cbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string devicePath;
      }

      [StructLayout(LayoutKind.Explicit)]
      private struct DevBroadcastDeviceInterfaceBuffer
      {
        [FieldOffset(0)]
        public int dbch_size;
        [FieldOffset(4)]
        public int dbch_devicetype;
        [FieldOffset(8)]
        public int dbch_reserved;

        public DevBroadcastDeviceInterfaceBuffer(int deviceType)
        {
          dbch_size = Marshal.SizeOf(typeof (USBClass.Win32Wrapper.DevBroadcastDeviceInterfaceBuffer));
          dbch_devicetype = deviceType;
          dbch_reserved = 0;
        }
      }

      public struct DEV_BROADCAST_HDR
      {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
      }

      public struct DEV_BROADCAST_HANDLE
      {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
        public IntPtr dbch_handle;
        public IntPtr dbch_hdevnotify;
        public Guid dbch_eventguid;
        public long dbch_nameoffset;
        public byte dbch_data;
        public byte dbch_data1;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      public struct DEV_BROADCAST_DEVICEINTERFACE
      {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
        public byte[] dbcc_classguid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] dbcc_name;
      }

      public struct DEV_BROADCAST_VOLUME
      {
        public int dbcv_size;
        public int dbcv_devicetype;
        public int dbcv_reserved;
        public int dbcv_unitmask;
        public short dbcv_flags;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      public struct DEV_BROADCAST_PORT
      {
        public int dbcp_size;
        public int dbcp_devicetype;
        public int dbcp_reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] dbcp_name;
      }

      public struct DEV_BROADCAST_OEM
      {
        public int dbco_size;
        public int dbco_devicetype;
        public int dbco_reserved;
        public int dbco_identifier;
        public int dbco_suppfunc;
      }
    }

    public delegate void USBDeviceEventHandler(object sender, USBClass.USBDeviceEventArgs e);

    public class USBDeviceEventArgs : EventArgs
    {
      public bool Cancel;
      public bool HookQueryRemove;

      public USBDeviceEventArgs()
      {
        Cancel = false;
        HookQueryRemove = false;
      }
    }

    public struct DeviceProperties
    {
      public string FriendlyName;
      public string DeviceDescription;
      public string DeviceType;
      public string DeviceManufacturer;
      public string DeviceClass;
      public string DeviceLocation;
      public string DevicePath;
      public string DevicePhysicalObjectName;
      public string COMPort;
    }
  }
}
