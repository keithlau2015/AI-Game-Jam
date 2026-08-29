namespace Platformer.Model
{
    public enum WorkerRole
    {
        Any = 0,
        Builder = 1,
        Analyst = 2,
        Courier = 3
    }

    public enum WorkerState
    {
        InRoster,
        Dragging,
        Working
    }

    public enum RoundPhase
    {
        Idle,
        Playing,
        PausedForEvent,
        Won,
        Lost
    }
}
