using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Node
{
    public Block block;     // 해당 노드에 있는 블럭

    public Vector2Int point;                    // NodeMap 좌표 (Key)
    public Vector2Int?[] linkedNode = null;     // 연결 된 노드
    public bool isOn = true; // 사용하지 않는 노드(투명 노드)

    private List<Vector2Int> nextNodePointList = new(); //다음 노드 찾기에 사용되는 임시 리스트

    public Node(Vector2Int point, Vector2Int?[] foundedLinkedNode)
    {
        this.point = point;
        linkedNode = foundedLinkedNode;
    }


    public Vector2Int? FindMoveNodePoint()
    {
        // 바로 아래 노드가 비어있으면 아래로 우선 이동
        var node = FindNode(Direction.DOWN);
        if (node != null && node.block == null)
        {
            return node.point;
        }

        // 아래 양 옆 노드가 비어있으면 빈 곳으로 이동.
        nextNodePointList.Clear();
        var DLNode = FindNode(Direction.DOWNLEFT);
        if (DLNode != null && DLNode.block == null)
            nextNodePointList.Add(DLNode.point);

        var DRNode = FindNode(Direction.DOWNRIGHT);
        if (DRNode != null && DRNode.block == null)
            nextNodePointList.Add(DRNode.point);


        if (nextNodePointList.Count == 0)
        {
            // 빈 노드가 없는 경우
            return null;
        }
        else if (nextNodePointList.Count == 1)
        {
            // 한곳만 비어있는 경우
            return nextNodePointList[0];
        }
        else
        {
            //둘 다 비어있다면 두 곳 중 랜덤으로 이동
            int randomIndex = Random.Range(0, nextNodePointList.Count);
            return nextNodePointList[randomIndex];
        }
    }


    public Node FindNode(Direction dir)
    {
        Vector2Int? targetNodePoint = linkedNode[(int)dir];
        if (targetNodePoint.HasValue)
        {
            return Board.instance.nodeMap[targetNodePoint.Value];
        }

        return null;
    }
}
