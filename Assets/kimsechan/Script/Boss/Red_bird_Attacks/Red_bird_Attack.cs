using UnityEngine;
using System.Collections;

public class Red_bird_Attack : Skill_based
{
    [Header("Boss Reference")]
    public BossManager bossManager;

    [Header("Bullet Settings")] 
    public int bulletCount = 5;
    public float spreadAngle = 240f;
    public float speed = 5f;
    public float bulletScale = 1f;

    [Header("Spawn Settings")]
    public float spawnY = 0f;
    public float spawnZ = 0f;

    [Header("Rotation")]
    public float rotationOffset = 0f;
    
    [Header("Heart Spawn Frequency")]
    [Tooltip("총 20번의 공격 중 하트를 소환할 공격 횟수 주기 (예: 5로 설정하면 5, 10, 15번째에 소환)")]
    public int heartSpawnFrequency = 5; 
    
    // 현재 실행 중인 공격 코루틴 참조
    private Coroutine currentAttackCoroutine;

    private const float ATTACK_DELAY = 0.4f;

    public override void Attack()
    {
        if (bossManager != null)
        {
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }
            currentAttackCoroutine = StartCoroutine(SequentialAttack());
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
        Debug.Log("Red_bird_Attack: Attack forcefully stopped by StopAttack.");
    }

    private IEnumerator SequentialAttack()
    {
        bossManager.OnSkill = true;

        Red_bird redBird = bossManager.GetComponent<Red_bird>();
        
        // 💡 [수정]: 총알이 발사되는 중앙 지점 (spawnPos) 계산
        Vector3 bulletCenterSpawnPos = new Vector3(transform.position.x, spawnY, spawnZ);
        
        Vector3 FireTornadoPos = new Vector3(bulletCenterSpawnPos.x, bulletCenterSpawnPos.y + 0.5f, bulletCenterSpawnPos.z);
        GameObject bullet2 = bossManager.UseSkill(4, FireTornadoPos, Quaternion.identity);
        
        const float CENTER_ANGLE = 180f;
        float waveSpread = spreadAngle;
        
        float angleStepValue = (bulletCount > 1) ? waveSpread / (bulletCount - 1) : 0f;

        for (int attackCount = 1; attackCount <= 20; attackCount++)
        {
            if (bossManager.IsPlayerInputLocked)
            {
                bossManager.OnSkill = false;
                
                Turn_Change turnChange = bossManager.GetComponent<Turn_Change>();
                if (turnChange != null)
                    turnChange.isAttacking = false;
                
                currentAttackCoroutine = null;
                Debug.Log("Red_bird_Attack: Attack interrupted by cinematic check.");
                yield break;
            }
            
            float startAngle;
            float angleStep;
            
            startAngle = CENTER_ANGLE - (waveSpread / 2f) + (attackCount - 1) * 8;
            angleStep = angleStepValue;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                float rad = (angle + rotationOffset) * Mathf.Deg2Rad;

                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                if (dir == Vector3.zero)
                    dir = Vector3.up;

                float bulletAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, bulletAngle);

                GameObject bullet = bossManager.UseSkill(0, bulletCenterSpawnPos, rot); // 💡 [수정]: bulletCenterSpawnPos 사용
                if (bullet == null) continue;

                bullet.transform.localScale = Vector3.one * bulletScale;

                BulletMovement bm = bullet.GetComponent<BulletMovement>();
                if (bm != null)
                    bm.SetDirection(dir.normalized * speed);
            }

            yield return new WaitForSeconds(ATTACK_DELAY);
        }

        bossManager.OnSkill = false;

        Turn_Change turn_Change = bossManager.GetComponent<Turn_Change>();
        if (turn_Change != null)
            turn_Change.isAttacking = false;
            
        currentAttackCoroutine = null;
    }
}
