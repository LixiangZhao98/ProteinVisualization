/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;

namespace TubeRendererInternals
{
	public static class RenderPipelineHelpers
	{
		const string urpKeyword = "Universal";
		const string hdrpKeyword = "HighDefinition";

		const string defaultShaderName = "Standard";
		const string urpDefaultShaderName = "Universal Render Pipeline/Lit";
		const string hdrpDefaultShaderName = "HDRP/Lit";

		const string vertexColorShaderName = "Hidden/StandardVertexColor";
		const string urpVertexColorShaderName = "Universal Render Pipeline/Particles/Lit";
		const string hdrpVertexColorShaderName = "UI/Unlit/Text"; // HDRP doesn not have a lit, textured, and vertex colored shader at this time (2019.4).

		static class ShaderIDs
		{
			// URP.
			public static readonly int baseMap = Shader.PropertyToID( "_BaseMap" );

			// HDRP.
			public static readonly int baseColorMap = Shader.PropertyToID( "_BaseColorMap" );
			public static readonly int metallic = Shader.PropertyToID( "_Metallic" );
			public static readonly int baseColor = Shader.PropertyToID( "_BaseColor" );
		}


		public static Material CreateRenderPipelineCompatibleMaterial()
		{
			#if UNITY_2019_3_OR_NEWER
			if( GraphicsSettings.currentRenderPipeline ) {
				string rpName = GraphicsSettings.currentRenderPipeline.ToString();
				if( rpName.Contains( urpKeyword ) ) {
					return new Material( Shader.Find( urpDefaultShaderName ) );
				} else if( rpName.Contains( hdrpKeyword ) ) {
					Material mat = new Material( Shader.Find( hdrpDefaultShaderName ) );
					mat.SetFloat( ShaderIDs.metallic, 0.8f ); // Texture is not visible when metallic is zero, which happens to be the default.
					mat.SetColor( ShaderIDs.baseColor, Color.white ); // Albedo deafult is less than one.
					return mat;
				}
			}
			#endif

			// Legacy build-in render pipeline
			return new Material( Shader.Find( defaultShaderName ) );
		}


		public static Material CreateRenderPipelineDependentVertexColorMaterial( bool hideAndDontSave = true )
		{
			Material material = null;
			#if UNITY_2019_3_OR_NEWER
			if( GraphicsSettings.currentRenderPipeline ) {
				string rpName = GraphicsSettings.currentRenderPipeline.ToString();
				if( rpName.Contains( urpKeyword ) ) {
					material = new Material( Shader.Find( urpVertexColorShaderName ) );
				} else if( rpName.Contains( hdrpKeyword ) ) {
					material = new Material( Shader.Find( hdrpVertexColorShaderName ) );
				}
			}
			
			# endif
			if( !material ) {
				material = new Material( Shader.Find( vertexColorShaderName ) );
			}
			if( hideAndDontSave ) material.hideFlags = HideFlags.HideAndDontSave;
			return material;
		}


		public static void SetRenderPipelineDependentMainTexture( Material material, Texture mainTexture )
		{
			#if UNITY_2019_3_OR_NEWER
			if( GraphicsSettings.currentRenderPipeline ) {
				string rpName = GraphicsSettings.currentRenderPipeline.ToString();
				if( rpName.Contains( urpKeyword ) ) {
					material.SetTexture( ShaderIDs.baseMap, mainTexture );
				} else if( rpName.Contains( hdrpKeyword ) ) {
					material.SetTexture( ShaderIDs.baseColorMap, mainTexture );
				}
				return;
			}
			#endif
			material.mainTexture = mainTexture;
		}
	}
}