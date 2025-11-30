using UnityEngine;
using System.Collections;

public class Tire_Normal_2 : Skill_based
{
    [Header("BossManager 참조")]
    public BossManager bossManager;

    [Header("소환 설정")]
    public int spawnCount = 5;             // 소환할 오브젝트 개수
    public float spawnRadius = 3f;         // 보스 주변 반경
    public float followSpeed = 2f;         // 보스를 따라다니는 속도
    public float throwDelay = 5f;          // 던지기까지 딜레이
    public float throwSpeed = 10f;         // 플레이어 방향 속도

    private Transform playerTarget;
    private Coroutine currentAttackCoroutine; // 현재 공격 코루틴
    private GameObject[] spawnedObjects;      // 한 번만 모아서 저장

    void Update()
    {
        if (bossManager == null)
            bossManager = FindObjectOfType<BossManager>();

        if (bossManager == null)
            Debug.LogError("씬에 BossManager가 없습니다!");

        GameObject player = GameObject.FindWithTag("Player");
        if (player)
            playerTarget = player.transform;
        else
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다!");
    }

    public override void Attack()
    {
        if (currentAttackCoroutine != null)
            StopCoroutine(currentAttackCoroutine);

        currentAttackCoroutine = StartCoroutine(SpawnFollowAndThrow());
    }

    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }
    }

    private IEnumerator SpawnFollowAndThrow()
    {
        // 1. 한 번만 소환
        spawnedObjects = new GameObject[spawnCount]; // 매번 새로 배열 생성

        float startAngle = 0f;
        float endAngle = 180f;
        float angleStep = (spawnCount > 1) ? (endAngle - startAngle) / (spawnCount - 1) : 0f;

        for (int i = 0; i < spawnCount; i++)
        {
            int skillIndex = Random.Range(0, bossManager.bossSkills.Count);
            float angle = startAngle + angleStep * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * spawnRadius;
            Vector3 spawnPos = bossManager.transform.position + offset;

            GameObject obj = bossManager.UseSkill(skillIndex, spawnPos, Quaternion.identity);
            spawnedObjects[i] = obj;
        }


        // 2. throwDelay 동안 보스를 따라다님 (생성 위치 유지하며)
        float elapsed = 0f;
        while (elapsed < throwDelay)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject obj = spawnedObjects[i];
                if (obj != null)
                {
                    float angle = startAngle + angleStep * i;
                    Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * spawnRadius;

                    Vector3 targetPos = bossManager.transform.position + offset;
                    obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPos, followSpeed * Time.deltaTime);
                }
            }

            yield return null;
        }

        // 3. 플레이어 방향으로 투사
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = spawnedObjects[i];
            if (obj != null && playerTarget != null)
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb == null)
                    rb = obj.AddComponent<Rigidbody2D>();

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0;

                // ⚡ 돌 던질 때 콜라이더 켜기
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = true;

                Vector2 dir = (playerTarget.position - obj.transform.position).normalized;
                rb.linearVelocity = dir * throwSpeed;
            }
        }

        currentAttackCoroutine = null;
    }
}
