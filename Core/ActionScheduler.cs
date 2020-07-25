using UnityEngine;

namespace RPG.Core
{
  public class ActionScheduler : MonoBehaviour
  {
    IAction m_CurrentAction;

    public void StartAction(IAction action)
    {
      if (m_CurrentAction == action) return;

      if (m_CurrentAction != null)
      {
        m_CurrentAction.Cancel();
      }
      m_CurrentAction = action;
    }

    public void CancelCurrentAction()
    {
      StartAction(null);
    }
  }
}