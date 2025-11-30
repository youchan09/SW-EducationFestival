using UnityEngine;

public class Enemey : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Weapon")
        {
            Debug.Log(other.name);
        }
    }
}
