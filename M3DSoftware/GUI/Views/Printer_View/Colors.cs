using OpenTK.Graphics;
using System;

namespace M3D.GUI.Views.Printer_View
{
  internal static class Colors
  {
    public static readonly Color4 black = new Color4(52, 52, 52, byte.MaxValue);
    public static readonly Color4 silver = new Color4(185, 185, 185, byte.MaxValue);
    public static readonly Color4 orange = new Color4(byte.MaxValue, 70, 0, byte.MaxValue);
    public static readonly Color4 green = new Color4(66, 187, 49, byte.MaxValue);
    public static readonly Color4 gray = new Color4(102, 102, 102, byte.MaxValue);
    public static readonly Color4 darkBlue = new Color4(1, 0, 138, byte.MaxValue);
    public static readonly Color4 grape = new Color4(93, 24, 106, byte.MaxValue);
    public static readonly Color4 lightBlue = new Color4(98, 181, 233, byte.MaxValue);
    public static readonly Color4 lightGreen = new Color4(144, 238, 144, byte.MaxValue);
    public static readonly Color4 natural = new Color4(0, 159, 236, byte.MaxValue);
    public static readonly Color4 neonOrange = new Color4(byte.MaxValue, 104, 1, byte.MaxValue);
    public static readonly Color4 red = new Color4(196, 4, 1, byte.MaxValue);
    public static readonly Color4 white = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static readonly Color4 brown = new Color4(101, 66, 34, byte.MaxValue);
    public static readonly Color4 yellow = new Color4(byte.MaxValue, 215, 0, byte.MaxValue);
    public static readonly Color4 clear = new Color4(105, 100, 110, 75);

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
