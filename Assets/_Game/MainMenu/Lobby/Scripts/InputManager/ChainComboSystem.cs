using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainComboSystem : MonoBehaviour
{
    public enum AttackInput
    {
        Punch,
        Kick
    }

    private QuantumLocalInput _localInput;

    private readonly ComboDefinition[] _combos =
    {
        new ComboDefinition {
            name = "SweepKick",
            sequence = new[] {
                AttackInput.Punch,
                AttackInput.Punch,
                AttackInput.Kick
            },
            OnExecute = DoSweepKick
        },
    };

    public ComboInputBuffer comboInputBuffer;
    
    private void Start()
    {
        _localInput = GetComponent<QuantumLocalInput>();
        comboInputBuffer = new ComboInputBuffer();
    }

    public class ComboInputBuffer
    {
        private readonly List<(AttackInput input, float time)> buffer = new();
        private readonly float _maxDelay = 0.5f; // max time between presses
        
        public void AddInput(AttackInput input)
        {
            buffer.Add((input, Time.time));
            CleanUp();
        }

        private void CleanUp()
        {
            var now = Time.time;
            buffer.RemoveAll(entry => now - entry.time > _maxDelay);
        }

        public List<AttackInput> GetSequence()
        {
            CleanUp();
            List<AttackInput> result = new();
        
            foreach (var entry in buffer) 
                result.Add(entry.input);
            return result;
        }

        public void Clear()
        {
            buffer.Clear();
        }
    }
    
    private static void DoSweepKick()
    {
        Debug.Log("Sweep Kick");
        QuantumLocalInput.Instance.QueueSweepKick();
    }

    public bool TryExecuteCombo(List<AttackInput> bufferSequence)
    {
        foreach (var combo in _combos)
        {
            if(bufferSequence.Count < combo.sequence.Length)
                continue;

            var start = bufferSequence.Count - combo.sequence.Length;

            for (var i = 0; i < combo.sequence.Length; i++)
            {
                if (bufferSequence[start + i] != combo.sequence[i])
                    goto NextCombo;
            }
            
            combo.OnExecute?.Invoke();
           comboInputBuffer.Clear();
            return true;
            
            NextCombo: ;
        }
        return false;
    }
    
}

[Serializable]
public class ComboDefinition
{
    public string name;
    public ChainComboSystem.AttackInput[] sequence;
    public Action OnExecute;
}

