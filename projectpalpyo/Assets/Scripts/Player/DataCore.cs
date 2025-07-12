using UnityEngine;

public class DataCore : MonoBehaviour
{
    [Header("데이터 코어 설정")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    
    private bool isCollected = false;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // 회전 애니메이션
        StartCoroutine(RotateCore());
    }
    
    System.Collections.IEnumerator RotateCore()
    {
        while (!isCollected)
        {
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
            yield return null;
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // 이펙트 재생
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // 사운드 재생
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // 게임 매니저에 알림
        GameManager.Instance.DataCoreCollected();
        
        // 오브젝트 비활성화
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        // 2초 후 완전 제거 (사운드 재생 시간 확보)
        Destroy(gameObject, 2f);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // UI에 상호작용 힌트 표시
            Debug.Log("E키를 눌러 데이터 코어 수집");
        }
    }
}