using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

public class LocalSceneLoader : MonoBehaviour
{
    public string sceneAddress = "SampleScene"; // 登録したアドレス名

    public void LoadLocalScene()
    {
        Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Single).Completed += OnSceneLoaded;
    }

    // private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
    // {
    //     if (handle.Status == AsyncOperationStatus.Succeeded)
    //     {
    //         Debug.Log("ローカルシーンのロード成功！");
    //     }
    //     else
    //     {
    //         Debug.LogError("シーンのロード失敗！");
    //     }
    // }
    void OnSceneLoaded(AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($" Scene loaded successfully: {handle.Result.Scene.name}");
        }
        else
        {
            Debug.LogError($" Failed to load scene: {handle.OperationException?.Message}");
        }
    }

    public void OnClickLoadSceneButton()
    {
        LoadLocalScene();
    }
}
