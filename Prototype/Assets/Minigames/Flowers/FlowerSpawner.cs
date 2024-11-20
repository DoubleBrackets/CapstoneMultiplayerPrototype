using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class FlowerSpawner : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> _flowerPrefab;

    [SerializeField]
    private NetworkProtag _networkProtag;

    [SerializeField]
    private Transform _spawnSource;

    public override void OnStartClient()
    {
        Debug.Log("flower spawner start");
        _networkProtag.OnJumpPerformed += OnFlowerJump;
    }

    public override void OnStopClient()
    {
        _networkProtag.OnJumpPerformed -= OnFlowerJump;
    }

    private void OnFlowerJump()
    {
        Debug.Log("flower jump");
        if (!IsOwner)
        {
            return;
        }

        int flowerId = LocalConnection.ClientId % _flowerPrefab.Count;
        BadLogger.LogTrace($"Spawning flower with id {flowerId}");
        float horizontalOffset = Random.Range(-0.4f, 0.4f);
        Vector3 pos = _spawnSource.position + Vector3.right * horizontalOffset;

        var tint = new Color(
            Random.Range(0.95f, 1f),
            Random.Range(0.95f, 1f),
            Random.Range(0.95f, 1f));

        float scaleAmt = Random.Range(0.8f, 1.2f);
        var scale = new Vector3(
            scaleAmt,
            scaleAmt,
            1f);

        RPC_SpawnFlower(pos, tint, scale, flowerId);

        GameObject flower =
            Instantiate(_flowerPrefab[flowerId],
                pos,
                Quaternion.identity);
        flower.transform.localScale = scale;
        flower.GetComponent<SpriteRenderer>().color = tint;
    }

    [ServerRpc]
    private void RPC_SpawnFlower(
        Vector3 pos,
        Color tint,
        Vector3 scale,
        int flowerTypeIndex,
        NetworkConnection conn = null)
    {
        BadLogger.LogDebug("Server RPC spawn flower");
        RPC_SpawnFlowerClient(pos, tint, scale, flowerTypeIndex, conn.ClientId);
    }

    [ObserversRpc]
    private void RPC_SpawnFlowerClient(
        Vector3 pos,
        Color tint,
        Vector3 scale,
        int flowerTypeIndex,
        int conId)
    {
        if (conId == LocalConnection.ClientId)
        {
            return;
        }

        BadLogger.LogDebug($"Client RPC spawn flower from {conId}");

        GameObject flower =
            Instantiate(_flowerPrefab[flowerTypeIndex],
                pos,
                Quaternion.identity);
        flower.transform.localScale = scale;
        flower.GetComponent<SpriteRenderer>().color = tint;
    }
}