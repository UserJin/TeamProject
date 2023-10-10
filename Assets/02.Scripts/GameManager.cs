using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager instance = null;

    public float slowTime = 0.1f;
    private bool _isSlowMode;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        _isSlowMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 슬로우 모드 활성화
    public void EnableSlowMode()
    {
        _isSlowMode = true;
        Time.timeScale = slowTime;
        Time.fixedDeltaTime = slowTime * 0.02f;
    }

    // 슬로우 모드 비활성화
    public void DisableSlowMode()
    {
        _isSlowMode = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    public bool isSlowMode()
    {
        return _isSlowMode;
    }
}
