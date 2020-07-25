using UnityEngine;
using UnityEngine.UI;
using System;
using RPG.Attributes;

namespace RPG.Combat
{
  public class EnemyHealthDisplay : MonoBehaviour
  {
    Fighter fighter;

    private void Awake()
    {
      fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
    }

    private void Update()
    {
      Health health = fighter.GetTarget();
      if (health == null)
      {
        GetComponent<Text>().text = "N/A";
        return;
      }
      GetComponent<Text>().text = String.Format("{0:0}/{1:0}", health.GetHealthPoints(), health.GetMaxHealthPoints());

    }
  }
}