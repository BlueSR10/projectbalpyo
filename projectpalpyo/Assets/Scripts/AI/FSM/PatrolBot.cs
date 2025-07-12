using UnityEngine;
using System.Collections.Generic;

public class PatrolBot : AIBase
{
    [Header("순찰 설정")]
    public Transform[] patrolPoints;
    public float waitTime = 2f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    
    private AIState currentState;
    private int currentPatrolIndex = 0;
    private float stateTimer = 0f;
    private Vector3 searchPosition;
    
    protected override void Start()
    {
        base.Start();
        
        // GameObject 생성 등 Unity API 호출은 여기서!
        currentState = AIState.Patrol;
        agent.speed = patrolSpeed;
        
        // 순찰 포인트가 설정되어 있다면 첫 번째 지점으로 이동
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
        else
        {
            // 순찰 포인트가 없다면 자동으로 생성
            CreateDefaultPatrolPoints();
        }
    }
    
    // 기본 순찰 포인트를 자동 생성하는 메서드
    void CreateDefaultPatrolPoints()
    {
        List<Transform> points = new List<Transform>();
        
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject($"PatrolPoint_{i}");
            point.transform.position = transform.position + new Vector3(
                Random.Range(-10f, 10f), 
                0, 
                Random.Range(-10f, 10f)
            );
            points.Add(point.transform);
        }
        
        patrolPoints = points.ToArray();
        agent.SetDestination(patrolPoints[0].position);
    }
    
    protected override void AIUpdate()
    {
        stateTimer += updateInterval;
        
        // 플레이어 감지
        bool playerDetected = DetectPlayer();
        
        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrolState(playerDetected);
                break;
            case AIState.Alert:
                HandleAlertState(playerDetected);
                break;
            case AIState.Chase:
                HandleChaseState(playerDetected);
                break;
            case AIState.Search:
                HandleSearchState(playerDetected);
                break;
        }
    }
    
    void HandlePatrolState(bool playerDetected)
    {
        if (playerDetected)
        {
            ChangeState(AIState.Alert);
            return;
        }
        
        // 순찰 포인트 도달 체크
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (stateTimer >= waitTime)
            {
                MoveToNextPatrolPoint();
                stateTimer = 0f;
            }
        }
    }
    
    void HandleAlertState(bool playerDetected)
    {
        if (playerDetected)
        {
            ChangeState(AIState.Chase);
        }
        else if (stateTimer >= 3f) // 3초 후 순찰로 복귀
        {
            ChangeState(AIState.Patrol);
        }
    }
    
    void HandleChaseState(bool playerDetected)
    {
        if (playerDetected)
        {
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(lastKnownPlayerPosition);
            stateTimer = 0f;
        }
        else if (stateTimer >= 5f) // 5초간 시야를 잃으면 수색
        {
            searchPosition = lastKnownPlayerPosition;
            ChangeState(AIState.Search);
        }
        
        // 플레이어 포획 체크
        if (player != null && Vector3.Distance(transform.position, player.position) < 2f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerCaught();
            }
        }
    }
    
    void HandleSearchState(bool playerDetected)
    {
        if (playerDetected)
        {
            ChangeState(AIState.Chase);
        }
        else if (stateTimer >= 10f) // 10초간 수색 후 순찰 복귀
        {
            ChangeState(AIState.Patrol);
        }
        
        // 수색 위치 도달 시 주변 탐색
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            randomDirection += searchPosition;
            randomDirection.y = transform.position.y;
            
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    
    void ChangeState(AIState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        switch (newState)
        {
            case AIState.Patrol:
                agent.speed = patrolSpeed;
                break;
            case AIState.Chase:
                agent.speed = chaseSpeed;
                break;
            case AIState.Search:
                agent.speed = patrolSpeed;
                agent.SetDestination(searchPosition);
                break;
        }
    }
    
    void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }
    
    // 카메라에 의해 플레이어가 발견되었을 때 호출
    public void OnPlayerSpotted(Vector3 playerPosition)
    {
        lastKnownPlayerPosition = playerPosition;
        isPlayerDetected = true;
        
        if (currentState == AIState.Patrol)
        {
            ChangeState(AIState.Alert);
        }
    }
}