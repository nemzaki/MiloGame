using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public new string name;
    public float poolTime;

    private float _timer;

    private void OnEnable()
    {
        _timer = poolTime;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            LocalPoolManager.Instance.ReturnObjectToPool(name, gameObject);
    }

    public void ReturnToPool()
    {
        LocalPoolManager.Instance.ReturnObjectToPool(name, gameObject);
    }
}
