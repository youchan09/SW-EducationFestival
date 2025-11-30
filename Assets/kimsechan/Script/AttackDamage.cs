using System.Collections;
using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    [Tooltip("플레이어에게 입힐 데미지 값")]
    public float damageAmount = 10f;
    
    [Header("데미지 간격 설정")]
    [Tooltip("데미지를 다시 입히기까지의 쿨다운 시간")]
    public float damageCooldown = 0.5f; // 쿨다운 시간 변수로 사용

    // 데미지를 입힐 수 있는 상태 (true일 때만 데미지 적용 가능)
    private bool canDamage = true; 

    // isTrigger 콜라이더와 다른 오브젝트가 충돌했을 때 호출됩니다.
    private void OnTriggerStay2D(Collider2D other)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // PlayerManager 컴포넌트를 가져옵니다.
            PlayerManager playerManager = other.GetComponent<PlayerManager>();

            // 플레이어 매니저가 있고, 현재 데미지를 입힐 수 있는 상태(canDamage가 true)인지 확인합니다.
            if (playerManager != null && canDamage)
            {
                // 1. 데미지 적용
                playerManager.take_Damage(damageAmount);
                Debug.Log($"플레이어에게 {damageAmount} 데미지를 입혔습니다.");
                
                // 2. 쿨다운 상태로 전환하고 코루틴 시작 (오직 한 번만 시작)
                canDamage = false;
                StartCoroutine(DamageCooldownRoutine());
                
            }
            // else: canDamage가 false이거나 PlayerManager가 없으므로 아무것도 하지 않습니다.
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerManager 컴포넌트를 가져옵니다.
            PlayerManager playerManager = collision.gameObject.GetComponent<PlayerManager>();

            // 플레이어 매니저가 있고, 현재 데미지를 입힐 수 있는 상태(canDamage가 true)인지 확인합니다.
            if (playerManager != null && canDamage)
            {
                // 1. 데미지 적용
                playerManager.take_Damage(damageAmount);
                Debug.Log($"플레이어에게 {damageAmount} 데미지를 입혔습니다.");
                
                // 2. 쿨다운 상태로 전환하고 코루틴 시작 (오직 한 번만 시작)
                canDamage = false;
                StartCoroutine(DamageCooldownRoutine());
                
            }
            // else: canDamage가 false이거나 PlayerManager가 없으므로 아무것도 하지 않습니다.
        }
    }

    // 데미지 쿨다운을 관리하는 코루틴
    IEnumerator DamageCooldownRoutine()
    {
        // 지정된 시간(damageCooldown) 동안 대기
        yield return new WaitForSeconds(damageCooldown);
        
        // 쿨다운이 끝났으므로 다시 데미지를 입힐 수 있도록 상태 변경
        canDamage = true;
    }
    
    // 🟢 오브젝트 풀링을 사용하는 경우, 재활성화될 때 canDamage를 초기화해야 합니다.
    private void OnEnable()
    {
        canDamage = true;
    }
}