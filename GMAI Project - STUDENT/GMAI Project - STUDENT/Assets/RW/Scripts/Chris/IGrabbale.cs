using System.Collections;
using UnityEngine;

namespace Assets.RW.Scripts.Chris
{//ignore this.
    public interface IGrabbale 
    {
        void Grab();

        void Drop();
    }

    public class LightBox : MonoBehaviour, IGrabbale
    {
        public void Drop()
        {
            //
        }

        public void Grab()
        {
            //Grab one with hand
        }
    }

    public class HeavyBox : MonoBehaviour, IGrabbale
    {
        public void Drop()
        {
            throw new System.NotImplementedException();
        }

        public void Grab()
        {
            //Grab two hand
        }
    }

    public class PlayerGrabber : MonoBehaviour
    {
        private IGrabbale item;

        void SenseStuff()
        {
            //sphere cast to get items
            var colliders =Physics.OverlapSphere(Vector3.zero, 5f);

            foreach(var collider in colliders)
            {
                IGrabbale item = collider.GetComponent<IGrabbale>();
                //for light and heavy boxes work :)))
                this.item = item;
            }

        }
    }

    public class BotBT : MonoBehaviour
    {
        //will cache the item into the bot inventory system.
        private IGrabbale item;

        //in one of chris many function
        void SenseStuff()
        {
            //sphere cast to get items
            var colliders = Physics.OverlapSphere(Vector3.zero, 5f);

            foreach (var collider in colliders)
            {
                IGrabbale item = collider.GetComponent<IGrabbale>();
                //for light and heavy boxes work :)))
                this.item = item;
            }
        }

    }
}