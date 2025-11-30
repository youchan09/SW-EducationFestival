using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 추가

public class Tornado : MonoBehaviour
{
    // --- 흡입 및 물리 설정 ---
    [Header("흡입 설정")]
    [Tooltip("토네이도의 중심 (Y축 로컬 오프셋)")]
    public float vortexYOffset = -1.8f;   // 토네이도 중심 오프셋 (토네이도 밑부분)
    [Tooltip("플레이어를 끌어당기는 힘의 크기")]
    public float pullForce = 30f;
    public float pullRadius = 5f;
    [Range(0f, 1f)]
    [Tooltip("흡입 적용의 부드러움/반응성 (0에 가까울수록 반응 빠름)")]
    public float damping = 0.1f;

    // --- 추적 설정 ---
    [Header("추적 설정")]
    [Tooltip("플레이어를 따라가는 속도 (클수록 더 빠르게 추적)")]
    public float followSpeed = 2f; 
    [Tooltip("토네이도가 플레이어를 따라갈 때 적용할 Y축 오프셋 (땅에 붙이는 용도)")]
    public float playerFollowYOffset = -2.9f; // 🟢 플레이어 추적 시 Y 오프셋 (-2.9f)

    // --- 지속 시간 및 데미지 설정 ---
    [Header("지속 시간 및 데미지 설정")]
    [Tooltip("토네이도가 자동으로 사라지기까지의 시간 (초)")]
    public float lifeTime = 8f; // 🟢 지속 시간 변수 (8초)
    [Tooltip("플레이어가 흡입 범위 내에 있을 때 초당 입힐 데미지")]
    public float damagePerSecond = 10f;
    [Tooltip("데미지를 입힐 간격 (초)")]
    public float damageInterval = 0.5f;

    // --- 범위 시각화 (GameObject) ---
    [Header("범위 시각화 (GameObject)")]
    [Tooltip("범위를 시각화할 자식 오브젝트의 Transform (예: 원형 스프라이트)")]
    public Transform rangeVisualizer; // 시각화 오브젝트의 Transform

    // --- 깊이 정렬을 위한 변수 ---
    private SpriteRenderer tornadoRenderer;
    private SpriteRenderer playerRenderer;
    private int baseTornadoSortingOrder;
    // ---------------------------------

    private Transform playerTarget;
    private Rigidbody2D playerRb2D;
    private PlayerManager playerManager;
    private bool wasInPullRange = false;
    private Coroutine damageCoroutine;
    private Coroutine lifeTimeCoroutine; // 🟢 생명 주기 코루틴 참조

    void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            playerRb2D = playerObj.GetComponent<Rigidbody2D>();
            playerRenderer = playerObj.GetComponent<SpriteRenderer>();

