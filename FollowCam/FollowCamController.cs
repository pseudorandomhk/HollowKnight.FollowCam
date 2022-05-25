using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FollowCam
{
    public class FollowCamController : MonoBehaviour
    {
        internal static FollowCamController instance { get; private set; }
        private static readonly float ZOOM_FACTOR = 0.05f;

        private readonly Camera cam;
        private readonly Rect textureRect, camRect;
        private readonly RenderTexture renderTexture;

        private int followCamHitboxDispState = 0, mainCamHitboxDispState = 0;
        private bool shown = true, showLoadScreens = true;

        public FollowCamController()
        {
            instance = this;
            cam = base.gameObject.AddComponent<Camera>();

            float camProp = GlobalSettings.instance.camProportion;
            camRect = new Rect(1 - camProp, 1 - camProp, camProp, camProp);
            cam.rect = camRect;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;

            renderTexture = new(Screen.width, Screen.height, 0);
            float scaledWidth = Screen.width * camProp, scaledHeight = Screen.height * camProp;
            textureRect = new(Screen.width - scaledWidth, 0, scaledWidth, scaledHeight);

            Transform t = cam.transform;
            t.parent = base.transform.parent;
            t.SetPositionZ(-200f);
            cam.depth = GameManager.instance.cameraCtrl.cam.depth + 10;

            GameCameras.instance.mainCamera.cullingMask &=
                ~(1 << FollowCam.HITBOX_REG_LAYER | 1 << FollowCam.HITBOX_MISC_LAYER);

            int hudLayer = GameCameras.instance.hudCanvas.gameObject.layer;
            var mask = cam.cullingMask;
            mask &= ~(1 << hudLayer);
            mask &= ~(1 << FollowCam.HITBOX_REG_LAYER | 1 << FollowCam.HITBOX_MISC_LAYER);
            cam.cullingMask = mask;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += AddSceneHitboxRenderers;
            ModHooks.ColliderCreateHook += CreateRenderer;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += CheckCamEnabled;
        }

        private void Update()
        {
            if (CameraControls.instance.toggleEnabled.WasPressed)
            {
                shown = !shown;
                cam.enabled = false;
                CheckCamEnabled();
            }

            if (CameraControls.instance.toggleCamBlanked.WasPressed)
                ToggleShowLoadScreens();

            if (CameraControls.instance.fCamChangeHitboxView.WasPressed)
                NextHitboxView(cam, ref followCamHitboxDispState);

            if (CameraControls.instance.mCamChangeHitboxView.WasPressed)
                NextHitboxView(GameCameras.instance.mainCamera, ref mainCamHitboxDispState);

            if (CameraControls.instance.zoomIn.WasPressed)
                cam.orthographicSize *= 1 - ZOOM_FACTOR;

            if (CameraControls.instance.zoomOut.WasPressed)
                cam.orthographicSize *= 1 + ZOOM_FACTOR;
        }

        private void OnGUI()
        {
            if (!shown || showLoadScreens || Event.current?.type != EventType.Repaint || GameManager.instance == null ||
                !GameManager.instance.IsGameplayScene())
                return;

            if (!renderTexture.IsCreated())
                renderTexture.Create();
            int depth = GUI.depth;
            GUI.depth = 0;
            cam.Render();
            GUI.DrawTexture(textureRect, renderTexture, ScaleMode.ScaleAndCrop, false);
            GUI.depth = depth;
        }

        private void CheckCamEnabled(Scene _, Scene __) => CheckCamEnabled();

        private void CheckCamEnabled()
        {
            if (!shown || !showLoadScreens)
                return;
            cam.enabled = GameManager.instance != null && GameManager.instance.IsGameplayScene();
        }

        private void NextHitboxView(Camera camera, ref int hitboxView)
        {
            hitboxView = (hitboxView + 1) % 3;
            if (hitboxView == 0)
            {
                camera.cullingMask &= ~(1 << FollowCam.HITBOX_REG_LAYER | 1 << FollowCam.HITBOX_MISC_LAYER);
            }
            else if (hitboxView == 1)
            {
                int mask = camera.cullingMask;
                mask &= ~(1 << FollowCam.HITBOX_MISC_LAYER);
                mask |= 1 << FollowCam.HITBOX_REG_LAYER;
                camera.cullingMask = mask;
            }
            else
            {
                camera.cullingMask |= (1 << FollowCam.HITBOX_REG_LAYER | 1 << FollowCam.HITBOX_MISC_LAYER);
            }
        }

        private void ToggleShowLoadScreens()
        {
            showLoadScreens = !showLoadScreens;
            if (showLoadScreens)
            {
                cam.targetTexture = null;
                cam.rect = camRect;
                CheckCamEnabled();
            }
            else
            {
                cam.targetTexture = renderTexture;
                cam.rect = new(0, 0, 1, 1);
                cam.enabled = false;
            }
        }

        private void AddSceneHitboxRenderers(Scene from, Scene to)
        {
            if (GameManager.instance == null || !GameManager.instance.IsGameplayScene())
                return;

            foreach (var c2d in Resources.FindObjectsOfTypeAll<Collider2D>())
            {
                AddLines.AttachLineRenderer(c2d);
            }
        }

        private void CreateRenderer(GameObject go)
        {
            foreach (var c2d in go.GetComponentsInChildren<Collider2D>())
            {
                AddLines.AttachLineRenderer(c2d);
            }
        }

        private void LateUpdate()
        {
            if (GameManager.instance == null || GameManager.instance.sceneName == "Menu_Title")
            {
                return;
            }

            var knightPosition = HeroController.instance.gameObject.transform.position;
            base.transform.position = new Vector3(knightPosition.x, knightPosition.y, base.transform.position.z);
        }
    }
}