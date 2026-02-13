using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneQuickOpen
{
    [MenuItem("Scene/Title Scene")]
    static void OpenTitleScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Title.unity");
    }

    [MenuItem("Scene/Loading Scene")]
    static void OpenLoadingScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity");
    }

    [MenuItem("Scene/Game Scene")]
    static void OpenGameScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
    }
}