using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public float lifetime = 4f;
    public float damagePerSecond = 3f;

    private float damageCooldown = 1f;
    private float nextDamageTime;

    void Start()
    {
        Destroy(gameObject, lifetime);
        nextDamageTime = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 💡 Rigidbody가 있는지 체크 (Trigger 작동 보장)
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("⚠️ Player에 Rigidbody가 없습니다. DamageZone 감지 안 될 수 있음.");
        }

        if (Time.time >= nextDamageTime)
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"🔥 DamageZone: {other.name}에게 {damagePerSecond} 데미지!");
                playerHealth.TakeDamage(damagePerSecond);
            }
            else
            {
                Debug.LogError("❌ Player에 Health 스크립트가 없음!");
            }

            nextDamageTime = Time.time + damageCooldown;
        }
    }
}