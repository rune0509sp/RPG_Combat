using UnityEngine;
using UnityEngine.UI;
using System;

namespace RPG.Stats
{
  public class LevelDisplay : MonoBehaviour
  {
    BaseStats baseStats;
    Text textDisplay;

    private void Start()
    {
      baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
      textDisplay = GetComponent<Text>();
    }

    private void Update()
    {
      textDisplay.text = baseStats.GetLevel().ToString();
    }
  }
}