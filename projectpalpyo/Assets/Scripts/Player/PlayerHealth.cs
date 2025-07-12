using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 상태")]
    public int maxHealth = 100;
    public float stealthLevel = 1f; // 은신 수준 (0: 완전 노출, 1: 완전 은신)
    
    private int currentHealth;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        UpdateStealthLevel();
    }
    
    void UpdateStealthLevel()
    {
        PlayerController controller = GetComponent<PlayerController>();
        
        // 앉기 상태에서 은신 수준 증가
        if (controller.IsCrouching())
        {
            stealthLevel = Mathf.Min(1f, stealthLevel + Time.deltaTime * 0.5f);
        }
        // 달리기 상태에서 은신 수준 감소
        else if (controller.IsSprinting())
        {
            stealthLevel = Mathf.Max(0.2f, stealthLevel - Time.deltaTime * 0.8f);
        }
        // 일반 이동 시 중간 수준
        else
        {
            stealthLevel = Mathf.Lerp(stealthLevel, 0.7f, Time.deltaTime * 2f);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망!");
        // 게임 오버 처리는 GameManager에서
    }
    
    public float GetStealthLevel() => stealthLevel;
    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;
}
