using UnityEngine;
using DG.Tweening; // DOTween 필요

public class BossHit : MonoBehaviour
{
    [Header("보스에게 줄 피해량")]
    public float damage = 10f;

    [Header("Damage Effect Settings")]
    public Color damageColor = Color.red; // 피격 시 색상
    public float flashDuration = 0.1f;    // 색상 변경 지속 시간

    private BossManager bossManager;
    private SpriteRenderer spriteRenderer; // 보스의 SpriteRenderer

    private void Start()
    {
        // 씬에서 BossManager 찾기
        bossManager = FindObjectOfType<BossManager>();
        if (bossManager == null)
            Debug.LogError("씬에 BossManager 오브젝트가 존재하지 않습니다!");

        // SpriteRenderer 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("BossHit 오브젝트에 SpriteRenderer가 없습니다!");
    }

    // 2D 일반 충돌 감지 (isTrigger 꺼져 있을 때)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Weapon") && bossManager != null)
        {
            // HP 감소
            bossManager.TakeDamage(damage);

            // 피격 시 색상 변경
            FlashDamageEffect();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Weapon")  && bossManager != null)
        {
            // HP 감소
            bossManager.TakeDamage(damage);

            // 피격 시 색상 변경
            FlashDamageEffect();
        }
    }

    private void FlashDamageEffect()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.DOKill(true);      // 기존 DOTween 효과 종료
        spriteRenderer.color = damageColor; // 빨간색으로 즉시 변경
        spriteRenderer.DOColor(Color.white, flashDuration); // 원래 색으로 복귀
    }
}