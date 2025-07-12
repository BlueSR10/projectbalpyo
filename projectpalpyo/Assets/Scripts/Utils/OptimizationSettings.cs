using UnityEngine;

[CreateAssetMenu(fileName = "OptimizationSettings", menuName = "AI/Optimization Settings")]
public class OptimizationSettings : ScriptableObject
{
    [Header("LOD 설정")]
    public float lodDistance1 = 20f;  // LOD 0-1 경계
    public float lodDistance2 = 50f;  // LOD 1-2 경계
    
    [Header("업데이트 주기")]
    public float highPriorityInterval = 0.1f;   // LOD 0
    public float mediumPriorityInterval = 0.5f; // LOD 1
    public float lowPriorityInterval = 1.0f;    // LOD 2
    
    [Header("성능 설정")]
    public int maxActiveAI = 10;
    public bool enableNavMeshOptimization = true;
    public bool enableVisionOptimization = true;
    
    [Header("품질 설정")]
    [Range(0.1f, 1f)]
    public float aiQualityMultiplier = 1f;
    
    public float GetUpdateInterval(int lodLevel)
    {
        switch (lodLevel)
        {
            case 0: return highPriorityInterval;
            case 1: return mediumPriorityInterval;
            case 2: return lowPriorityInterval;
            default: return highPriorityInterval;
        }
    }
}