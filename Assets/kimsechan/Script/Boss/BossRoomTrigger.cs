using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] private BossManager bossManager;

    private void Awake()
    {
        if (bossManager == null)
            bossManager = FindObjectOfType<BossManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossManager.StartBossEncounter();
            Destroy(gameObject); // 트리거 한번만
        }
    }
}