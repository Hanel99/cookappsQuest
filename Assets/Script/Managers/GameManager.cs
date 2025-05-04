using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 전체적인 게임 진행을 관리.
    // 보드 인게임 로직 등에 관련한건 Board에서 처리

    public static GameManager instance { get; private set; }



    void Awake()
    {
        instance = this;

    }


    void Start()
    {

    }






    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBlock(BlockType.Normal, BlockColor.Red);
        }
    }




    void SpawnBlock(BlockType type, BlockColor color)
    {
        // 블럭 생성
        var block = Board.instance.SpawnBlock(type, color);
        block?.CheckMoveable();
    }
}
