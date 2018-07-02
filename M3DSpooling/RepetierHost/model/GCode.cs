// Decompiled with JetBrains decompiler
// Type: RepetierHost.model.GCode
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RepetierHost.model
{
  internal class GCode
  {
    public static NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;
    public static string floatNoExp = "0.#####";
    private ushort fields = 128;
    public bool forceAscii;
    public bool hostCommand;
    public bool comment;
    public string orig;
    private ushort fields2;
    private int n;
    private ushort g;
    private ushort m;
    private byte t;
    private float x;
    private float y;
    private float z;
    private float e;
    private float f;
    private float ii;
    private float j;
    private float r;
    private int s;
    private int p;
    private string text;

    public GCode(string s)
    {
      Parse(s);
    }

    public GCode()
    {
    }

    public bool hasCode
    {
      get
      {
        return fields != (ushort) 128;
      }
    }

    public bool hasText
    {
      get
      {
        return ((uint)fields & 32768U) > 0U;
      }
    }

    public bool isOnlyComment
    {
      get
      {
        if (fields == (ushort) 128)
        {
          return comment;
        }

        return false;
      }
    }

    public string Text
    {
      get
      {
        return text;
      }
      set
      {
        text = PrinterCompatibleString.RemoveIllegalCharacters(value);
        if (text.Length > 16)
        {
          ActivateV2OrForceAscii();
        }

        fields |= (ushort) 32768;
      }
    }

    public bool hasN
    {
      get
      {
        return ((uint)fields & 1U) > 0U;
      }
    }

    public int N
    {
      get
      {
        return n;
      }
      set
      {
        n = value;
        fields |= (ushort) 1;
      }
    }

    public bool hasM
    {
      get
      {
        return ((uint)fields & 2U) > 0U;
      }
    }

    public ushort M
    {
      get
      {
        return m;
      }
      set
      {
        m = value;
        fields |= (ushort) 2;
      }
    }

    public bool hasG
    {
      get
      {
        return ((uint)fields & 4U) > 0U;
      }
    }

    public ushort G
    {
      get
      {
        return g;
      }
      set
      {
        g = value;
        fields |= (ushort) 4;
      }
    }

    public bool hasT
    {
      get
      {
        return ((uint)fields & 512U) > 0U;
      }
    }

    public byte T
    {
      get
      {
        return t;
      }
      set
      {
        t = value;
        fields |= (ushort) 512;
      }
    }

    public bool hasS
    {
      get
      {
        return ((uint)fields & 1024U) > 0U;
      }
    }

    public int S
    {
      get
      {
        return s;
      }
      set
      {
        s = value;
        fields |= (ushort) 1024;
      }
    }

    public bool hasP
    {
      get
      {
        return ((uint)fields & 2048U) > 0U;
      }
    }

    public int P
    {
      get
      {
        return p;
      }
      set
      {
        p = value;
        fields |= (ushort) 2048;
      }
    }

    public bool hasX
    {
      get
      {
        return ((uint)fields & 8U) > 0U;
      }
    }

    public float X
    {
      get
      {
        return x;
      }
      set
      {
        x = value;
        fields |= (ushort) 8;
      }
    }

    public bool hasY
    {
      get
      {
        return ((uint)fields & 16U) > 0U;
      }
    }

    public float Y
    {
      get
      {
        return y;
      }
      set
      {
        y = value;
        fields |= (ushort) 16;
      }
    }

    public bool hasZ
    {
      get
      {
        return ((uint)fields & 32U) > 0U;
      }
    }

    public float Z
    {
      get
      {
        return z;
      }
      set
      {
        z = value;
        fields |= (ushort) 32;
      }
    }

    public bool hasE
    {
      get
      {
        return ((uint)fields & 64U) > 0U;
      }
    }

    public float E
    {
      get
      {
        return e;
      }
      set
      {
        e = value;
        fields |= (ushort) 64;
      }
    }

    public bool hasF
    {
      get
      {
        return ((uint)fields & 256U) > 0U;
      }
    }

    public float F
    {
      get
      {
        return f;
      }
      set
      {
        f = value;
        fields |= (ushort) 256;
      }
    }

    public bool hasI
    {
      get
      {
        return ((uint)fields2 & 1U) > 0U;
      }
    }

    public float I
    {
      get
      {
        return ii;
      }
      set
      {
        ii = value;
        fields2 |= (ushort) 1;
        ActivateV2OrForceAscii();
      }
    }

    public bool hasJ
    {
      get
      {
        return ((uint)fields2 & 2U) > 0U;
      }
    }

    public float J
    {
      get
      {
        return j;
      }
      set
      {
        j = value;
        fields2 |= (ushort) 2;
        ActivateV2OrForceAscii();
      }
    }

    public bool hasR
    {
      get
      {
        return ((uint)fields2 & 4U) > 0U;
      }
    }

    public float R
    {
      get
      {
        return r;
      }
      set
      {
        r = value;
        fields2 |= (ushort) 4;
        ActivateV2OrForceAscii();
      }
    }

    public bool isV2
    {
      get
      {
        return ((uint)fields & 4096U) > 0U;
      }
    }

    public byte[] getBinary(int version)
    {
      if (version >= 2)
      {
        ActivateV2OrForceAscii();
      }

      var isV2 = this.isV2;
      var memoryStream = new MemoryStream();
      var binaryWriter = new BinaryWriter((Stream) memoryStream, Encoding.ASCII);
      binaryWriter.Write(fields);
      if (isV2)
      {
        binaryWriter.Write(fields2);
        if (hasText)
        {
          binaryWriter.Write((byte)text.Length);
        }
      }
      if (hasN)
      {
        binaryWriter.Write((ushort) (n & (int) ushort.MaxValue));
      }

      if (isV2)
      {
        if (hasM)
        {
          binaryWriter.Write(m);
        }

        if (hasG)
        {
          binaryWriter.Write(g);
        }
      }
      else
      {
        if (hasM)
        {
          binaryWriter.Write((byte)m);
        }

        if (hasG)
        {
          binaryWriter.Write((byte)g);
        }
      }
      if (hasX)
      {
        binaryWriter.Write(x);
      }

      if (hasY)
      {
        binaryWriter.Write(y);
      }

      if (hasZ)
      {
        binaryWriter.Write(z);
      }

      if (hasE)
      {
        binaryWriter.Write(e);
      }

      if (hasF)
      {
        binaryWriter.Write(f);
      }

      if (hasT)
      {
        binaryWriter.Write(t);
      }

      if (hasS)
      {
        binaryWriter.Write(s);
      }

      if (hasP)
      {
        binaryWriter.Write(p);
      }

      if (hasI)
      {
        binaryWriter.Write(ii);
      }

      if (hasJ)
      {
        binaryWriter.Write(j);
      }

      if (hasR)
      {
        binaryWriter.Write(r);
      }

      if (hasText)
      {
        var num = text.Length;
        if (isV2)
        {
          for (var index = 0; index < num; ++index)
          {
            binaryWriter.Write((byte)text[index]);
          }
        }
        else
        {
          if (num > 16)
          {
            num = 16;
          }

          int index;
          for (index = 0; index < num; ++index)
          {
            binaryWriter.Write((byte)text[index]);
          }

          for (; index < 16; ++index)
          {
            binaryWriter.Write((byte) 0);
          }
        }
      }
      var num1 = 0;
      var num2 = 0;
      binaryWriter.Flush();
      memoryStream.Flush();
      foreach (var num3 in memoryStream.ToArray())
      {
        num1 = (num1 + (int) num3) % (int) byte.MaxValue;
        num2 = (num2 + num1) % (int) byte.MaxValue;
      }
      binaryWriter.Write((byte) num1);
      binaryWriter.Write((byte) num2);
      binaryWriter.Close();
      memoryStream.Flush();
      return memoryStream.ToArray();
    }

    public string getAscii(bool inclLine, bool inclChecksum)
    {
      if (hostCommand)
      {
        return orig;
      }

      var stringBuilder = new StringBuilder();
      if (inclLine && hasN)
      {
        stringBuilder.Append("N");
        stringBuilder.Append(n);
        stringBuilder.Append(" ");
      }
      if (forceAscii)
      {
        var length = orig.IndexOf(';');
        if (length < 0)
        {
          stringBuilder.Append(orig);
        }
        else
        {
          stringBuilder.Append(orig.Substring(0, length).Trim());
        }
      }
      else
      {
        if (hasM)
        {
          stringBuilder.Append("M");
          stringBuilder.Append(m);
        }
        if (hasG)
        {
          stringBuilder.Append("G");
          stringBuilder.Append(g);
        }
        if (hasT)
        {
          if (hasM)
          {
            stringBuilder.Append(" ");
          }

          stringBuilder.Append("T");
          stringBuilder.Append(t);
        }
        if (hasX)
        {
          stringBuilder.Append(" X");
          stringBuilder.Append(x.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasY)
        {
          stringBuilder.Append(" Y");
          stringBuilder.Append(y.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasZ)
        {
          stringBuilder.Append(" Z");
          stringBuilder.Append(z.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasE)
        {
          stringBuilder.Append(" E");
          stringBuilder.Append(e.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasF)
        {
          stringBuilder.Append(" F");
          stringBuilder.Append(f.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasI)
        {
          stringBuilder.Append(" I");
          stringBuilder.Append(ii.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasJ)
        {
          stringBuilder.Append(" J");
          stringBuilder.Append(j.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasR)
        {
          stringBuilder.Append(" R");
          stringBuilder.Append(r.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (hasS)
        {
          stringBuilder.Append(" S");
          stringBuilder.Append(s);
        }
        if (hasP)
        {
          stringBuilder.Append(" P");
          stringBuilder.Append(p);
        }
        if (hasText)
        {
          stringBuilder.Append(" ");
          stringBuilder.Append(text);
        }
      }
      if (inclChecksum)
      {
        var num1 = 0;
        foreach (var ch in stringBuilder.ToString())
        {
          num1 ^= (int) ch & (int) byte.MaxValue;
        }

        var num2 = num1 ^ 32;
        stringBuilder.Append(" *");
        stringBuilder.Append(num2);
      }
      if (orig != null && orig.IndexOf(";") == 0)
      {
        return orig;
      }

      return stringBuilder.ToString();
    }

    private void ActivateV2OrForceAscii()
    {
      fields |= (ushort) 4096;
    }

    private void AddCode(char c, string val)
    {
      double.TryParse(val, NumberStyles.Float, (IFormatProvider)GCode.format, out var result);
      switch (c)
      {
        case 'A':
          E = (float) result;
          forceAscii = true;
          break;
        case 'E':
          E = (float) result;
          break;
        case 'F':
          F = (float) result;
          break;
        case 'G':
          if (result > (double) byte.MaxValue)
          {
            ActivateV2OrForceAscii();
          }

          G = (ushort) result;
          break;
        case 'I':
          I = (float) result;
          break;
        case 'J':
          J = (float) result;
          break;
        case 'M':
          if (result > (double) byte.MaxValue)
          {
            ActivateV2OrForceAscii();
          }

          M = (ushort) result;
          break;
        case 'N':
          N = (int) result;
          break;
        case 'P':
          P = (int) result;
          break;
        case 'R':
          R = (float) result;
          break;
        case 'S':
          S = (int) result;
          break;
        case 'T':
          if (result > (double) byte.MaxValue)
          {
            forceAscii = true;
          }

          T = (byte) result;
          break;
        case 'X':
          X = (float) result;
          break;
        case 'Y':
          Y = (float) result;
          break;
        case 'Z':
          Z = (float) result;
          break;
        default:
          forceAscii = true;
          break;
      }
    }

    public void Parse(string line)
    {
      hostCommand = false;
      orig = line.Trim();
      if (orig.StartsWith("@") || orig.StartsWith(";@"))
      {
        hostCommand = true;
      }
      else
      {
        fields = (ushort) 128;
        fields2 = (ushort) 0;
        var length1 = orig.Length;
        var num = 0;
        var c = ';';
        var startIndex1 = 0;
        for (var length2 = 0; length2 < length1; ++length2)
        {
          var ch = orig[length2];
          if (num == 0 && ch >= 'a' && ch <= 'z')
          {
            ch -= ' ';
            orig = orig.Substring(0, length2) + ch.ToString() + orig.Substring(length2 + 1);
          }
          if (num == 0 && ch >= 'A' && ch <= 'Z')
          {
            c = ch;
            num = 1;
            startIndex1 = length2 + 1;
          }
          else
          {
            if (num == 1 && (ch == ' ' || ch == '\t' || ch == ';'))
            {
              AddCode(c, orig.Substring(startIndex1, length2 - startIndex1));
              num = 0;
              if (hasM && (m == (ushort) 23 || m == (ushort) 28 || (m == (ushort) 29 || m == (ushort) 30) || (m == (ushort) 32 || m == (ushort) 117)))
              {
                var startIndex2 = length2;
                while (startIndex2 < orig.Length && char.IsWhiteSpace(orig[startIndex2]))
                {
                  ++startIndex2;
                }

                var index = startIndex2;
                while (index < orig.Length && (m == (ushort) 117 || !char.IsWhiteSpace(orig[index])))
                {
                  ++index;
                }

                Text = orig.Substring(startIndex2, index - startIndex2);
                if (Text.Length > 16)
                {
                  ActivateV2OrForceAscii();
                  break;
                }
                break;
              }
            }
            if (ch == ';')
            {
              break;
            }
          }
        }
        if (num == 1)
        {
          AddCode(c, orig.Substring(startIndex1, orig.Length - startIndex1));
        }

        comment = fields == (ushort) 128;
      }
    }

    public override string ToString()
    {
      return getAscii(true, true);
    }

    public void Assert()
    {
      var parametersEnumArray = (GCode.ParametersEnum[]) null;
      var AtLeastOnePeramiter = new bool?();
      if (isOnlyComment)
      {
        return;
      }

      if (hasG && hasM)
      {
        throw new Exception("Gcode cannot be set to both M and G Types");
      }

      string command;
      if (hasG)
      {
        command = string.Format("G{0}", (object)G);
        switch (G)
        {
          case 0:
          case 1:
            parametersEnumArray = new GCode.ParametersEnum[5]
            {
              GCode.ParametersEnum.X,
              GCode.ParametersEnum.Y,
              GCode.ParametersEnum.Z,
              GCode.ParametersEnum.E,
              GCode.ParametersEnum.F
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 4:
            parametersEnumArray = new GCode.ParametersEnum[2]
            {
              GCode.ParametersEnum.S,
              GCode.ParametersEnum.P
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 20:
          case 28:
          case 33:
          case 90:
          case 91:
            break;
          case 30:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.Z
            };
            break;
          case 32:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.Z
            };
            break;
          case 92:
            parametersEnumArray = new GCode.ParametersEnum[4]
            {
              GCode.ParametersEnum.X,
              GCode.ParametersEnum.Y,
              GCode.ParametersEnum.Z,
              GCode.ParametersEnum.E
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          default:
            throw new Exception(string.Format("Unknown G Code:{0}", (object)G));
        }
      }
      else
      {
        if (!hasM)
        {
          throw new Exception("Gcode must be a M or G code");
        }

        command = string.Format("M{0}", (object)M);
        switch (M)
        {
          case 0:
          case 1:
          case 17:
          case 18:
          case 20:
          case 21:
          case 22:
          case 23:
          case 24:
          case 25:
          case 27:
          case 28:
          case 29:
          case 30:
          case 31:
          case 32:
          case 105:
          case 107:
          case 114:
          case 116:
          case 117:
          case 303:
          case 304:
          case 405:
          case 572:
          case 573:
          case 576:
          case 578:
          case 581:
          case 583:
          case 683:
          case 684:
          case 997:
          case 998:
          case 999:
          case 1011:
          case 1012:
          case 1013:
          case 1014:
          case 5680:
          case 16007:
          case 17013:
          case 18010:
          case 19007:
          case 20904:
          case 21914:
          case 23975:
            break;
          case 26:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.S
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 104:
          case 109:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.S
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 106:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.S
            };
            break;
          case 110:
            parametersEnumArray = new GCode.ParametersEnum[1];
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 115:
            if (hasS && S == 628)
            {
              parametersEnumArray = new GCode.ParametersEnum[1]
              {
                GCode.ParametersEnum.S
              };
              break;
            }
            break;
          case 140:
          case 190:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.S
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 333:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.X
            };
            break;
          case 404:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.R
            };
            break;
          case 420:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.T
            };
            break;
          case 570:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.P
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 571:
            parametersEnumArray = new GCode.ParametersEnum[2]
            {
              GCode.ParametersEnum.X,
              GCode.ParametersEnum.Y
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 575:
            parametersEnumArray = new GCode.ParametersEnum[6]
            {
              GCode.ParametersEnum.S,
              GCode.ParametersEnum.P,
              GCode.ParametersEnum.T,
              GCode.ParametersEnum.E,
              GCode.ParametersEnum.I,
              GCode.ParametersEnum.R
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 577:
            parametersEnumArray = new GCode.ParametersEnum[6]
            {
              GCode.ParametersEnum.X,
              GCode.ParametersEnum.Y,
              GCode.ParametersEnum.Z,
              GCode.ParametersEnum.E,
              GCode.ParametersEnum.F,
              GCode.ParametersEnum.I
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 580:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.X
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 618:
            parametersEnumArray = new GCode.ParametersEnum[3]
            {
              GCode.ParametersEnum.S,
              GCode.ParametersEnum.P,
              GCode.ParametersEnum.T
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 619:
            parametersEnumArray = new GCode.ParametersEnum[2]
            {
              GCode.ParametersEnum.S,
              GCode.ParametersEnum.T
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          case 682:
            parametersEnumArray = new GCode.ParametersEnum[5]
            {
              GCode.ParametersEnum.X,
              GCode.ParametersEnum.Y,
              GCode.ParametersEnum.Z,
              GCode.ParametersEnum.E,
              GCode.ParametersEnum.R
            };
            AtLeastOnePeramiter = new bool?(true);
            break;
          case 5321:
            parametersEnumArray = new GCode.ParametersEnum[1]
            {
              GCode.ParametersEnum.X
            };
            AtLeastOnePeramiter = new bool?(false);
            break;
          default:
            throw new Exception(string.Format("Unknown M Code:{0}", (object)M));
        }
      }
      if (string.IsNullOrEmpty(command))
      {
        throw new Exception("There is a coverage error in Gcode.Assert");
      }

      if (parametersEnumArray == null)
      {
        parametersEnumArray = new GCode.ParametersEnum[0];
      }

      AssertHelperCanOnlyBe(command, parametersEnumArray);
      if (!AtLeastOnePeramiter.HasValue)
      {
        return;
      }

      AssertHelperMustHave(command, parametersEnumArray, AtLeastOnePeramiter);
    }

    private void AssertHelperMustHave(string command, GCode.ParametersEnum[] optionalFeilds, bool? AtLeastOnePeramiter)
    {
      GenerateFeildFlags(optionalFeilds, out var feildFlag1, out var feildFlag2);
      if (!(!AtLeastOnePeramiter.Value ? ((int) feildFlag1 & (int)fields) != (int)fields && ((int) feildFlag2 & (int)fields2) != (int)fields2 : ((int) feildFlag1 & (int)fields) <= 0 && ((int) feildFlag2 & (int)fields2) <= 0))
      {
        return;
      }

      var list = new List<string>();
      FeildListBuilderF1((ushort) ((uint) feildFlag1 & (uint)fields), ref list);
      FeildListBuilderF2((ushort) ((uint) feildFlag2 & (uint)fields2), ref list);
      var str = (list.Count != 1 ? string.Format("{0} must have at least one of the following argument", (object) command) : string.Format("{0} must have the following argument", (object) command)) + listBuilder(list, "or") + " in numeric form";
    }

    private void AssertHelperCanOnlyBe(string command, GCode.ParametersEnum[] arrayParramiters)
    {
      GenerateFeildFlags(arrayParramiters, out var feildFlag1, out var feildFlag2);
      var num = (ushort) ((uint)fields & 4294930297U);
      if (((int) feildFlag1 & (int) num) != (int) num || ((int) feildFlag2 & (int)fields2) != (int)fields2)
      {
        var list = new List<string>();
        FeildListBuilderF1((ushort) ((uint) ~feildFlag1 & (uint) num), ref list);
        FeildListBuilderF2((ushort) ((uint) ~feildFlag2 & (uint)fields2), ref list);
        throw new Exception(string.Format("{0} does not take argument ", (object) command) + listBuilder(list, "and"));
      }
    }

    private void GenerateFeildFlags(GCode.ParametersEnum[] arrayParramiters, out ushort feildFlag1, out ushort feildFlag2)
    {
      feildFlag1 = (ushort) 1;
      feildFlag2 = (ushort) 0;
      foreach (GCode.ParametersEnum arrayParramiter in arrayParramiters)
      {
        switch (arrayParramiter)
        {
          case GCode.ParametersEnum.N:
          case GCode.ParametersEnum.X:
          case GCode.ParametersEnum.Y:
          case GCode.ParametersEnum.Z:
          case GCode.ParametersEnum.E:
          case GCode.ParametersEnum.F:
          case GCode.ParametersEnum.T:
          case GCode.ParametersEnum.S:
          case GCode.ParametersEnum.P:
            feildFlag1 |= (ushort) (GCode.Feilds1TypeEnum) Enum.Parse(typeof (GCode.Feilds1TypeEnum), arrayParramiter.ToString());
            goto case GCode.ParametersEnum.M;
          case GCode.ParametersEnum.M:
          case GCode.ParametersEnum.G:
            feildFlag1 |= (ushort) 1;
            continue;
          case GCode.ParametersEnum.I:
          case GCode.ParametersEnum.J:
          case GCode.ParametersEnum.R:
            feildFlag2 |= (ushort) (GCode.Feilds2TypeEnum) Enum.Parse(typeof (GCode.Feilds2TypeEnum), arrayParramiter.ToString());
            goto case GCode.ParametersEnum.M;
          default:
            throw new Exception("Hole found in assert coverage");
        }
      }
    }

    private void FeildListBuilderF1(ushort feildsFlag, ref List<string> list)
    {
      foreach (var obj in Enum.GetValues(typeof (GCode.Feilds1TypeEnum)))
      {
        var num = (ushort) (GCode.Feilds1TypeEnum) obj;
        if (((int) num & (int) feildsFlag) == (int) num)
        {
          list.Add(obj.ToString());
        }
      }
    }

    private void FeildListBuilderF2(ushort feildsFlag, ref List<string> list)
    {
      foreach (var obj in Enum.GetValues(typeof (GCode.Feilds2TypeEnum)))
      {
        var num = (ushort) (GCode.Feilds2TypeEnum) obj;
        if (((int) num & (int) feildsFlag) == (int) num)
        {
          list.Add(obj.ToString());
        }
      }
    }

    private string listBuilder(List<string> list, string conjunction)
    {
      var str1 = "";
      if (list.Count == 1)
      {
        str1 += list[0];
      }
      else if (list.Count > 0)
      {
        var str2 = str1 + "s ";
        for (var index = 0; index < list.Count - 1; ++index)
        {
          str2 += list[index];
          if (index != list.Count - 2)
          {
            str2 += ", ";
          }
        }
        str1 = str2 + " " + conjunction + " " + list[list.Count - 1].ToString();
      }
      return str1;
    }

    private enum Feilds1TypeEnum
    {
      N = 1,
      M = 2,
      G = 4,
      X = 8,
      Y = 16, // 0x00000010
      Z = 32, // 0x00000020
      E = 64, // 0x00000040
      F = 256, // 0x00000100
      T = 512, // 0x00000200
      S = 1024, // 0x00000400
      P = 2048, // 0x00000800
    }

    private enum Feilds2TypeEnum
    {
      I = 1,
      J = 2,
      R = 4,
    }

    private enum ParametersEnum
    {
      N,
      M,
      G,
      X,
      Y,
      Z,
      E,
      F,
      T,
      S,
      P,
      I,
      J,
      R,
    }
  }
}
