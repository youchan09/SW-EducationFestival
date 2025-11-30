using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    public GameObject slowProjectilePrefab; 
    public Transform firePoint;             
    public Transform player;                

    public Animator animator;               
    public string attackTriggerName = "AttackTrigger"; 

    private float fireInterval = 3f;        
    private float fireTimer = 0f;
    private bool isAttacking = false;
    public Health bossHealth; 
    
    public Slider bossHpSlider; // ✨ Slider 연결 필드
    

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<Health>();

        // ✨ Health 스크립트에 UI 슬라이더 연결 및 초기화
        if (bossHealth != null && bossHpSlider != null)
        {
            bossHealth.hpSlider = bossHpSlider;
            
            // Health 스크립트의 Lerp 기능이 정상 작동하도록 Slider의 기본 설정을 보장
            bossHpSlider.maxValue = 1f;
            bossHpSlider.minValue = 0f;
            
            // 만약 시작 시 부드럽게 채워지는 기능을 원한다면, 
            // bossHpSlider.value를 0으로 설정하고 Health.Start()가 targetValue를 1로 설정하도록 맡깁니다.
            // 하지만 안정성을 위해 Health 스크립트의 Start()에 맡깁니다.
            
            Debug.Log("✅ BossController: Health 스크립트와 Slider 연결 완료.");
        }
    }
    
    // ... (나머지 함수는 기존 코드 유지)
    void Update()
    {
        fireTimer += Time.deltaTime;

        if (!isAttacking && fireTimer >= fireInterval)
        {
            Attack();
            fireTimer = 0f;
        }
    }

    void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
            isAttacking = true;
        }
    }

    public void FireSlowProjectile()
    {
        if (slowProjectilePrefab == null || firePoint == null || player == null)
            return;

        GameObject proj = Instantiate(slowProjectilePrefab, firePoint.position, Quaternion.identity);
        SlowProjectile sp = proj.GetComponent<SlowProjectile>();
        if (sp != null)
            sp.SetTarget(player.position);

        isAttacking = false;
    }
}