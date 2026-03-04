namespace GorillaLibrary.Events.System
{
    public interface IEvent { }
    public abstract class CancellableEvent : IEvent
    {
        public bool Cancelled { get; set; }
    }
}
