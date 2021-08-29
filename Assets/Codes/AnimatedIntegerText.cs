using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using V2;

namespace BanGround.Compoments
{
    public class AnimatedIntegerText : Text
    {
        [SerializeField]
        private Transition m_TransitionType = Transition.Linear;
        [SerializeField]
        private float m_TransitionTime = 1.0f;
        [SerializeField]
        private bool m_GoZeroFirst = false;

        private float startTime;
        private int previous = 0;
        private int current = 0;
        private int target = 0;
        [SerializeField]
        private int next = 0;

        public int number
        {
            get
            {
                return next;
            }
            set
            {
                next = value;
                target = m_GoZeroFirst ? 0 : next;
                startTime = Time.time;
            }
        }

        protected override void Start()
        {
            base.Start();

            previous = current = target = next;
        }

        private void LateUpdate()
        {
            if (previous != target)
            {
                float progress = (Time.time - startTime) / m_TransitionTime;

                if (progress >= 1.0f)
                {
                    current = target;
                    previous = target;

                    if (target == 0 && next != 0)
                    {
                        target = next;
                        startTime = Time.time;
                    }
                }
                else
                {
                    current = TransitionLib.Lerp(previous, target, progress, m_TransitionType);
                }

                base.text = current.ToString();
            }
        }
    }
}
