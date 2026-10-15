using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("��������� ������ Local Play")]
    public string targetSceneName = "local play";
    public float loadDelay = 0.3f;

    private Button localPlayButton;
    private AudioSource audioSource;

    void Start()
    {
        // ������������� ������ Local Play
        InitializeLocalPlayButton();
    }

    void InitializeLocalPlayButton()
    {
        // �������� ����������
        localPlayButton = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        // ��������� ������� ������
        if (localPlayButton == null)
        {
            Debug.LogError("MainMenu ������ ���� �� ������� � ����������� Button!");
            return;
        }

        // ����������� ����� � ������
        localPlayButton.onClick.AddListener(OnLocalPlayButtonClick);

        Debug.Log($"������ 'lockal play' ��������� ��� ��������: '{targetSceneName}'");
    }

    void OnLocalPlayButtonClick()
    {
        // ������������� ���� ���� ����
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // ��������� ����� � ���������
        Invoke("LoadTargetScene", loadDelay);

        // ��������� ������ ����� �������� ��������� �������
        localPlayButton.interactable = false;
    }

    void LoadTargetScene()
    {
        Debug.Log($"��������� �����: '{targetSceneName}'");

        // ��������� ���������� �� �����
        if (IsSceneInBuildSettings(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"����� '{targetSceneName}' �� ������� � Build Settings!");

            // �������� ������ �������
            if (localPlayButton != null)
                localPlayButton.interactable = true;
        }
    }

    bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    // ����� ��� �������� ��������� ����� �� �������
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ����� ��� ������ �� ����
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}