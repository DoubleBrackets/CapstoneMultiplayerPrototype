using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
///     Handles initializing unity services
/// </summary>
public class UnityServiceManager : MonoBehaviour
{
    public delegate void UnityServicesStateChangedHandler(
        ServicesInitializationState previousState,
        ServicesInitializationState newState);

    public static List<UnityServiceManager> Instances { get; set; } = new();

    public event UnityServicesStateChangedHandler UnityServicesStateChanged;

    private bool _isTryingToInitialize;
    private ServicesInitializationState _unityServicesState;

    protected void Awake()
    {
        Instances.Add(this);
        TryInitializeUnityServices(gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private void Update()
    {
        ServicesInitializationState newState = UnityServices.State;
        if (newState != _unityServicesState)
        {
            UnityServicesStateChanged?.Invoke(_unityServicesState, newState);
            BadLogger.LogTrace($"Unity Services state changed from {_unityServicesState} to {newState}");
        }

        _unityServicesState = UnityServices.State;
    }

    protected void OnDestroy()
    {
        Instances.Remove(this);
    }

    /// <summary>
    ///     Block until Unity Services are initialized. Signs in anonymously if not already signed in.
    /// </summary>
    /// <param name="token"></param>
    public async UniTask WaitForInitialization(CancellationToken token)
    {
        while (!AuthenticationService.Instance.IsSignedIn ||
               UnityServices.State != ServicesInitializationState.Initialized)
        {
            if (!_isTryingToInitialize)
            {
                TryInitializeUnityServices(token).Forget();
            }

            await UniTask.DelayFrame(1, cancellationToken: token);
        }
    }

    /// <summary>
    ///     Try to initialize and sign in anonymously to Unity Services.
    /// </summary>
    /// <param name="token"></param>
    private async UniTask TryInitializeUnityServices(CancellationToken token)
    {
        if (_isTryingToInitialize)
        {
            return;
        }

        _isTryingToInitialize = true;
        try
        {
            if (UnityServices.State != ServicesInitializationState.Uninitialized)
            {
                return;
            }

            BadLogger.LogTrace("Initializing Unity Services");

            await UnityServices.InitializeAsync();

            token.ThrowIfCancellationRequested();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                BadLogger.LogTrace("Signing in anonymously");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                BadLogger.LogTrace(
                    $"Signed in anonymously successfully with id {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        finally
        {
            _isTryingToInitialize = false;
        }
    }
}