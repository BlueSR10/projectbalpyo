using UnityEngine;
using System.Collections.Generic;

public class CommanderBot : AIBase
{
    [Header("Commander 설정")]
    public float commandRange = 30f;
    public GameObject[] subordinateBots;
    
    private GOAPPlanner planner;
    private Queue<GOAPAction> actionQueue;
    private GOAPAction currentAction;
    private WorldState worldState;
    private Dictionary<string, bool> goals;
    
    protected override void Start()
    {
        base.Start();
        
        planner = new GOAPPlanner();
        actionQueue = new Queue<GOAPAction>();
        worldState = new WorldState();
        goals = new Dictionary<string, bool>();
        
        InitializeGoals();
        InitializeWorldState();
    }
    
    void InitializeGoals()
    {
        goals["PlayerCaptured"] = true;
    }
    
    void InitializeWorldState()
    {
        worldState.SetState("PlayerVisible", false);
        worldState.SetState("PlayerCaptured", false);
        worldState.SetState("BotsAvailable", true);
        worldState.SetState("CommanderInPosition", true);
    }
    
    protected override void AIUpdate()
    {
        UpdateWorldState();
        
        // 현재 행동이 없거나 완료되었으면 새 계획 수립
        if (currentAction == null || currentAction.IsComplete())
        {
            CreateNewPlan();
        }
        
        // 현재 행동 실행
        if (currentAction != null)
        {
            currentAction.Execute(this);
        }
    }
    
    void UpdateWorldState()
    {
        bool playerDetected = DetectPlayer();
        worldState.SetState("PlayerVisible", playerDetected);
        
        if (playerDetected)
        {
            lastKnownPlayerPosition = player.position;
        }
        
        // 부하 로봇들의 상태 체크
        bool botsAvailable = CheckSubordinateBotsAvailable();
        worldState.SetState("BotsAvailable", botsAvailable);
    }
    
    bool CheckSubordinateBotsAvailable()
    {
        foreach (var bot in subordinateBots)
        {
            if (bot != null && Vector3.Distance(transform.position, bot.transform.position) <= commandRange)
            {
                return true;
            }
        }
        return false;
    }
    
    void CreateNewPlan()
    {
        // 사용 가능한 행동들
        List<GOAPAction> availableActions = new List<GOAPAction>
        {
            new MoveToPlayerAction(),
            new CommandBotsAction(),
            new SearchAreaAction(),
            new BlockExitAction()
        };
        
        // 계획 수립
        var plan = planner.CreatePlan(availableActions, worldState, goals);
        
        // 계획을 큐에 추가
        actionQueue.Clear();
        foreach (var action in plan)
        {
            actionQueue.Enqueue(action);
        }
        
        // 다음 행동 시작
        if (actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
        }
    }
    
    public void CommandSubordinateBots(Vector3 targetPosition)
    {
        foreach (var bot in subordinateBots)
        {
            if (bot != null)
            {
                var botAI = bot.GetComponent<AIBase>();
                if (Vector3.Distance(transform.position, bot.transform.position) <= commandRange)
                {
                    // 부하 로봇에게 목표 위치 전달
                    bot.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(targetPosition);
                }
            }
        }
    }
    
    public WorldState GetWorldState() => worldState;
    public Vector3 GetLastKnownPlayerPosition() => lastKnownPlayerPosition;
}