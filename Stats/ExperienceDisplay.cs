using UnityEngine;
using UnityEngine.UI;
using System;

namespace RPG.Stats
{
  public class ExperienceDisplay : MonoBehaviour
  {
    Experience experience;
    Text textDisplay;

    private void Awake()
    {
      experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
      textDisplay = GetComponent<Text>();
    }

    private void Update()
    {
      textDisplay.text = String.Format("{0:0}", experience.GetPoints());
    }
  }
}