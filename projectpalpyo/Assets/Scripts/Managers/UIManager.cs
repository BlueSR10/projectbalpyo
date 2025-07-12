using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("게임 UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI dataCoreText;
    public TextMeshProUGUI warningText;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI gameOverMessage;
    
    [Header("메인 메뉴 UI")]
    public Button startButton;
    public Button quitButton;
    
    void Start()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
            
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            });
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                Application.Quit();
            });
        }
    }
    
    public void UpdateGameUI(int caughtCount, int maxCaught, int collectedCores, int requiredCores)
    {
        if (healthText != null)
        {
            healthText.text = $"경고: {caughtCount}/{maxCaught}";
        }
        
        if (dataCoreText != null)
        {
            dataCoreText.text = $"데이터 코어: {collectedCores}/{requiredCores}";
        }
    }
    
    public void ShowCaughtWarning()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
            warningText.text = "경고: 보안 로봇에게 발견됨!";
            Invoke("HideWarning", 2f);
        }
    }
    
    void HideWarning()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }
    
    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (gameOverMessage != null)
        {
            gameOverMessage.text = "미션 실패!\n보안 시스템에 의해 포획되었습니다.";
        }
    }
    
    public void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        if (gameOverMessage != null)
        {
            gameOverMessage.text = "미션 성공!\n모든 데이터를 확보하고 탈출했습니다.";
        }
    }
}