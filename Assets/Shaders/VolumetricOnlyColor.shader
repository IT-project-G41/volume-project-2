Shader "VolumeRendering/VolumetricOnlyColor"
{
	Properties
	{
		_Volume("Volume", 3D) = "" {}
		_Range("Range", Range(1.0, 5.0)) = 1.2
		_Threshold("Threshold", Range(0.0, 1.0)) = 0.95
		_min_Range("min_Range", Range(0.0, 1.0)) = 0.01
		_max_Range("max_Range", Range(0.0, 1.0)) = 0.99
		_SliceMin("Slice min", Vector) = (0.0, 0.0, 0.0, -1.0)
		_SliceMax("Slice max", Vector) = (1.0, 1.0, 1.0, -1.0)
	}

		CGINCLUDE

			ENDCG

			SubShader{
				Cull Back
				Blend SrcAlpha OneMinusSrcAlpha
				ZTest Always

				Pass
				{
					CGPROGRAM

					#ifndef __VOLUME_RENDERING_INCLUDED__
					#define __VOLUME_RENDERING_INCLUDED__

					#include "UnityCG.cginc"

					#ifndef ITERATIONS
					#define ITERATIONS 256
					#endif


					#ifndef AmountOfSegments
					#define AmountOfSegments 6
					#endif

					//half4 _Color;
					half4 _Colors[AmountOfSegments];
					float _Density[AmountOfSegments];
					float _Intensity[AmountOfSegments];
					float _Range;
					sampler3D _Volume;
					half _Threshold;
					half _min_Range, _max_Range;
					half3 _SliceMin, _SliceMax;
					float4x4 _AxisRotationMatrix;
					float3 aabbMin, aabbMax;
					float _MaxDimention;


					struct Ray {
						float3 origin;
						float3 dir;
					};

					struct AABB {
						float3 min;
						float3 max;
					};

					bool intersect(Ray r, AABB aabb, out float t0, out float t1)
					{
						float3 invR = 1.0 / r.dir;
						float3 tbot = invR * (aabb.min - r.origin);
						float3 ttop = invR * (aabb.max - r.origin);
						float3 tmin = min(ttop, tbot);
						float3 tmax = max(ttop, tbot);
						float2 t = max(tmin.xx, tmin.yz);
						t0 = max(t.x, t.y);
						t = min(tmax.xx, tmax.yz);
						t1 = min(t.x, t.y);
						return t0 <= t1;
					}

					float3 localize(float3 p) {
						return mul(unity_WorldToObject, float4(p, 1)).xyz;
					}

					float3 get_uv(float3 p) {
						return (p + 0.5);
					}

					float sample_volume(float3 p)
					{
						// work out were the matrix rotation should be
						float3 axis = mul(_AxisRotationMatrix, float4(p, 0)).xyz;
						axis = get_uv(axis);

						// make sure this object should remain visible
						float min = step(_SliceMin.x, axis.x) * step(_SliceMin.y, axis.y) * step(_SliceMin.z, axis.z);
						float max = step(axis.x, _SliceMax.x) * step(axis.y, _SliceMax.y) * step(axis.z, _SliceMax.z);

						return min * max;
					}

					  struct appdata
					  {
						float4 vertex : POSITION;
						float2 uv : TEXCOORD0;
					  };

					  struct v2f
					  {
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float3 world : TEXCOORD1;
						float3 local : TEXCOORD2;
					  };

					  v2f vert(appdata v)
					  {
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.uv = v.uv;
						o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
						o.local = v.vertex.xyz;
						return o;
					  }

					  fixed4 frag(v2f i) : SV_Target
					  {
						Ray ray;
					  // ray.origin = localize(i.world);
					  ray.origin = i.local;

					  // world space direction to object space
					  float3 dir = (i.world - _WorldSpaceCameraPos);
					  ray.dir = normalize(mul(unity_WorldToObject, dir));

					  // work out what is less the Overall aabb or the bounding
					  aabbMin.x = min(aabbMin.x, _SliceMin.x);
					  aabbMax.x = max(aabbMax.x, _SliceMax.x);
					  aabbMin.y = min(aabbMin.y, _SliceMin.y);
					  aabbMax.y = max(aabbMax.y, _SliceMax.y);
					  aabbMin.z = min(aabbMin.z, _SliceMin.z);
					  aabbMax.z = max(aabbMax.z, _SliceMax.z);

					  // calcualte the aabb
					  AABB aabb;
					  aabb.min = aabbMin.xyz + float3(-0.5, -0.5, -0.5);
					  aabb.max = aabbMax.xyz + float3(-0.5, -0.5, -0.5);

					  float tnear;
					  float tfar;
					  intersect(ray, aabb, tnear, tfar);

					  tnear = max(0.0, tnear);

					  float3 start = ray.origin + ray.dir * tnear;
					  //float3 start = ray.origin;
					  float3 end = ray.origin + ray.dir * tfar;
					  float dist = abs(tfar - tnear); // float dist = distance(start, end);
					  float step_size = dist / float(ITERATIONS);
					  float3 ds = normalize(end - start) * step_size;

					  float4 dst = float4(0, 0, 0, 0);
					  float3 p = start;

					  [unroll]
					  for (int iter = 0; iter < ITERATIONS; iter++)
					  {
						  // get shader info
						  float3 uv = get_uv(p);

						  float4 tex = tex3D(_Volume, uv);

						  tex *= sample_volume(p);

						  dst = (tex * tex.a) + dst * (1 + float(iter)/ float(ITERATIONS));

						  // work out what the threashhold is
						  if (dst.a > 1 - _Threshold) 
						  {
							  return dst;
						  }

						  p += ds;
					}

				  return float4(0,0,0,0);
				}

				#endif 
				#pragma vertex vert
				#pragma fragment frag

				ENDCG
			}
		}
}
