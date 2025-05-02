using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Pooling Settings")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int initialSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // 초기 풀 생성
        for (int i = 0; i < initialSize; i++)
        {
            AddObjectToPool();
        }
    }

    // 새 오브젝트 생성 후 풀에 추가
    private void AddObjectToPool()
    {
        GameObject obj = Instantiate(blockPrefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }



    // 풀에서 오브젝트 가져오기
    public GameObject Spawn()
    {
        if (pool.Count == 0)
        {
            AddObjectToPool();
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    // 오브젝트 풀에 반환
    public void Recycle(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}