using UnityEngine;

public class ConnectionCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _connectionPanel;

    private void OnEnable()
    {
        NetworkManager.s_OnClientConnected += HidePanel;
    }

    private void OnDisable()
    {
        NetworkManager.s_OnClientConnected -= HidePanel;
    }

    private void HidePanel()
    {
        _connectionPanel.SetActive(false);
    }
}
