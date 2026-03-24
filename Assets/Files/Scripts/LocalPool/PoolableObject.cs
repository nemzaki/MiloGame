using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public new string name;
    public float poolTime;

    private void OnEnable()
    {
        StartCoroutine(Return());
        //Debug.Log(transform.position);
    }

    IEnumerator Return()
    {
        yield return new WaitForSeconds(poolTime);
        LocalPoolManager.Instance.ReturnObjectToPool(name, gameObject);
    }

    public void ReturnToPool()
    {
        LocalPoolManager.Instance.ReturnObjectToPool(name, gameObject);
    }
}