            // PlayerManager 참조 추가 및 오류 체크
            playerManager = playerObj.GetComponent<PlayerManager>();
            if (playerManager == null)
                Debug.LogError("Player 오브젝트에 PlayerManager 컴포넌트가 없습니다! 데미지 기능이 작동하지 않습니다.");
        }

        tornadoRenderer = GetComponent<SpriteRenderer>();
        if (tornadoRenderer != null)
            baseTornadoSortingOrder = tornadoRenderer.sortingOrder;
        
        SetupRangeVisualization();
    }

    // 🟢 오브젝트가 활성화될 때마다 호출되어 생명 주기 코루틴을 시작합니다.
    private void OnEnable()
    {
        // 이전 코루틴이 남아있을 경우 중지 (안전 장치)
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
        }
        // 설정된 lifeTime(8초) 후 비활성화하는 코루틴 시작
        lifeTimeCoroutine = StartCoroutine(DeactivateAfterDelay(lifeTime));
    }

    // 🟢 오브젝트가 비활성화될 때 (풀로 돌아갈 때) 호출되어 정리 작업을 수행합니다.
    private void OnDisable()
    {
        ClearEffectsAndStopCoroutines();
        
        // 생명 주기 코루틴도 중지하여 재사용 시 오작동을 방지
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }
    }

    // 🟢 잔상 버그를 방지하고 모든 효과(데미지) 코루틴을 중지합니다.
    private void ClearEffectsAndStopCoroutines()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        // 잔상 버그 수정: 끌려가는 도중에 토네이도가 사라질 경우, 플레이어의 속도를 0으로 초기화
        if (playerRb2D != null && wasInPullRange)
        {
            playerRb2D.linearVelocity = Vector2.zero;
            wasInPullRange = false;
        }
    }

    // 🟢 토네이도의 모든 효과를 즉시 중지하고 플레이어 속도 초기화 후 오브젝트를 비활성화(풀 반환)합니다.
    private void DeactivateTornado()
    {
        ClearEffectsAndStopCoroutines();

        // 🟢🟢🟢 핵심: 오브젝트 비활성화 (풀로 반환) 🟢🟢🟢
        gameObject.SetActive(false);
    }
    
    // 🟢 지정된 시간(delay) 후에 토네이도를 비활성화하는 코루틴
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateTornado();
    }

    void FixedUpdate()
    {
        if (playerTarget == null || playerRb2D == null) return;

        // 토네이도의 중심 좌표 계산
        Vector3 center3D = transform.position + transform.up * vortexYOffset;
        Vector2 center = new Vector2(center3D.x, center3D.y);
        Vector2 playerPos = playerTarget.position;

        float dist = Vector2.Distance(center, playerPos);
        bool inRange = dist <= pullRadius;

        if (inRange)
        {
            // 흡입 로직: 원하는 속도를 계산하고 감쇠를 적용하여 속도 변경
            Vector2 toCenter = (center - playerPos).normalized;
            Vector2 desiredVelocity = toCenter * pullForce;
            Vector2 velocityChange = desiredVelocity - playerRb2D.linearVelocity;
            playerRb2D.linearVelocity += velocityChange * (1 - damping);
            wasInPullRange = true;
            
            // 데미지 로직 시작
            if (damageCoroutine == null && playerManager != null)
            {
                damageCoroutine = StartCoroutine(ApplyDamageOverTime(damagePerSecond, damageInterval));
            }
        }
        else // inRange == false
        {
            if (wasInPullRange)
            {
                // 범위에서 벗어났을 때 속도 초기화 (이 로직은 그대로 유지)
                playerRb2D.linearVelocity = Vector2.zero;
                wasInPullRange = false;
            }
            
            // 데미지 로직 중지
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    // 데미지 코루틴 함수: 일정 시간 간격으로 데미지 적용
    private IEnumerator ApplyDamageOverTime(float dps, float interval)
    {
        float damagePerHit = dps * interval; 
        
        while (true)
        {
            if (playerTarget != null && playerManager != null)
            {
                Vector3 center3D = transform.position + transform.up * vortexYOffset;
                Vector2 center = new Vector2(center3D.x, center3D.y);
                float dist = Vector2.Distance(center, playerTarget.position);

                // 코루틴 내부에서 다시 한번 범위 체크 (안전 장치)
                if (dist <= pullRadius)
                {
                    // PlayerManager의 데미지 함수 호출
                    // (PlayerManager 스크립트에 take_Damage 함수가 있다고 가정합니다.)
                    // 이 부분은 가정이며 실제 게임 환경에 맞게 수정해야 합니다.
                    // playerManager.take_Damage(damagePerHit); 
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }

    void Update()
    {
        // 플레이어 추적 로직
        if (playerTarget != null)
        {
            Vector3 targetPosition = playerTarget.position;
            
            // 🟢 플레이어 위치에 Y 오프셋을 적용하여 따라가도록 변경
            targetPosition.y += playerFollowYOffset; 

            // Z축은 변경하지 않도록 기존 Z 값을 사용
            targetPosition.z = transform.position.z; 

            // Lerp를 사용하여 부드럽게 이동
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
        }

        HandleDepthSorting();
    }

    // 플레이어와 토네이도의 깊이 정렬 처리
    private void HandleDepthSorting()
    {
        if (playerTarget == null || playerRenderer == null || tornadoRenderer == null) return;

        // 토네이도의 Y축 기준 위치 (vortexYOffset)
        float tornadoYRef = transform.position.y + vortexYOffset;
        float playerY = playerTarget.position.y;

        // 플레이어가 토네이도 기준점보다 위에 있으면(화면 위쪽), 정렬 순서를 낮춰 뒤에 배치
        if (playerY > tornadoYRef)
            playerRenderer.sortingOrder = baseTornadoSortingOrder - 1;
        // 플레이어가 토네이도 기준점보다 아래에 있으면(화면 아래쪽), 정렬 순서를 높여 앞에 배치
        else
            playerRenderer.sortingOrder = baseTornadoSortingOrder + 1;

        tornadoRenderer.sortingOrder = baseTornadoSortingOrder;
    }

    // 시각화 오브젝트의 크기와 위치를 설정
    private void SetupRangeVisualization()
    {
        if (rangeVisualizer == null)
        {
            Debug.LogWarning("Range Visualizer Transform이 설정되지 않았습니다. 범위 시각화가 작동하지 않습니다.");
            return;
        }
        
        // 1. 시각화 오브젝트 위치 설정 (토네이도 중심 오프셋에 맞춤)
        rangeVisualizer.localPosition = new Vector3(0f, vortexYOffset, 0f);

        // 2. 시각화 오브젝트 크기 설정
        float scale = pullRadius * 2f; 
        rangeVisualizer.localScale = new Vector3(scale, scale, 1f);

        // 오브젝트 활성화 확인
        rangeVisualizer.gameObject.SetActive(true);
    }

    // 💡 런타임에 pullRadius가 변할 때 시각화 크기를 갱신하는 함수
    private void UpdateRangeVisualization()
    {
        if (rangeVisualizer != null)
        {
            float scale = pullRadius * 2f; 
            rangeVisualizer.localScale = new Vector3(scale, scale, 1f);
            rangeVisualizer.localPosition = new Vector3(0f, vortexYOffset, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터 상에서만 보이는 Gizmos를 사용하여 중심점과 흡입 범위 표시
        Vector3 center = transform.position + transform.up * vortexYOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, 0.2f); // 중심점
        Gizmos.color = new Color(1, 0, 0, 0.15f); // 흡입 범위
        Gizmos.DrawWireSphere(center, pullRadius);
    }
}