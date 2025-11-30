using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class Red_bird : BossManager
{
    [HideInInspector] public Transform target;
    public float moveSpeed = 3f;
    // 🌟 보스 스프라이트 페이드 아웃 시간 설정
    public float bossFadeDuration = 1.0f; 

    [Header("Heart Spawn Settings")]
    public GameObject heartPrefab;
    public float spawnInterval = 10f;
    public Vector2 spawnRangeX = new Vector2(-8f, 8f);
    public Vector2 spawnRangeY = new Vector2(-4f, 4f);
    public float heartZ = 0f;
    private Coroutine heartSpawnCoroutine;

    [Header("Die & Scene Transition")]
    public string nextSceneName = "GameClearScene";

    // 🌟 보스 사망 상태를 추적하는 변수 추가 (중복 호출 방지) 🌟
    private bool isDying = false; 
    private SpriteRenderer[] srs; // 모든 스프라이트 렌더러를 저장할 배열

    protected override void Start()
    {
        base.Start();
        FindPlayerByTag();
        heartSpawnCoroutine = StartCoroutine(HeartSpawnLoop());
        // 🌟 Start에서 모든 스프라이트 렌더러를 미리 찾아둡니다.
        srs = GetComponentsInChildren<SpriteRenderer>(); 
    }

    private void Update()
    {
        // 🌟 죽음 상태에서는 모든 움직임을 멈춥니다. 🌟
        if (isDying) return; 
        
        if (!target)
            FindPlayerByTag();

        if (!OnSkill && !IsPlayerInputLocked)
            MoveToTarget();

        CheckForDeath(); 
        
        // 🚨 테스트 목적으로만 P 키를 사용하고, 죽음 확인 로직을 여기에 추가
        if (Input.GetKeyDown(KeyCode.P))
        {
            Hp -= MaxHp;
        }
    }

    // ----------------------------------------------------
    // ⚔️ HP가 0 이하인지 확인하고 사망 처리하는 핵심 함수 추가
    // ----------------------------------------------------
    public void CheckForDeath()
    {
        if (isDying || Hp > 0) return; 

        isDying = true; // 사망 처리 시작 플래그 설정
        
        OnBossDie();
    }

    void MoveToTarget()
    {
        if (target == null) return;

        float targetX = target.position.x;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void FindPlayerByTag()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("⚠️ Player not found. Check the Player tag!");
    }

    private IEnumerator HeartSpawnLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => !IsPlayerInputLocked);
            yield return new WaitForSeconds(spawnInterval);

            if (IsPlayerInputLocked) continue;

            if (heartPrefab == null)
            {
                Debug.LogWarning("Heart Prefab is not assigned!");
                continue;
            }

            float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
            float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
            Vector3 spawnPosition = new Vector3(randomX, randomY, heartZ);

            Instantiate(heartPrefab, spawnPosition, Quaternion.identity);
        }
    }

    protected override void OnPhaseTwoStart()
    {
        base.OnPhaseTwoStart();

        Red_bird_FireHp[] existingHearts = FindObjectsByType<Red_bird_FireHp>(FindObjectsSortMode.None);

        if (existingHearts.Length > 0)
        {
            foreach (Red_bird_FireHp heart in existingHearts)
                Destroy(heart.gameObject);

            Debug.Log($"2페이즈 진입: 기존 하트 {existingHearts.Length}개 제거됨");
        }
    }

    // ----------------------------------------------------
    // 👻 보스 스프라이트를 투명하게 만드는 페이드 아웃 코루틴 추가
    // ----------------------------------------------------
    private IEnumerator FadeOutBoss()
    {
        float timer = 0f;
        
        // 보스의 모든 자식 스프라이트의 알파값을 1.0f에서 0f로 변경
        while (timer < bossFadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / bossFadeDuration);

            foreach (var sr in srs)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
            yield return null;
        }
        
        // 완전히 사라진 후 오브젝트 비활성화 (선택적)
        // gameObject.SetActive(false);
    }

    //--------------------------------------------
    // 🚨 보스 사망 → 씬 페이드 전환 로직 (수정됨)
    //--------------------------------------------
    public void OnBossDie()
    {
        // 1. 보스 고유 로직 중지 (하트 스폰)
        if (heartSpawnCoroutine != null)
            StopCoroutine(heartSpawnCoroutine);

        // 2. 보스 스프라이트 페이드 아웃 시작
        StartCoroutine(DieAndTransitionRoutine());
    }
    
    //--------------------------------------------
    // 🔗 보스 페이드 아웃 완료 후 씬 전환을 시작하는 코루틴
    //--------------------------------------------
    private IEnumerator DieAndTransitionRoutine()
    {
        // 보스 스프라이트 투명화 코루틴 시작
        yield return StartCoroutine(FadeOutBoss());

        Debug.Log("✅ 보스 스프라이트 페이드 아웃 완료. 씬 전환 시작.");

        // 씬 페이드 전환 시작 (화면 전체를 검게 만듦)
        if (SceneFader.Instance != null)
        {
            // SceneFader의 fadeDuration에 따라 화면 전체가 검게 변하며 씬 전환 시작
            SceneFader.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogError("SceneFader Instance를 찾을 수 없습니다. 즉시 씬 전환합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float centerX = (spawnRangeX.x + spawnRangeX.y) / 2f;
        float centerY = (spawnRangeY.x + spawnRangeY.y) / 2f;
        float sizeX = spawnRangeX.y - spawnRangeX.x;
        float sizeY = spawnRangeY.y - spawnRangeY.x;

        Vector3 center = new Vector3(centerX, centerY, heartZ);
        Vector3 size = new Vector3(sizeX, sizeY, 0.1f);

        Gizmos.DrawWireCube(center, size);
    }
}