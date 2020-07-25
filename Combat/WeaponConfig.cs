using RPG.Attributes;
using UnityEngine;

namespace RPG.Combat
{
  [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
  public class WeaponConfig : ScriptableObject
  {
    [SerializeField] private Weapon m_equippedPrefab = null;
    [SerializeField] private AnimatorOverrideController m_animatorOverride = null;
    [SerializeField] private float m_WeaponDamage = 5f;
    [SerializeField] private float m_PercentageBonus = 0;
    [SerializeField] private float m_WeaponRange = 2f;
    [SerializeField] bool isRightHanded = true;
    [SerializeField] Projectile projectile = null;

    const string weaponName = "Weapon";

    public Weapon Spawn(Transform rightHand, Transform leftHand, Animator animator)
    {
      DestroyOldWeapon(rightHand, leftHand);
      Weapon weapon = null;
      if (m_equippedPrefab != null)
      {
        weapon = Instantiate(m_equippedPrefab, GetTransform(rightHand, leftHand));
        weapon.gameObject.name = weaponName;
      }

      // animator.runtimeAnimatorController는 최상위 컨트롤러임. Player의 Animator에 있음.
      // Player의 Animator의 컨트롤러에는 기본 unarmed(펀치) 애니메이션이 장착 되어있음.
      // 최상위 애니메이션(펀치)를 AnumatorOverrideController로 캐스팅해서 변수로 넣어줌.
      var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
      if (m_animatorOverride != null)
      {
        animator.runtimeAnimatorController = m_animatorOverride;
      }
      else if (overrideController != null)
      {
        // 위에서 최상위 animator의 애니메이션 컨트롤러를 overrideController에 넣어주었으니 else if가 실행된다
        // 최상위 animator runtimeAnimatorController에 처음 저장한 최상위 에니메이터를 넣어 실행한다.
        // 이렇게 하는이유는, 만약 Fireball에 동작 애니메이션이 설정안되어있을때
        // Sword를 먹고 Fireball을 먹으면 Fireball을 발사할때 Sword 동작 애니메이션이 실행된다.
        // 만약 해당 무기의 애니메이션이 저장 되지않았다면 기본(펀치)동작 애니메이션이 실행되도록 한다.
        animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
      }
      return weapon;
    }

    private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
    {
      Transform oldWeapon = rightHand.Find(weaponName);
      if (oldWeapon == null)
      {
        oldWeapon = leftHand.Find(weaponName);
      }
      if (oldWeapon == null) return;

      oldWeapon.name = "DESTROYING";
      Destroy(oldWeapon.gameObject);
    }

    public bool Hasprojectile()
    {
      return projectile != null;
    }

    public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target, GameObject instigator, float calculatedDamage)
    {
      Projectile projectileInstance = Instantiate(projectile, GetTransform(rightHand, leftHand).position, Quaternion.identity);
      projectileInstance.SetTarget(target, instigator, calculatedDamage);
    }

    private Transform GetTransform(Transform rightHand, Transform leftHand)
    {
      Transform handTransform;
      if (isRightHanded) handTransform = rightHand;
      else handTransform = leftHand;
      return handTransform;
    }

    public float GetDamage()
    {
      return m_WeaponDamage;
    }

    public float GetPercentageBonus()
    {
      return m_PercentageBonus;
    }

    public float GetRange()
    {
      return m_WeaponRange;
    }
  }
}