internal sealed record RunResult(
    string Command,
    int ExitCode,
    string Stdout,
    string Stderr,
    bool TimedOut)
{
    public bool Success => !TimedOut && ExitCode == 0;
}

internal sealed record Failure(
    string Operation,
    string Reason,
    DiffInput Input,
    RunResult Ours,
    RunResult TeamB);
