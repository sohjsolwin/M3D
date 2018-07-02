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
      m_percentageDelagate = perDel;
      m_percentIndex = 0;
      m_stageCount = StageCount;
    }

    public void SetStage(int StageIndexSize)
    {
      m_inc = StageIndexSize / (100 / m_stageCount);
      m_index = m_inc;
    }

    public void Process(int index)
    {
      if (index < m_index)
      {
        return;
      }

      m_index += m_inc;
      ProgressHelper.PercentageDelagate percentageDelagate = m_percentageDelagate;
      if (percentageDelagate == null)
      {
        return;
      }

      percentageDelagate(++m_percentIndex);
    }

    public delegate void PercentageDelagate(int percentage);
  }
}
