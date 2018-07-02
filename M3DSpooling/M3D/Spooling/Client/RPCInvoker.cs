using M3D.Spooling.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.Spooling.Client
{
  public class RPCInvoker
  {
    private object class_instance;

    public RPCInvoker(object class_instance)
    {
      this.class_instance = class_instance;
    }

    public object CallMethod(object object_instance, RPCInvoker.RPC call)
    {
      MethodInfo method;
      if (call.parameters != null)
      {
        Type[] types = new Type[call.parameters.Length];
        for (var index = 0; index < call.parameters.Length; ++index)
        {
          types[index] = call.parameters[index].GetType();
        }

        method = object_instance.GetType().GetMethod(call.name, types);
      }
      else
      {
        method = object_instance.GetType().GetMethod(call.name);
      }

      if (method == null)
      {
        throw new MissingMethodException("the method is not in the class");
      }

      return method.Invoke(object_instance, call.parameters);
    }

    public object CallMethod(RPCInvoker.RPC call)
    {
      return CallMethod(class_instance, call);
    }

    public struct RPC
    {
      [XmlAttribute("name")]
      public string name;
      [XmlAttribute("callID")]
      public uint callID;
      [XmlIgnore]
      public PrinterSerialNumber serialnumber;
      [XmlIgnore]
      public Guid lockID;
      [XmlElement("FilamentSpool", Type = typeof (FilamentSpool))]
      [XmlElement("BedOffsets", Type = typeof (BedOffsets))]
      [XmlElement("BacklashSettings", Type = typeof (BacklashSettings))]
      [XmlElement("PrinterSerialNumber", Type = typeof (PrinterSerialNumber))]
      [XmlElement("Point5D", Type = typeof (Vector5D))]
      [XmlElement("Point4D", Type = typeof (Vector4D))]
      [XmlElement("JobOptions", Type = typeof (JobOptions))]
      [XmlElement("JobParams", Type = typeof (JobParams))]
      [XmlElement("VersionNumber", Type = typeof (VersionNumber))]
      [XmlElement("string", Type = typeof (string))]
      [XmlElement("uint", Type = typeof (uint))]
      [XmlElement("float", Type = typeof (float))]
      [XmlElement("int", Type = typeof (int))]
      [XmlElement("bool", Type = typeof (bool))]
      [XmlElement("stringarray", Type = typeof (string[]))]
      public object[] parameters;
      private static XmlSerializer __class_serializer;

      public RPC(string name, params object[] param)
      {
        this = new RPCInvoker.RPC(null, Guid.Empty, 0U, name, param);
      }

      public RPC(PrinterSerialNumber serialnumber, Guid lockID, uint callID, string name, params object[] param)
      {
        this.name = name;
        parameters = param;
        this.serialnumber = serialnumber;
        this.lockID = lockID;
        this.callID = callID;
      }

      public string Serialize()
      {
        var settings = new XmlWriterSettings
        {
          OmitXmlDeclaration = true
        };
        var stringWriter = new StringWriter();
        var xmlWriter = XmlWriter.Create(stringWriter, settings);
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", "");
        try
        {
          RPCInvoker.RPC.ClassSerializer.Serialize(xmlWriter, this, namespaces);
        }
        catch (Exception ex)
        {
          if (Debugger.IsAttached)
          {
            Debugger.Break();
          }

          throw;
        }
        return stringWriter.ToString();
      }

      public void Deserialize(string serialization)
      {
        using (var textReader = (TextReader) new StringReader(serialization))
        {
          this = (RPCInvoker.RPC) RPCInvoker.RPC.ClassSerializer.Deserialize(textReader);
        }
      }

      [XmlAttribute("printer")]
      public string PrinterSerial
      {
        get
        {
          if (!(serialnumber != null))
          {
            return null;
          }

          return serialnumber.ToString();
        }
        set
        {
          serialnumber = new PrinterSerialNumber(value);
        }
      }

      [XmlAttribute("lock")]
      public string LockSerial
      {
        get
        {
          return lockID.ToString();
        }
        set
        {
          lockID = Guid.Parse(value);
        }
      }

      private static XmlSerializer ClassSerializer
      {
        get
        {
          if (RPCInvoker.RPC.__class_serializer == null)
          {
            RPCInvoker.RPC.__class_serializer = new XmlSerializer(typeof (RPCInvoker.RPC));
          }

          return RPCInvoker.RPC.__class_serializer;
        }
      }
    }
  }
}
