using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;
using UnityEngine.Events;
using System;

namespace RPG.Attributes
{
  public class Health : MonoBehaviour, ISaveable
  {
    [SerializeField] float regenerationPercentage = 70;

    // [SerializeField] UnityEvent<float> takeDamage;
    // 로 할경우 unity tool에서 식별이안됨.
    // [SerializeField]는 제네릭을 식별할 수 없음
    // Event 등록할때 Prefab 등록시 주의 사항:
    // 폴더에있는 Prefab을 등록하면안되고 Health컴포넌트를 가지고있는 Charactor Prefab에 들어가서
    // Charactor Prefab의 Health 컴포넌트 takeDamage 이벤트에, Charactor Prefab에 등록되어있는
    // Damage Text Spawner 게임오브젝트 를 넣어줘야함
    [System.Serializable]
    public class TakeDamageEvent : UnityEvent<float>
    { }
    [SerializeField] TakeDamageEvent takeDamage;
    [SerializeField] UnityEvent onDie;

    private LazyValue<float> m_HealthPoints;

    bool m_IsDead = false;

    private void Awake()
    {
      m_HealthPoints = new LazyValue<float>(GetInitialHealth);
    }

    private float GetInitialHealth()
    {
      return GetComponent<BaseStats>().GetStat(Stat.Health);
    }

    private void Start()
    {
      m_HealthPoints.ForceInit();
    }

    private void OnEnable()
    {
      GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
    }

    private void OnDisable()
    {
      GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
    }

    public bool IsDead()
    {
      return m_IsDead;
    }

    public void TakeDamage(GameObject instigator, float damage)
    {

      m_HealthPoints.value = Mathf.Max(m_HealthPoints.value - damage, 0);
      if (m_HealthPoints.value == 0)
      {
        onDie.Invoke();
        Die();
        AwardExperience(instigator);
      }
      else
      {
        takeDamage.Invoke(damage);
      }
    }

    public void Heal(float healthToRestore)
    {
      m_HealthPoints.value = Mathf.Min(m_HealthPoints.value + healthToRestore, GetMaxHealthPoints());
    }

    private void AwardExperience(GameObject instigator)
    {
      Experience experience = instigator.GetComponent<Experience>();
      if (experience == null) return;

      float reward = GetComponent<BaseStats>().GetStat(Stat.ExperienceReward);
      experience.GainExperience(reward);
    }

    public float GetHealthPoints()
    {
      return m_HealthPoints.value;
    }

    public float GetMaxHealthPoints()
    {
      return GetComponent<BaseStats>().GetStat(Stat.Health);
    }

    public float GetPercentage()
    {
      return 100 * GetFraction();
    }

    public float GetFraction()
    {
      return m_HealthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
    }

    private void Die()
    {
      if (m_IsDead) return;
      m_IsDead = true;
      GetComponent<Animator>().SetTrigger("die");
      GetComponent<ActionScheduler>().CancelCurrentAction();
    }

    private void RegenerateHealth()
    {
      float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
      m_HealthPoints.value = Mathf.Max(m_HealthPoints.value, regenHealthPoints);
    }

    public object CaptureSate()
    {
      return m_HealthPoints.value;
    }

    public void RestoreSate(object state)
    {
      m_HealthPoints.value = (float)state;

      if (m_HealthPoints.value <= 0) Die();
    }

  }
}
