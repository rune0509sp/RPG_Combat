using System;
using GameDevTV.Utils;
using UnityEngine;


namespace RPG.Stats
{
  public class BaseStats : MonoBehaviour
  {
    [Range(1, 99)] [SerializeField] private int startingLevel = 1;
    [SerializeField] private CharacterClass characterClass;
    [SerializeField] private Progression progression = null;
    [SerializeField] GameObject levelUpParticleEffect = null;
    [SerializeField] bool shouldUseModifiers = false;

    public event Action onLevelUp;

    LazyValue<int> curruntLevel;

    Experience experience;

    private void Awake()
    {
      experience = GetComponent<Experience>();
      curruntLevel = new LazyValue<int>(CalculateLevel);
    }

    private void Start()
    {
      curruntLevel.ForceInit();
    }

    private void OnEnable()
    {
      if (experience != null)
      {
        experience.onExperienceGained += UpdateLevel;
      }
    }

    private void OnDisable()
    {
      if (experience != null)
      {
        experience.onExperienceGained -= UpdateLevel;
      }
    }

    private void UpdateLevel()
    {
      int newLevel = CalculateLevel();
      if (newLevel > curruntLevel.value)
      {
        curruntLevel.value = newLevel;
        LevelUpEffect();
        onLevelUp();
      }
    }

    private void LevelUpEffect()
    {
      Instantiate(levelUpParticleEffect, transform);
    }

    public float GetStat(Stat stat)
    {
      return (GetBaseStat(stat) + GetAdditiveModifire(stat)) * (1 + GetPercentageModifier(stat) / 100);
    }

    private float GetBaseStat(Stat stat)
    {
      return progression.GetStat(stat, characterClass, GetLevel());
    }

    public int GetLevel()
    {
      return curruntLevel.value;
    }

    private float GetAdditiveModifire(Stat stat)
    {
      if (!shouldUseModifiers) return 0;
      float total = 0;
      foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
      {
        foreach (float modifier in provider.GetPercentageModifiers(stat))
        {
          total += modifier;
        }
      }
      return total;
    }

    private float GetPercentageModifier(Stat stat)
    {
      if (!shouldUseModifiers) return 0;

      float total = 0;
      foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
      {
        foreach (float modifier in provider.GetAdditiveModifier(stat))
        {
          total += modifier;
        }
      }
      return total;
    }

    private int CalculateLevel()
    {
      Experience experience = GetComponent<Experience>();
      if (experience == null) return startingLevel;

      float currentXP = experience.GetPoints();
      int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
      for (int level = 1; level <= penultimateLevel; level++)
      {
        // Stat.ExperienceToLevelUp 은 0 index에 레벨 2에 필요한 경험치를 입력해놓았음.
        // XPToLevelUp에 level = 1 을 넣었을경우 2레벨에 필요한 경험치를 리턴해줌.
        float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);

        // 만약 2레벨에 필요한 경험치가 현재 경험치보다 많다면, 그대로의 레벨을 돌려줌.
        if (XPToLevelUp > currentXP) return level;
      }

      // XPToLevelUp의 어떠한 값보다 currentXP가 더 높을경우, penultimateLevel(Stat.ExpericenToLevelUp의 index)의 +1 만큼
      // 돌려준다. Stat.ExperienceToLevelUp 은 0 index에 레벨 2에 필요한 경험치를 입력해놓았기 때문임.
      return penultimateLevel + 1;
    }
  }
}