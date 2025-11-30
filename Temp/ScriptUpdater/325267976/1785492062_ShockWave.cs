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

    // ⭐ 3방향 발사 설정 ⭐
    [Header("다중 발사 설정")]
    [Tooltip("총 발사 개수 (3개 고정)")]
    private const int NUMBER_OF_SHOTS = 3;
    [Tooltip("가운데 충격파를 기준으로 양쪽 충격파가 벌어지는 각도 (예: 15도)")]
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
        // 1. 경고 인디케이터 생성 및 위치 조정 (이전 로직 유지)
        GameObject indicator = null;
        if (warningIndicatorPrefab != null)
        {
            indicator = Instantiate(warningIndicatorPrefab, firePoint.position, firePoint.rotation);
            
            // 조준 및 위치 조정
            Vector3 directionToPlayer = player.position - firePoint.position;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            indicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 

            float halfLength = indicator.transform.localScale.y / 2f; 
            indicator.transform.position += indicator.transform.up * halfLength;
        }
        
        // 2. 경고 시간 대기 (1초)
        yield return new WaitForSeconds(warningTime);
        
        // 3. 경고 인디케이터 파괴
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // 4. ⭐ ShockWave 3개 발사 ⭐
        SpawnShockWavesBurst(); 
    }

    // ⭐ 3개의 충격파를 발사하는 새로운 함수 ⭐
    void SpawnShockWavesBurst()
    {
        if (shockWavePrefab == null) 
        {
            Debug.LogError("ShockWave Prefab이 연결되지 않았습니다!");
            return;
        }

        // 1. 기본 조준 방향 계산 (플레이어 방향)
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized; 
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // 2. 각도 오프셋 설정 (중앙, 왼쪽, 오른쪽)
        float[] angleOffsets = { 0f, spreadAngle, -spreadAngle }; // 0도, +15도, -15도 (예시)

        // 3. 3번 반복하여 충격파 생성 및 발사
        for (int i = 0; i < NUMBER_OF_SHOTS; i++)
        {
            float currentAngle = baseAngle + angleOffsets[i];
            
            // 발사 방향 벡터 재계산 (삼각함수 사용)
            Vector3 fireDirection = new Vector3(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                0f
            ).normalized;

            CreateAndLaunchShockWave(fireDirection);
        }

        Debug.Log($"💥 ShockWave 3개가 {spreadAngle} 각도로 발사되었습니다!");
    }

    // 개별 ShockWave를 생성하고 날리는 로직
    void CreateAndLaunchShockWave(Vector3 fireDirection)
    {
        // 1. ShockWave 생성
        GameObject shockWave = Instantiate(shockWavePrefab, firePoint.position, Quaternion.identity);
        
        // 2. 회전 설정 (발사 방향으로)
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
        shockWave.transform.rotation = Quaternion.Euler(0, 0, angle + 90f); 

        // 3. 위치 조정: 가장자리가 firePoint에 오도록 조정
        float halfLength = shockWave.transform.localScale.y / 2f; 
        shockWave.transform.position += shockWave.transform.up * halfLength;
        
        // 4. ShockWave 날리기 (Rigidbody2D 사용)
        Rigidbody2D rb2d = shockWave.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            // 계산된 방향과 속도를 곱하여 속도 벡터를 설정
            rb2d.linearVelocity = fireDirection * shockWaveSpeed; 
        }
        else
        {
            Debug.LogError("ShockWave 프리팹에 Rigidbody2D 컴포넌트가 없습니다! 비행할 수 없습니다.");
        }

        // 5. 유지 시간 후 파괴
        Destroy(shockWave, shockWaveDuration);
    }
}