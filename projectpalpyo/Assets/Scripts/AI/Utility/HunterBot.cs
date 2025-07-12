using UnityEngine;
using System.Collections.Generic;

public class HunterBot : AIBase
{
    [Header("Hunter 설정")]
    public float huntSpeed = 4f;
    public float ambushRange = 15f;
    
    private List<UtilityAction> availableActions;
    private UtilityAction currentAction;
    private UtilityCalculator calculator;
    
    protected override void Start()
    {
        base.Start();
        calculator = new UtilityCalculator();
        InitializeActions();
        agent.speed = huntSpeed;
    }
    
    void InitializeActions()
    {
        availableActions = new List<UtilityAction>
        {
            new ChaseAction(this),
            new AmbushAction(this),
            new PatrolAction(this),
            new SearchAction(this)
        };
    }
    
    protected override void AIUpdate()
    {
        bool playerDetected = DetectPlayer();
        
        // 각 행동의 유틸리티 계산
        UtilityAction bestAction = null;
        float bestUtility = 0f;
        
        foreach (var action in availableActions)
        {
            float utility = calculator.CalculateUtility(action, this, playerDetected);
            
            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestAction = action;
            }
        }
        
        // 행동 변경 또는 계속 실행
        if (bestAction != currentAction || bestUtility > 0.8f)
        {
            currentAction?.Stop();
            currentAction = bestAction;
            currentAction?.Execute();
        }
        
        currentAction?.Update();
    }
    
    // 외부에서 접근 가능한 속성들
    public bool IsPlayerDetected => isPlayerDetected;
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
    public float DistanceToPlayer => distanceToPlayer;
    public UnityEngine.AI.NavMeshAgent Agent => agent;
    public Transform Player => player;
}