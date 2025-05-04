using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{


    public Button button;          // 블록 버튼
    public Image blockImage;      // 블록 이미지

    public Vector2Int nodePoint; //해당 블럭이 있는 노드의 인덱스
    public BlockType blockType = BlockType.Normal;
    public Node node;               // 블럭이 있는 노드


    //private
    private bool onMoving = false;
    private Coroutine cor = null;
    private float moveDuration = 0.2f; // 블럭 이동 속도


    public void Awake()
    {
        TryGetComponent<Button>(out button);
    }

    public void SetBlockData(Vector2Int nodePoint, BlockType type, BlockColor color)
    {
        this.nodePoint = nodePoint;
        blockType = type;
        node = Board.instance.GetNode(nodePoint);
        node.block = this;
        blockImage.sprite = ResourceManager.instance.GetBlockImage(color);
    }


    public void OnClickBlock()
    {
        // 예외 조건
        // if (GameManager.gameState != GameManager.GameState.Play) return;                    // 게임 플레이중 체크...
        // if (GameManager.directionState == GameManager.DirectionState.Skill_Shuffle) return; // 셔플 연출중 체크...
        // if (blockState != BlockState.Active && !forceTouch) return;                         // 활성 블록 && 강제터치 체크...

        if (onMoving) return;


        //@@@ 삭제 테스트
        RemoveBlock();
        node.GetNode(Direction.UPLEFT)?.block?.RemoveBlock();
        node.GetNode(Direction.UPLEFT)?.GetNode(Direction.UPLEFT)?.block?.RemoveBlock();

        Board.instance.CheckMoveableAllBlock(nodePoint);
    }


    public void RemoveBlock()
    {
        DOTween.Kill(gameObject);
        if (cor != null) StopCoroutine(cor);


        node.block = null;

        //@@@ 이건 고민좀 해보자. 연쇄작업이 일어나야 함
        //여기서 할게 아니고 매치가 일어났으면 이동 후 보드에서 전체순회로 확인하는걸로
        // node.FindNode(Direction.UP)?.block?.CheckMoveable();
        // node.FindNode(Direction.UPLEFT)?.block?.CheckMoveable();
        // node.FindNode(Direction.UPRIGHT)?.block?.CheckMoveable();     
        ObjectPool.instance.Recycle(this);
    }


    #region Move Logic

    public void CheckMoveable()
    {
        // 아래, 좌하 우하단에 이동가능한지 체크 및 이동 로직
        var nodePoint = node.FindMoveNodePoint();
        if (nodePoint != null)
            StartCoroutine(Co_MoveBlock(nodePoint.Value));
    }

    private IEnumerator Co_MoveBlock(Vector2Int targetNodePoint)
    {
        onMoving = true;

        MoveBlock(targetNodePoint);

        Vector2 targetPos = Board.instance.GetBlockPosition(targetNodePoint);
        transform.DOLocalMove(targetPos, moveDuration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(moveDuration);
        onMoving = false;
        CheckMoveable();
    }

    public void MoveBlock(Vector2Int targetNodePoint)
    {
        var targetNode = Board.instance.GetNode(targetNodePoint);

        node.block = null;
        node = targetNode;
        targetNode.block = this;
        nodePoint = targetNode.point;
    }

    #endregion


    #region Match Logic

    public void CheckMatchable()
    {

    }

    #endregion









}
