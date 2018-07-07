using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class SwitchHandler : MonoBehaviour
    {
        public string Name;

        void OnEnable()
        {

        }

        DateTime lastDownTime = DateTime.Now;
        public void OnPointerDown()
        {
            TimeSpan span = DateTime.Now - lastDownTime;
            if (span.TotalMilliseconds > 200)
            {
                lastDownTime = DateTime.Now;
                bool down = CrossPlatformInputManager.GetButton(Name);
                Image img = GetComponent<Image>();
                if (down)
                {
                    CrossPlatformInputManager.SetButtonUp(Name);
                    img.color = Color.white * 0.8f;
                }
                else
                {
                    CrossPlatformInputManager.SetButtonDown(Name);
                    img.color = Color.cyan * 0.7f;
                }
            }
        }
        

        public void Update()
        {

        }
    }
}