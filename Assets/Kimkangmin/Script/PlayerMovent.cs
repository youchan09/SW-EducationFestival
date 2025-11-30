using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float currentSpeed;
    private bool isSlowed = false;
    private float slowTimer = 0f;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        // 이동 입력
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        transform.Translate(dir * currentSpeed * Time.deltaTime, Space.World);

        // 슬로우 해제 타이머
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                isSlowed = false;
                currentSpeed = moveSpeed;
            }
        }
    }

    // 슬로우 적용 함수
    public void ApplySlow(float slowPercent, float duration)
    {
        if (isSlowed) return; // 이미 슬로우면 덮어쓰기 방지
        isSlowed = true;
        currentSpeed = moveSpeed * slowPercent;
        slowTimer = duration;
    }
}