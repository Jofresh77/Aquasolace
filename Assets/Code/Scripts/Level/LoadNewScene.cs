using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Scripts
{
    public class LoadNewScene : MonoBehaviour
    {
        [SerializeField] GameObject loadingBar;
        
        private void Start()
        {
            StartCoroutine(LoadAsynchronously());
        }
        
        private IEnumerator LoadAsynchronously()
        {
            yield return new WaitForSeconds(0.35f);
            AsyncOperation operation = SceneManager.LoadSceneAsync(PlayerPrefs.GetString("Scene to go to"));

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                
                var localScale = loadingBar.transform.localScale;
                loadingBar.transform.localScale = new Vector3(progress, localScale.y, localScale.z);
                
                yield return null;
            }
        }
    }
}
