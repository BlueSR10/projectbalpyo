using System.Collections.Generic;

public abstract class GOAPAction
{
    protected Dictionary<string, bool> preconditions;
    protected Dictionary<string, bool> effects;
    protected float cost;
    protected bool isComplete;
    
    public GOAPAction()
    {
        preconditions = new Dictionary<string, bool>();
        effects = new Dictionary<string, bool>();
        isComplete = false;
    }
    
    public abstract void Execute(CommanderBot commander);
    public abstract float GetCost();
    
    public bool CanExecute(WorldState state)
    {
        foreach (var condition in preconditions)
        {
            if (state.GetState(condition.Key) != condition.Value)
            {
                return false;
            }
        }
        return true;
    }
    
    public void ApplyEffects(WorldState state)
    {
        foreach (var effect in effects)
        {
            state.SetState(effect.Key, effect.Value);
        }
    }
    
    public bool IsComplete() => isComplete;
    protected void SetComplete() => isComplete = true;
}

// GOAP 행동들 구현
public class MoveToPlayerAction : GOAPAction
{
    public MoveToPlayerAction()
    {
        preconditions["PlayerVisible"] = true;
        effects["CommanderInPosition"] = true;
        cost = 2f;
    }
    
    public override void Execute(CommanderBot commander)
    {
        var agent = commander.GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.SetDestination(commander.GetLastKnownPlayerPosition());
        
        if (!agent.pathPending && agent.remainingDistance < 3f)
        {
            SetComplete();
        }
    }
    
    public override float GetCost() => cost;
}

public class CommandBotsAction : GOAPAction
{
    public CommandBotsAction()
    {
        preconditions["BotsAvailable"] = true;
        preconditions["PlayerVisible"] = true;
        effects["PlayerCaptured"] = true;
        cost = 1f;
    }
    
    public override void Execute(CommanderBot commander)
    {
        commander.CommandSubordinateBots(commander.GetLastKnownPlayerPosition());
        SetComplete();
    }
    
    public override float GetCost() => cost;
}

public class SearchAreaAction : GOAPAction
{
    public SearchAreaAction()
    {
        preconditions["PlayerVisible"] = false;
        effects["PlayerVisible"] = true;
        cost = 3f;
    }
    
    public override void Execute(CommanderBot commander)
    {
        // 랜덤한 수색 지역으로 이동
        var randomPoint = commander.transform.position + UnityEngine.Random.insideUnitSphere * 15f;
        randomPoint.y = commander.transform.position.y;
        
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            commander.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.position);
        }
        
        SetComplete();
    }
    
    public override float GetCost() => cost;
}

public class BlockExitAction : GOAPAction
{
    public BlockExitAction()
    {
        preconditions["BotsAvailable"] = true;
        effects["PlayerCaptured"] = true;
        cost = 4f;
    }
    
    public override void Execute(CommanderBot commander)
    {
        // 출구 근처로 부하 로봇들 배치
        var exitDoor = UnityEngine.GameObject.FindGameObjectWithTag("ExitDoor");
        if (exitDoor != null)
        {
            commander.CommandSubordinateBots(exitDoor.transform.position);
        }
        SetComplete();
    }
    
    public override float GetCost() => cost;
}