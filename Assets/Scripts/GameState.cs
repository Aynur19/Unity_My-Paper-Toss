using UnityEngine;

public enum GameMode
{
	Easy = 0,
	Medium = 1,
	Hard = 2
}

public static class GameState
{
	public static GameMode GameMode = GameMode.Easy;
}
