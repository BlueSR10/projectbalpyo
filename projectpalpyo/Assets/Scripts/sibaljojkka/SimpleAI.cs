using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{
    [Header("AI 설정")]
    public float detectionRange = 8f;
    public float loseRange = 12f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    
    [Header("최적화 설정")]
    public float visionAngle = 120f; // 시야각 최적화
    
    // FSM 상태 정의
    public enum AIState
    {
        Patrol, Alert, Chase, Search
    }
    
    [Header("FSM 상태")]
    public AIState currentState = AIState.Patrol;
    
    private NavMeshAgent agent;
    private Transform player;
    private bool isOnNavMesh = false;
    
    // FSM 관련 변수들
    private float stateTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 patrolTarget;
    private float patrolWaitTime = 3f;
    
    // 최적화 변수들
    private float lastDistanceCheck = 0f;
    private float distanceCheckInterval = 0.2f; // 거리 체크 최적화
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        PlaceOnNavMesh();
        
        if (isOnNavMesh)
        {
            agent.speed = patrolSpeed;
            ChangeState(AIState.Patrol);
            
            // Update 분산: 0.1초마다 실행 (최적화 1)
            InvokeRepeating("AIUpdate", Random.Range(0f, 0.1f), 0.1f);
            
            Debug.Log($"{gameObject.name}: 최적화된 FSM AI 시작!");
        }
    }
    
    void PlaceOnNavMesh()
    {
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(transform.position, out hit, 20f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            isOnNavMesh = true;
            Debug.Log($"{gameObject.name}: NavMesh 위 배치 완료!");
            return;
        }
        
        Vector3[] testPositions = {
            new Vector3(0, 0, 0), new Vector3(5, 0, 5), new Vector3(-5, 0, -5),
            new Vector3(3, 0, 0), new Vector3(-3, 0, 0)
        };
        
        foreach (Vector3 testPos in testPositions)
        {
            if (NavMesh.SamplePosition(testPos, out hit, 10f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                isOnNavMesh = true;
                Debug.Log($"{gameObject.name}: 대체 위치에서 NavMesh 배치 완료!");
                return;
            }
        }
        
        Debug.LogError($"{gameObject.name}: NavMesh 찾기 실패!");
    }
    
    // 최적화된 AI 업데이트 (0.1초마다 실행)
    void AIUpdate()
    {
        if (!isOnNavMesh || player == null) return;
        
        // 거리 기반 LOD 최적화 (최적화 2)
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > 50f)
        {
            // 너무 멀면 AI 비활성화
            enabled = false;
            return;
        }
        
        stateTimer += 0.1f; // Update 간격에 맞춤
        bool canSeePlayer = CanSeePlayerOptimized(distance);
        
        // FSM 상태별 처리
        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrolState(canSeePlayer, distance);
                break;
            case AIState.Alert:
                HandleAlertState(canSeePlayer, distance);
                break;
            case AIState.Chase:
                HandleChaseState(canSeePlayer, distance);
                break;
            case AIState.Search:
                HandleSearchState(canSeePlayer, distance);
                break;
        }
        
        // 포획 체크
        if (distance < 2f)
        {
            Debug.Log($"{gameObject.name}: 플레이어 포획!");
        }
    }
    
    // 최적화된 플레이어 감지 (최적화 3: 시야각 + 레이캐스트)
    bool CanSeePlayerOptimized(float distance)
    {
        if (distance > detectionRange) return false;
        
        // 시야각 체크 (뒤에 있으면 감지 안함)
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle > visionAngle * 0.5f) return false; // 120도 시야각
        
        // 거리 체크 간격 최적화
        if (Time.time - lastDistanceCheck < distanceCheckInterval) 
        {
            return distance < detectionRange; // 간단한 거리 체크만
        }
        lastDistanceCheck = Time.time;
        
        // 레이캐스트로 장애물 체크
        Vector3 rayStart = transform.position + Vector3.up * 1.5f;
        RaycastHit hit;
        
        if (Physics.Raycast(rayStart, directionToPlayer, out hit, distance))
        {
            return hit.collider.CompareTag("Player");
        }
        
        return false;
    }
    
    void HandlePatrolState(bool canSeePlayer, float distance)
    {
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            ChangeState(AIState.Alert);
            return;
        }
        
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            if (stateTimer >= patrolWaitTime)
            {
                SetRandomPatrolTarget();
                stateTimer = 0f;
            }
        }
    }
    
    void HandleAlertState(bool canSeePlayer, float distance)
    {
        if (canSeePlayer)
        {
            ChangeState(AIState.Chase);
            return;
        }
        
        if (stateTimer >= 2f)
        {
            ChangeState(AIState.Search);
        }
    }
    
    void HandleChaseState(bool canSeePlayer, float distance)
    {
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(player.position);
            stateTimer = 0f;
        }
        else if (stateTimer >= 3f)
        {
            ChangeState(AIState.Search);
        }
        
        if (distance > loseRange)
        {
            ChangeState(AIState.Search);
        }
    }
    
    void HandleSearchState(bool canSeePlayer, float distance)
    {
        if (canSeePlayer)
        {
            ChangeState(AIState.Chase);
            return;
        }
        
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            Vector3 searchPoint = lastKnownPlayerPosition + Random.insideUnitSphere * 5f;
            searchPoint.y = transform.position.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        
        if (stateTimer >= 10f)
        {
            ChangeState(AIState.Patrol);
        }
    }
    
    void ChangeState(AIState newState)
    {
        Debug.Log($"{gameObject.name}: {currentState} → {newState}");
        
        currentState = newState;
        stateTimer = 0f;
        
        switch (newState)
        {
            case AIState.Patrol:
                agent.speed = patrolSpeed;
                SetRandomPatrolTarget();
                break;
            case AIState.Alert:
                agent.speed = patrolSpeed;
                agent.SetDestination(transform.position);
                break;
            case AIState.Chase:
                agent.speed = chaseSpeed;
                break;
            case AIState.Search:
                agent.speed = patrolSpeed;
                agent.SetDestination(lastKnownPlayerPosition);
                break;
        }
    }
    
    void SetRandomPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
    }
    
    // 최적화 4: 멀어지면 다시 활성화
    void OnBecameVisible()
    {
        enabled = true;
    }
}