using M3D.Spooling.Common;
using M3D.Spooling.Core;

namespace M3D.Spooling.Client
{
  internal class AsyncCallObject : IAsyncCallResult
  {
    public AsyncCallback callback;
    public CallBackType callbackType;
    public CommandResult callresult;
    public object state;
    public uint callID;
    public bool idle_callback;

    public AsyncCallObject(AsyncCallback callback, object state, IPrinter printer)
    {
      this.callback = callback;
      this.state = state;
      callbackType = CallBackType.CallID;
      callID = GetNextCallID(printer.Info.synchronization.LastCompletedRPCID);
    }

    private uint GetNextCallID(uint lastID)
    {
      return (uint) ((ulong) lastID + (ulong) (SpoolerServer.RandomGenerator.Next() % 256)) % 429367296U + 1U;
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
        return callresult;
      }
    }

    public bool CalledOnIdle
    {
      get
      {
        return idle_callback;
      }
    }
  }
}
