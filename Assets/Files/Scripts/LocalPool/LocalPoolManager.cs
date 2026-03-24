using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPoolManager : MonoBehaviour
{
    public static LocalPoolManager Instance { set; get; }

    [System.Serializable]
    public class ObjectPoolItem
    {
        public string objectName;
        public GameObject objectPrefab;
        public int poolSize;
        public Transform manager;
    }


    public List<ObjectPoolItem> objectsToPool;
    private Dictionary<string, Queue<GameObject>> objectPoolDictionary;

    private void Start()
    {
        objectPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Initialize the object pool for each object in the list
        foreach (var poolItem in objectsToPool)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolItem.poolSize; i++)
            {
                GameObject obj = Instantiate(poolItem.objectPrefab,
                    poolItem.manager);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            objectPoolDictionary.Add(poolItem.objectName, objectPool);
        }

        Instance = this;
    }

    public GameObject GetObjectFromPool(string objectName)
    {
        if (objectPoolDictionary.ContainsKey(objectName))
        {
            Queue<GameObject> objectPool = objectPoolDictionary[objectName];

            if (objectPool.Count > 0)
            {
                GameObject obj = objectPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Find the ObjectPoolItem for the specified objectName
                ObjectPoolItem poolItem = objectsToPool.Find(item => item.objectName == objectName);
                if (poolItem != null)
                {
                    // Instantiate a new object and add it to the pool
                    GameObject obj = Instantiate(poolItem.objectPrefab, poolItem.manager);
                    obj.SetActive(true);
                    return obj;
                }
                else
                {
                    Debug.LogWarning("No ObjectPoolItem found for objectName: " + objectName);
                    return null;
                }
            }
        }

        Debug.LogWarning("Object name not found in the pool: " + objectName);
        return null;
    }

    public void ReturnObjectToPool(string objectName, GameObject obj)
    {
        if (objectPoolDictionary.ContainsKey(objectName))
        {
            ObjectPoolItem poolItem = objectsToPool.Find(item => item.objectName == objectName);
            obj.transform.SetParent(poolItem.manager);
            // Deactivate the object and return it to the pool
            obj.SetActive(false);
            objectPoolDictionary[objectName].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Object name not found in the pool: " + objectName);
        }
    }
}
