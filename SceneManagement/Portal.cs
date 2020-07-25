using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Control;


namespace RPG.SceneManagement
{
  public class Portal : MonoBehaviour
  {
    enum DestinationIdentifire
    {
      A, B, C, D, E
    }

    [SerializeField] int sceneToLoad = -1;
    [SerializeField] Transform m_SpawnPoint;
    [SerializeField] DestinationIdentifire destination;
    [SerializeField] float m_FadeOutTime = 1f;
    [SerializeField] float m_FadeInTime = 2f;
    [SerializeField] float m_FadeWaitTime = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
      if (other.tag == "Player")
      {
        // 2번연속 충돌감지 방지. (2번연속 포탈에 충돌 감지될경우 scene이 2번로드되서 spawn위치가 이상해짐)
        GetComponent<Collider>().enabled = false;
        StartCoroutine(Transition());
      }
    }

    private IEnumerator Transition()
    {
      if (sceneToLoad < 0)
      {
        Debug.LogError("Scene to load not set.");
        yield break;
      }

      // 현재 오브젝트를 다른씬 로드할때 남겨놓음. destroy할 경우 밑에 코드들이 전부 날라감.
      DontDestroyOnLoad(gameObject);
      Fader fader = FindObjectOfType<Fader>();
      SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
      var playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
      playerController.enabled = false;
      // Remove control

      if (fader != null)
        yield return fader.FadeOut(m_FadeOutTime);

      savingWrapper.Save();

      yield return SceneManager.LoadSceneAsync(sceneToLoad);
      // Remove control form loaded player
      // 씬을 넘어왔으니 플레이어를 다시 찾아야한다.
      var newPlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
      newPlayerController.enabled = false;

      savingWrapper.Load();

      Portal otherPortal = GetOtherPortal();
      UpdatePlayer(otherPortal);

      savingWrapper.Save();

      // 2번연속 충돌감지 방지하기위해 꺼놓은 충돌감지를 새로운 scene이로드 됬을때 다시 켜준다.
      GetComponent<Collider>().enabled = true;

      yield return new WaitForSeconds(m_FadeWaitTime);
      // if (fader != null)
      // yield return fader.FadeIn(m_fadeInTime);
      // Fader에서 Coroutine이 아닌 IEnumerator를 return 할때 사용했음
      // yield return이 끝나기전까지 newPlayerController.enabled = true;를 하지못하기때문에
      // Fade하는동안 Player는 움직이지 못함.
      // Fade가 끝나지 않아도 플레이어를 움직이게 하기위해
      // FadeIn은 Coroutine 자체를 return 하게해서 다른 곳에서 Coroutine을 실행 할 수 있게한다.
      // FadeIn, Fadeout을 void로 하지않은 이유는 Coroutine을 return 하면 yield return도 사용가능하기 때문.
      if (fader != null)
        fader.FadeIn(m_FadeInTime);

      // Restore control
      // 해당 포털이 Destroy하기전에 빠르게 포탈을 계속 이동할경우
      // Player위치렉이 걸린다.
      // 로딩이 다 끝나고 안정될때까지 Player를 못움직이게해야함.
      newPlayerController.enabled = true;
      Destroy(gameObject);
    }

    private void UpdatePlayer(Portal otherPortal)
    {
      GameObject player = GameObject.FindWithTag("Player");
      // player.transform.position = otherPortal.m_SpawnPoint.position;
      // navmesh가 그려지기전에 player의 포지션을정해버리면 spawn이 제대로 되지 않을 수 있다.
      NavMeshAgent playerNavMesh = player.GetComponent<NavMeshAgent>();
      playerNavMesh.enabled = false;
      playerNavMesh.Warp(otherPortal.m_SpawnPoint.position);
      player.transform.rotation = otherPortal.m_SpawnPoint.rotation;
      playerNavMesh.enabled = true;
    }

    private Portal GetOtherPortal()
    {
      foreach (Portal portal in FindObjectsOfType<Portal>())
      {
        if (portal == this) continue;
        if (portal.destination != destination) continue;

        return portal;
      }
      return null;
    }
  }
}