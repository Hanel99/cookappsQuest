using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public static Board Instance;

    // public int blockListCount => blockList.Count;                                     // 블럭 리스크에 남아있는 블럭 개수
    // public List<Block> blockList = new List<Block>();                                 // 실제 데이터가 있는 블럭 리스트
    // public List<Transform> blockPositionList = new List<Transform>();                 // 블럭 포지션 저장할 리스트
    // public List<Transform> selectBlockPositionList = new List<Transform>();           // 선택 블럭 포지션 저장할 리스트
    public Dictionary<int, Node> nodeMap = new Dictionary<int, Node>(); // 노드 찾아오기용 노드맵
    // private List<Node> nodeList = new List<Node>();

    // public List<Block> selectBlockList = new List<Block>();                 //선택 블록 리스트
    // private List<int> blockTypeList = new List<int>();                              //블럭에 입힐 아이콘 타입 리스트

    // public int matchingQueueCount => matchingQueue.Count;                   //매칭 큐 개수. 타임매니저에서 게임오버 시키기 전 확인할 용도
    // private Queue<List<Block>> matchingQueue = new Queue<List<Block>>();    // 매칭 목록 큐
    // private bool isMatchingDirection = false;                               // 매칭 연출중...

    // private readonly float shuffleTime = 0.75f;         // 블록 섞는 시간
    // private readonly float blockMoveTime = 0.2f;        // 블록 이동 & 블록 재정렬 연출 시간
    // private readonly float mergeTime = 0.3f;            // 블록 머지 연출 시간

    // private float blockMergeSpeed = 1f;
    // private float mergeDuration => mergeTime * blockMergeSpeed;
    // private float moveDuration => blockMoveTime * blockMergeSpeed;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {

    }


    #region Node

    public Node GetNode(int? point)
    {
        Node node = null;
        if (point != null)
            node = nodeMap[point.Value];

        return node;
    }

    #endregion Node

}
