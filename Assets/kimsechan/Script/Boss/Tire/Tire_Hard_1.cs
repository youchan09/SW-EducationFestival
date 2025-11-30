using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Random = UnityEngine.Random; // DOTween 사용

public class Tire_Hard_1 : Skill_based
{
    // === 맵 및 경로 설정 ===
    private const float MAP_LEFT_X = -33.0f;
    private const float MAP_RIGHT_X = 33.0f;
    private const float EXIT_STOP_X_LEFT = -46.0f;
    private const float EXIT_STOP_X_RIGHT = 46.0f;
    
    // === 설정 변수 ===
    [Header("Dependencies")]
    public BossManager bossManager;
    
    [Header("Tire Settings")]
    [Tooltip("패턴을 사용할 오브젝트의 인덱스 (BossManager의 SkillList 참고)")]
    public int skillIndex = 2;
    
    [Header("Attack Settings")]
    [Tooltip("연속 돌진 횟수 (마지막 돌진 제외)")]
    public int consecutiveDashes = 3;
    
    [Tooltip("마지막 돌진 거리 비율 (0~1)")]
    public float finalDashDistanceMultiplier = 0.5f;
    
    [Tooltip("돌진 시간 (초)")]
    public float chargeDuration = 0.75f; 
    
    [Tooltip("연속 돌진 시작 간격 (초)")]
    public float dashInterval = 1.5f; // 이 값은 사용되지 않음
    
    [Tooltip("최종 돌진 멈춤 시간")]
    public float finalDashDelay = 1.0f; // 이 값은 이제 최종 돌진 로직이 제거되어 사용되지 않습니다.
    
    [Tooltip("맵 밖으로 퇴장할 때 이동 거리")]
    public float exitDistance = 30f;
    
    [Tooltip("퇴장 이동 시간")]
    public float exitDuration = 0.5f; // 퇴장 시간을 0.5초로 단축했습니다.
    
    [Tooltip("공격 오브젝트 크기 배율")]
    public float attackObjectScale = 2.0f;
    
    [Header("Warning Settings")]
    [Tooltip("위험 표시 인덱스")]
    public int warningIndex = 5;
    
    [Tooltip("경고 표시 유지 시간")]
    public float warningDuration = 0.5f; // 경고 시간을 0.5초로 단축했습니다.
    
    private Transform playerTarget;
    private Coroutine currentCoroutine;

    private void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;

