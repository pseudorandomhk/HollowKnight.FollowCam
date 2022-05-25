using System.Collections.Generic;
using System.Reflection;
using InControl;

namespace FollowCam
{
    public class CameraControls : PlayerActionSet
    {
        private static CameraControls _instance = new();
        public static CameraControls instance => _instance;

        public readonly PlayerAction toggleEnabled,
            toggleCamBlanked,
            fCamChangeHitboxView,
            mCamChangeHitboxView,
            zoomIn,
            zoomOut;

        public CameraControls()
        {
            toggleEnabled = base.CreatePlayerAction("ToggleEnabled");
            toggleCamBlanked = base.CreatePlayerAction("ToggleCamBlanked");
            fCamChangeHitboxView = base.CreatePlayerAction("ChangeFollowHitboxView");
            mCamChangeHitboxView = base.CreatePlayerAction("ChangeMainHitboxView");
            zoomIn = base.CreatePlayerAction("ZoomIn");
            zoomOut = base.CreatePlayerAction("ZoomOut");
        }

        public void EnsureRegistered()
        {
            var sets = (List<PlayerActionSet>)typeof(InputManager)
                .GetField("playerActionSets", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null);
            if (!sets.Contains(this))
                sets.Add(this);
        }
    }
}