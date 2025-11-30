using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 
using DG.Tweening; 

public class Tire : BossManager
{
    [Header("타겟 설정")]
    public Transform playerTarget;

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("보스 구성 오브젝트")]
    // 0: 아래(정면), 1: 위(뒷면), 2: 좌우
    public GameObject frontPart;
    public GameObject backPart;
    public GameObject sidePart;

    [Header("Animation 설정")]
    public Sprite frontA, frontB;
    public Sprite backA, backB;
    public Sprite sideA, sideB;

    [Header("Animation 속도")]
    public float interval = 0.15f;
    
    // 🌟 추가된 변수 🌟
    [Header("Die & Scene Transition")]
    public string nextSceneName = "GameClearScene";
    private bool isDying = false; // 사망 처리 중복 호출 방지 플래그
    // 🌟 ----------------- 🌟

    private SpriteRenderer frontSR;
    private SpriteRenderer backSR;
    private SpriteRenderer sideSR;

    private float timerFront = 0f;
    private float timerBack  = 0f;
    private float timerSide  = 0f;

    private float originalZ;
    private const float LOWER_Z = -4.1f;
    
    private bool canMove = false;
    private bool isFadedOut = false; // 50% HP 페이드 아웃 상태 추적

    protected override void Start()
    {
        base.Start();

        if (frontPart == null || backPart == null || sidePart == null)
        {
            Debug.LogError("보스 파트를 모두 연결해주세요.");
            return;
        }

        frontSR = frontPart.GetComponent<SpriteRenderer>();
        backSR  = backPart.GetComponent<SpriteRenderer>();
        sideSR  = sidePart.GetComponent<SpriteRenderer>();

        originalZ = transform.position.z;

        ActivateFront();
    }

    void Update()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p) playerTarget = p.transform;
        }
        // 🌟 사망 상태라면 모든 로직을 중지합니다.
        if (isDying) return; 
        
        // 🌟 매 프레임 사망 여부를 확인합니다.
        CheckForDeath(); 
        
        if (playerTarget == null) return;

        // ★ HP 절반 체크 → 페이드아웃 (Phase 2 visual state)
        if (!isFadedOut && Hp <= MaxHp * 0.5f)
        {
            StartCoroutine(FadeOutAndDisable());
            isFadedOut = true;
        }

        if (isEncounterStarted && !canMove)
            StartCoroutine(Delay());

        // canMove이 true이고 페이드아웃 상태가 아닐 때만 이동/애니메이션 실행
        if (isEncounterStarted && canMove && !isFadedOut)
        {
            MoveTowardsPlayer();
            AnimateSprites();
        }

        if (Input.GetKeyDown(KeyCode.O)) Hp -= MaxHp / 2;
        if(Input.GetKeyDown(KeyCode.P)) Hp -= MaxHp;
    }

    // ----------------------------------------------------
    // ⚔️ HP가 0 이하인지 확인하고 사망 처리하는 핵심 함수
    // ----------------------------------------------------
    public void CheckForDeath()
    {
        if (isDying || Hp > 0) return; 

        isDying = true; // 사망 처리 시작 플래그 설정
        OnBossDie();
    }

    //--------------------------------------------
    // 🚨 보스 사망 → 씬 전환 메인 함수
    //--------------------------------------------
    public void OnBossDie()
    {
        // 1. 보스의 모든 이동 및 애니메이션 중지
        canMove = false; 

        // 2. 만약 50% 페이드아웃이 아직 실행되지 않았다면, 여기서 강제로 시작합니다.
        if (!isFadedOut)
        {
            // DieAndTransitionRoutine에서 FadeOutAndDisable을 호출하므로, 여기서 직접 호출할 필요는 없습니다.
            // isFadedOut = true; // 플래그만 설정
        }

        // 3. 씬 전환 코루틴을 시작하여 화면 전체 페이드 아웃 및 씬 전환을 수행합니다.
        StartCoroutine(DieAndTransitionRoutine());
    }
    
    //--------------------------------------------
    // 🔄 씬 전환을 관리하는 코루틴 (화면 페이드 아웃 담당)
    //--------------------------------------------
    private IEnumerator DieAndTransitionRoutine()
    {
        // 1. 보스 스프라이트의 페이드 아웃이 완료되도록 기다립니다. 
        yield return StartCoroutine(FadeOutAndDisable());

        // 2. SceneFader 호출 시도 (화면 전체 페이드 아웃)
        if (SceneFader.Instance != null)
        {
            Debug.Log("✅ 보스 사망! SceneFader를 통해 화면 전체 페이드 아웃 후 씬 전환을 시작합니다.");
            SceneFader.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            // SceneFader가 없으면 즉시 씬 전환합니다.
            Debug.LogError("SceneFader Instance를 찾을 수 없습니다. 즉시 씬 전환합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    //--------------------------------------------
    // 👻 보스 스프라이트의 투명화 (50% HP 또는 사망 시) - 태그 기반 로직 추가
    //--------------------------------------------
    IEnumerator FadeOutAndDisable()
    {
        float duration = 1.0f;

        // 🌟 변경된 로직: 모든 SpriteRenderer와 "Fade_Out" 태그를 가진 오브젝트의 SpriteRenderer를 가져옵니다.
        
        // 보스 오브젝트의 모든 SpriteRenderer를 찾습니다.
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        
        // 씬에서 "Fade_Out" 태그를 가진 오브젝트를 찾습니다.
        // 이 보스의 자식이 아닌 다른 오브젝트의 페이드 아웃이 필요하다면 사용됩니다.
        // GameObject[] fadeOutObjects = GameObject.FindGameObjectsWithTag("Fade_Out");
        
        // 여기서는 보스의 자식 중에서만 처리하는 것이 일반적이므로, GetComponentsInChildren만 사용하거나, 
        // 만약 'Fade_Out' 태그를 가진 자식만 필요하다면 별도의 로직이 필요합니다. 
        // 현재는 'GetComponentsInChildren'으로 보스 전체를 페이드 아웃시키겠습니다.
        // 만약 'Fade_Out' 태그가 붙은 부품만 페이드 아웃시키고 싶다면, 아래와 같이 변경해야 합니다.
        
        // --- 태그 기반으로만 페이드 아웃할 경우 (선택적) ---
        /*
        List<SpriteRenderer> targetSRs = new List<SpriteRenderer>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Fade_Out"))
            {
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) targetSRs.Add(sr);
            }
        }
        SpriteRenderer[] srsToFade = targetSRs.ToArray();
        */
        // ----------------------------------------------------

        // 알파 1 → 0
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);

            foreach (var sr in srs) // 현재는 모든 자식 SpriteRenderer를 사용
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

        // 완전히 숨기기
        frontPart.SetActive(false);
        backPart.SetActive(false);
        sidePart.SetActive(false);

        canMove = false; // 더 이상 이동 X
    }


    // ------------------ 이동 로직 -------------------
    private void MoveTowardsPlayer()
    {
        if (playerTarget == null) return;
        
        Vector3 dir = playerTarget.position - transform.position;
        float step = moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        // 대각선 이동 제거: X 또는 Y 중 큰 방향으로만 이동
        if (absX >= absY)
        {
            // 좌우 이동
            newPos.x += Mathf.Sign(dir.x) * step;
            ActivateSide(Mathf.Sign(dir.x));
        }
        else
        {
            // 상하 이동
            newPos.y += Mathf.Sign(dir.y) * step;
            if (Mathf.Sign(dir.y) > 0)
                ActivateBack();
            else
                ActivateFront();
        }

        // Z 깊이 업데이트
        Vector3 pos = newPos;
        pos.z = (newPos.y < playerTarget.position.y) ? LOWER_Z : originalZ;
        transform.position = pos;
    }

    // ------------------ 스프라이트 활성화 -------------------
    void ActivateFront()
    {
        frontPart.SetActive(true);
        backPart.SetActive(false);
        sidePart.SetActive(false);
    }

    void ActivateBack()
    {
        frontPart.SetActive(false);
        backPart.SetActive(true);
        sidePart.SetActive(false);
    }

    void ActivateSide(float dirX)
    {
        frontPart.SetActive(false);
        backPart.SetActive(false);
        sidePart.SetActive(true);

        Vector3 scale = sidePart.transform.localScale;
        sidePart.transform.localScale = new Vector3(Mathf.Abs(scale.x) * Mathf.Sign(dirX), scale.y, scale.z);
    }

    // ------------------ 스프라이트 깜빡임 -------------------
    void AnimateSprites()
    {
        if (frontPart.activeSelf)
        {
            timerFront += Time.deltaTime;
            if (timerFront >= interval)
            {
                frontSR.sprite = (frontSR.sprite == frontA) ? frontB : frontA;
                timerFront = 0f;
            }
        }

        if (backPart.activeSelf)
        {
            timerBack += Time.deltaTime;
            if (timerBack >= interval)
            {
                backSR.sprite = (backSR.sprite == backA) ? backB : backA;
                timerBack = 0f;
            }
        }

        if (sidePart.activeSelf)
        {
            timerSide += Time.deltaTime;
            if (timerSide >= interval)
            {
                sideSR.sprite = (sideSR.sprite == sideA) ? sideB : sideA;
                timerSide = 0f;
            }
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3f);
        canMove = true;
    }
}