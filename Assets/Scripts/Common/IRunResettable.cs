// Anything that changes state during a run and has to be put back on death.
// Junctions arm/disarm, pickups hide/show. Missing one of these is invisible until you replay.
public interface IRunResettable
{
    void ResetForNewRun();
}
