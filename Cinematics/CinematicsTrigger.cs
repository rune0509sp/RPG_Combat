using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
  public class CinematicsTrigger : MonoBehaviour
  {
    private bool m_AlreadyTriggered = false;
    private void OnTriggerEnter(Collider other)
    {
      if (other.tag == "Player" && !m_AlreadyTriggered)
      {
        m_AlreadyTriggered = true;
        GetComponent<PlayableDirector>().Play();
      }
    }
  }
}
