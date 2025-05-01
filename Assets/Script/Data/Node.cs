using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    Upper_UPLEFT = 0,
    Upper_UPRIGHT = 1,
    Upper_DOWNLEFT = 2,
    Upper_DOWNRIGHT = 3,
    Under_UPLEFT = 4,
    Under_UPRIGHT = 5,
    Under_DOWNLEFT = 6,
    Under_DOWNRIGHT = 7,
}

[System.Serializable]
public class Node
{
    public Block block;     // 해당 노드에 있는 블럭

    public Vector3Int point;                    // NodeMap 좌표 (Key)
    public Vector3Int?[] linkedNode = null;     // 연결 된 노드

    public Node(Vector3Int?[] foundedLinkedNode)
    {
        linkedNode = foundedLinkedNode;
    }


    public Node FindTarget(Direction dir)
    {
        Vector3Int? pt = linkedNode[(int)dir];
        if (pt.HasValue)
        {
            return Board.Instance.nodeMap[pt.Value];
        }

        return null;
    }
}
