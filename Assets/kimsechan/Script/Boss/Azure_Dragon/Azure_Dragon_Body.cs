using UnityEngine;
using DG.Tweening; // ✨ DOTween 사용을 위해 추가

public class Azure_Dragon_Body : MonoBehaviour
{
    public float Input_Damage = 20;
    
    [Header("Damage Effect Settings")]
    [Tooltip("피해 입었을 때 변할 색상")]
    public Color damageColor = Color.red; 
    [Tooltip("피해 효과 지속 시간 (빨간색으로 변했다 돌아오는 시간)")]
    public float flashDuration = 0.1f; 

    // 보스 매니저에 접근하기 위한 변수 (최적화를 위해 미리 찾아두는 것이 좋습니다)
    private BossManager bossManager;
    private SpriteRenderer spriteRenderer; // ✨ SpriteRenderer 컴포넌트

    void Start()
    {
        // 씬에서 BossManager 컴포넌트를 가진 오브젝트를 찾습니다.
        bossManager = FindObjectOfType<BossManager>();
        
        // ✨ SpriteRenderer 컴포넌트를 가져옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (bossManager == null)
        {
            Debug.LogError("씬에서 BossManager 오브젝트를 찾을 수 없습니다.");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer 컴포넌트가 Azure_Dragon_Body 오브젝트에 없습니다.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            // 몬스터의 몸체(BossBody)가 무기 태그를 가진 오브젝트와 충돌했을 때 실행

            if (bossManager != null)
            {
                bossManager.TakeDamage(Input_Damage);

                // ✨ 데미지 입었을 때 빨간색으로 변하는 시각 효과 실행
                FlashDamageEffect();

                Debug.Log($"무기 충돌! {this.gameObject.name}가 보스에게 {Input_Damage} 피해를 입혔습니다.");
            }
        }
    }
    
    private void FlashDamageEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill(true); 

            // 1. 색상을 damageColor (빨간색)로 즉시 변경
            spriteRenderer.color = damageColor;

            spriteRenderer.DOColor(Color.white, flashDuration);
        }
    }
}