using System.Collections;
using UnityEngine;

public class Red_bord_FireTornado : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Destroy());
    }
    public IEnumerator Destroy()
    {
        yield return new WaitForSeconds(7f);
        Destroy(gameObject);
    }
}
