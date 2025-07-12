using System.Collections.Generic;
using System.Linq;

public class GOAPPlanner
{
    public List<GOAPAction> CreatePlan(List<GOAPAction> availableActions, WorldState currentState, Dictionary<string, bool> goals)
    {
        List<GOAPAction> plan = new List<GOAPAction>();
        
        // 간단한 그리디 플래닝 알고리즘
        WorldState workingState = new WorldState(currentState);
        
        while (!GoalsMet(workingState, goals))
        {
            GOAPAction bestAction = null;
            float bestCost = float.MaxValue;
            
            foreach (var action in availableActions)
            {
                if (action.CanExecute(workingState) && action.GetCost() < bestCost)
                {
                    bestAction = action;
                    bestCost = action.GetCost();
                }
            }
            
            if (bestAction != null)
            {
                plan.Add(bestAction);
                bestAction.ApplyEffects(workingState);
                availableActions.Remove(bestAction);
            }
            else
            {
                break; // 더 이상 실행 가능한 행동이 없음
            }
        }
        
        return plan;
    }
    
    bool GoalsMet(WorldState state, Dictionary<string, bool> goals)
    {
        foreach (var goal in goals)
        {
            if (state.GetState(goal.Key) != goal.Value)
            {
                return false;
            }
        }
        return true;
    }
}