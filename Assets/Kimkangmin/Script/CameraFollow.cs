using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float followSpeed = 5f;
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

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}