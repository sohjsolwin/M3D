using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using M3D.Spooling.Sockets;
using System;
using System.IO;
using System.Xml.Serialization;

namespace M3D.Spooling.Core.Controllers
{
  internal abstract class BaseController
  {
    private object statusthreadsync = new object();
    protected bool broadcast_shutdown = true;
    private IBroadcastServer broadcastserver;
    protected Logger logger;
    protected PrinterConnection base_printer;
    protected PrinterInfo printerInfo;
    private InternalPrinterProfile printerProfile;

    public BaseController(PrinterConnection base_printer, PrinterInfo info, Logger logger, IBroadcastServer broadcastserver, InternalPrinterProfile printerProfile)
    {
      this.printerProfile = printerProfile;
      this.base_printer = base_printer;
      this.logger = logger;
      printerInfo = info;
      this.broadcastserver = broadcastserver;
    }

    protected bool WriteToSerial(byte[] command)
    {
      return base_printer.WriteToSerial(command);
    }

    protected string ReadExisting()
    {
      return base_printer.ReadExisting();
    }

    protected bool IsSerialOpen
    {
      get
      {
        return base_printer.IsOpen;
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      logger.WriteLog(text, type);
    }

    public bool GantryClipsRemoved
    {
      get
      {
        return PersistantDetails.GantryClipsRemoved;
      }
      set
      {
        PersistantDetails.GantryClipsRemoved = value;
        SavePersistantData();
      }
    }

    protected void LoadPersistantData()
    {
      var flag = false;
      if (MyPrinterInfo.serial_number != PrinterSerialNumber.Undefined)
      {
        var path = Path.Combine(Paths.SpoolerStorage, MyPrinterInfo.MySerialNumber + ".db2");
        try
        {
          var textReader = (TextReader) new StreamReader(path);
          PersistantDetails = (PersistantData) PersistantData.ClassSerializer.Deserialize(textReader);
          textReader.Close();
          flag = true;
        }
        catch (Exception ex)
        {
        }
      }
      if (!MyPrinterProfile.OptionsConstants.CheckGantryClips)
      {
        PersistantDetails.GantryClipsRemoved = true;
      }

      if (flag || CheckLegacyPrinterHistory())
      {
        return;
      }

      PersistantDetails = new PersistantData();
      if (!MyPrinterProfile.OptionsConstants.CheckGantryClips)
      {
        PersistantDetails.GantryClipsRemoved = true;
      }

      SavePersistantData();
    }

    public bool SavePersistantData()
    {
      var flag = true;
      if (MyPrinterInfo.serial_number != PrinterSerialNumber.Undefined && PersistantDetails != null)
      {
        var str = Path.Combine(Paths.SpoolerStorage, MyPrinterInfo.MySerialNumber + ".db2");
        try
        {
          var textWriter = (TextWriter) new StreamWriter(str);
          var namespaces = new XmlSerializerNamespaces();
          namespaces.Add(string.Empty, string.Empty);
          PersistantData.ClassSerializer.Serialize(textWriter, PersistantDetails, namespaces);
          textWriter.Close();
          FileUtils.GrantAccess(str);
        }
        catch (Exception ex)
        {
          flag = false;
        }
      }
      return flag;
    }

    private bool CheckLegacyPrinterHistory()
    {
      var path = Path.Combine(Paths.SpoolerStorage, "printerhistory.db2");
      try
      {
        var textReader = (TextReader) new StreamReader(path);
        string str;
        do
        {
          str = textReader.ReadLine();
          if (string.IsNullOrEmpty(str))
          {
            return false;
          }
        }
        while (!str.Contains("<Parameter Name=\"" + MyPrinterInfo.MySerialNumber + "\""));
        PersistantDetails = new PersistantData
        {
          GantryClipsRemoved = str.Contains("gantry_clips_removed=\"true\"")
        };
        SavePersistantData();
        return true;
      }
      catch (Exception ex)
      {
      }
      return false;
    }

    public abstract CommandResult WriteManualCommands(params string[] command);

    public abstract void DoConnectionLogic();

    public virtual void Shutdown()
    {
      SavePersistantData();
      if (!broadcast_shutdown || BroadcastServer == null)
      {
        return;
      }

      BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterShutdown, MySerialNumber, "null").Serialize());
    }

    public abstract void UpdateFirmware();

    public abstract void SetFanConstants(FanConstValues.FanType fanType);

    public abstract void SetExtruderCurrent(ushort current);

    public abstract bool Idle { get; }

    public abstract bool IsWorking { get; }

    public abstract bool HasActiveJob { get; }

    public abstract bool IsPrinting { get; }

    public bool IsPausedorPrinting
    {
      get
      {
        if (!IsPaused)
        {
          return IsPrinting;
        }

        return true;
      }
    }

    public abstract int GetJobsCount();

    public bool IsPaused
    {
      get
      {
        if (Status != PrinterStatus.Firmware_PrintingPaused)
        {
          return Status == PrinterStatus.Firmware_PrintingPausedProcessing;
        }

        return true;
      }
    }

    public bool IsPausedorPausing
    {
      get
      {
        if (!IsPaused)
        {
          return Status == PrinterStatus.Firmware_IsWaitingToPause;
        }

        return true;
      }
    }

    public ISerialPortIo SerialPort
    {
      get
      {
        return base_printer.SerialPort;
      }
    }

    public PrinterSerialNumber MySerialNumber
    {
      get
      {
        return MyPrinterInfo.serial_number;
      }
      set
      {
        MyPrinterInfo.serial_number = value;
      }
    }

    public Extruder ExtruderDetails
    {
      get
      {
        return MyPrinterInfo.extruder;
      }
    }

    public Hardware HardwareDetails
    {
      get
      {
        return MyPrinterInfo.hardware;
      }
    }

    public Calibration CalibrationDetails
    {
      get
      {
        return MyPrinterInfo.calibration;
      }
    }

    public Accessories AccessoryDetails
    {
      get
      {
        return MyPrinterInfo.accessories;
      }
    }

    public PersistantData PersistantDetails
    {
      get
      {
        return MyPrinterInfo.persistantData;
      }
      private set
      {
        MyPrinterInfo.persistantData = value;
      }
    }

    protected IBroadcastServer BroadcastServer
    {
      get
      {
        return broadcastserver;
      }
    }

    protected PrinterInfo MyPrinterInfo
    {
      get
      {
        return printerInfo;
      }
    }

    public PrinterStatus Status
    {
      get
      {
        lock (statusthreadsync)
        {
          return MyPrinterInfo.Status;
        }
      }
      protected set
      {
        lock (statusthreadsync)
        {
          MyPrinterInfo.Status = value;
        }
      }
    }

    public InternalPrinterProfile MyPrinterProfile
    {
      get
      {
        return printerProfile;
      }
    }
  }
}
