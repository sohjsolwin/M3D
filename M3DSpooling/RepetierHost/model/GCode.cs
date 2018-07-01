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
      this.Parse(s);
    }

    public GCode()
    {
    }

    public bool hasCode
    {
      get
      {
        return this.fields != (ushort) 128;
      }
    }

    public bool hasText
    {
      get
      {
        return ((uint) this.fields & 32768U) > 0U;
      }
    }

    public bool isOnlyComment
    {
      get
      {
        if (this.fields == (ushort) 128)
          return this.comment;
        return false;
      }
    }

    public string Text
    {
      get
      {
        return this.text;
      }
      set
      {
        this.text = PrinterCompatibleString.RemoveIllegalCharacters(value);
        if (this.text.Length > 16)
          this.ActivateV2OrForceAscii();
        this.fields |= (ushort) 32768;
      }
    }

    public bool hasN
    {
      get
      {
        return ((uint) this.fields & 1U) > 0U;
      }
    }

    public int N
    {
      get
      {
        return this.n;
      }
      set
      {
        this.n = value;
        this.fields |= (ushort) 1;
      }
    }

    public bool hasM
    {
      get
      {
        return ((uint) this.fields & 2U) > 0U;
      }
    }

    public ushort M
    {
      get
      {
        return this.m;
      }
      set
      {
        this.m = value;
        this.fields |= (ushort) 2;
      }
    }

    public bool hasG
    {
      get
      {
        return ((uint) this.fields & 4U) > 0U;
      }
    }

    public ushort G
    {
      get
      {
        return this.g;
      }
      set
      {
        this.g = value;
        this.fields |= (ushort) 4;
      }
    }

    public bool hasT
    {
      get
      {
        return ((uint) this.fields & 512U) > 0U;
      }
    }

    public byte T
    {
      get
      {
        return this.t;
      }
      set
      {
        this.t = value;
        this.fields |= (ushort) 512;
      }
    }

    public bool hasS
    {
      get
      {
        return ((uint) this.fields & 1024U) > 0U;
      }
    }

    public int S
    {
      get
      {
        return this.s;
      }
      set
      {
        this.s = value;
        this.fields |= (ushort) 1024;
      }
    }

    public bool hasP
    {
      get
      {
        return ((uint) this.fields & 2048U) > 0U;
      }
    }

    public int P
    {
      get
      {
        return this.p;
      }
      set
      {
        this.p = value;
        this.fields |= (ushort) 2048;
      }
    }

    public bool hasX
    {
      get
      {
        return ((uint) this.fields & 8U) > 0U;
      }
    }

    public float X
    {
      get
      {
        return this.x;
      }
      set
      {
        this.x = value;
        this.fields |= (ushort) 8;
      }
    }

    public bool hasY
    {
      get
      {
        return ((uint) this.fields & 16U) > 0U;
      }
    }

    public float Y
    {
      get
      {
        return this.y;
      }
      set
      {
        this.y = value;
        this.fields |= (ushort) 16;
      }
    }

    public bool hasZ
    {
      get
      {
        return ((uint) this.fields & 32U) > 0U;
      }
    }

    public float Z
    {
      get
      {
        return this.z;
      }
      set
      {
        this.z = value;
        this.fields |= (ushort) 32;
      }
    }

    public bool hasE
    {
      get
      {
        return ((uint) this.fields & 64U) > 0U;
      }
    }

    public float E
    {
      get
      {
        return this.e;
      }
      set
      {
        this.e = value;
        this.fields |= (ushort) 64;
      }
    }

    public bool hasF
    {
      get
      {
        return ((uint) this.fields & 256U) > 0U;
      }
    }

    public float F
    {
      get
      {
        return this.f;
      }
      set
      {
        this.f = value;
        this.fields |= (ushort) 256;
      }
    }

    public bool hasI
    {
      get
      {
        return ((uint) this.fields2 & 1U) > 0U;
      }
    }

    public float I
    {
      get
      {
        return this.ii;
      }
      set
      {
        this.ii = value;
        this.fields2 |= (ushort) 1;
        this.ActivateV2OrForceAscii();
      }
    }

    public bool hasJ
    {
      get
      {
        return ((uint) this.fields2 & 2U) > 0U;
      }
    }

    public float J
    {
      get
      {
        return this.j;
      }
      set
      {
        this.j = value;
        this.fields2 |= (ushort) 2;
        this.ActivateV2OrForceAscii();
      }
    }

    public bool hasR
    {
      get
      {
        return ((uint) this.fields2 & 4U) > 0U;
      }
    }

    public float R
    {
      get
      {
        return this.r;
      }
      set
      {
        this.r = value;
        this.fields2 |= (ushort) 4;
        this.ActivateV2OrForceAscii();
      }
    }

    public bool isV2
    {
      get
      {
        return ((uint) this.fields & 4096U) > 0U;
      }
    }

    public byte[] getBinary(int version)
    {
      if (version >= 2)
        this.ActivateV2OrForceAscii();
      bool isV2 = this.isV2;
      MemoryStream memoryStream = new MemoryStream();
      BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream, Encoding.ASCII);
      binaryWriter.Write(this.fields);
      if (isV2)
      {
        binaryWriter.Write(this.fields2);
        if (this.hasText)
          binaryWriter.Write((byte) this.text.Length);
      }
      if (this.hasN)
        binaryWriter.Write((ushort) (this.n & (int) ushort.MaxValue));
      if (isV2)
      {
        if (this.hasM)
          binaryWriter.Write(this.m);
        if (this.hasG)
          binaryWriter.Write(this.g);
      }
      else
      {
        if (this.hasM)
          binaryWriter.Write((byte) this.m);
        if (this.hasG)
          binaryWriter.Write((byte) this.g);
      }
      if (this.hasX)
        binaryWriter.Write(this.x);
      if (this.hasY)
        binaryWriter.Write(this.y);
      if (this.hasZ)
        binaryWriter.Write(this.z);
      if (this.hasE)
        binaryWriter.Write(this.e);
      if (this.hasF)
        binaryWriter.Write(this.f);
      if (this.hasT)
        binaryWriter.Write(this.t);
      if (this.hasS)
        binaryWriter.Write(this.s);
      if (this.hasP)
        binaryWriter.Write(this.p);
      if (this.hasI)
        binaryWriter.Write(this.ii);
      if (this.hasJ)
        binaryWriter.Write(this.j);
      if (this.hasR)
        binaryWriter.Write(this.r);
      if (this.hasText)
      {
        int num = this.text.Length;
        if (isV2)
        {
          for (int index = 0; index < num; ++index)
            binaryWriter.Write((byte) this.text[index]);
        }
        else
        {
          if (num > 16)
            num = 16;
          int index;
          for (index = 0; index < num; ++index)
            binaryWriter.Write((byte) this.text[index]);
          for (; index < 16; ++index)
            binaryWriter.Write((byte) 0);
        }
      }
      int num1 = 0;
      int num2 = 0;
      binaryWriter.Flush();
      memoryStream.Flush();
      foreach (byte num3 in memoryStream.ToArray())
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
      if (this.hostCommand)
        return this.orig;
      StringBuilder stringBuilder = new StringBuilder();
      if (inclLine && this.hasN)
      {
        stringBuilder.Append("N");
        stringBuilder.Append(this.n);
        stringBuilder.Append(" ");
      }
      if (this.forceAscii)
      {
        int length = this.orig.IndexOf(';');
        if (length < 0)
          stringBuilder.Append(this.orig);
        else
          stringBuilder.Append(this.orig.Substring(0, length).Trim());
      }
      else
      {
        if (this.hasM)
        {
          stringBuilder.Append("M");
          stringBuilder.Append(this.m);
        }
        if (this.hasG)
        {
          stringBuilder.Append("G");
          stringBuilder.Append(this.g);
        }
        if (this.hasT)
        {
          if (this.hasM)
            stringBuilder.Append(" ");
          stringBuilder.Append("T");
          stringBuilder.Append(this.t);
        }
        if (this.hasX)
        {
          stringBuilder.Append(" X");
          stringBuilder.Append(this.x.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasY)
        {
          stringBuilder.Append(" Y");
          stringBuilder.Append(this.y.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasZ)
        {
          stringBuilder.Append(" Z");
          stringBuilder.Append(this.z.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasE)
        {
          stringBuilder.Append(" E");
          stringBuilder.Append(this.e.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasF)
        {
          stringBuilder.Append(" F");
          stringBuilder.Append(this.f.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasI)
        {
          stringBuilder.Append(" I");
          stringBuilder.Append(this.ii.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasJ)
        {
          stringBuilder.Append(" J");
          stringBuilder.Append(this.j.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasR)
        {
          stringBuilder.Append(" R");
          stringBuilder.Append(this.r.ToString(GCode.floatNoExp, (IFormatProvider) GCode.format));
        }
        if (this.hasS)
        {
          stringBuilder.Append(" S");
          stringBuilder.Append(this.s);
        }
        if (this.hasP)
        {
          stringBuilder.Append(" P");
          stringBuilder.Append(this.p);
        }
        if (this.hasText)
        {
          stringBuilder.Append(" ");
          stringBuilder.Append(this.text);
        }
      }
      if (inclChecksum)
      {
        int num1 = 0;
        foreach (char ch in stringBuilder.ToString())
          num1 ^= (int) ch & (int) byte.MaxValue;
        int num2 = num1 ^ 32;
        stringBuilder.Append(" *");
        stringBuilder.Append(num2);
      }
      if (this.orig != null && this.orig.IndexOf(";") == 0)
        return this.orig;
      return stringBuilder.ToString();
    }

    private void ActivateV2OrForceAscii()
    {
      this.fields |= (ushort) 4096;
    }

    private void AddCode(char c, string val)
    {
      double result;
      double.TryParse(val, NumberStyles.Float, (IFormatProvider) GCode.format, out result);
      switch (c)
      {
        case 'A':
          this.E = (float) result;
          this.forceAscii = true;
          break;
        case 'E':
          this.E = (float) result;
          break;
        case 'F':
          this.F = (float) result;
          break;
        case 'G':
          if (result > (double) byte.MaxValue)
            this.ActivateV2OrForceAscii();
          this.G = (ushort) result;
          break;
        case 'I':
          this.I = (float) result;
          break;
        case 'J':
          this.J = (float) result;
          break;
        case 'M':
          if (result > (double) byte.MaxValue)
            this.ActivateV2OrForceAscii();
          this.M = (ushort) result;
          break;
        case 'N':
          this.N = (int) result;
          break;
        case 'P':
          this.P = (int) result;
          break;
        case 'R':
          this.R = (float) result;
          break;
        case 'S':
          this.S = (int) result;
          break;
        case 'T':
          if (result > (double) byte.MaxValue)
            this.forceAscii = true;
          this.T = (byte) result;
          break;
        case 'X':
          this.X = (float) result;
          break;
        case 'Y':
          this.Y = (float) result;
          break;
        case 'Z':
          this.Z = (float) result;
          break;
        default:
          this.forceAscii = true;
          break;
      }
    }

    public void Parse(string line)
    {
      this.hostCommand = false;
      this.orig = line.Trim();
      if (this.orig.StartsWith("@") || this.orig.StartsWith(";@"))
      {
        this.hostCommand = true;
      }
      else
      {
        this.fields = (ushort) 128;
        this.fields2 = (ushort) 0;
        int length1 = this.orig.Length;
        int num = 0;
        char c = ';';
        int startIndex1 = 0;
        for (int length2 = 0; length2 < length1; ++length2)
        {
          char ch = this.orig[length2];
          if (num == 0 && ch >= 'a' && ch <= 'z')
          {
            ch -= ' ';
            this.orig = this.orig.Substring(0, length2) + ch.ToString() + this.orig.Substring(length2 + 1);
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
              this.AddCode(c, this.orig.Substring(startIndex1, length2 - startIndex1));
              num = 0;
              if (this.hasM && (this.m == (ushort) 23 || this.m == (ushort) 28 || (this.m == (ushort) 29 || this.m == (ushort) 30) || (this.m == (ushort) 32 || this.m == (ushort) 117)))
              {
                int startIndex2 = length2;
                while (startIndex2 < this.orig.Length && char.IsWhiteSpace(this.orig[startIndex2]))
                  ++startIndex2;
                int index = startIndex2;
                while (index < this.orig.Length && (this.m == (ushort) 117 || !char.IsWhiteSpace(this.orig[index])))
                  ++index;
                this.Text = this.orig.Substring(startIndex2, index - startIndex2);
                if (this.Text.Length > 16)
                {
                  this.ActivateV2OrForceAscii();
                  break;
                }
                break;
              }
            }
            if (ch == ';')
              break;
          }
        }
        if (num == 1)
          this.AddCode(c, this.orig.Substring(startIndex1, this.orig.Length - startIndex1));
        this.comment = this.fields == (ushort) 128;
      }
    }

    public override string ToString()
    {
      return this.getAscii(true, true);
    }

    public void Assert()
    {
      GCode.ParametersEnum[] parametersEnumArray = (GCode.ParametersEnum[]) null;
      bool? AtLeastOnePeramiter = new bool?();
      if (this.isOnlyComment)
        return;
      if (this.hasG && this.hasM)
        throw new Exception("Gcode cannot be set to both M and G Types");
      string command;
      if (this.hasG)
      {
        command = string.Format("G{0}", (object) this.G);
        switch (this.G)
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
            throw new Exception(string.Format("Unknown G Code:{0}", (object) this.G));
        }
      }
      else
      {
        if (!this.hasM)
          throw new Exception("Gcode must be a M or G code");
        command = string.Format("M{0}", (object) this.M);
        switch (this.M)
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
            if (this.hasS && this.S == 628)
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
            throw new Exception(string.Format("Unknown M Code:{0}", (object) this.M));
        }
      }
      if (string.IsNullOrEmpty(command))
        throw new Exception("There is a coverage error in Gcode.Assert");
      if (parametersEnumArray == null)
        parametersEnumArray = new GCode.ParametersEnum[0];
      this.AssertHelperCanOnlyBe(command, parametersEnumArray);
      if (!AtLeastOnePeramiter.HasValue)
        return;
      this.AssertHelperMustHave(command, parametersEnumArray, AtLeastOnePeramiter);
    }

    private void AssertHelperMustHave(string command, GCode.ParametersEnum[] optionalFeilds, bool? AtLeastOnePeramiter)
    {
      ushort feildFlag1;
      ushort feildFlag2;
      this.GenerateFeildFlags(optionalFeilds, out feildFlag1, out feildFlag2);
      if (!(!AtLeastOnePeramiter.Value ? ((int) feildFlag1 & (int) this.fields) != (int) this.fields && ((int) feildFlag2 & (int) this.fields2) != (int) this.fields2 : ((int) feildFlag1 & (int) this.fields) <= 0 && ((int) feildFlag2 & (int) this.fields2) <= 0))
        return;
      List<string> list = new List<string>();
      this.FeildListBuilderF1((ushort) ((uint) feildFlag1 & (uint) this.fields), ref list);
      this.FeildListBuilderF2((ushort) ((uint) feildFlag2 & (uint) this.fields2), ref list);
      string str = (list.Count != 1 ? string.Format("{0} must have at least one of the following argument", (object) command) : string.Format("{0} must have the following argument", (object) command)) + this.listBuilder(list, "or") + " in numeric form";
    }

    private void AssertHelperCanOnlyBe(string command, GCode.ParametersEnum[] arrayParramiters)
    {
      ushort feildFlag1;
      ushort feildFlag2;
      this.GenerateFeildFlags(arrayParramiters, out feildFlag1, out feildFlag2);
      ushort num = (ushort) ((uint) this.fields & 4294930297U);
      if (((int) feildFlag1 & (int) num) != (int) num || ((int) feildFlag2 & (int) this.fields2) != (int) this.fields2)
      {
        List<string> list = new List<string>();
        this.FeildListBuilderF1((ushort) ((uint) ~feildFlag1 & (uint) num), ref list);
        this.FeildListBuilderF2((ushort) ((uint) ~feildFlag2 & (uint) this.fields2), ref list);
        throw new Exception(string.Format("{0} does not take argument ", (object) command) + this.listBuilder(list, "and"));
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
      foreach (object obj in Enum.GetValues(typeof (GCode.Feilds1TypeEnum)))
      {
        ushort num = (ushort) (GCode.Feilds1TypeEnum) obj;
        if (((int) num & (int) feildsFlag) == (int) num)
          list.Add(obj.ToString());
      }
    }

    private void FeildListBuilderF2(ushort feildsFlag, ref List<string> list)
    {
      foreach (object obj in Enum.GetValues(typeof (GCode.Feilds2TypeEnum)))
      {
        ushort num = (ushort) (GCode.Feilds2TypeEnum) obj;
        if (((int) num & (int) feildsFlag) == (int) num)
          list.Add(obj.ToString());
      }
    }

    private string listBuilder(List<string> list, string conjunction)
    {
      string str1 = "";
      if (list.Count == 1)
        str1 += list[0];
      else if (list.Count > 0)
      {
        string str2 = str1 + "s ";
        for (int index = 0; index < list.Count - 1; ++index)
        {
          str2 += list[index];
          if (index != list.Count - 2)
            str2 += ", ";
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
