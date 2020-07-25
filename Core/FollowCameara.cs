using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
  public class FollowCameara : MonoBehaviour
  {
    [SerializeField] Transform target;

    // Update is called once per frame
    void LateUpdate()
    {
      this.transform.position = target.position;
    }
  }

}