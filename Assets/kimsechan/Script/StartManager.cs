using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public List<GameObject> job = new List<GameObject>();
    
    // 위치를 저장할 변수 (기본값은 0,0,0)
    public Vector3 spawnPoint = Vector3.zero; 
    
    [HideInInspector] public int Job = -1; 

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // [수정됨 1] 좌표(pos)를 매개변수로 받아옵니다.
    public void StartJobCoroutine()
    {
        StartCoroutine(_Job());
        SceneManager.LoadScene(0); // 0번 씬으로 이동
    }

    IEnumerator _Job()
    {
        // 씬 로드가 시작된 직후 안전하게 한 프레임 대기
        yield return null; 

        yield return new WaitUntil(() => Job >= 0);

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            Debug.Log("이미 플레이어가 존재합니다.");
            yield break; 
        }

        if (Job < job.Count)
        {
            // [수정됨 2] 저장해둔 spawnPoint 위치에 생성합니다.
            GameObject player = Instantiate(job[Job], spawnPoint, Quaternion.identity);

            if (player.GetComponent<PlayerManager>() == null)
            {
                player.AddComponent<PlayerManager>();
                player.tag = "Player";
            }
        }
        else
        {
            Debug.LogWarning("잘못된 직업 번호입니다.");
        }
    }

    public void SelectJob(int jobIndex)
    {
        Job = jobIndex;
        Debug.Log("선택한 직업 번호: " + Job);
    }
}