using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 전체적인 게임 진행을 관리.
    // 보드 인게임 로직 등에 관련한건 Board에서 처리

    public static GameManager instance { get; private set; }
    private GameState gameState = GameState.Ready;
    public GameState GameState => gameState;

    public int specialBlockCount = 5; // 현재 스테이지 스페셜 블럭 개수
    public int score = 0;
    public int maxScore = 1000;
    public int leftMoveCount = 40; // 남은 이동 횟수




    void Awake()
    {
        instance = this;

    }


    void Start()
    {
        gameState = GameState.Ready;

        specialBlockCount = 5;
        score = 0;
        maxScore = 1000;
        leftMoveCount = 40;

        // 그 외 스크립트에서 Start에서 실행해야 할 메소드들 시작
        Board.instance.Initialize();
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
