using UnityEngine;

public class ObjectRotater : MonoBehaviour
{
    public Transform weapon; // 회전할 자식 오브젝트

    void Update()
    {
        if (weapon == null) return;

        // 마우스의 월드 좌표 얻기
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 플레이어(부모)의 위치
        Vector2 playerPos = transform.position;

        // 방향 벡터 = 마우스 - 플레이어
        Vector2 direction = (Vector2)(mouseWorldPos - (Vector3)playerPos);

        // 방향 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // weapon 회전 (스프라이트가 위쪽을 바라본다면 -90도 보정)
        weapon.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }
}