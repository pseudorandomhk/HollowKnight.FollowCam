using System;
using System.Linq;
using System.Reflection;
using InControl;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace FollowCam
{
    internal class FollowCam : Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings>
    {
        public static readonly int HITBOX_REG_LAYER = 6, HITBOX_MISC_LAYER = 7;

        internal static FollowCam Instance { get; private set; }
        private GameObject followCamParent;

        public FollowCam() : base("FollowCam") { }

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool ToggleButtonInsideMenu => true;

        public override void Initialize()
        {
            Log("Initializing");

            Instance = this;
            followCamParent = new GameObject();
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
            Mouse mouse = Mouse.None;
            if (!Enum.TryParse(bind, out Key key) && !Enum.TryParse<Mouse>(bind, out mouse))
                return false;

            if (mouse != Mouse.None)
                action.AddBinding(new MouseBindingSource(mouse));
            else
                action.AddBinding(new KeyBindingSource(key));

            return true;
        }

        public MenuScreen GetMenuScreen(MenuScreen modlist, ModToggleDelegates? toggleDelegates) =>
            MenuHelper.GetMenuScreen(modlist, toggleDelegates!);

        public void OnLoadGlobal(GlobalSettings gs)
        {
            GlobalSettings.instance = gs ?? GlobalSettings.instance;

            if (GlobalSettings.instance.camProportion is < 0 or > 1)
            {
                LogWarn("Camera proportion setting must be between 0 and 1");
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