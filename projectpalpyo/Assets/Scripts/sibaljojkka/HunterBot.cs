using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class HunterBot : MonoBehaviour
{
    [Header("AI 설정")]
    public float detectionRange = 10f;
    public float health = 100f;
    public float maxHealth = 100f;
    
    private NavMeshAgent agent;
    private Transform player;
    private List<AIAction> availableActions;
    private AIAction currentAction;
    
   void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        InitializeActions();
        agent.speed = 4f;
        
        // Update 분산: 0.15초마다 실행 (최적화 1)
        InvokeRepeating("OptimizedUpdate", Random.Range(0f, 0.15f), 0.15f);
        
        Debug.Log($"{gameObject.name}: 최적화된 HunterBot 시작!");
    }

    // 기존 Update를 OptimizedUpdate로 변경
    void OptimizedUpdate()
    {
        if (!agent.isOnNavMesh || player == null) return;
        
        // 거리 기반 LOD (최적화 2)
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > 60f)
        {
            enabled = false; // 너무 멀면 비활성화
            return;
        }
        
        // 매 프레임마다 최적의 행동 계산
        AIAction bestAction = CalculateBestAction();
        
        if (bestAction != currentAction)
        {
            currentAction?.Stop();
            currentAction = bestAction;
            currentAction?.Start();
        }
        
        currentAction?.Execute();
        
        // 체력 회복
        if (health < maxHealth)
        {
            health += 1.5f; // 0.15초마다 실행되므로 조정
            health = Mathf.Min(health, maxHealth);
        }
    }

    // 최적화 3: 멀어지면 다시 활성화
    void OnBecameVisible()
    {
        enabled = true;
    }
    
    void InitializeActions()
    {
        availableActions = new List<AIAction>
        {
            new ChaseAction(this),
            new PatrolAction(this),
            new FleeAction(this),
            new AmbushAction(this)
        };
    }
    
    AIAction CalculateBestAction()
    {
        AIAction bestAction = null;
        float bestUtility = 0f;
        
        foreach (var action in availableActions)
        {
            float utility = action.CalculateUtility();
            
            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestAction = action;
            }
        }
        
        return bestAction;
    }
    
    // 데미지 받기 (플레이어가 근처에 있을 때 체력 감소)
    void TakeDamage()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < 3f)
        {
            health -= 20f * Time.deltaTime;
            health = Mathf.Max(health, 0f);
        }
    }
    
    void LateUpdate()
    {
        TakeDamage();
    }
    
    // 다른 스크립트에서 접근할 수 있는 속성들
    public Transform Player => player;
    public NavMeshAgent Agent => agent;
    public float DistanceToPlayer => Vector3.Distance(transform.position, player.position);
    public float HealthPercentage => health / maxHealth;
}

// ============================================
// AI 행동 기본 클래스
// ============================================
public abstract class AIAction
{
    protected HunterBot bot;
    
    public AIAction(HunterBot bot)
    {
        this.bot = bot;
    }
    
    public abstract float CalculateUtility();
    public abstract void Start();
    public abstract void Execute();
    public abstract void Stop();
}

// ============================================
// 추격 행동
// ============================================
public class ChaseAction : AIAction
{
    public ChaseAction(HunterBot bot) : base(bot) { }
    
    public override float CalculateUtility()
    {
        float distance = bot.DistanceToPlayer;
        float health = bot.HealthPercentage;
        
        // 가까이 있고 체력이 높을수록 추격 가치 높음
        float distanceUtility = Mathf.Clamp01(1f - (distance / 15f)); // 15m 이내에서 가치 있음
        float healthUtility = health; // 체력이 높을수록 가치 있음
        
        float utility = distanceUtility * 0.8f + healthUtility * 0.2f;
        
        return distance < 12f ? utility : 0f; // 12m 이내에서만 추격
    }
    
    public override void Start()
    {
        bot.Agent.speed = 5f; // 추격 시 빠르게
        Debug.Log($"{bot.name}: 추격 모드 시작!");
    }
    
