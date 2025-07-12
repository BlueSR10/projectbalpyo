using UnityEngine;

public abstract class UtilityAction
{
    protected HunterBot bot;
    
    public UtilityAction(HunterBot bot)
    {
        this.bot = bot;
    }
    
    public abstract void Execute();
    public abstract void Update();
    public abstract void Stop();
}

// 추적 행동
public class ChaseAction : UtilityAction
{
    public ChaseAction(HunterBot bot) : base(bot) { }
    
    public override void Execute()
    {
        if (bot.Player != null)
        {
            bot.Agent.SetDestination(bot.Player.position);
        }
    }
    
    public override void Update()
    {
        if (bot.IsPlayerDetected && bot.Player != null)
        {
            bot.Agent.SetDestination(bot.Player.position);
        }
    }
    
    public override void Stop() { }
}

// 매복 행동
public class AmbushAction : UtilityAction
{
    private Vector3 ambushPosition;
    
    public AmbushAction(HunterBot bot) : base(bot) { }
    
    public override void Execute()
    {
        // 플레이어 예상 경로에 매복지 설정
        Vector3 playerDirection = bot.Player.GetComponent<Rigidbody>().linearVelocity.normalized;
        ambushPosition = bot.Player.position + playerDirection * 10f;
        
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(ambushPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
        }
    }
    
    public override void Update() { }
    public override void Stop() { }
}

// 수색 행동
public class SearchAction : UtilityAction
{
    public SearchAction(HunterBot bot) : base(bot) { }
    
    public override void Execute()
    {
        bot.Agent.SetDestination(bot.LastKnownPlayerPosition);
    }
    
    public override void Update() { }
    public override void Stop() { }
}

// 순찰 행동
public class PatrolAction : UtilityAction
{
    private Vector3[] patrolPoints = new Vector3[4];
    private int currentIndex = 0;
    
    public PatrolAction(HunterBot bot) : base(bot) 
    {
        // 순찰 지점 초기화 (봇 주변 랜덤 위치)
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Vector3 randomPoint = bot.transform.position + Random.insideUnitSphere * 20f;
            randomPoint.y = bot.transform.position.y;
            
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                patrolPoints[i] = hit.position;
            }
            else
            {
                patrolPoints[i] = bot.transform.position;
            }
        }
    }
    
    public override void Execute()
    {
        bot.Agent.SetDestination(patrolPoints[currentIndex]);
    }
    
    public override void Update()
    {
        if (!bot.Agent.pathPending && bot.Agent.remainingDistance < 1f)
        {
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
            bot.Agent.SetDestination(patrolPoints[currentIndex]);
        }
    }
    
    public override void Stop() { }
}