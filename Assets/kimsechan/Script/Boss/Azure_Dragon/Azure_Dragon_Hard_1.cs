using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Azure_Dragon_Hard_1 : Skill_based
{
    // 맵 환경에 따라 이 값들을 조정해야 합니다.
    private const float MAP_LEFT_X = -33.0f;  // 맵의 왼쪽 경계 X 좌표
    private const float MAP_RIGHT_X = 33.0f; // 맵의 오른쪽 경계 X 좌표
    private const float HORIZONTAL_ATTACK_Y = 0.0f; // 좌우 공격 시 드래곤의 기본 Y 위치
    
    // 상/하 공격 경로
    private const float VERTICAL_SPAWN_Y_UP = 16.3f; // 위쪽 스폰 Y
    private const float VERTICAL_TARGET_Y_DOWN = -16.3f; // 아래쪽 목표 Y
    private const float VERTICAL_SPAWN_Y_DOWN = -16.3f; // 아래쪽 스폰 Y
    private const float VERTICAL_TARGET_Y_UP = 16.3f; // 위쪽 목표 Y

    // 퇴장 중 멈춤 경계선
    private const float EXIT_STOP_X_LEFT = -46.0f; // 좌측 퇴장 시 멈춤 X (우->좌 공격)
    private const float EXIT_STOP_X_RIGHT = 46.0f; // 우측 퇴장 시 멈춤 X (좌->우 공격)
    private const float EXIT_STOP_Y_DOWN = -35.0f; // 하방 퇴장 시 멈춤 Y (상->하 공격)
    private const float EXIT_STOP_Y_UP = 35.0f; // 상방 퇴장 시 멈춤 Y (하->상 공격)

    [Header("Dependencies")]
    public BossManager bossManager;

    [Header("Dragon Settings")]
    [Tooltip("드래곤 스킬 인덱스 (기본 3)")]
    public int dragonSkillIndex = 3;

    public float delay = 4f; // 멈춤 시간으로 재활용
    
    [Tooltip("드래곤 돌진 시간 (초)")]
    public float chargeDuration = 1.0f; 
    
    [Tooltip("맵 밖으로 퇴장할 때 이동 거리")]
    public float exitDistance = 30f; 

    [Tooltip("퇴장 이동 시간")]
    public float exitDuration = 1.5f; 
    
    [Header("Warning Settings")]
    [Tooltip("좌우 공격 시 위험 표시 인덱스 (요청: 5)")]
    public int horizontalWarningIndex = 5;
    
    [Tooltip("상하 공격 시 위험 표시 인덱스 (요청: 4)")]
    public int verticalWarningIndex = 4;
    
    private Transform playerTarget;
    private Coroutine currentCoroutine;
    

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;

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

        currentCoroutine = StartCoroutine(DragonSequence());
    }

    public override void StopAttack()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    private IEnumerator DragonSequence()
    {
        if (playerTarget == null || bossManager == null)
            yield break;

        Vector3 playerPos = playerTarget.position;
        playerPos.z = 0;

        Vector3 dragonSpawnPos = Vector3.zero;
        Vector3 warningSpawnPos = Vector3.zero;
        Quaternion dragonRotation = Quaternion.identity; 
        Quaternion warningRotation = Quaternion.identity; 
        int warningIndex = 0;
        
        Vector3 finalAttackTargetPos = Vector3.zero;
        float targetScaleX = 1f; 

        int attackDirection = Random.Range(0, 4);
        
        // 퇴장 중 멈춤을 위한 변수
        Vector3 stopPos = Vector3.zero;
        bool shouldStop = false;

        if (attackDirection == 0) // 오른쪽에서 나타나 왼쪽으로 공격 (우 -> 좌)
        {
            warningIndex = horizontalWarningIndex;
            warningRotation = Quaternion.identity; // 0도
            
            dragonSpawnPos.y = playerPos.y; 
            dragonSpawnPos.x = MAP_RIGHT_X;
            finalAttackTargetPos = new Vector3(MAP_LEFT_X, playerPos.y, 0);
            warningSpawnPos = new Vector3(0, playerPos.y, 0); 
            dragonRotation = Quaternion.identity; // 0도
            targetScaleX = 1f; 

            // 좌측 퇴장 시 멈춤
            stopPos = new Vector3(EXIT_STOP_X_LEFT, playerPos.y, 0);
            shouldStop = true;
        }
        else if (attackDirection == 1) // 왼쪽에서 나타나 오른쪽으로 공격 (좌 -> 우)
        {
            warningIndex = horizontalWarningIndex;
            warningRotation = Quaternion.identity; // 0도

            dragonSpawnPos.y = playerPos.y; 
            dragonSpawnPos.x = MAP_LEFT_X;
            finalAttackTargetPos = new Vector3(MAP_RIGHT_X, playerPos.y, 0);
            warningSpawnPos = new Vector3(0, playerPos.y, 0); 
            dragonRotation = Quaternion.identity;  // 0도
            targetScaleX = -1f; 
            
            // 우측 퇴장 시 멈춤
            stopPos = new Vector3(EXIT_STOP_X_RIGHT, playerPos.y, 0);
            shouldStop = true; 
        }
        else if (attackDirection == 2) // 위쪽에서 나타나 아래로 공격 (상 -> 하)
        {
            warningIndex = verticalWarningIndex; 
            warningRotation = Quaternion.Euler(0, 0, 90f); // ✨ 90도 회전
            
            dragonSpawnPos.x = playerPos.x; 
            dragonSpawnPos.y = VERTICAL_SPAWN_Y_UP; 
            finalAttackTargetPos = new Vector3(playerPos.x, VERTICAL_TARGET_Y_DOWN, 0);
            warningSpawnPos = new Vector3(playerPos.x, HORIZONTAL_ATTACK_Y, 0); 
            dragonRotation = Quaternion.Euler(0, 0, 90f); // ✨ 90도 회전
            targetScaleX = 1f;
            
            // 하방 퇴장 시 멈춤
            stopPos = new Vector3(playerPos.x, EXIT_STOP_Y_DOWN, 0);
            shouldStop = true;
        }
        else // attackDirection == 3, 아래쪽에서 나타나 위로 공격 (하 -> 상)
        {
            warningIndex = verticalWarningIndex; 
            warningRotation = Quaternion.Euler(0, 0, 90f); // ✨ 90도 회전
            
            dragonSpawnPos.x = playerPos.x; 
            dragonSpawnPos.y = VERTICAL_SPAWN_Y_DOWN; 
            finalAttackTargetPos = new Vector3(playerPos.x, VERTICAL_TARGET_Y_UP, 0);
            warningSpawnPos = new Vector3(playerPos.x, HORIZONTAL_ATTACK_Y, 0); 
            dragonRotation = Quaternion.Euler(0, 0, 270f); // ✨ 270도 회전
            targetScaleX = 1f;

            // 상방 퇴장 시 멈춤
            stopPos = new Vector3(playerPos.x, EXIT_STOP_Y_UP, 0);
            shouldStop = true;
        }
        
        float totalMoveDistance = 1.0f; 


        // 2. 위험 표시 생성 및 대기
        // ✨ warningRotation 적용
        GameObject warning = bossManager.UseSkill(warningIndex, warningSpawnPos, warningRotation); 
        
        yield return new WaitForSeconds(1.0f); 
        
        if (warning != null) 
            warning.SetActive(false);


        // 3. 드래곤 소환 및 돌진
        // ✨ dragonRotation 적용
        GameObject dragon = bossManager.UseSkill(dragonSkillIndex, dragonSpawnPos, dragonRotation); 
        
        if (dragon != null)
        {
            // 초기화 강화: UseSkill에서 회전을 적용했으므로, 여기서는 위치만 재설정
            dragon.transform.position = dragonSpawnPos;
            // dragon.transform.rotation = dragonRotation; // UseSkill에서 이미 적용됨
            
            dragon.transform.DOKill();
            SpriteRenderer sr = dragon.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.DOFade(1f, 0f); 
            
            // 스케일 적용
            Vector3 currentScale = dragon.transform.localScale;
            dragon.transform.localScale = new Vector3(targetScaleX * Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

            // 돌진 실행 및 완료 대기
            yield return dragon.transform.DOMove(finalAttackTargetPos, chargeDuration).SetEase(Ease.Linear).WaitForCompletion();
        }
        
        // 4. 맵 밖으로 퇴장 (Sequence 사용, 페이드 아웃 없음)

        Vector3 exitDirection = (finalAttackTargetPos - dragonSpawnPos).normalized; 
        Vector3 exitPos = finalAttackTargetPos + exitDirection * exitDistance; 
        
        if (dragon != null)
        {
            Sequence sequence = DOTween.Sequence();
            
            if (shouldStop)
            {
                // 4-1. 멈춤 지점까지 1차 이동 (exitDuration을 기준으로 시간 배분)
                float distanceToStop = Vector3.Distance(finalAttackTargetPos, stopPos);
                float distanceToExit = Vector3.Distance(finalAttackTargetPos, exitPos);
                
                if (distanceToExit <= 0.01f) distanceToExit = 0.01f; 

                float moveDuration1 = exitDuration * (distanceToStop / distanceToExit); 

                sequence.Append(dragon.transform.DOMove(stopPos, moveDuration1).SetEase(Ease.Linear));
                
                // 4-2. delay 동안 멈춤
                sequence.AppendInterval(delay);
                
                // 4-3. 최종 퇴장 위치까지 2차 이동 (남은 시간 배분)
                float moveDuration2 = exitDuration - moveDuration1;
                sequence.Append(dragon.transform.DOMove(exitPos, moveDuration2).SetEase(Ease.Linear));
            }
            else
            {
                // 멈춤이 필요 없을 경우: 최종 퇴장 위치까지 한 번에 이동
                sequence.Append(dragon.transform.DOMove(exitPos, exitDuration).SetEase(Ease.Linear));
            }

            // 시퀀스가 끝날 때까지 대기
            yield return sequence.WaitForCompletion();
        }

        // 5. 퇴장 완료 후 즉시 비활성화
        if (dragon != null)
        {
            dragon.SetActive(false);
        }

        currentCoroutine = null;
    }
}