using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRayController : MonoBehaviour
{
    public enum RayCastMode
    {
        Single,
        Multiple
    }

    public enum CameraMoveDir
    {
        Center,
        Left,
        Right,
        Up,
        Down
    }

    private Vector3 LeftMax = new Vector3(-1.3f, 0.0f, 0.0f);
    private Vector3 RightMax = new Vector3(1.3f, 0.0f, 0.0f);
    private Vector3 UpMax = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 DownMax = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 Center;

    private Vector3 Goal;

    private Camera cam;

    public Action OnRayCastNull;
    public Action<Collider> OnRayCastHit;
    public Action<RaycastHit []> OnRayCastHits;
    public Action<Vector3> OnMouseMovedInterval;

    private Action currentUpdateMethod;

    private RayCastMode currentMode;
    private LayerMask mask;
    private GameObject prevHoveredObject = null;

    public Vector3 currentUpdatedMousePoint;

    public float DragInterval = 70;
    public int MouseIntervalX = 26;
    public int MouseIntervalY = 26;

    public RayCastMode SetMode
    { set
        {
            currentMode = value;
            if (value == RayCastMode.Single) currentUpdateMethod = SingleModeUpdate;
            else currentUpdateMethod = MultipleModeUpdate;
        }
    }

    private void Awake()
    {
        SetMode = RayCastMode.Single;
        prevHoveredObject = gameObject;
    }

    private void OnEnable()
    {
        MoveCamera(CameraMoveDir.Center);
    }

    public void ClearPoint()
    {
        if(Input.touchCount > 0)
        {
            currentUpdatedMousePoint = Input.GetTouch(0).position;
        }
        else
        {
            currentUpdatedMousePoint = Vector3.zero;
        }
        
    }

    public void Initialize()
    {
        cam = GetComponent<Camera>();
        Center = transform.localPosition;
        LeftMax = transform.localPosition + LeftMax;
        RightMax = transform.localPosition + RightMax;
        UpMax = transform.localPosition + UpMax;
        DownMax = transform.localPosition + DownMax;
        Goal = Center;
    }

    public void MoveCamera(CameraMoveDir dir)
    {
        switch(dir)
        {
            case CameraMoveDir.Left:
                Goal = LeftMax;
                break;
            case CameraMoveDir.Right:
                Goal = RightMax;
                break;
            case CameraMoveDir.Up:
                Goal = UpMax;
                break;
            case CameraMoveDir.Down:
                Goal = DownMax;
                break;
            default:
                Goal = Center;
                break;
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        //if (UICamera.isOverUI) return;
#endif
        currentUpdateMethod.Invoke();
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(Goal,transform.localPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Goal, Time.fixedDeltaTime);
        }
    }

    private bool CheckNullTouch()
    {
        GameObject hoverObj = UICamera.hoveredObject;

        if (prevHoveredObject == null) prevHoveredObject = gameObject;
        if (hoverObj.name != prevHoveredObject.name)
        {
            prevHoveredObject = hoverObj;

            if (hoverObj.name == "UI Root")
            {
                Debug.Log("Touch Null ");
                if (OnRayCastNull != null) OnRayCastNull.Invoke();

                return true;
            }
        }

        return false;
    }

    private void SingleModeUpdate()
    {
        // UI일때 필터링
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(ray.origin, ray.direction, 100.0f))
            {
                currentUpdatedMousePoint = Input.mousePosition;
                Debug.Log("Clicked Something.");
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            currentUpdatedMousePoint = Input.mousePosition;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100.0f))
            {
                if (OnRayCastHit != null) OnRayCastHit.Invoke(hit.collider);
                Debug.Log("Start");
            }

            return;
        }
        else if (Input.GetMouseButton(0))
        {
            if (OnMouseMovedInterval == null) return;

            if (Mathf.Abs(Input.mousePosition.x - currentUpdatedMousePoint.x) >= MouseIntervalX ||
                Mathf.Abs(Input.mousePosition.y - currentUpdatedMousePoint.y) >= MouseIntervalY)
            {
                if(currentUpdatedMousePoint != Vector3.zero)
                {
                    Vector3 interval = Input.mousePosition - currentUpdatedMousePoint;

                    if (interval.magnitude < DragInterval) return;

                    //Debug.Log(interval.magnitude);

                    interval.x /= MouseIntervalX;
                    interval.y /= MouseIntervalY;

                    OnMouseMovedInterval.Invoke(interval);
                    Debug.Log("Do");
                }

                currentUpdatedMousePoint = Input.mousePosition;
            }
        }
        if (Input.touchCount == 0 ) return;

        Touch currentTouch = Input.GetTouch(0);

        if (currentTouch.phase == TouchPhase.Began)
        {
            if (OnRayCastHit == null) return;

            currentUpdatedMousePoint = currentTouch.position;

            Ray ray = cam.ScreenPointToRay(currentTouch.position);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100.0f))
            {
                OnRayCastHit.Invoke(hit.collider);
            }
           
            return;
        }
        else if (currentTouch.phase == TouchPhase.Moved)
        {
            if (OnMouseMovedInterval == null) return;

            Vector3 touchPosition = new Vector3(currentTouch.position.x,
                                              currentTouch.position.y,
                                              0.0f);

            if (currentUpdatedMousePoint != Vector3.zero)
            {
                Vector3 interval = touchPosition - currentUpdatedMousePoint;

                if (interval.magnitude < DragInterval) return;

                touchPosition.x /= MouseIntervalX;
                touchPosition.y /= MouseIntervalY;

                OnMouseMovedInterval.Invoke(touchPosition);
            }

            currentUpdatedMousePoint = touchPosition;
        }
    }

    private void MultipleModeUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (OnRayCastHits == null) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);

            if(hits.Length != 0)
            {
                 OnRayCastHits.Invoke(hits);
            }
        }
    }
}
