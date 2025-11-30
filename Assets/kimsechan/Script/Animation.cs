using UnityEngine;

public class Animation : MonoBehaviour
{
    [Header("프레임 설정")]
    public Sprite frameA;
    public Sprite frameB;

    [Header("애니메이션 속도")]
    public float interval = 0.15f;

    private SpriteRenderer sr;
    private float timer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!sr.enabled) return; // 꺼져있으면 건너뜀

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            // 스프라이트 교체
            sr.sprite = (sr.sprite == frameA) ? frameB : frameA;
            timer = 0f;
        }
    }
}