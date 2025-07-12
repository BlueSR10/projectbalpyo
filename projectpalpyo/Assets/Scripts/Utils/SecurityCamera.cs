using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("카메라 설정")]
    public float rotationSpeed = 30f;
    public float rotationRange = 90f;
    public float detectionRange = 15f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    
    [Header("알람 설정")]
    public Light alarmLight;
    public AudioSource alarmSound;
    
    private float currentRotation = 0f;
    private bool rotatingRight = true;
    private bool playerDetected = false;
    private Transform player;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (alarmLight != null)
            alarmLight.color = Color.green;
    }
    
    void Update()
    {
        RotateCamera();
        DetectPlayer();
    }
    
    void RotateCamera()
    {
        if (playerDetected) return; // 플레이어 감지 시 회전 중지
        
        float rotationDelta = rotationSpeed * Time.deltaTime;
        
        if (rotatingRight)
        {
            currentRotation += rotationDelta;
            if (currentRotation >= rotationRange / 2)
            {
                rotatingRight = false;
            }
        }
        else
        {
            currentRotation -= rotationDelta;
            if (currentRotation <= -rotationRange / 2)
            {
                rotatingRight = true;
            }
        }
        
        transform.localRotation = Quaternion.Euler(0, currentRotation, 0);
    }
    
    void DetectPlayer()
    {
        if (player == null) return;
        
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 거리 체크
        if (distToPlayer <= detectionRange)
        {
            // 시야각 체크 (카메라가 플레이어 방향을 보고 있는지)
            if (Vector3.Angle(transform.forward, dirToPlayer) < 30f)
            {
                // 장애물 체크
                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleLayer))
                {
                    // 플레이어의 은신 수준 고려
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    float detectionChance = 1f;
                    
                    if (playerHealth != null)
                    {
                        detectionChance = 1f - playerHealth.GetStealthLevel() * 0.8f; // 은신시 감지 확률 감소
                    }
                    
                    if (Random.Range(0f, 1f) < detectionChance)
                    {
                        TriggerAlarm();
                    }
                }
            }
        }
        else
        {
            // 플레이어가 범위를 벗어나면 알람 해제
            if (playerDetected)
            {
                StopAlarm();
            }
        }
    }
    
    void TriggerAlarm()
    {
        if (playerDetected) return;
        
        playerDetected = true;
        
        // 알람 라이트 빨간색으로 변경
        if (alarmLight != null)
        {
            alarmLight.color = Color.red;
        }
        
        // 알람 사운드 재생
        if (alarmSound != null && !alarmSound.isPlaying)
        {
            alarmSound.Play();
        }
        
        // 주변 AI들에게 플레이어 위치 알림
        NotifyNearbyAI();
        
        Debug.Log("보안 카메라가 플레이어를 감지했습니다!");
    }
    
    void StopAlarm()
    {
        playerDetected = false;
        
        // 알람 라이트 초록색으로 변경
        if (alarmLight != null)
        {
            alarmLight.color = Color.green;
        }
        
        // 알람 사운드 중지
        if (alarmSound != null)
        {
            alarmSound.Stop();
        }
    }
    
    void NotifyNearbyAI()
    {
        // 반경 30m 내의 모든 AI에게 플레이어 위치 정보 전달
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 30f);
        
        foreach (var collider in nearbyColliders)
        {
            AIBase ai = collider.GetComponent<AIBase>();
            if (ai != null)
            {
                // AI에게 플레이어 위치 정보 전달 (protected 멤버에 접근하기 위해 public 메서드 필요)
                ai.GetComponent<MonoBehaviour>().SendMessage("OnPlayerSpotted", player.position, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 시야각 그리기
        Vector3 leftBoundary = Quaternion.Euler(0, -30f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, 30f, 0) * transform.forward * detectionRange;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}