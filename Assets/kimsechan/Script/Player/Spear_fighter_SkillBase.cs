using UnityEngine;

public abstract class Spear_fighter_SkillBase : MonoBehaviour
{
    public string skillName;
    public float cooldown;
    [HideInInspector]public float currentCooldown;
    public bool isAvailable => currentCooldown <= 0f;

    public abstract void Activate(); // 실제 발동 로직

    public virtual void UpdateCooldown()
    {
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
    }
}