public interface IAbility
{
    public void ActivateAbility();
    public bool IsOnCooldown { get; }
}
