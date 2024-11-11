using System.Collections.Generic;
using GameKit.Dependencies.Utilities.Types;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameList")]
public class MinigameListSO : ScriptableObject
{
    [SerializeField]
    [Scene]
    private List<string> _minigameScenes;

    [SerializeField]
    [Scene]
    private string _startScene;

    [SerializeField]
    [Scene]
    private string _mainMenuScene;

    public IReadOnlyList<string> MinigameScenes => _minigameScenes;

    public string StartScene => _startScene;

    public string MainMenuScene => _mainMenuScene;
}