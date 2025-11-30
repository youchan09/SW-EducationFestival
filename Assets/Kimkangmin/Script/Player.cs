using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    void FixedUpdate()
    {
        _Move();
    }
    
    void _Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(h, v, 0).normalized * speed * Time.deltaTime;
        transform.Translate(movement);
        if (h == 0)
        {
            
        }
    }
}
