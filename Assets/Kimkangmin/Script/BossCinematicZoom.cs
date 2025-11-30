using UnityEngine;
using System.Collections;

public class BossCinematicZoom : MonoBehaviour
{
    public Health bossHealth;           // 보스의 체력 관리 스크립트
    public Camera mainCamera;           // 메인 카메라
    public float zoomInSize = 3f;       // 줌인 크기
    public float zoomDuration = 2f;     // 줌 유지 시간
    public float zoomSpeed = 3f;        // 줌 속도

    private bool hasZoomed = false;     // 한 번만 실행되도록
    private float originalSize;
    private Vector3 originalPosition;

    private MonoBehaviour[] scriptsToPause; // 일시정지시킬 스크립트들 저장

    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<Health>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            originalSize = mainCamera.orthographicSize;
            originalPosition = mainCamera.transform.position;
        }
    }

    void Update()
    {
        if (bossHealth == null || mainCamera == null) return;

        // 💥 보스 체력이 절반 이하로 떨어졌고 아직 연출이 안 나갔을 때
        if (!hasZoomed && bossHealth.currentHP <= bossHealth.maxHP / 2f)
        {
            hasZoomed = true;
            StartCoroutine(ZoomInAndOut());
        }
    }

    IEnumerator ZoomInAndOut()
    {
        // 1️⃣ 모든 움직임 관련 스크립트 일시정지 (Health 제외)
        PauseMovement(true);

        // 2️⃣ 카메라 줌인
        float elapsed = 0f;
        Vector3 bossPos = transform.position + new Vector3(0, 0, -10f);
        while (elapsed < 1f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(originalSize, zoomInSize, elapsed);
            mainCamera.transform.position = Vector3.Lerp(originalPosition, bossPos, elapsed);
            elapsed += Time.unscaledDeltaTime * zoomSpeed;
            yield return null;
        }

        mainCamera.orthographicSize = zoomInSize;
        mainCamera.transform.position = bossPos;

        // 3️⃣ 줌 유지
        yield return new WaitForSecondsRealtime(zoomDuration);

        // 4️⃣ 카메라 복귀
        elapsed = 0f;
        while (elapsed < 1f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(zoomInSize, originalSize, elapsed);
            mainCamera.transform.position = Vector3.Lerp(bossPos, originalPosition, elapsed);
            elapsed += Time.unscaledDeltaTime * zoomSpeed;
            yield return null;
        }

        mainCamera.orthographicSize = originalSize;
        mainCamera.transform.position = originalPosition;

        // 5️⃣ 모든 움직임 재개
        PauseMovement(false);

        Debug.Log("🎥 보스 체력 절반 연출 완료 (줌 + 일시정지)");
    }

    // ▶ 모든 오브젝트의 움직임만 일시정지 (Health는 유지)
    void PauseMovement(bool pause)
    {
        // Scene 내 모든 MonoBehaviour 가져오기
        scriptsToPause = FindObjectsOfType<MonoBehaviour>();

        foreach (var script in scriptsToPause)
        {
            if (script is Health) continue; // 체력 관련은 멈추지 않음
            if (script is BossCinematicZoom) continue; // 자기 자신도 멈추지 않음

            script.enabled = !pause; // pause=true면 비활성화
        }

        Debug.Log(pause ? "⏸️ 움직임 멈춤" : "▶ 움직임 재개");
    }
}
