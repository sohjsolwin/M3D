// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.AsyncCallObject
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.callbackType = CallBackType.CallID;
      this.callID = this.GetNextCallID(printer.Info.synchronization.LastCompletedRPCID);
    }

    private uint GetNextCallID(uint lastID)
    {
      return (uint) ((ulong) lastID + (ulong) (SpoolerServer.RandomGenerator.Next() % 256)) % 429367296U + 1U;
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
        return this.callresult;
      }
    }

    public bool CalledOnIdle
    {
      get
      {
        return this.idle_callback;
      }
    }
  }
}
