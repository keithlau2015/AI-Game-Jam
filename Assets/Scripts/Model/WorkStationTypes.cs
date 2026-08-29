namespace Platformer.Model
{
    public enum WorkStationMode
    {
        PermanentProduction,
        TimedTask
    }

    public enum TaskAreaPhase
    {
        WaitingToSpawn,
        MissedSpawn,
        Active,
        Completed,
        Failed
    }
}
