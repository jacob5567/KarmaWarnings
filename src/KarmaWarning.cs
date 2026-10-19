namespace KarmaWarnings
{
    // One warning to show: a line saying what the action is, under an "Increases Karma" headline. Amount isn't shown; it
    // is what decides whether the warning applies at all (it must be above 0).
    internal struct KarmaWarning
    {
        public int Amount;
        // For a warning that tells the player how to avoid the Karma: only the text is shown, without the headline.
        public bool HideHeadline;
        public string Text;
        // Higher wins when several warnings apply at once.
        public int Priority;
    }
}
