using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 3f;      // 공격이 닿는 거리
    public float attackDamage = 15f;    // 공격 데미지
    
    [Header("타겟 레이어 설정")]
    [Tooltip("주요 타겟 (예: 보스)이 포함된 레이어")]
    public LayerMask primaryTargetLayer;       // ⭐ 기존 targetLayer를 primaryTargetLayer로 변경 ⭐
    
    [Tooltip("보조 타겟 (예: 잡몹)이 포함된 레이어")]
    public LayerMask secondaryTargetLayer;     // ⭐ 새로 추가된 보조 타겟 레이어 ⭐


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("스페이스바 입력 감지! 2D 공격 시도..."); 
            TryAttackTarget(); 
        }
    }

    void TryAttackTarget()
    {
        // ⭐ 두 레이어를 OR 연산하여 하나의 공격 마스크로 통합 ⭐
        int combinedLayerMask = primaryTargetLayer.value | secondaryTargetLayer.value;

        // Physics2D.OverlapCircleAll을 사용하여 공격 범위 내의 모든 대상을 감지합니다.
        Collider2D[] hits2D = Physics2D.OverlapCircleAll(transform.position, attackRange, combinedLayerMask);

        if (hits2D.Length == 0)
        {
            Debug.LogWarning($"❌ 2D 콜라이더를 찾지 못했습니다. (타겟 레이어 설정과 콜라이더를 확인하세요.)");
        }

        foreach (Collider2D hit in hits2D)
        {
            Health targetHealth = hit.GetComponent<Health>(); 
            
            if (targetHealth != null)
            {
                Debug.Log($"✅ 타겟 감지 성공: {hit.gameObject.name}"); 
                targetHealth.TakeDamage(attackDamage);
                Debug.Log($"✅ 데미지 전달 성공! {hit.gameObject.name}에게 {attackDamage} 데미지를 입힘!");
            }
            else
            {
                Debug.LogError($"❌ Health 컴포넌트를 찾을 수 없습니다. {hit.gameObject.name}는 공격 대상이 아닙니다.");
            }
        }
    }

    // Scene 뷰에서 공격 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
}