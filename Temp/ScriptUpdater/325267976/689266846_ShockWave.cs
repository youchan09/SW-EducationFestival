using UnityEngine;

public class ShockWave : MonoBehaviour
{
    // 현무의 현재 위치 (투사체 발사 위치)
    [Header("발사 설정")]
    [Tooltip("발사할 투사체 프리팹")]
    public GameObject projectilePrefab;
    [Tooltip("투사체의 속도")]
    public float projectileSpeed = 10f;

    [Header("발사 주기 설정")]
    [Tooltip("투사체 세트를 발사하는 주기 (초)")]
    public float cycleInterval = 5f;
    [Tooltip("한 주기 동안 발사할 투사체 개수")]
    public int projectilesPerCycle = 3;
    [Tooltip("한 주기 내에서 투사체 간의 시간 간격 (초)")]
    public float delayBetweenShots = 0.5f;

    private Transform player; // 플레이어 Transform
    private float timer;      // 발사 주기 타이머

    void Start()
    {
        // 씬에서 'Player' 태그를 가진 오브젝트를 찾습니다.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요!");
        }

        // 초기 타이머 설정 (첫 발사는 cycleInterval 후에 시작)
        timer = cycleInterval;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // 5초 주기가 되면 발사 코루틴 시작
            StartCoroutine(ShootProjectileBurst());
            // 타이머 재설정
            timer = cycleInterval;
        }
    }

    // 투사체를 연속해서 발사하는 코루틴
    System.Collections.IEnumerator ShootProjectileBurst()
    {
        for (int i = 0; i < projectilesPerCycle; i++)
        {
            Shoot();
            // 다음 투사체를 발사하기 전에 잠시 대기
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    // 실제 투사체를 생성하고 날리는 로직
    void Shoot()
    {
        // 1. 투사체 생성 (Quaternion.identity로 일단 회전 없이 생성)
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 2. 플레이어 방향 계산 및 정규화
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // --- ⭐ 추가된 부분 시작 ⭐ ---
        // 3. 투사체의 회전 방향 설정
        // LookRotation: 방향 벡터를 받아 회전(Quaternion)으로 변환합니다.
        // Vector3.forward (0, 0, 1)이 투사체의 앞쪽 축이라고 가정합니다.
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        projectile.transform.rotation = targetRotation;
        // --- ⭐ 추가된 부분 끝 ⭐ ---


        // 4. 투사체 이동
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = directionToPlayer * projectileSpeed;
        }
        else
        {
            // Rigidbody가 없으면 자체 이동 로직 추가 (Projectile 스크립트)
            Projectile p = projectile.GetComponent<Projectile>();
            if (p != null)
            {
                p.Initialize(directionToPlayer, projectileSpeed);
            }
            else
            {
                Debug.LogWarning("투사체 프리팹에 Rigidbody 또는 Projectile 스크립트가 없습니다!");
            }
        }
    }
}