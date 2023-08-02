using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Starfish : MonoBehaviour
	{
		const int ARM_COUNT = 5;
		const float ARM_OFFSET = 0.1f;
		const int POINT_COUNT = 20;

		TubeRenderer[] _armTubes;
		
		
		void Start()
		{	
			// Create arms.
			_armTubes = new TubeRenderer[ARM_COUNT];
			for( int a=0; a<ARM_COUNT; a++ )
			{
				// Create and new game object and TubeRendeder component.
				_armTubes[a] = new GameObject( "Arm" + a ).AddComponent<TubeRenderer>();
				_armTubes[a].transform.parent = transform;
				
				// Set edge count.
				_armTubes[a].edgeCount = 6;
				
				// Set no caps.
				_armTubes[a].caps = TubeRenderer.CapMode.None;
				
				// Define uv mapping.
				_armTubes[a].uvRect = new Rect( 0, 0, 4, 1 );
				
				// Optimise for realtime manipulation.
				_armTubes[a].MarkDynamic();
				
				// Create point, radius and color arrays.
				_armTubes[a].points = new Vector3[POINT_COUNT];
				_armTubes[a].radiuses = new float[POINT_COUNT];
				_armTubes[a].colors = new Color32[POINT_COUNT];
				
				// Define radiuses.
				for( int p=0; p<POINT_COUNT; p++ ) _armTubes[a].radiuses[p] = (1-(p/(POINT_COUNT-1f))) * ARM_OFFSET*1.49f;
				
				// Rotate and position.
				float armNorm = a / (float) ARM_COUNT;
				_armTubes[a].transform.Rotate( 0, armNorm * 360, 0 );
				_armTubes[a].transform.Translate( ARM_OFFSET, 0, 0, Space.Self );
			}

			// Create (hard coded) head.
			TubeRenderer headTube = new GameObject("Head").AddComponent<TubeRenderer>();
			headTube.transform.parent = transform;
			headTube.edgeCount = ARM_COUNT;
			headTube.caps = TubeRenderer.CapMode.End;
			headTube.uvRectCap = new Rect( 0, 0, 1/6f, 1/6f );
			headTube.radius = ARM_OFFSET*1.24f;
			headTube.points = new Vector3[]{ Vector3.zero, Vector3.up * ARM_OFFSET*1.29f };
			headTube.colors = new Color32[]{ Color.black, Color.black };
			
			// Rotate the head tube around it's forward direction to match the arms (could use transform.Rotate instead).
			headTube.forwardAngleOffset = -18;
			
			// Add a material with a shader that takes vertex colors.
			Material material = RenderPipelineHelpers.CreateRenderPipelineDependentVertexColorMaterial();
			Texture texture = ExampleHelpers.CreateTileTexture( 6 );
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture );
			headTube.GetComponent<Renderer>().sharedMaterial = material;
			for( int a=0; a<ARM_COUNT; a++ ) _armTubes[a].GetComponent<Renderer>().sharedMaterial = material;
		}
		
		
		void Update()
		{
			// Calculate colors for one arm.
			for( int p=0; p<POINT_COUNT; p++ ) _armTubes[0].colors[p] = PerlinColorAtOffset( -p*0.1f );

			// Overwite to trigger mesh update.
			_armTubes[0].colors = _armTubes[0].colors;

			// Reuse colors for the rest of the arms.
			for( int a=1; a<_armTubes.Length; a++ ) _armTubes[a].colors = _armTubes[0].colors;

			// Do freaky arm animation ...
			for( int a=0; a<_armTubes.Length; a++ ){
				float angleY = 0;
				float angleZ = 0;
				for( int p=0; p<POINT_COUNT; p++ ){
					float norm = p / (POINT_COUNT-1f);
					Vector3 offset = Vector3.right * Mathf.Lerp( 0.1f, 0.01f, norm );
					if( p > 1 ){
						angleY += (Mathf.PerlinNoise( norm*4 , a*10 - Time.time*0.8f )*2-1 ) * 60 * norm;
						angleZ += (Mathf.PerlinNoise( norm*4 , 500+a*10 - Time.time*0.3f )*2-1 ) * 60 * norm;
						offset = Quaternion.Euler( 0, angleY, angleZ ) * offset;
					}
					if( p > 0 ) _armTubes[a].points[p] = _armTubes[a].points[p-1] + offset;
				}
				_armTubes[a].points = _armTubes[a].points;
			}
		}
		
		
		Color PerlinColorAtOffset( float offset ){
			return Color.Lerp( Color.black, Color.white, Mathf.InverseLerp( 0.3f, 0.7f, Mathf.PerlinNoise( offset + Time.time*2.2f, 0  ) ) );
		}
	}
}