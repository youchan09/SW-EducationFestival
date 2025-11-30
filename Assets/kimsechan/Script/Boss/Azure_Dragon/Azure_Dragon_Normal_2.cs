using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요
using System.Collections.Generic; // Skill_based에 필요할 수 있으므로 추가
using DG.Tweening; // DOTween 사용을 위해 추가

// Skill_based를 상속받으며, 추상 메서드를 모두 구현해야 함
public class Azure_Dragon_Normal_2 : Skill_based
{
    // BossManager 참조 (UseSkill 메서드를 호출하기 위해 필요)
    [Header("Dependencies")]
    public BossManager bossManager; // Inspector에서 할당 필요

    // 설정 변수
    [Header("Storm Attack Settings")]
    [Tooltip("예고 원 스킬 인덱스 (일반적으로 1)")]
    public int warningMarkIndex = 1;
    [Tooltip("폭풍 스킬 인덱스 (요청에 따라 2)")]
    public int stormSkillIndex = 2;
    [Tooltip("경고 마커 표시 후 폭풍 스킬 발동까지의 대기 시간 (1.0초)")]
    public float stormDelay = 1.0f;
    [Tooltip("경고 마커의 Y 오프셋 (요청에 따라 -1.8f였으나, 폭풍에만 적용하도록 변경. 현재는 0)")]
    public float warningYOffset = -1.8f; // 경고 마크의 Y 오프셋
    [Tooltip("폭풍 스킬이 소환될 때 플레이어 위치 기준 Y 오프셋 (위로 올림)")]
    public float stormYOffset = 3.0f; // 폭풍의 Y 오프셋 (위로 올리기 위해 3.0f로 설정)
    [Tooltip("경고 마크의 크기 조절 (선택 사항)")]
    public float warningMarkScale = 2.0f;

    [Header("DOTween Settings")]
    [Tooltip("DOTween 애니메이션의 총 지속 시간")]
    public float DotweenDuration = 0.5f;
    [Tooltip("애니메이션 시작 시 폭풍이 줄어들 최종 크기 비율 (원래 크기 1.0 대비)")]
    public float shrinkScale = 0.2f; // 작아지는 크기 비율 (최종 크기에 비례하여 시작)
    [Tooltip("DOTween 애니메이션 후 폭풍이 최종적으로 가질 크기 비율 (기본 1.0)")]
    public float finalScaleRatio = 0.7f; // 최종 크기 비율 (1.0보다 작게 설정하여 전체 크기를 줄임)

    private Transform playerTarget;
    private Coroutine currentAttackCoroutine;

    void Start()
    {
        // Start에서는 초기화만 담당합니다.
        
        // "Player" 태그를 가진 오브젝트를 찾아 플레이어 위치를 참조
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player object (tag: Player) not found!");
        }

        if (bossManager == null)
        {
            // 부모 또는 씬에서 BossManager를 찾으려는 시도
            bossManager = GetComponentInParent<BossManager>();
            if (bossManager == null)
            {
                // 부모에 없으면 씬 전체에서 검색
                bossManager = FindObjectOfType<BossManager>();
            }
            if (bossManager == null)
            {
                Debug.LogError("BossManager not found! Please assign it in the Inspector.");
            }
        }
    }

    // 추상 메서드 구현: 외부에서 공격 시작을 요청할 때 실행되는 부분
    public override void Attack()
    {
        if (bossManager == null || playerTarget == null)
        {
            Debug.LogError("Attack failed: BossManager or PlayerTarget is missing!");
            return;
        }

        // 기존 공격이 있다면 중지하고 새로 시작
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }

        // 폭풍 공격 시퀀스를 코루틴으로 시작
        currentAttackCoroutine = StartCoroutine(StormAttackSequence());
        
        // 필요한 경우, bossManager.OnSkill = true; 와 같은 상태 설정 로직을 추가합니다.
    }
    
    // 추상 메서드 구현: 외부에서 공격 중지를 요청할 때 실행되는 부분
    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        // 필요한 경우, bossManager.OnSkill = false; 와 같은 상태 정리 로직을 추가합니다.
        Debug.Log("Azure_Dragon_Normal_2: Storm Attack forcefully stopped by StopAttack.");
    }

    // 공격 로직을 담은 코루틴 (실제 소환 및 타이밍 제어)
    private IEnumerator StormAttackSequence()
    {
        if (playerTarget == null || bossManager == null) 
        {
            currentAttackCoroutine = null;
            yield break;
        }

        Vector3 targetPosition = playerTarget.position;
        targetPosition.z = 0f; // 2D 환경을 위해 Z축 고정

        // 1. 빨간 원(경고 마크) 소환 위치 계산: Y 오프셋 적용
        Vector3 warningSpawnPosition = new Vector3(
            targetPosition.x, 
            targetPosition.y + warningYOffset, 
            targetPosition.z
        );
        
        // 1-1. 빨간 원 소환 (Index 1)
        GameObject warningMark = bossManager.UseSkill(
            warningMarkIndex, 
            warningSpawnPosition, 
            Quaternion.identity
        );

        if (warningMark != null)
        {
            warningMark.transform.localScale = Vector3.one * warningMarkScale; 
        }

        // 2. 1초 대기
        yield return new WaitForSeconds(stormDelay); // 1.0초 딜레이

        // 3. 폭풍 스킬 소환 위치 계산: Y 오프셋(위로 올림) 적용
        Vector3 stormSpawnPosition = new Vector3(
            targetPosition.x, 
            targetPosition.y + stormYOffset, // 폭풍에 새로운 Y 오프셋 적용
            targetPosition.z
        );
        
        // 3-1. 폭풍 스킬 소환 (Index 2)
        GameObject storm = bossManager.UseSkill(
            stormSkillIndex, 
            stormSpawnPosition, 
            Quaternion.identity
        );

        // 3-2. DOTween 애니메이션 적용 (작아졌다 최종 크기로)
        if (storm != null)
        {
            // 최종 크기를 finalScaleRatio (0.7f)만큼 줄여서 미리 저장
            Vector3 finalScale = Vector3.one * finalScaleRatio; 
            
            // 1. 초기 크기를 최종 크기에 shrinkScale(0.2f)을 곱한 값으로 설정
            storm.transform.localScale = finalScale * shrinkScale;
            
            // 2. 최종 크기(finalScale)까지 DotweenDuration 동안 애니메이션 (튕기는 효과)
            storm.transform.DOScale(finalScale, DotweenDuration).SetEase(Ease.OutBack);
        }
        
        // 4. 빨간 원(경고 마크) 제거
        // 폭풍 스킬이 활성화되는 순간 경고 마크를 비활성화합니다.
        if (warningMark != null && warningMark.activeSelf)
        {
            warningMark.SetActive(false);
            // 풀링 시스템을 사용하는 경우: warningMark.GetComponent<ObjectPoolItem>().Release(); 와 같은 로직이 필요합니다.
        }

        currentAttackCoroutine = null; // 코루틴이 완료되었음을 표시
    }

    // Update는 이 스킬에서는 필요하지 않습니다.
    void Update()
    {
        
    }
}