using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class ResourcesInstance_Test : MonoBehaviour
    {
        public ResourcesInstance_RegisterMono handler;
        public string testResourcesPath;


        void OnEnable()
        {
            handler.InstanceResourceBroadcast(testResourcesPath, transform.position, transform.rotation);
        }
    }
}
