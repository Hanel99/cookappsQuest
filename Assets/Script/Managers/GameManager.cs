using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 전체적인 게임 진행을 관리.
    // 보드 인게임 로직 등에 관련한건 Board에서 처리

    public static GameManager instance { get; private set; }
    private GameState gameState = GameState.Ready;
    public GameState GameState => gameState;

    [HideInInspector] public int normalBlockScore => 100;
    [HideInInspector] public int specialBlockScore => 500;

    //private
    private int specialBlockCount = 5; // 현재 스테이지 스페셜 블럭 개수
    public int SpecialBlockCount => specialBlockCount;
    private int score = 0;
    private int maxScore = 4000;
    private int leftMoveCount = 40; // 남은 이동 횟수





    void Awake()
    {
        instance = this;

    }


    void Start()
    {
        Application.targetFrameRate = 60;

        specialBlockCount = 5;
        score = 0;
        maxScore = 4000;
        leftMoveCount = 40;

        // 그 외 스크립트에서 Start에서 실행해야 할 메소드들 시작
        Board.instance.Initialize();
        UpdateUI();
        ChangeGameState(GameState.Ready);
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


    public void ChangeGameState(GameState state)
    {
        gameState = state;
    }

    public void AddScore(BlockType type)
    {
        //스코어 상한선은 없음
        if (type == BlockType.Normal)
            score += normalBlockScore;
        else if (type == BlockType.Special)
            score += specialBlockScore;
    }

    public void AddSpecialBlockCount(int value)
    {
        //말은 add인데 깎는용으로 사용
        specialBlockCount += value;
        if (specialBlockCount < 0)
            specialBlockCount = 0;
    }

    public void AddMoveCount(int value)
    {
        leftMoveCount += value;
        if (leftMoveCount < 0)
            leftMoveCount = 0;
    }

    public void UpdateUI()
    {
        UIManager.instance.UpdateLeftSpecialBlockCount(specialBlockCount);
        UIManager.instance.UpdateLeftMoveCount(leftMoveCount);
        float barSize = (float)score / maxScore;
        UIManager.instance.UpdateScore(score, barSize);


        if (specialBlockCount <= 0)
        {
            GameClearProcess();
        }
        else if (leftMoveCount <= 0)
        {
            GameOverProcess();
        }
    }



    public void GameClearProcess()
    {
        //게임 클리어 처리
        ChangeGameState(GameState.GameClear);
        UIManager.instance.ShowGameClearPopup(true);
    }

    public void GameOverProcess()
    {
        //게임 오버 처리
        ChangeGameState(GameState.GameOver);
        UIManager.instance.ShowGameOverPopup(true);
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
