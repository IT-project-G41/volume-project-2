Shader "VolumeRendering/VolumeRendering"
{
	Properties
	{
		//_Color("Color", Color) = (1, 1, 1, 1)
		_Volume("Volume", 3D) = "" {}
		_Intensity("Intensity", Range(1.0, 5.0)) = 1.2
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

				/**
				Pass{
					NAME "OUTLINE"

					Cull Front   //The command is to remove the front triangles and render only the back


					CGPROGRAM
					#pragma vertex vert
        			#pragma fragment frag
			
					#include "UnityCG.cginc"
			
					half _Outline;

					struct a2v {
						float4 vertex : POSITION;
						float3 normal : NORMAL;
					}; 
			
					struct v2f {
						float4 pos : SV_POSITION;
					};


					v2f vert(a2v v){
						v2f f;
						//float3 normal = UnityObjectToWorldNormal(v.normal);
						float3 normal = v.normal;
						normal.z += 1;

						f.pos = UnityObjectToClipPos(v.vertex + float4(normalize(normal), 0) * 1.05);
						return f;
					}

					float4 frag(v2f f) : SV_Target{
						return float4(0, 0, 0, 1);
					}

					ENDCG
				}
				*/


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
					sampler3D _Volume;
					half _Intensity, _Threshold;
					half _min_Range, _max_Range;
					half3 _SliceMin, _SliceMax;
					float4x4 _AxisRotationMatrix;
					

					/**
					存储了每个点的坐标以及每个点到摄像机的方向向量
					stored the coordinates of each point under the object space and the direction vector from each point to camera
					*/
					struct Ray {
						float3 origin;    // 模型空间的xyz坐标
						float3 dir;    // 世界空间下点到摄像机的方向向量转模型空间
					};

					// min 和max均是两个常量，猜测为一个区间
					struct AABB {
						float3 min;
						float3 max;
					};

					bool intersect(Ray r, AABB aabb, out float t0, out float t1)
					{
						float3 invR = 1.0 / r.dir;     // r 中的所有值取倒数 -> (方向向量取倒数)
						float3 tbot = invR * (aabb.min - r.origin);   // 上面所有值乘下面min和origin的差值   新的向量
						float3 ttop = invR * (aabb.max - r.origin);   // 上面所有值乘下面max和origin的差值   新的向量
						float3 tmin = min(ttop, tbot);   // 取两个新向量中的最小值
						float3 tmax = max(ttop, tbot);   // 取两个新向量中的最大值
						float2 t = max(tmin.xx, tmin.yz);
						t0 = max(t.x, t.y);
						t = min(tmax.xx, tmax.yz);
						t1 = min(t.x, t.y);
						return t0 <= t1;
					}


					// 世界空间坐标转模型空间
					float3 localize(float3 p) {
						return mul(unity_WorldToObject, float4(p, 1)).xyz;
					}

					float3 get_uv(float3 p) {
						//float3 local = localize(p);
						return (p + 0.5);
					}


					// 输出值要么是v，要么是0
					float sample_volume(float tex, float3 p){
						//
						float v = tex * _Intensity;   //颜色增强

						// work out were the matrix rotation should be
						float3 axis = mul(_AxisRotationMatrix, float4(p, 0)).xyz;
						axis = get_uv(axis);

						// make sure this object should remain visible
						// SliceMin < axis < SliceMax
						float min = step(_SliceMin.x, axis.x) * step(_SliceMin.y, axis.y) * step(_SliceMin.z, axis.z);
						float max = step(axis.x, _SliceMax.x) * step(axis.y, _SliceMax.y) * step(axis.z, _SliceMax.z);

						return v * min * max;
					}



					bool outside(float3 uv){
						const float EPSILON = 0.01;
						float lower = -EPSILON;
						float upper = 1 + EPSILON;
						return (
								  uv.x < lower || uv.y < lower || uv.z < lower || uv.x > upper || uv.y > upper || uv.z > upper
							  );
					}

					float4 map(float density) {
						//work out what color to use by finding the correct index
						int index = 0;

						[unroll]
						for (int iter = 0; iter < AmountOfSegments; iter++){
							// increment the index if correct
							index += (density > _Density[iter]);
						}

						// return the part of the color between the two colors
						return lerp(_Colors[index], _Colors[index - 1], (density -	_Density[index - 1]) / (_Density[index] - _Density[index - 1]));
					}

					struct appdata{
						float4 vertex : POSITION;   
						float2 uv : TEXCOORD0;    //模型纹理，，，这玩意儿居然还有纹理？
					};

					struct v2f{
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float3 world : TEXCOORD1;   // 世界空间坐标值
						float3 local : TEXCOORD2;   // 模型空间坐标的xyz值
					};

					v2f vert(appdata v){

						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);   // 模型空间坐标至剪裁空间坐标
						o.uv = v.uv;
						o.world = mul(unity_ObjectToWorld, v.vertex).xyz;   // 将顶点坐标从模型空间转到世界空间
						o.local = v.vertex.xyz;   
						return o;
					}

					fixed4 frag(v2f i) : SV_Target{
						Ray ray;
						// ray.origin = localize(i.world);
						ray.origin = i.local;

						// world space direction to object space
						float3 dir = (i.world - _WorldSpaceCameraPos);    // 世界空间下点到摄像机的方向向量
						ray.dir = normalize(mul(unity_WorldToObject, dir));

						AABB aabb;
						//aabb.min = float3(-0.5, -0.5, -0.5);
						//aabb.max = float3(0.5, 0.5, 0.5);

						aabb.min = float3(-1, -1, -1);
						aabb.max = float3(1, 1, 1);

						float tnear;
						float tfar;
						intersect(ray, aabb, tnear, tfar);

						tnear = max(0.0, tnear);    // 让tnear恒大于等于0

						float3 start = ray.origin + ray.dir * tnear;
						//float3 start = ray.origin;
						float3 end = ray.origin + ray.dir * tfar;
						float dist = abs(tfar - tnear); // float dist = distance(start, end);
						float step_size = dist / float(ITERATIONS);
						float3 ds = normalize(end - start) * step_size;

						float4 dst = float4(0, 0, 0, 0);
						float3 p = start;

						[unroll]
						for (int iter = 0; iter < ITERATIONS; iter++){

							// get shader info
							float3 uv = get_uv(p);   //p中每个值加0.5

							float4 tex = tex3D(_Volume, uv);   //体积纹理
							float currentValue = tex.a;   // 将体积纹理的透明度作为当前值传入

							// get the darkness of the value
							float v = sample_volume(currentValue, p);

							// work out if it is in the threashhold
							//float inRange = (v > _min_Range) * (v < _max_Range) * (v*255 > _Density[1]);
						
							// create the new color
							float4 src = map(v*255);   // 通过透明度乘255计算出一个新的颜色

							// make the color transperent so you can see behind it
							src.a *= 0.5;   // 将新颜色的透明的折半
							// washes out the voxel
							src.rgb *= dst.a;

							// blend the color with the ones behind it
							//dst = ((1.0 - dst.a) * src + dst) * inRange;
							dst = ((1.0 - dst.a) * src + dst);
							p += ds;

							// work out what the threashhold is
							if (dst.a > _Threshold){
								break;
							}
						}

						  return dst;
					  }

				  #endif 
							  #pragma vertex vert
							  #pragma fragment frag

							  ENDCG
						  }
		}
}
