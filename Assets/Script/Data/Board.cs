using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance;


    [Header("Transform")]
    public Transform blockRoot;
    private Vector2Int rootNodePoint = new Vector2Int(3, 0); // 블럭이 스폰될 노드 포지션
    public List<Transform> blockPositionList = new List<Transform>();                 // 블럭, 노드 위치 포지션 저장할 리스트
    public Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>(); // 노드 찾아오기용 노드맵
    public List<Node> nodeList = new(); //디버그용 노드 리스트. 딕셔너리 안보여서...


    [Header("Block")]
    public List<Block> blockList = new List<Block>(); // 블럭 리스트    






    private bool isMatchingAnimationAct = false;

    // private float blockMergeSpeed = 1f;
    // private float mergeDuration => mergeTime * blockMergeSpeed;
    // private float moveDuration => blockMoveTime * blockMergeSpeed;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetNodeMap();
        blockList.Clear();
    }


    #region Node



    public void SetNodeMap()
    {
        nodeMap.Clear();
        nodeList.Clear();

        // 노드 생성 및 셋팅
        for (int x = 0; x < StaticGameData.BoardXSize; ++x)
        {
            for (int y = 0; y < StaticGameData.BoardYSize; ++y)
            {
                var point = new Vector2Int(x, y);
                Node node = new Node(point, GetLinkedNode(point));
                node.isOn = IsValid(point);

                nodeMap.Add(point, node);
                nodeList.Add(node);
            }
        }
    }

    private Vector2Int?[] GetLinkedNode(Vector2Int point)
    {
        //맨 왼쪽칸부터 오른쪽 순으로 x -> 0 1 ... StaticGameData.BoardXSize
        //맨 위쪽칸부터 아래칸 순으로 y -> 0 1 ... StaticGameData.BoardYSize 까지
        //x가 홀수일 경우를 정 위치로 기준. 짝수일 경우 y를 좀 더 내린 포지션으로 생각한다.

        // x가 홀수인 경우와 짝수인 경우 연결 노드 공식이 다르다.
        // (1,1)를 예시로 들면, (1,0), (0,1), (2,1), (1,2), (0,2), (2,2) 순으로 연결된 노드가 있다.
        // (x,y) => (x,y-1), (x-1,y), (x+1,y), (x,y+1), (x-1,y+1), (x+1,y+1)

        // (2,2)를 예시로 들면, (2,1), (1,1), (3,1), (2,3), (1,2), (3,2) 순으로 연결된 노드가 있다.
        // (x,y) => (x,y-1), (x-1,y-1), (x+1,y-1), (x,y+1), (x-1,y), (x+1,y)

        int calibrationValue = point.x % 2 == 0 ? -1 : 0;

        Vector2Int UP = point + new Vector2Int(0, -1);
        Vector2Int UPLEFT = point + new Vector2Int(-1, calibrationValue);
        Vector2Int UPRIGHT = point + new Vector2Int(1, calibrationValue);
        Vector2Int DOWN = point + new Vector2Int(0, 1);
        Vector2Int DOWNLEFT = point + new Vector2Int(-1, calibrationValue + 1);
        Vector2Int DOWNRIGHT = point + new Vector2Int(1, calibrationValue + 1);

        Vector2Int?[] v = new Vector2Int?[6]; // valid가 아니라면 null로 저장
        if (IsValid(UP)) v[0] = UP;
        if (IsValid(UPLEFT)) v[1] = UPLEFT;
        if (IsValid(UPRIGHT)) v[2] = UPRIGHT;
        if (IsValid(DOWN)) v[3] = DOWN;
        if (IsValid(DOWNLEFT)) v[4] = DOWNLEFT;
        if (IsValid(DOWNRIGHT)) v[5] = DOWNRIGHT;
        return v;
    }

    private bool IsValid(Vector2Int point)
    {
        // 해당 노드가 유효한지 확인
        bool isValid = point.x >= 0 && point.x < StaticGameData.BoardXSize
             && point.y >= 0 && point.y < StaticGameData.BoardYSize;

        // 21 스테이지에서 사용하지 않는 특수 노드의 경우
        if (point.x == 0 && point.y == 0
            || point.x == 0 && point.y == 1
            || point.x == 0 && point.y == 2
            || point.x == 0 && point.y == 6

            || point.x == 1 && point.y == 0
            || point.x == 1 && point.y == 1
            || point.x == 1 && point.y == 6

            || point.x == 2 && point.y == 0
            || point.x == 2 && point.y == 1

            // || point.x == 3 && point.y == 0 // 블록 스폰할 노드

            || point.x == 4 && point.y == 0
            || point.x == 4 && point.y == 1

            || point.x == 5 && point.y == 0
            || point.x == 5 && point.y == 1
            || point.x == 5 && point.y == 6

            || point.x == 6 && point.y == 0
            || point.x == 6 && point.y == 1
            || point.x == 6 && point.y == 2
            || point.x == 6 && point.y == 6
            )
        {
            isValid = false;
        }

        return isValid;
    }




    public Node GetNode(Vector2Int? point)
    {
        Node node = null;
        if (point != null)
            node = nodeMap[point.Value];

        return node;
    }

    #endregion Node



    #region Block

    int blockSpawnCount = 0;
    public Block SpawnBlock(BlockType type, BlockColor color)
    {
        if (nodeMap[rootNodePoint].block != null)
        {
            HLLogger.Log("@@@ block is already exist");
            return null;
        }

        Block block = ObjectPool.instance.Spawn();
        block.name = $"block_{blockSpawnCount.ToString("D3")}";

        block.SetBlockData(rootNodePoint, type, color);
        block.transform.SetParent(blockRoot);
        block.transform.localPosition = GetBlockPosition(rootNodePoint);

        blockSpawnCount++;
        blockList.Add(block);
        return block;
    }

    public void RecycleBlock(Block block)
    {
        blockList.Remove(block);
        ObjectPool.instance.Recycle(block);
    }

    public Vector2 GetBlockPosition(Vector2Int nodePoint)
    {
        return blockPositionList[nodePoint.y + (nodePoint.x * StaticGameData.BoardXSize)].localPosition;
    }


    #endregion

}
