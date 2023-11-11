using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ResizeOnDetection : MonoBehaviour
{
    public GameManager gameManager;
    public ARManager ARManager;

    public float initialSize = 1.0f; // The initial size of the road object
    public float maxSize = 5.0f; // The maximum size the road can be resized to
    public float minSize = 0.5f; // The minimum size the road can be resized to


    public Transform carHolder;

    public Transform finishHolder;

    public GameObject car;

    private DefaultObserverEventHandler observerEventHandler;
    private bool isTargetFound = false;

    private bool isCarMoving = false;
    private float timeMoving = 0f;

    float speed = 0.3f;
    float speedMlp = 1f;
    float holdTime = 0.2f;

    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        ARManager = GameObject.FindAnyObjectByType<ARManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager cannot find");
        }
        // Get the DefaultObserverEventHandler component from the parent "ImageTarget"
        observerEventHandler = transform.parent.GetComponent<DefaultObserverEventHandler>();

        if (observerEventHandler != null)
        {
            // Subscribe to the OnTargetFound and OnTargetLost events
            observerEventHandler.OnTargetFound.AddListener(OnTargetFound);
            observerEventHandler.OnTargetLost.AddListener(OnTargetLost);
        }
        else
        {
            Debug.LogError("DefaultObserverEventHandler not found on the parent ImageTarget object.");
        }

        // Hide buttons when the scene starts
    }

    private void LateUpdate()
    {
        // Update the timer if the car is moving
        if (isCarMoving)
        {
            MoveCarTowardsFinish();
            UpdateTimer();
        }
    }


    public string UpdateTimer()
    {
        timeMoving += Time.deltaTime;
        return (timeMoving + " s");
    }

    public float GetTimer() {
        return timeMoving;
    }

    public string UpdateSpeed()
    {
        return ((speed * speedMlp * 3.6f) + " km/h");
    }

    public bool GetFinish() 
    {
        if (isCarMoving)
        {
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (observerEventHandler != null)
        {
            // Unsubscribe from the events when the object is destroyed
            observerEventHandler.OnTargetFound.RemoveListener(OnTargetFound);
            observerEventHandler.OnTargetLost.RemoveListener(OnTargetLost);
        }
    }

    private void OnTargetFound()
    {
        // ImageTarget detected, resize the road to the initial size
        ResizeRoad(initialSize);

        ARManager.SetCurrentRoad(this);

        // Show buttons

        isTargetFound = true;
    }

    private void OnTargetLost()
    {
        // ImageTarget lost, reset the road size to the initial size (optional)
        // You can implement this method based on your requirements when the target is lost
        ResizeRoad(initialSize);

        // Hide buttons

        isTargetFound = false;
    }

    private void ResizeRoad(float size)
    {
        if (isTargetFound)
        {
            // Clamp the size within the specified range
            size = Mathf.Clamp(size, minSize, maxSize);

            // Scale the road object to the new size
            transform.localScale = new Vector3(size, 1.0f, size);
        }
    }

    public float MakeRoadLonger()
    {
        if (isTargetFound)
        {
            // Increase the size of the road
            float newSize = transform.localScale.x + 0.1f;
            ResizeRoad(newSize);
            return newSize;
        }
        return transform.localScale.x;
    }

    public float MakeRoadShorter()
    {
        if (isTargetFound)
        {
            // Decrease the size of the road
            float newSize = transform.localScale.x - 0.1f;
            ResizeRoad(newSize);
            return newSize;
        }
        return transform.localScale.x;
    }

    public void SpawnCar()
    {
        Debug.Log("Hello");
        gameManager.LoadCarPrefab(carHolder, true);
        car = gameManager.carPrefab;
    }

    public void StartCar()
    {
        isCarMoving = true;
    }

    public void MoveCarTowardsFinish()
    {
        if (car == null || finishHolder == null)
        {
            Debug.LogError("Car or FinishHolder is not set.");
            return;
        }

        // Move car towards the finish point
        car.transform.position = Vector3.MoveTowards(car.transform.position, finishHolder.position, speed * speedMlp * Time.deltaTime);

        // Check if the car has reached the finish point within a tolerance
        if (Vector3.Distance(car.transform.position, finishHolder.position) < 0.1f)
        {
            // Car has reached the finish
            isCarMoving = false;
        }
    }
}
