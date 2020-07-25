using UnityEngine;
using RPG.Saving;
using System;

namespace RPG.Stats
{
  public class Experience : MonoBehaviour, ISaveable
  {
    [SerializeField] float experiencePoints = 0;

    // public delegate void ExperienceGainedDelegate();
    // return type이 void에 매개변수가 없는 delegate는 System 네임스페이스에 있는 Action으로 대체 가능
    // public event ExperienceGainedDelegate onExperienceGained;

    public event Action onExperienceGained;

    public void GainExperience(float experience)
    {
      experiencePoints += experience;
      onExperienceGained();
    }

    public float GetPoints()
    {
      return experiencePoints;
    }

    public object CaptureSate()
    {
      return experiencePoints;
    }

    public void RestoreSate(object state)
    {
      experiencePoints = (float)state;
    }
  }
}