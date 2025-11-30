using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float followSpeed = 5f;
    private BossManager bossManager;

    void Awake()
    {
        bossManager = FindObjectOfType<BossManager>();
        if (bossManager == null)
        {
            Debug.LogWarning("BossManager를 씬에서 찾을 수 없습니다. 카메라 팔로우가 항상 실행됩니다.");
        }
    }

    void Update()
    {
        LateUpdate();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        if (target == null) return;
        if (bossManager != null && bossManager.IsPlayerInputLocked)
        {
            return;
        }

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}