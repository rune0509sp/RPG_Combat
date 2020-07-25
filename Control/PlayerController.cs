using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Attributes;
using System;
using UnityEngine.EventSystems;
using UnityEngine.AI;

namespace RPG.Control
{
  public class PlayerController : MonoBehaviour
  {
    private Mover m_Mover;
    private Fighter m_Fighter;
    private Health m_Health;

    // unity tool에서 struct를 식별할 수 있게 해야한다.
    [System.Serializable]
    struct CursorMapping
    {
      public CursorType type;
      public Texture2D texture;
      public Vector2 hotspot;
    }

    [SerializeField] CursorMapping[] cursorMappings = null;
    [SerializeField] float maxNavMeshProjectionDistance = 1f;
    [SerializeField] float raycastRadius = 1f;

    private void Awake()
    {
      m_Mover = GetComponent<Mover>();
      m_Fighter = GetComponent<Fighter>();
      m_Health = GetComponent<Health>();
    }

    private void Update()
    {
      if (InteractWithUI()) return;
      if (m_Health.IsDead())
      {
        SetCursor(CursorType.None);
        return;
      }
      if (InteractWithComponent()) return;
      if (InteractWithMovement()) return;

      SetCursor(CursorType.None);
    }

    private bool InteractWithUI()
    {
      // When The pointer is over UI return true
      // Fader gameobject가 모든 스크린위를 덮고 있을 수 있다.
      // Fader의 CanvasGroup의 Interactable과 BlocksRaycast 체크를 풀어줘야한다.
      // 그렇지않으면 Fader로 인해 계속 true를 return 하게된다.
      if (EventSystem.current.IsPointerOverGameObject())
      {
        SetCursor(CursorType.UI);
        return true;
      };
      return false;
    }

    private bool InteractWithComponent()
    {
      RaycastHit[] hits = RaycastAllSorted();
      foreach (RaycastHit hit in hits)
      {
        IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
        foreach (IRaycastable raycastable in raycastables)
        {
          if (raycastable.HandleRaycast(this))
          {
            SetCursor(raycastable.GetCursorType());
            return true;
          }
        }
      }
      return false;
    }

    private RaycastHit[] RaycastAllSorted()
    {
      RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);
      float[] distances = new float[hits.Length];

      for (int i = 0; i < hits.Length; i++)
      {
        distances[i] = hits[i].distance;
      }
      // distances를 기준으로 hits를 Sort 해준다.
      // ex distances{2.5, 1.5} hits{Weapon, Enemy} 일 경우
      // distances{1.5, 2.5} hits{Enemy, Weapon}으로 Sort해줌.
      // Sort<TKey,TValue>(TKey[], TValue[])  
      Array.Sort(distances, hits);
      return hits;
    }

    private bool InteractWithMovement()
    {
      Vector3 target;
      bool hasHit = RaycastNavMesh(out target);
      if (hasHit)
      {
        if (!GetComponent<Mover>().CanMoveTo(target)) return false;

        if (Input.GetMouseButtonDown(0))
        {
          m_Mover.StartMoveAction(target, 1f);
        }
        SetCursor(CursorType.Movement);
        return true;
      }
      return false;
    }

    private bool RaycastNavMesh(out Vector3 target)
    {
      target = new Vector3();
      RaycastHit hit;
      bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
      if (!hasHit) return false;

      NavMeshHit navMeshHit;
      // Sampleposition Find the closest position on the NavMesh
      bool hasCastToNavMesh = NavMesh.SamplePosition(
        hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);

      if (!hasCastToNavMesh) return false;

      target = navMeshHit.position;

      return true;
    }

    private void SetCursor(CursorType type)
    {
      CursorMapping mapping = GetCursorMapping(type);
      Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
    }

    private CursorMapping GetCursorMapping(CursorType type)
    {
      foreach (CursorMapping mapping in cursorMappings)
      {
        if (mapping.type == type)
          return mapping;
      }
      return cursorMappings[0];
    }

    private static Ray GetMouseRay()
    {
      return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
  }
}