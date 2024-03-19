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

    public PartEffectController.GroundType groundType;

    public string roadName;
    public Color roadNameColor;
    
    
    public Transform carHolder;

    public Transform finishHolder;

    public GameObject car;

    public GameObject ghostT;
    
    public Transform carHolder2;

    public Transform finishHolder2;

    public GameObject car2;

    public bool IsGhostAvailable { get; private set; } = false;

    private DefaultObserverEventHandler observerEventHandler;
    private bool isTargetFound = false;

    // private bool isCarMoving = false;
    // private float timeMoving = 0f;


    private float elapsed = 0f;
    private float elapsedGhost = 0f;
    
    
    
    
    float speed = 0.3f;
    private float projSpeed = 0f;
    public float speedMlp = 1f;
    // float holdTime = 0.2f;

    private float ghostSpeed;
    private float ghostProjSpeed;
    
    
    public float lastSize;

    public float GetTimeMoving()
    {
        return elapsed;
    }

    public float GetSpeed()
    {
        return lastSpeed;
    }
    
    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        ARManager = GameObject.FindAnyObjectByType<ARManager>();
        lastSize = initialSize;
        if (gameManager == null)
        {
            Debug.LogError("GameManager cannot find");
        }
        // Get the DefaultObserverEventHandler component from the parent "ImageTarget"
        observerEventHandler = transform.parent.GetComponent<DefaultObserverEventHandler>();

        ghostT.SetActive(false);

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

    // private void LateUpdate()
    // {
    //     // Update the timer if the car is moving
    //     if (isCarMoving)
    //     {
    //         MoveCarTowardsFinish();
    //         UpdateTimer();
    //     }
    // }


    // public string UpdateTimer()
    // {
    //     timeMoving += Time.deltaTime;
    //     return (timeMoving.ToString("F") + " s");
    // }
    //
    // public float GetTimer() {
    //     return timeMoving;
    // }

    private float lastSpeed = 0;
    private float lastSpeedGhost = 0;
    
    
    
    // public string UpdateSpeed()
    // {
    //     var projectedSpeed = Mathf.Lerp(PartEffectController.I.shownSpeedRange.x, PartEffectController.I.shownSpeedRange.y,speed - PartEffectController.I.GetMinMaxSpeed().x) /
    //                          (PartEffectController.I.GetMinMaxSpeed().y - PartEffectController.I.GetMinMaxSpeed().x);
    //     lastSpeed = projectedSpeed;
    //     return (projectedSpeed.ToString("F1")+ " km/sa");
    // }

    // public string UpdateFriction()
    // {
    //     return ("%" + speedMlp);
    // }

    // public bool GetFinish() 
    // {
    //     if (isCarMoving)
    //     {
    //         return false;
    //     }
    //     return true;
    // }

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
        // if (!ARManager.DidSkipDangerInfo)
        // {
        //     return;
        // }
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
            lastSize = size;
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
        // Debug.Log("Hello");
        gameManager.LoadCarPrefab(carHolder, true);
        car = gameManager.carPrefab;

        if (gameManager.LoadGhostCar(groundType, carHolder2, true))
        {
            car2 = gameManager.bestScoresHolder.GetBestCarObject(groundType);
            IsGhostAvailable = true;
        }
        else
        {
            IsGhostAvailable = false;
        }
        ghostT.SetActive(IsGhostAvailable);
    }

    private float curScaleMlpSpeed = 1f;
    
    
    private IEnumerator MoveCarRoutine()
    {
        var curDur = PartEffectController.I.GetDuration(speed);
        elapsed = 0f;
        ARManager.speedText.text = (projSpeed.ToString("F1") + " km/sa");
        lastSpeed = projSpeed;
        while (elapsed <= curDur)
        {
            car.transform.position = Vector3.Lerp(carHolder.position, finishHolder.position, elapsed / curDur);
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime;
            ARManager.timerText.text = (elapsed.ToString("F") + " s");
        }

        elapsed = curDur;
        ARManager.timerText.text = (elapsed.ToString("F") + " s");
        car.transform.position = finishHolder.position;
        
        ARManager.I.Finish();
    }

    private IEnumerator MoveGhostCarRoutine()
    {
        var curDur = PartEffectController.I.GetDuration(ghostSpeed);
        elapsedGhost = 0f;
        // ARManager.speedText.text = (projSpeed.ToString("F1") + " km/sa");
        lastSpeedGhost = ghostProjSpeed;
        while (elapsedGhost <= curDur)
        {
            car2.transform.position = Vector3.Lerp(carHolder2.position, finishHolder2.position, elapsedGhost / curDur);
            yield return new WaitForEndOfFrame();
            elapsedGhost += Time.deltaTime;
            // ARManager.timerText.text = (elapsed.ToString("F") + " s");
        }

        elapsedGhost = curDur;
        // ARManager.timerText.text = (elapsed.ToString("F") + " s");
        car2.transform.position = finishHolder2.position;
        
        // ARManager.I.Finish();
    }

    private Coroutine carMoveRoutine;
    private Coroutine ghostCarMoveRoutine;
    
    public void StartCar()
    {
        speed = PartEffectController.I.GetSpeed(gameManager.lastCarProps, groundType);
        projSpeed = PartEffectController.I.GetProjectedSpeed(speed);
        curScaleMlpSpeed = lastSize / initialSize;

        if (IsGhostAvailable)
        {
            ghostSpeed = gameManager.bestScoresHolder.GetBestSpeed(groundType);
            ghostProjSpeed = PartEffectController.I.GetProjectedSpeed(ghostSpeed);
            if (ghostCarMoveRoutine != null)
            {
                StopCoroutine(ghostCarMoveRoutine);
            }

            ghostCarMoveRoutine = StartCoroutine(MoveGhostCarRoutine());
        }
        
        if (carMoveRoutine != null)
        {
            StopCoroutine(carMoveRoutine);
        }

        carMoveRoutine = StartCoroutine(MoveCarRoutine());

        // isCarMoving = true;
    }

    // public void MoveCarTowardsFinish()
    // {
    //     if (car == null || finishHolder == null)
    //     {
    //         Debug.LogError("Car or FinishHolder is not set.");
    //         return;
    //     }
    //
    //     // Move car towards the finish point
    //     car.transform.position = Vector3.MoveTowards(car.transform.position, finishHolder.position, speed * speedMlp * curScaleMlpSpeed * Time.deltaTime);
    //
    //     // Check if the car has reached the finish point within a tolerance
    //     if (Vector3.Distance(car.transform.position, finishHolder.position) < 0.1f)
    //     {
    //         // Car has reached the finish
    //         isCarMoving = false;
    //     }
    // }
}
