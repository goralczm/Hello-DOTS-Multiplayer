using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchmakingHandler : MonoBehaviour
{
    public static event Action s_OnMatchRequested;
    public static event Action<string> s_OnMatchStatusChanged;
    public static event Action<bool> s_OnMatchRequestEnded;
    
    [Header("References")]
    [SerializeField] private NetworkManager _networkManager;

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

    public async void StartMatchmaking()
    {
        if (_createTicketResponse != null) return;

        _createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Player>
        {
            new (AuthenticationService.Instance.PlayerId)
        }, new CreateTicketOptions { QueueName = DefaultQueue });
        
        s_OnMatchRequested?.Invoke();
        s_OnMatchStatusChanged?.Invoke("Starting matchmaking...");
        
        _pollTicketTimer = PollTicketTimerMax;
    }

    private async void PollMatchmakerTicker()
    {
        TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(_createTicketResponse.Id);

        if (ticketStatusResponse == null)
        {
            s_OnMatchStatusChanged?.Invoke("Waiting for response...");
            return;
        }

        if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
        {
            MultiplayAssignment multiplayAssignment = ticketStatusResponse.Value as MultiplayAssignment;

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    CancelMatchFinding();
                    s_OnMatchStatusChanged?.Invoke("Connecting...");
                    _networkManager.SetIPAddress(multiplayAssignment.Ip);
                    _networkManager.SetPort(multiplayAssignment.Port.ToString());
                    _networkManager.ConnectWithInternal();
                    s_OnMatchRequestEnded?.Invoke(true);
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    s_OnMatchStatusChanged?.Invoke("Waiting for other players...");
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    CancelMatchFinding();
                    s_OnMatchStatusChanged?.Invoke("Failed to find matching servers");
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    CancelMatchFinding();
                    s_OnMatchStatusChanged?.Invoke("Connection timeout");
                    break;
            }
        }
    }

    public async void CancelMatchFinding()
    {
        if (_createTicketResponse != null)
        {
            s_OnMatchStatusChanged?.Invoke("Matchmaking canceled");
            s_OnMatchRequestEnded?.Invoke(false);
            await MatchmakerService.Instance.DeleteTicketAsync(_createTicketResponse.Id);
            _createTicketResponse = null;
        }
    }

    private async void OnDisable()
    {
        await Task.Run(delegate { CancelMatchFinding(); });
    }
}
