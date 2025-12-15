using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 씬 이동/재로딩을 위해 필요

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI 연결")]
    public GameObject gameOverPanel;
    public GameObject optionsMenuPanel; // [추가] 옵션 메뉴 패널

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI killText;
    public TextMeshProUGUI scoreText;

    [Header("게임 상태")]
    public bool isControlActive = true; // 조작 허용 여부
    private bool isGameOver = false;
    private bool isOptionsOpen = false; // 옵션 창이 열렸는지 확인

    [Header("게임 데이터")]
    public float survivalTime = 0f;
    public int killCount = 0;
    public int score = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 마우스 중앙 위치
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 1f;
        isControlActive = true;
        isGameOver = false;
        isOptionsOpen = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false); // 시작할 땐 꺼둠
    }

    void Update()
    {
        // ESC 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 게임 오버 상태가 아닐 때만 작동
            if (!isGameOver)
            {
                ToggleOptionsMenu();
            }
        }

        if (!isGameOver && !isOptionsOpen)
        {
            survivalTime += Time.deltaTime;
        }
    }

    // 옵션 메뉴를 켜고 끄는 함수
    public void ToggleOptionsMenu()
    {
        isOptionsOpen = !isOptionsOpen; // 상태 뒤집기 (True <-> False)

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(isOptionsOpen);
        }

        if (isOptionsOpen)
        {
            // 메뉴가 열렸을 때: 멈춤
            Time.timeScale = 0f;          // 시간 정지
            isControlActive = false;      // 조작 차단

            // (만약 마우스가 안 보인다면 아래 주석 해제)
            // Cursor.visible = true;
            // Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 메뉴가 닫혔을 때: 재개
            Time.timeScale = 1f;          // 시간 흐름
            isControlActive = true;       // 조작 허용

            // (FPS 모드라면 다시 마우스 잠금 필요할 수 있음)
            // Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // 재시작 버튼 기능
    public void RestartGame()
    {
        Time.timeScale = 1f; // 시간 배속을 1로 돌리고 리로드

        // 현재 씬의 이름을 가져와서 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인 메뉴로 나가기
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // 씬 이름 확인 필요
    }

    // --- 기존 점수/게임오버 로직 ---
    public void AddScore(int scoreAmount)
    {
        if (isGameOver) return;
        killCount++;
        score += scoreAmount;
    }

    public void OnGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isControlActive = false; // 조작 차단

        // 옵션 메뉴가 켜져 있었다면 끄기
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        UpdateResultUI();
    }

    void UpdateResultUI()
    {
        int minutes = Mathf.FloorToInt(survivalTime / 60F);
        int seconds = Mathf.FloorToInt(survivalTime % 60F);

        if (timeText != null)
            timeText.text = string.Format("Survival Time : {0:00}:{1:00}", minutes, seconds);

        if (killText != null)
            killText.text = "Killed Zombie : " + killCount;

        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }
}