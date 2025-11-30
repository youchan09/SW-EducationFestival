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

    void Start()
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
        GameObject[] spawnedObjects = new GameObject[spawnCount];

        // 1. 보스 주변 랜덤 위치에 소환
        for (int i = 0; i < spawnCount; i++)
        {
            int skillIndex = Random.Range(0, bossManager.bossSkills.Count);
            Vector3 randomPos = bossManager.transform.position + (Vector3)(Random.insideUnitCircle * spawnRadius);

            GameObject obj = bossManager.UseSkill(skillIndex, randomPos, Quaternion.identity);
            spawnedObjects[i] = obj;
        }

        // 2. throwDelay 동안 보스를 따라다님
        float elapsed = 0f;
        while (elapsed < throwDelay)
        {
            elapsed += Time.deltaTime;

            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    Vector3 dirToBoss = (bossManager.transform.position - obj.transform.position);
                    obj.transform.position += dirToBoss.normalized * followSpeed * Time.deltaTime;
                }
            }

            yield return null;
        }

        // 3. 플레이어 방향으로 투사
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null && playerTarget != null)
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb == null)
                    rb = obj.AddComponent<Rigidbody2D>();

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0;

                Vector2 dir = (playerTarget.position - obj.transform.position).normalized;
                rb.linearVelocity = dir * throwSpeed;
            }
        }

        currentAttackCoroutine = null;
    }
}
