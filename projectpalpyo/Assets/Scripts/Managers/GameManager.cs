using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("게임 설정")]
    public int maxCaughtCount = 3;
    public int requiredDataCores = 3;
    
    private int playerCaughtCount = 0;
    private int collectedDataCores = 0;
    private bool gameOver = false;
    
    [Header("UI 참조")]
    public UIManager uiManager;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
            
        UpdateUI();
    }
    
    public void PlayerCaught()
    {
        if (gameOver) return;
        
        playerCaughtCount++;
        Debug.Log($"플레이어 포착! ({playerCaughtCount}/{maxCaughtCount})");
        
        if (uiManager != null)
            uiManager.ShowCaughtWarning();
        
        UpdateUI();
        
        if (playerCaughtCount >= maxCaughtCount)
        {
            GameOver(false);
        }
    }
    
    public void DataCoreCollected()
    {
        if (gameOver) return;
        
        collectedDataCores++;
        Debug.Log($"데이터 코어 수집! ({collectedDataCores}/{requiredDataCores})");
        
        UpdateUI();
        
        if (collectedDataCores >= requiredDataCores)
        {
            // 출구 활성화
            GameObject exitDoor = GameObject.FindGameObjectWithTag("ExitDoor");
            if (exitDoor != null)
            {
                exitDoor.GetComponent<ExitDoor>().ActivateExit();
            }
        }
    }
    
    public void PlayerEscaped()
    {
        if (gameOver) return;
        
        if (collectedDataCores >= requiredDataCores)
        {
            GameOver(true);
        }
        else
        {
            Debug.Log("모든 데이터 코어를 수집하지 않았습니다!");
        }
    }
    
    void GameOver(bool victory)
    {
        gameOver = true;
        
        if (victory)
        {
            Debug.Log("게임 승리!");
            if (uiManager != null)
                uiManager.ShowVictoryScreen();
        }
        else
        {
            Debug.Log("게임 오버!");
            if (uiManager != null)
                uiManager.ShowGameOverScreen();
        }
        
        // 3초 후 메인 메뉴로
        Invoke("ReturnToMainMenu", 3f);
    }
    
    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateGameUI(playerCaughtCount, maxCaughtCount, collectedDataCores, requiredDataCores);
        }
    }
    
    public bool IsGameOver() => gameOver;
    public int GetCollectedDataCores() => collectedDataCores;
    public int GetPlayerCaughtCount() => playerCaughtCount;
}