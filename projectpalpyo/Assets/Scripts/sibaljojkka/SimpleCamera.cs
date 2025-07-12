using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    [Header("카메라 설정")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 1.6f, 0); // 플레이어 눈 높이
    
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        // 카메라를 플레이어의 자식으로 만들어서 함께 회전
        transform.SetParent(player);
        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;
    }
}