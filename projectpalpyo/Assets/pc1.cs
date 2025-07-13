using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;       // 생성할 오브젝트 (예: 경찰)
    public float spawnInterval = 2f;       // 생성 주기 (초)
    public int maxPoliceCount = 5;         // 맵 위 최대 경찰 수
    public string policeTag = "AI";    // 경찰 오브젝트에 부여된 태그

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 현재 씬에 존재하는 경찰 수 세기
            int currentPolice = GameObject.FindGameObjectsWithTag(policeTag).Length;

            if (currentPolice < maxPoliceCount)
            {
                Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}