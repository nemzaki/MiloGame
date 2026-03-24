using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectsInPool : MonoBehaviour
{
    public static ObjectsInPool Instance { set; get; }

    [Header("Impacts")]
    public string bulletHitDefault = "BulletHit";
    public string hitLight = "HitLight";
    public string hitMedium = "HitMedium";
    public string hitHeavy = "HitHeavy";
    
    private void Awake()
    {
        Instance = this;
    }


}
