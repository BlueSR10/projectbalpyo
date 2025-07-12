using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{
    [Header("AI 설정")]
    public float detectionRange = 8f;
    public float moveSpeed = 3f;
    
    private NavMeshAgent agent;
    private Transform player;
    private bool isOnNavMesh = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // NavMesh 찾기 (범위 대폭 확장)
        PlaceOnNavMesh();
        
        if (isOnNavMesh)
        {
            agent.speed = moveSpeed;
            Debug.Log($"{gameObject.name}: AI 준비 완료!");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: NavMesh 못 찾음. 수동으로 위치 조정 필요!");
        }
    }
    
    void PlaceOnNavMesh()
    {
        NavMeshHit hit;
        
        // 1차: 현재 위치에서 넓은 범위로 검색
        if (NavMesh.SamplePosition(transform.position, out hit, 20f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            isOnNavMesh = true;
            Debug.Log($"{gameObject.name}: NavMesh 위 배치 완료!");
            return;
        }
        
        // 2차: 여러 위치에서 시도
        Vector3[] testPositions = {
            new Vector3(0, 0, 0),      // 중앙
            new Vector3(5, 0, 5),      // 우상
            new Vector3(-5, 0, -5),    // 좌하
            new Vector3(3, 0, 0),      // 우측
            new Vector3(-3, 0, 0)      // 좌측
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
        
        Debug.LogError($"{gameObject.name}: 모든 위치에서 NavMesh 찾기 실패!");
    }
    
    void Update()
    {
        if (!isOnNavMesh || player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        // 간단한 추적 (오류 방지)
        if (distance < detectionRange)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                Debug.Log($"{gameObject.name}: 플레이어 추격 중!");
            }
        }
        
        // 포획 체크
        if (distance < 2f)
        {
            Debug.Log("AI에게 잡혔습니다!");
        }
    }
}