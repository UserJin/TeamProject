using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager instance = null;

    private ScoreManager scoreManager;

    public float slowTime = 0.1f;
    private bool _isSlowMode;

    public float playTime;

    public GameObject _gameoverPannel;
    public GameObject _gameClearPannel;
    
    public enum State
    {
        RUN,
        GAMEOVER,
        GAMECLEAR
    }

    public State state;

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
        playTime = 0.0f;
        state = State.RUN;
        _gameClearPannel.SetActive(false);
        _gameoverPannel.SetActive(false);
        scoreManager = new ScoreManager();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.RUN)
        {
            playTime += Time.unscaledDeltaTime;
        }
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

    // 슬로우 모드 여부 반환
    public bool isSlowMode()
    {
        return _isSlowMode;
    }

    // 플레이어가 사망할 경우 발동
    void OnPlayerDie()
    {
        state = State.GAMEOVER;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _gameoverPannel.SetActive(true);
    }

    void OnPlayerClear()
    {
        state = State.GAMECLEAR;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _gameClearPannel.SetActive(true);
        _gameClearPannel.transform.Find("Score").GetComponent<TMP_Text>().text = $"Score: {scoreManager.Score}";
        _gameClearPannel.transform.Find("Rank").GetComponent<TMP_Text>().text = $"Rank: {scoreManager.CheckRank()}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        DisableSlowMode();
        _gameClearPannel.SetActive(false);
        _gameoverPannel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        state = State.RUN;
        playTime = 0.0f;
    }
}
