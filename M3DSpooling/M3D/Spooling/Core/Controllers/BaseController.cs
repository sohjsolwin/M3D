// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.BaseController
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.printerInfo = info;
      this.broadcastserver = broadcastserver;
    }

    protected bool WriteToSerial(byte[] command)
    {
      return this.base_printer.WriteToSerial(command);
    }

    protected string ReadExisting()
    {
      return this.base_printer.ReadExisting();
    }

    protected bool IsSerialOpen
    {
      get
      {
        return this.base_printer.IsOpen;
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      this.logger.WriteLog(text, type);
    }

    public bool GantryClipsRemoved
    {
      get
      {
        return this.PersistantDetails.GantryClipsRemoved;
      }
      set
      {
        this.PersistantDetails.GantryClipsRemoved = value;
        this.SavePersistantData();
      }
    }

    protected void LoadPersistantData()
    {
      bool flag = false;
      if (this.MyPrinterInfo.serial_number != PrinterSerialNumber.Undefined)
      {
        string path = Path.Combine(Paths.SpoolerStorage, this.MyPrinterInfo.MySerialNumber + ".db2");
        try
        {
          TextReader textReader = (TextReader) new StreamReader(path);
          this.PersistantDetails = (PersistantData) PersistantData.ClassSerializer.Deserialize(textReader);
          textReader.Close();
          flag = true;
        }
        catch (Exception ex)
        {
        }
      }
      if (!this.MyPrinterProfile.OptionsConstants.CheckGantryClips)
        this.PersistantDetails.GantryClipsRemoved = true;
      if (flag || this.CheckLegacyPrinterHistory())
        return;
      this.PersistantDetails = new PersistantData();
      if (!this.MyPrinterProfile.OptionsConstants.CheckGantryClips)
        this.PersistantDetails.GantryClipsRemoved = true;
      this.SavePersistantData();
    }

    public bool SavePersistantData()
    {
      bool flag = true;
      if (this.MyPrinterInfo.serial_number != PrinterSerialNumber.Undefined && this.PersistantDetails != null)
      {
        string str = Path.Combine(Paths.SpoolerStorage, this.MyPrinterInfo.MySerialNumber + ".db2");
        try
        {
          TextWriter textWriter = (TextWriter) new StreamWriter(str);
          XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
          namespaces.Add(string.Empty, string.Empty);
          PersistantData.ClassSerializer.Serialize(textWriter, (object) this.PersistantDetails, namespaces);
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
      string path = Path.Combine(Paths.SpoolerStorage, "printerhistory.db2");
      try
      {
        TextReader textReader = (TextReader) new StreamReader(path);
        string str;
        do
        {
          str = textReader.ReadLine();
          if (string.IsNullOrEmpty(str))
            return false;
        }
        while (!str.Contains("<Parameter Name=\"" + this.MyPrinterInfo.MySerialNumber + "\""));
        this.PersistantDetails = new PersistantData();
        this.PersistantDetails.GantryClipsRemoved = str.Contains("gantry_clips_removed=\"true\"");
        this.SavePersistantData();
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
      this.SavePersistantData();
      if (!this.broadcast_shutdown || this.BroadcastServer == null)
        return;
      this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterShutdown, this.MySerialNumber, "null").Serialize());
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
        if (!this.IsPaused)
          return this.IsPrinting;
        return true;
      }
    }

    public abstract int GetJobsCount();

    public bool IsPaused
    {
      get
      {
        if (this.Status != PrinterStatus.Firmware_PrintingPaused)
          return this.Status == PrinterStatus.Firmware_PrintingPausedProcessing;
        return true;
      }
    }

    public bool IsPausedorPausing
    {
      get
      {
        if (!this.IsPaused)
          return this.Status == PrinterStatus.Firmware_IsWaitingToPause;
        return true;
      }
    }

    public ISerialPortIo SerialPort
    {
      get
      {
        return this.base_printer.SerialPort;
      }
    }

    public PrinterSerialNumber MySerialNumber
    {
      get
      {
        return this.MyPrinterInfo.serial_number;
      }
      set
      {
        this.MyPrinterInfo.serial_number = value;
      }
    }

    public Extruder ExtruderDetails
    {
      get
      {
        return this.MyPrinterInfo.extruder;
      }
    }

    public Hardware HardwareDetails
    {
      get
      {
        return this.MyPrinterInfo.hardware;
      }
    }

    public Calibration CalibrationDetails
    {
      get
      {
        return this.MyPrinterInfo.calibration;
      }
    }

    public Accessories AccessoryDetails
    {
      get
      {
        return this.MyPrinterInfo.accessories;
      }
    }

    public PersistantData PersistantDetails
    {
      get
      {
        return this.MyPrinterInfo.persistantData;
      }
      private set
      {
        this.MyPrinterInfo.persistantData = value;
      }
    }

    protected IBroadcastServer BroadcastServer
    {
      get
      {
        return this.broadcastserver;
      }
    }

    protected PrinterInfo MyPrinterInfo
    {
      get
      {
        return this.printerInfo;
      }
    }

    public PrinterStatus Status
    {
      get
      {
        lock (this.statusthreadsync)
          return this.MyPrinterInfo.Status;
      }
      protected set
      {
        lock (this.statusthreadsync)
          this.MyPrinterInfo.Status = value;
      }
    }

    public InternalPrinterProfile MyPrinterProfile
    {
      get
      {
        return this.printerProfile;
      }
    }
  }
}
