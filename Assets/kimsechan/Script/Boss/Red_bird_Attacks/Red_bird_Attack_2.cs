using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic; // List/Dictionary를 사용하지 않더라도 Unity 컬렉션 사용을 위해 포함

public class Red_bird_Attack_2 : Skill_based
{
    [Header("Boss Reference")]
    public BossManager bossManager;

    [Header("Bullet Settings (Random Targeting)")]
    public float speed = 10f; // 총알 속도
    public float bulletScale = 5f; // 총알 크기
    [Tooltip("한 번의 버스트로 동시에 발사할 탄환 개수")]
    public int pelletsPerShot = 5; // 💡 [수정/추가]: 한번에 발사할 탄환 개수
    private const int BULLET_POOL_INDEX = 1; // UseSkill 인덱스 1 (DOTween 총알)
    
    [Header("Targeting Settings")]
    [Tooltip("플레이어 위치를 중심으로 랜덤하게 탄착 지점을 잡을 반경")]
    public float randomTargetRadius = 5f; // 랜덤 타겟팅 반경
    
    [Header("Trap Settings (Flooring)")]
    [Tooltip("장판(덫)으로 사용할 오브젝트 풀 인덱스 (일반적으로 3)")]
    public int trapPoolIndex = 3; 
    public float trapScale = 1.0f; // 장판 오브젝트의 크기

    [Header("Timing and Repetition")]
    [Tooltip("총 공격(탄환 버스트 발사 및 장판 생성) 반복 횟수")]
    public int attackRepeatCount = 8; // 총 공격 반복 횟수
    [Tooltip("다음 공격 버스트까지의 대기 시간 (0.4f는 이전 버전의 8발/1.5초 기준)")]
    private const float ATTACK_DELAY = 0.5f; // 공격 사이 간격 
    
    [Header("Spawn Location")]
    public float spawnY = 0f;
    public float spawnZ = -1f;
    
    // 💡 [추가]: 현재 실행 중인 공격 코루틴 참조
    private Coroutine currentAttackCoroutine;

    // Red_bird 컴포넌트를 캐시하여 플레이어 target에 접근
    // private Red_bird redBird; // 사용하지 않으므로 주석 처리

    public void Awake()
    {
        // BossManager를 찾습니다.
        GameObject bossManagerObject = GameObject.Find("BossManager");
        if (bossManagerObject != null)
        {
            bossManager = bossManagerObject.GetComponent<BossManager>();
            // redBird = bossManagerObject.GetComponent<Red_bird>(); // 제거
        }

        if (bossManager == null)
        {
            Debug.LogError("Red_bird_Attack_2: BossManager is not found in the scene or missing the component.");
        }
    }

    public override void Attack()
    {
        if (bossManager != null)
        {
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }
            currentAttackCoroutine = StartCoroutine(RepeatAttack());
        }
    }
    
    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        // 상태 정리
        if (bossManager != null)
        {
            bossManager.OnSkill = false;
            
            Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
            if (turnChange != null)
            {
                turnChange.isAttacking = false;
            }
        }
        Debug.Log("Red_bird_Attack_2: Attack forcefully stopped by StopAttack.");
    }

    private IEnumerator RepeatAttack()
    {
        bossManager.OnSkill = true;

        if (PlayerManager.instance == null)
        {
            Debug.LogError("Dependency missing (PlayerManager). Stopping attack.");
            bossManager.OnSkill = false;
            yield break;
        }

        // attackRepeatCount만큼 반복하며 탄환 묶음(pelletsPerShot 개)을 발사합니다.
        for (int i = 0; i < attackRepeatCount; i++)
        {
            if (bossManager.IsPlayerInputLocked)
            {
                bossManager.OnSkill = false;
                yield break;
            }
            
            Debug.Log($"[Attack 2] 🚀 {i + 1} / {attackRepeatCount}번째 랜덤 타겟팅 공격 시작 (동시 {pelletsPerShot}발).");
            
            // 1. 발사 위치 설정 (보스 위치)
            Vector3 startPos = new Vector3(transform.position.x, spawnY, spawnZ);
            
            // 2. 공격 시작 시점의 플레이어 위치를 기준점으로 저장
            Vector3 playerPos = PlayerManager.instance.transform.position;
            
            // 💡 [수정]: pelletsPerShot 개수만큼 총알을 동시에 발사하는 루프
            for (int j = 0; j < pelletsPerShot; j++)
            {
                // A. 랜덤 타겟 위치 계산 (플레이어 주변)
                // 원형 범위 내에서 랜덤 위치를 구함 (각 탄환마다 고유한 랜덤 값 사용)
                Vector2 randomCircle = Random.insideUnitCircle * randomTargetRadius;
                
                Vector3 targetPos = playerPos + new Vector3(randomCircle.x, randomCircle.y, 0f);
                
                // 총알이 Z=-1에서 이동할 수 있도록 Z축을 통일합니다.
                // 장판은 OnComplete에서 Z=0으로 설정됩니다.
                targetPos.z = spawnZ; 

                // B. 방향 및 회전 계산
                Vector3 direction = (targetPos - startPos).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle); 

                // C. 총알 생성 (BULLET_POOL_INDEX = 1)
                GameObject bullet = bossManager.UseSkill(BULLET_POOL_INDEX, startPos, rot);
                if (bullet == null)
                {
                    // 총알 생성 실패 시, 나머지 루프는 계속 실행
                    continue; 
                }

                bullet.transform.localScale = Vector3.one * bulletScale;

                // D. 이동 거리 및 시간 계산
                float distance = Vector3.Distance(startPos, targetPos);
                float duration = distance / speed; 
                GameObject currentBullet = bullet;

                // E. DOMove 트윈 시작
                currentBullet.transform.DOMove(targetPos, duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        if (currentBullet == null || !currentBullet.activeSelf)
                        {
                            return; 
                        }
                        
                        // 1. 장판 소환: 총알이 멈춘 위치(targetPos)에 장판을 생성합니다.
                        Vector3 trapSpawnPos = currentBullet.transform.position;
                        trapSpawnPos.z = 0f; // 장판은 Z=0 (바닥)에 깔리도록 합니다.
                        
                        // 총알을 풀로 되돌립니다.
                        currentBullet.SetActive(false); 
                        
                        // 장판 오브젝트 (trapPoolIndex = 3)
                        GameObject trap = bossManager.UseSkill(trapPoolIndex, trapSpawnPos, Quaternion.identity);

                        if (trap != null)
                        {
                            trap.transform.localScale = Vector3.one * trapScale;
                            trap.SetActive(true);
                        }
                    });

            } // 💡 [추가]: pelletsPerShot 루프 끝 (5발 동시 발사 완료)
            
            Debug.Log($"[Attack 2] ✅ {i + 1}번째 공격 버스트 ({pelletsPerShot}발) 완료.");

            // 다음 공격 묶음(버스트)까지 대기
            yield return new WaitForSeconds(ATTACK_DELAY);
        }
        
        // 스킬 완료 후 상태 정리
        bossManager.OnSkill = false;
        
        Turn_Change turn_Change = bossManager.GetComponent<Turn_Change>();
        if (turn_Change != null)
            turn_Change.isAttacking = false;
            
        currentAttackCoroutine = null;
    }

    /// <summary>
    /// 이 버전에서는 사용되지 않습니다. 로직은 RepeatAttack()에 통합되었습니다.
    /// </summary>
    private IEnumerator FireShotgunAndSpawnTrap()
    {
        yield break;
    }
}