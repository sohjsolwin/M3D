using M3D.Spooling.Common;

namespace M3D.Spooling.Client
{
  public class SimpleAsyncCallResult : IAsyncCallResult
  {
    private object state;
    private CommandResult result;

    public SimpleAsyncCallResult(object state, CommandResult result)
    {
      this.state = state;
      this.result = result;
    }

    public object AsyncState
    {
      get
      {
        return state;
      }
    }

    public CommandResult CallResult
    {
      get
      {
        return result;
      }
    }

    public bool CalledOnIdle
    {
      get
      {
        return false;
      }
    }
  }
}
