using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Text tempText; //@@@ 삭제 필요. 디버그용 텍스트


    public Button button;          // 블록 버튼
    public Image blockImage;      // 블록 이미지

    public Vector2Int nodePoint; //해당 블럭이 있는 노드의 인덱스
    public BlockType blockType = BlockType.Normal;
    public Node node;               // 블럭이 있는 노드. 참고용으로만 사용하는걸 권장


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


        tempText.text = $"{nodePoint.x}, {nodePoint.y}";
    }

    public void ResetNode(Vector2Int nodePoint)
    {
        this.nodePoint = nodePoint;
        node = Board.instance.GetNode(nodePoint);
        node.block = this;

        tempText.text = $"{nodePoint.x}, {nodePoint.y}";
    }


    public void OnClickBlock()
    {
        // 예외 조건
        // if (GameManager.gameState != GameManager.GameState.Play) return;                    // 게임 플레이중 체크...
        // if (GameManager.directionState == GameManager.DirectionState.Skill_Shuffle) return; // 셔플 연출중 체크...
        // if (blockState != BlockState.Active && !forceTouch) return;                         // 활성 블록 && 강제터치 체크...





        // if (onMoving) return;

        // //@@@ 삭제 테스트
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






    #region Drag Logic

    private bool isDragging = false; // 드래그 중인지 여부
    private Vector2 startPosition;
    private Vector2 dragPosition;
    private Vector2 dragVector;
    private float dragDistance = 80f; // 최소 거리 

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        startPosition = eventData.pressPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging == false) return;

        dragPosition = eventData.position;
        dragVector = dragPosition - startPosition;

        if (dragVector.magnitude >= dragDistance)
        {
            Vector2 direction = dragVector.normalized;

            CheckAndSwapBlock(this, GetDragDirection(direction));
            isDragging = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HLLogger.Log($"OnEndDrag {gameObject.name}");
        isDragging = false;
    }

    private Direction GetDragDirection(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle >= 70 && angle < 110)
            return Direction.UP;
        else if (angle >= 0 && angle < 70)
            return Direction.UPRIGHT;
        else if (angle >= 110 && angle < 180)
            return Direction.UPLEFT;
        else if (angle >= -110 && angle < -70)
            return Direction.DOWN;
        else if (angle >= -180 && angle < -110)
            return Direction.DOWNLEFT;
        else if (angle >= -110 && angle < 0)
            return Direction.DOWNRIGHT;
        else
            return Direction.ERROR; // 알수 없는 방향
    }

    public void CheckAndSwapBlock(Block clickBlock, Direction dir)
    {
        if (onMoving) return;
        if (dir == Direction.ERROR) return;

        var targetNode = clickBlock.node.GetNode(dir);
        if (targetNode == null || targetNode.block == null)
            return;

        // 클릭블럭과 드래그 방향의 노드에 있는 블럭을 스왑
        Board.instance.SwapBlock(this, targetNode.block);
    }

    #endregion









    #region Move Logic

    public void CheckMoveable()
    {
        // 아래, 좌하 우하단에 이동가능한지 체크 및 이동 로직
        var nodePoint = node.FindMoveNodePoint();
        if (nodePoint != null)
        {
            MoveBlockToEmptyNode(nodePoint.Value);
            DoMoveAnimation(nodePoint.Value, () => CheckMoveable());
        }
    }

    public void DoMoveAnimation(Vector2Int targetNodePoint, Action callback = null)
    {
        if (onMoving) return;
        if (cor != null) StopCoroutine(cor);
        cor = StartCoroutine(Co_MoveBlockAnimation(targetNodePoint, callback));
    }

    private IEnumerator Co_MoveBlockAnimation(Vector2Int targetNodePoint, Action callback = null)
    {
        onMoving = true;

        Vector2 targetPos = Board.instance.GetBlockPosition(targetNodePoint);
        transform.DOLocalMove(targetPos, moveDuration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(moveDuration);
        onMoving = false;

        callback?.Invoke();
    }

    public void MoveBlockToEmptyNode(Vector2Int targetNodePoint)
    {
        var targetNode = Board.instance.GetNode(targetNodePoint);

        node.block = null;
        node = targetNode;
        targetNode.block = this;
        nodePoint = targetNode.point;

        tempText.text = $"{nodePoint.x}, {nodePoint.y}";
    }

    #endregion


    #region Match Logic

    public void CheckMatchable()
    {

    }

    #endregion









}
