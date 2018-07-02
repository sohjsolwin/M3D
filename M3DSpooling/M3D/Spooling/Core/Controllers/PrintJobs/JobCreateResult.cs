using M3D.Spooling.Client;
using System.Collections.Generic;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal class JobCreateResult
  {
    public ProcessReturn Result;
    public List<MessageType> Warnings;
    public AbstractJob Job;

    public JobCreateResult(AbstractJob job, ProcessReturn result, List<MessageType> warnings)
    {
      Warnings = warnings;
      Result = result;
      Job = job;
    }
  }
}
