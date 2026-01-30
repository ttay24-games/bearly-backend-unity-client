#nullable enable
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cysharp.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using TG24.BearlyBackend.Grpc;
using UnityEngine;

public class TestHttp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        try
        {
            await RunTest();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Awaitable RunTest()
    {
        using var handler = new YetAnotherHttpHandler()
        {
            Http2Only = true,
        };
        var httpClient = new HttpClient(handler);

        //var result = await httpClient.GetStringAsync("https://www.example.com");
        //Debug.Log(result);

        using var channel = GrpcChannel.ForAddress("http://localhost:5282", new GrpcChannelOptions { HttpHandler = handler });
        var authClient = new Auth.AuthClient(channel);
        var greeterClient = new GreeterTest.GreeterTestClient(channel);
        var lobbyClient = new LobbyService.LobbyServiceClient(channel);

        await CreateLobby(lobbyClient, null);
    }

    private async Awaitable CreateLobby(LobbyService.LobbyServiceClient client, string? jwt)
    {
        try
        {
            Debug.Log("Calling create lobby service...");
            Metadata? metadata = null;
            if (!string.IsNullOrEmpty(jwt))
            {
                metadata = new Metadata { { "Authorization", $"Bearer {jwt}" } };
            }

            var callOptions = new CallOptions(metadata);

            var resp = await client.CreateLobbyAsync(new CreateLobbyRequest
            {
                Name = "test lobby from client",
                MaxPlayers = 8,
            }, callOptions);
            Debug.Log($"Created lobby: {resp.Lobby.Id} with {resp.Lobby.Name}");
        }
        catch (RpcException e)
        {
            Debug.LogError($"create lobby service call failed: {e.Status}");
        }
    }
}
