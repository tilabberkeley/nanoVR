using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Invertebrate : MonoBehaviour
	{
		TubeRenderer _tube;

		const int pointCount = 24;


		void Awake()
		{
			// Add a tube, set texture, points, radius and uv mapping, then optimise for realtime.
			_tube = gameObject.AddComponent<TubeRenderer>();
			Material material = _tube.GetComponent<Renderer>().material;
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, ExampleHelpers.CreateTileTexture( 12 ) );  // URP and HDRP compatibility.
			_tube.points = new Vector3[ pointCount ];
			_tube.radius = 0.3f;
			_tube.uvRect = new Rect( 0, 0, 2, 1 );
			_tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			_tube.MarkDynamic();
		}


		void Update()
		{
			// Operate directly on the points.
			for( int p = 0; p < pointCount; p++ ) {
				float y = p / (pointCount-1f) * 3;
				float t = y * 0.5f + Time.time * 0.5f;
				float x = Mathf.PerlinNoise( t, 0 )*2-1;
				float z = Mathf.PerlinNoise( t, 93.17f )*2-1;
				_tube.points[ p ].Set( x, y, z );
			}
			// Then apply.
			_tube.points = _tube.points;
		}
	}
}