        // BossManager 찾기
        if (bossManager == null)
        {
            bossManager = GetComponentInParent<BossManager>();
            if (bossManager == null)
                bossManager = FindObjectOfType<BossManager>();
        }
    }

    public override void Attack()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(TireHorizontalAttackPattern());
    }

    public override void StopAttack()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }
    
    // 연속 돌진만 반복하는 패턴
    private IEnumerator TireHorizontalAttackPattern()
    {
        // consecutiveDashes 횟수만큼 일반 돌진을 반복합니다.
        for (int i = 0; i < consecutiveDashes; i++)
        {
            // 돌진 시작 (isFinalDash = false, full distance)
            yield return StartCoroutine(TireHorizontalAttackSequence(false, 1f));
        }

        // ⭐️ 제거됨: 별도의 최종 돌진 호출 로직을 제거했습니다.
        // yield return StartCoroutine(TireHorizontalAttackSequence(true, finalDashDistanceMultiplier));
        
        currentCoroutine = null;
    }
    
    // 단일 돌진 시퀀스
    private IEnumerator TireHorizontalAttackSequence(bool isFinalDash, float distanceMultiplier = 1f)
    {
        if (playerTarget == null || bossManager == null)
            yield break;

        Vector3 playerPos = playerTarget.position;
        playerPos.z = 0;

        Vector3 tireSpawnPos = Vector3.zero;
        Vector3 warningSpawnPos = Vector3.zero;
        Quaternion warningRotation = Quaternion.identity;
        
        Vector3 finalAttackTargetPos = Vector3.zero;
        float targetScaleX = 1f;
        
        Vector3 stopPos = Vector3.zero;

        // 공격 방향 결정
        int attackDirection = Random.Range(0, 2); // 0: 우->좌, 1: 좌->우

        if (attackDirection == 0)
        {
            tireSpawnPos.y = playerPos.y;
            tireSpawnPos.x = MAP_RIGHT_X;
            finalAttackTargetPos = new Vector3(MAP_LEFT_X, playerPos.y, 0);
            warningSpawnPos = new Vector3(0, playerPos.y, 0);
            targetScaleX = -1f;
            stopPos = new Vector3(EXIT_STOP_X_LEFT, playerPos.y, 0);
        }
        else
        {
            tireSpawnPos.y = playerPos.y;
            tireSpawnPos.x = MAP_LEFT_X;
            finalAttackTargetPos = new Vector3(MAP_RIGHT_X, playerPos.y, 0);
            warningSpawnPos = new Vector3(0, playerPos.y, 0);
            targetScaleX = 1f;
            stopPos = new Vector3(EXIT_STOP_X_RIGHT, playerPos.y, 0);
        }

        // 위험 표시
        GameObject warning = bossManager.UseSkill(warningIndex, warningSpawnPos, warningRotation);
        if (warning != null)
        {
            float mapWidth = MAP_RIGHT_X - MAP_LEFT_X;
            Vector3 warningScale = warning.transform.localScale;
            warning.transform.localScale = new Vector3(mapWidth, warningScale.y, warningScale.z);
        }

        yield return new WaitForSeconds(warningDuration); 

        if (warning != null) warning.SetActive(false);

        // 공격 오브젝트 소환
        GameObject tire = bossManager.UseSkill(skillIndex, tireSpawnPos, Quaternion.identity);

        if (tire != null)
        {
            tire.transform.position = tireSpawnPos;
            tire.transform.DOKill();

            Vector3 currentScale = tire.transform.localScale;
            currentScale.x *= attackObjectScale;
            currentScale.y *= attackObjectScale;
            tire.transform.localScale = new Vector3(targetScaleX * Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

            // 돌진 거리 조정 (isFinalDash가 false이므로 항상 전체 거리를 돌진합니다.)
            Vector3 adjustedTargetPos = finalAttackTargetPos;
            if (isFinalDash) 
            {
                adjustedTargetPos = Vector3.Lerp(tireSpawnPos, finalAttackTargetPos, distanceMultiplier);
            }

            // 돌진 실행 및 완료 대기
            yield return tire.transform.DOMove(adjustedTargetPos, chargeDuration).SetEase(Ease.Linear).WaitForCompletion();
        }

        // 퇴장 및 최종 처리
        Vector3 exitDirection = (finalAttackTargetPos - tireSpawnPos).normalized;
        Vector3 exitPos = finalAttackTargetPos + exitDirection * exitDistance;

        if (tire != null)
        {
            Sequence sequence = DOTween.Sequence();

            // 1차 이동 시간 계산 (돌진 끝 -> 맵 밖 멈춤 지점)
            float distanceToStop = Vector3.Distance(finalAttackTargetPos, stopPos);
            float distanceToExit = Vector3.Distance(finalAttackTargetPos, exitPos);
            if (distanceToExit < 0.01f) distanceToExit = 0.01f;
            
            float moveDuration1 = exitDuration * (distanceToStop / distanceToExit); 

            // 1차 이동 (맵 밖 멈춤 지점까지)
            sequence.Append(tire.transform.DOMove(stopPos, moveDuration1).SetEase(Ease.Linear));

            if (isFinalDash) // isFinalDash가 false이므로 이 블록은 실행되지 않습니다.
            {
                // 최종 돌진 로직 (제거됨)
                sequence.AppendInterval(finalDashDelay); 
                SpriteRenderer sr = tire.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sequence.Append(sr.DOFade(0f, 0.5f).SetEase(Ease.Linear));
                else
                    sequence.AppendInterval(0.5f);
            }
            else
            {
                // 일반 돌진 퇴장: 2차 이동 (맵 밖 최종 퇴장 위치까지)
                float moveDuration2 = exitDuration - moveDuration1;
                if (moveDuration2 < 0) moveDuration2 = 0;
                sequence.Append(tire.transform.DOMove(exitPos, moveDuration2).SetEase(Ease.Linear));
            }

            yield return sequence.WaitForCompletion();
        }

        if (tire != null)
            tire.SetActive(false);
    }
}