using System;
using System.IO;
using System.Linq;
using System.Reflection;
using InControl;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace FollowCam
{
    internal class FollowCam : Mod
    {
        public static readonly int HITBOX_REG_LAYER = 6, HITBOX_MISC_LAYER = 7;
        private static string globalSettingsSuffix = "FollowCam.GlobalSettings.json";

        internal static FollowCam Instance { get; private set; }
        private GameObject followCamParent;

        public FollowCam() : base("FollowCam") => Initialize();

        public override string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool ToggleButtonInsideMenu => true;

        public void Initialize()
        {
            Log("Initializing");
            Instance = this;

            string globalSettingsPath = Path.Combine(Application.persistentDataPath, globalSettingsSuffix);
            if (!File.Exists(globalSettingsPath))
            {
                Log("Global settings don't exist");
                OnLoadGlobal(null);
            }
            else
            {
                var reader = new StreamReader(File.OpenRead(globalSettingsPath));
                string json = reader.ReadToEnd();

                try
                {
                    OnLoadGlobal(JsonUtility.FromJson<GlobalSettings>(json));
                }
                catch (Exception e)
                {
                    Log("[ERROR] Could not load global settings: " + e);
                }
            }

            followCamParent = new GameObject("FC::FollowCamParent");
            UObject.DontDestroyOnLoad(followCamParent);

            // Instead of ModHooks.FinishedModLoading, for compatability with 1.5.75
            On.HeroController.Start += AttachCam;

            Log("Initialized");
        }

        private void AttachCam(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            On.HeroController.Start -= AttachCam;
            AttachCam();
        }

        private void AttachCam()
        {
            followCamParent.AddComponent<FollowCamController>();
        }

        public static bool TryAddKeybind(PlayerAction action, string bind)
        {
            bool key = Enum.IsDefined(typeof(Key), bind);
            if (!key && !Enum.IsDefined(typeof(Mouse), bind))
                return false;

            if (!key)
                action.AddBinding(new MouseBindingSource((Mouse) Enum.Parse(typeof(Mouse), bind)));
            else
                action.AddBinding(new KeyBindingSource((Key) Enum.Parse(typeof(Key), bind)));

            return true;
        }

        public void OnLoadGlobal(GlobalSettings gs)
        {
            GlobalSettings.instance = gs ?? GlobalSettings.instance;

            if (GlobalSettings.instance.camProportion is < 0 or > 1)
            {
                Log("Camera proportion setting must be between 0 and 1");
                GlobalSettings.instance.camProportion = 0.25f;
            }

            if (!TryAddKeybind(CameraControls.instance.toggleEnabled, GlobalSettings.instance.toggleEnabled))
                CameraControls.instance.toggleEnabled.AddBinding(new KeyBindingSource(Key.E));
            if (!TryAddKeybind(CameraControls.instance.fCamChangeHitboxView, GlobalSettings.instance.followCamChangeHitboxState))
                CameraControls.instance.fCamChangeHitboxView.AddBinding(new KeyBindingSource(Key.F));
            if (!TryAddKeybind(CameraControls.instance.mCamChangeHitboxView, GlobalSettings.instance.mainCamChangeHitboxState))
                CameraControls.instance.mCamChangeHitboxView.AddBinding(new KeyBindingSource(Key.M));
            if (!TryAddKeybind(CameraControls.instance.toggleCamBlanked, GlobalSettings.instance.toggleBlankerShown))
                CameraControls.instance.toggleCamBlanked.AddBinding(new KeyBindingSource(Key.B));
            if (!TryAddKeybind(CameraControls.instance.zoomIn, GlobalSettings.instance.zoomIn))
                CameraControls.instance.zoomIn.AddBinding(new KeyBindingSource(Key.Equals));
            if (!TryAddKeybind(CameraControls.instance.zoomOut, GlobalSettings.instance.zoomOut))
                CameraControls.instance.zoomOut.AddBinding(new KeyBindingSource(Key.Minus));
        }

        public GlobalSettings OnSaveGlobal()
        {
            GlobalSettings.instance.toggleEnabled =
                CameraControls.instance.toggleEnabled.Bindings.FirstOrDefault()?.Name;
            GlobalSettings.instance.toggleBlankerShown =
                CameraControls.instance.toggleCamBlanked.Bindings.FirstOrDefault()?.Name;
            GlobalSettings.instance.mainCamChangeHitboxState =
                CameraControls.instance.mCamChangeHitboxView.Bindings.FirstOrDefault()?.Name;
            GlobalSettings.instance.followCamChangeHitboxState =
                CameraControls.instance.fCamChangeHitboxView.Bindings.FirstOrDefault()?.Name;
            GlobalSettings.instance.zoomIn =
                CameraControls.instance.zoomIn.Bindings.FirstOrDefault()?.Name;
            GlobalSettings.instance.zoomOut =
                CameraControls.instance.zoomOut.Bindings.FirstOrDefault()?.Name;

            return GlobalSettings.instance;
        }
    }
}