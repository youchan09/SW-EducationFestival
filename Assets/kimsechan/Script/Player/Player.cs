using UnityEngine;

public class Player : MonoBehaviour
{
    private Red_bird_FireHp bossHp;
    
    void FixedUpdate()
    {
        _Move();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bossHp = FindAnyObjectByType<Red_bird_FireHp>(); 
            
            if (bossHp != null)
            {
                // 데미지 10 하드코딩 사용
                bossHp.OnHit(10);
                Debug.Log("Fire에게 데미지 10 적용");
            }
            else
            {
                Debug.LogWarning("Fire를 찾을 수 없습니다! (Start 시점 이후 심장이 파괴되었거나 아직 생성되지 않았을 수 있습니다)");
            }
        }
    }

    void _Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        // PlayerManager.instance.speed에 의존하는 이동 로직
        transform.position += move.normalized * Time.deltaTime * PlayerManager.instance.speed;
    }
}