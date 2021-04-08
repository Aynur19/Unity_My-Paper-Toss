using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Lean.Touch;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnv : MonoBehaviour
{
	public bool IsTrashThrowed;

	[SerializeField]
	private PaperTrash paperTrash;
	private PaperTrash currentTrash;

	[SerializeField]
	private LeanFingerUp fingerUp;
	
	[SerializeField]
	private LeanFingerSwipe fingerSwipe;

	[SerializeField]
	private Fan fan;

	[SerializeField]
	private float strengthForce;

	[SerializeField]
	private int trashes;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI recordText;

	[SerializeField]
	private TextMeshProUGUI trashesText;

	[SerializeField]
	private TextMeshProUGUI statsScoreText;

	[SerializeField]
	private TextMeshProUGUI statsThrowsText;
	
	[SerializeField]
	private TextMeshProUGUI statsHitsText;

	[SerializeField]
	private TextMeshProUGUI statsHitPercentageText;

	[SerializeField]
	public Animator animGameOver;

	private Camera mainCamera;
	private float angle;
	private float force;
	
	private int record;
	private int score;
	private int throws;
	private int hits;
	private bool isActiveGameOver;

	private void Awake()
	{
		LoadGame();
		mainCamera = Camera.main;
		
		isActiveGameOver = false;
	
		fingerUp.OnFinger.AddListener(x => OnFingerUp());
		fingerSwipe.OnDelta.AddListener(x => SwipeAction(x));
		fingerSwipe.OnDistance.AddListener(x => OnDistance(x));

		animGameOver.SetBool("IsHidden", true);
	}

	private void Start()
	{
		CreateTrash();
		UpdateStats();
	}

	private void ShowGameOver()
	{
		animGameOver.SetBool("IsHidden", false);
		isActiveGameOver = true;
	}

	public void HideGameOver()
	{
		animGameOver.SetBool("IsHidden", true);
		isActiveGameOver = false;
	}


	private void SwipeAction(Vector2 delta)
	{
		if(delta.sqrMagnitude > 0.0f)
		{
			angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
		}
	}

	private void OnDistance(float distance)
	{
		force = distance;
	}

	private void UpdateScore(bool isGoal = true)
	{
		if(isGoal)
		{
			hits++;
			score += 100;
		}
		else
		{
			score -= 100;
			hits--;

			if(score < 0)
			{
				score = 0;
			}

			if(hits < 0)
			{
				hits = 0;
			}
		}


	}

	private void UpdateRecord()
	{
		if(record < score)
		{
			record = score;
		}

		recordText.text = GetRecord();
	}

	private void UpdateTrashCount(bool isGoal)
	{
		if(isGoal)
		{
			trashes++;
		}
		else
		{
			trashes--;
		}

		if(trashes <= 0)
		{
			trashes = 0;
		}
		
	}

	public void UpdateGameInfo(bool isGoal = true)
	{
		UpdateScore(isGoal);
		UpdateTrashCount(isGoal);

		UpdateStats();
	}

	private void UpdateStats()
	{
		scoreText.text = GetScore();
		recordText.text = GetRecord();
		trashesText.text = GetTrashCount();

		statsScoreText.text = $"{score}";
		statsThrowsText.text = $"{throws}";
		statsHitsText.text = $"{hits}";

		if(throws == 0)
		{
			statsHitPercentageText.text = $"{0:F}";
		}
		else
		{
			statsHitPercentageText.text = $"{((float)hits / (float)throws):F}";
		}
	}

	private void GameOver()
	{
		ShowGameOver();
	}

	private string GetScore()
	{
		return $"Score: {score}";
	}

	private string GetRecord()
	{
		return $"Record: {record}";
	}

	private string GetTrashCount()
	{
		return $"Trashes: {trashes}";
	}

	private void CreateTrash()
	{
		var cameraTransform = mainCamera.transform;
		var trashPosition = cameraTransform.position;
		currentTrash = Instantiate(paperTrash, trashPosition, Quaternion.identity, cameraTransform);
		currentTrash.transform.position += new Vector3(0.1f, -0.2f, 0.4f);
		currentTrash.GameEnv = this;
	}

	private void OnFingerUp()
	{
		if(trashes <= 0 && !isActiveGameOver)
		{
			GameOver();
			return;
		}

		if(IsTrashThrowed)
		{
			return;
		}

		var cameraTransform = mainCamera.transform;
		var forward = cameraTransform.forward;

		var speed = (Quaternion.Euler(-20, angle, 0) * forward * force) / (strengthForce / currentTrash.TrashRigidbody.mass);
		currentTrash.TrashRigidbody.isKinematic = false;
		currentTrash.TrashRigidbody.AddForce(speed, ForceMode.Impulse);

		StartCoroutine(fan.ApplyForce(currentTrash));
		IsTrashThrowed = true;
		throws++;
		if(trashes > 0)
		{
			trashes--;
		}
		StartCoroutine(TakeNewShell());
		StartCoroutine(currentTrash.StartDestroy());
	}

	private IEnumerator TakeNewShell()
	{
		yield return new WaitForSeconds(2);

		if(trashes > 0)
		{
			IsTrashThrowed = false;
			if(fan.IsVariableLocation)
			{
				fan.ReversePosition();
			}
			fan.NewForce();
		
			CreateTrash();
		}
		
		UpdateStats();
	}

	public void SaveGame()
	{
		UpdateRecord();
		BinaryFormatter bf = new BinaryFormatter();

		PlayerAchievs achievs = new PlayerAchievs();
		achievs.MaxScore = record;
		FileStream saveFile = File.Create($"{Application.persistentDataPath}/{nameof(PlayerAchievs)}.dat");
		bf.Serialize(saveFile, achievs);
		saveFile.Close();
		Debug.Log("Player achievs saved!");

		var savedGame = new LastGame();
		savedGame.GameMode = GameState.GameMode;
		savedGame.Score = score;
		savedGame.Trashes = trashes;
		saveFile = File.Create($"{Application.persistentDataPath}/{nameof(LastGame)}.dat");
		bf.Serialize(saveFile, savedGame);
		saveFile.Close();
		Debug.Log("Game saved!");

		SceneManager.LoadScene(0);
	}

	public void LoadGame()
	{
		var filepath = $"{Application.persistentDataPath}/{nameof(PlayerAchievs)}.dat";
		if(File.Exists(filepath))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream loadFile = File.Open(filepath, FileMode.Open);

			PlayerAchievs achievs = (PlayerAchievs)bf.Deserialize(loadFile);
			loadFile.Close();

			record = achievs.MaxScore;

			Debug.Log("Player achievs loaded!");
		}
		else
		{
			Debug.Log("Player achievs not saved!");
		}

		filepath = $"{ Application.persistentDataPath}/{ nameof(LastGame)}.dat";
		if(File.Exists(filepath))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream loadFile = File.Open(filepath, FileMode.Open);

			LastGame savedGame = (LastGame)bf.Deserialize(loadFile);
			loadFile.Close();

			if(savedGame.Trashes > 0)
			{
				GameState.GameMode = savedGame.GameMode;
				score = savedGame.Score;
				trashes = savedGame.Trashes;
				Debug.Log("Last game loaded!");
			}
			else
			{
				Debug.Log("Last game overed!");
			}
		}
		else
		{
			Debug.Log("Last Game not saved!");
		}
	}
}

[Serializable]
class LastGame
{
	public int Score;
	public int Trashes;
	public GameMode GameMode;
}

[Serializable]
class PlayerAchievs
{
	public int MaxScore;
}