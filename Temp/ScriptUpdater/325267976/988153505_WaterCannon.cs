using UnityEngine;
using System.Collections; // IEnumerater/yield return 사용을 위해 필수

// ✅ 파일 이름과 클래스 이름이 일치해야 합니다. (WaterCannon.cs)
public class WaterCannon : MonoBehaviour
{
    // ===========================================
    // ✅ 변수 선언 위치 (클래스 멤버 변수)
    // ===========================================
    [Header("발사 설정")]
    [Tooltip("발사할 물대포 프리팹")]
    public GameObject waterCannonPrefab; 
    [Tooltip("물대포의 속도")]
    public float projectileSpeed = 15f; 

    [Header("스킬 주기 설정")]
    [Tooltip("스킬을 사용하는 주기 (예: 7초마다 발사)")]
    public float skillInterval = 7f;
    [Tooltip("투사체 세트 발사 횟수 (예: 5발 연속 발사)")]
    public int shotsPerBurst = 5; 
    [Tooltip("연속 발사 시 투사체 간의 간격 (초)")]
    public float delayBetweenShots = 0.2f; 

    private Transform player; 
    private float timer;      

    void Start()
    {
        // 'Player' 태그를 가진 오브젝트를 찾습니다.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요!");
        }

        // 초기 타이머 설정
        timer = skillInterval;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // ✅ 오류 해결: StartWaterCannon 대신 FireWaterCannonBurst 호출
            StartCoroutine(FireWaterCannonBurst()); 
            // 타이머 재설정
            timer = skillInterval;
        }
    }

    // 물대포를 연속 발사하는 코루틴
    IEnumerator FireWaterCannonBurst()
    {
        for (int i = 0; i < shotsPerBurst; i++)
        {
            FireWaterCannon();
            // 다음 발사까지 대기
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    // 실제 물대포를 생성하고 날리는 로직 (2D 환경 기준 및 플레이어 방향 회전)
    void FireWaterCannon()
    {
        // 1. 물대포 생성
        GameObject projectile = Instantiate(waterCannonPrefab, transform.position, Quaternion.identity);

        // 2. 플레이어 방향 계산 및 정규화
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // 3. 투사체의 회전 방향 설정 (2D 환경 기준)
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // 물줄기가 플레이어를 바라보도록 Z축 회전 적용
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 4. 투사체 이동 (Rigidbody 또는 Projectile 스크립트 사용)
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = directionToPlayer * projectileSpeed;
        }
        else
        {
            // Rigidbody가 없으면 Projectile 스크립트를 찾아 초기화
            // (Projectile.cs 스크립트가 별도로 존재해야 함)
            Projectile p = projectile.GetComponent<Projectile>(); 
            if (p != null)
            {
                p.Initialize(directionToPlayer, projectileSpeed);
            }
            else
            {
                Debug.LogWarning("투사체 프리팹에 Rigidbody 또는 Projectile.cs 스크립트가 없습니다!");
                Destroy(projectile); 
            }
        }
    }
}