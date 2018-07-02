using M3D.Spooling.Common;

namespace M3D.Spooling.Client
{
  public interface IAsyncCallResult
  {
    object AsyncState { get; }

    CommandResult CallResult { get; }

    bool CalledOnIdle { get; }
  }
}
