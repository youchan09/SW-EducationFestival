using UnityEngine;
using DG.Tweening;

public class NormalAttack : Spear_fighter_SkillBase
{
    private Rigidbody2D rb;
    public Transform playerPos;
    private bool isAttacking;
    private bool isReturning;

    public float damage = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Activate()
    {
        if (!isAvailable || isAttacking) return;

        currentCooldown = cooldown;
        isAttacking = true;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Vector2 dir = transform.up * transform.localScale.x;
        rb.AddForce(dir * 3, ForceMode2D.Impulse);

        DOVirtual.DelayedCall(0.2f, () =>
        {
            rb.linearVelocity = Vector2.zero;
            isReturning = true;
        });
    }

    private void Update()
    {
        UpdateCooldown();

        if (isReturning && playerPos != null)
        {
            Vector3 cur = transform.position;
            Vector3 target = new Vector3(playerPos.position.x, playerPos.position.y, cur.z);

            transform.position = Vector3.Lerp(cur, target, Time.deltaTime * 10f);

            if (Vector2.Distance(transform.position, playerPos.position) < 0.1f)
            {
                isReturning = false;
                isAttacking = false;
                rb.constraints = RigidbodyConstraints2D.None;
            }
        }
    }

    // 🔹 벽용 일반 충돌 감지
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("벽에 닿음 → 복귀 시작");
            rb.linearVelocity = Vector2.zero;
            isReturning = true;
        }
    }
}