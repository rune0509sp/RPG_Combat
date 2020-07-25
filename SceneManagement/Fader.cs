using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.SceneManagement
{
  public class Fader : MonoBehaviour
  {
    CanvasGroup m_CanvasGroup;
    // FadeOut 과 FadeIn 동시 실행 방지.
    // 두 Coroutine이 동시 실행되면 while 루프에서 영원히 못빠져나옴
    Coroutine currentActiveFade = null;

    // SavingWrapper의 Start()에서 Fader fader = FindObjectOfType<Fader>();를 부르기때문에
    // start 전에 컴포넌트를 찾아줘야한다. awake사용
    private void Awake()
    {
      m_CanvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeOutImmediate()
    {
      m_CanvasGroup.alpha = 1;
    }

    public Coroutine FadeOut(float time)
    {
      return Fade(1f, time);
    }

    public Coroutine FadeIn(float time)
    {
      return Fade(0, time);
    }

    // 해당 Fade 또는 FadeOut, FadeIn을 부른 함수에서
    // Fade가 끝나지 않아도 다른 행동을 하기위해
    // Fade는 Coroutine 자체를 return 하게한다.
    // FadeIn, Fadeout, Fade을 void로 하지않은 이유는 Coroutine을 return 하면 yield return도 사용가능하기 때문.
    public Coroutine Fade(float target, float time)
    {
      if (currentActiveFade != null)
      {
        StopCoroutine(currentActiveFade);
      }
      currentActiveFade = StartCoroutine(FadeRoutine(target, time));
      return currentActiveFade;
    }

    private IEnumerator FadeRoutine(float target, float time)
    {
      while (!Mathf.Approximately(m_CanvasGroup.alpha, target))
      {
        // alpha += 1 / time / deltaTime
        // alpha += deltaTime / time
        m_CanvasGroup.alpha = Mathf.MoveTowards(m_CanvasGroup.alpha, target, Time.deltaTime / time);
        yield return null;
      }
    }
  }
}