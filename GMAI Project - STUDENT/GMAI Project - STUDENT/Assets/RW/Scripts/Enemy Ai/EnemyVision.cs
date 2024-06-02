using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assets.RW.Scripts.Enemy_Ai
{
    // Manages the AI vision.
    public class EnemyVision : MonoBehaviour
    {
        public float fieldOfView = 90.0f; // Object within this angle are seen.
        public float closeFieldDistance = 1.0f; // Objects below this distance is always seen.

        public List<Collider> colliders = new List<Collider>();
        public List<GameObject> visibles = new List<GameObject>();


        public float lastBulletSeenTime;

        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject == null) return;
            if ( !colliders.Contains(other))
            {
                colliders.Add(other);
                colliders.RemoveAll((c) => c == null);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (colliders.Contains(other))
            {
                colliders.Remove(other);
                colliders.RemoveAll((c) => c == null);
            }
        }


        void UpdateVisibility()
        {
            visibles.Clear();
            foreach (var c in colliders)
            {
                if (c == null)
                    continue;

                var go = c.attachedRigidbody != null ? c.attachedRigidbody.gameObject : null;

                bool isVisible = false;

                if (go != null)
                {

                    float angle = Vector3.Angle(this.transform.forward, go.transform.position - this.transform.position);

                    bool isInClosedField = Vector3.Distance(go.transform.position, this.transform.position) <= closeFieldDistance;
                    bool isInFieldOfView = Mathf.Abs(angle) <= fieldOfView * 0.5f;


                    isVisible = isInClosedField || (isInFieldOfView && HasLoS(go.gameObject));

                }

                if (isVisible && !visibles.Contains(go))
                {
                    visibles.Add(go);
                }

            }
        }

        bool HasLoS(GameObject target) // Line of sight test.
        {
            bool has = false;
            var targetDirection = (target.transform.position - this.transform.position).normalized;

            Ray ray = new Ray(this.transform.position, targetDirection);
            var hits = Physics.RaycastAll(ray, float.PositiveInfinity);

            float minD = float.PositiveInfinity;
            GameObject closest = null;

            foreach (var h in hits)
            {
                if(h.collider == null) continue;

                float d = Vector3.Distance(h.point, this.transform.position);
                var o = h.collider.gameObject;
                if (d <= minD && o != this.gameObject)
                {
                    minD = d;
                    closest = o;
                }
            }

            has = closest == target;

            return has;
        }


        // Use this for initialization
        void Start()
        {
            lastBulletSeenTime = float.NegativeInfinity;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateVisibility();
            DebugVisualise();
        }

        void DebugVisualise()
        {
            foreach (var o in visibles)
            {
                Debug.DrawLine(transform.position, o.transform.position, Color.red);

            }
        }
    }
}