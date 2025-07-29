using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchFinder : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TextMeshProUGUI _connectionStateText;
    [SerializeField] private GameObject _cancelButton;
    [SerializeField] private GameObject _findMatchButton;

    private const string DefaultQueue = "Casual";
    private const float PollTicketTimerMax = 1.1f;

    private CreateTicketResponse _createTicketResponse;
    private float _pollTicketTimer;


    private void Update()
    {
        if (_createTicketResponse != null)
        {
            _pollTicketTimer -= Time.deltaTime;
            if (_pollTicketTimer <= 0f)
            {
                _pollTicketTimer = PollTicketTimerMax;

                PollMatchmakerTicker();
            }
        }
    }

    public async void FindMatch()
    {
        if (_createTicketResponse != null) return;

        _createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Unity.Services.Matchmaker.Models.Player>
        {
            new Unity.Services.Matchmaker.Models.Player(AuthenticationService.Instance.PlayerId)
        }, new CreateTicketOptions { QueueName = DefaultQueue });

        _connectionStateText.SetText("Created ticket");
        _cancelButton.SetActive(true);
        _pollTicketTimer = PollTicketTimerMax;
    }

    private async void PollMatchmakerTicker()
    {
        TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(_createTicketResponse.Id);

        if (ticketStatusResponse == null)
        {
            _connectionStateText.SetText("Waiting for response...");
            return;
        }

        if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
        {
            MultiplayAssignment multiplayAssignment = ticketStatusResponse.Value as MultiplayAssignment;

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    CancelFinder();
                    _connectionStateText.SetText("Connecting...");
                    _networkManager.SetIPAddress(multiplayAssignment.Ip);
                    _networkManager.SetPort(multiplayAssignment.Port.ToString());
                    _networkManager.ConnectWithInternal();
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    _connectionStateText.SetText($"Waiting for other players...");
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    CancelFinder();
                    _connectionStateText.SetText("Failed");
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    CancelFinder();
                    _connectionStateText.SetText("Timeout");
                    break;
            }
        }
    }

    public async void CancelFinder()
    {
        if (_createTicketResponse != null)
        {
            await MatchmakerService.Instance.DeleteTicketAsync(_createTicketResponse.Id);
            _createTicketResponse = null;
            _cancelButton.SetActive(false);
        }
    }

    private async void OnDisable()
    {
        await Task.Run(delegate { CancelFinder(); });
    }
}
