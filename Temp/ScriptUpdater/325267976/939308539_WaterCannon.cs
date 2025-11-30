using UnityEngine;
using System.Collections;

public class WaterCannon : MonoBehaviour
{
    [Header("물줄기 설정")]
    [Tooltip("물줄기 시각 효과로 사용할 프리팹 (물줄기 애니메이션/파티클)")]
    public GameObject waterStreamPrefab; 
    [Tooltip("물줄기가 닿는 최대 거리")]
    public float maxStreamDistance = 15f; 
    [Tooltip("물줄기 공격의 데미지")]
    public int damageAmount = 20;

    [Header("스킬 주기 설정")]
    [Tooltip("물대포 스킬을 사용하는 주기 (초)")]
    public float skillInterval = 7f; 
    [Tooltip("물줄기 시각 효과가 유지되는 시간 (초)")]
    public float streamDuration = 0.8f; 

    private Transform player; 
    private float timer;      

    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요!");
        }

        timer = skillInterval;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // 스킬 주기 도달 시 물대포 스킬 실행
            StartWaterCannon();
            // 타이머 재설정
            timer = skillInterval;
        }
    }

    // 물줄기 스킬 발동 로직
    void FireWaterCannon()
    {
        // 1. 물대포 생성
        GameObject projectile = Instantiate(waterCannonPrefab, transform.position, Quaternion.identity);

        // 2. 플레이어 방향 계산 및 정규화
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // 3. 투사체의 회전 방향 설정 (2D 환경 기준)
        // Atan2(y, x)를 사용하여 방향 벡터의 각도(라디안)를 계산한 후, 도(Degree)로 변환합니다.
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);  // 이 부분이 중요합니다.
        
        // 일반적으로 2D 스프라이트가 오른쪽(X축)을 향할 경우:
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 만약 투사체 스프라이트가 위쪽(Y축)을 기본 방향으로 한다면:
        // projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); 

        // 4. 투사체 이동 (이전과 동일)
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = directionToPlayer * projectileSpeed;
        }
    }

    // 충돌 처리 로직 (Raycast 사용)
    void CastWaterStreamDamage(Vector3 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxStreamDistance);

        // 충돌체 확인 (3D라면 RaycastHit hit = Physics.Raycast(...) 사용)
        if (hit.collider != null)
        {
            // 플레이어 태그 확인
            if (hit.collider.CompareTag("Player"))
            {
                // TODO: 여기에 플레이어에게 데미지를 입히는 로직을 추가합니다.
                Debug.Log($"플레이어 적중! 데미지: {damageAmount}");
            }
        }
    }

    // 물줄기 시각 효과 재생 로직
    void ShowWaterStreamVisual(Vector3 direction)
    {
        if (waterStreamPrefab == null) return;

        // 물줄기 프리팹 생성
        GameObject stream = Instantiate(waterStreamPrefab, transform.position, Quaternion.identity);

        // 2D 회전 적용 (물줄기가 플레이어 방향을 향하도록)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        stream.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 물줄기를 현무의 앞쪽으로 살짝 이동 (선택 사항)
        stream.transform.position += direction * 0.5f; 

        // 물줄기 길이를 Raycast 거리만큼 조정 (프리팹 설정에 따라 다름)
        // 만약 프리팹이 LineRenderer나 파티클이라면, 여기서 길이를 설정해야 합니다.
        
        // 일정 시간 후 파괴
        Destroy(stream, streamDuration);
    }
}