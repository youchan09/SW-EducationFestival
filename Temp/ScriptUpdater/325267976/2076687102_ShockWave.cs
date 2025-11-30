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
    public float shockWaveSpeed = 15f; // ⭐ 새로 추가된 충격파 속도 변수 ⭐
    public float shockWaveDuration = 3f; // 충격파가 날아가는 동안 유지되는 시간
    
    [Header("발사 주기 설정")]
    public float cycleInterval = 5f; 
    
    [Header("경고 설정")]
    public float warningTime = 1f; 

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

        // 4. ShockWave 발사
        SpawnShockWave(); 
    }

    void SpawnShockWave()
    {
        if (shockWavePrefab == null) 
        {
            Debug.LogError("ShockWave Prefab이 연결되지 않았습니다!");
            return;
        }
        
        // 1. ShockWave 생성
        GameObject shockWave = Instantiate(shockWavePrefab, firePoint.position, firePoint.rotation);
        
        // 2. 방향 설정
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized; // 정규화 (방향만)
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        shockWave.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 

        // 3. 위치 조정: 가장자리가 firePoint에 오도록 조정
        float halfLength = shockWave.transform.localScale.y / 2f; 
        shockWave.transform.position += shockWave.transform.up * halfLength;
        
        // 4. ⭐ ShockWave 날리기 (Rigidbody2D 사용) ⭐
        Rigidbody2D rb2d = shockWave.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            // 계산된 방향과 속도를 곱하여 속도 벡터를 설정
            rb2d.linearVelocity = directionToPlayer * shockWaveSpeed; 
            Debug.Log("🚀 ShockWave 발사됨! Velocity: " + rb2d.linearVelocity);
        }
        else
        {
            Debug.LogError("ShockWave 프리팹에 Rigidbody2D 컴포넌트가 없습니다! 비행할 수 없습니다.");
        }

        // 5. 유지 시간 후 파괴
        Destroy(shockWave, shockWaveDuration);
    }
}