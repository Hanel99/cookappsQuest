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
    public Vector2Int RootNodePoint => rootNodePoint;
    public List<Transform> blockPositionList = new List<Transform>();                 // 블럭, 노드 위치 포지션 저장할 리스트
    public Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>(); // 노드 찾아오기용 노드맵
    public List<Node> nodeList = new(); //디버그용 노드 리스트. 딕셔너리 안보여서... @@@ 다 만들고 삭제 필요


    [Header("Block")]
    public List<Block> blockList = new List<Block>(); // 블럭 리스트    
    public List<Block> specialBlockList = new List<Block>(); // 특수 블럭(팽이) 리스트



    //private

    private readonly int boardXSize = 7;      // 가로 블럭 칸 개수
    private readonly int boardYSize = 7;      // 세로 블럭 칸 개수
    private bool isMatchingAnimationAct = false; // 액션 중에는 터치 불가능하게... ui 전체 커버 이미지로 덮어도 되는데 이렇게 구현
    public bool IsMatchingAnimationAct => isMatchingAnimationAct;

    private Coroutine blockDropCoroutine = null; // 블럭 드랍 코루틴 
    private float moveSearchDuration = 0.15f; // 다음 블럭 서치 속도
    private int blockSpawnReadyCount = 0; // 블럭 스폰해야 할 카운트. 1 이상이면 update에서 블록을 스폰하려고 시도.

    private HashSet<Vector2Int> matchNodePointList = new HashSet<Vector2Int>(); // 매치된 블록들. 스왑 및 삭제 로직에서 사용




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
        if (block.blockType != BlockType.Normal) blockList.Add(block);

        return block;
    }

    public Vector2 GetBlockPosition(Vector2Int nodePoint)
    {
        return blockPositionList[nodePoint.y + (nodePoint.x * boardXSize)].localPosition;
    }



    public void RemoveMatchBlocks()
    {
        isMatchingAnimationAct = true;
        foreach (var point in matchNodePointList)
        {
            // 매치된 블럭들에 대해서는 블럭을 삭제
            var block = nodeMap[point].block;
            if (block != null)
            {
                if (moveCheckStartPoint.y < point.y)
                    moveCheckStartPoint = new Vector2Int(0, point.y);

                blockList.Remove(block);
                block.RemoveBlock();
            }
        }

        // 블럭 드랍 체크
        CheckMoveableAllBlock(moveCheckStartPoint);
        matchNodePointList.Clear();
    }


    #endregion






    #region SearchLogic


    void MatchCheckAllBlock()
    {
        // 모든 블럭이 3개 이상 연결되었는지를 체크
        bool isMatch = false;

        foreach (var node in nodeMap)
        {
            if (node.Value != null)
            {
                var nodePoints = node.Value.point;
                isMatch = isMatch || MatchCheck(nodePoints);
            }
        }

        if (isMatch)
        {
            var s1 = string.Join(", ", matchNodePointList.Select(p => p.ToString()).ToArray());
            HLLogger.Log($"MatchCheckAllBlock : {isMatch} - {s1}");
            RemoveMatchBlocks();
        }
        else
            isMatchingAnimationAct = false;
    }

    bool MatchCheck(List<Vector2Int> nodePoints)
    {
        bool isMatch = false;

        foreach (var nodePoint in nodePoints)
        {
            isMatch = MatchCheck(nodePoint) || isMatch;
        }

        return isMatch;
    }

    bool MatchCheck(Vector2Int nodePoint)
    {
        // 해당 좌표 블럭이 3개 이상 연결되었는지를 체크

        // 우선 널처리...
        if (nodeMap[nodePoint] == null || nodeMap[nodePoint].block == null)
            return false;

        var block = nodeMap[nodePoint].block;

        // 특수블럭인 경우 검사하지 않음
        if (block.blockType != BlockType.Normal)
            return false;

        Node node1 = null;
        Node node2 = null;
        BlockColor color = block.blockColor;
        bool isMatch = false;


        //1 각 방향으로 두개 동일 블럭이 있는지 체크
        for (Direction dir = Direction.UP; dir < Direction.ERROR; ++dir)
        {
            //해당 방향 노드가 없거나, 블럭이 없거나, 다른 블럭이면 다음 방향 검사
            node1 = nodeMap[nodePoint].GetNode(dir);
            if (node1 == null || node1.block == null || node1.block.blockColor != color) continue;

            node2 = node1.GetNode(dir);
            if (node2 == null || node2.block == null || node2.block.blockColor != color) continue;

            // 두개 블럭이 동일하면 매치 가능
            isMatch = true;
            matchNodePointList.Add(node1.point);
            matchNodePointList.Add(node2.point);

            // 4개까지 연결되었는지 체크
            var node3 = node1.GetNode(dir);
            if (node3 != null && node3.block != null && node3.block.blockColor == color)
                matchNodePointList.Add(node3.point);
        }



        //2 양쪽의 반대 반향으로 동일 블럭이 있는지 체크
        CheckReverseSideNode(Direction.UP, Direction.DOWN);
        CheckReverseSideNode(Direction.UPLEFT, Direction.DOWNRIGHT);
        CheckReverseSideNode(Direction.UPRIGHT, Direction.DOWNLEFT);
        if (isMatch)
            matchNodePointList.Add(nodePoint);


        // 전방향 체크했는데 없으면 해당 블럭은 매치 불가능
        if (isMatch)
        {
            //@@@ 디버그용
            var s1 = string.Join(", ", matchNodePointList.Select(p => p.ToString()).ToArray());
            HLLogger.Log($"MatchCheck {nodePoint} : {isMatch} - {s1}");
        }
        return isMatch;


        void CheckReverseSideNode(Direction dir1, Direction dir2)
        {
            node1 = nodeMap[nodePoint].GetNode(dir1);
            node2 = nodeMap[nodePoint].GetNode(dir2);
            if (node1 != null && node1.block != null && node1.block.blockColor == color && node2 != null && node2.block != null && node2.block.blockColor == color)
            {
                isMatch = true;
                matchNodePointList.Add(node1.point);
                matchNodePointList.Add(node2.point);

                // 4개까지 연결되었는지 체크
                var node3 = node1.GetNode(dir1);
                if (node3 != null && node3.block != null && node3.block.blockColor == color)
                    matchNodePointList.Add(node3.point);
                node3 = node1.GetNode(dir2);
                if (node3 != null && node3.block != null && node3.block.blockColor == color)
                    matchNodePointList.Add(node3.point);
            }
        }
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

        isMatchingAnimationAct = true;
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

        isMatchingAnimationAct = false;
        MatchCheckAllBlock();
    }


    #endregion



    #region SwapLogic

    Vector2Int moveCheckStartPoint = new Vector2Int(0, 0);
    public void SwapBlock(Block blockA, Block blockB)
    {
        var tempNodePoint = blockA.node.point;

        blockA.ResetNode(blockB.node.point);
        blockB.ResetNode(tempNodePoint);

        // 블럭 스왑 후 매치 체크
        List<Vector2Int> checkList = new List<Vector2Int>() { blockA.nodePoint, blockB.nodePoint };

        isMatchingAnimationAct = true;
        blockA.DoMoveAnimation(blockA.node.point);
        blockB.DoMoveAnimation(blockB.node.point, () =>
        {
            if (MatchCheck(checkList))
            {
                RemoveMatchBlocks();
            }
            else
            {
                // 매치가 없으면 블럭을 원래 위치로 되돌림
                blockB.ResetNode(blockA.node.point);
                blockA.ResetNode(tempNodePoint);

                blockA.DoMoveAnimation(blockA.node.point);
                blockB.DoMoveAnimation(blockB.node.point);
                isMatchingAnimationAct = false;
            }
        });
    }



    #endregion

}
