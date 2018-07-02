// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.Cura15_04.SlicerConnectionCura
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI;
using M3D.Slicer.General;
using M3D.Spooling.Common.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace M3D.Slicer.Cura15_04
{
  public class SlicerConnectionCura : SlicerConnectionBase
  {
    private Thread slicerThread;
    private string slicer_process_arg;
    private object slicer_status_block;
    private SlicerConnectionCura.SlicerInformation m_slicer_information;
    private Stopwatch slicer_timer;

    public SlicerConnectionCura(string WorkingFolder, string ExeResourceFolder)
      : base(WorkingFolder, ExeResourceFolder, (SmartSlicerSettingsBase) new SmartSlicerSettingsCura15_04())
    {
      InitSlicerCommunication();
    }

    private void InitSlicerCommunication()
    {
      slicerThread = (Thread) null;
      slicer_status_block = new object();
      slicer_timer = new Stopwatch();
      m_slicer_information.status.is_done = true;
      m_slicer_information.status.is_slicing = false;
      m_slicer_information.status.estimatedpercentcomplete = 0.0f;
    }

    public override SlicerResult StartSlicingUsingCurrentSettings(string modelfilename, string outputgcode, PrintSettings printsettings)
    {
      var cultureInfo = new CultureInfo("en-US");
      if (GetSlicerStatus().is_slicing)
      {
        return SlicerResult.Result_AlreadyStarted;
      }

      var str1 = " -m " + printsettings.transformation.m[0, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[0, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[0, 2].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 2].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 2].ToString((IFormatProvider) cultureInfo);
      var num1 = (int) ((double) printsettings.transformation.m[3, 0] * 1000.0);
      var num2 = (int) ((double) printsettings.transformation.m[3, 1] * 1000.0);
      var str2 = " -s posx=" + num1.ToString((IFormatProvider) cultureInfo) + " -s posy=" + num2.ToString((IFormatProvider) cultureInfo);
      using (var writer = new StreamWriter(Paths.SlicerSettingsPath(SlicerSettings.ConfigurationFileName)))
      {
        SlicerSettings.SerializeToSlicer(writer);
      }

      FileUtils.GrantAccess(Paths.SlicerSettingsPath(SlicerSettings.ConfigurationFileName));
      slicer_process_arg = string.Format("-p -v {0} {1} {2} -s autoCenter=1 {3} {4}", (object) string.Format("-o \"{0}\"", (object) Path.Combine(Paths.WorkingFolder, outputgcode)), (object) string.Format("-c \"{0}\"", (object) Path.Combine(Paths.WorkingFolder, SlicerSettings.ConfigurationFileName)), (object) string.Format("\"{0}\"", (object) modelfilename), (object) str2, (object) str1);
      StartSlicerThread();
      return SlicerResult.Result_OK;
    }

    public override void Cancel()
    {
      if (slicerThread == null)
      {
        return;
      }

      slicerThread.Abort();
      m_slicer_information.status.is_done = true;
      m_slicer_information.status.is_slicing = false;
      m_slicer_information.status.estimatedpercentcomplete = 0.0f;
    }

    private SlicerStatus GetSlicerStatus()
    {
      lock (slicer_status_block)
      {
        return m_slicer_information.status;
      }
    }

    public override int EstimatedPrintTimeSeconds
    {
      get
      {
        return GetSlicerStatus().results.print_time_s;
      }
    }

    public override float EstimatedFilament
    {
      get
      {
        return (float)GetSlicerStatus().results.filament;
      }
    }

    public override float EstimatedPercentComplete
    {
      get
      {
        return GetSlicerStatus().estimatedpercentcomplete;
      }
    }

    public void SlicerThreadBody()
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      var flag1 = false;
      var process = new Process();
      process.StartInfo.FileName = FullExecutablePath;
      process.StartInfo.Arguments = slicer_process_arg;
      process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.WorkingDirectory = WorkingFolder;
      process.OutputDataReceived += new DataReceivedEventHandler(CuraOutputHandler);
      process.ErrorDataReceived += new DataReceivedEventHandler(CuraOutputHandler);
      try
      {
        var flag2 = process.Start();
        lock (slicer_status_block)
        {
          m_slicer_information.status.is_done = !flag2;
          m_slicer_information.status.is_slicing = flag2;
          m_slicer_information.status.results.filament = -1;
          m_slicer_information.status.results.print_time_s = -1;
        }
        if (!flag2)
        {
          return;
        }

        slicer_timer.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        SendMessageToQueue("Slicer Started");
        while (!process.HasExited)
        {
          Thread.Sleep(1);
        }

        process.Close();
      }
      catch (ThreadAbortException ex1)
      {
        try
        {
          process.Kill();
        }
        catch (Exception ex2)
        {
        }
        flag1 = true;
      }
      slicer_timer.Stop();
      lock (slicer_status_block)
      {
        m_slicer_information.status.estimatedpercentcomplete = 1f;
      }

      if (!flag1)
      {
        SendMessageToQueue("Slicer Finished");
      }
      else
      {
        SendMessageToQueue("Slicer Canceled");
      }

      lock (slicer_status_block)
      {
        m_slicer_information.status.is_done = true;
        m_slicer_information.status.is_slicing = false;
      }
    }

    private void CuraOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (string.IsNullOrEmpty(outLine.Data))
      {
        return;
      }

      ProcessCuraOutput(outLine.Data);
      SendMessageToQueue(outLine.Data);
    }

    private void ProcessCuraOutput(string outputline)
    {
      slicer_timer.Stop();
      m_slicer_information.elasped_time = slicer_timer.ElapsedMilliseconds;
      slicer_timer.Start();
      m_slicer_information.total_elasped_time += m_slicer_information.elasped_time;
      var num1 = -1;
      SlicerConnectionCura.SlicerProgressMode modeFromLine = GetModeFromLine(outputline);
      switch (modeFromLine)
      {
        case SlicerConnectionCura.SlicerProgressMode.Progress_inset:
        case SlicerConnectionCura.SlicerProgressMode.Progress_skin:
        case SlicerConnectionCura.SlicerProgressMode.Progress_export:
          var num2 = outputline.IndexOf(':', 9);
          var num3 = outputline.IndexOf(':', num2 + 1);
          num1 = int.Parse(outputline.Substring(num2 + 1, num3 - (num2 + 1)));
          m_slicer_information.cur_progress_mode = modeFromLine;
          break;
        case SlicerConnectionCura.SlicerProgressMode.Progress_LayerCount:
          m_slicer_information.num_layers = int.Parse(outputline.Substring(13));
          break;
        case SlicerConnectionCura.SlicerProgressMode.Progress_PrintTime:
          lock (slicer_status_block)
          {
            if (!int.TryParse(outputline.Substring(12), out m_slicer_information.status.results.print_time_s))
            {
              m_slicer_information.status.results.print_time_s = 0;
              break;
            }
            break;
          }
        case SlicerConnectionCura.SlicerProgressMode.Progress_Filament:
          lock (slicer_status_block)
          {
            if (!int.TryParse(outputline.Substring(10), out m_slicer_information.status.results.filament))
            {
              m_slicer_information.status.results.filament = 0;
              break;
            }
            break;
          }
      }
      if (m_slicer_information.num_layers <= 0 || num1 < 0)
      {
        return;
      }

      var num4 = 1f;
      var num5 = (float) (1 + m_slicer_information.num_layers * 3);
      if (m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_inset)
      {
        num4 = (float) (1 + num1);
      }
      else if (m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_skin)
      {
        num4 = (float) (1 + m_slicer_information.num_layers + num1);
      }
      else if (m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_export)
      {
        num4 = (float) (1 + m_slicer_information.num_layers * 2 + num1);
      }

      var num6 = (float) (3.0 + (1.0 - (double) num4 / (double) num5));
      var num7 = (float) ((double)m_slicer_information.total_elasped_time / (double) num4 * (double)m_slicer_information.num_layers * (double) num6 / 1000.0);
      var num8 = (float)m_slicer_information.total_elasped_time / 1000f;
      lock (slicer_status_block)
      {
        m_slicer_information.status.estimatedpercentcomplete = num8 / num7;
      }
    }

    private SlicerConnectionCura.SlicerProgressMode GetModeFromLine(string outputline)
    {
      if (outputline.Length > 12 && outputline.Substring(0, 12) == "Layer count:")
      {
        return SlicerConnectionCura.SlicerProgressMode.Progress_LayerCount;
      }

      if (outputline.Length > 15 && outputline.Substring(0, 15) == "Progress:inset:")
      {
        return SlicerConnectionCura.SlicerProgressMode.Progress_inset;
      }

      if (outputline.Length > 14 && outputline.Substring(0, 14) == "Progress:skin:")
      {
        return SlicerConnectionCura.SlicerProgressMode.Progress_skin;
      }

      if (outputline.Length > 16 && outputline.Substring(0, 16) == "Progress:export:")
      {
        return SlicerConnectionCura.SlicerProgressMode.Progress_export;
      }

      if (outputline.Length > 11 && outputline.Substring(0, 11) == "Print time:")
      {
        return SlicerConnectionCura.SlicerProgressMode.Progress_PrintTime;
      }

      return outputline.Length > 9 && outputline.Substring(0, 9) == "Filament:" ? SlicerConnectionCura.SlicerProgressMode.Progress_Filament : SlicerConnectionCura.SlicerProgressMode.Progress_other;
    }

    private void StartSlicerThread()
    {
      m_slicer_information.cur_progress_mode = SlicerConnectionCura.SlicerProgressMode.Progress_other;
      m_slicer_information.elasped_time = 0L;
      m_slicer_information.num_layers = 0;
      m_slicer_information.total_elasped_time = 0L;
      lock (slicer_status_block)
      {
        m_slicer_information.status.results.filament = -1;
        m_slicer_information.status.results.print_time_s = -1;
        m_slicer_information.status.estimatedpercentcomplete = 0.0f;
      }
      slicerThread = new Thread(new ThreadStart(SlicerThreadBody))
      {
        Name = "Slicer"
      };
      slicerThread.Start();
    }

    public override string SlicerPath
    {
      get
      {
        return Path.Combine("CuraEngine", "CuraEngine.exe");
      }
    }

    private enum SlicerProgressMode
    {
      Progress_inset,
      Progress_skin,
      Progress_export,
      Progress_LayerCount,
      Progress_PrintTime,
      Progress_Filament,
      Progress_other,
    }

    private struct SlicerInformation
    {
      public SlicerStatus status;
      public int num_layers;
      public long elasped_time;
      public long total_elasped_time;
      public SlicerConnectionCura.SlicerProgressMode cur_progress_mode;
    }
  }
}
