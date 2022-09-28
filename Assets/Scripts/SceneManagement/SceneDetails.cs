using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;

    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            // play music of the scene
            if (sceneMusic != null)
                AudioManager.Instance.PlayMusic(sceneMusic, fade: true);

            // Load all connected scenes
            foreach(var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            // Unload the scenes that are no longer connected
            var previousScene = GameController.Instance.PreviousScene;
            if (previousScene != null)
            {
                var previouslyLoadedScenes = previousScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)   
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();
                }
                if (!connectedScenes.Contains(previousScene))
                    previousScene.UnloadScene();
            }
        }   
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            Debug.Log("Have loaded " + gameObject.name);
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }        
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    private List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currentScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currentScene).ToList();
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
