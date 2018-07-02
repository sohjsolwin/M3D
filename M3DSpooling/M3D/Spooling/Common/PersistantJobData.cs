namespace M3D.Spooling.Common
{
  public class PersistantJobData
  {
    public JobParams Params;
    public ulong CurrentLineNumber;

    public PersistantJobData()
    {
      Params = new JobParams();
      CurrentLineNumber = 0UL;
    }

    public PersistantJobData(ulong CurrentLineNumber, JobParams Params)
    {
      this.Params = Params;
      this.CurrentLineNumber = CurrentLineNumber;
    }

    public PersistantJobData(PersistantJobData other)
    {
      Params = new JobParams(other.Params);
      CurrentLineNumber = other.CurrentLineNumber;
    }
  }
}
