using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    [HideInInspector] public Slider hpSlider; 
    public float smoothSpeed = 5f;
    private float targetValue; 

    void Start()
    {
        currentHP = maxHP;
        targetValue = 1f;
        if (hpSlider != null) 
            hpSlider.value = currentHP / maxHP; 
    }

    void Update()
    {
        if (hpSlider != null)
        {
            hpSlider.value = Mathf.Lerp(hpSlider.value, targetValue, Time.deltaTime * smoothSpeed);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        targetValue = currentHP / maxHP; 
        
        Debug.Log($"✅ 데미지 적용! 오브젝트: {gameObject.name}, New CurrentHP: {currentHP}");

        // ⭐ 핵심 조건: HP가 0 이하일 때만 사망 처리 ⭐
        if (currentHP <= 0)
            Die(); 
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");

        // 1. 꼬리(뱀) 사망 시 BossPhaseManager 호출 (페이즈 전환)
        bool isSnake = gameObject.name.Contains("Snake-slow_0") || gameObject.name.Contains("꼬리");
        if (isSnake) 
        {
            BossPhaseManager manager = FindObjectOfType<BossPhaseManager>(); 
            if (manager != null)
            {
                // 현무 스크립트는 여기서 즉시 작동 시작!
                manager.OnSnakeKilled(); 
            }
        }
        
        // 2. 사망 오브젝트의 모든 스크립트 비활성화
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) 
            {
                script.enabled = false;
            }
        }
        
        // 3. ⭐ 뱀 오브젝트 즉시 파괴 (지연 시간 제거) ⭐
        if (isSnake)
        {
            Destroy(gameObject); // 지연 시간 없이 즉시 파괴
        }
        else
        {
            Destroy(gameObject);
        }
    }
}