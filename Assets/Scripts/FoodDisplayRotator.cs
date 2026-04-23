using UnityEngine;
using UnityEngine.EventSystems;

public class FoodDisplayRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float autoRotationSpeed = 45f;
    [SerializeField] private float interactionSpinDegrees = 360f;
    [SerializeField] private float interactionSpinSpeed = 300f;
    [SerializeField] private float swipeTriggerThreshold = 28f;
    [SerializeField] private float pinchZoomSensitivity = 0.003f;
    [SerializeField] private float mouseWheelZoomSensitivity = 0.2f;
    [SerializeField] private float minScaleMultiplier = 0.75f;
    [SerializeField] private float maxScaleMultiplier = 1.65f;

    private bool interactionEnabled = true;
    private float queuedSpinDegrees;
    private Vector3 initialScale;
    private float currentScaleMultiplier = 1f;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void Configure(Vector3 axis, float speed)
    {
        rotationAxis = axis.sqrMagnitude > 0f ? axis.normalized : Vector3.up;
        autoRotationSpeed = speed;
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;
    }

    private void Update()
    {
        bool isUserInteracting = IsUserInteracting();

        if (TryGetSwipeDirection(out float swipeDirection))
        {
            QueueInteractionSpin(swipeDirection);
        }

        if (Mathf.Abs(queuedSpinDegrees) > 0.01f)
        {
            float step = interactionSpinSpeed * Time.deltaTime;
            float rotationStep = Mathf.Min(Mathf.Abs(queuedSpinDegrees), step) * Mathf.Sign(queuedSpinDegrees);
            transform.Rotate(rotationAxis, rotationStep, Space.World);
            queuedSpinDegrees -= rotationStep;
        }
        else if (!isUserInteracting)
        {
            transform.Rotate(rotationAxis, autoRotationSpeed * Time.deltaTime, Space.Self);
        }

        if (TryGetPinchDelta(out float pinchDelta))
            ApplyZoom(pinchDelta * pinchZoomSensitivity);

        if (TryGetScrollDelta(out float scrollDelta))
            ApplyZoom(scrollDelta * mouseWheelZoomSensitivity);
    }

    private void QueueInteractionSpin(float swipeDirection)
    {
        if (Mathf.Abs(swipeDirection) <= 0f)
            return;

        queuedSpinDegrees = -Mathf.Sign(swipeDirection) * interactionSpinDegrees;
    }

    private bool TryGetSwipeDirection(out float swipeDirection)
    {
        swipeDirection = 0f;

        if (!interactionEnabled)
            return false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && !IsTouchOverUI(touch.fingerId))
            {
                swipeDirection = touch.deltaPosition.x;
                return Mathf.Abs(swipeDirection) >= swipeTriggerThreshold;
            }

            return false;
        }

        if (Input.GetMouseButton(0) && !IsPointerOverUI())
        {
            swipeDirection = Input.GetAxis("Mouse X") * 20f;
            return Mathf.Abs(swipeDirection) >= swipeTriggerThreshold;
        }

        return false;
    }

    private bool TryGetPinchDelta(out float pinchDelta)
    {
        pinchDelta = 0f;

        if (!interactionEnabled || Input.touchCount < 2)
            return false;

        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        if (IsTouchOverUI(firstTouch.fingerId) || IsTouchOverUI(secondTouch.fingerId))
            return false;

        Vector2 firstPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

        float previousDistance = Vector2.Distance(firstPreviousPosition, secondPreviousPosition);
        float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);

        pinchDelta = currentDistance - previousDistance;
        return Mathf.Abs(pinchDelta) > 0.01f;
    }

    private bool TryGetScrollDelta(out float scrollDelta)
    {
        scrollDelta = 0f;

        if (!interactionEnabled || IsPointerOverUI())
            return false;

        scrollDelta = Input.mouseScrollDelta.y;
        return Mathf.Abs(scrollDelta) > 0.001f;
    }

    private void ApplyZoom(float zoomDelta)
    {
        currentScaleMultiplier = Mathf.Clamp(
            currentScaleMultiplier + zoomDelta,
            minScaleMultiplier,
            maxScaleMultiplier);

        transform.localScale = initialScale * currentScaleMultiplier;
    }

    private static bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsUserInteracting()
    {
        if (!interactionEnabled)
            return false;

        if (Input.touchCount > 1)
            return true;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!IsTouchOverUI(touch.fingerId))
                return true;
        }

        return (Input.GetMouseButton(0) || Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f) && !IsPointerOverUI();
    }
}
