using TMPro;
using UnityEngine;

public class ConnectionCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private TextMeshProUGUI _connectionInfoText;

    private void OnEnable()
    {
        NetworkEvents.s_OnClientConnectedLocal += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkEvents.s_OnClientConnectedLocal -= OnClientConnected;
    }

    private void OnClientConnected(object sender, ClientConnectedLocalEventArgs e)
    {
        _connectionPanel.SetActive(false);
        _connectionInfoText.SetText($"IP: {e.IPv4}:{e.Port}");
    }
}
