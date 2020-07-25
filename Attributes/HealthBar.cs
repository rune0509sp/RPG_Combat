using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
  public class HealthBar : MonoBehaviour
  {
    [SerializeField] Health healthcomponent = null;
    [SerializeField] RectTransform foreground = null;
    [SerializeField] Canvas rootCanvas = null;

    void Update()
    {
      if (Mathf.Approximately(healthcomponent.GetFraction(), 0) || Mathf.Approximately(healthcomponent.GetFraction(), 1))
      {
        rootCanvas.enabled = false;
        return;
      }
      rootCanvas.enabled = true;
      foreground.localScale = new Vector3(healthcomponent.GetFraction(), 1, 1);
    }
  }
}