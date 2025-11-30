using UnityEngine;

public abstract class Skill_based : MonoBehaviour
{
    public abstract void Attack();
    
    // 💡 [추가]: 2페이즈 진입 등 외부에서 공격을 강제로 중지시키기 위해 모든 스킬이 구현해야 하는 메서드
    public abstract void StopAttack();
}
