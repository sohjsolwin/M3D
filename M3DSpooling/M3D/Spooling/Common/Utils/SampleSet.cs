// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.SampleSet
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Common.Utils
{
  public class SampleSet : CircularBuffer<double>
  {
    private double m_sum;
    private double m_sumSqrd;
    private double m_lastData;

    public SampleSet(int capacity)
      : base(capacity)
    {
      m_sum = 0.0;
      m_sumSqrd = 0.0;
      m_lastData = double.NaN;
    }

    public override double Add(double newData)
    {
      m_lastData = newData;
      var num = base.Add(newData);
      m_sum -= num;
      m_sum += newData;
      m_sumSqrd = m_sum * m_sum;
      return num;
    }

    public override void Clear()
    {
      base.Clear();
      m_sum = 0.0;
      m_sumSqrd = 0.0;
      m_lastData = double.NaN;
    }

    public double LastQueuedData
    {
      get
      {
        return m_lastData;
      }
    }

    public bool Available
    {
      get
      {
        return Count > 1;
      }
    }

    public bool isWindowFull()
    {
      return Count == Capacity;
    }

    public float SampleMean
    {
      get
      {
        return (float)m_sum / (float)Count;
      }
    }

    public float SampleVariance
    {
      get
      {
        return (float) (m_sumSqrd - m_sum * m_sum / (double)Count) / (float) (Count - 1);
      }
    }

    public float SampleStdDev
    {
      get
      {
        return (float) Math.Sqrt((double)SampleVariance);
      }
    }
  }
}
