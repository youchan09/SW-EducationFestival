using UnityEngine;
using System.Collections; 

public class WaterCannon : MonoBehaviour
{
    // ================== 인스펙터 설정 변수 ==================
    public GameObject waterStreamPrefab;
    
    [Tooltip("물줄기 발사 전 2초간 표시될 경고 오브젝트 프리팹을 여기에 연결하세요.")]
    public GameObject warningIndicatorPrefab; 
    
    public Transform playerTransform; 
    public Transform firePoint;       
    
    // 공격 쿨타임: 물줄기 유지 시간 2초 + 다음 공격까지 대기 시간 6초 = 8초
    public float attackCooldown = 8f; 
    
    [Header("물줄기 애니메이션 설정")]
    public float maxScaleY = 5f;       
    public float scaleDuration = 0.5f; 
    public float totalDuration = 2f;   
    public float warningTime = 2f;     // 물줄기 발사 전 2초 경고

    private float lastAttackTime;

    void Update()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (playerTransform != null && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (warningIndicatorPrefab != null)
        {
            // 1. 경고 인디케이터 생성 및 조준
            GameObject indicator = Instantiate(warningIndicatorPrefab, firePoint.position, firePoint.rotation);
            
            Vector3 directionToPlayer = playerTransform.position - firePoint.position;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            indicator.transform.rotation = Quaternion.Euler(0, 0, angle - 900f); 

            // 2. 코루틴 시작: 대기 -> 물줄기 생성
            StartCoroutine(PrepareAndScaleWaterStream(indicator));
        }
        else
        {
             StartCoroutine(PrepareAndScaleWaterStream(null)); 
        }
    }

    IEnumerator PrepareAndScaleWaterStream(GameObject indicator)
    {
        // 1. 경고 시간 대기 (2초)
        if (indicator != null)
        {
            yield return new WaitForSeconds(warningTime);
            Destroy(indicator);
        }

        // 2. 물줄기 생성 및 조준
        if (waterStreamPrefab == null) yield break;

        GameObject waterStream = Instantiate(waterStreamPrefab, firePoint.position, firePoint.rotation);
        
        Vector3 directionToPlayer = playerTransform.position - firePoint.position;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        waterStream.transform.rotation = Quaternion.Euler(0, 0, angle + 90f); 

        // 3. 스케일 증가 애니메이션
        Vector3 initialScale = waterStream.transform.localScale;
        waterStream.transform.localScale = new Vector3(initialScale.x, 0f, initialScale.z);

        float timer = 0f;
        while (timer < scaleDuration)
        {
            float ratio = timer / scaleDuration; 
            float currentScaleY = Mathf.Lerp(0f, maxScaleY, ratio);

            waterStream.transform.localScale = new Vector3(initialScale.x, currentScaleY, initialScale.z);
            
            timer += Time.deltaTime;
            yield return null; 
        }
        
        waterStream.transform.localScale = new Vector3(initialScale.x, maxScaleY, initialScale.z);


        // 4. 물줄기 유지 시간 대기 및 파괴
        float waitTime = totalDuration - scaleDuration;
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        Destroy(waterStream);
    }
}