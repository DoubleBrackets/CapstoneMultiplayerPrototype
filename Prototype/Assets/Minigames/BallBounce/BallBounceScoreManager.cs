using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

namespace Minigames.BallBounce
{
    public class BallBounceScoreManager : NetworkBehaviour
    {
        [SerializeField]
        private TMP_Text _scoreText;

        [SerializeField]
        private TMP_Text _maxScoreText;

        public static BallBounceScoreManager Instance { get; private set; }

        [SerializeField]
        private readonly SyncStopwatch _timeInAir = new();

        private float _maxScore;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            _timeInAir.Update(Time.deltaTime);

            _scoreText.text = $"{_timeInAir.Elapsed:0.00}";
        }

        public override void OnStartServer()
        {
            _timeInAir.StartStopwatch();
        }

        public override void OnStartClient()
        {
            _timeInAir.OnChange += TimeInAirOnChange;
        }

        private void TimeInAirOnChange(SyncStopwatchOperation op, float next1, bool asserver)
        {
        }

        [Server]
        public void ResetScore()
        {
            if (_timeInAir.Elapsed > _maxScore)
            {
                _maxScore = _timeInAir.Elapsed;
                ObserversRpc_UpdateHighScore(_maxScore);
            }

            _timeInAir.StartStopwatch();
        }

        [ObserversRpc(RunLocally = true)]
        private void ObserversRpc_UpdateHighScore(float newScore)
        {
            _maxScore = newScore;
            _maxScoreText.text = "HighScore: " + $"{newScore:0.00}";
        }
    }
}