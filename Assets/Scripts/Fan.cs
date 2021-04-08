using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class Fan : MonoBehaviour
{
	[SerializeField]
	private Image windArrow;

	[SerializeField]
	private TextMeshProUGUI windForce;

	[SerializeField]
	private float force;

	[SerializeField]
	private float radius;

	[SerializeField]
	private GameObject internalShaftAxle;

	[SerializeField]
	private RectTransform directionArrow;

	[SerializeField]
	private float strengthForce;

	[SerializeField]
	private Transform capTransform;

	[SerializeField]
	private bool isVariableLocation;
	public bool IsVariableLocation => isVariableLocation;

	[SerializeField]
	private float minForce;

	[SerializeField]
	private float maxForce;

	private LayerMask trashMask;
	private int throwsCount;
	private bool isLeft;

	private void Awake()
	{
		switch(GameState.GameMode)
		{
			case GameMode.Medium:
				isVariableLocation = false;
				maxForce = 10;
				minForce = 5;
				break;
			case GameMode.Hard:
				isVariableLocation = true;
				maxForce = 10;
				minForce = 5;
				break;
			case GameMode.Easy:
			default:
				isVariableLocation = false;
				maxForce = 5;
				minForce = 1;
				break;
		}
	}

	private void Start()
	{
		trashMask = LayerMask.NameToLayer("Trash");
		throwsCount = 0;
		windForce.text = $"{force:F}";

		ReverseWindArrow();
		ReverseWindForce();
		NewForce();
	}

	private void FixedUpdate()
	{
		PropellerRotation(force);
	}

	public IEnumerator ApplyForce(PaperTrash trash)
	{
		yield return new WaitForSeconds(0.2f);

		if(!trash.IsFanned)
		{
			trash.IsFanned = true;
			trash.TrashRigidbody.AddForce(transform.forward * GetAppliedForce(trash), ForceMode.Impulse);
		}

	}

	private float GetAppliedForce(PaperTrash trash)
	{
		var distance = Vector3.Distance(capTransform.position, trash.transform.position);
		if(distance < 1f)
		{
			distance = 1f;
		}

		var trashArea = trash.Area;
		if(trashArea < 1f)
		{
			trashArea = 1;
		}
		trashArea /= 2;

		var appliedForce = force / (distance * distance) * trashArea;
		return appliedForce;
	}

	public void NewForce()
	{
		force = Random.Range(minForce, maxForce);
		windForce.text = $"{force:F}";
	}

	public void ReversePosition()
	{
		transform.Rotate(Vector3.up, 180);
		var position = transform.localPosition;
		position.x = -position.x;
		transform.localPosition = position;
		
		ReverseWindArrow();
		ReverseWindForce();
	}

	private void ReverseWindArrow()
	{
		var windArrowScale = windArrow.rectTransform.localScale;
		windArrow.rectTransform.localScale = new Vector3(-windArrowScale.x, windArrowScale.y, windArrowScale.z);
	}

	private void ReverseWindForce()
	{
		var windForceScale = windForce.rectTransform.localScale;
		windForce.rectTransform.localScale = new Vector3(-windForceScale.x, windForceScale.y, windForceScale.z);
	}

	private void PropellerRotation(float force)
	{
		internalShaftAxle.transform.Rotate(0f, 0f, force * strengthForce);
	}
}
