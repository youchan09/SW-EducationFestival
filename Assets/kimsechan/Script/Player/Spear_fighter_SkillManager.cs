using UnityEngine;

public class Spear_fighter_SkillManager : MonoBehaviour
{
    public Spear_fighter_SkillBase[] skills; // Inspector에 등록
    public KeyCode[] skillKeys = { KeyCode.Mouse0, KeyCode.Q, KeyCode.E, KeyCode.R };

    void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] == null) continue;

            skills[i].UpdateCooldown();

            if (Input.GetKeyDown(skillKeys[i]))
            {
                skills[i].Activate();
            }
        }
    }
}