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
      this.m_sum = 0.0;
      this.m_sumSqrd = 0.0;
      this.m_lastData = double.NaN;
    }

    public override double Add(double newData)
    {
      this.m_lastData = newData;
      double num = base.Add(newData);
      this.m_sum -= num;
      this.m_sum += newData;
      this.m_sumSqrd = this.m_sum * this.m_sum;
      return num;
    }

    public override void Clear()
    {
      base.Clear();
      this.m_sum = 0.0;
      this.m_sumSqrd = 0.0;
      this.m_lastData = double.NaN;
    }

    public double LastQueuedData
    {
      get
      {
        return this.m_lastData;
      }
    }

    public bool Available
    {
      get
      {
        return this.Count > 1;
      }
    }

    public bool isWindowFull()
    {
      return this.Count == this.Capacity;
    }

    public float SampleMean
    {
      get
      {
        return (float) this.m_sum / (float) this.Count;
      }
    }

    public float SampleVariance
    {
      get
      {
        return (float) (this.m_sumSqrd - this.m_sum * this.m_sum / (double) this.Count) / (float) (this.Count - 1);
      }
    }

    public float SampleStdDev
    {
      get
      {
        return (float) Math.Sqrt((double) this.SampleVariance);
      }
    }
  }
}
