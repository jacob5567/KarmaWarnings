namespace KarmaWarnings
{
    // Looks for one kind of Karma risk. Read-only: never call a method that changes the run (see KarmaGate).
    internal interface IWarningDetector
    {
        bool TryGetWarning(WarningContext context, out KarmaWarning warning);
    }
}
