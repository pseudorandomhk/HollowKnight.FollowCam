using Modding;
using Modding.Menu;
using Modding.Menu.Config;

namespace FollowCam
{
    public static class MenuHelper
    {
        private static MenuScreen root;

        public static MenuScreen GetMenuScreen(MenuScreen modList, ModToggleDelegates? toggleDelegates)
        {
            MenuBuilder builder = MenuUtils.CreateMenuBuilderWithBackButton("FollowCam", modList, out _);
            builder.CreateTitle("FollowCam Keybinds", MenuTitleStyle.vanillaStyle);

            builder.AddContent(RegularGridLayout.CreateVerticalLayout(105f),
                c =>
                {
                    c.AddKeybind("toggle_enable_bind", CameraControls.instance.toggleEnabled,
                        new KeybindConfig
                        {
                            Label = "Toggle visibility",
                            Style = KeybindStyle.VanillaStyle
                        });
                    c.AddKeybind("show_trans_blanker", CameraControls.instance.toggleCamBlanked,
                        new KeybindConfig
                        {
                            Label = "Show in transitions",
                            Style = KeybindStyle.VanillaStyle
                        });
                    c.AddKeybind("fcam_hitbox_state", CameraControls.instance.fCamChangeHitboxView,
                        new KeybindConfig
                        {
                            Label = "Cycle hitboxes in view",
                            Style = KeybindStyle.VanillaStyle
                        });
                    c.AddKeybind("mcam_hitbox_state", CameraControls.instance.mCamChangeHitboxView,
                        new KeybindConfig
                        {
                            Label = "Cycle hitboxes in game",
                            Style = KeybindStyle.VanillaStyle
                        });
                    c.AddKeybind("zoom_in", CameraControls.instance.zoomIn,
                        new KeybindConfig
                        {
                            Label = "Zoom in",
                            Style = KeybindStyle.VanillaStyle
                        });
                    c.AddKeybind("zoom_out", CameraControls.instance.zoomOut,
                        new KeybindConfig
                        {
                            Label = "Zoom out",
                            Style = KeybindStyle.VanillaStyle
                        });
                });

            root = builder.Build();
            return root;
        }
    }
}