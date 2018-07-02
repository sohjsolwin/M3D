
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SerialPortTester
{
  public class SerialPortFixer : IDisposable
  {
    private const int DcbFlagAbortOnError = 14;
    private const int CommStateRetries = 10;
    private SafeFileHandle m_Handle;

    public static void Execute(string portName)
    {
      using (new SerialPortFixer(portName))
      {
        ;
      }
    }

    public void Dispose()
    {
      if (m_Handle == null)
      {
        return;
      }

      m_Handle.Close();
      m_Handle = null;
    }

    private SerialPortFixer(string portName)
    {
      if (portName == null || !portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Invalid Serial Port", nameof (portName));
      }

      SafeFileHandle file = SerialPortFixer.CreateFile("\\\\.\\" + portName, -1073741824, 0, IntPtr.Zero, 3, 1073741824, IntPtr.Zero);
      if (file.IsInvalid)
      {
        SerialPortFixer.WinIoError();
      }

      try
      {
        switch (SerialPortFixer.GetFileType(file))
        {
          case 0:
          case 2:
            m_Handle = file;
            InitializeDcb();
            break;
          default:
            throw new ArgumentException("Invalid Serial Port", nameof (portName));
        }
      }
      catch
      {
        file.Close();
        m_Handle = null;
        throw;
      }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetCommState(SafeFileHandle hFile, ref SerialPortFixer.Dcb lpDcb);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetCommState(SafeFileHandle hFile, ref SerialPortFixer.Dcb lpDcb);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, ref SerialPortFixer.Comstat lpStat);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr securityAttrs, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetFileType(SafeFileHandle hFile);

    private void InitializeDcb()
    {
      var lpDcb = new SerialPortFixer.Dcb();
      GetCommStateNative(ref lpDcb);
      lpDcb.Flags &= 4294950911U;
      SetCommStateNative(ref lpDcb);
    }

    private static string GetMessage(int errorCode)
    {
      var lpBuffer = new StringBuilder(512);
      if (SerialPortFixer.FormatMessage(12800, new HandleRef(null, IntPtr.Zero), errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0)
      {
        return lpBuffer.ToString();
      }

      return "Unknown Error";
    }

    private static int MakeHrFromErrorCode(int errorCode)
    {
      return -2147024896 | errorCode;
    }

    private static void WinIoError()
    {
      var lastWin32Error = Marshal.GetLastWin32Error();
      throw new IOException(SerialPortFixer.GetMessage(lastWin32Error), SerialPortFixer.MakeHrFromErrorCode(lastWin32Error));
    }

    private void GetCommStateNative(ref SerialPortFixer.Dcb lpDcb)
    {
      var lpErrors = 0;
      var lpStat = new SerialPortFixer.Comstat();
      for (var index = 0; index < 10; ++index)
      {
        if (!SerialPortFixer.ClearCommError(m_Handle, ref lpErrors, ref lpStat))
        {
          SerialPortFixer.WinIoError();
        }

        if (SerialPortFixer.GetCommState(m_Handle, ref lpDcb))
        {
          break;
        }

        if (index == 9)
        {
          SerialPortFixer.WinIoError();
        }
      }
    }

    private void SetCommStateNative(ref SerialPortFixer.Dcb lpDcb)
    {
      var lpErrors = 0;
      var lpStat = new SerialPortFixer.Comstat();
      for (var index = 0; index < 10; ++index)
      {
        if (!SerialPortFixer.ClearCommError(m_Handle, ref lpErrors, ref lpStat))
        {
          SerialPortFixer.WinIoError();
        }

        if (SerialPortFixer.SetCommState(m_Handle, ref lpDcb))
        {
          break;
        }

        if (index == 9)
        {
          SerialPortFixer.WinIoError();
        }
      }
    }

    private struct Comstat
    {
      public readonly uint Flags;
      public readonly uint cbInQue;
      public readonly uint cbOutQue;
    }

    private struct Dcb
    {
      public readonly uint DCBlength;
      public readonly uint BaudRate;
      public uint Flags;
      public readonly ushort wReserved;
      public readonly ushort XonLim;
      public readonly ushort XoffLim;
      public readonly byte ByteSize;
      public readonly byte Parity;
      public readonly byte StopBits;
      public readonly byte XonChar;
      public readonly byte XoffChar;
      public readonly byte ErrorChar;
      public readonly byte EofChar;
      public readonly byte EvtChar;
      public readonly ushort wReserved1;
    }
  }
}
