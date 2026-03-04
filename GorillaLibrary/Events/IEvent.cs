namespace GorillaLibrary.Events
{
    public interface IEvent { }
    public abstract class CancellableEvent : IEvent
    {
        public bool Cancelled { get; set; }
    }
}
