using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Control;
using RPG.Attributes;

namespace RPG.Combat
{
  public class WeaponPickup : MonoBehaviour, IRaycastable
  {
    [SerializeField] WeaponConfig weapon = null;
    [SerializeField] float healthToRestore = 0;
    [SerializeField] float respawnTime = 5;

    private void OnTriggerEnter(Collider other)
    {
      if (other.tag.Equals("Player"))
      {
        Pickup(other.gameObject);
      }
    }

    private void Pickup(GameObject subject)
    {
      if (weapon != null)
      {
        subject.GetComponent<Fighter>().EquipWeapon(weapon);
      }
      if (healthToRestore > 0)
      {
        subject.GetComponent<Health>().Heal(healthToRestore);
      }
      StartCoroutine(HideForSeconds(respawnTime));
    }

    private IEnumerator HideForSeconds(float seconds)
    {
      ShowPickup(false);
      yield return new WaitForSeconds(seconds);
      ShowPickup(true);
    }

    private void ShowPickup(bool shouldShow)
    {
      // this.gameobject.SetActive(shouldShow)를 안쓰는이유.
      // this( 본채 )가 비활성화되면 코루틴도 작동이 멈춘다.
      // 물체를 숨길때는
      // collider를 비활성해 충돌 효과를 없에주고
      // child object( 이미지 등 .. )을 비활성화해준다.
      GetComponent<Collider>().enabled = shouldShow;
      foreach (Transform child in transform)
      {
        child.gameObject.SetActive(shouldShow);
      }
    }

    public bool HandleRaycast(PlayerController callingController)
    {
      if (Input.GetMouseButtonDown(0))
      {
        Pickup(callingController.gameObject);
      }
      return true;
    }

    public CursorType GetCursorType()
    {
      return CursorType.Pickup;
    }
  }
}