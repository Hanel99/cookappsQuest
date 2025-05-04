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
    // 보드 인게임 로직 등에 관련을 처리
    // 게임 진행이나 상황 등은 GameManager에서 관리

    public static Board instance;


    [Header("Transform")]
    public Transform blockRoot;
    private Vector2Int rootNodePoint = new Vector2Int(3, 0); // 블럭이 스폰될 노드 포지션
    public List<Transform> blockPositionList = new List<Transform>();                 // 블럭, 노드 위치 포지션 저장할 리스트
    public Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>(); // 노드 찾아오기용 노드맵
    public List<Node> nodeList = new(); //디버그용 노드 리스트. 딕셔너리 안보여서... @@@ 다 만들고 삭제 필요


    [Header("Block")]
    public List<Block> blockList = new List<Block>(); // 블럭 리스트    


    [Header("CanvasData")]
    public float canvasScaleFactor;


    //private

    private readonly int boardXSize = 7;      // 가로 블럭 칸 개수
    private readonly int boardYSize = 7;      // 세로 블럭 칸 개수
    private bool isMatchingAnimationAct = false; // 액션 중에는 터치 불가능하게

    private Coroutine blockDropCoroutine = null; // 블럭 드랍 코루틴 
    private float moveSearchDuration = 0.3f; // 다음 블럭 서치 속도
    private int blockSpawnReadyCount = 0; // 블럭 스폰해야 할 카운트. 1 이상이면 update에서 블록을 스폰하려고 시도.




    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Initialize();
        canvasScaleFactor = transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void Initialize()
    {
        SetNodeMap();
        InitBoardBlock();
    }

    private void InitBoardBlock()
    {
        // 초기 블럭들 배치

        blockList.Clear();

        //@@@ 3개 이상 연결된 블럭이 있으면 안됨.

        foreach (var node in nodeMap)
        {
            if (node.Value.isOn)
            {
                BlockColor color = (BlockColor)UnityEngine.Random.Range(0, (int)BlockColor.Count);

                SpawnBlock(BlockType.Normal, color, node.Key);
            }
        }
    }




    void Update()
    {
        //@@@ TODO 일단 이렇게 해놓고 최적화는 나중에 처리하기로...

        if (blockSpawnReadyCount > 0)
        {
            if (nodeMap[rootNodePoint].block == null)
            {
                BlockColor color = (BlockColor)UnityEngine.Random.Range(0, (int)BlockColor.Count);
                var block = SpawnBlock(BlockType.Normal, color);
                block?.CheckMoveable();
                --blockSpawnReadyCount;
            }
            else
            {
                nodeMap[rootNodePoint].block.CheckMoveable();
            }
        }
    }

    public void AddBlockSpawnCount(int count = 1)
    {
        blockSpawnReadyCount += count;
    }




    #region Node

    public void SetNodeMap()
    {
        nodeMap.Clear();
        nodeList.Clear();

        // 노드 생성 및 셋팅
        for (int x = 0; x < boardXSize; ++x)
        {
            for (int y = 0; y < boardYSize; ++y)
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
        //맨 왼쪽칸부터 오른쪽 순으로 x -> 0 1 ... boardXSize
        //맨 위쪽칸부터 아래칸 순으로 y -> 0 1 ... boardYSize 까지
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
        bool isValid = point.x >= 0 && point.x < boardXSize
                    && point.y >= 0 && point.y < boardYSize;

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
    public Block SpawnBlock(BlockType type, BlockColor color, Vector2Int? nodePoint = null)
    {
        if (nodePoint == null)
            nodePoint = rootNodePoint;

        if (nodeMap[nodePoint.Value].block != null)
        {
            HLLogger.Log("@@@ block is already exist");
            return null;
        }

        Block block = ObjectPool.instance.Spawn();
        block.name = $"block_{blockSpawnCount.ToString("D3")}";

        block.SetBlockData(nodePoint.Value, type, color);
        block.transform.SetParent(blockRoot);
        block.transform.localPosition = GetBlockPosition(nodePoint.Value);

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
        return blockPositionList[nodePoint.y + (nodePoint.x * boardXSize)].localPosition;
    }


    #endregion


    #region SearchLogic



    void MatchCheckAllBlock()
    {
        // 모든 블럭이 3개 이상 연결되었는지를 체크


    }

    bool MatchCheck(List<Vector2Int> nodePoints)
    {
        // 해당 좌표 블럭이 3개 이상 연결되었는지를 체크
        // 블럭이 3개 이상, 5개 이하 연결된 경우 매치 처리

        foreach (var nodePoint in nodePoints)
        {
            if (nodeMap[nodePoint] == null || nodeMap[nodePoint].block == null)
                return false;
        }

        // 해당 좌표 블럭이 3개 이상 연결되었는지를 체크



        return false;
    }


    public void CheckMoveableAllBlock(Vector2Int? startPoint = null)
    {
        if (blockDropCoroutine != null)
            StopCoroutine(blockDropCoroutine);

        blockDropCoroutine = StartCoroutine(Co_CheckMoveableAllBlock(startPoint));
    }



    IEnumerator Co_CheckMoveableAllBlock(Vector2Int? startPoint)
    {
        // 시작 지점부터 모든 노드를 순회해서 떨어질 수 있는 블럭이 있으면 순차적으로 드랍.
        // 시작지점이 null이면 최하단 노드부터 시작.

        int startY = startPoint != null ? startPoint.Value.y : boardYSize - 1;

        //1. y 시작 y좌표 또는 boardYSize ~ 0 순회
        //2. 홀수인 x boardXSize ~ 0 순회
        //3. 짝수인 x boardXSize ~ 0 순회
        //4. 0 0 에 도착하면 멈춤.
        //5. 1~3 을 반복할때 일정 시간 딜레이 간격이 필요

        for (int y = startY; y >= 0; --y)
        {
            for (int x = 1; x < boardXSize; x += 2)
            {
                Vector2Int point = new Vector2Int(x, y);
                nodeMap[point]?.block?.CheckMoveable();
            }
            // yield return new WaitForSeconds(moveSearchDuration);

            for (int x = 0; x < boardXSize; x += 2)
            {
                Vector2Int point = new Vector2Int(x, y);
                nodeMap[point]?.block?.CheckMoveable();
            }

            yield return new WaitForSeconds(moveSearchDuration);
        }


    }

    void CheckMatchableAllBlock()
    {
        // 시작 지점부터 모든 노드를 순회해서 매치가 가능한 블럭이 있는지를 확인.
        // 








    }


    #endregion



    #region SwapLogic


    public void SwapBlock(Block blockA, Block blockB)
    {
        HLLogger.Log($"SwapBlock {blockA.nodePoint} <-> {blockB.nodePoint}");

        var tempNodePoint = blockA.node.point;

        blockA.ResetNode(blockB.node.point);
        blockB.ResetNode(tempNodePoint);

        // 블럭 스왑 후 매치 체크
        List<Vector2Int> checkList = new List<Vector2Int>() { blockA.nodePoint, blockB.nodePoint };
        blockA.DoMoveAnimation(blockA.node.point);
        blockB.DoMoveAnimation(blockB.node.point, () => { MatchCheck(checkList); });
    }


    #endregion

}
