using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Attributes;
using GameDevTV.Utils;
using System;

namespace RPG.Control
{
  public class AIController : MonoBehaviour
  {
    [SerializeField] private float m_chaseDistance = 5f;
    [SerializeField] private float m_SuspicionTime = 3f;
    [SerializeField] private float m_AggroCoolDownTime = 5f;
    [SerializeField] private PatrolPath m_PatrolPath;
    [SerializeField] private float m_WaypointTolerance = 1f;
    [SerializeField] private float m_WaypointDwellTime = 2f;
    [Range(0, 1)] [SerializeField] private float m_PattrolSpeedFraction = 0.2f;
    [SerializeField] float m_ShoutDistance = 5f;

    private GameObject player;
    private Fighter m_Fighter;
    private Health m_Health;
    private Mover m_Mover;
    private float m_TimeSinceLastSawPlayer = Mathf.Infinity;
    private float m_TimeSinceArrivedAtWaypoint = Mathf.Infinity;


    LazyValue<Vector3> m_GuardPosition;
    float timeSinceAggrevated = Mathf.Infinity;
    private int m_CurrentWaypointIndex = 0;

    private void Awake()
    {
      player = GameObject.FindWithTag("Player");
      m_Fighter = GetComponent<Fighter>();
      m_Health = GetComponent<Health>();
      m_Mover = GetComponent<Mover>();
      m_GuardPosition = new LazyValue<Vector3>(GetGuardPosition);
    }
    private void Start()
    {
      m_GuardPosition.ForceInit();
    }
    private void Update()
    {
      if (m_Health.IsDead()) return;

      if (IsAggrevated() && m_Fighter.CanAttack(player))
      {
        AttackBehaviour();
      }
      else if (m_TimeSinceLastSawPlayer < m_SuspicionTime)
      {
        SuspicionBehaviour();
      }
      else
      {
        PatrolBehaviour();
      };

      // 추후 코루틴으로 변경
      UpdateTimers();
    }

    public void Aggrevate()
    {
      timeSinceAggrevated = 0;
    }

    private Vector3 GetGuardPosition()
    {
      return transform.position;
    }

    private void UpdateTimers()
    {
      m_TimeSinceLastSawPlayer += Time.deltaTime;
      m_TimeSinceArrivedAtWaypoint += Time.deltaTime;
      timeSinceAggrevated += Time.deltaTime;
    }

    private void PatrolBehaviour()
    {
      Vector3 nextPosition = m_GuardPosition.value;
      if (m_PatrolPath != null)
      {
        if (AtWaypoint())
        {
          m_TimeSinceArrivedAtWaypoint = 0;
          CycleWaypoint();
        }
        nextPosition = GetCurrentWaypoint();
      }

      if (m_TimeSinceArrivedAtWaypoint > m_WaypointDwellTime)
        m_Mover.StartMoveAction(nextPosition, m_PattrolSpeedFraction);
    }

    private bool AtWaypoint()
    {
      float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
      return distanceToWaypoint < m_WaypointTolerance;
    }

    private void CycleWaypoint()
    {
      m_CurrentWaypointIndex = m_PatrolPath.GetNextIndex(m_CurrentWaypointIndex);
    }

    private Vector3 GetCurrentWaypoint()
    {
      return m_PatrolPath.GetWaypoint(m_CurrentWaypointIndex);
    }

    private void SuspicionBehaviour()
    {
      GetComponent<ActionScheduler>().CancelCurrentAction();
    }

    private void AttackBehaviour()
    {
      m_TimeSinceLastSawPlayer = 0;
      m_Fighter.Attack(player);

      AggrevateNearbyEnemies();
    }

    private void AggrevateNearbyEnemies()
    {
      RaycastHit[] hits = Physics.SphereCastAll(transform.position, m_ShoutDistance, Vector3.up, 0);
      foreach (RaycastHit hit in hits)
      {
        AIController ai = hit.collider.GetComponent<AIController>();
        if (ai == null) continue;

        ai.Aggrevate();
      }
    }

    private bool IsAggrevated()
    {
      float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
      return distanceToPlayer < m_chaseDistance || timeSinceAggrevated < m_AggroCoolDownTime;
    }

    // called by Unity
    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawWireSphere(transform.position, m_chaseDistance);
    }
  }
}
