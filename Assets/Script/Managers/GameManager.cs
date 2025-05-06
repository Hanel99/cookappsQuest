using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 전체적인 게임 진행을 관리.
    // 보드 인게임 로직 등에 관련한건 Board에서 처리

    public static GameManager instance { get; private set; }
    private GameState gameState = GameState.Ready;



    void Awake()
    {
        instance = this;

    }


    void Start()
    {
        gameState = GameState.Ready;

    }






    void Update()
    {
#if UNITY_EDITOR
        // 에디터에서만 사용할 치트
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Board.instance.AddBlockSpawnCount();
        // }
#endif




    }
}
