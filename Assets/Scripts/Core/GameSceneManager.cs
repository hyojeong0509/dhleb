using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 씬 전환을 관리하는 싱글톤 매니저
public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager _instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSceneManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSceneManager");
                    _instance = go.AddComponent<GameSceneManager>();
                }
            }
            return _instance;
        }
    }

    [Header("씬 이름 설정")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string loadingSceneName = "Loading";
    [SerializeField] private string gameSceneName = "Game";

    [Header("로딩 설정")]
    [Tooltip("로딩 씬을 사용할지 여부 (false면 바로 전환)")]
    [SerializeField] private bool useLoadingScene = true;
    
    [Tooltip("최소 로딩 시간 (초) - 너무 빠르게 전환되는 것을 방지")]
    [SerializeField] private float minLoadingTime = 2.5f;

    private string targetSceneName; // 로딩 후 이동할 씬 이름
    private bool isLoading = false; // 현재 로딩 중인지
    private AsyncOperation currentAsyncOperation; // 현재 비동기 로딩 작업

    void Awake()
    {
        // 싱글톤 패턴
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Title 씬으로 이동
    public void LoadTitleScene()
    {
        LoadScene(titleSceneName);
    }

    // Game 씬으로 이동
    public void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }

    // 특정 씬으로 이동 (로딩 씬 사용)
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"SceneManager: 이미 로딩 중입니다. 씬 전환 요청 무시: {sceneName}");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneManager: 씬 이름이 비어있습니다.");
            return;
        }

        targetSceneName = sceneName;

        if (useLoadingScene)
        {
            // 로딩 씬을 거쳐서 이동
            StartCoroutine(LoadSceneWithLoading());
        }
        else
        {
            // 바로 씬 전환
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    // 로딩 씬을 거쳐서 씬 전환 (비동기 로딩)
    private IEnumerator LoadSceneWithLoading()
    {
        isLoading = true;

        // 1. 로딩 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(loadingSceneName);
        yield return null; // 한 프레임 대기 (로딩 씬이 완전히 로드될 때까지)

        // 2. 실제 게임 씬을 비동기로 로드
        currentAsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetSceneName);
        currentAsyncOperation.allowSceneActivation = false; // 자동 전환 방지

        float loadingStartTime = Time.time;
        float loadingProgress = 0f;

        // 3. 로딩 진행률 체크 및 최소 로딩 시간 대기
        while (!currentAsyncOperation.isDone)
        {
            loadingProgress = currentAsyncOperation.progress;

            // 최소 로딩 시간이 지났고, 로딩이 90% 이상 완료되면 전환 허용
            // (Unity는 90%까지 로드하고 마지막 10%는 allowSceneActivation이 true일 때 완료)
            if (Time.time - loadingStartTime >= minLoadingTime && loadingProgress >= 0.9f)
            {
                currentAsyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        currentAsyncOperation = null;
        isLoading = false;
    }

    // 현재 로딩 진행률 가져오기 (0~1)
    public float GetLoadingProgress()
    {
        if (currentAsyncOperation != null)
        {
            // Unity는 90%까지 로드하고, 나머지 10%는 allowSceneActivation이 true일 때 완료
            // 따라서 0.9f를 곱해서 더 정확한 진행률 표시
            return Mathf.Clamp01(currentAsyncOperation.progress / 0.9f);
        }
        return 0f;
    }

    // 현재 로딩 중인지 확인
    public bool IsLoading()
    {
        return isLoading;
    }

    // 로딩 씬 사용 여부 설정
    public void SetUseLoadingScene(bool use)
    {
        useLoadingScene = use;
    }
}

