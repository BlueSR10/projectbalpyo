using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;
    
    [Header("최적화 설정")]
    public int maxActiveAI = 10;
    public float updateInterval = 0.1f;
    public bool enableLOD = true;
    
    private List<AIBase> allAI;
    private List<AIBase> activeAI;
    private Transform player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        allAI = new List<AIBase>();
        activeAI = new List<AIBase>();
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 씬의 모든 AI 수집
        AIBase[] aiComponents = FindObjectsOfType<AIBase>();
        allAI.AddRange(aiComponents);
        
        StartCoroutine(ManageAI());
    }
    
    IEnumerator ManageAI()
    {
        while (true)
        {
            if (enableLOD && player != null)
            {
                UpdateAILOD();
            }
            
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    void UpdateAILOD()
    {
        // 플레이어와의 거리 순으로 정렬
        allAI.Sort((ai1, ai2) => 
        {
            float dist1 = Vector3.Distance(ai1.transform.position, player.position);
            float dist2 = Vector3.Distance(ai2.transform.position, player.position);
            return dist1.CompareTo(dist2);
        });
        
        // 가까운 AI만 활성화
        for (int i = 0; i < allAI.Count; i++)
        {
            if (i < maxActiveAI)
            {
                allAI[i].enabled = true;
            }
            else
            {
                allAI[i].enabled = false;
            }
        }
    }
    
    public void RegisterAI(AIBase ai)
    {
        if (!allAI.Contains(ai))
        {
            allAI.Add(ai);
        }
    }
    
    public void UnregisterAI(AIBase ai)
    {
        allAI.Remove(ai);
        activeAI.Remove(ai);
    }
}