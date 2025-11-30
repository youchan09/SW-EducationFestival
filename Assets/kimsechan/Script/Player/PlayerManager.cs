using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [HideInInspector] public Image Hpbar; 
    
    public float speed = 10f;
    public float attackPower = 10f; 
    private float currentHp;
    public float maxHp = 100;

    [Header("무적 시간 설정")]
    public float invincibleDuration = 0.5f; // 1초 동안 무적
    private bool isInvincible = false;      // 무적 상태 플래그

    private bool isFirstLoad = true;
    private bool sceneEventsRegistered = false;

    public float Hp
    {
        get { return currentHp; }
        set
        {
            currentHp = Mathf.Clamp(value, 0, maxHp);
            float targetfill = currentHp / maxHp;
            if (Hpbar != null)
                DOTween.To(() => Hpbar.fillAmount, x => Hpbar.fillAmount = x, targetfill, 0.1f);

            if (currentHp <= 0) Die();
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!sceneEventsRegistered)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            sceneEventsRegistered = true;
        }
    }

    void Start()
    {
        if (isFirstLoad)
        {
            currentHp = maxHp;
            isFirstLoad = false;
        }
        InitializeHpbar();
        if(Hpbar != null) Hpbar.fillAmount = currentHp / maxHp;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        InitializeHpbar();

        // 씬 이동 시 HP를 50으로 설정
        Hp += 50;

        if(Hpbar != null) Hpbar.fillAmount = currentHp / maxHp;
    }

    private void OnDestroy()
    {
        if (sceneEventsRegistered)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeHpbar()
    {
        if (Hpbar != null) return;

        GameObject hpObj = GameObject.FindGameObjectWithTag("PlayerHpbar");
        if (hpObj != null)
        {
            Hpbar = hpObj.GetComponent<Image>();
            if (Hpbar == null)
                Debug.LogError("PlayerHpbar 태그 오브젝트에 Image 컴포넌트가 없습니다!");
        }
        else
        {
            Debug.LogWarning("씬에 'PlayerHpbar' 태그를 가진 오브젝트가 없습니다!");
        }
    }

    // 🔥 데미지 처리: 무적 상태 체크
    public void take_Damage(float damage)
    {
        if (isInvincible) return; // 무적이면 데미지 무시

        Hp -= damage;
        Debug.Log("현재 HP: " + Hp);

        // 무적 상태 시작
        StartCoroutine(InvincibleCoroutine());
    }

    // 무적 코루틴
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // ⚡ 시각적으로 깜빡이게 하고 싶으면 여기서 SpriteRenderer 깜빡임 추가 가능
        yield return new WaitForSeconds(invincibleDuration);

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        Player player = GetComponentInParent<Player>();
        if (player != null)
            player.enabled = false;
        else
            Debug.LogWarning("사망 처리: 'Player' 스크립트를 찾을 수 없습니다.");
    }
}