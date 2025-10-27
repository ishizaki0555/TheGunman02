// LoadScene.cs
//
// シーン遷移をAdditiveモードで行う
//

using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // PlayerPrefsに保存されているシーン名のシーンをAdditiveモードで読み込む
    void Start()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("SelectedScene"), LoadSceneMode.Additive);
    }
}
