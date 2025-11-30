using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BossSkillClass
{
    public string name;           
    public GameObject skills;     
    public int Damage = 10;
    public int poolSize = 20;     

    [HideInInspector] public Queue<GameObject> poolQueue;

    public void InitializePool(Transform parent)
    {
        if (skills == null)
        {
            Debug.LogWarning($"⚠️ BossSkillClass '{name}'에 스킬 프리팹이 비어 있습니다!");
            return;
        }

        poolQueue = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = GameObject.Instantiate(skills);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public void DeactivateAllObjects()
    {
        if (poolQueue == null) return;

        foreach (GameObject obj in poolQueue)
        {
            if (obj != null && obj.activeSelf)
                obj.SetActive(false);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (poolQueue == null || poolQueue.Count == 0)
        {
            Debug.LogWarning($"⚠️ {name} 풀에 남은 오브젝트가 없습니다! 새로 생성합니다.");
            return GameObject.Instantiate(skills, position, rotation);
        }

        GameObject obj = poolQueue.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        poolQueue.Enqueue(obj);

        return obj;
    }
}

public class BossManager : MonoBehaviour
{
    [Header("Boss Settings")]
    public float MaxHp = 100f;
    private float CurrentHp;
    private Color originalColor = Color.white;
    protected bool isPhaseTwo = false;

    protected bool isEncounterStarted = false; 

    [Tooltip("최대 HP 대비 2페이즈 진입 비율 (예: 0.5f는 50% HP)")]
    public float PhaseTwoThreshold = 0.5f;
    
    [Tooltip("시네마틱 시 보스를 비추는 시간")]
    public float CinematicHoldDuration = 3.0f; 
    
    [Tooltip("보스전 시작 후 첫 공격까지의 딜레이")]
    public float initialAttackDelay = 1.0f;

    [Tooltip("스킬 발동(OnSkill=true) 후 다음 공격까지의 딜레이 (스킬 지속 시간 포함)")]
    public float AttackCycleDuration = 5.0f;

    [Header("Skill Pooling")]
    public List<BossSkillClass> bossSkills = new List<BossSkillClass>();

    public Image Hpbar; 
    [HideInInspector] public bool OnSkill = false;
    [HideInInspector] public int SkillRand = 0;
    [HideInInspector] public bool Normal = true;
    [HideInInspector] public bool IsPlayerInputLocked = false;
    [HideInInspector] public GameObject battleWall;

    private Coroutine hitCoroutine;
    protected Coroutine skillCoolCoroutine;
    private Transform playerTarget;
    protected Turn_Change turnChange;

    public float Hp
    {
        get => CurrentHp;
        set
        {
            CurrentHp = Mathf.Clamp(value, 0, MaxHp);
            float targetFill = CurrentHp / MaxHp;

            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitEffect());

            DOTween.Kill(Hpbar);
            DOTween.To(() => Hpbar.fillAmount,
                       x => Hpbar.fillAmount = x,
                       targetFill, 0.5f).SetEase(Ease.OutCubic);

            if (CurrentHp <= MaxHp * PhaseTwoThreshold && !isPhaseTwo)
            {
                Normal = false;
                isPhaseTwo = true;
                OnPhaseTwoStart();
                StartCoroutine(PhaseTwoCinematic(false));
            }
            if (CurrentHp <= 0 && isEncounterStarted) 
            {
                StartDeathSequence();
            }
        }
    }

    private void Awake()
    {
        CurrentHp = MaxHp;

        if (battleWall == null)
        {
            GameObject wallObj = GameObject.FindGameObjectWithTag("Wall");
            if (wallObj != null)
            {
                battleWall = wallObj;
                battleWall.SetActive(false);
            }
            else
            {
                Debug.LogWarning("⚠️ 태그 'Wall'을 가진 오브젝트를 찾지 못했습니다!");
            }
        }
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;
        else
            Debug.LogError("Player 오브젝트(태그: Player)를 찾을 수 없습니다!");

        turnChange = GetComponent<Turn_Change>();
        if (turnChange == null)
            Debug.LogError("Turn_Change 컴포넌트를 찾을 수 없습니다.");

        foreach (var skill in bossSkills)
            skill.InitializePool(this.transform);

        // HP바 처음에 비활성화
        if (Hpbar != null)
            Hpbar.gameObject.SetActive(false);
    }

    protected virtual void Start() { }

    public void StartBossEncounter()
    {
        if (isEncounterStarted) return;
        
        isEncounterStarted = true;
        
        if (battleWall != null)
        {
            battleWall.SetActive(true);
        }

        // HP바 켜기
        if (Hpbar != null)
            Hpbar.gameObject.SetActive(true);

        StartCoroutine(PhaseTwoCinematic(false)); 
    }

    protected virtual void OnPhaseTwoStart() { }

    private IEnumerator PhaseTwoCinematic(bool isDying)
    {
        if (skillCoolCoroutine != null)
            StopCoroutine(skillCoolCoroutine);
        if (turnChange != null)
            turnChange.StopAttackLoop();

        foreach (var skill in bossSkills)
            skill.DeactivateAllObjects();

        OnSkill = true; 
        IsPlayerInputLocked = true; 

        Camera mainCam = Camera.main;
        bool wasChild = false;

        if (mainCam != null)
        {
            if (mainCam.transform.parent != null && mainCam.transform.parent == playerTarget)
            {
                mainCam.transform.SetParent(null);
                wasChild = true;
            }

            Vector3 bossPos = transform.position;
            Vector3 targetPos = new Vector3(bossPos.x, bossPos.y, mainCam.transform.position.z);

            yield return mainCam.transform.DOMove(targetPos, 1.0f)
                .SetEase(Ease.InOutSine).WaitForCompletion();
            
            if (isDying || !Normal) 
            {
                 transform.DOShakePosition(0.4f, strength: new Vector3(0.3f, 0, 0), vibrato: 20, randomness: 90, fadeOut: true);
            }

            if(!isDying) yield return new WaitForSeconds(CinematicHoldDuration); 

            if (wasChild && playerTarget != null)
            {
                mainCam.transform.SetParent(playerTarget);
                mainCam.transform.localPosition = new Vector3(0, 0, -5.9f);
            }
            else if (playerTarget != null)
            {
                Vector3 playerCamPos = new Vector3(playerTarget.position.x, playerTarget.position.y, mainCam.transform.position.z);
                yield return mainCam.transform.DOMove(playerCamPos, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
            }
        }

        if (!isDying)
        {
            yield return new WaitForSeconds(1.0f);
            OnSkill = false;
            IsPlayerInputLocked = false;

            skillCoolCoroutine = StartCoroutine(SkillCool()); 
            
            if (turnChange != null)
            {
                if (Normal)
                    turnChange.StartPhaseOneLoop(); 
                else
                    turnChange.StartPhaseTwoLoop(); 
            }
        }
    }

    protected IEnumerator DieCoroutine()
    {
        isEncounterStarted = false;

        yield return StartCoroutine(PhaseTwoCinematic(true)); 

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        const float fadeDuration = 1.0f;

        if (sr != null)
        {
            sr.color = Color.red;
            yield return sr.DOColor(Color.white, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
            sr.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(fadeDuration + 0.1f); 

        Destroy(gameObject);

        if (battleWall != null)
            battleWall.SetActive(false);

        if (Hpbar != null)
            Hpbar.gameObject.SetActive(false); // 보스 사망 시 HP바 끔
    }

    protected IEnumerator SkillCool()
    {
        yield return new WaitForSeconds(initialAttackDelay);

        while (true)
        {
            OnSkill = true; 
            SkillRand = Random.Range(0, bossSkills.Count);
            yield return new WaitForSeconds(AttackCycleDuration); 
            OnSkill = false; 
            yield return new WaitForSeconds(AttackCycleDuration); 
        }
    }

    public void StartDeathSequence()
    {
        StartCoroutine(DeathShakeAndDie());
    }

    private IEnumerator DeathShakeAndDie()
    {
        StartCoroutine(DieCoroutine());
        yield break;
    }

    public void TakeDamage(float damage)
    {
        if (CurrentHp <= 0 || !isEncounterStarted) return; 
        Hp -= damage;
    }

    private IEnumerator HitEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            hitCoroutine = null;
            yield break;
        }

        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;

        hitCoroutine = null;
    }

    public GameObject UseSkill(int index, Vector3 position, Quaternion rotation)
    {
        if (index < 0 || index >= bossSkills.Count)
            return null;

        BossSkillClass skill = bossSkills[index];
        if (skill.poolQueue.Count == 0)
            return null;

        GameObject obj = skill.poolQueue.Dequeue();
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            
            skill.poolQueue.Enqueue(obj);
        }

        return obj;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) Hp -= MaxHp;
        if(Input.GetKeyDown(KeyCode.O)) Hp -= MaxHp / 2;
    }
    
}
