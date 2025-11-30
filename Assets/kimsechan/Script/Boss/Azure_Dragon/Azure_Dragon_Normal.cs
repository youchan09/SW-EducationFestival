using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; 

public class Azure_Dragon_Normal : Skill_based
{
    [Header("Boss Reference")]
    public BossManager bossManager; 
    
    [Header("Lightning Settings")]
    [Tooltip("BossManager.bossSkills 리스트에서 번개 스킬의 인덱스 (0으로 가정)")]
    public int lightningSkillIndex = 0; 
    
    [Header("Warning Settings")]
    [Tooltip("BossManager.bossSkills 리스트에서 예고 원 스킬의 인덱스 (1로 가정)")]
    public int warningMarkIndex = 1;
    [Tooltip("예고 마커가 표시되는 시간 (번개 스폰 전 대기 시간)")]
    public float warningDuration = 0.5f; // 원래 값으로 복원
    
    // 👇 빨간 원 크기 조절을 위한 변수
    [Tooltip("예고 마크(빨간 원)의 스케일 비율 (기본 1.0, 0.5로 줄이면 절반 크기)")]
    public float warningMarkScale = 2.0f; 

    // --- 6방향 피자컷 확산(Radial Spread) 설정 ---
    [Header("Radial Spread Settings (6-Way Pizza Cut)")]
    [Tooltip("번개가 퍼져나가는 총 단계 (링의 개수). 8을 기본으로 사용.")]
    public int spreadStages = 8;
    // --- 거리 간격 설정 (Inspector에서 조절 가능) ---
    [Tooltip("중심(플레이어 위치)에서 첫 번째 링까지의 반지름/거리 (최소 시작 거리). 2.0으로 조정.")]
    public float initialRadius = 2.0f; 
    [Tooltip("링이 퍼질 때마다 반지름이 증가하는 정도 (링 간의 거리). 2.0으로 조정.")]
    public float radiusIncrementPerStage = 2.0f;
    // ------------------------------------
    [Tooltip("한 링의 6방향 번개 타격 후 다음 링까지의 대기 시간 (순차적/혼란 패턴 딜레이)")]
    public float delayBetweenStages = 0.2f; // 원래 값으로 복원
    
    // --- 요청: 전체 패턴 반복 횟수 추가 및 타이밍 제어 ---
    [Header("Pattern Repetition")]
    [Tooltip("전체 방사형 패턴을 반복할 횟수 (기존 1회에서 5회로 변경).")]
    public int totalPatternRepeats = 5; 

    [Header("Pattern Timing (Repeat)")]
    [Tooltip("전체 6방향 확산 패턴이 반복되어 새로 시작되는 주기. (요청: 0.5초마다 새 패턴 시작)")]
    public float patternRepeatInterval = 0.5f; 
    // ------------------------------------
    
    // 👇 번개 Y 오프셋 (Inspector에서 조절 가능)
    [Tooltip("번개가 스폰될 플레이어 위치 기준 Y 오프셋")]
    public float lightningYOffset = 3f; 
    [Tooltip("번개가 활성화된 후 자동으로 비활성화되는 시간")]
    public float lightningActiveDuration = 0.3f; 
    
    // 👇 추가: 빨간 원 비활성화 딜레이 (번개 애니메이션 지속 시간에 맞춤)
    [Header("Warning Mark Deactivation")]
    [Tooltip("번개가 스폰된 후, 해당 위치의 경고 마크가 사라질 때까지의 지연 시간 (번개 애니메이션 지속 시간).")]
    public float warningMarkDeactivationDelay = 0.27f;

    private Coroutine currentAttackCoroutine;
    private Transform playerTarget; 
    private Vector3 initialPlayerPosition; // 패턴 시작 시 플레이어 위치 저장

    private void Awake()
    {
        if (bossManager == null)
        {
            bossManager = GetComponentInParent<BossManager>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;
        else
            Debug.LogError("Player 오브젝트(태그: Player)를 찾을 수 없습니다! 번개 스킬이 정상 작동하지 않습니다.");
    }

    public override void Attack()
    {
        if (bossManager != null && playerTarget != null)
        {
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }
            // 패턴 반복을 시작하는 런처 코루틴 시작
            currentAttackCoroutine = StartCoroutine(LightningAttackSequence());
        }
        else
        {
            Debug.LogError("Azure_Dragon_Normal: BossManager 또는 PlayerTarget이 할당되지 않았습니다!");
        }
    }
    
    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        if (bossManager != null)
        {
            bossManager.OnSkill = false; 
            
            Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
            if (turnChange != null)
            {
                turnChange.isAttacking = false;
            }
        }
        
