using UnityEngine;
using System.Collections; 

public class ShockWave : MonoBehaviour
{
    // ================== ShockWave 공격 설정 ==================
    [Header("ShockWave 설정")]
    public GameObject shockWavePrefab;
    public GameObject warningIndicatorPrefab; 
    public Transform firePoint;       
    
    [Header("발사 속도 및 유지")]
    public float shockWaveSpeed = 15f; 
    public float shockWaveDuration = 3f; 
    
    [Header("발사 주기 설정")]
    public float cycleInterval = 5f; 
    
    [Header("경고 설정")]
    public float warningTime = 1f; 

    // 3방향 발사 설정
    [Header("다중 발사 설정")]
    private const int NUMBER_OF_SHOTS = 3;
    public float spreadAngle = 15f; 

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

        timer = cycleInterval;

        if (firePoint == null)
        {
            firePoint = this.transform; 
        }
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(PrepareAndSpawnShockWave()); 
            timer = cycleInterval;
        }
    }

    IEnumerator PrepareAndSpawnShockWave()
    {
        // 1. 경고 시점의 플레이어 위치 저장
        Vector3 targetPosition = player.position;

        GameObject indicator = null;
        if (warningIndicatorPrefab != null)
        {
            indicator = Instantiate(warningIndicatorPrefab, firePoint.position, firePoint.rotation);
            
            // 조준 방향 계산
            Vector3 directionToTarget = targetPosition - firePoint.position;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            
            // ⭐ 2. 경고 프리팹 180도 회전 ⭐
            // 기존 각도에 180도를 더하여 회전 방향을 반대로 만듭니다.
            indicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90f); // -90f 대신 +90f를 사용하거나, -90f 후 180f를 더합니다.
            // (Note: -90f가 정면일 경우, 180f를 더하면 +90f가 됩니다. 스프라이트 방향에 따라 보정 각도는 달라질 수 있습니다.)
            
            // 3. 위치 조정 (삼각형의 밑변을 firePoint에 맞춤)
            // 삼각형의 밑변이 firePoint에 오도록 이동시키려면, 꼭짓점을 맞추던 것과 마찬가지로 길이의 절반만큼 이동합니다.
            float halfLength = indicator.transform.localScale.y / 2f; 
            indicator.transform.position += indicator.transform.up * halfLength;
        }
        
        // 4. 경고 시간 대기 (1초)
        yield return new WaitForSeconds(warningTime);
        
        // 5. 경고 인디케이터 파괴
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // 6. ShockWave 3개 발사 (저장된 위치를 목표로 전달)
        SpawnShockWavesBurst(targetPosition); 
    }

    void SpawnShockWavesBurst(Vector3 targetPosition)
    {
        if (shockWavePrefab == null) 
        {
            Debug.LogError("ShockWave Prefab이 연결되지 않았습니다!");
            return;
        }

        // 1. 기본 조준 방향 계산 (저장된 목표 위치를 향함)
        Vector3 directionToTarget = (targetPosition - firePoint.position).normalized; 
        float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        
        // 2. 각도 오프셋 설정
        float[] angleOffsets = { 0f, spreadAngle, -spreadAngle };

        // 3. 3번 반복하여 충격파 생성 및 발사
        for (int i = 0; i < NUMBER_OF_SHOTS; i++)
        {
            float currentAngle = baseAngle + angleOffsets[i];
            
            // 발사 방향 벡터 재계산
            Vector3 fireDirection = new Vector3(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                0f
            ).normalized;

            CreateAndLaunchShockWave(fireDirection);
        }
    }

    void CreateAndLaunchShockWave(Vector3 fireDirection)
    {
        GameObject shockWave = Instantiate(shockWavePrefab, firePoint.position, Quaternion.identity);
        
        // ⭐ 충격파는 정방향(플레이어를 향하는 방향)으로 회전 ⭐
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
        shockWave.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 

        // 4. 위치 조정: 가장자리가 firePoint에 오도록 조정
        float halfLength = shockWave.transform.localScale.y / 2f; 
        shockWave.transform.position += shockWave.transform.up * halfLength;
        
        Rigidbody2D rb2d = shockWave.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = fireDirection * shockWaveSpeed; 
        }
        else
        {
            Debug.LogError("ShockWave 프리팹에 Rigidbody2D 컴포넌트가 없습니다! 비행할 수 없습니다.");
        }

        Destroy(shockWave, shockWaveDuration);
    }
}