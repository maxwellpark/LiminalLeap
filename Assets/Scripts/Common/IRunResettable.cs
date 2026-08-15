// Run state that has to be put back on death. Missing one is invisible until you replay.
public interface IRunResettable
{
    void ResetForNewRun();
}
