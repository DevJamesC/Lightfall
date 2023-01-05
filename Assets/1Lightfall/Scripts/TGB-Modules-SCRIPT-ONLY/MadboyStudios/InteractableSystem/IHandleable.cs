namespace MBS.InteractionSystem
{
    public interface IHandleable : IInteractable
    {
        void Pickup();
        void Putdown();
    }
}
