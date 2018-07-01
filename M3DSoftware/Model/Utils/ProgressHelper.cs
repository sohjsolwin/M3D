// Decompiled with JetBrains decompiler
// Type: M3D.Model.Utils.ProgressHelper
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

namespace M3D.Model.Utils
{
  public class ProgressHelper
  {
    private int m_inc;
    private int m_index;
    private int m_stageCount;
    private int m_percentIndex;
    private ProgressHelper.PercentageDelagate m_percentageDelagate;

    public ProgressHelper(ProgressHelper.PercentageDelagate perDel, int StageCount)
    {
      this.m_percentageDelagate = perDel;
      this.m_percentIndex = 0;
      this.m_stageCount = StageCount;
    }

    public void SetStage(int StageIndexSize)
    {
      this.m_inc = StageIndexSize / (100 / this.m_stageCount);
      this.m_index = this.m_inc;
    }

    public void Process(int index)
    {
      if (index < this.m_index)
        return;
      this.m_index += this.m_inc;
      ProgressHelper.PercentageDelagate percentageDelagate = this.m_percentageDelagate;
      if (percentageDelagate == null)
        return;
      percentageDelagate(++this.m_percentIndex);
    }

    public delegate void PercentageDelagate(int percentage);
  }
}
