using UnityEngine;
using System.Collections;

public class Tire_Normal_2 : Skill_based
{
    [Header("보스 스킬 오브젝트")]
    public GameObject[] skillPrefabs; // UseSkill 0,1,2 같은 오브젝트들

    [Header("소환 설정")]
    public int spawnCount = 5;             // 소환할 오브젝트 개수
    public float spawnRadius = 3f;         // 보스 주변 반경
    public float throwDelay = 5f;          // 던지기까지 딜레이

    [Header("투사체 설정")]
    public float throwSpeed = 10f;         // 플레이어 방향 속도

    private Transform playerTarget;
    private Coroutine currentAttackCoroutine; // 현재 공격 코루틴

    void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindWithTag("Player");
        if (player)
            playerTarget = player.transform;
    }

    // ------------------ 추상 클래스 구현 ------------------
    public override void Attack()
    {
        if (currentAttackCoroutine != null)
            StopCoroutine(currentAttackCoroutine);

        currentAttackCoroutine = StartCoroutine(SpawnAndThrow());
    }

    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }
    }

    // ------------------ 스킬 코루틴 ------------------
    private IEnumerator SpawnAndThrow()
    {
        GameObject[] spawnedObjects = new GameObject[spawnCount];

        // 1. 주변에 랜덤 위치로 5개 소환
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = skillPrefabs[Random.Range(0, skillPrefabs.Length)];
            Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);
            spawnedObjects[i] = obj;
        }

        // 2. throwDelay 만큼 대기
        yield return new WaitForSeconds(throwDelay);

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

        // 코루틴 종료
        currentAttackCoroutine = null;
    }
}
