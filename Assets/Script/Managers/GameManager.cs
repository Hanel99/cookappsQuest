using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
