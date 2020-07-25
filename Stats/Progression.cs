using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPG.Stats
{
  [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
  public class Progression : ScriptableObject
  {
    [SerializeField] ProgressionCharacterClass[] characterClasses = null;

    Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;

    public float GetStat(Stat stat, CharacterClass characterClass, int level)
    {
      // 일일이 GetStat 할때마다 foreach로 모든 저장된 내용을 훑어보면 코스트가 비싸기때문에
      // 첫 GetStat에서 Dictionary로 캐싱 해준다.
      BuildLookup();
      if (!lookupTable.ContainsKey(characterClass)) return 0;

      float[] levels = lookupTable[characterClass][stat];
      if (levels.Length < level) return 0;

      return levels[level - 1];
    }

    public int GetLevels(Stat stat, CharacterClass characterClass)
    {
      BuildLookup();
      float[] levels = lookupTable[characterClass][stat];
      return levels.Length;
    }

    // 일일이 GetStat 할때마다 foreach로 모든 저장된 내용을 훑어보면 코스트가 비싸기때문에
    // 첫 GetStat에서 Dictionary로 캐싱 해준다.
    private void BuildLookup()
    {
      if (lookupTable != null) return;
      lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();
      foreach (ProgressionCharacterClass progressionClass in characterClasses)
      {
        var statLookupTable = new Dictionary<Stat, float[]>();
        foreach (ProgressionStat progressionStat in progressionClass.GetStats())
        {
          statLookupTable[progressionStat.GetStat()] = progressionStat.GetLevels();
        }
        lookupTable[progressionClass.GetCharacterClass()] = statLookupTable;
      }
    }

    [System.Serializable]
    class ProgressionCharacterClass
    {
      [SerializeField] CharacterClass characterClass;
      //[SerializeField] float[] health;
      [SerializeField] ProgressionStat[] stats;

      public CharacterClass GetCharacterClass()
      {
        return characterClass;
      }

      public ProgressionStat[] GetStats()
      {
        return stats;
      }
    }

    [System.Serializable]
    class ProgressionStat
    {
      [SerializeField] private Stat stat;
      [SerializeField] private float[] levels;

      public Stat GetStat()
      {
        return stat;
      }

      public float[] GetLevels()
      {
        return levels;
      }
    }

  }
}