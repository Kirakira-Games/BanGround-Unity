using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Codes
{
    class func_rotate : MonoBehaviour
    {
        public float speed;

        private Vector3 angles;

        private void Start()
        {
            angles = transform.rotation.eulerAngles;
        }

        private void Update()
        {
            angles.z += speed;
            transform.rotation = Quaternion.Euler(angles.x, angles.y, angles.z);
        }
    }
}
