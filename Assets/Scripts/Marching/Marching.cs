using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace MarchingCubesProject
{
    public abstract class Marching
    {

        public int SurfaceMin { get; set; }
        public int SurfaceMax { get; set; }

        private uint[] Cube { get; set; }

        /// <summary>
        /// Winding order of triangles use 2,1,0 or 0,1,2
        /// </summary>
        protected int[] WindingOrder { get; private set; }

        public Marching(int surfacemin = 1, int surfaceMax = int.MaxValue)
        {
            SurfaceMin = surfacemin;
            SurfaceMax = surfaceMax;
            Cube = new uint[8];
            WindingOrder = new int[] { 0, 1, 2 };
        }

        public virtual void Generate(uint[] voxels, int width, int height, int depth, ref List<Vector3> verts, ref List<int> indices)
        {

            if (SurfaceMin > 0.0f)
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
            else
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }

            int x, y, z, i;
            int ix, iy, iz;
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    for (z = 0; z < depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        for (i = 0; i < 8; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            iy = y + VertexOffset[i, 1];
                            iz = z + VertexOffset[i, 2];

                            Cube[i] = voxels[ix + iy * width + iz * width * height];
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, ref verts, ref indices);
                    }
                }
            }
        }

         /// <summary>
        /// MarchCube performs the Marching algorithm on a single cube
        /// </summary>
        protected abstract void March(int x, int y, int z, uint[] cube, ref List<Vector3> vertList, ref List<int> indexList);

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? SurfaceMin : (SurfaceMin - v1) / delta;
        }

        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
	    {
	        {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
	        {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
	    };

    }

}
