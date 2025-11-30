using UnityEngine;

public class Throw : Spear_fighter_SkillBase
{
    private bool Thrown = false; // 던졌는지 상태 체크
    public GameObject Prefab;    // 던질 창 프리팹
    public Transform throwPoint; // 창이 날아갈 시작 위치
    public float throwForce = 10f;

    public override void Activate()
    {
        // 쿨타임 체크 및 이미 던졌으면 return
        if (!isAvailable || Thrown) return;

        currentCooldown = cooldown;
        Thrown = true;

        // 창 생성
        GameObject thrownSpear = Instantiate(Prefab, throwPoint.position, Quaternion.identity);

        // Rigidbody2D로 날리기
        Rigidbody2D spearRb = thrownSpear.GetComponent<Rigidbody2D>();
        if (spearRb != null)
        {
            // 오른쪽으로 던지기 (localScale.x 방향 반영)
            Vector2 dir = throwPoint.right; 
            spearRb.AddForce(dir * throwForce, ForceMode2D.Impulse);
        }

        // TODO: 창을 다시 주워서 주먹 공격으로 바꾸는 로직은 나중에 추가
    }
}