using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private float slidingTime;
    [SerializeField] private float yDistance;

    private Vector3 startingPos;
    private Vector3 endPos;
    private bool _isTimerRunning;

    private float currentSign = 1f;
    private float _currentTimer;

    private float _currentYValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPos = transform.position;
        endPos = transform.position - Vector3.up * yDistance;
        _currentTimer = slidingTime;
    }


    // Update is called once per frame
    void Update()
    {
        if (_isTimerRunning)
        {
            _currentTimer += Time.deltaTime * currentSign;

            float t = _currentTimer / slidingTime;
            Vector3 nowPos = Vector3.Lerp(endPos, startingPos, t);
            transform.position = nowPos;

            if (_currentTimer / slidingTime >= 1)
            {
                _isTimerRunning = false;
            }
            else if (_currentTimer / slidingTime <= 0)
            {
                _isTimerRunning = false;
            }
        }
    }

    public void Slide()
    {
        _isTimerRunning = true;
        currentSign *= -1f;

        // if (currentSign > 0)
        // {
        //     _currentTimer = 0;
        // }
        // else
        // {
        //     _currentTimer = slidingTime;
        // }
    }
}