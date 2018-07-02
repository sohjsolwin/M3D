using M3D.Spooling.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace M3D.Spooling.ConnectionManager
{
  public class MacUSBPrinterFinder : IUSBPrinterFinder
  {
    private int BUFFER_LENGTH = 30;
    private List<string> com_ports = new List<string>();
    private int usbVendor;
    private int usbProduct;

    public override List<string> UsbAttached(uint VID, uint PID)
    {
      usbVendor = (int) VID;
      usbProduct = (int) PID;
      lock (com_ports)
      {
        com_ports.Clear();
        try
        {
          if (!refreshComPorts(out com_ports))
          {
            return new List<string>();
          }
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Error in USB: " + ex.Message);
        }
      }
      return new List<string>(com_ports);
    }

    [DllImport("MacUSB.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern MacUSBPrinterFinder.RefType refreshComPorts(int usbVendor, int usbProduct, StringBuilder buffer, ref int bufferLenght);

    public bool refreshComPorts(out List<string> comPorts)
    {
      var buffer = new StringBuilder(BUFFER_LENGTH);
      comPorts = null;
      MacUSBPrinterFinder.RefType refType;
      do
      {
        var capacity = buffer.Capacity;
        refType = MacUSBPrinterFinder.refreshComPorts(usbVendor, usbProduct, buffer, ref capacity);
        if (refType == MacUSBPrinterFinder.RefType.Failure)
        {
          return false;
        }

        if (refType == MacUSBPrinterFinder.RefType.BufferToSmall)
        {
          BUFFER_LENGTH = capacity;
          buffer = new StringBuilder(BUFFER_LENGTH + 1);
        }
      }
      while (refType != MacUSBPrinterFinder.RefType.Success);
      string[] strArray = buffer.ToString().Split(new char[1]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
      comPorts = new List<string>(strArray);
      return true;
    }

    private enum RefType
    {
      Failure,
      Success,
      BufferToSmall,
    }
  }
}
