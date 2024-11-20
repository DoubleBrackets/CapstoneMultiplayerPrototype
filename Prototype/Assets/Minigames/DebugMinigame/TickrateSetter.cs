using FishNet.Object;
using TMPro;
using UnityEngine;

namespace Minigames.DebugMinigame
{
    public class TickrateSetter : NetworkBehaviour
    {
        [SerializeField]
        private TMP_InputField _inputField;

        private void Awake()
        {
            _inputField.text = "...";
            _inputField.interactable = false;
        }

        public override void OnStartServer()
        {
            _inputField.text = "1";
            _inputField.interactable = true;
            _inputField.onSubmit.AddListener(OnSubmit);

            UpdateTickrate(_inputField.text);
        }

        public override void OnStopServer()
        {
            _inputField.onSubmit.RemoveListener(OnSubmit);
        }

        private void OnSubmit(string value)
        {
            if (!ServerManager.Started)
            {
                return;
            }

            UpdateTickrate(value);
        }

        [Server]
        private void UpdateTickrate(string value)
        {
            if (!ushort.TryParse(value, out ushort tickrate))
            {
                _inputField.text = TimeManager.TickRate.ToString();
                return;
            }

            if (tickrate < 1)
            {
                _inputField.text = TimeManager.TickRate.ToString();
                return;
            }

            TimeManager.SetTickRate(tickrate);
            _inputField.text = tickrate.ToString();

            ObserversRpc_SetTickrate(tickrate);
        }

        [ObserversRpc(BufferLast = true)]
        private void ObserversRpc_SetTickrate(ushort tickrate)
        {
            TimeManager.SetTickRate(tickrate);
            _inputField.text = tickrate.ToString();
        }
    }
}