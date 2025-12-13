using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public string SceneName = "";

    // 이동할 씬의 이름을 문자열로 입력받는 함수
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 게임 종료 기능 
    public void ExitGame()
    {
        Debug.Log("게임 종료 버튼이 눌렸습니다."); // 콘솔창 확인용

    #if UNITY_EDITOR
        // 에디터에서 플레이 중일 때는 플레이 모드를 멈춤
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // 실제 빌드된 게임에서는 프로그램을 종료
        Application.Quit();
    #endif
    }
}
