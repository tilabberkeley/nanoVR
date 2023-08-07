using UnityEngine;

namespace TubeRendererExamples
{
	public class ExampleHelpers
	{
		public static Texture2D CreateTileTexture( int sqrTileCount )
		{
			Texture2D texture = new Texture2D( 256, 256 );
			Color32[] px = new Color32[ texture.width * texture.height ];
			int p = 0;
			for( int y=0; y<texture.height; y++ ){
				float yNorm = y / (float) texture.height;
				for( int x=0; x<texture.width; x++ ){
					float xNorm = x / (float) texture.width;
					bool isWhite = (int) (yNorm*sqrTileCount) % 2 == 0;
					if( (int) (xNorm*sqrTileCount) % 2 == 0 ) isWhite = !isWhite;
					px[p++] = isWhite ? new Color32(255,255,255,255) : new Color32(0,0,0,255);
				}
			}
			texture.SetPixels32(px);
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.Apply();
			texture.hideFlags = HideFlags.HideAndDontSave;
			texture.name = "ChessTileTexture_" + sqrTileCount;
			return texture;
		}
	}
}