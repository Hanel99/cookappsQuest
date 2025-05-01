using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public enum BlockState
    {
        Empty,
    }

    public enum BlockType
    {
        Normal,
        PengE,
        Count,
    }

    private Button button;          // 블록 버튼

    public int Index;               // Index & ID
    public Vector3Int point;
    public BlockState blockState = BlockState.Empty;

    public BlockType blockType = BlockType.Normal;
    public Node node;               // 블럭이 있는 노드

    private bool onMerging = false;


    public void Awake()
    {
        TryGetComponent<Button>(out button);
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }

    public void SetBlockData(int index, int x, int y, int z, BlockState blockState, BlockType type)
    {
        Index = index;
        point = new Vector3Int(x, y, z);
        this.blockState = blockState;
        blockType = type;
        node = Board.Instance.GetNode(point);
    }



    public void ResetBlockData(Block blockData)
    {
        //블럭 데이터 다시 셋팅. 셔플 등에서 사용할 용도

        blockType = blockData.blockType;
        point = blockData.point;
    }

    public void SetBlockPosition(Transform t, int z)
    {
        transform.position = t.position;
        if (z == 1 || z == 3)
            transform.position += Board.Instance.blockDistanceVector;
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
