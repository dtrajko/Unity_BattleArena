using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
	private Transform _XForm_Camera;
	private Transform _XForm_Parent;
	private Vector3 _LocalRotation;

	private float _CameraDistance = 10f;
	public float MouseSensitivity = 4.0f;
	public float ScrolSensitivity = 2.0f;
	public float OrbitDampening = 10.0f;
	public float ScrollDampening = 6.0f;
	public bool CameraDisabled = false;


	void Start()
	{
		this._XForm_Camera = this.transform;
		this._XForm_Parent = this.transform.parent;
	}

	void LateUpdate()
	{
		if(Input.GetAxis("Mouse X") !=0 || Input.GetAxis("Mouse Y") !=0)
		{
			_LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
			_LocalRotation.y -= Input.GetAxis("Mouse Y") * MouseSensitivity;

			_LocalRotation.y = Mathf.Clamp(_LocalRotation.y, -30f, 90f);

			Quaternion QT = Quaternion.Euler (_LocalRotation.y, _LocalRotation.x, 0);
			this._XForm_Parent.rotation = Quaternion.Lerp (this._XForm_Parent.rotation, QT, Time.deltaTime * OrbitDampening);
		}
	}
}