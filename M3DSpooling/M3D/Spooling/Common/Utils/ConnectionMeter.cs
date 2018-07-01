// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.ConnectionMeter
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.Spooling.Common.Utils
{
  public class ConnectionMeter
  {
    private Stopwatch oktimer = new Stopwatch();
    private Stopwatch delaytimer = new Stopwatch();
    private const double StandardDeviationLimit = 0.2;
    private const int MaxSamples = 20;
    private const double FirmwareBufferFillRate = 0.5;
    private const long FirmwareBufferFillRateConst = 500;
    private const double ConstantDelayAverageSpeedPercent = 0.5;
    private const long ConstantDelayAverageSpeedPercentMS = 500;
    private ThreadSafeVariable<ConnectionMeter.Mode> _delayMode;
    private ConnectionMeter.Phase currentPhase;
    private bool isMetering;
    private long current_delay;
    private int numOKsBeforeRS;
    private SampleSet OKsBeforeRSSampleData;
    private SampleSet TimeBeforeRSSampleData;
    private uint firmwareCodeBufferSize;

    public ConnectionMeter(ConnectionMeter.Mode mode, uint firmwareCodeBufferSize)
    {
      this.firmwareCodeBufferSize = firmwareCodeBufferSize;
      this._delayMode = new ThreadSafeVariable<ConnectionMeter.Mode>(mode);
      this.OKsBeforeRSSampleData = new SampleSet(20);
      this.TimeBeforeRSSampleData = new SampleSet(20);
    }

    public void StartMetering()
    {
      this.ResetData();
      this.isMetering = true;
      this.oktimer.Restart();
    }

    public void StopMetering()
    {
      this.StopTimers();
      this.isMetering = false;
    }

    public bool CanSendNextCommand
    {
      get
      {
        if (this.current_delay <= 0L)
          return true;
        if (!this.delaytimer.IsRunning)
          this.delaytimer.Restart();
        if (this.delaytimer.ElapsedMilliseconds < this.current_delay)
          return false;
        if (this.DelayMode == ConnectionMeter.Mode.DelayWhenBufferIsFull)
          this.current_delay = 0L;
        return true;
      }
    }

    public ConnectionMeter.Mode DelayMode
    {
      get
      {
        return this._delayMode.Value;
      }
      set
      {
        this._delayMode.Value = value;
      }
    }

    public bool IsMetering
    {
      get
      {
        return this.isMetering;
      }
    }

    public double AvgCMDsBeforeRS
    {
      get
      {
        return (double) this.OKsBeforeRSSampleData.SampleMean;
      }
    }

    public double AvgRSDelay
    {
      get
      {
        return (double) this.TimeBeforeRSSampleData.SampleMean;
      }
    }

    public double RSDelayStandardDeviation
    {
      get
      {
        return (double) this.TimeBeforeRSSampleData.SampleStdDev;
      }
    }

    public void OKReceived()
    {
      this.oktimer.Restart();
      ++this.numOKsBeforeRS;
    }

    public void RSReceived()
    {
      this.oktimer.Stop();
      double num = (double) this.oktimer.ElapsedMilliseconds / 1000.0;
      this.OKsBeforeRSSampleData.Add((double) this.numOKsBeforeRS);
      this.TimeBeforeRSSampleData.Add(num);
      this.RestartTimers();
      this.numOKsBeforeRS = 0;
      this.UpdatePhase();
    }

    public void WaitReceived()
    {
      this.ResetData();
    }

    public void CommandSent()
    {
      this.RestartTimers();
    }

    private void ResetData()
    {
      this.StopTimers();
      this.oktimer.Reset();
      this.delaytimer.Reset();
      this.OKsBeforeRSSampleData.Clear();
      this.TimeBeforeRSSampleData.Clear();
      this.currentPhase = ConnectionMeter.Phase.FindingSteadyState;
    }

    private void UpdatePhase()
    {
      if (this.currentPhase == ConnectionMeter.Phase.FindingSteadyState && this.RSDelayStandardDeviation < 0.2)
      {
        this.currentPhase = ConnectionMeter.Phase.Metering;
      }
      else
      {
        if (this.currentPhase != ConnectionMeter.Phase.Metering)
          return;
        if (this.RSDelayStandardDeviation >= 0.2)
        {
          this.currentPhase = ConnectionMeter.Phase.FindingSteadyState;
          this.current_delay = 0L;
        }
        else
        {
          if (this.current_delay != 0L)
            return;
          if (this.DelayMode == ConnectionMeter.Mode.DelayEveryCommand)
            this.current_delay = (long) (this.AvgRSDelay * 500.0);
          else if (this.DelayMode == ConnectionMeter.Mode.DelayWhenBufferIsFull)
            this.current_delay = (long) (this.AvgRSDelay * 500.0);
          this.delaytimer.Restart();
        }
      }
    }

    private void StopTimers()
    {
      this.oktimer.Stop();
      this.delaytimer.Stop();
    }

    private void RestartTimers()
    {
      this.oktimer.Restart();
    }

    private void CalculateRollingAverage(CircularBuffer<double> sampleData, CircularBuffer<double> rollingAvgs)
    {
      int num1 = 0;
      double num2 = 0.0;
      foreach (double num3 in (IEnumerable<double>) sampleData)
      {
        num2 += num3;
        ++num1;
      }
      double num4 = num2 / (double) num1;
      rollingAvgs.Add(num4);
    }

    public enum Mode
    {
      DelayEveryCommand,
      DelayWhenBufferIsFull,
    }

    private enum Phase
    {
      FindingSteadyState,
      Metering,
    }
  }
}
