using UnityEngine;
using UnityEngine.UI;

namespace RPG.Core
{
  public class CameraFacing : MonoBehaviour
  {
    private void LateUpdate()
    {
      transform.forward = Camera.main.transform.forward;
    }
  }
}
