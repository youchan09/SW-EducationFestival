using UnityEngine;
using UnityEngine.UI; 

public class BossPhaseManager : MonoBehaviour
{
    // ===========================================
    // 인스펙터 설정 변수
    // ===========================================
    [Header("페이즈 이동 설정")]
    [Tooltip("카메라와 플레이어가 이동할 목표 위치 (빈 게임 오브젝트)")]
    public Transform bossMoveTarget;

    [Header("보스 구성 요소")]
    [Tooltip("현무 본체의 모든 공격 스크립트(WaterCannon, ShockWave, HailStoneAttack)를 연결해야 합니다!")]
    public MonoBehaviour[] turtleAttackScripts; 

    [Tooltip("씬에 배치된 보스 HP 슬라이더 UI")]
    public Slider bossHPSlider;

    // ===========================================
    // 내부 변수
    // ===========================================
    private GameObject player;
    private GameObject mainCamera;
    
    private GameObject snakeObject; 
    private Health turtleHealth;
    private Health snakeHealth; 

    void Start()
    {
        // 1. 컴포넌트 및 오브젝트 찾기
        player = GameObject.FindWithTag("Player");
        mainCamera = Camera.main.gameObject;
        turtleHealth = GetComponent<Health>(); 
        snakeObject = GameObject.Find("Snake"); 

        if (snakeObject != null)
        {
            snakeHealth = snakeObject.GetComponent<Health>();
        }
        
        // 2. 현무 본체의 공격 스크립트 초기 비활성화 (뱀이 죽기 전까지 현무 공격 금지)
        foreach (MonoBehaviour script in turtleAttackScripts)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
        
        // 3. HP 슬라이더 초기 설정 및 가시성 제어
        if (bossHPSlider != null)
        {
            if (snakeHealth != null)
            {
                bossHPSlider.gameObject.SetActive(true); 
                snakeHealth.hpSlider = bossHPSlider;
            }
            else
            {
                bossHPSlider.gameObject.SetActive(false); 
            }
        }
    }

    // 뱀 사망 시 Health.cs에서 호출
    public void OnSnakeKilled()
    {
        Debug.Log("꼬리(뱀) 사망! 2페이즈(현무) 전환을 시작합니다.");
        
        // ⭐ 추가된 기능: 모든 MiniSnack 잡몹 제거 ⭐
        MiniSnack[] miniSnacks = FindObjectsOfType<MiniSnack>();
        foreach (MiniSnack snack in miniSnacks)
        {
            Destroy(snack.gameObject);
        }
        Debug.Log($"🧹 {miniSnacks.Length} 마리의 MiniSnack 잡몹이 제거되었습니다.");
        
        // 뱀 사망 시 HP 바를 즉시 안보이게 함
        if (bossHPSlider != null)
        {
            bossHPSlider.gameObject.SetActive(false);
        }
        
        // 1. HP 바 전환: 현무 본체의 HP 바로 연결
        if (turtleHealth != null && bossHPSlider != null)
        {
            turtleHealth.hpSlider = bossHPSlider;
            
            // 현무의 HP를 최대치로 설정
            turtleHealth.currentHP = turtleHealth.maxHP;
            
            // 기존 코드 유지
            bossHPSlider.value = turtleHealth.currentHP / turtleHealth.maxHP;
        }

        // 2. 현무 본체의 공격 스크립트 활성화 (2페이즈 시작)
        foreach (MonoBehaviour script in turtleAttackScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }
        
        // 3. 플레이어 및 카메라 순간 이동
        if (bossMoveTarget != null && player != null && mainCamera != null)
        {
            // 플레이어와 카메라를 목표 위치로 순간 이동
            player.transform.position = bossMoveTarget.position;
            mainCamera.transform.position = bossMoveTarget.position;
        }

        // 현무의 모든 설정이 완료된 후 HP 바를 다시 보이게 함
        if (bossHPSlider != null)
        {
            bossHPSlider.gameObject.SetActive(true);
        }
    }
}