        Debug.Log("Azure_Dragon_Normal: Attack forcefully stopped by StopAttack.");
    }

    // 메인 코루틴: 반복 실행 간격을 제어하는 런처 역할
    private IEnumerator LightningAttackSequence()
    {
        bossManager.OnSkill = true; // 스킬 발동 시작
        
        // --- 전체 패턴 5회 반복 루프 ---
        for (int repeat = 0; repeat < totalPatternRepeats; repeat++)
        {
            // 각 패턴은 플레이어의 현재 위치를 고정합니다.
            if (playerTarget != null)
            {
                initialPlayerPosition = playerTarget.position;
                initialPlayerPosition.z = 0f;
            }
            else
            {
                Debug.LogError("PlayerTarget is missing during attack sequence.");
                break; // 플레이어가 없으면 반복 중단
            }

            // 개별 패턴 실행을 비동기(Coroutine)로 시작합니다. (이전 패턴이 끝나기 전에 다음 패턴 시작)
            StartCoroutine(ExecuteSingleRadialAttack(initialPlayerPosition));
            
            // 다음 패턴이 시작될 때까지 patternRepeatInterval(0.5초)만큼 대기합니다.
            yield return new WaitForSeconds(patternRepeatInterval);
        }
        // --- 전체 패턴 5회 반복 루프 끝 ---

        // 모든 패턴이 시작된 후, 가장 긴 패턴의 완료를 위해 충분히 대기
        // (원래 패턴 실행 시간은 약 2.47초이므로 3초 대기)
        yield return new WaitForSeconds(3.0f); 

        // 모든 공격이 끝난 후 상태 정리
        bossManager.OnSkill = false;

        Turn_Change turn_Change = bossManager.GetComponent<Turn_Change>();
        if (turn_Change != null)
            turn_Change.isAttacking = false;
            
        currentAttackCoroutine = null;
    }
    
    // 개별 패턴 실행 로직 (마크 생성, 타격, 정리)
    private IEnumerator ExecuteSingleRadialAttack(Vector3 centerPosition)
    {
        // 6방향 (피자 6등분) 벡터 계산
        float angleIncrement = 360f / 6f; // 60도 간격
        Vector2[] directions = new Vector2[6];
        for (int j = 0; j < 6; j++)
        {
            float angle = j * angleIncrement; 
            float radians = angle * Mathf.Deg2Rad;
            // Cos(angle), Sin(angle)을 사용하여 6개의 정규화된 방향 벡터를 생성
            directions[j] = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
        }
        
        // ----------------------------------------------------
        // Phase 1: 모든 경고 마크를 한 번에 생성
        // ----------------------------------------------------
        
        // 경고 마크들을 스테이지(링)별로 분리하여 저장할 리스트
        List<List<GameObject>> warningMarksPerStage = new List<List<GameObject>>();
        
        for (int i = 0; i < spreadStages; i++) 
        {
            // 현재 링에 대한 경고 마크 리스트 생성
            List<GameObject> currentStageMarks = new List<GameObject>();
            float currentRadius = initialRadius + (i * radiusIncrementPerStage);

            foreach (Vector2 dir in directions)
            {
                Vector3 spawnPosition = centerPosition + (Vector3)(dir * currentRadius);
                
                // 예고 원은 타격 위치에 표시
                GameObject warningMark = bossManager.UseSkill(
                    warningMarkIndex, 
                    new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), 
                    Quaternion.identity
                );

                if (warningMark != null)
                {
                    warningMark.transform.localScale = Vector3.one * warningMarkScale; 
                    currentStageMarks.Add(warningMark);
                }
            }
            // 현재 링의 경고 마크 리스트를 메인 리스트에 추가
            warningMarksPerStage.Add(currentStageMarks);
        }

        // 모든 경고 마크 생성 후 warningDuration만큼 대기 (0.5초)
        yield return new WaitForSeconds(warningDuration);
        
        // 중단 체크
        if (bossManager.Hp <= 0)
        {
            // 모든 경고 마크 비활성화
            foreach (var stageMarks in warningMarksPerStage)
            {
                foreach (GameObject wm in stageMarks)
                {
                    if (wm != null && wm.activeSelf) wm.SetActive(false);
                }
            }
            yield break;
        }
        
        // ----------------------------------------------------
        // Phase 2: 번개 타격 및 지연된 경고 마크 제거
        // ----------------------------------------------------
        
        // spreadStages 횟수만큼 (링 개수만큼) 순차적으로 타격
        for (int i = 0; i < spreadStages; i++)
        {
            // 중단 체크
            if (bossManager.Hp <= 0)
            {
                // 아직 비활성화되지 않은 경고 마크들을 모두 비활성화합니다.
                for (int j = i; j < warningMarksPerStage.Count; j++)
                {
                    foreach (GameObject wm in warningMarksPerStage[j])
                    {
                        if (wm != null && wm.activeSelf) wm.SetActive(false);
                    }
                }
                yield break;
            }

            float currentRadius = initialRadius + (i * radiusIncrementPerStage);
            
            // 6방향에 동시에 번개 스폰 (각 링별 타격)
            foreach (Vector2 dir in directions)
            {
                Vector3 spawnPosition = centerPosition + (Vector3)(dir * currentRadius);
                
                // 번개는 Y 오프셋을 적용하여 상공에서 떨어지는 것처럼 보이게 스폰
                Vector3 lightningSpawnPosition = spawnPosition;
                lightningSpawnPosition.y += lightningYOffset; 

                GameObject lightning = bossManager.UseSkill(
                    lightningSkillIndex, 
                    lightningSpawnPosition, 
                    Quaternion.identity
                );
                
                // 번개 이펙트가 짧게 지속되도록 설정
                if (lightning != null)
                {
                    StartCoroutine(DeactivateAfterDelay(lightning, lightningActiveDuration));
                }
            }
            
            // 해당 링(Stage)의 번개 타격 후, 그 링에 해당하는 경고 마크들을 0.27초 후에 비활성화합니다.
            if (i < warningMarksPerStage.Count)
            {
                foreach (GameObject wm in warningMarksPerStage[i])
                {
                    // 경고 마크 비활성화를 코루틴으로 실행하여 딜레이 적용
                    StartCoroutine(DeactivateAfterDelay(wm, warningMarkDeactivationDelay));
                }
            }
            
            // 다음 링까지 delayBetweenStages만큼 대기 (순차 패턴 느낌)
            yield return new WaitForSeconds(delayBetweenStages);
        }
        
        // ----------------------------------------------------
        // Phase 3: 정리 
        // ----------------------------------------------------
        
        // 마지막 링의 경고 마크 비활성화 코루틴이 완료될 때까지 대기
        yield return new WaitForSeconds(warningMarkDeactivationDelay + 0.1f);
    }
    
    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.activeSelf)
        {
            obj.SetActive(false);
        }
    }
}