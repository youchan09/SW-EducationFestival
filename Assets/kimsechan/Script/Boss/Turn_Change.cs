using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn_Change : MonoBehaviour
{
    [Header("Phase 1 Settings")]
    // [수정]: Red_bird_SkilBase -> Skill_based
    public List<Skill_based> attackPatterns = new List<Skill_based>();
    public float delayBetweenAttacks = 1f; // 패턴 사이 딜레이 (페이즈 1)

    [Header("Phase 2 Settings")]
    // [수정]: Red_bird_SkilBase -> Skill_based
    public List<Skill_based> skillAttackPatterns = new List<Skill_based>();
    public float delayBetweenSkillAttacks = 0.5f; // 패턴 사이 딜레이 (페이즈 2)

    private BossManager boss;
    [HideInInspector] public bool isAttacking = false; 
    
    private Coroutine mainLoopCoroutine;
    // [수정]: Red_bird_SkilBase -> Skill_based
    private Skill_based currentPattern;

    private void Start()
    {
        boss = GetComponent<BossManager>();
        
        // ❌ [수정]: Start()에서 공격 루프를 자동으로 시작하지 않습니다. 
        // 보스전 시작은 BossManager의 StartBossEncounter() 호출에 의해 제어됩니다.
    }

    /// <summary>
    /// 1페이즈 공격 루프를 시작합니다. BossManager.StartBossEncounter()에서 호출됩니다.
    /// </summary>
    public void StartPhaseOneLoop()
    {
        if (mainLoopCoroutine != null) return;
        
        Debug.Log("Turn_Change: 1페이즈 공격 루프 시작!");
        mainLoopCoroutine = StartCoroutine(BossAttackPhaseLoop());
    }

    // 외부(BossManager)에서 호출하여 공격 루프를 강제로 중지하는 메서드
    public void StopAttackLoop()
    {
        if (mainLoopCoroutine != null)
        {
            StopCoroutine(mainLoopCoroutine);
            mainLoopCoroutine = null;
        }
        if (currentPattern != null)
        {
            // 패턴을 중지하고 바로 참조를 해제합니다.
            currentPattern.StopAttack(); 
            currentPattern = null;
        }
        isAttacking = false; 
        Debug.Log("Turn_Change: 시네마틱을 위해 공격 루프 강제 중지됨.");
    }
    
    public void StartPhaseTwoLoop()
    {
        if (mainLoopCoroutine == null)
        {
            mainLoopCoroutine = StartCoroutine(PhaseTwoContinuation());
        }
    }
    
    private IEnumerator PhaseTwoContinuation()
    {
        yield return new WaitUntil(() => boss != null); 
        
        Debug.Log("Turn_Change: 2페이즈 공격 루프 재시작!");
        
        yield return StartCoroutine(RunAttackLoop(
            skillAttackPatterns, delayBetweenSkillAttacks, () => !boss.Normal && boss.Hp > 0
        ));

        mainLoopCoroutine = null;
        Debug.Log("Turn_Change: 공격 루프 종료.");
    }

    private IEnumerator BossAttackPhaseLoop()
    {
        yield return new WaitUntil(() => boss != null);
        
        yield return StartCoroutine(RunAttackLoop(
            attackPatterns, delayBetweenAttacks, () => boss.Normal
        ));
        mainLoopCoroutine = null;
    }

    // [수정]: List<Red_bird_SkilBase> -> List<Skill_based>
    private IEnumerator RunAttackLoop(
        List<Skill_based> patterns, 
        float delay,
        System.Func<bool> condition)
    {
        int index = 0;

        while (condition.Invoke())
        {
            if (patterns.Count == 0)
            {
                Debug.LogError($"Turn_Change: 현재 페이즈의 패턴 목록이 비어 있습니다. 인스펙터 설정을 확인해주세요.");
                yield return new WaitForSeconds(1f); 
                continue;
            }

            // [수정]: 패턴 타입
            Skill_based pattern = patterns[index];
            currentPattern = pattern; 
            
            // 패턴 실행 (이 시점에서 BossManager.OnSkill이 true가 됩니다.)
            pattern.Attack();

            // 스킬 코루틴이 끝날 때까지 기다립니다. (boss.OnSkill이 false가 될 때까지)
            yield return new WaitUntil(() => !boss.OnSkill || !condition.Invoke() || boss.IsPlayerInputLocked);

            // 중단/페이즈 전환 조건 확인
            if (!condition.Invoke() || boss.IsPlayerInputLocked)
            {
                if (currentPattern != null)
                {
                    currentPattern.StopAttack();
                }
                isAttacking = false; 
                currentPattern = null;
                Debug.Log("Turn_Change: 루프 중단 (전환 또는 시네마틱 잠금).");
                break;
            }
            
            // 다음 공격 준비
            isAttacking = false; 
            
            index++;
            if (index >= patterns.Count)
                index = 0;

            yield return new WaitForSeconds(delay);
        }
        
        isAttacking = false;
        currentPattern = null; 
    }
}