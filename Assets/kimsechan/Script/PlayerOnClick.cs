using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerOnClick : MonoBehaviour
{
    // [인스펙터 설정] 선택된 직업의 무기/캐릭터/설명 이미지 프리팹 또는 UI GameObject 리스트
    // 이 리스트를 관리하여 활성화/비활성화 할 수 있습니다.
    public List<GameObject> job_image_display = new List<GameObject>(); 

    private StartManager pSM;
    private int selectedJobIndex = 0; // 직업 인덱스를 더 명확하게 변경
    
    [Obsolete("Obsolete")]
    private void Start()
    {
        pSM = FindObjectOfType<StartManager>();
        
        // 초기화: 모든 이미지를 비활성화하여 시작합니다.
        foreach (GameObject img in job_image_display)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }
    }

    // 직업 선택 함수들 (각 버튼에 연결)
    public void OnClick_1() { 
        SetJobIndex(0); 
    }
    public void OnClick_2() { 
        SetJobIndex(1); 
    }
    public void OnClick_3() { 
        SetJobIndex(2); 
    }
    public void OnClick_4() { 
        SetJobIndex(3); 
    }

    /// <summary>
    /// 직업 인덱스를 설정하고, 해당 직업 이미지를 활성화합니다.
    /// </summary>
    private void SetJobIndex(int index)
    {
        if (pSM == null) return;
        
        // 1. 인덱스 업데이트
        selectedJobIndex = index;
        
        // 2. StartManager에 인덱스 전달 (무기 미리보기/애니메이션 트리거)
        pSM.SelectJob(selectedJobIndex);
        
        // 3. UI 이미지 관리: 이전 이미지를 끄고, 새 이미지를 는 로직
        // 이 로직은 StartManager의 DisplayJobWeapon 함수와 겹칠 수 있으므로, 
        // 씬에서 표시할 요소가 3D 무기가 아닌 UI 패널이라면 여기에 넣을 수 있습니다.

        if (index >= 0 && index < job_image_display.Count)
        {
            // 모든 이미지를 비활성화
            for (int j = 0; j < job_image_display.Count; j++)
            {
                if (job_image_display[j] != null)
                {
                    job_image_display[j].SetActive(false);
                }
            }
            
            // 선택된 이미지만 활성화
            job_image_display[index].SetActive(true);
        }
    }
    
    public void StartOnClick()
    {
        if (pSM == null) return;
        
        // StartManager에 최종 인덱스를 다시 전달 (확인용)
        pSM.SelectJob(selectedJobIndex);
        
        // StartManager에게 플레이어 생성 및 씬 전환을 지시
        pSM.StartJobCoroutine();
    }
}