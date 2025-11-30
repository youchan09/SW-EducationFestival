using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

    public void Initialize(Vector3 direction, float moveSpeed)
    {
        moveDirection = direction;
        speed = moveSpeed;
        // 일정 시간 후 스스로 파괴 (예: 5초 후)
        Destroy(gameObject, 5f); 
    }

    void Update()
    {
        // Rigidbody를 사용하지 않고 Update에서 직접 이동
        if (speed > 0)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    // 충돌 처리 (옵션)
    private void OnTriggerEnter(Collider other)
    {
        // 예시: 플레이어와 충돌 시 파괴
        if (other.CompareTag("Player"))
        {
            // 플레이어에게 피해를 주는 로직
            Destroy(gameObject);
        }
        // 다른 것에 부딪혀도 파괴
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}