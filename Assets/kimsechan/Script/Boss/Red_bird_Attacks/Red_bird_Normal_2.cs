using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Red_bird_Normal_2 : Skill_based
{
    [Header("Boss Reference")]
    public BossManager bossManager;

    [Header("Bullet Settings")]
    public int bulletCount = 5;       // 한 번에 쏠 총알 수 (5개 유지)
    public float spreadAngle = 240f;  // 부채꼴 총 각도 (간격 넓게: 240도 유지)
    public float speed = 5f;
    public float bulletScale = 1f;

    [Header("Spawn Settings")]
    public float spawnY = 0f;
    public float spawnZ = 0f;

    [Header("Rotation")]
    public float rotationOffset = 0f;
    
    // 공격 간격을 0.5초로 유지
    private const float ATTACK_DELAY = 0.5f; 
    
    // 💡 [추가]: 현재 실행 중인 공격 코루틴 참조
    private Coroutine currentAttackCoroutine;
    
    // Attack 오버라이드 구조 (코루틴 시작) 유지
    public override void Attack()
    {
        // 스킬 실행을 담당하는 상위 GameObject에서 Coroutine을 실행해야 합니다.
        if (bossManager != null && bossManager.GetComponent<MonoBehaviour>() != null)
        {
            // 💡 [수정]: 기존 코루틴이 실행 중이라면 중지하고 새로 시작 (안전성 확보)
            if (currentAttackCoroutine != null)
            {
                bossManager.GetComponent<MonoBehaviour>().StopCoroutine(currentAttackCoroutine);
            }
            currentAttackCoroutine = bossManager.GetComponent<MonoBehaviour>().StartCoroutine(SequentialAttack());
        }
    }
    
    // 💡 [추가]: Turn_Change.cs에서 호출하여 공격을 강제로 중지하는 메서드 (Red_bird_SkilBase에 정의가 필요함)
    public override void StopAttack() 
    {
        if (currentAttackCoroutine != null)
        {
            // 보스 매니저에서 코루틴 중지
            bossManager.GetComponent<MonoBehaviour>().StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
            
            // OnSkill 플래그를 바로 해제하여 상태를 정리
            bossManager.OnSkill = false;
            
            // Turn_Change의 isAttacking 플래그도 false로 설정해야 합니다.
            Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
            if (turnChange != null)
            {
                turnChange.isAttacking = false;
            }
        }
    }

    private System.Collections.IEnumerator SequentialAttack()
    {
        bossManager.OnSkill = true; 

        if (bossManager == null || bossManager.bossSkills.Count == 0)
        {
            currentAttackCoroutine = null; // 코루틴 종료 시 참조 해제
            yield break;
        }

        Vector3 spawnPos = new Vector3(transform.position.x, spawnY, spawnZ);
        
        const float CENTER_ANGLE = 180f; 
        float waveSpread = spreadAngle / 2f;
        
        // bulletCount가 1일 경우 0으로 나누는 오류 방지
        float angleStepValue = (bulletCount > 1) ? waveSpread / (bulletCount - 1) : 0f; 
        
        for (int attackCount = 0; attackCount < 7; attackCount++)
        {
            // 💡 [추가]: 시네마틱 중단 여부 체크 (RunAttackLoop의 조건과 별개로 코루틴 내부에서 체크)
            if (bossManager.IsPlayerInputLocked)
            {
                // 강제 중단 시 OnSkill 해제 후 종료
                bossManager.OnSkill = false;
                currentAttackCoroutine = null;
                yield break;
            }
            
            float startAngle;
            float angleStep;

            if (attackCount % 2 == 0) // 0, 2, 4, 6번째 공격 (기본 위치)
            {
                startAngle = CENTER_ANGLE - (waveSpread / 2f); 
                angleStep = angleStepValue;
            }
            else // 1, 3, 5번째 공격 (틈새 메우기)
            {
                startAngle = (CENTER_ANGLE - (waveSpread / 2f)) + (angleStepValue / 2f); 
                angleStep = angleStepValue; 
            }
            
            // ----------------------------------------------------------------------------------
            
            // ✅ 한 번의 발사 (탄막 bulletCount개) 루프
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                float rad = (angle + rotationOffset) * Mathf.Deg2Rad;

                // 기존 코드 (0도가 위쪽 기준) 유지
                Vector3 dir = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0);
                if (dir == Vector3.zero)
                    dir = Vector3.up;

                float bulletAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                
                // ✅ 수정: 최종 회전에 90도를 추가합니다.
                Quaternion rot = Quaternion.Euler(0, 0, bulletAngle - 90f + 90f);

                GameObject bullet = bossManager.UseSkill(0, spawnPos, rot);
                if (bullet == null) continue;

                bullet.transform.localScale = Vector3.one * bulletScale;

                BulletMovement bm = bullet.GetComponent<BulletMovement>();
                if (bm != null)
                    bm.SetDirection(dir.normalized * speed);
            }

            // ✅ 다음 공격까지 잠시 대기
            yield return new WaitForSeconds(ATTACK_DELAY);
        }
        
        // ✅ 스킬 완료 후 OnSkill을 false로 설정하여 이동 재개 및 다음 쿨타임 시작
        bossManager.OnSkill = false;
        currentAttackCoroutine = null; // 코루틴 종료 시 참조 해제
    }
}
