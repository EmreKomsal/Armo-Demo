using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ResizeOnDetection : MonoBehaviour
{
    public GameManager gameManager;

    public float initialSize = 1.0f; // The initial size of the road object
    public float maxSize = 5.0f; // The maximum size the road can be resized to
    public float minSize = 0.5f; // The minimum size the road can be resized to

    public GameObject[] buttonsToToggle; // Array of buttons to toggle visibility

    public Button btnStart;

    public Transform carHolder;

    public Transform finishHolder;

    public GameObject car;

    private DefaultObserverEventHandler observerEventHandler;
    private bool isTargetFound = false;

    private bool isCarMoving = false;
    private float timeMoving = 0f;

    float speed = 0.3f;
    float speedMlt = 1f;
    float holdTime = 0.2f;

    public TMP_Text timer;

    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
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
        SetButtonsVisibility(false);
    }

    private void Update()
    {
        // Update the timer if the car is moving
        if (isCarMoving)
        {
            timeMoving += Time.deltaTime;
            // Optional: Debug the timer
            timer.text = ("Car moving for: " + timeMoving + " seconds");
        }
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

        // Show buttons
        SetButtonsVisibility(true);

        isTargetFound = true;
    }

    private void OnTargetLost()
    {
        // ImageTarget lost, reset the road size to the initial size (optional)
        // You can implement this method based on your requirements when the target is lost
        ResizeRoad(initialSize);

        // Hide buttons
        SetButtonsVisibility(false);

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

    public void MakeRoadLonger()
    {
        if (isTargetFound)
        {
            // Increase the size of the road
            float newSize = transform.localScale.x + 0.1f;
            ResizeRoad(newSize);
        }
    }

    public void MakeRoadShorter()
    {
        if (isTargetFound)
        {
            // Decrease the size of the road
            float newSize = transform.localScale.x - 0.1f;
            ResizeRoad(newSize);
        }
    }

    public void SpawnCar()
    {
        SetButtonsVisibility(false);
        //Spawn car to start of the road
        gameManager.LoadCarPrefab(carHolder);
        car = gameManager.carPrefab;
        btnStart.onClick.AddListener(MoveCarToFinish);
        btnStart.gameObject.SetActive(true);
    }



    private void SetButtonsVisibility(bool visible)
    {
        foreach (GameObject button in buttonsToToggle)
        {
            button.SetActive(visible);
        }
    }


    public void MoveCarToFinish()
    {
        StartCoroutine(MoveCarCoroutine(speed, holdTime));
        btnStart.GetComponent<Button>().interactable = false;
    }

    private IEnumerator MoveCarCoroutine(float speed, float holdTime)
    {
        // Wait for the specified hold time before moving the car
        yield return new WaitForSeconds(holdTime);

        isCarMoving = true;
        timeMoving = 0f; // Reset the timer

        while (Vector3.Distance(car.transform.position, finishHolder.position) > 0.1f) // Tolerance of 0.1 units
        {
            car.transform.position = Vector3.MoveTowards(car.transform.position, finishHolder.position, speed * speedMlp * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        isCarMoving = false; // Stop the timer when the car reaches the finish point
    }
}
