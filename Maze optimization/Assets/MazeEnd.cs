using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
public class MazeEnd : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            
        }
    }
}
