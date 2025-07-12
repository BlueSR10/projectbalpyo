using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [Header("출구 설정")]
    public GameObject lockedEffect;
    public GameObject unlockedEffect;
    public AudioClip unlockSound;
    
    private bool isUnlocked = false;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (lockedEffect != null)
            lockedEffect.SetActive(true);
            
        if (unlockedEffect != null)
            unlockedEffect.SetActive(false);
    }
    
    public void ActivateExit()
    {
        if (isUnlocked) return;
        
        isUnlocked = true;
        
        // 이펙트 전환
        if (lockedEffect != null)
            lockedEffect.SetActive(false);
            
        if (unlockedEffect != null)
            unlockedEffect.SetActive(true);
        
        // 사운드 재생
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        Debug.Log("출구가 활성화되었습니다!");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isUnlocked)
        {
            GameManager.Instance.PlayerEscaped();
        }
        else if (other.CompareTag("Player") && !isUnlocked)
        {
            Debug.Log("모든 데이터 코어를 수집해야 출구를 열 수 있습니다!");
        }
    }
}