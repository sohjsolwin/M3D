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
      _delayMode = new ThreadSafeVariable<ConnectionMeter.Mode>(mode);
      OKsBeforeRSSampleData = new SampleSet(20);
      TimeBeforeRSSampleData = new SampleSet(20);
    }

    public void StartMetering()
    {
      ResetData();
      isMetering = true;
      oktimer.Restart();
    }

    public void StopMetering()
    {
      StopTimers();
      isMetering = false;
    }

    public bool CanSendNextCommand
    {
      get
      {
        if (current_delay <= 0L)
        {
          return true;
        }

        if (!delaytimer.IsRunning)
        {
          delaytimer.Restart();
        }

        if (delaytimer.ElapsedMilliseconds < current_delay)
        {
          return false;
        }

        if (DelayMode == ConnectionMeter.Mode.DelayWhenBufferIsFull)
        {
          current_delay = 0L;
        }

        return true;
      }
    }

    public ConnectionMeter.Mode DelayMode
    {
      get
      {
        return _delayMode.Value;
      }
      set
      {
        _delayMode.Value = value;
      }
    }

    public bool IsMetering
    {
      get
      {
        return isMetering;
      }
    }

    public double AvgCMDsBeforeRS
    {
      get
      {
        return OKsBeforeRSSampleData.SampleMean;
      }
    }

    public double AvgRSDelay
    {
      get
      {
        return TimeBeforeRSSampleData.SampleMean;
      }
    }

    public double RSDelayStandardDeviation
    {
      get
      {
        return TimeBeforeRSSampleData.SampleStdDev;
      }
    }

    public void OKReceived()
    {
      oktimer.Restart();
      ++numOKsBeforeRS;
    }

    public void RSReceived()
    {
      oktimer.Stop();
      var num = oktimer.ElapsedMilliseconds / 1000.0;
      OKsBeforeRSSampleData.Add(numOKsBeforeRS);
      TimeBeforeRSSampleData.Add(num);
      RestartTimers();
      numOKsBeforeRS = 0;
      UpdatePhase();
    }

    public void WaitReceived()
    {
      ResetData();
    }

    public void CommandSent()
    {
      RestartTimers();
    }

    private void ResetData()
    {
      StopTimers();
      oktimer.Reset();
      delaytimer.Reset();
      OKsBeforeRSSampleData.Clear();
      TimeBeforeRSSampleData.Clear();
      currentPhase = ConnectionMeter.Phase.FindingSteadyState;
    }

    private void UpdatePhase()
    {
      if (currentPhase == ConnectionMeter.Phase.FindingSteadyState && RSDelayStandardDeviation < 0.2)
      {
        currentPhase = ConnectionMeter.Phase.Metering;
      }
      else
      {
        if (currentPhase != ConnectionMeter.Phase.Metering)
        {
          return;
        }

        if (RSDelayStandardDeviation >= 0.2)
        {
          currentPhase = ConnectionMeter.Phase.FindingSteadyState;
          current_delay = 0L;
        }
        else
        {
          if (current_delay != 0L)
          {
            return;
          }

          if (DelayMode == ConnectionMeter.Mode.DelayEveryCommand)
          {
            current_delay = (long) (AvgRSDelay * 500.0);
          }
          else if (DelayMode == ConnectionMeter.Mode.DelayWhenBufferIsFull)
          {
            current_delay = (long) (AvgRSDelay * 500.0);
          }

          delaytimer.Restart();
        }
      }
    }

    private void StopTimers()
    {
      oktimer.Stop();
      delaytimer.Stop();
    }

    private void RestartTimers()
    {
      oktimer.Restart();
    }

    private void CalculateRollingAverage(CircularBuffer<double> sampleData, CircularBuffer<double> rollingAvgs)
    {
      var num1 = 0;
      var num2 = 0.0;
      foreach (var num3 in sampleData)
      {
        num2 += num3;
        ++num1;
      }
      var num4 = num2 / num1;
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
