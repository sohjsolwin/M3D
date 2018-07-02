using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;

namespace M3D.Spooling.Common.Utils
{
  internal sealed class MovementUtility
  {
    public static void VGetEffectiveMovementGCode(Trilean bExtruderIsHomed, bool bExtruderZValid, Trilean bExtruderInRelativeMode, Vector3D op3dDestination, Vector3D op3dInitial, ref GCode ogcGCodeEffectiveMove)
    {
      if (bExtruderInRelativeMode == Trilean.True)
      {
        if (bExtruderIsHomed == Trilean.True)
        {
          if (ogcGCodeEffectiveMove.HasX)
          {
            ogcGCodeEffectiveMove.X = op3dDestination.x - op3dInitial.x;
          }

          if (ogcGCodeEffectiveMove.HasY)
          {
            ogcGCodeEffectiveMove.Y = op3dDestination.y - op3dInitial.y;
          }
        }
        if (!bExtruderZValid || !ogcGCodeEffectiveMove.HasZ)
        {
          return;
        }

        ogcGCodeEffectiveMove.Z = op3dDestination.z - op3dInitial.z;
      }
      else
      {
        if (Trilean.False != bExtruderInRelativeMode)
        {
          return;
        }

        if (bExtruderIsHomed == Trilean.True)
        {
          if (ogcGCodeEffectiveMove.HasX)
          {
            ogcGCodeEffectiveMove.X = op3dDestination.x;
          }

          if (ogcGCodeEffectiveMove.HasY)
          {
            ogcGCodeEffectiveMove.Y = op3dDestination.y;
          }
        }
        if (!bExtruderZValid || !ogcGCodeEffectiveMove.HasZ)
        {
          return;
        }

        ogcGCodeEffectiveMove.Z = op3dDestination.z;
      }
    }

    public static Vector3D Op3dCalculateDestination(Trilean bExtruderIsHomed, bool bExtruderZValid, Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      var op3dInitial1 = new Vector3D(op3dInitial);
      if (bExtruderIsHomed == Trilean.True)
      {
        op3dInitial1 = MovementUtility.Op3dCalculateDestinationXYOnly(bExtruderInRelativeMode, ogcGCodeRequest, op3dInitial1);
      }

      if (bExtruderZValid)
      {
        op3dInitial1 = MovementUtility.Op3dCalculateDestinationZOnly(bExtruderInRelativeMode, ogcGCodeRequest, op3dInitial1);
      }

      return op3dInitial1;
    }

    public static Vector3D Op3dCalculateDestinationXYOnly(Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      var vector3D = new Vector3D(op3dInitial);
      if (bExtruderInRelativeMode == Trilean.True)
      {
        if (ogcGCodeRequest.HasX)
        {
          vector3D.x += ogcGCodeRequest.X;
        }

        if (ogcGCodeRequest.HasY)
        {
          vector3D.y += ogcGCodeRequest.Y;
        }
      }
      else
      {
        if (ogcGCodeRequest.HasX)
        {
          vector3D.x = ogcGCodeRequest.X;
        }

        if (ogcGCodeRequest.HasY)
        {
          vector3D.y = ogcGCodeRequest.Y;
        }
      }
      return vector3D;
    }

    public static Vector3D Op3dCalculateDestinationZOnly(Trilean bExtruderInRelativeMode, GCode ogcGCodeRequest, Vector3D op3dInitial)
    {
      var vector3D = new Vector3D(op3dInitial);
      if (ogcGCodeRequest.HasZ)
      {
        if (bExtruderInRelativeMode == Trilean.True)
        {
          vector3D.z += ogcGCodeRequest.Z;
        }
        else
        {
          vector3D.z = ogcGCodeRequest.Z;
        }
      }
      return vector3D;
    }

    public static Vector3D Op3dCalculateDestinationWithClipping(Trilean bExtruderIsHomed, bool bExtruderZValid, ref bool bDestinationHasBeenClipped, Vector3D op3dDestination, Vector3D op3dInitial, PrinterProfile printerProfile)
    {
      var intercept = new Vector3D();
      bDestinationHasBeenClipped = false;
      if (bExtruderIsHomed == Trilean.True)
      {
        bDestinationHasBeenClipped = printerProfile.PrinterSizeConstants.WarningRegion.LineIntercepts(out intercept, op3dInitial, op3dDestination);
      }
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
