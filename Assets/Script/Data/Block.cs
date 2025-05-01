using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public enum BlockType
    {
        Normal,
        PengE,
        Count,
    }

    private Button button;          // 블록 버튼
    public Image blockImage;      // 블록 이미지

    public int index;               // Index & ID
    public int nodePoint; //해당 블럭이 있는 노드의 인덱스
    public BlockType blockType = BlockType.Normal;
    public Node node;               // 블럭이 있는 노드

    private bool onMoving = false;


    public void Awake()
    {
        TryGetComponent<Button>(out button);
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }

    public void SetBlockData(int index, int nodePoint, BlockType type, BlockColor color)
    {
        this.index = index;
        this.nodePoint = nodePoint;
        blockType = type;
        node = Board.Instance.GetNode(nodePoint);
        blockImage.sprite = ResourceManager.instance.GetBlockImage(color);
    }



    public void ResetBlockData(Block blockData)
    {
        blockType = blockData.blockType;
        nodePoint = blockData.nodePoint;
    }

    public void SetBlockPosition(Transform t, int z)
    {
        transform.position = t.position;
        // if (z == 1 || z == 3)
        // transform.position += Board.Instance.blockDistanceVector;
    }

    public void OnClickBlock(bool forceTouch = false)
    {
        // 예외 조건
        // if (GameManager.gameState != GameManager.GameState.Play) return;                    // 게임 플레이중 체크...
        // if (GameManager.directionState == GameManager.DirectionState.Skill_Shuffle) return; // 셔플 연출중 체크...
        // if (blockState != BlockState.Active && !forceTouch) return;                         // 활성 블록 && 강제터치 체크...

        CheckBlockLink();
    }


    public void CheckBlockLink()
    {

    }


}
