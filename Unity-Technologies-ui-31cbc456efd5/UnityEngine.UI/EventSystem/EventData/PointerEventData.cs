using System;
using System.Text;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Each touch event creates one of these containing all the relevant information.
    /// 点击事件类
    /// 它存储了大部分的事件系统逻辑需要的数据，包括按下时的位置，松开与按下的时间差，
    //  拖动的位移差，点击到的物体等等，承载了所有输入事件需要的数据。事件数据模块的意义
    //  所在便是存储数据并为逻辑部分做好准备。
    /// </summary>
    public class PointerEventData : BaseEventData
    {
        /// <summary>
        /// Input press tracking.
        /// </summary>
        public enum InputButton
        {
            /// <summary>
            /// Left button
            /// </summary>
            Left = 0,

            /// <summary>
            /// Right button.
            /// </summary>
            Right = 1,

            /// <summary>
            /// Middle button
            /// </summary>
            Middle = 2
        }

        /// <summary>
        /// The state of a press for the given frame.
        /// </summary>
        public enum FramePressState
        {
            /// <summary>
            /// Button was pressed this frame.
            /// </summary>
            Pressed,

            /// <summary>
            /// Button was released this frame.
            /// </summary>
            Released,

            /// <summary>
            /// Button was pressed and released this frame.
            /// </summary>
            PressedAndReleased,

            /// <summary>
            /// Same as last frame.
            /// </summary>
            NotChanged
        }

        /// <summary>
        /// The object that received 'OnPointerEnter'.
        /// </summary>
        public GameObject pointerEnter { get; set; }

        // The object that received OnPointerDown
        //接收OnPointDown事件的物体
        private GameObject m_PointerPress;

        /// <summary>
        /// The raw GameObject for the last press event. This means that it is the 'pressed' GameObject even if it can not receive the press event itself.
        /// 上一接收OnPointDown事件的物体
        /// </summary>
        public GameObject lastPress { get; private set; }

        /// <summary>
        /// 接收按下事件的无法响应处理的物体
        /// The object that the press happened on even if it can not handle the press event.
        /// </summary>
        public GameObject rawPointerPress { get; set; }

        /// <summary>
        /// The object that is receiving 'OnDrag'.
        /// 接收OnDrag事件的物体
        /// </summary>
        public GameObject pointerDrag { get; set; }

        /// <summary>
        /// RaycastResult associated with the current event.
        /// </summary>
        public RaycastResult pointerCurrentRaycast { get; set; }

        /// <summary>
        /// RaycastResult associated with the pointer press.
        /// </summary>
        public RaycastResult pointerPressRaycast { get; set; }

        public List<GameObject> hovered = new List<GameObject>();

        /// <summary>
        /// Is it possible to click this frame
        /// </summary>
        public bool eligibleForClick { get; set; }

        /// <summary>
        /// Id of the pointer (touch id).
        /// </summary>
        public int pointerId { get; set; }

        /// <summary>
        /// Current pointer position.
        /// 目前触碰点的位置
        /// </summary>
        public Vector2 position { get; set; }

        /// <summary>
        /// Pointer delta since last update.
        /// 滚轮的偏移速度
        /// </summary>
        public Vector2 delta { get; set; }

        /// <summary>
        /// Position of the press.
        /// 按下的位置
        /// </summary>
        public Vector2 pressPosition { get; set; }

        /// <summary>
        /// World-space position where a ray cast into the screen hits something
        /// </summary>

        [Obsolete("Use either pointerCurrentRaycast.worldPosition or pointerPressRaycast.worldPosition")]
        public Vector3 worldPosition { get; set; }

        /// <summary>
        /// World-space normal where a ray cast into the screen hits something
        /// </summary>
        [Obsolete("Use either pointerCurrentRaycast.worldNormal or pointerPressRaycast.worldNormal")]
        public Vector3 worldNormal { get; set; }

        /// <summary>
        /// The last time a click event was sent. Used for double click
        /// 上次点击的时间，用来实现双击效果
        /// </summary>
        public float clickTime { get; set; }

        /// <summary>
        /// Number of clicks in a row.
        /// 点击的次数
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        /// using UnityEngine.EventSystems;// Required when using Event data.
        ///
        /// public class ExampleClass : MonoBehaviour, IPointerDownHandler
        /// {
        ///     public void OnPointerDown(PointerEventData eventData)
        ///     {
        ///         //Grab the number of consecutive clicks and assign it to an integer varible.
        ///         int i = eventData.clickCount;
        ///         //Display the click count.
        ///         Debug.Log(i);
        ///     }
        /// }
        /// </code>
        /// </example>
        public int clickCount { get; set; }

        /// <summary>
        /// The amount of scroll since the last update.
        /// </summary>
        public Vector2 scrollDelta { get; set; }

        /// <summary>
        /// Should a drag threshold be used?
        /// </summary>
        /// <remarks>
        /// If you do not want a drag threshold set this to false in IInitializePotentialDragHandler.OnInitializePotentialDrag.
        /// </remarks>
        public bool useDragThreshold { get; set; }

        /// <summary>
        /// Is a drag operation currently occuring.
        /// </summary>
        public bool dragging { get; set; }

        /// <summary>
        /// The EventSystems.PointerEventData.InputButton for this event.
        /// </summary>
        public InputButton button { get; set; }

        //构造函数
        public PointerEventData(EventSystem eventSystem) : base(eventSystem)
        {
            eligibleForClick = false;

            pointerId = -1;
            position = Vector2.zero; // Current position of the mouse or touch event
            delta = Vector2.zero; // Delta since last update
            pressPosition = Vector2.zero; // Delta since the event started being tracked
            clickTime = 0.0f; // The last time a click event was sent out (used for double-clicks)
            clickCount = 0; // Number of clicks in a row. 2 for a double-click for example.

            scrollDelta = Vector2.zero;
            useDragThreshold = true;
            dragging = false;
            button = InputButton.Left;
        }

        /// <summary>
        /// Is the pointer moving.
        /// </summary>
        public bool IsPointerMoving()
        {
            return delta.sqrMagnitude > 0.0f;
        }

        /// <summary>
        /// Is scroll being used on the input device.
        /// </summary>
        public bool IsScrolling()
        {
            return scrollDelta.sqrMagnitude > 0.0f;
        }

        /// <summary>
        /// The camera associated with the last OnPointerEnter event.
        /// </summary>
        public Camera enterEventCamera
        {
            get { return pointerCurrentRaycast.module == null ? null : pointerCurrentRaycast.module.eventCamera; }
        }

        /// <summary>
        /// The camera associated with the last OnPointerPress event.
        /// </summary>
        public Camera pressEventCamera
        {
            get { return pointerPressRaycast.module == null ? null : pointerPressRaycast.module.eventCamera; }
        }

        /// <summary>
        /// The GameObject that received the OnPointerDown.
        /// </summary>
        public GameObject pointerPress
        {
            get { return m_PointerPress; }
            set
            {
                if (m_PointerPress == value)
                    return;

                lastPress = m_PointerPress;
                m_PointerPress = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Position</b>: " + position);
            sb.AppendLine("<b>delta</b>: " + delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
            sb.AppendLine("<b>Current Raycast:</b>");
            sb.AppendLine(pointerCurrentRaycast.ToString());
            sb.AppendLine("<b>Press Raycast:</b>");
            sb.AppendLine(pointerPressRaycast.ToString());
            return sb.ToString();
        }
    }
}
