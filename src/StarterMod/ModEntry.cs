namespace StarterMod
{
    public sealed class ModEntry : IMod
    {
        internal static ModLog? Log;
        internal static bool Active { get; private set; }
        private static bool hooksRegistered;
        internal static bool BigFirst => Settings.Current.BigFirst;
        internal static bool FromEnd => Settings.Current.FromEnd;

        public void Init(ModManifest manifest)
        {
            Log = new ModLog(manifest);
            _ = GameAccess.Selection; // Validate version-sensitive private members before enabling input patches.
            Settings.Init(manifest);
            Log.Log("Better Hotkeys initialized. Settings loaded; no game save accessed during initialization.");
        }

        public void OnEnable()
        {
            Settings.ApplyPending();
            Active = true;
            EnsureHooks();
            Log?.Log("Better Hotkeys enabled. Input mode: " + (Settings.Current.Takeover ? "TakeOver" : "VanillaWithHelpers"));
            CompatibilityDiagnostics.Report();
        }

        public void OnDisable()
        {
            Active = false;
            OrganizerInput.Reset();
            if (hooksRegistered)
            {
                ModHook.OnGameLoadedNormal -= OrganizerInput.Reset;
                ModHook.OnLeavingStoreEarly -= OrganizerInput.Reset;
                ModHook.OnReturningStoreLate -= OrganizerInput.Reset;
                hooksRegistered = false;
            }
            Log?.Log("Better Hotkeys disabled.");
        }

        internal static void EnsureHooks()
        {
            if (hooksRegistered) return;
            ModHook.OnGameLoadedNormal += OrganizerInput.Reset;
            ModHook.OnLeavingStoreEarly += OrganizerInput.Reset;
            ModHook.OnReturningStoreLate += OrganizerInput.Reset;
            hooksRegistered = true;
        }

        internal static void RememberSort(bool big, bool end)
        {
            Settings.RememberSort(big, end);
        }

        internal static void Fail(System.Exception error)
        {
            Active = false;
            Log?.Error("Better Hotkeys disabled after an input error: " + error);
            OrganizerInput.Reset();
        }
    }
}
