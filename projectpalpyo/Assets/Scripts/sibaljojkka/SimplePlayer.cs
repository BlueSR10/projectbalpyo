using UnityEngine;

public class SimplePlayer : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 5f;
    public float mouseSensitivity = 100f;
    
    private Rigidbody rb;
    private Camera playerCamera;
    private float xRotation = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleInteraction();
    }
    
    void HandleMouseLook()
    {
        // 마우스 델타 직접 계산 (Input System 우회)
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        // 마우스 입력이 없으면 종료
        if (mouseDelta.magnitude == 0) return;
        
        // 마우스 민감도 적용
        mouseDelta *= mouseSensitivity * Time.deltaTime;
        
        // Y축 회전 (좌우 보기) - 플레이어 회전
        transform.Rotate(Vector3.up * mouseDelta.x);
        
        // X축 회전 (상하 보기) - 카메라 회전
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
    
    void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // WASD 입력
        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        
        // 이동 방향 계산 (플레이어가 바라보는 방향 기준)
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        direction = direction.normalized;
        
        // Rigidbody로 이동
        rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);
    }
    
    void HandleInteraction()
    {
        // E키로 목표물 수집
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 2f);
            foreach (var obj in nearbyObjects)
            {
                if (obj.name == "Target")
                {
                    obj.gameObject.SetActive(false);
                    Debug.Log("Target Collected! You Win!");
                }
            }
        }
        
        // ESC로 마우스 잠금/해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}