    public override void Execute()
    {
        bot.Agent.SetDestination(bot.Player.position);
    }
    
    public override void Stop() { }
}

// ============================================
// 순찰 행동
// ============================================
public class PatrolAction : AIAction
{
    private Vector3 patrolTarget;
    private float patrolTimer = 0f;
    
    public PatrolAction(HunterBot bot) : base(bot) { }
    
    public override float CalculateUtility()
    {
        float distance = bot.DistanceToPlayer;
        float health = bot.HealthPercentage;
        
        // 멀리 있거나 체력이 보통일 때 순찰 가치 높음
        float distanceUtility = Mathf.Clamp01(distance / 20f); // 멀수록 좋음
        float healthUtility = health > 0.3f ? 0.5f : 0.1f; // 체력 30% 이상일 때 순찰
        
        return distanceUtility * 0.6f + healthUtility * 0.4f;
    }
    
    public override void Start()
    {
        bot.Agent.speed = 3f; // 순찰 시 보통 속도
        SetNewPatrolTarget();
        Debug.Log($"{bot.name}: 순찰 모드 시작!");
    }
    
    public override void Execute()
    {
        patrolTimer += Time.deltaTime;
        
        if (!bot.Agent.pathPending && bot.Agent.remainingDistance < 1f || patrolTimer > 8f)
        {
            SetNewPatrolTarget();
            patrolTimer = 0f;
        }
    }
    
    void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 12f;
        randomDirection += bot.transform.position;
        randomDirection.y = bot.transform.position.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 12f, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            bot.Agent.SetDestination(patrolTarget);
        }
    }
    
    public override void Stop() { }
}

// ============================================
// 도망 행동
// ============================================
public class FleeAction : AIAction
{
    public FleeAction(HunterBot bot) : base(bot) { }
    
    public override float CalculateUtility()
    {
        float distance = bot.DistanceToPlayer;
        float health = bot.HealthPercentage;
        
        // 가까이 있고 체력이 낮을수록 도망 가치 높음
        float distanceUtility = distance < 8f ? 1f : 0f; // 8m 이내에서만
        float healthUtility = 1f - health; // 체력 낮을수록 높음
        
        return distanceUtility * healthUtility;
    }
    
    public override void Start()
    {
        bot.Agent.speed = 6f; // 도망 시 가장 빠르게
        Debug.Log($"{bot.name}: 도망 모드 시작!");
    }
    
    public override void Execute()
    {
        // 플레이어 반대 방향으로 도망
        Vector3 fleeDirection = (bot.transform.position - bot.Player.position).normalized;
        Vector3 fleeTarget = bot.transform.position + fleeDirection * 10f;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, 10f, NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
        }
    }
    
    public override void Stop() { }
}

// ============================================
// 매복 행동
// ============================================
public class AmbushAction : AIAction
{
    public AmbushAction(HunterBot bot) : base(bot) { }
    
    public override float CalculateUtility()
    {
        float distance = bot.DistanceToPlayer;
        float health = bot.HealthPercentage;
        
        // 적당한 거리에 있고 체력이 높을 때 매복 가치 있음
        float distanceUtility = (distance > 8f && distance < 15f) ? 0.8f : 0f;
        float healthUtility = health > 0.7f ? 0.6f : 0f;
        
        return distanceUtility * healthUtility * Random.Range(0.3f, 1f); // 랜덤 요소 추가
    }
    
    public override void Start()
    {
        bot.Agent.speed = 2f; // 매복 시 천천히
        Debug.Log($"{bot.name}: 매복 모드 시작!");
    }
    
    public override void Execute()
    {
        // 플레이어 예상 경로에 매복
        Vector3 playerVelocity = bot.Player.GetComponent<Rigidbody>().linearVelocity;
        Vector3 predictedPosition = bot.Player.position + playerVelocity * 3f;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(predictedPosition, out hit, 8f, NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
        }
    }
    
    public override void Stop() { }
}