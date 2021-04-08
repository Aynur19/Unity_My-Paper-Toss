using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
	[SerializeField]
	private Button btnStartGameAR;

	[SerializeField]
	private Button btnStartGame;

	[SerializeField]
	private Button btnExitGame;

	[SerializeField]
	private List<Toggle> gameModeGroup;

	[SerializeField]
	public Animator animGameMode;

	private void Awake()
	{
		LoadPlayerPrefs();
	}

	private void Start()
	{
		HideGameMode();
		GameModeCheck();

		btnExitGame.onClick.AddListener(() => Application.Quit());
		btnStartGame.onClick.AddListener(() => ToLoadScene(1));
		btnStartGameAR.onClick.AddListener(() => ToLoadScene(2));

		for(int i = 0; i < gameModeGroup.Count; i++)
		{
			int buf = i;
			gameModeGroup[buf].onValueChanged.AddListener((x) =>
			{
				OnSelectMode(buf, x);
			});

		}
	}

	private void GameModeCheck()
	{
		switch(GameState.GameMode)
		{
			case GameMode.Easy:
				gameModeGroup[(int)GameMode.Easy].isOn = true;
				break;
			case GameMode.Medium:
				gameModeGroup[(int)GameMode.Medium].isOn = true;
				break;
			case GameMode.Hard:
				gameModeGroup[(int)GameMode.Hard].isOn = true;
				break;
			default:
				gameModeGroup[(int)GameMode.Easy].isOn = true;
				break;
		}
	}

	private void ToLoadScene(int sceneNumber)
	{
		SavePlayerPrefs();
		SceneManager.LoadSceneAsync(sceneNumber);
		//SceneManager.LoadSceneAsync(2);
	}

	private void OnSelectMode(int gameMode, bool isSelect)
	{
		if(isSelect)
		{
			switch(gameMode)
			{
				case (int)GameMode.Easy:
					GameState.GameMode = GameMode.Easy;
					break;
				case (int)GameMode.Medium:
					GameState.GameMode = GameMode.Medium;
					break;
				case (int)GameMode.Hard:
					GameState.GameMode = GameMode.Hard;
					break;
				default:
					GameState.GameMode = GameMode.Easy;
					break;
			}
		}
		else if(!ModeIsSelected())
		{
			if(gameMode == (int)GameMode.Easy)
			{
				GameState.GameMode = GameMode.Medium;
				gameModeGroup[(int)GameMode.Medium].isOn = true;
			}
			else
			{
				GameState.GameMode = GameMode.Easy;
				gameModeGroup[(int)GameMode.Easy].isOn = true;
			}
		}

		LoggingGameMode();
	}

	private bool ModeIsSelected()
	{
		var result = false;
		gameModeGroup.ForEach(mode =>
		{
			if(mode.isOn)
			{
				result = true;
				return;
			}
		});

		return result;
	}

	public void ShowGameMode()
	{
		animGameMode.SetBool("IsHidden", false);
	}

	public void HideGameMode()
	{
		animGameMode.SetBool("IsHidden", true);
	}

	private void SavePlayerPrefs()
	{
		PlayerPrefs.SetInt($"{nameof(GameState.GameMode)}", (int)GameState.GameMode);
		PlayerPrefs.Save();
		Debug.Log("Game data saved!");
	}

	private void LoadPlayerPrefs()
	{
		if(PlayerPrefs.HasKey(nameof(GameState.GameMode)))
		{
			GameState.GameMode = (GameMode)PlayerPrefs.GetInt(nameof(GameState.GameMode));
			Debug.Log($"Game data loaded!");
		}
		else
		{
			GameState.GameMode = GameMode.Easy;
		}
	}

	#region Logs
	private void LoggingGameMode()
	{

		for(int i = 0; i < gameModeGroup.Count; i++)
		{
			if(gameModeGroup[i].isOn)
			{
				switch(i)
				{
					case (int)GameMode.Easy:
						Debug.Log($"Easy");
						break;
					case (int)GameMode.Medium:
						Debug.Log($"Medium");
						break;
					case (int)GameMode.Hard:
						Debug.Log($"Hard");
						break;
					default:
						Debug.Log($"Default");
						break;
				}
			}
		}
	}
	#endregion
}
