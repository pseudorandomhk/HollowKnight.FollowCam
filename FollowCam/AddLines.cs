using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using UnityEngine;

namespace FollowCam
{
    public static class AddLines
    {
        private static readonly Dictionary<HitboxType, Material> _materials = new();
        private static readonly float CIRCLE_THETA_SCALE = 0.01f;
        private static readonly string TAG = "FC::Hitbox";

        private static float lineWidth = 0.1f;

        static AddLines()
        {
            foreach (HitboxType t in (HitboxType[])Enum.GetValues(typeof(HitboxType)))
            {
                _materials[t] = new Material(Shader.Find("Sprites/Default"));
                _materials[t].color = t.GetColor();
            }
        }

        public static void AttachLineRenderer(Collider2D c2d)
        {
            if (c2d == null || (c2d.gameObject.scene.name == "DontDestroyOnLoad" &&
                                c2d.gameObject.GetComponentInChildren<LineRenderer>()))
                return;

            HitboxType t = FindHitboxType(c2d);
            GameObject go = new GameObject(TAG);
            go.layer = t == HitboxType.Other ? FollowCam.HITBOX_MISC_LAYER : FollowCam.HITBOX_REG_LAYER;
            go.transform.parent = c2d.transform;
            var renderer = InitLineRenderer(go, t);

            var listener = go.AddComponent<ListenForColliderInactive>();
            listener.c2d = c2d;
            listener.render = renderer;

            if (c2d is BoxCollider2D bc2d)
            {
                Vector2 half = bc2d.size / 2f;
                Vector3 offset = bc2d.offset;
                Vector3[] points =
                {
                    new Vector3(-half.x, half.y) + offset,
                    (Vector3)half + offset,
                    new Vector3(half.x, -half.y) + offset,
                    -(Vector3)half + offset
                };
                renderer.SetVertexCount(4);
                renderer.SetPositions(points);
            }
            else if (c2d is EdgeCollider2D ec2d)
            {
                renderer.SetVertexCount(ec2d.pointCount);
                renderer.SetPositions(ec2d.points.Select(p => (Vector3)p).ToArray());
            }
            else if (c2d is CircleCollider2D cc2d)
            {
                int npoints = (int)(1 / CIRCLE_THETA_SCALE);
                renderer.SetVertexCount(npoints);
                Vector3[] points = new Vector3[npoints];
                float theta = 0, r = cc2d.radius;
                for (int i = 0; i < npoints; i++)
                {
                    theta += 2 * Mathf.PI * CIRCLE_THETA_SCALE;
                    points[i] = new(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
                }

                renderer.SetPositions(points);
            }
            else if (c2d is PolygonCollider2D pc2d)
            {
                var renderers = new LineRenderer[pc2d.pathCount];
                renderers[0] = renderer;
                for (int i = 0; i < pc2d.pathCount; i++)
                {
                    if (i > 0)
                    {
                        renderers[i] = InitLineRenderer(go, t);
                        ListenForColliderInactive lfci = go.AddComponent<ListenForColliderInactive>();
                        lfci.c2d = pc2d;
                        lfci.render = renderers[i];
                    }

                    Vector3[] points = pc2d.GetPath(i).Select(p => (Vector3)p).ToArray();
                    renderers[i].SetVertexCount(points.Length);
                    renderers[i].SetPositions(points);
                }
            }
            else
            {
                FollowCam.Instance.Log($"unsupported collider {c2d.GetType()} on {c2d.transform.parent}");
            }
        }

        private static LineRenderer InitLineRenderer(GameObject parent, HitboxType t)
        {
            LineRenderer renderer = parent.AddComponent<LineRenderer>();

            Transform transform = renderer.transform;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            renderer.sharedMaterial = _materials[t];
            renderer.SetColors(t.GetColor(), t.GetColor());
            renderer.SetWidth(lineWidth, lineWidth);
            renderer.sortingOrder = 100;
            renderer.useWorldSpace = false;
            // renderer.sortingLayerID = SortingLayer.NameToID("Over");

            return renderer;
        }

        public static HitboxType FindHitboxType(Collider2D c)
        {
            GameObject go = c.gameObject, knight = null;
            if (HeroController.instance != null)
                knight = HeroController.instance.gameObject;

            if (go.LocateMyFSM("damages_hero"))
                return HitboxType.Enemy;
            if (go.layer == (int)PhysLayers.TERRAIN)
                return go.name.Contains("Breakable") || go.name.Contains("Collapse")
                    ? HitboxType.Breakable
                    : HitboxType.Terrain;
            if (knight != null && go == knight && !c.isTrigger)
                return HitboxType.Knight;
            if (go.name == "Damager" || go.LocateMyFSM("damages_enemy") || go.LocateMyFSM("Damage"))
                return HitboxType.Attack;
            if (c.isTrigger && c.GetComponent<HazardRespawnTrigger>())
                return HitboxType.HazardRespawn;
            if (c.isTrigger && c.GetComponent<TransitionPoint>())
                return HitboxType.Gate;
            return HitboxType.Other;
        }
    }

    public enum HitboxType
    {
        Knight,
        Enemy,
        Attack,
        Terrain,
        Trigger,
        Breakable,
        Gate,
        HazardRespawn,
        Other
    }

    public static class HitboxTypeExtensions
    {
        private static readonly Dictionary<HitboxType, Color> colorinfo = new()
        {
            { HitboxType.Knight, Color.yellow },
            { HitboxType.Enemy, new Color(0.8f, 0, 0) },
            { HitboxType.Attack, Color.cyan },
            { HitboxType.Terrain, new Color(0, 0.8f, 0) },
            { HitboxType.Trigger, new Color(0.5f, 0.5f, 1f) },
            { HitboxType.Breakable, new Color(1f, 0.75f, 0.8f) },
            { HitboxType.Gate, new Color(0, 0, 0.5f) },
            { HitboxType.HazardRespawn, new Color(0.5f, 0, 0.5f) },
            { HitboxType.Other, new Color(0.8f, 0.6f, 0.4f) }
        };

        public static Color GetColor(this HitboxType t)
        {
            return colorinfo[t];
        }
    }
}