using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    // public Text tempText; //@@@ 삭제 필요. 디버그용 텍스트


    public Image blockImage;      // 블록 이미지

    public Vector2Int nodePoint; //해당 블럭이 있는 노드의 인덱스
    public BlockType blockType = BlockType.Normal;
    public BlockColor blockColor = BlockColor.Blue;
    public Node node;               // 블럭이 있는 노드. 참고용으로만 사용하는걸 권장


    //private
    private SpriteAnimator spriteAnimator = null;
    private bool onAnimation = false;
    private int hp = 0; // 팽이 블럭의 경우 2번 데미지를 받으면 파괴
    private Coroutine moveAni = null;
    private Coroutine removeAni = null;
    private float moveDuration = 0.09f; // 블럭 이동 속도
    private float removeDuration = 0.15f; // 블럭 삭제 연출 속도


    public void Awake()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();
    }

    public void SetBlockData(Vector2Int nodePoint, BlockType type, BlockColor color)
    {
        this.nodePoint = nodePoint;
        blockType = type;
        node = Board.instance.GetNode(nodePoint);
        node.block = this;
        blockColor = color;

        if (blockType == BlockType.Special)
        {
            blockImage.sprite = ResourceManager.instance.GetSpecialBlockImage(0);
            hp = 2;
        }
        else
        {
            blockImage.sprite = ResourceManager.instance.GetBlockImage(color);
            hp = 0;
        }

        transform.localScale = Vector3.one;
        blockImage.color = Color.white;

        // tempText.text = $"{nodePoint.x}, {nodePoint.y}" + ((blockType == BlockType.Special) ? $"\nHP {hp}" : "");
    }

    public void ResetNode(Vector2Int nodePoint)
    {
        this.nodePoint = nodePoint;
        node = Board.instance.GetNode(nodePoint);
        node.block = this;

        // tempText.text = $"{nodePoint.x}, {nodePoint.y}" + ((blockType == BlockType.Special) ? $"\nHP {hp}" : "");
    }

    public void ResetBlockColor()
    {
        //초기 보드 셋팅 중 블럭 색상 변경 용
        blockColor++;
        if (blockColor > BlockColor.Yellow) blockColor = BlockColor.Blue; // 블럭 색상 순환

        blockImage.sprite = ResourceManager.instance.GetBlockImage(blockColor);
    }





    #region Drag Logic

    private bool isDragging = false; // 드래그 중인지 여부
    private Vector2 startPosition;
    private Vector2 dragPosition;
    private Vector2 dragVector;
    private float dragDistance = 80f; // 최소 거리 

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Board.instance.IsMatchingAnimationAct) return;
        if (blockType == BlockType.Special) return;

        isDragging = true;
        startPosition = eventData.pressPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Board.instance.IsMatchingAnimationAct) return;
        if (blockType == BlockType.Special) return;
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
        if (onAnimation) return;
        if (dir == Direction.ERROR) return;

        // 해당방향에 노드가 없거나, 블럭이 없거나, 루트노드의 경우는 이동 불가
        var targetNode = clickBlock.node.GetNode(dir);
        if (targetNode == null || targetNode.block == null || targetNode.point == Board.instance.RootNodePoint)
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
        if (moveAni != null) StopCoroutine(moveAni);
        moveAni = StartCoroutine(Co_MoveBlockAnimation(targetNodePoint, callback));
    }

    private IEnumerator Co_MoveBlockAnimation(Vector2Int targetNodePoint, Action callback = null)
    {
        onAnimation = true;

        Vector2 targetPos = Board.instance.GetBlockPosition(targetNodePoint);
        transform.DOLocalMove(targetPos, moveDuration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(moveDuration);
        onAnimation = false;

        callback?.Invoke();
    }

    public void MoveBlockToEmptyNode(Vector2Int targetNodePoint)
    {
        var targetNode = Board.instance.GetNode(targetNodePoint);

        node.block = null;
        node = targetNode;
        targetNode.block = this;
        nodePoint = targetNode.point;

        // tempText.text = $"{nodePoint.x}, {nodePoint.y}" + ((blockType == BlockType.Special) ? $"\nHP {hp}" : "");
    }

    #endregion


    #region Match & Remove Logic

    public void OnDamage(int damage = 1)
    {
        //특수 블럭에 데미지 부여
        if (blockType == BlockType.Normal) return;

        hp -= damage;
        if (hp <= 0)
        {
            RemoveBlock();
        }
        else
        {
            // 블럭 데미지 1인 상태
            spriteAnimator.PlayAnimation();
            // tempText.text = $"{nodePoint.x}, {nodePoint.y}" + $"\nHP {hp}";
        }
    }

    public void DoRemoveAnimation()
    {
        if (onAnimation) return;

        if (removeAni != null) StopCoroutine(removeAni);
        removeAni = StartCoroutine(Co_RemoveBlockAnimation());
    }

    private IEnumerator Co_RemoveBlockAnimation()
    {
        onAnimation = true;
        transform.DOScale(1.5f, removeDuration).SetEase(Ease.Linear);
        blockImage.DOFade(0, removeDuration).SetEase(Ease.Linear);

        //게임 데이터 갱신
        GameManager.instance.AddScore(blockType);
        if (blockType == BlockType.Special)
        {
            // 특수 블럭 제거 
            GameManager.instance.AddSpecialBlockCount(-1);
        }

        node.block = null;
        Board.instance.AddBlockSpawnCount();

        yield return new WaitForSeconds(removeDuration);
        onAnimation = false;
        ObjectPool.instance.Recycle(this);
    }



    public void RemoveBlock()
    {
        DOTween.Kill(gameObject);
        if (moveAni != null) StopCoroutine(moveAni);

        DoRemoveAnimation();
    }


    #endregion









}
