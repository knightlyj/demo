using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
    [RequireComponent(typeof(Image))]
    public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {


        public enum ControlStyle
        {
            Absolute, // operates from teh center of the image
            Relative, // operates from the center of the initial touch
            Swipe, // swipe to touch touch no maintained center
        }


        public ControlStyle controlStyle = ControlStyle.Absolute; // control style to use
        public string horizontalAxisName = "CameraX"; // The name given to the horizontal axis for the cross platform input
        public string verticalAxisName = "CameraY"; // The name given to the vertical axis for the cross platform input
        public float Xsensitivity = 1f;
        public float Ysensitivity = 1f;

        Vector3 m_StartPos;
        Vector2 m_PreviousDelta;
        Vector3 m_JoytickOutput;
        CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
        CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input
        bool m_Dragging;
        int m_Id = -1;
        Vector2 m_PreviousTouchPos; // swipe style control touch

        
        Vector3 m_PreviousMouse;
        void OnEnable()
        {
            CreateVirtualAxes();
        }

        void CreateVirtualAxes()
        {
            m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);

            m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
        }

        void UpdateVirtualAxes(Vector3 value)
        {
            value = value.normalized;

            m_HorizontalVirtualAxis.Update(value.x);
            m_VerticalVirtualAxis.Update(value.y);

        }


        public void OnPointerDown(PointerEventData data)
        {
            m_Dragging = true;
            m_Id = data.pointerId;

        }

        void Update()
        {
            if (!m_Dragging)
            {
                return;
            }
            Vector2 pointerDelta;
            pointerDelta.x = Input.mousePosition.x - m_PreviousMouse.x;
            pointerDelta.y = Input.mousePosition.y - m_PreviousMouse.y;
            m_PreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
            UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));

        }


        public void OnPointerUp(PointerEventData data)
        {
            m_Dragging = false;
            m_Id = -1;
            UpdateVirtualAxes(Vector3.zero);
        }

        void OnDisable()
        {
            if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
        }
    }
}