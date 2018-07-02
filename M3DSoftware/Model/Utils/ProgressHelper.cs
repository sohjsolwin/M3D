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
