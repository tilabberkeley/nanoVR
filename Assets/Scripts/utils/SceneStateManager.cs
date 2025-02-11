using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    private string _savePath;

    [SerializeField]
    private GameObject _fileImportButton;
    [SerializeField]
    private GameObject _fileExportButton;

    private FileImport _fileImport;
    private FileExport _fileExport;

    private void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "scene_save.sc");
        _fileImport = _fileImportButton.GetComponent<FileImport>();
        _fileExport = _fileExportButton.GetComponent<FileExport>();

        // Load scene automatically on startup if the file exists
        if (File.Exists(_savePath))
        {
            LoadScene();
        }
    }

    public void SaveScene()
    {
        string jsonData = SerializeScene();
        File.WriteAllText(_savePath, jsonData);
        Debug.Log("Scene saved.");
    }

    public void LoadScene()
    {
        if (File.Exists(_savePath))
        {
            // Restart scene first.
            RestartScene();

            DeserializeScene(_savePath);
        }
        else
        {
            Debug.LogWarning("No saved scene found.");
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string SerializeScene()
    {
        string scjson = _fileExport.GetSCJSON();
        return scjson;
    }

    private void DeserializeScene(string filePath)
    {
        _fileImport.LoadFile(filePath);
    }
}
