using System.Collections.Generic;
using System.Linq;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
#if UNITY_SERVER
using Unity.Services.Multiplay;
#endif

public class DedicatedServer : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    
#if UNITY_SERVER
    private const float BackfillTickTimerMax = 1.1f;

    private List<int> _connectedClients = new();
    private IServerQueryHandler _serverQueryHandler;
    private MatchmakingResults _matchmakingResults;
    private string _backfillTicketId;
    private float _backfillTickTimer;
    
    public int ClientsCount => _connectedClients.Count;
    public bool HasAvailableSlots => ClientsCount < 5;
    public List<int> ConnectedClients => _connectedClients;

    private void OnEnable()
    {
        NetworkEvents.s_OnClientConnected += AddClient;
        NetworkEvents.s_OnClientDisconnected += RemoveClient;
        NetworkEvents.s_OnClientConnected += UpdateBackfillTicket;
        NetworkEvents.s_OnClientDisconnected += UpdateBackfillTicket;
    }

    private void OnDisable()
    {
        NetworkEvents.s_OnClientConnected -= AddClient;
        NetworkEvents.s_OnClientDisconnected -= RemoveClient;
        NetworkEvents.s_OnClientConnected -= UpdateBackfillTicket;
        NetworkEvents.s_OnClientDisconnected -= UpdateBackfillTicket;
    }

    private void AddClient(int clientId)
    {
        _connectedClients.Add(clientId);
    }

    private void RemoveClient(int clientId)
    {
        _connectedClients.Remove(clientId);
    }

    private async void Start()
    {
        MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
        multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
        multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
        multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
        multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
        IServerEvents serverEvent = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

        _serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(5, $"Sandbox_{UnityEngine.Random.Range(0, 10000)}", "Casual", "v1", "Sandbox");

        var serverConfig = MultiplayService.Instance.ServerConfig;
        if (serverConfig.AllocationId != "")
            MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
    }

    private void Update()
    {
        if (_serverQueryHandler != null)
        {
            _serverQueryHandler.CurrentPlayers = (ushort)ClientsCount;
            _serverQueryHandler.UpdateServerCheck();
        }

        if (_backfillTicketId != "")
        {
            _backfillTickTimer -= Time.deltaTime;
            if (_backfillTickTimer <= 0)
            {
                _backfillTickTimer = BackfillTickTimerMax;

                ApproveBackfillTicket();
            }
        }
    }

    private void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj)
    {
        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"Allocation ID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"Query Port[{serverConfig.QueryPort}]");
        Debug.Log($"Log Directory[{serverConfig.ServerLogDirectory}]");

        SetupBackfillTickets();
    }
    private async void SetupBackfillTickets()
    {
        _matchmakingResults = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();

        _backfillTicketId = _matchmakingResults.BackfillTicketId;
        _backfillTickTimer = BackfillTickTimerMax;
    }

    private async void ApproveBackfillTicket()
    {
        if (!HasAvailableSlots) return;

        BackfillTicket ticket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_backfillTicketId);
        _backfillTicketId = ticket.Id;
    }

    private async void UpdateBackfillTicket(int _)
    {
        if (!HasAvailableSlots ||
            _matchmakingResults == null ||
            _backfillTicketId == "")
            return;

        List<Unity.Services.Matchmaker.Models.Player> playerList = new();

        foreach (int clientId in ConnectedClients)
            playerList.Add(new Unity.Services.Matchmaker.Models.Player(clientId.ToString()));

        Team teamToCopy = _matchmakingResults.MatchProperties.Teams[0];

        List<Team> teams = new()
        {
            new Team(
            teamToCopy.TeamName,
            teamToCopy.TeamId,
            playerList.Select(p => p.Id).ToList())
        };

        MatchProperties properties = new MatchProperties(
            teams,
            playerList,
            _matchmakingResults.MatchProperties.Region,
            _backfillTicketId
        );

        try
        {
            await MatchmakerService.Instance.UpdateBackfillTicketAsync(_backfillTicketId,
                new BackfillTicket(_backfillTicketId, properties: new BackfillTicketProperties(properties))
            );
        }
        catch (MatchmakerServiceException e)
        {
            Debug.Log($"Error during backfill ticket update: {e}");
        }
    }

    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {

    }

    private void MultiplayEventCallbacks_Error(MultiplayError obj)
    {

    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj)
    {

    }
#endif
}
