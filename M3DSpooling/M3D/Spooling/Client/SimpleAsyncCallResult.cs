// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.SimpleAsyncCallResult
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        return this.state;
      }
    }

    public CommandResult CallResult
    {
      get
      {
        return this.result;
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
