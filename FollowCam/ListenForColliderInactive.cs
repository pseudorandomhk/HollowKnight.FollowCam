using System;
using UnityEngine;

namespace FollowCam
{
    public class ListenForColliderInactive : MonoBehaviour
    {
        public Collider2D c2d;
        public LineRenderer render;
        
        private void Update()
        {
            if (c2d == null)
                Destroy(this);
            render.enabled = c2d.isActiveAndEnabled;
        }
    }
}