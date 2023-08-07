using UnityEngine;
using System.Collections;

namespace TubeRendererExamples
{
	public class CameraMan : MonoBehaviour
	{
		public float speed = 4;
		public Vector3 focusPoint = Vector3.zero;
		public bool hover = false;
		public float hoverRange = 0.5f;
		
		Camera _cam;
		
		
		void Awake()
		{
			_cam = gameObject.GetComponentInChildren( typeof( Camera ) ) as Camera;
		}
		
		
		void LateUpdate()
		{
			// Rotate camera for dramatic effect.
			transform.Rotate( Vector3.up, Time.deltaTime * speed );
			
			// Hover camera for EXTRA sugar.
			if( hover ) transform.position = Vector3.up * Mathf.Sin( Time.time*0.5f ) * hoverRange;
			
			// Look at focus point.
			_cam.transform.LookAt( focusPoint );
		}
	}
}

