using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가

public class Red_bird_FireHp : MonoBehaviour
{
    // 가져온 BossManager 인스턴스를 저장할 변수
    private BossManager bossManager;
    
    private float FireHp = 100f;

    // ✅ 추가: 히트 효과를 위한 변수
    private Color originalColor = Color.white;          
    private Color hitColor; // 💡 추가: 지정된 피격 색상
    private Coroutine hitCoroutine;
    private SpriteRenderer sr;
    
    // 💡 방어력: 2페이즈 시 받는 데미지 감소율 (damage * defense)
    public float defense = 0.5f; 

    private void Start()
    {
        // 1. 씬에서 태그가 "Boss"인 오브젝트를 찾습니다.
        GameObject bossObject = GameObject.FindGameObjectWithTag("Boss");

        if (bossObject != null)
        {
            // 2. Boss 오브젝트에서 BossManager 컴포넌트를 가져옵니다.
            bossManager = bossObject.GetComponent<BossManager>();

            if (bossManager == null)
            {
                Debug.LogError("⚠️ 'Boss' 태그가 있는 오브젝트에서 BossManager 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("⚠️ 씬에서 'Boss' 태그가 지정된 오브젝트를 찾을 수 없습니다.");
        }
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
            if (!ColorUtility.TryParseHtmlString("#FF7373", out hitColor))
            {
                // 파싱 실패 시 기본값으로 Color.red 사용
                hitColor = Color.red; 
            }
        }
        
        // 💡 FireHp 초기화: BossManager가 초기화된 후 실행되어야 합니다.
        if (bossManager != null)
        {
             // 보스 HP의 1/10을 심장의 HP로 설정
            FireHp = bossManager.MaxHp / 10f; 
        } else {
             FireHp = 10f; // BossManager가 없으면 기본값으로 설정
        }

        
        StartCoroutine(Destroy());
    }

    public void OnHit(float damage)
    {
        float damageToBoss = damage;
        
        // 💡 [수정]: 페이즈 분기 로직 적용
        if (bossManager != null && !bossManager.Normal) // 2페이즈 (Normal == false)
        {
            // 2페이즈: 방어력 적용
            damageToBoss *= defense;
        }
        
        // --- 피격 효과 및 HP 처리 ---
        
        if (sr != null)
        {
            // 히트 이펙트 코루틴 중단 및 시작
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
            }
            hitCoroutine = StartCoroutine(HitEffect());
        }
        
        // 심장 HP 감소 (항상 원본 데미지 사용)
        FireHp -= damage; 

        // 보스 HP 감소 (페이즈에 따라 보정된 데미지 사용)
        if (bossManager != null)
        {
            bossManager.TakeDamage(damageToBoss);
        }

        if (FireHp <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    // ✅ 추가: 보스 매니저와 유사한 히트 이펙트 코루틴
    private IEnumerator HitEffect()
    {
        // 1. 심장을 즉시 지정된 색상으로 변경합니다.
        sr.color = hitColor;

        // 2. 잠시 대기합니다. (이 시간이 연속 피격 시 갱신됨)
        yield return new WaitForSeconds(0.1f);
        
        // 3. 원래 색상으로 되돌립니다.
        sr.color = originalColor;
        
        // 4. 코루틴이 완전히 종료되었음을 표시합니다.
        hitCoroutine = null;
    }

    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(4.5f);
        
        // 💡 심장이 자연적으로 사라질 때 코루틴이 남아있으면 정리합니다.
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }
        
        Destroy(gameObject);
    }
}
