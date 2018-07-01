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
      this.InitSlicerCommunication();
    }

    private void InitSlicerCommunication()
    {
      this.slicerThread = (Thread) null;
      this.slicer_status_block = new object();
      this.slicer_timer = new Stopwatch();
      this.m_slicer_information.status.is_done = true;
      this.m_slicer_information.status.is_slicing = false;
      this.m_slicer_information.status.estimatedpercentcomplete = 0.0f;
    }

    public override SlicerResult StartSlicingUsingCurrentSettings(string modelfilename, string outputgcode, PrintSettings printsettings)
    {
      CultureInfo cultureInfo = new CultureInfo("en-US");
      if (this.GetSlicerStatus().is_slicing)
        return SlicerResult.Result_AlreadyStarted;
      string str1 = " -m " + printsettings.transformation.m[0, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[0, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[0, 2].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[1, 2].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 0].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 1].ToString((IFormatProvider) cultureInfo) + "," + printsettings.transformation.m[2, 2].ToString((IFormatProvider) cultureInfo);
      int num1 = (int) ((double) printsettings.transformation.m[3, 0] * 1000.0);
      int num2 = (int) ((double) printsettings.transformation.m[3, 1] * 1000.0);
      string str2 = " -s posx=" + num1.ToString((IFormatProvider) cultureInfo) + " -s posy=" + num2.ToString((IFormatProvider) cultureInfo);
      using (StreamWriter writer = new StreamWriter(Paths.SlicerSettingsPath(this.SlicerSettings.ConfigurationFileName)))
        this.SlicerSettings.SerializeToSlicer(writer);
      FileUtils.GrantAccess(Paths.SlicerSettingsPath(this.SlicerSettings.ConfigurationFileName));
      this.slicer_process_arg = string.Format("-p -v {0} {1} {2} -s autoCenter=1 {3} {4}", (object) string.Format("-o \"{0}\"", (object) Path.Combine(Paths.WorkingFolder, outputgcode)), (object) string.Format("-c \"{0}\"", (object) Path.Combine(Paths.WorkingFolder, this.SlicerSettings.ConfigurationFileName)), (object) string.Format("\"{0}\"", (object) modelfilename), (object) str2, (object) str1);
      this.StartSlicerThread();
      return SlicerResult.Result_OK;
    }

    public override void Cancel()
    {
      if (this.slicerThread == null)
        return;
      this.slicerThread.Abort();
      this.m_slicer_information.status.is_done = true;
      this.m_slicer_information.status.is_slicing = false;
      this.m_slicer_information.status.estimatedpercentcomplete = 0.0f;
    }

    private SlicerStatus GetSlicerStatus()
    {
      lock (this.slicer_status_block)
        return this.m_slicer_information.status;
    }

    public override int EstimatedPrintTimeSeconds
    {
      get
      {
        return this.GetSlicerStatus().results.print_time_s;
      }
    }

    public override float EstimatedFilament
    {
      get
      {
        return (float) this.GetSlicerStatus().results.filament;
      }
    }

    public override float EstimatedPercentComplete
    {
      get
      {
        return this.GetSlicerStatus().estimatedpercentcomplete;
      }
    }

    public void SlicerThreadBody()
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      bool flag1 = false;
      Process process = new Process();
      process.StartInfo.FileName = this.FullExecutablePath;
      process.StartInfo.Arguments = this.slicer_process_arg;
      process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.WorkingDirectory = this.WorkingFolder;
      process.OutputDataReceived += new DataReceivedEventHandler(this.CuraOutputHandler);
      process.ErrorDataReceived += new DataReceivedEventHandler(this.CuraOutputHandler);
      try
      {
        bool flag2 = process.Start();
        lock (this.slicer_status_block)
        {
          this.m_slicer_information.status.is_done = !flag2;
          this.m_slicer_information.status.is_slicing = flag2;
          this.m_slicer_information.status.results.filament = -1;
          this.m_slicer_information.status.results.print_time_s = -1;
        }
        if (!flag2)
          return;
        this.slicer_timer.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        this.SendMessageToQueue("Slicer Started");
        while (!process.HasExited)
          Thread.Sleep(1);
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
      this.slicer_timer.Stop();
      lock (this.slicer_status_block)
        this.m_slicer_information.status.estimatedpercentcomplete = 1f;
      if (!flag1)
        this.SendMessageToQueue("Slicer Finished");
      else
        this.SendMessageToQueue("Slicer Canceled");
      lock (this.slicer_status_block)
      {
        this.m_slicer_information.status.is_done = true;
        this.m_slicer_information.status.is_slicing = false;
      }
    }

    private void CuraOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (string.IsNullOrEmpty(outLine.Data))
        return;
      this.ProcessCuraOutput(outLine.Data);
      this.SendMessageToQueue(outLine.Data);
    }

    private void ProcessCuraOutput(string outputline)
    {
      this.slicer_timer.Stop();
      this.m_slicer_information.elasped_time = this.slicer_timer.ElapsedMilliseconds;
      this.slicer_timer.Start();
      this.m_slicer_information.total_elasped_time += this.m_slicer_information.elasped_time;
      int num1 = -1;
      SlicerConnectionCura.SlicerProgressMode modeFromLine = this.GetModeFromLine(outputline);
      switch (modeFromLine)
      {
        case SlicerConnectionCura.SlicerProgressMode.Progress_inset:
        case SlicerConnectionCura.SlicerProgressMode.Progress_skin:
        case SlicerConnectionCura.SlicerProgressMode.Progress_export:
          int num2 = outputline.IndexOf(':', 9);
          int num3 = outputline.IndexOf(':', num2 + 1);
          num1 = int.Parse(outputline.Substring(num2 + 1, num3 - (num2 + 1)));
          this.m_slicer_information.cur_progress_mode = modeFromLine;
          break;
        case SlicerConnectionCura.SlicerProgressMode.Progress_LayerCount:
          this.m_slicer_information.num_layers = int.Parse(outputline.Substring(13));
          break;
        case SlicerConnectionCura.SlicerProgressMode.Progress_PrintTime:
          lock (this.slicer_status_block)
          {
            if (!int.TryParse(outputline.Substring(12), out this.m_slicer_information.status.results.print_time_s))
            {
              this.m_slicer_information.status.results.print_time_s = 0;
              break;
            }
            break;
          }
        case SlicerConnectionCura.SlicerProgressMode.Progress_Filament:
          lock (this.slicer_status_block)
          {
            if (!int.TryParse(outputline.Substring(10), out this.m_slicer_information.status.results.filament))
            {
              this.m_slicer_information.status.results.filament = 0;
              break;
            }
            break;
          }
      }
      if (this.m_slicer_information.num_layers <= 0 || num1 < 0)
        return;
      float num4 = 1f;
      float num5 = (float) (1 + this.m_slicer_information.num_layers * 3);
      if (this.m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_inset)
        num4 = (float) (1 + num1);
      else if (this.m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_skin)
        num4 = (float) (1 + this.m_slicer_information.num_layers + num1);
      else if (this.m_slicer_information.cur_progress_mode == SlicerConnectionCura.SlicerProgressMode.Progress_export)
        num4 = (float) (1 + this.m_slicer_information.num_layers * 2 + num1);
      float num6 = (float) (3.0 + (1.0 - (double) num4 / (double) num5));
      float num7 = (float) ((double) this.m_slicer_information.total_elasped_time / (double) num4 * (double) this.m_slicer_information.num_layers * (double) num6 / 1000.0);
      float num8 = (float) this.m_slicer_information.total_elasped_time / 1000f;
      lock (this.slicer_status_block)
        this.m_slicer_information.status.estimatedpercentcomplete = num8 / num7;
    }

    private SlicerConnectionCura.SlicerProgressMode GetModeFromLine(string outputline)
    {
      if (outputline.Length > 12 && outputline.Substring(0, 12) == "Layer count:")
        return SlicerConnectionCura.SlicerProgressMode.Progress_LayerCount;
      if (outputline.Length > 15 && outputline.Substring(0, 15) == "Progress:inset:")
        return SlicerConnectionCura.SlicerProgressMode.Progress_inset;
      if (outputline.Length > 14 && outputline.Substring(0, 14) == "Progress:skin:")
        return SlicerConnectionCura.SlicerProgressMode.Progress_skin;
      if (outputline.Length > 16 && outputline.Substring(0, 16) == "Progress:export:")
        return SlicerConnectionCura.SlicerProgressMode.Progress_export;
      if (outputline.Length > 11 && outputline.Substring(0, 11) == "Print time:")
        return SlicerConnectionCura.SlicerProgressMode.Progress_PrintTime;
      return outputline.Length > 9 && outputline.Substring(0, 9) == "Filament:" ? SlicerConnectionCura.SlicerProgressMode.Progress_Filament : SlicerConnectionCura.SlicerProgressMode.Progress_other;
    }

    private void StartSlicerThread()
    {
      this.m_slicer_information.cur_progress_mode = SlicerConnectionCura.SlicerProgressMode.Progress_other;
      this.m_slicer_information.elasped_time = 0L;
      this.m_slicer_information.num_layers = 0;
      this.m_slicer_information.total_elasped_time = 0L;
      lock (this.slicer_status_block)
      {
        this.m_slicer_information.status.results.filament = -1;
        this.m_slicer_information.status.results.print_time_s = -1;
        this.m_slicer_information.status.estimatedpercentcomplete = 0.0f;
      }
      this.slicerThread = new Thread(new ThreadStart(this.SlicerThreadBody));
      this.slicerThread.Name = "Slicer";
      this.slicerThread.Start();
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
