using UnityEngine;
using System.Collections;

public class SlowProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 targetPosition;

    public GameObject miniSnakePrefab;
    private bool hasSpawnedSnake = false;

    public int damage = 5; // ✨ 직격 데미지 5로 설정

    public GameObject imagePrefab;

    // ✨ 1. 영역(그림)이 위치할 Z축 기준값
    public float groundZValue = 0f; 
    
    // ✨ 2. 뱀이 영역보다 얼마나 더 아래에 위치할지 결정하는 오프셋
    public float snakeZOffset = -0.5f; 

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    void Update()
    {
        if (targetPosition == Vector3.zero) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (!hasSpawnedSnake && Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            hasSpawnedSnake = true;

            // 🎨 1. 그림 (데미지 영역) 생성 (Z축 고정)
            if (imagePrefab != null)
            {
                Vector3 spawnPosition = transform.position;
                spawnPosition.z = groundZValue; 
                
                GameObject img = Instantiate(imagePrefab, spawnPosition, Quaternion.identity);
                
                DamageZone dz = img.GetComponent<DamageZone>();
                if (dz != null)
                {
                    dz.lifetime = 4f; 
                    dz.damagePerSecond = 3f;
                    Debug.Log("✅ 데미지 영역(그림) 생성 Z: " + spawnPosition.z);
                }
                else
                {
                    Destroy(img, 4f); 
                    Debug.LogError("DamageZone.cs 스크립트가 imagePrefab에 없습니다.");
                }
            }

            // 🐍 0.7초 뒤에 뱀 생성 코루틴 시작
            StartCoroutine(SpawnSnakeAfterDelay(0.7f));
        }
    }
    
    // ✨ 2. 뱀 생성 위치 조정 함수 (기존 로직 유지)
    private IEnumerator SpawnSnakeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (miniSnakePrefab != null)
        {
            Vector2 snakeSpawnPosition = transform.position;
            
            // 💡 뱀의 Z축 계산 로직 유지: groundZValue + snakeZOffset 사용

            Instantiate(miniSnakePrefab, snakeSpawnPosition, Quaternion.identity);
        }

        Destroy(gameObject); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Destroy(gameObject);
    }
}