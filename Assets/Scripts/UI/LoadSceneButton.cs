using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField]
        private string SceneToLoad;

        public void LoadTargetScene()
        {
            SceneManager.LoadSceneAsync(SceneToLoad);
        }
    }
}