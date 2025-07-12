using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class AIBase : MonoBehaviour
{
    [Header("AI 기본 설정")]
    public float detectionRange = 10f;
    public float fieldOfView = 60f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    
    [Header("최적화 설정")]
    public int lodLevel = 0;
    public float updateInterval = 0.1f;
    
    protected NavMeshAgent agent;
    protected Transform player;
    protected AIVision vision;
    protected bool isPlayerDetected = false;
    protected Vector3 lastKnownPlayerPosition;
    
    // 최적화용 변수
    protected float distanceToPlayer;
    protected bool isInUpdateRange = true;
    
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<AIVision>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 최적화된 업데이트 시작
        StartCoroutine(OptimizedUpdate());
    }
    
    // 최적화된 업데이트 시스템
    protected virtual IEnumerator OptimizedUpdate()
    {
        while (true)
        {
            // 플레이어와의 거리 계산
            if (player != null)
            {
                distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                // LOD 레벨 결정
                if (distanceToPlayer < 20f)
                    lodLevel = 0; // 전체 기능
                else if (distanceToPlayer < 50f)
                    lodLevel = 1; // 기본 기능만
                else
                    lodLevel = 2; // 비활성화
                
                // LOD에 따른 업데이트 간격 조정
                updateInterval = lodLevel == 0 ? 0.1f : (lodLevel == 1 ? 0.5f : 1.0f);
                
                // LOD 2가 아닐 때만 AI 업데이트
                if (lodLevel < 2)
                {
                    AIUpdate();
                }
            }
            
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    // 각 AI가 구현해야 하는 업데이트 메서드
    protected abstract void AIUpdate();
    
    // 플레이어 감지 공통 로직
    protected bool DetectPlayer()
    {
        if (player == null || lodLevel > 1) return false;
        
        // LOD 1에서는 간단한 거리 체크만
        if (lodLevel == 1)
        {
            return distanceToPlayer <= detectionRange;
        }
        
        // LOD 0에서는 완전한 감지 시스템
        return vision.CanSeePlayer(player);
    }

    public void OnPlayerSpotted(Vector3 playerPosition)
    {
        lastKnownPlayerPosition = playerPosition;
        isPlayerDetected = true;
        
        // AI별 대응 로직
        OnPlayerSpottedByCamera(playerPosition);
    }

    protected virtual void OnPlayerSpottedByCamera(Vector3 playerPosition)
    {
        // 각 AI 클래스에서 오버라이드하여 구현
    }
}

