using Quantum;
using Unity.Cinemachine;
using UnityEngine;

public class CheckCameraEnable : MonoBehaviour
{
    private CinemachineCamera _camera;

    private void Awake()
    {
        _camera = GetComponent<CinemachineCamera>();    
    }
    
    void Start()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        
        if(frame.RuntimeConfig.freeMove)
            _camera.enabled = false;
    }
    
}
