using TMPro;
using UnityEngine;

public class ConnectionCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private GameObject _connectButton;
    [SerializeField] private TextMeshProUGUI _connectionInfoText;

    private void OnEnable()
    {
        NetworkEvents.s_OnLocalClientConnected += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkEvents.s_OnLocalClientConnected -= OnClientConnected;
    }

    private void OnClientConnected(object sender, NetworkEvents.LocalClientConnectedEventArgs e)
    {
        _connectionPanel.SetActive(false);
        _connectionInfoText.SetText($"IP: {e.IPv4}:{e.Port}");
    }
}
