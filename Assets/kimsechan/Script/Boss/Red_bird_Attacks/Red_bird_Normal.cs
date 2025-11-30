using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Red_bird_Normal : Skill_based
{
    [Header("Boss Reference")]
    public BossManager bossManager;

    [Header("Bullet Settings")]
    public float speed = 5f;
    public float bulletScale = 1f;
    
    private const int FIRE_COUNT = 5;
    private const float TOTAL_TIME = 2f;
    private const float ATTACK_DELAY = TOTAL_TIME / FIRE_COUNT; // 0.4초 간격

    [Header("Spawn Settings")]
    public float spawnY = 0f;
    public float spawnZ = -1f;
    
    // 💡 [추가]: 현재 실행 중인 공격 코루틴 참조
    private Coroutine currentAttackCoroutine;

    public override void Attack()
    {
        if (bossManager != null)
        {
            // 💡 [수정]: 기존 코루틴이 있다면 중지 (안전성)
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }
            // 💡 [수정]: 코루틴 참조를 저장하여 StopAttack에서 중지할 수 있도록 합니다.
            currentAttackCoroutine = StartCoroutine(RepeatAttack());
        }
    }
    
    // 💡 [구현]: Red_bird_SkilBase에서 요구하는 강제 중지 메서드
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
        Debug.Log("Red_bird_Normal: Attack forcefully stopped by StopAttack.");
    }

    private IEnumerator RepeatAttack()
    {
        bossManager.OnSkill = true;

        for (int i = 0; i < FIRE_COUNT; i++)
        {
            // 💡 [추가]: 시네마틱 중단 여부 체크
            if (bossManager.IsPlayerInputLocked)
            {
                // 강제 중단 시 상태 정리
                bossManager.OnSkill = false;
                
                Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
                if (turnChange != null)
                    turnChange.isAttacking = false;
                
                currentAttackCoroutine = null;
                Debug.Log("Red_bird_Normal: Attack interrupted by cinematic check.");
                yield break;
            }
            
            if (PlayerManager.instance == null)
            {
                Debug.LogError("PlayerManager 인스턴스를 찾을 수 없습니다!");
                
                // 에러 발생 시 상태 정리 후 종료
                bossManager.OnSkill = false;
                Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
                if (turnChange != null) turnChange.isAttacking = false;
                currentAttackCoroutine = null;
                yield break; 
            }

            Vector3 startPos = new Vector3(transform.position.x, spawnY, spawnZ);
            Vector3 targetPos = PlayerManager.instance.transform.position;
            
            // 1. 방향 벡터 계산
            Vector3 direction = (targetPos - startPos).normalized;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            Quaternion rot = Quaternion.Euler(0, 0, angle); 

            // 4. UseSkill 메서드를 통해 총알 생성 (인덱스 1 사용)
            GameObject bullet = bossManager.UseSkill(1, startPos, rot);
            if (bullet == null)
            {
                yield return new WaitForSeconds(ATTACK_DELAY);
                continue;
            }

            bullet.transform.localScale = Vector3.one * bulletScale;

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / speed; 
            GameObject currentBullet = bullet;

            // 5. DOMove 트윈 시작
            currentBullet.transform.DOMove(targetPos, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (currentBullet == null || !currentBullet.activeSelf)
                    {
                        return; 
                    }
                    
                    Vector3 spawnPos2 = currentBullet.transform.position;
                    spawnPos2.z = -0.5f;
                    
                    // 첫 번째 총알을 풀로 되돌립니다.
                    currentBullet.SetActive(false); 
                    
                    // 두 번째 총알 (인덱스 2 사용)
                    GameObject bullet_2 = bossManager.UseSkill(2, spawnPos2, Quaternion.identity);

                    if (bullet_2 != null)
                        bullet_2.transform.localScale = Vector3.one * bulletScale;
                });

            yield return new WaitForSeconds(ATTACK_DELAY);
        }
        
        // 💡 [추가]: 스킬 완료 후 상태 정리
        bossManager.OnSkill = false;
        
        Turn_Change turn_Change = bossManager.GetComponent<Turn_Change>();
        if (turn_Change != null)
            turn_Change.isAttacking = false;
            
        // 💡 [추가]: 코루틴 완료 시 참조 해제
        currentAttackCoroutine = null;
    }
}
