using System.Collections;
using UnityEngine;

public class Ped_bird_Flooring : MonoBehaviour
{
    [Header("Duration")]
    public float bulletcontinue = 1f; // 오브젝트가 자동으로 사라질 시간
    
    [Header("Combat")]
    public int damage = 1;
    [Tooltip("지속 데미지(DoT) 간격")]
    public float damageInterval = 0.5f; // ✅ 추가: 데미지를 줄 간격 (0.5초마다)
    
    private Coroutine damageCoroutine;
    private bool isPlayerInside = false; // ✅ 추가: 플레이어가 영역 내에 있는지 확인하는 플래그

    private void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 시간 제한 코루틴 시작
        StartCoroutine(AutoDeactivate());
    }

    // 오브젝트의 생명 주기 관리 코루틴
    private IEnumerator AutoDeactivate()
    {
        // 설정된 시간(1초) 후 오브젝트 비활성화
        yield return new WaitForSeconds(bulletcontinue);
        
        // 장판이 사라질 때 데미지 코루틴이 실행 중이면 중지합니다.
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        this.gameObject.SetActive(false);
    }
    
    // 플레이어가 장판에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInside = true;
            // 플레이어가 들어오면 지속 데미지 코루틴을 시작합니다.
            damageCoroutine = StartCoroutine(DamageOverTime());
        }
    }

    // 플레이어가 장판에서 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInside = false;
            // 플레이어가 나가면 데미지 코루틴을 중지합니다.
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }
    }

    // 지속 데미지 코루틴
    private IEnumerator DamageOverTime()
    {
        // 플레이어가 영역 안에 있는 동안 반복
        while (isPlayerInside)
        {
            // 1. 데미지 간격만큼 대기
            yield return new WaitForSeconds(damageInterval);

            // 2. 데미지 적용
            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.take_Damage(damage);
            }
        }
    }
}
