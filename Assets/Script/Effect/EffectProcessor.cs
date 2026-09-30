using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectProcessor : MonoBehaviour
{
    public static EffectProcessor Instance { get; private set; }

    private List<IEffectExecutor> executors = new();
    private Queue<(EffectExecutionContext context, EffectBinding binding)> effectQueue = new();
    private bool isProcessing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RegisterExecutor(new DamageEffectExecutor());
        // ���� �߰��� Executor�� ���⼭ ���
    }

    public void EnqueueEffects(EffectExecutionContext context, IEnumerable<EffectBinding> bindings)
    {
        foreach (var binding in bindings)
        {
            if (binding != null && binding.EffectDefinition != null)
            {
                effectQueue.Enqueue((context, binding));
            }
        }

        if (!isProcessing)
        {
            StartCoroutine(ProcessQueueRoutine());
        }
    }          

    private IEnumerator ProcessQueueRoutine()
    {
        isProcessing = true;

        while (effectQueue.Count > 0)
        {
            var (context, binding) = effectQueue.Dequeue();

            ExecuteSingleEffect(context, binding);

            yield return new WaitForEndOfFrame();
        }

        isProcessing = false;
    }
    private void ExecuteSingleEffect(EffectExecutionContext context, EffectBinding binding)
    {
        if (!EffectEvaluator.CanExecute(context, binding))
        {
            return;
        }

        foreach (var executor in executors)
        {
            if (executor.CanExecute(binding))
            {
                if (context.SourceUnit != null && context.SourceUnit.TriggerListener != null)
                {
                    context.SourceUnit.TriggerListener.IncrementTriggerCount(binding);
                }

                executor.TryExecute(context, binding);
                break;
            }
        }
    }

    private void RegisterExecutor(IEffectExecutor executor)
    {
        if (!executors.Contains(executor))
        {
            executors.Add(executor);
        }
    }
}