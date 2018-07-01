// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Colors
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using System;

namespace M3D.GUI.Views.Printer_View
{
  internal static class Colors
  {
    public static readonly Color4 black = new Color4((byte) 52, (byte) 52, (byte) 52, byte.MaxValue);
    public static readonly Color4 silver = new Color4((byte) 185, (byte) 185, (byte) 185, byte.MaxValue);
    public static readonly Color4 orange = new Color4(byte.MaxValue, (byte) 70, (byte) 0, byte.MaxValue);
    public static readonly Color4 green = new Color4((byte) 66, (byte) 187, (byte) 49, byte.MaxValue);
    public static readonly Color4 gray = new Color4((byte) 102, (byte) 102, (byte) 102, byte.MaxValue);
    public static readonly Color4 darkBlue = new Color4((byte) 1, (byte) 0, (byte) 138, byte.MaxValue);
    public static readonly Color4 grape = new Color4((byte) 93, (byte) 24, (byte) 106, byte.MaxValue);
    public static readonly Color4 lightBlue = new Color4((byte) 98, (byte) 181, (byte) 233, byte.MaxValue);
    public static readonly Color4 lightGreen = new Color4((byte) 144, (byte) 238, (byte) 144, byte.MaxValue);
    public static readonly Color4 natural = new Color4((byte) 0, (byte) 159, (byte) 236, byte.MaxValue);
    public static readonly Color4 neonOrange = new Color4(byte.MaxValue, (byte) 104, (byte) 1, byte.MaxValue);
    public static readonly Color4 red = new Color4((byte) 196, (byte) 4, (byte) 1, byte.MaxValue);
    public static readonly Color4 white = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static readonly Color4 brown = new Color4((byte) 101, (byte) 66, (byte) 34, byte.MaxValue);
    public static readonly Color4 yellow = new Color4(byte.MaxValue, (byte) 215, (byte) 0, byte.MaxValue);
    public static readonly Color4 clear = new Color4((byte) 105, (byte) 100, (byte) 110, (byte) 75);

    public static Color4 GetColorFromSN(string serialNumber)
    {
      switch (serialNumber.Substring(0, 2))
      {
        case "0R":
        case "OR":
          return Colors.orange;
        case "BL":
          return Colors.lightBlue;
        case "CL":
          return Colors.clear;
        case "GR":
          return Colors.green;
        case "PL":
        case "PR":
        case "PU":
          return Colors.grape;
        case "SL":
          return Colors.silver;
        case "WH":
          return Colors.white;
        default:
          return Colors.black;
      }
    }

    public static Color4 GetColor(string color)
    {
      switch (color)
      {
        case "Black":
          return Colors.black;
        case "Brown":
          return Colors.brown;
        case "Clear":
          return Colors.clear;
        case "Dark Blue":
        case "DarkBlue":
          return Colors.darkBlue;
        case "Grape":
          return Colors.grape;
        case "Gray":
          return Colors.gray;
        case "Green":
          return Colors.green;
        case "Light Blue":
        case "LightBlue":
          return Colors.lightBlue;
        case "Light Green":
        case "LightGreen":
          return Colors.lightGreen;
        case "Natural":
          return Colors.natural;
        case "Neon Orange":
        case "NeonOrange":
          return Colors.neonOrange;
        case "Orange":
          return Colors.orange;
        case "Purple":
          return Colors.grape;
        case "Red":
          return Colors.red;
        case "Silver":
          return Colors.silver;
        case "White":
          return Colors.white;
        case "Yellow":
          return Colors.yellow;
        default:
          throw new ArgumentException("Not a valid color");
      }
    }
  }
}
