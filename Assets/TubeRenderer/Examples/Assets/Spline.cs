using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Spline : MonoBehaviour
	{
		SplineMaker _splineMaker;
		Vector3[] _anchorPoints;


		void Start()
		{
			// Add TubeRendeder component.
			TubeRenderer tube = gameObject.AddComponent<TubeRenderer>();

			// Optimise for realtime manipulation.
			tube.MarkDynamic();
			
			// Set a texture and a uv mapping.
			Texture texture = ExampleHelpers.CreateTileTexture( 12 );
			Material material = tube.GetComponent<Renderer>().sharedMaterial;
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture );
			tube.uvRect = new Rect( 0, 0, 6, 1 );
			tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );

			// Add a SplineMaker component.
			_splineMaker = gameObject.AddComponent<SplineMaker>();

			// Set the spline resolution.
			_splineMaker.pointsPerSegment = 16;

			// Route curve points from spline to tube.
			_splineMaker.onUpdated.AddListener( ( points ) => tube.points = points );

			// Create anchor points for curve.
			_anchorPoints = new Vector3[6];
			for( int a = 0; a < _anchorPoints.Length; a++ ) _anchorPoints[a] = new Vector3();
		}
		
		
		void Update()
		{
			// Update the anchors with some animation.
			for( int a = 0; a < _anchorPoints.Length; a++ ){
				float radialAngle = ( a / (float) _anchorPoints.Length ) * Mathf.PI * 2;
				float x = Mathf.Cos( radialAngle ) * 0.6f;
				float y = Mathf.Cos( Time.time * 0.8f + (a%2) * Mathf.PI + a ) * 0.5f;
				float z = Mathf.Sin( radialAngle );
				_anchorPoints[a].Set( x, y, z);
			}

			// Apply anchors to trigger computation of spline.
			_splineMaker.anchorPoints = _anchorPoints;
		}

	}
}