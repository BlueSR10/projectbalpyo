using UnityEngine;

public class DataCollector : MonoBehaviour
{
    [Header("수집 설정")]
    public float collectionRange = 2f;
    
    private Camera mainCamera; // Cinemachine이 제어하는 메인 카메라 사용
    
    void Start()
    {
        mainCamera = Camera.main; // Cinemachine Brain이 있는 메인 카메라
    }
    
    void Update()
    {
        CheckForDataCore();
    }
    
    void CheckForDataCore()
    {
        // Input Manager의 Fire3 (E키) 사용
        if (Input.GetButtonDown("Fire3"))
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, collectionRange))
            {
                DataCore dataCore = hit.collider.GetComponent<DataCore>();
                if (dataCore != null)
                {
                    dataCore.Collect();
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}