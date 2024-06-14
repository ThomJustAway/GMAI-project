using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assets.RW.Scripts.Enemy_Ai
{
    // Manages the AI vision.
    // This is taken from Chris so some shout out since I didn't
    // want to create it from scratch.
    public class EnemyVision : MonoBehaviour
    {
        public float fieldOfView = 90.0f; // Object within this angle are seen.
        public float closeFieldDistance = 1.0f; // Objects below this distance is always seen.

        public List<Collider> colliders = new List<Collider>();
        public List<GameObject> visibles = new List<GameObject>();

        /// <summary>
        /// Will check the what has entered the collider and add it
        /// to the list of collider that is already detected.
        /// </summary>
        /// <param name="other">collider that has been detected.</param>
        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject == null) return;
            if ( !colliders.Contains(other))
            {//if there is a collider then add it to the list.
                colliders.Add(other);
                colliders.RemoveAll((c) => c == null);
            }
        }

        void OnTriggerExit(Collider other)
        {//make sure to remove the collider if it is out of the trigger.
            if (colliders.Contains(other))
            {
                colliders.Remove(other);
                colliders.RemoveAll((c) => c == null);
            }
        }


        void UpdateVisibility()
        {
            //will consistently update the visible array.
            visibles.Clear();
            foreach (var c in colliders)
            {
                //so it would check what is visible to the users
                if (c == null)
                    continue;
                
                var go = c.attachedRigidbody != null ? c.attachedRigidbody.gameObject : null;
                //will check if there is a rigidboy, if have, then it 
                //is a viable gameobject to retrieve and just take the gameobject. else ignore it.
                bool isVisible = false;

                if (go != null)
                {
                    //will check the angle if it is within range.
                    float angle = Vector3.Angle(this.transform.forward, go.transform.position - this.transform.position);

                    bool isInClosedField = Vector3.Distance(go.transform.position, this.transform.position) <= closeFieldDistance;
                    bool isInFieldOfView = Mathf.Abs(angle) <= fieldOfView * 0.5f;
                    //check if the object is within a certain distance and is within a certain range.

                    isVisible = isInClosedField || (isInFieldOfView && HasLoS(go.gameObject));

                }

                if (isVisible && !visibles.Contains(go))
                {
                    visibles.Add(go);
                }

            }
        }

        /// <summary>
        /// This would check if the target object is not block by anything. This 
        /// is to make sure the the enemy cant see through walls.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool HasLoS(GameObject target) // Line of sight test.
        {
            var targetDirection = (target.transform.position - this.transform.position).normalized;
            //create a new ray to raycast to the target gameobject.
            Ray ray = new Ray(this.transform.position, targetDirection);
            var hits = Physics.RaycastAll(ray, float.PositiveInfinity);

            float minD = float.PositiveInfinity;
            GameObject closest = null;

            //will determine if the target object is closest.
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
            //check if the target that was found is the target we want.
            return closest == target;
        }


        // Update is called once per frame
        void Update()
        {
            UpdateVisibility();
            //DebugVisualise();
        }

        //for debugging purposes.
        void DebugVisualise()
        {
            foreach (var o in visibles)
            {
                Debug.DrawLine(transform.position, o.transform.position, Color.red);

            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, closeFieldDistance);
        }
    }
}