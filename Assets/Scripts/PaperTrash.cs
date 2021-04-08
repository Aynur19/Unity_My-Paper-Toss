using System.Collections;
using UnityEngine;

public class PaperTrash : MonoBehaviour
{
	[SerializeField]
	private Rigidbody trashRigidbody;
	public Rigidbody TrashRigidbody => trashRigidbody;

	[SerializeField]
	private SphereCollider sphereCollider;

	public GameEnv GameEnv { get; set; }

	public bool IsFanned;
	private bool isDestroyed;
	private LayerMask trashBinMask;
	private LayerMask groundMask;
	private LayerMask trashCounterMask;
	private bool isGoal;
	private float area;
	public float Area => area;

	private void Start()
	{
		trashBinMask = LayerMask.NameToLayer("TrashBin");
		groundMask = LayerMask.NameToLayer("Ground");
		trashCounterMask = LayerMask.NameToLayer("TrashCounter");

		area = GetSphereColliderArea();
	}

	private float GetSphereColliderArea()
	{
		return (4 * Mathf.PI * Mathf.Pow(sphereCollider.radius * GetAvgLocalScale(), 2f));
	}

	private float GetAvgLocalScale()
	{
		return (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3;
	}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if(collision.gameObject.layer == groundMask)
	//	{
	//		CheckCollision();
	//	}
	//}

	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == trashCounterMask.value)
		{
			isGoal = true;
			GameEnv.UpdateGameInfo(isGoal);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.layer == trashCounterMask.value)
		{
			isGoal = false;
			GameEnv.UpdateGameInfo(isGoal);
		}
	}

	//private void CheckCollision()
	//{
	//	if(!isDestroyed)
	//	{
	//		if(GameEnv == null)
	//		{
	//			GameEnv = new GameEnv();
	//		}
	//		GameEnv.IsTrashThrowed = false;
	//		isDestroyed = true;
	//		StartCoroutine(StartDestroy());
	//	}
	//}

	public IEnumerator StartDestroy()
	{
		yield return new WaitForSeconds(10);
		Destroy(gameObject);
	}

	//private IEnumerator StartDestroy()
	//{
	//	yield return new WaitForSeconds(10);
	//	Destroy(gameObject);
	//}
}
