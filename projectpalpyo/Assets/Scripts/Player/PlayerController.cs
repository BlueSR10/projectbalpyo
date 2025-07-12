using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float jumpForce = 5f;
    
    [Header("회전 설정")]
    public float rotationSpeed = 10f;
    
    private Rigidbody rb;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;
    private float currentSpeed;
    
    // 카메라는 Cinemachine이 자동으로 관리
    private Camera mainCamera;
    
    // 오디오 관련
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.5f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main; // Cinemachine이 제어하는 메인 카메라
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentSpeed = moveSpeed;
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleFootsteps();
    }
    
    void HandleInput()
    {
        // 달리기 (Input Manager의 Fire1 = Left Shift)
        isSprinting = Input.GetButton("Fire1") && !isCrouching;
        
        // 앉기 (Input Manager의 Fire2 = Left Ctrl)
        if (Input.GetButtonDown("Fire2"))
        {
            isCrouching = !isCrouching;
            transform.localScale = isCrouching ? 
                new Vector3(1f, 0.5f, 1f) : Vector3.one;
        }
        
        // 점프 (Input Manager의 Jump = Space)
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        // 메뉴/취소 (Input Manager의 Cancel = ESC)
        if (Input.GetButtonDown("Cancel"))
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("메뉴 토글 요청");
            }
            else
            {
                Application.Quit(); // 개발 중에만 즉시 종료
            }
        }
    }
    
    void HandleMovement()
    {
        // Input Manager의 Horizontal/Vertical 사용
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 현재 속도 결정
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = moveSpeed;
        
        // 카메라 기준으로 이동 방향 계산 (Cinemachine 카메라 사용)
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        
        // Y축 제거 (수평 이동만)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // 이동 벡터 계산
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Rigidbody에 적용
        rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        
        // 이동 방향으로 플레이어 회전
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void HandleFootsteps()
    {
        // 이동 중일 때만 발소리
        if (rb.linearVelocity.magnitude > 0.1f && isGrounded)
        {
            footstepTimer += Time.deltaTime;
            
            // 속도에 따른 발소리 간격 조정
            float currentInterval = footstepInterval / (currentSpeed / moveSpeed);
            
            if (footstepTimer >= currentInterval)
            {
                // 발소리 재생 (오디오 클립이 있다면)
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.Play();
                }
                
                footstepTimer = 0f;
            }
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        isGrounded = collision.gameObject.CompareTag("Ground") || 
                    collision.gameObject.CompareTag("Floor");
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || 
            collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
        }
    }
    
    // AI가 플레이어의 이동 속도를 참조할 수 있도록
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsCrouching() => isCrouching;
    public bool IsSprinting() => isSprinting;
}