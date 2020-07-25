using UnityEngine;
using RPG.Core;
using RPG.Movement;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;

namespace RPG.Combat
{
  public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
  {
    private Health target;

    private ActionScheduler m_ActionScheduler;
    private Mover m_Mover;
    private Animator m_Animator;

    [SerializeField] private float m_TimebetweenAttacks = 1f;
    [SerializeField] private Transform rightHandTransform = null;
    [SerializeField] private Transform leftHandTransform = null;
    [SerializeField] private WeaponConfig defaultWeapon = null;

    private float m_TimeSinceLastAttack = Mathf.Infinity;
    WeaponConfig currentWeaponConfig;
    LazyValue<Weapon> currentWeapon;

    private void Awake()
    {
      m_ActionScheduler = GetComponent<ActionScheduler>();
      m_Mover = GetComponent<Mover>();
      m_Animator = GetComponent<Animator>();
      currentWeaponConfig = defaultWeapon;
      currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
    }
    private void Start()
    {
      currentWeapon.ForceInit();
    }

    private void Update()
    {
      m_TimeSinceLastAttack += Time.deltaTime;

      if (target == null) return;
      if (target.IsDead()) return;

      if (!GetIsInRange(target.transform))
      {
        m_Mover.MoveTo(target.transform.position, 1f);
      }
      else
      {
        m_Mover.Cancel();
        AttackBehaviour();
      }
    }

    private Weapon SetupDefaultWeapon()
    {
      return AttachWeapon(defaultWeapon);
    }

    public void EquipWeapon(WeaponConfig weapon)
    {
      currentWeaponConfig = weapon;
      currentWeapon.value = AttachWeapon(weapon);
    }

    private Weapon AttachWeapon(WeaponConfig weapon)
    {
      Animator animator = GetComponent<Animator>();
      return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
    }

    public Health GetTarget()
    {
      return target;
    }

    private void AttackBehaviour()
    {
      transform.LookAt(target.transform);
      if (m_TimeSinceLastAttack > m_TimebetweenAttacks)
      {
        // animation has trigger of Hit method
        // this will trigger the Hit() event.
        TriggerAttact();
        m_TimeSinceLastAttack = 0;
      }
    }

    private void TriggerAttact()
    {
      m_Animator.ResetTrigger("stopAttack");
      m_Animator.SetTrigger("attack");
    }

    // 근거리 무기 Animation에 Hit method trigger가 있음. 해당 animation이 trigger가 있는 frame에 도착하면
    // 자동으로 Hit 메소드가 call됨. 활의 경우 shoot이벤트가 있기때문에 활은 Hit 메소드를 부를 수 없음.
    // Animation Event
    private void Hit()
    {
      if (target == null) return;

      float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

      if (currentWeapon.value != null)
      {
        currentWeapon.value.OnHit();
      }

      if (currentWeaponConfig.Hasprojectile())
      {
        currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
      }
      else
      {
        target.TakeDamage(gameObject, damage);
      }
    }

    // 활 Animation에 Shoot 메서드 trigger가 있음.
    private void Shoot()
    {
      Hit();
    }

    private bool GetIsInRange(Transform targetTransform)
    {
      return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
    }

    public void Attack(GameObject combatTarget)
    {
      m_ActionScheduler.StartAction(this);
      target = combatTarget.GetComponent<Health>();
    }

    public bool CanAttack(GameObject combatTarget)
    {
      if (combatTarget == null) return false;
      if (!m_Mover.CanMoveTo(combatTarget.transform.position) &&
        !GetIsInRange(combatTarget.transform))
      {
        return false;
      }
      Health targetToTest = combatTarget.GetComponent<Health>();
      return targetToTest != null && !targetToTest.IsDead();
    }

    public void Cancel()
    {
      StopAttack();
      target = null;
      m_Mover.Cancel();
    }

    private void StopAttack()
    {
      m_Animator.ResetTrigger("attack");
      m_Animator.SetTrigger("stopAttack");
    }

    public IEnumerable<float> GetAdditiveModifier(Stat stat)
    {
      if (stat == Stat.Damage)
      {
        yield return currentWeaponConfig.GetDamage();
      }
    }

    public IEnumerable<float> GetPercentageModifiers(Stat stat)
    {
      if (stat == Stat.Damage)
      {
        yield return currentWeaponConfig.GetPercentageBonus();
      }
    }

    public object CaptureSate()
    {
      return currentWeaponConfig.name;
    }

    public void RestoreSate(object state)
    {
      string weaponName = (string)state;
      // Resources 폴더는 특별하다. Resources폴더에 있는 파일들은 이렇게 불러 올 수 있음.
      // 캐싱하는게 좋을거다.
      WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
      EquipWeapon(weapon);
    }
  }
}