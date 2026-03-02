using UnityEngine;
using UnityEngine.Events;

public class SlidingDoor : MonoBehaviour
{
    public UnityEvent SubePorton;
    [SerializeField] private float preWarmtime;
    [SerializeField] private float slidingTime;
    [SerializeField] private float yDistance;

    private Vector3 startingPos;
    private Vector3 endPos;
    private bool _isTimerRunning;
    private bool _isWarming;

    private float currentSign = 1f;
    private float _currentTimer;

    private float _currentYValue;

    void Start()
    {
        currentSign = 1f;
        _currentTimer = 0;
        startingPos = transform.position;
        endPos = transform.position + Vector3.up * yDistance;
    }


    void Update()
    {
        if (_isWarming)
        {
            _currentTimer += Time.deltaTime;

            if (_currentTimer > preWarmtime)
            {
                currentSign *= -1f;

                if (currentSign > 0)
                {
                    _currentTimer = 0;
                }
                else if (currentSign < 0)
                {
                    _currentTimer = slidingTime;
                }
                
                _isWarming = false;
                SubePorton?.Invoke();
            }
        }
        else
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
    }

    public void Slide()
    {
        _isTimerRunning = true;
        _isWarming = true;

        _currentTimer = 0;
    }
}