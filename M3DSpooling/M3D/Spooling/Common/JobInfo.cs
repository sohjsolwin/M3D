// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.JobInfo
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class JobInfo
  {
    [XmlIgnore]
    private string jobname;
    [XmlIgnore]
    private string user;
    [XmlIgnore]
    private JobStatus status;
    [XmlIgnore]
    private float percent_complete;
    [XmlIgnore]
    private float time;
    [XmlIgnore]
    private string preview_image_file_name;
    [XmlIgnore]
    private JobParams param;

    public JobInfo(JobInfo other)
    {
      this.jobname = other.jobname;
      this.user = other.user;
      this.status = other.status;
      this.percent_complete = other.percent_complete;
      this.time = other.time;
      this.preview_image_file_name = other.preview_image_file_name;
      this.param = other.param;
    }

    public JobInfo()
    {
      this.jobname = "";
      this.user = "";
      this.status = JobStatus.Cancelled;
      this.percent_complete = 0.0f;
      this.time = 0.0f;
      this.preview_image_file_name = "";
      this.param = new JobParams("", "", "", FilamentSpool.TypeEnum.NoFilament, 0.0f, 0.0f);
    }

    public JobInfo(string jobname, string user, JobStatus status, string preview_image_file_name, float percent_complete, float time_remaining, JobParams jobParams)
    {
      this.jobname = jobname;
      this.user = user;
      this.status = status;
      this.preview_image_file_name = !string.IsNullOrEmpty(preview_image_file_name) ? preview_image_file_name : "null";
      this.percent_complete = percent_complete;
      this.time = time_remaining;
      this.param = jobParams;
    }

    [XmlAttribute("JobName")]
    public string JobName
    {
      get
      {
        return this.jobname;
      }
      set
      {
        this.jobname = value;
      }
    }

    [XmlAttribute("User")]
    public string User
    {
      get
      {
        return this.user;
      }
      set
      {
        this.user = value;
      }
    }

    [XmlAttribute("PercentComplete")]
    public float PercentComplete
    {
      get
      {
        return this.percent_complete;
      }
      set
      {
        this.percent_complete = value;
      }
    }

    [XmlAttribute("TimeRemaining")]
    public float TimeRemaining
    {
      get
      {
        return this.time;
      }
      set
      {
        this.time = value;
      }
    }

    [XmlAttribute("PreviewImageFileName")]
    public string PreviewImageFileName
    {
      get
      {
        if (string.IsNullOrEmpty(this.preview_image_file_name))
          return "null";
        return this.preview_image_file_name;
      }
      set
      {
        this.preview_image_file_name = value;
      }
    }

    [XmlAttribute("Status")]
    public JobStatus Status
    {
      get
      {
        return this.status;
      }
      set
      {
        this.status = value;
      }
    }

    [XmlElement("Params")]
    public JobParams Params
    {
      get
      {
        return this.param;
      }
      set
      {
        this.param = value;
      }
    }
  }
}
