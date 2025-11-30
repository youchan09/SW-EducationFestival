// Hailstone.cs (우박 프리팹에 부착)
using UnityEngine;

public class Hailstone : MonoBehaviour
{
    // HailstoneAttack.cs에서 플레이어의 Y 위치를 받아옵니다.
    [HideInInspector] public float destroyYBoundary; 

    // 우박이 플레이어 위치에 닿기 *직전*에 사라지도록 약간의 오프셋을 줍니다.
    private const float Y_OFFSET = 0.1f; 

    void Update()
    {
        // ⭐ 조건 2 충족: 우박의 Y 좌표가 설정된 파괴 경계(플레이어 Y 위치)에 도달하면 파괴 ⭐
        // 경계에서 Y_OFFSET만큼 위에서 사라지도록 설정하여 확실하게 소멸시킵니다.
        if (transform.position.y < destroyYBoundary + Y_OFFSET)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어에게 충돌 시 데미지 처리 및 파괴 (이 로직은 유지)
        if (other.CompareTag("Player"))
        {
            // TODO: 여기에 데미지 처리 로직을 추가
            
            // 우박 파괴
            Destroy(gameObject);
        }
    }
}