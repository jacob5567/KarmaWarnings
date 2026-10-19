<!-- cope:profile -->
## Build and test notes (maintained by COPE; edit freely)
- Check: `dotnet build -c Release -p:CopyToProfile=false`
- Test signal: compile-only: a pass proves the code compiles against the local RoR2, Unity and RiskOfOptions DLLs; no tests exist, behavior is only checked in-game.
- Oracle: not-possible: the code only runs inside Risk of Rain 2 (BepInEx plugin) and there is no test project or harness.
- Hazards: the default build copies the DLL and pdb into the r2modman profile (`CopyToProfile` target); `-p:CopyToProfile=false` disables it. `./build.sh` also builds and writes dist/ (gitignored) but is for packaging; CI only publishes a prebuilt zip to Thunderstore, so never run that workflow.
- Worktree needs: none (references the game install and r2modman profile by absolute path under `$HOME`; an optional gitignored `Directory.Build.props.user` can override them but is not present here)
- Baseline: pass (2026-10-19, 479955b)
<!-- /cope:profile -->
