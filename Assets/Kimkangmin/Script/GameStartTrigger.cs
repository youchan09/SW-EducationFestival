using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap Collider 2D 타입을 사용하기 위해 추가

public class GameStartTrigger : MonoBehaviour
{
    [Tooltip("뱀 오브젝트의 이동/공격 스크립트들을 여기에 연결하세요. (선택 사항)")]
    public MonoBehaviour[] scriptsToActivate; 
    
    [Tooltip("맵 경계 콜라이더가 부착된 Tilemap 게임 오브젝트들을 여기에 연결하세요.")]
    // ⭐ GameObject 배열로 변경하여 Tilemap 오브젝트 자체를 드래그할 수 있게 함 ⭐
    public GameObject[] boundaryTilemapObjectsToActivate; 
    
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other) // 2D 게임의 경우
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Debug.Log("🔔 플레이어 진입 감지! 뱀 스크립트 및 맵 경계 콜라이더를 활성화합니다.");
            
            ActivateSnakeScripts();
            ActivateBoundaryColliders(); // ⭐ 변경된 활성화 함수 호출 ⭐
            
            hasTriggered = true;
            Destroy(gameObject); 
        }
    }

    void ActivateSnakeScripts()
    {
        foreach (MonoBehaviour script in scriptsToActivate)
        {
            if (script != null)
            {
                script.enabled = true;
                Debug.Log($"-> 뱀 스크립트 활성화: {script.GetType().Name}");
            }
        }
    }
    
    // ⭐ GameObject에서 Tilemap Collider 2D를 찾아 활성화하는 함수 ⭐
    void ActivateBoundaryColliders()
    {
        foreach (GameObject tilemapObj in boundaryTilemapObjectsToActivate)
        {
            if (tilemapObj != null)
            {
                // Tilemap Collider 2D 컴포넌트 찾기
                TilemapCollider2D tileCollider = tilemapObj.GetComponent<TilemapCollider2D>();

                // 만약 Tilemap Collider 2D가 없다면 Collider2D를 찾습니다.
                Collider2D generalCollider = tileCollider != null ? tileCollider : tilemapObj.GetComponent<Collider2D>();

                if (generalCollider != null)
                {
                    generalCollider.enabled = true; // 컴포넌트 활성화
                    Debug.Log($"-> 맵 경계 콜라이더 활성화: {tilemapObj.name}의 {generalCollider.GetType().Name}");
                }
                else
                {
                    Debug.LogError($"❌ {tilemapObj.name}에서 Tilemap Collider 2D 또는 Collider2D 컴포넌트를 찾을 수 없습니다.");
                }
            }
        }
    }
}