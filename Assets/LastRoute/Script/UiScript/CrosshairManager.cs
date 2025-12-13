using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    [Header("UI 조준점 이미지")]
    public RectTransform crosshairImage; // 움직일 이미지의 RectTransform

    void Start()
    {
        // 1. 시스템 기본 마우스 커서를 숨깁니다.
        Cursor.visible = false;

        // (선택사항) 마우스를 게임 화면 안에 가둘지 설정
         Cursor.lockState = CursorLockMode.Confined; 
    }

    void Update()
    {
        // 2. 조준점 이미지를 현재 마우스 위치로 이동시킵니다.
        if (crosshairImage != null)
        {
            crosshairImage.position = Input.mousePosition;
        }

        // (옵션) 게임 중 ESC 누르면 마우스 커서 다시 보이게 하기
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
        }
        
    }
}