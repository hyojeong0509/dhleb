using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오브젝트 풀 - Instantiate/Destroy 대신 재사용으로 GC 부담 감소
/// </summary>
public class ObjectPool<T> where T : Component
{
    readonly T _prefab;
    readonly Transform _parent;
    readonly Queue<T> _pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize = 0, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateNew());
    }

    T CreateNew()
    {
        var go = Object.Instantiate(_prefab.gameObject, _parent);
        go.SetActive(false);
        return go.GetComponent<T>();
    }

    /// <summary>풀에서 오브젝트 가져오기. 없으면 새로 생성</summary>
    public T Get()
    {
        T obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = CreateNew();
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    /// <summary>풀에 오브젝트 반환</summary>
    public void Return(T obj)
    {
        if (obj == null) return;
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    public int Count => _pool.Count;
}
