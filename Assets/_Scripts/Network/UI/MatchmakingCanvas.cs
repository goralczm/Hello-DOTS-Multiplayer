using TMPro;
using UnityEngine;

public class MatchmakingCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _matchStatusText;
    [SerializeField] private GameObject _cancelMatchmakingButton;
    [SerializeField] private GameObject _startMatchmakingButton;

    private void OnEnable()
    {
        MatchmakingHandler.s_OnMatchRequested += OnMatchRequested;
        MatchmakingHandler.s_OnMatchStatusChanged += UpdateMatchStatusText;
        MatchmakingHandler.s_OnMatchRequestEnded += OnMatchRequestEnded;
    }

    private void OnDisable()
    {
        MatchmakingHandler.s_OnMatchRequested -= OnMatchRequested;
        MatchmakingHandler.s_OnMatchStatusChanged -= UpdateMatchStatusText;
        MatchmakingHandler.s_OnMatchRequestEnded -= OnMatchRequestEnded;
    }

    private void OnMatchRequested()
    {
        _startMatchmakingButton.SetActive(false);
        _cancelMatchmakingButton.SetActive(true);
    }

    private void UpdateMatchStatusText(string status)
    {
        _matchStatusText.SetText(status);
    }

    private void OnMatchRequestEnded(bool success)
    {
        _startMatchmakingButton.SetActive(true);
        _cancelMatchmakingButton.SetActive(false);
    }
}
