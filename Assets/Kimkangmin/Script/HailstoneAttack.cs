using UnityEngine;
using Random = UnityEngine.Random;

public class HailstoneAttack : MonoBehaviour
{
    // ===========================================
    // 연속 소환 설정
    // ===========================================
    public GameObject hailstonePrefab;
    
    [Header("연속 소환 타이머")]
    [Tooltip("우박이 떨어지는 시간 간격 (초)")]
    public float spawnInterval = 0.5f; 
    private float nextSpawnTime = 0f;

    [Header("낙하 설정")]
    public float spawnY = 8f;            // 소환 시작 Y 좌표 (높은 곳)
    public float verticalDropSpeed = 7f; // Y축 하강 속도

    [Tooltip("우박의 약간의 무작위 좌우 흔들림 속도 (0으로 설정하면 수직 낙하)")]
    public float maxHorizontalSpeed = 1.5f; 
    
    public float spawnDepthZ = 0f; 

    // ===========================================
    // 내부 변수
    // ===========================================
    private Transform playerTransform; 

    void Start()
    {
        // 1. 플레이어 Transform 찾기
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요!");
        }

        // 2. 초기 소환 시간을 현재 시간으로 설정하여 즉시 시작 준비
        nextSpawnTime = Time.time;
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (Time.time >= nextSpawnTime)
        {
            // ⭐ 조건 1 만족: 단일 소환 함수 호출 ⭐
            SpawnSingleHailstone(); 
            // 다음 소환 시간 업데이트
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnSingleHailstone()
    {
        if (playerTransform == null) return;
    
        // 1. 소환 위치 계산
        float targetX = playerTransform.position.x;
        
        // ⭐ 핵심: 플레이어의 현재 Y 좌표를 파괴 경계로 설정합니다. ⭐
        float targetDestroyY = playerTransform.position.y; 

        Vector3 spawnPosition = new Vector3(targetX, spawnY, spawnDepthZ);

        // 2. 프리팹 생성
        GameObject newHail = Instantiate(hailstonePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = newHail.GetComponent<Rigidbody2D>();

        // 3. Rigidbody2D에 속도 부여
        if (rb != null)
        {
            float randomVX = Random.Range(-maxHorizontalSpeed, maxHorizontalSpeed);
            float constantVY = -verticalDropSpeed; 
            rb.linearVelocity = new Vector2(randomVX, constantVY);
        }

        // 4. Hailstone 스크립트에 목표 Y 좌표 전달
        Hailstone hailScript = newHail.GetComponent<Hailstone>();
        if(hailScript != null)
        {
            // ⭐ 조건 2 구현: 이 값을 Hailstone 스크립트로 전달합니다. ⭐
            hailScript.destroyYBoundary = targetDestroyY; 
        }
        else
        {
            Debug.LogError("우박 프리팹에 Hailstone.cs 스크립트가 없습니다!");
        }
    }
}