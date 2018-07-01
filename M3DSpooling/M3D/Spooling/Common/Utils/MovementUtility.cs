// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.MovementUtility
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;

namespace M3D.Spooling.Common.Utils
{
  internal sealed class MovementUtility
  {
    public static void vGetEffectiveMovementGCode(Trilean bExtruderIsHomed, bool bExtruderZValid, Trilean bExtruderInRelativeMode, Vector3D op3dDestination, Vector3D op3dInitial, ref GCode ogcGCodeEffectiveMove)
    {
      if (bExtruderInRelativeMode == Trilean.True)
      {
        if (bExtruderIsHomed == Trilean.True)
        {
          if (ogcGCodeEffectiveMove.hasX)
            ogcGCodeEffectiveMove.X = op3dDestination.x - op3dInitial.x;
          if (ogcGCodeEffectiveMove.hasY)
            ogcGCodeEffectiveMove.Y = op3dDestination.y - op3dInitial.y;
        }
        if (!bExtruderZValid || !ogcGCodeEffectiveMove.hasZ)
          return;
        ogcGCodeEffectiveMove.Z = op3dDestination.z - op3dInitial.z;
      }
      else
      {
        if (Trilean.False != bExtruderInRelativeMode)
          return;
        if (bExtruderIsHomed == Trilean.True)
        {
          if (ogcGCodeEffectiveMove.hasX)
            ogcGCodeEffectiveMove.X = op3dDestination.x;
          if (ogcGCodeEffectiveMove.hasY)
            ogcGCodeEffectiveMove.Y = op3dDestination.y;
        }
        if (!bExtruderZValid || !ogcGCodeEffectiveMove.hasZ)
          return;
        ogcGCodeEffectiveMove.Z = op3dDestination.z;
      }
    }

    public static Vector3D op3dCalculateDestination(Trilean bExtruderIsHomed, bool bExtruderZValid, Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      Vector3D op3dInitial1 = new Vector3D(op3dInitial);
      if (bExtruderIsHomed == Trilean.True)
        op3dInitial1 = MovementUtility.op3dCalculateDestinationXYOnly(bExtruderInRelativeMode, ogcGCodeRequest, op3dInitial1);
      if (bExtruderZValid)
        op3dInitial1 = MovementUtility.op3dCalculateDestinationZOnly(bExtruderInRelativeMode, ogcGCodeRequest, op3dInitial1);
      return op3dInitial1;
    }

    public static Vector3D op3dCalculateDestinationXYOnly(Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      Vector3D vector3D = new Vector3D(op3dInitial);
      if (bExtruderInRelativeMode == Trilean.True)
      {
        if (ogcGCodeRequest.hasX)
          vector3D.x += ogcGCodeRequest.X;
        if (ogcGCodeRequest.hasY)
          vector3D.y += ogcGCodeRequest.Y;
      }
      else
      {
        if (ogcGCodeRequest.hasX)
          vector3D.x = ogcGCodeRequest.X;
        if (ogcGCodeRequest.hasY)
          vector3D.y = ogcGCodeRequest.Y;
      }
      return vector3D;
    }

    public static Vector3D op3dCalculateDestinationZOnly(Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      Vector3D vector3D = new Vector3D(op3dInitial);
      if (ogcGCodeRequest.hasZ)
      {
        if (bExtruderInRelativeMode == Trilean.True)
          vector3D.z += ogcGCodeRequest.Z;
        else
          vector3D.z = ogcGCodeRequest.Z;
      }
      return vector3D;
    }

    public static Vector3D op3dCalculateDestinationWithClipping(Trilean bExtruderIsHomed, bool bExtruderZValid, ref bool bDestinationHasBeenClipped, Vector3D op3dDestination, Vector3D op3dInitial, PrinterProfile printerProfile)
    {
      Vector3D intercept = new Vector3D();
      bDestinationHasBeenClipped = false;
      if (bExtruderIsHomed == Trilean.True)
        bDestinationHasBeenClipped = printerProfile.PrinterSizeConstants.WarningRegion.LineIntercepts(out intercept, op3dInitial, op3dDestination);
      else if (bExtruderZValid)
      {
        intercept.x = 0.0f;
        intercept.y = 0.0f;
        bDestinationHasBeenClipped = printerProfile.PrinterSizeConstants.UnhomedSafeZRange.Intercepts(out intercept.z, op3dInitial.z, op3dDestination.z);
      }
      return intercept;
    }
  }
}
