using System.Collections.Generic;

namespace M3D.Spooling.Core
{
  internal static class FanConstValues
  {
    public static Dictionary<FanConstValues.FanType, FanConstValues.FanValues> FanConstants = new Dictionary<FanConstValues.FanType, FanConstValues.FanValues>() { { FanConstValues.FanType.HengLiXin, new FanConstValues.FanValues() { Offset = 200, Scale = 0.2165354f } }, { FanConstValues.FanType.Listener, new FanConstValues.FanValues() { Offset = 145, Scale = 0.3333333f } }, { FanConstValues.FanType.Shenzhew, new FanConstValues.FanValues() { Offset = 82, Scale = 0.3843137f } }, { FanConstValues.FanType.Xinyujie, new FanConstValues.FanValues() { Offset = 200, Scale = 0.2165354f } } };

    public enum FanType
    {
      HengLiXin = 1,
      Listener = 2,
      Shenzhew = 3,
      Xinyujie = 4,
      None = 255, // 0x000000FF
    }

    public class FanValues
    {
      public int Offset;
      public float Scale;
    }
  }
}
