using UnityEngine;
using UnityEngine.EventSystems;

public class RotateCar : MonoBehaviour, IDragHandler
{
    public GameObject objectToRotate;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = Input.mousePosition;
    }

    void Update()
    {
        // Check for mouse input if running on desktop
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            if (Input.GetMouseButton(0))
            {
                Rotate(Input.mousePosition);
            }
        }
        // Check for touch input if running on a mobile device
        else if (Application.platform == RuntimePlatform.Android ||
                 Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    Rotate(touch.position);
                }
            }
        }

        lastPosition = Input.mousePosition;
    }

    public void Rotate(Vector3 currentPosition)
    {
        float rotationAmount = currentPosition.x - lastPosition.x;
        objectToRotate.transform.Rotate(0, -rotationAmount, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Rotate(Input.mousePosition);
    }
}
