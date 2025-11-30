using UnityEngine;

public class SnakeAttack : MonoBehaviour
{
    public GameObject Slow;

    void SlowAttack()
    {
        GameObject slow = Instantiate(Slow, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
