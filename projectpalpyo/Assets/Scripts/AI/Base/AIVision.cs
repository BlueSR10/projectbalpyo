using UnityEngine;

public class AIVision : MonoBehaviour
{
    [Header("시야 설정")]
    public float viewRadius = 10f;
    public float viewAngle = 90f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    
    public bool CanSeePlayer(Transform player)
    {
        if (player == null) return false;
        
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        
        // 시야각 체크
        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            
            // 거리 체크
            if (distToPlayer <= viewRadius)
            {
                // 장애물 체크
                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // 시야 시각화 (씬 뷰에서)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        
        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2, false);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }
    
    Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}