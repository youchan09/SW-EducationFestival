using UnityEngine;

public class Tornado : MonoBehaviour
{
    [Header("흡입 설정")]
    [Tooltip("토네이도의 중심 (Y축 로컬 오프셋)")]
    public float vortexYOffset = -1.8f;
    [Tooltip("플레이어를 끌어당기는 힘의 크기")]
    public float pullForce = 15f; // 기본 힘을 5f에서 15f로 증가
    [Tooltip("흡입 범위의 반지름")]
    public float pullRadius = 5f;

    [Header("범위 시각화 설정")]
    [Tooltip("원형 범위를 그릴 때 사용할 LineRenderer")]
    public LineRenderer rangeLineRenderer;
    [Tooltip("원형을 그릴 때 사용할 꼭짓점(Vertex) 개수")]
    public int segments = 50;

    [Header("참조")]
    private Transform playerTarget;
    private Rigidbody2D playerRb2D; 
    private bool wasInPullRange = false; // 직전 프레임에 범위 내에 있었는지 추적

    void Awake()
    {
        // "Player" 태그를 가진 오브젝트를 찾아 플레이어 타겟으로 설정
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            playerRb2D = playerObj.GetComponent<Rigidbody2D>();
            
            if (playerRb2D == null)
            {
                 Debug.LogError("Player 오브젝트에 Rigidbody2D 컴포넌트가 없습니다! 흡입 스킬이 물리적으로 작동하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("Player 오브젝트(태그: Player)를 찾을 수 없습니다!");
        }

        // 런타임 시각화 초기화
        SetupRangeVisualization();
    }

    void FixedUpdate()
    {
        // 물리 연산은 FixedUpdate에서 처리
        if (playerTarget != null && playerRb2D != null)
        {
            // 1. 토네이도의 중심점을 계산 (로컬 yOffset 적용)
            Vector3 centerPosition3D = transform.position + transform.up * vortexYOffset;
            Vector2 centerPosition = new Vector2(centerPosition3D.x, centerPosition3D.y);
            Vector2 playerPosition = playerTarget.position;
            
            float distanceToCenter = Vector2.Distance(centerPosition, playerPosition);

            // 플레이어가 흡입 범위(pullRadius) 내에 있는지 확인
            bool isInRange = distanceToCenter <= pullRadius;

            if (isInRange) 
            {
                // 범위 안에 있을 때: 흡입 힘 적용
                Vector2 directionToCenter = (centerPosition - playerPosition).normalized;
                float strengthMultiplier = 1 - (distanceToCenter / pullRadius);
                
                Vector2 force = directionToCenter * pullForce * strengthMultiplier;
                playerRb2D.AddForce(force, ForceMode2D.Force); 

                wasInPullRange = true;
            }
            else if (wasInPullRange)
            {
                // 범위 밖으로 나가는 순간: 잔여 운동량(관성) 제거
                // 이전에 힘을 받고 있었으나 지금은 범위 밖인 경우, 속도를 0으로 설정합니다.
                playerRb2D.linearVelocity = Vector2.zero;
                wasInPullRange = false;
            }
        }
    }
    
    void Update()
    {
        UpdateRangeVisualization();
    }
    
    private void SetupRangeVisualization()
    {
        if (rangeLineRenderer == null)
        {
            GameObject lineObj = new GameObject("PullRadius_Visualizer");
            lineObj.transform.SetParent(this.transform);
            lineObj.transform.localPosition = new Vector3(0, vortexYOffset, 0); 
            rangeLineRenderer = lineObj.AddComponent<LineRenderer>();

            rangeLineRenderer.useWorldSpace = false; 
            rangeLineRenderer.loop = true;          
            rangeLineRenderer.startWidth = 0.1f;    
            rangeLineRenderer.endWidth = 0.1f;
            rangeLineRenderer.positionCount = segments + 1; 
            
            // 2D 렌더링을 위해 Sorting Layer 설정 (필수)
            rangeLineRenderer.sortingLayerName = "Default"; // 프로젝트의 기본 Sorting Layer 사용
            rangeLineRenderer.sortingOrder = 100;           // 다른 스프라이트 위에 표시되도록 높은 값 설정
            
            rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rangeLineRenderer.startColor = Color.red;
            rangeLineRenderer.endColor = Color.red;
        }
        
        CreateCirclePoints();
    }
    
    /// <summary>
    /// LineRenderer의 꼭짓점 위치를 계산하여 원형을 만듭니다.
    /// </summary>
    private void CreateCirclePoints()
    {
        if (rangeLineRenderer == null) return;
        
        float deltaAngle = 360f / segments;
        rangeLineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * deltaAngle;
            float x = pullRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = pullRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 point = new Vector3(x, y, 0); 
            rangeLineRenderer.SetPosition(i, point);
        }
    }
    private void UpdateRangeVisualization()
    {
        if (rangeLineRenderer != null)
        {
            rangeLineRenderer.transform.localPosition = new Vector3(0, vortexYOffset, 0);
        }
    }


    // 디버깅을 위해 에디터에서 흡입 중심점과 범위를 시각화 (게임 실행 시에는 이 부분이 보이지 않습니다.)
    private void OnDrawGizmosSelected()
    {
        // 토네이도의 로컬 Y 오프셋을 적용한 중심 위치 계산 (2D에서는 Z축 무시)
        Vector3 centerPosition = transform.position + transform.up * vortexYOffset;
        
        // 흡입 중심점을 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(centerPosition, 0.2f); 

        // 흡입 반경을 표시
        Gizmos.color = new Color(0.5f, 0, 0.5f, 0.5f); 
        Gizmos.DrawWireSphere(centerPosition, pullRadius);
    }
}