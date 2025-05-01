using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    UP = 0,
    UPLEFT = 1,
    UPRIGHT = 2,
    DOWN = 3,
    DOWNLEFT = 4,
    DOWNRIGHT = 5,
}

[System.Serializable]
public class Node
{
    public Block block;     // 해당 노드에 있는 블럭

    public int point;                    // NodeMap 좌표 (Key)
    public int?[] linkedNode = null;     // 연결 된 노드

    public Node(int?[] foundedLinkedNode)
    {
        linkedNode = foundedLinkedNode;
    }


    public Node FindTarget(Direction dir)
    {
        int? pt = linkedNode[(int)dir];
        if (pt.HasValue)
        {
            return Board.Instance.nodeMap[pt.Value];
        }

        return null;
    }
}
