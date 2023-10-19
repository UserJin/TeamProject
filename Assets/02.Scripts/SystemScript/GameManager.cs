using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager instance = null;

    public event EventHandler OnGamePause;

    // 점수 관련 매니저
    public ScoreManager scoreManager;

    public TMP_Text cur_score;
    public TMP_Text cur_combo;
    public Image crosshair;

    // 슬로우모드 느려짐 배율
    public float slowTime;
    private bool _isSlowMode;

    // 현재까지 소모된 시간
    public float playTime;

    // 게임 클리어 및 패배 시 표시하는 UI 오브젝트
    public GameObject _gameoverPannel;
    public GameObject _gameClearPannel;

    public AudioSource audioSource;
    
    public enum State
    {
        RUN,
        GAMEOVER,
        GAMECLEAR
    }

    // 게임매니저의 현재 상태
    public State state;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGame();
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.RUN)
        {
            playTime += Time.unscaledDeltaTime;
            SetScore();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); // esc누르면 종료
    }

    // 변수 초기화 함수
    void InitGame()
    {
        // 커서 비활성화 및 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        DisableSlowMode();
        playTime = 0.0f;
        state = State.RUN;
        _gameClearPannel.SetActive(false);
        _gameoverPannel.SetActive(false);
        scoreManager = new ScoreManager();
        SetUI(true);
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

    // 플레이어가 사망할 경우 실행
    void OnPlayerDie()
    {
        state = State.GAMEOVER;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnGamePause?.Invoke(this, EventArgs.Empty);
        SetUI(false);
        _gameoverPannel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    // 플레이어가 게임을 클리어하면 실행
    void OnPlayerClear()
    {
        state = State.GAMECLEAR;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ComboManager.instance.StopCombo();
        OnGamePause?.Invoke(this, EventArgs.Empty);
        _gameClearPannel.SetActive(true);
        _gameClearPannel.transform.Find("Score").GetComponent<TMP_Text>().text = $"Score: {scoreManager.Score}";
        _gameClearPannel.transform.Find("MaxCombo").GetComponent<TMP_Text>().text = $"Max combo: {ComboManager.instance.maxCombo}";
        _gameClearPannel.transform.Find("Rank").GetComponent<TMP_Text>().text = $"Rank: {scoreManager.CheckRank()}";
        SetUI(false);
        Time.timeScale = 0.0f;
    }

    // 플레이어가 재시작 버튼을 누르면 실행
    public void RestartGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        InitGame();
    }

    void SetScore()
    {
        cur_score.text = $"Score: {scoreManager.Score:00000}";
    }

    // UI 활성화 및 비활성화 함수
    void SetUI(bool b)
    {
        cur_combo.gameObject.SetActive(b);
        cur_score.gameObject.SetActive(b);
        crosshair.gameObject.SetActive(b);
    }
}
