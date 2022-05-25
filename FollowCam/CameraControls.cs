using InControl;

namespace FollowCam
{
    public class CameraControls : PlayerActionSet
    {
        private static CameraControls _instance;
        public static CameraControls instance => _instance ??= new();
        
        public readonly PlayerAction toggleEnabled, toggleCamBlanked,
            fCamChangeHitboxView, mCamChangeHitboxView,
            zoomIn, zoomOut;

        public CameraControls()
        {
            toggleEnabled = base.CreatePlayerAction("ToggleEnabled");
            toggleCamBlanked = base.CreatePlayerAction("ToggleCamBlanked");
            fCamChangeHitboxView = base.CreatePlayerAction("ChangeFollowHitboxView");
            mCamChangeHitboxView = base.CreatePlayerAction("ChangeMainHitboxView");
            zoomIn = base.CreatePlayerAction("ZoomIn");
            zoomOut = base.CreatePlayerAction("ZoomOut");
        }
    }
}