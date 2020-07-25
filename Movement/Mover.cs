using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;

namespace RPG.Movement
{
  public class Mover : MonoBehaviour, IAction, ISaveable
  {
    [SerializeField] float m_MaxSpeed = 6f;
    [SerializeField] float maxNavPathLength = 40f;

    private NavMeshAgent m_NavMeshAgent;
    private Animator m_Animator;
    private ActionScheduler m_ActionScheduler;
    private Health m_Health;

    private void Awake()
    {
      m_NavMeshAgent = GetComponent<NavMeshAgent>();
      m_Animator = GetComponent<Animator>();
      m_ActionScheduler = GetComponent<ActionScheduler>();
      m_Health = GetComponent<Health>();
    }

    void Update()
    {
      m_NavMeshAgent.enabled = !m_Health.IsDead();
      UpdateAnimator();
    }

    public void StartMoveAction(Vector3 destination, float speedFraction)
    {
      m_ActionScheduler.StartAction(this);
      MoveTo(destination, speedFraction);
    }

    public bool CanMoveTo(Vector3 destination)
    {
      NavMeshPath path = new NavMeshPath();

      // 내위치로부터 target까지, NavMesh 영역까지 거리 계산
      // path에 out됨.
      bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
      if (!hasPath) return false;
      // path.status를 검색해서 pathComplete(적합한경로)가 아닐경우 return false;
      if (path.status != NavMeshPathStatus.PathComplete) return false;

      // 마우스 거리가 지정한 거리(maxNavPathLength)보다멀경우 이동 불가능
      if (GetPathLength(path) > maxNavPathLength) return false;

      return true;
    }

    public void MoveTo(Vector3 destination, float speedFraction)
    {
      m_NavMeshAgent.isStopped = false;
      m_NavMeshAgent.speed = m_MaxSpeed * Mathf.Clamp01(speedFraction);
      m_NavMeshAgent.SetDestination(destination);
    }

    public void Cancel()
    {
      m_NavMeshAgent.isStopped = true;
    }

    private void UpdateAnimator()
    {
      Vector3 velocity = m_NavMeshAgent.velocity;
      Vector3 localVelocity = this.transform.InverseTransformDirection(velocity);
      float speed = localVelocity.z;
      m_Animator.SetFloat("forwardSpeed", speed);
    }

    private float GetPathLength(NavMeshPath path)
    {
      // corner 간의 거리로 path까지의 최단 거리를 잰다.
      // corner 설명그림
      // ㄱ\_.(4)    (mouse point at 4)
      //   ㄱ\_.(3)
      //     ㄱ\.(2)
      // --------------------.(1)
      // o(Player)
      //
      float total = 0;
      // 코너가 2개이하인 경우 코너간 거리 계산이 불가능하니 0을 return
      if (path.corners.Length < 2) return total;

      // 모든 코너들을 돌아보면서 코너간의 거리를 계산해 total에 더해준다.
      // 코너 설명그림의 .과 .의 위치
      for (int i = 0; i < path.corners.Length - 1; i++)
      {
        total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
      }

      return total;
    }

    public object CaptureSate()
    {
      return new SerializableVector3(transform.position);
    }

    public void RestoreSate(object state)
    {
      SerializableVector3 position = (SerializableVector3)state;
      m_NavMeshAgent.enabled = false;
      transform.position = position.Tovector();
      m_NavMeshAgent.enabled = true;
      GetComponent<ActionScheduler>().CancelCurrentAction();
    }
  }
}
