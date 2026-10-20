using System.Runtime.CompilerServices;

// The dev-only KarmaWarnings.Tests plugin reads the controller's warning. It is never shipped.
[assembly: InternalsVisibleTo("KarmaWarnings.Tests")]
