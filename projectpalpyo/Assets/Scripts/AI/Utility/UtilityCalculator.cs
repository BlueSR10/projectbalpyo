using UnityEngine;

public class UtilityCalculator
{
    public float CalculateUtility(UtilityAction action, HunterBot bot, bool playerDetected)
    {
        float utility = 0f;
        
        switch (action.GetType().Name)
        {
            case "ChaseAction":
                if (playerDetected)
                    utility = 1f - (bot.DistanceToPlayer / bot.detectionRange);
                break;
                
            case "AmbushAction":
                if (playerDetected && bot.DistanceToPlayer > 10f)
                    utility = 0.7f;
                break;
                
            case "SearchAction":
                if (!playerDetected && bot.LastKnownPlayerPosition != Vector3.zero)
                    utility = 0.5f;
                break;
                
            case "PatrolAction":
                utility = 0.2f; // 기본 행동
                break;
        }
        
        return Mathf.Clamp01(utility);
    }
}