
public interface IEffectExecutor
{
    bool CanExecute(EffectBinding binding);
    bool TryExecute(EffectExecutionContext context, EffectBinding binding);
}