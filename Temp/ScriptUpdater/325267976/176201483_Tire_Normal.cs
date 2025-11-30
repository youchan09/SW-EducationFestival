using UnityEngine;
using System.Collections;
using DG.Tweening; // DOTween 사용을 위해 필요합니다.
using System.Collections.Generic; // 리스트 사용을 위해 추가

// Skill_based 추상 클래스는 수정하지 않습니다.
public class Tire_Normal : Skill_based
{
    // --- 인스펙터에서 설정할 변수 ---

    [Header("깜빡임 설정")]
    public float warningTime = 1f; // 깜빡이는 시간 간격 (예고 시간)

    [Header("돌진 설정")]
    public float dashForce = 2000f; // 돌진에 사용할 힘 (AddForce에 사용)
    public float dashDuration = 0.3f; // 돌진이 지속될 시간

    // 스크립트가 붙은 오브젝트의 자식들 중 SpriteRenderer를 가진 모든 오브젝트를 저장합니다.
    private List<SpriteRenderer> childRenderers = new List<SpriteRenderer>();
    private List<Color> originalColors = new List<Color>();
    
    // 현재 진행 중인 코루틴을 저장합니다.
    private Coroutine currentWarningCoroutine; 
    
    // 돌진 로직 변수
    private Rigidbody2D rb;
    private Vector2 dashDirection;
    private bool isDashing = false;

    void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기 (돌진에 필수)
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 컴포넌트가 필요합니다. 이 오브젝트에 Rigidbody2D를 추가해주세요!");
            enabled = false; 
            return;
        }
        
        // 1. 자식 오브젝트에서 모든 SpriteRenderer와 그 색상을 찾습니다.
        childRenderers.Clear();
        originalColors.Clear();
        
        // 자신을 제외한 모든 자식 오브젝트의 SpriteRenderer를 가져옵니다.
        // GetComponentsInChildren(true)는 비활성화된 자식도 포함합니다.
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        
        // 자신(부모)의 SpriteRenderer를 제외하고 자식들만 리스트에 추가합니다.
        foreach (SpriteRenderer r in renderers)
        {
            if (r.gameObject != gameObject) // 자신(스크립트가 붙은 오브젝트)이 아닐 경우
            {
                childRenderers.Add(r);
                originalColors.Add(r.color); // 원래 색상 저장
            }
        }

        if (childRenderers.Count == 0)
        {
            Debug.LogError("이 오브젝트의 자식 오브젝트 중 SpriteRenderer를 가진 오브젝트가 없습니다. 확인해주세요.");
        }
    }

    // Skill_based 추상 메서드 구현: 깜빡임 시작 및 돌진 준비
    public override void Attack()
    {
        if (rb == null) return; // Rigidbody가 없으면 실행 불가
        if (isDashing) return; // 이미 돌진 중이면 무시
        
        // 랜덤으로 상하 또는 좌우 중 한 방향 선택 (돌진 방향)
        dashDirection = ChooseRandomDirection();
        
        // 이전에 실행 중이던 코루틴이 있다면 안전하게 멈춥니다.
        if (currentWarningCoroutine != null) StopCoroutine(currentWarningCoroutine);
            
        // 깜빡임 코루틴 시작
        currentWarningCoroutine = StartCoroutine(ColorWarningSequence());
    }

    // Skill_based 추상 메서드 구현: 깜빡임 중지 및 색상 복구 + 돌진 중지
    public override void StopAttack()
    {
        // 1. 코루틴 중지
        if (currentWarningCoroutine != null)
        {
            StopCoroutine(currentWarningCoroutine);
            currentWarningCoroutine = null;
        }

        // 2. DOTween 애니메이션 중지 및 초기 색상 복구
        for (int i = 0; i < childRenderers.Count; i++)
        {
            SpriteRenderer r = childRenderers[i];
            if (r != null)
            {
                r.DOKill(); // 현재 진행 중인 DOTween 애니메이션 중지
                // 인덱스 i를 사용하여 정확한 원래 색상으로 복구
                if (i < originalColors.Count) 
                {
                    r.color = originalColors[i]; 
                }
            }
        }
        
        // 3. 돌진 중지 시 Rigidbody 속도 초기화 (돌진 중지)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 속도 초기화
            rb.angularVelocity = 0f; // 회전 속도도 혹시 모르니 초기화
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

    private IEnumerator ColorWarningSequence()
    {
        // 깜빡임 시작
        isDashing = true; // 깜빡임 시작과 동시에 플래그 설정
        
        // 1. 예고: DOTween을 사용하여 빨갛게 깜빡이거나 즉시 변경
        foreach (SpriteRenderer r in childRenderers)
        {
            if (r != null)
            {
                // 색상을 warningTime 동안 빨간색으로 빠르게 깜빡이게 설정
                r.DOColor(Color.red, 0.1f) 
                  .SetLoops(-1, LoopType.Yoyo) // 무한 반복 (빨강 <-> 원래색)
                  .SetEase(Ease.Flash); // 예고 효과를 위해 Ease.Flash 사용
            }
        }

        // 2. warningTime 만큼 대기 (이 시간 동안 깜빡입니다)
        yield return new WaitForSeconds(warningTime);

        // --- 돌진 단계 시작 ---
        
        // 3. DOTween 애니메이션 중지 및 색상 복구 (돌진 직전)
        // StopAttack 로직을 여기서 부분적으로 실행
        for (int i = 0; i < childRenderers.Count; i++)
        {
            SpriteRenderer r = childRenderers[i];
            if (r != null)
            {
                r.DOKill(); 
                if (i < originalColors.Count) 
                {
                    r.color = originalColors[i]; 
                }
            }
        }

        // 4. 돌진 시작 (AddForce 사용 - 요청 사항)
        if (rb != null)
        {
            // 원하는 방향으로 Impulse 모드를 사용하여 순간적으로 큰 힘을 가합니다.
            rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        }
        
        // 5. dashDuration 동안 돌진을 유지
        yield return new WaitForSeconds(dashDuration);
        
        // 6. 돌진 종료 및 정리
        StopAttack();
    }
}