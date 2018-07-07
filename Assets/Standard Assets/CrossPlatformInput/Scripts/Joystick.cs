using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        float moveRange = 0.2f;
        int MovementRange = 100;
        public string horizontalAxisName = "Forward"; // The name given to the horizontal axis for the cross platform input
        public string verticalAxisName = "Right"; // The name given to the vertical axis for the cross platform input

        Vector3 m_StartPos;
        CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
        CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

        void OnEnable()
        {
            MovementRange = (int)(Screen.height * moveRange);
            m_StartPos = transform.position;
            CreateVirtualAxes();
        }

        void UpdateVirtualAxes(Vector3 value)
        {
            var delta = m_StartPos - value;
            delta.y = -delta.y;
            delta /= MovementRange;

            m_HorizontalVirtualAxis.Update(-delta.x);
            m_VerticalVirtualAxis.Update(delta.y);
            
        }

        void CreateVirtualAxes()
        {
            m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);

        }


        public void OnDrag(PointerEventData data)
        {
            Vector3 newPos = Vector3.zero;

            int deltaX = (int)(data.position.x - m_StartPos.x);
            int deltaY = (int)(data.position.y - m_StartPos.y);

            newPos.x = deltaX;
            newPos.y = deltaY;

            if(newPos.sqrMagnitude > MovementRange * MovementRange)
            {
                newPos *= MovementRange / newPos.magnitude;
            }

            transform.position = new Vector3(m_StartPos.x + newPos.x, m_StartPos.y + newPos.y, m_StartPos.z + newPos.z);
            UpdateVirtualAxes(transform.position);
        }


        public void OnPointerUp(PointerEventData data)
        {
            transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
        }


        public void OnPointerDown(PointerEventData data)
        {

        }

        void OnDisable()
        {
            m_HorizontalVirtualAxis.Remove();
            m_VerticalVirtualAxis.Remove();
        }
    }
}