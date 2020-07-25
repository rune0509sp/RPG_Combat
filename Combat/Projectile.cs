using UnityEngine;
using RPG.Attributes;
using UnityEngine.Events;

namespace RPG.Combat
{
  public class Projectile : MonoBehaviour
  {
    [SerializeField] private float m_Speed = 1f;
    [SerializeField] private bool m_IsHoming = false;
    [SerializeField] private GameObject m_HitEffect = null;
    [SerializeField] float m_MaxLifeTime = 10;
    [SerializeField] GameObject[] m_DestroyOnHit = null;
    [SerializeField] float m_LifeAfterImpact = 2;
    [SerializeField] UnityEvent onHit;

    Health target = null;
    GameObject instigator = null;
    float damage = 0;

    private void Start()
    {
      transform.LookAt(GetAimLocation());
    }

    // Update is called once per frame
    void Update()
    {
      if (target == null) return;
      if (m_IsHoming && !target.IsDead()) transform.LookAt(GetAimLocation());
      // Vector3.forward LookAt으로 정한 *앞(파랑색 화살표) 방향으로 전진한다. 
      transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
    }

    public void SetTarget(Health target, GameObject instigator, float damage)
    {
      this.target = target;
      this.damage = damage;
      this.instigator = instigator;

      Destroy(gameObject, m_MaxLifeTime);
    }

    private Vector3 GetAimLocation()
    {
      CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
      if (targetCapsule == null)
      {
        return target.transform.position;
      }
      // target.position 으로 target의 위치를 얻고
      // + Vector3.up * targetCapsule.height / 2 로 상대의 타격객체의 몸통을 노린다. /2 보다 *0.5가 더 빠름.
      return target.transform.position + Vector3.up * (targetCapsule.height * 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.GetComponent<Health>() != target) return;
      if (target.IsDead()) return;
      if (m_HitEffect != null) Instantiate(m_HitEffect, GetAimLocation(), transform.rotation);
      target.TakeDamage(instigator, damage);

      m_Speed = 0;

      onHit.Invoke();

      // m_DestroyOnHit는 SerializeField
      // 자신의 화살 이미지의 3d object prefab이 들어있다.
      // 먼저 화살 이미지 제거.(?)
      foreach (GameObject toDestory in m_DestroyOnHit)
      {
        Destroy(gameObject);
      }
      Destroy(gameObject, m_LifeAfterImpact);
    }
  }

}