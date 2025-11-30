using System;
using System.Collections; // Coroutine 사용을 위해 추가
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening; 
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object; // 씬 전환을 위해 추가

public class Azure_Dragon : BossManager
{
    // ✨ 씬 전환을 위한 변수 (Inspector에서 설정)
    [Header("Scene Transition")]
    [Tooltip("보스 처치 후 로드할 씬의 이름")]
    public string nextSceneName = "GameClearScene"; // 기본값 설정 (Inspector에서 수정 가능)

    // ✨ 2페이즈 애니메이션 설정
    [Header("Azure Dragon Phase Transitions")]
    [Tooltip("2페이즈 진입 시 보스가 고정적으로 이동할 Y 좌표 (X=0, Y=50.0)")]
    private const float PHASE_TWO_FIXED_Y = 50.0f; 
    private const float PHASE_TWO_FIXED_X = 0f;
    
    [Tooltip("2페이즈 이동 애니메이션 지속 시간")]
    public float animationDuration = 2.0f; // 2페이즈 이동 속도는 유지

    // 참고: 이전의 HP 10% 관련 상수와 로직(Hp setter, StartFinalPhaseSequence)은 모두 제거되었습니다.

    // 2D 충돌 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            // BossManager의 TakeDamage 메서드를 호출하여 HP 감소와 이펙트 처리를 일임
            TakeDamage(10);
        }
    }

    // 참고: Hp 프로퍼티는 기본 BossManager 로직을 사용합니다.

    /// <summary>
    /// BossManager의 OnPhaseTwoStart 메서드를 오버라이드합니다.
    /// 2페이즈 진입 시 청룡이 고정된 위치(0, 50)로 즉시 이동하며, 추가 애니메이션 로직을 제외합니다.
    /// </summary>
    protected override void OnPhaseTwoStart()
    {
        // Turn_Change 컴포넌트가 존재한다면 (위치 추적 금지)
        Turn_Change turnChange = GetComponent<Turn_Change>();
        if (turnChange != null)
        {
            // 위치 추적 비활성화 (보스쪽으로 따라가지 않게)
            turnChange.enabled = false;
        }

        // ✨ 청룡을 고정된 위치(0, 50)로 이동
        Vector3 targetPos = new Vector3(PHASE_TWO_FIXED_X, PHASE_TWO_FIXED_Y, transform.position.z);
        transform.DOMove(targetPos, animationDuration).SetEase(Ease.OutSine);
    }
    
    /// <summary>
    /// **BossManager의 PhaseTwoCinematic을 new로 재정의하여 보스 흔들림 및 카메라 로직을 제거하고,
    /// 2페이즈 진입 후 패턴 시작만 처리합니다.**
    /// </summary>
    protected new IEnumerator PhaseTwoCinematic(bool isDying) 
    {
        // 사망 시퀀스(isDying=true)는 StartDeathSequence에서 즉시 씬 전환으로 대체됩니다.
        if (isDying)
        {
            yield break;
        }

        // 2페이즈 진입 시 (isDying=false)
        if (skillCoolCoroutine != null)
            StopCoroutine(skillCoolCoroutine);
        
        Turn_Change turnChange = GetComponent<Turn_Change>(); 

        if (turnChange != null)
            turnChange.StopAttackLoop();

        foreach (var skill in bossSkills)
            skill.DeactivateAllObjects();

        OnSkill = true; 
        IsPlayerInputLocked = true; 
        
        // ✨ 카메라 이동 및 보스 흔들림 로직을 모두 제거하고 잠시 대기
        yield return new WaitForSeconds(1.0f); // 약간의 딜레이만 줍니다.

        OnSkill = false;
        IsPlayerInputLocked = false;

        skillCoolCoroutine = StartCoroutine(SkillCool()); 
        
        if (turnChange != null)
        {
            // Turn_Change가 OnPhaseTwoStart에서 비활성화되었을 수 있으므로 다시 활성화
            turnChange.enabled = true; 
            turnChange.StartPhaseTwoLoop(); 
        }
    }


    /// <summary>
    /// BossManager의 StartDeathSequence 메서드를 재정의하여 사망 시 즉시 씬 전환을 실행합니다.
    /// 보스 오브젝트(gameObject)를 제외한 모든 DontDestroyOnLoad 오브젝트와 "Player" 태그 오브젝트를 파괴합니다.
    /// </summary>
    public new void StartDeathSequence()
    {
        Debug.Log($"보스 처치 완료! 씬 '{nextSceneName}' 로 전환을 시도합니다.");

        // 1. 모든 패턴 및 코루틴 중지
        StopAllCoroutines(); 
        
        // 2. 턴 체인지(위치 추적) 비활성화
        Turn_Change turnChange = GetComponent<Turn_Change>();
        if (turnChange != null)
        {
            turnChange.StopAttackLoop();
            turnChange.enabled = false;
        }
        
        // 3. DO Tween 애니메이션 중지
        transform.DOKill();

        // 4. ✨ DontDestroyOnLoad 및 "Player" 오브젝트 파괴 로직 ✨

        // 4a. "Player" 태그를 가진 오브젝트를 찾아서 파괴합니다.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                Debug.Log($"플레이어 오브젝트 파괴 (Tag: Player): {player.name}");
                Destroy(player);
            }
        }
        GameObject[] fade_outs = GameObject.FindGameObjectsWithTag("fade_out");
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                Debug.Log($"fade_out 오브젝트 파괴: {fade_outs}");
                Destroy(fade_outs[0]);
            }
        }
        // 4b. "Player" 태그 외의 나머지 DontDestroyOnLoad 오브젝트를 파괴합니다. (보스 오브젝트 제외)
        // DontDestroyOnLoad 씬의 오브젝트들은 obj.scene.name이 "" 입니다.
        GameObject[] allActiveObjects = FindObjectsOfType<GameObject>();
        
        for (int i = 0; i < allActiveObjects.Length; i++)
        {
            GameObject obj = allActiveObjects[i];
            
            // 오브젝트가 DontDestroyOnLoad 씬에 있고, 현재 보스 오브젝트가 아닐 때 파괴합니다.
            if (obj != null && obj != gameObject && obj.scene.name == "")
            {
                Debug.Log($"DontDestroyOnLoad 오브젝트 파괴: {obj.name}");
                Destroy(obj);
            }
        }
        
        // 참고: 현재 씬에 있는 다른 오브젝트들은 SceneManager.LoadScene이 호출될 때 자동으로 파괴됩니다.
        // 보스 오브젝트(gameObject)는 여기서 파괴하지 않으며, 씬 로드에 따라 파괴되거나 유지됩니다.

        // 5. 씬 전환 실행
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("다음 씬 이름(nextSceneName)이 설정되지 않았습니다. Inspector를 확인해주세요.");
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate에서 Hp == 0일 때 사망 시퀀스를 시작합니다.
        if (Hp <= 0) 
        {
             StartDeathSequence();
             // Hp가 0이 된 직후에 StartDeathSequence를 한 번만 호출하도록 막기 위해 스크립트를 비활성화합니다.
             enabled = false; 
        }
    }
}