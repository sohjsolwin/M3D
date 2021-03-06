﻿namespace M3D
{
  public class ImageResourceMapping
  {
    public static ImageResourceMapping.PixelCoordinate PrinterColorPosition(string sn)
    {
      sn = sn.Substring(0, 2);
      if (!string.IsNullOrEmpty(sn))
      {
        sn = sn.ToUpperInvariant();
      }

      var pixelCoordinate = new ImageResourceMapping.PixelCoordinate();
      switch (sn)
      {
        case "0R":
        case "OR":
          pixelCoordinate.u0 = 512;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 608;
          pixelCoordinate.v1 = 95;
          break;
        case "BL":
          pixelCoordinate.u0 = 704;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 800;
          pixelCoordinate.v1 = 95;
          break;
        case "CL":
          pixelCoordinate.u0 = 320;
          pixelCoordinate.v0 = 96;
          pixelCoordinate.u1 = 415;
          pixelCoordinate.v1 = 189;
          break;
        case "GR":
          pixelCoordinate.u0 = 609;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 703;
          pixelCoordinate.v1 = 95;
          break;
        case "PL":
        case "PR":
        case "PU":
          pixelCoordinate.u0 = 416;
          pixelCoordinate.v0 = 96;
          pixelCoordinate.u1 = 512;
          pixelCoordinate.v1 = 189;
          break;
        case "SL":
          pixelCoordinate.u0 = 416;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 512;
          pixelCoordinate.v1 = 95;
          break;
        case "WH":
          pixelCoordinate.u0 = 801;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 894;
          pixelCoordinate.v1 = 95;
          break;
        default:
          pixelCoordinate.u0 = 321;
          pixelCoordinate.v0 = 0;
          pixelCoordinate.u1 = 415;
          pixelCoordinate.v1 = 95;
          break;
      }
      return pixelCoordinate;
    }

    public struct PixelCoordinate
    {
      public int u0;
      public int v0;
      public int u1;
      public int v1;
    }
  }
}
