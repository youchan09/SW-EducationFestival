using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween 사용을 위해 필요합니다.

public class Tire_Normal : Skill_based
{
    // --- 인스펙터에서 설정할 변수 ---

    [Header("이동 속도")]
    public float dashSpeed = 10f; // 돌진 속도

    [Header("돌진 예고 시간")]
    public float warningTime = 1f; // 돌진 전 대기 시간

    [Header("SpriteRenderer (색상 변경 대상)")]
    public SpriteRenderer bodyRenderer; // 보스의 몸체 SpriteRenderer

    [Header("돌진 설정")]
    [Tooltip("돌진 후 원래 위치로 돌아오거나 멈출지 결정")]
    public bool returnAfterDash = false; 

    private Vector2 dashDirection;
    private bool isDashing = false;
    private Color originalColor;
    
    // Rigidbody2D를 사용하여 물리적인 움직임을 처리합니다.
    private Rigidbody2D rb;
    
    // 현재 진행 중인 코루틴을 저장하여 StopAttack에서 멈출 수 있게 합니다.
    private Coroutine currentDashCoroutine; 

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 컴포넌트가 필요합니다. 이 오브젝트에 Rigidbody2D를 추가해주세요!");
            // Rigidbody2D가 없으면 스크립트 실행을 중단합니다.
            enabled = false; 
            return;
        }

        if (bodyRenderer != null)
        {
            // 원래 색상 저장 (보통 흰색)
            originalColor = bodyRenderer.color;
        }
        else
        {
            Debug.LogError("Body Renderer가 연결되지 않았습니다. 인스펙터를 확인해주세요.");
        }
    }

    // Skill_based 추상 메서드 구현: 스킬 공격 시작
    public override void Attack()
    {
        if (!isDashing)
        {
            // 랜덤으로 상하 또는 좌우 중 한 방향 선택
            dashDirection = ChooseRandomDirection();
            
            // 이전에 실행 중이던 코루틴이 있다면 안전하게 멈춥니다.
            if (currentDashCoroutine != null) StopCoroutine(currentDashCoroutine);
            
            // 돌진 예고 코루틴 시작
            currentDashCoroutine = StartCoroutine(DashWithWarning());
        }
    }

    // Skill_based 추상 메서드 구현: 스킬 공격 중지
    public override void StopAttack()
    {
        // 1. 코루틴 중지
        if (currentDashCoroutine != null)
        {
            StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = null;
        }

        // 2. DOTween 애니메이션 중지 및 초기 색상 복구
        if (bodyRenderer != null)
        {
            bodyRenderer.DOKill(); // 현재 진행 중인 DOTween 애니메이션 중지
            bodyRenderer.color = originalColor; // 원래 색상으로 즉시 복구
        }
        
        // 3. 돌진 중지 시 Rigidbody 속도 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // velocity로 수정
        }

        isDashing = false;
    }

    private Vector2 ChooseRandomDirection()
    {
        // 0:위, 1:아래, 2:왼쪽, 3:오른쪽
        int dir = Random.Range(0, 4); 
        switch (dir)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.right; // 안전을 위한 기본값
        }
    }

    private IEnumerator DashWithWarning()
    {
        isDashing = true;

        // 1. 예고: DOTween을 사용하여 빨갛게 깜빡이거나 즉시 변경
        if (bodyRenderer != null)
        {
            // 색상을 warningTime 동안 빨간색으로 빠르게 변경
            bodyRenderer.DOColor(Color.red, 0.1f) 
                        .SetLoops(-1, LoopType.Yoyo) // 무한 반복 (빨강 <-> 원래색)
                        .SetEase(Ease.Flash); // 예고 효과를 위해 Ease.Flash 사용
        }

        // 2. warningTime 만큼 대기
        yield return new WaitForSeconds(warningTime);

        // 3. DOTween 애니메이션 중지 및 색상 복구 (돌진 직전)
        if (bodyRenderer != null)
        {
            bodyRenderer.DOKill(); // 색상 애니메이션 중지
            bodyRenderer.color = originalColor; // 원래 색상으로 되돌리기
        }

        // 4. 돌진 시작 (Rigidbody 사용)
        if (rb != null)
        {
            // 원하는 방향으로 속도를 설정합니다.
            rb.linearVelocity = dashDirection * dashSpeed; // velocity로 수정
        }
        
        // 돌진이 끝나는 지점이나 조건이 아직 없으므로,
        // isDashing 플래그가 StopAttack에 의해 꺼질 때까지 돌진을 유지합니다.
        while (isDashing) 
        {
            // Rigidbody를 사용하므로 이 루프는 상태를 유지하는 역할만 합니다.
            // 여기에 돌진 거리 제한/충돌 검사 로직 등을 추가해야 합니다.
            yield return null;
        }
        
        // 5. 돌진이 멈춘 후 정리 (속도 초기화)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // velocity로 수정
        }
        isDashing = false;
    }
}