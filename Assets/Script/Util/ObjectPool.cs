using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance { get; private set; }


    [Header("Pooling Settings")]
    // 풀링할게 블럭밖에 없으니 간단하게...
    [SerializeField] private Block blockPrefab;
    [SerializeField] private int initialSize = 40;

    private Queue<Block> blockPool = new Queue<Block>();

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            MakeBlock();
        }
    }

    // 새 오브젝트 생성 후 풀에 추가
    private void MakeBlock()
    {
        Block obj = Instantiate(blockPrefab, transform);
        obj.gameObject.SetActive(false);
        blockPool.Enqueue(obj);
    }


    #region Spawn & Recycle

    public Block Spawn()
    {
        if (blockPool.Count == 0)
        {
            MakeBlock();
        }

        Block block = blockPool.Dequeue();
        block.gameObject.SetActive(true);
        return block;
    }

    public void Recycle(Block block)
    {
        block.gameObject.SetActive(false);
        block.transform.SetParent(transform);
        blockPool.Enqueue(block);
    }

    #endregion


}