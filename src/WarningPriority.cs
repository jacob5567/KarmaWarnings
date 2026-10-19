namespace KarmaWarnings
{
    // Which warning wins when several apply at once: the one that is hardest to undo.
    internal static class WarningPriority
    {
        public const int Stage = 100;
        public const int Interaction = 90;
        public const int Combat = 80;
        public const int Item = 60;
        public const int Loop = 20;
    }
}
