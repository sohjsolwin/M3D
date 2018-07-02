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
      jobname = other.jobname;
      user = other.user;
      status = other.status;
      percent_complete = other.percent_complete;
      time = other.time;
      preview_image_file_name = other.preview_image_file_name;
      param = other.param;
    }

    public JobInfo()
    {
      jobname = "";
      user = "";
      status = JobStatus.Cancelled;
      percent_complete = 0.0f;
      time = 0.0f;
      preview_image_file_name = "";
      param = new JobParams("", "", "", FilamentSpool.TypeEnum.NoFilament, 0.0f, 0.0f);
    }

    public JobInfo(string jobname, string user, JobStatus status, string preview_image_file_name, float percent_complete, float time_remaining, JobParams jobParams)
    {
      this.jobname = jobname;
      this.user = user;
      this.status = status;
      this.preview_image_file_name = !string.IsNullOrEmpty(preview_image_file_name) ? preview_image_file_name : "null";
      this.percent_complete = percent_complete;
      time = time_remaining;
      param = jobParams;
    }

    [XmlAttribute("JobName")]
    public string JobName
    {
      get
      {
        return jobname;
      }
      set
      {
        jobname = value;
      }
    }

    [XmlAttribute("User")]
    public string User
    {
      get
      {
        return user;
      }
      set
      {
        user = value;
      }
    }

    [XmlAttribute("PercentComplete")]
    public float PercentComplete
    {
      get
      {
        return percent_complete;
      }
      set
      {
        percent_complete = value;
      }
    }

    [XmlAttribute("TimeRemaining")]
    public float TimeRemaining
    {
      get
      {
        return time;
      }
      set
      {
        time = value;
      }
    }

    [XmlAttribute("PreviewImageFileName")]
    public string PreviewImageFileName
    {
      get
      {
        if (string.IsNullOrEmpty(preview_image_file_name))
        {
          return "null";
        }

        return preview_image_file_name;
      }
      set
      {
        preview_image_file_name = value;
      }
    }

    [XmlAttribute("Status")]
    public JobStatus Status
    {
      get
      {
        return status;
      }
      set
      {
        status = value;
      }
    }

    [XmlElement("Params")]
    public JobParams Params
    {
      get
      {
        return param;
      }
      set
      {
        param = value;
      }
    }
  }
}
