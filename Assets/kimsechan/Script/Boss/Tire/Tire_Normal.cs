using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Tire_Normal : Skill_based
{
    [Header("예고 설정")]
    public float warningTime = 1f; // 빨간색 예고 시간

    [Header("돌진 설정")]
    public float dashSpeed = 15f; // 돌진 속도
    public float dashDuration = 1f; // 돌진 지속 시간

    [Header("타겟 설정")]
    public Transform playerTarget; // 바라볼 대상

    private List<SpriteRenderer> childRenderers = new List<SpriteRenderer>();
    private List<Color> originalColors = new List<Color>();

    private Coroutine currentAttackCoroutine;

    void Start()
    {
        // 자식 오브젝트들의 SpriteRenderer와 원래 색상 저장
        childRenderers.Clear();
        originalColors.Clear();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer r in renderers)
        {
            if (r.gameObject != gameObject)
            {
                childRenderers.Add(r);
                originalColors.Add(r.color);
            }
        }

        if (childRenderers.Count == 0)
        {
            Debug.LogError("자식 SpriteRenderer가 없습니다.");
            enabled = false;
        }

        // 플레이어 타겟이 없으면 자동으로 찾아오기
        
    }

    public override void Attack()
    {
        if (currentAttackCoroutine != null)
            StopCoroutine(currentAttackCoroutine);

        currentAttackCoroutine = StartCoroutine(WarningAndDashSequence());
    }

    public override void StopAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        // DOTween으로 깜빡임 중이면 즉시 복구
        for (int i = 0; i < childRenderers.Count; i++)
        {
            if (childRenderers[i] != null)
            {
                childRenderers[i].DOKill();
                childRenderers[i].color = originalColors[i];
            }
        }
    }

    private IEnumerator WarningAndDashSequence()
    {
        for (int j = 0; j < 3; j++)
        {
                // 1. 빨간색 깜빡이기
                 foreach (var r in childRenderers)
                 {
                     if (r != null)
                     {
                         r.DOColor(Color.red, 0.1f)
                           .SetLoops(-1, LoopType.Yoyo)
                           .SetEase(Ease.Flash);
                     }
                 }
         
                 // 2. 예고 시간 대기
                 yield return new WaitForSeconds(warningTime);
         
                 // 3. 색상 원래대로 복구
                 for (int i = 0; i < childRenderers.Count; i++)
                 {
                     if (childRenderers[i] != null)
                     {
                         childRenderers[i].DOKill();
                         childRenderers[i].color = originalColors[i];
                     }
                 }
         
                 // 4. 플레이어를 향하는 방향 계산
                 Vector2 dashDir = Vector2.zero;
                 if (playerTarget != null)
                 {
                     Vector2 dir = (playerTarget.position - transform.position);
                     // 좌우 또는 상하 중 큰 축만 선택
                     if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                         dashDir = new Vector2(Mathf.Sign(dir.x), 0f);
                     else
                         dashDir = new Vector2(0f, Mathf.Sign(dir.y));
                 }
         
                 // 5. dashDuration 동안 한 방향으로 이동
                 float elapsed = 0f;
                 while (elapsed < dashDuration)
                 {
                     transform.Translate(dashDir * dashSpeed * Time.deltaTime);
                     elapsed += Time.deltaTime;
                     yield return null;
                 }
         
                 // 6. 공격 종료
                 currentAttackCoroutine = null;   
        }
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }
    }
}
