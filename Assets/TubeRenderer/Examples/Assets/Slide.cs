/*
	NOTE

	This example was originally made in ancient times when Unity supported concave mesh
	collisions. Since PhysX 3 was introduced back in 2015, the support was dropped.
	https://forum.unity.com/threads/why-concave-colliders-arent-allowed-anymore.335516/

	Without concave mesh colliders, it is a lot harder to craete perfect collision for
	tubes in a standadised way. In this example we use a collection of BoxColldiers as
	a work-around. That of course, is not a catch all solution.
*/

using UnityEngine;
using System.Collections;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Slide : MonoBehaviour
	{
		Material _tileMaterial;
		
		int POINT_COUNT = 64;

		void Start()
		{
			// Add a TubeRenderer component.
			TubeRenderer tube = gameObject.AddComponent<TubeRenderer>();
			
			// Define uv mapping.
			tube.uvRect = new Rect( 0, 0, 4, 1 );
			tube.uvRectCap = new Rect( 0.543f, 0, 0.33f, 0.33f );
			
			// Set a global radius for the tube.
			tube.radius = 0.5f;
			
			// Reduce tube mesh to four edges.
			tube.edgeCount = 4;

			// Set normal mode to hard edges.
			tube.normalMode = TubeRenderer.NormalMode.HardEdges;

			// Roll the tube 45 degrees, so that edges will align with the box colliders (added later).
			tube.forwardAngleOffset = 45;

			// Create point array.
			tube.points = new Vector3[ POINT_COUNT ];
			
			// Define points.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float angle = norm * Mathf.PI*2 * 0.7f;
				float radius = Mathf.Lerp( 2, 0.8f, norm );
				float y = Mathf.Lerp( 2, 0, norm );
				tube.points[p] = new Vector3( Mathf.Cos(angle)*radius, y, Mathf.Sin(angle)*radius );
			}
			
			// IMPORTANT! call ForceUpdate to generate the mesh immediately, before adding the MeshCollder.
			tube.ForceUpdate();

			// In the old days, we would just add a MeshCollider. The reference to the tube mesh woud be set automatically.
			//gameObject.AddComponent<MeshCollider>();

			// ... Now, we have to make a compund colloder. In this example we build it from boxes.
			float boxSize = 1 / Mathf.Sqrt( 2 ) * tube.radius * 2;
			for( int p = 0; p < POINT_COUNT; p++ ) {
				GameObject boxObject = GameObject.CreatePrimitive( PrimitiveType.Cube );
				boxObject.transform.position = tube.points[p];
				boxObject.transform.rotation = tube.GetRotationAtPoint( p );
				boxObject.transform.localScale = new Vector3( boxSize, boxSize, tube.GetLengthAtPoint( p ) * 0.5f );
				boxObject.GetComponent<MeshRenderer>().enabled = false;
				boxObject.transform.SetParent( transform );
			}

			// Create a material at apply it to the tube.
			_tileMaterial = RenderPipelineHelpers.CreateRenderPipelineCompatibleMaterial();
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( _tileMaterial, ExampleHelpers.CreateTileTexture( 6 ) );
			tube.GetComponent<Renderer>().sharedMaterial = _tileMaterial;
			
			// Destroy the TubeRenderer component to free up memory.
			Destroy( tube );
			
			// Start the rain.
			StartCoroutine( RainCoroutine() );
		}
		
		
		IEnumerator RainCoroutine()
		{
			while( true )
			{
				// Generate ball.
				GameObject ball = GameObject.CreatePrimitive( PrimitiveType.Sphere );
				ball.transform.position = new Vector3( 1.4f, 2f, -0.1f );
				ball.transform.localScale = Vector3.one * 0.5f;
				ball.AddComponent<Rigidbody>();
				ball.GetComponent<Rigidbody>().mass = 1f;
				ball.GetComponent<Rigidbody>().drag = 0.01f;
				ball.GetComponent<Rigidbody>().angularDrag = 0.05f;
				ball.GetComponent<Rigidbody>().AddForce( new Vector3( 0, -200, 800 ) );
				ball.GetComponent<Renderer>().sharedMaterial = _tileMaterial;
				
				// You are dying from the moment you are born.
				StartCoroutine( WaitAndDestroy( ball ) );
				
				// Wait before we generate next ball.
				yield return new WaitForSeconds( 0.3f );
			}
		}
		
		
		IEnumerator WaitAndDestroy( GameObject go )
		{
			yield return new WaitForSeconds( 4 );
			Destroy( go );
		}
	}
}