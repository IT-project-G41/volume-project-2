using System;

namespace octTree
{
    public class OctTreeInt
    {
        private readonly OctTreeNodeInt root;

        public OctTreeInt(int[] baseData, int x, int y, int z)
        {
            root = OctTreeNodeInt.BuildOctTree(x, y, z, baseData);
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Collections.Generic.Queue<OctTreeNodeInt> levelOrderedQueue = new System.Collections.Generic.Queue<OctTreeNodeInt>();

            // add the root to the queue
            levelOrderedQueue.Enqueue(root);

            // loop though all of the nodes on the tree
            while(levelOrderedQueue.Count > 0)
            {
                // get the current object to view the toString of
                OctTreeNodeInt current = levelOrderedQueue.Dequeue();

                // collect all of the children
                for(int index = 0; index < current.Children.Length; index++)
                {
                    if (current.Children[index] != null)
                        levelOrderedQueue.Enqueue(current.Children[index]);
                }

                // append the currents to string before moving on
                sb.Append(current.ToString());
            }

            // to string 
            return sb.ToString();
        }

        class OctTreeNodeInt
        {
            public int Count;

            public int Level;

            public int MaxLevel;

            public int Average = 0;

            public int Max = int.MinValue;

            public int Min = int.MaxValue;

            public OctTreeNodeInt[] Children = new OctTreeNodeInt[0];

            public int minX, minY, minZ;

            public int maxX, maxY, maxZ;

            public static OctTreeNodeInt BuildOctTree(int x, int y, int z, int[] data)
            {
                // create a complet new tree by recucivly creating a tree node
                return new OctTreeNodeInt(0, 0, 0, x, y, z, data, z, y, z, 0);
            }

            private OctTreeNodeInt(int minX, int minY, int minZ, int maxX, int maxY, int maxZ, int[] data, int originalX, int originalY, int OriginalZ, int level)
            {
                // save the current cords
                this.minX = minX;
                this.maxX = maxX;
                this.minY = minY;
                this.maxY = maxY;
                this.minZ = minZ;
                this.maxZ = maxZ;

                // counter
                Count = (maxX - minX) * (maxY - minY) * (maxZ - minZ);
                this.Level = level;

                // depending on the size of the argument will change what this will do
                if (Count <= 1)
                {
                    // create a leaf node for this object
                    InitalizeLeafNode(minX, minY, minZ, data, originalX, originalY, OriginalZ, level + 1);
                }
                else
                {
                    // create leaf nodes for all instances
                    Children = new OctTreeNodeInt[8];

                    int changeX = UnityEngine.Mathf.CeilToInt((maxX - minX) / 2f);
                    int changeY = UnityEngine.Mathf.CeilToInt((maxY - minY) / 2f);
                    int changeZ = UnityEngine.Mathf.CeilToInt((maxZ - minZ) / 2f);

                    if (changeX < 0) changeX = 0;
                    if (changeY < 0) changeY = 0;
                    if (changeZ < 0) changeZ = 0;

                    // create all of the children objects
                    if (calcSize(minX, minY, minZ, maxX - changeX, maxY - changeY, maxZ - changeZ) > 0)
                        Children[0] = new OctTreeNodeInt(minX, minY, minZ, maxX - changeX, maxY - changeY, maxZ - changeZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(minX, maxY - changeY, minZ, maxX - changeX, maxY, maxZ - changeZ)> 0)
                        Children[1] = new OctTreeNodeInt(minX, maxY - changeY, minZ, maxX - changeX, maxY, maxZ - changeZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, minY, minZ, maxX, maxY - changeY, maxZ - changeZ) > 0)
                        Children[2] = new OctTreeNodeInt(maxX - changeX, minY, minZ, maxX, maxY - changeY, maxZ - changeZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, maxY - changeY, minZ, maxX, maxY, maxZ - changeZ) > 0)
                        Children[3] = new OctTreeNodeInt(maxX - changeX, maxY - changeY, minZ, maxX, maxY, maxZ - changeZ, data, originalX, originalY, OriginalZ, level + 1);

                    if (calcSize(minX, minY, maxZ - changeZ, maxX - changeX, maxY - changeY, maxZ) > 0)
                        Children[4] = new OctTreeNodeInt(minX, minY, maxZ - changeZ, maxX - changeX, maxY - changeY, maxZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(minX, maxY - changeY, maxZ - changeZ, maxX - changeX, maxY, maxZ) > 0)
                        Children[5] = new OctTreeNodeInt(minX, maxY - changeY, maxZ - changeZ, maxX - changeX, maxY, maxZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, minY, maxZ - changeZ, maxX, maxY - changeY, maxZ) > 0)
                        Children[6] = new OctTreeNodeInt(maxX - changeX, minY, maxZ - changeZ, maxX, maxY - changeY, maxZ, data, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, maxY - changeY, maxZ - changeZ, maxX, maxY, maxZ) > 0)
                        Children[7] = new OctTreeNodeInt(maxX - changeX, maxY - changeY, maxZ - changeZ, maxX, maxY, maxZ, data, originalX, originalY, OriginalZ, level + 1);

                    InializeNode();
                }
            }

            private static int calcSize(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
            {
                return (maxX - minX) * (maxY - minY) * (maxZ - minZ);
            }

            private void InitalizeLeafNode(int x, int y, int z, int[] data, int originalX, int originalY, int OriginalZ, int level)
            {
                // get the data point
                int index = this.Get(x, y, z, originalX, originalY, OriginalZ);

                // set all the values to the data entries value
                this.Max = data[index];
                this.Min = data[index];
                this.Average = data[index];

                // set the tree data
                this.MaxLevel = this.Level;
            }

            private void InializeNode()
            {
                for (int index = 0; index < Children.Length; index++)
                {
                    if (Children[index] != null)
                    {
                        // set the max value found
                        if (Children[index].Max > this.Max)
                        {
                            this.Max = Children[index].Max;
                        }

                        // set the min value found
                        if (Children[index].Min < this.Min)
                        {
                            this.Min = Children[index].Min;
                        }

                        // get values for the averate
                        this.Average += Children[index].Average;

                        // set the max value found
                        if (Children[index].MaxLevel > this.MaxLevel)
                        {
                            this.MaxLevel = Children[index].MaxLevel;
                        }
                    }
                }

                // finalize the average
                this.Average /= this.Children.Length;
            }

            public int Get(int x, int y, int z, int originalX, int originalY, int OriginalZ)
            {
                return (z * (originalY * originalX)) + (y * originalX) + x;
            }

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{");
                sb.Append("level: ");
                sb.Append(Level);
                sb.Append(",Min: ");
                sb.Append(minX);
                sb.Append(",");
                sb.Append(minY);
                sb.Append(",");
                sb.Append(minZ);
                sb.Append(",");
                sb.Append("Max: ");
                sb.Append(maxX);
                sb.Append(",");
                sb.Append(maxY);
                sb.Append(",");
                sb.Append(maxZ);
                sb.Append(",");
                sb.Append("Count: ");
                sb.Append(this.Count);
                sb.AppendLine("}");
                return sb.ToString();
            }
        }
    }

    public class OctTreeMedicalData
    {
        private readonly OctTreeNodeMedicalData root;

        public OctTreeMedicalData(uint[] ImageBuffer, DicomGrid.TypeOfVoxel[] types, int largestSegmentID, int[][] RelatedSegments, int x, int y, int z)
        {
            root = OctTreeNodeMedicalData.BuildOctTree(x, y, z, ImageBuffer, types, largestSegmentID, RelatedSegments);
        }

        public Point3D FindClosestBorderPos(int x, int y, int z)
        {
            return this.root.FindClosestBorderPosition(x, y, z);
        }

        public Point3D FindClosestBorderPos(int x, int y, int z, UnityEngine.Vector3 correction)
        {
            return this.root.FindClosestBorderPosition(x, y, z, correction);
        }

        /// <summary>
        /// Runs a level based search on Oct tree
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Collections.Generic.Queue<OctTreeNodeMedicalData> levelOrderedQueue = new System.Collections.Generic.Queue<OctTreeNodeMedicalData>();

            // add the root to the queue
            levelOrderedQueue.Enqueue(root);

            // loop though all of the nodes on the tree
            while (levelOrderedQueue.Count > 0)
            {
                // get the current object to view the toString of
                OctTreeNodeMedicalData current = levelOrderedQueue.Dequeue();

                // collect all of the children
                for (int index = 0; index < current.Children.Length; index++)
                {
                    if (current.Children[index] != null)
                        levelOrderedQueue.Enqueue(current.Children[index]);
                }

                // append the currents to string before moving on
                sb.Append(current.ToString());
            }

            // to string 
            return sb.ToString();
        }

        class OctTreeNodeMedicalData
        {
            public int Count;

            public int Level;

            public int MaxLevel;

            public uint Average = 0;

            public uint Max = uint.MinValue;

            public uint Min = uint.MaxValue;

            public OctTreeNodeMedicalData[] Children = new OctTreeNodeMedicalData[0];

            public int minX, minY, minZ;

            public int maxX, maxY, maxZ;

            public int amountOfBorderTypes = 0, amountOfInsideTypes = 0, amountOfOutsideTypes = 0;
            //public int amountOfLargest;

            //public float chanceOfBorder, chanceOfInside, chanceOfOutside, chanceOfLargest;

            //public int[] regions = new int[0];

            public static OctTreeNodeMedicalData BuildOctTree(int x, int y, int z, uint[] data, DicomGrid.TypeOfVoxel[] types, int largestSegmentID, int[][] relatedSegments)
            {
                // create a complet new tree by recucivly creating a tree node
                return new OctTreeNodeMedicalData(0, 0, 0, x, y, z, data, types, largestSegmentID, z, y, z, 0);
            }

            private OctTreeNodeMedicalData(int minX, int minY, int minZ, int maxX, int maxY, int maxZ, uint[] data, DicomGrid.TypeOfVoxel[] types, int largestSegmentID, int originalX, int originalY, int OriginalZ, int level)
            {
                // save the current cords
                this.minX = minX;
                this.maxX = maxX;
                this.minY = minY;
                this.maxY = maxY;
                this.minZ = minZ;
                this.maxZ = maxZ;

                // counter
                Count = (maxX - minX) * (maxY - minY) * (maxZ - minZ);
                this.Level = level;

                // depending on the size of the argument will change what this will do
                if (Count <= 1)
                {
                    // create a leaf node for this object
                    InitalizeLeafNode(minX, minY, minZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                }
                else
                {
                    // create leaf nodes for all instances
                    Children = new OctTreeNodeMedicalData[8];

                    int changeX = UnityEngine.Mathf.CeilToInt((maxX - minX) / 2f);
                    int changeY = UnityEngine.Mathf.CeilToInt((maxY - minY) / 2f);
                    int changeZ = UnityEngine.Mathf.CeilToInt((maxZ - minZ) / 2f);

                    if (changeX < 0) changeX = 0;
                    if (changeY < 0) changeY = 0;
                    if (changeZ < 0) changeZ = 0;

                    // create all of the children objects
                    if (calcSize(minX, minY, minZ, maxX - changeX, maxY - changeY, maxZ - changeZ) > 0)
                        Children[0] = new OctTreeNodeMedicalData(minX, minY, minZ, maxX - changeX, maxY - changeY, maxZ - changeZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(minX, maxY - changeY, minZ, maxX - changeX, maxY, maxZ - changeZ) > 0)
                        Children[1] = new OctTreeNodeMedicalData(minX, maxY - changeY, minZ, maxX - changeX, maxY, maxZ - changeZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, minY, minZ, maxX, maxY - changeY, maxZ - changeZ) > 0)
                        Children[2] = new OctTreeNodeMedicalData(maxX - changeX, minY, minZ, maxX, maxY - changeY, maxZ - changeZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, maxY - changeY, minZ, maxX, maxY, maxZ - changeZ) > 0)
                        Children[3] = new OctTreeNodeMedicalData(maxX - changeX, maxY - changeY, minZ, maxX, maxY, maxZ - changeZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);

                    if (calcSize(minX, minY, maxZ - changeZ, maxX - changeX, maxY - changeY, maxZ) > 0)
                        Children[4] = new OctTreeNodeMedicalData(minX, minY, maxZ - changeZ, maxX - changeX, maxY - changeY, maxZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(minX, maxY - changeY, maxZ - changeZ, maxX - changeX, maxY, maxZ) > 0)
                        Children[5] = new OctTreeNodeMedicalData(minX, maxY - changeY, maxZ - changeZ, maxX - changeX, maxY, maxZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, minY, maxZ - changeZ, maxX, maxY - changeY, maxZ) > 0)
                        Children[6] = new OctTreeNodeMedicalData(maxX - changeX, minY, maxZ - changeZ, maxX, maxY - changeY, maxZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);
                    if (calcSize(maxX - changeX, maxY - changeY, maxZ - changeZ, maxX, maxY, maxZ) > 0)
                        Children[7] = new OctTreeNodeMedicalData(maxX - changeX, maxY - changeY, maxZ - changeZ, maxX, maxY, maxZ, data, types, largestSegmentID, originalX, originalY, OriginalZ, level + 1);

                    InializeNode();
                }
            }

            private static int calcSize(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
            {
                return (maxX - minX) * (maxY - minY) * (maxZ - minZ);
            }

            private void InitalizeLeafNode(int x, int y, int z, uint[] data, DicomGrid.TypeOfVoxel[] types, int largestSegmentID, int originalX, int originalY, int OriginalZ, int level)
            {
                // get the data point
                int index = this.Get(x, y, z, originalX, originalY, OriginalZ);

                // set all the values to the data entries value
                this.Max = data[index];
                this.Min = data[index];
                this.Average = data[index];

                // set the tree data
                this.MaxLevel = this.Level;

                switch (types[index])
                {
                    case DicomGrid.TypeOfVoxel.border:
                        this.amountOfBorderTypes = 1;
                        break;
                    case DicomGrid.TypeOfVoxel.inside:
                        this.amountOfInsideTypes = 1;
                        break;
                    case DicomGrid.TypeOfVoxel.outside:
                        this.amountOfOutsideTypes = 1;
                        break;
                }

                this.maxX = x;
                this.maxY = y;
                this.maxZ = z;
                this.minX = x;
                this.minY = y;
                this.minZ = z;
            }

            private void InializeNode()
            {
                for (int index = 0; index < Children.Length; index++)
                {
                    if (Children[index] != null)
                    {
                        // set the max value found
                        if (Children[index].Max > this.Max)
                        {
                            this.Max = Children[index].Max;
                        }

                        // set the min value found
                        if (Children[index].Min < this.Min)
                        {
                            this.Min = Children[index].Min;
                        }

                        // get values for the average
                        this.Average += Children[index].Average;

                        // set the max value found
                        if (Children[index].MaxLevel > this.MaxLevel)
                        {
                            this.MaxLevel = Children[index].MaxLevel;
                        }

                        // Get the info for the different types
                        this.amountOfBorderTypes += Children[index].amountOfBorderTypes;
                        this.amountOfInsideTypes += Children[index].amountOfInsideTypes;
                        this.amountOfOutsideTypes += Children[index].amountOfOutsideTypes;

                        // work out bounding box
                        if (Children[index].maxX > this.maxX)
                        {
                            this.maxX = Children[index].maxX;
                        }
                        if (Children[index].maxY > this.maxY)
                        {
                            this.maxY = Children[index].maxY;
                        }
                        if (Children[index].maxZ > this.maxZ)
                        {
                            this.maxZ = Children[index].maxZ;
                        }
                        if (Children[index].minX < this.minX)
                        {
                            this.minX = Children[index].minX;
                        }
                        if (Children[index].minY > this.minY)
                        {
                            this.minY = Children[index].minY;
                        }
                        if (Children[index].minZ > this.minZ)
                        {
                            this.minZ = Children[index].minZ;
                        }
                    }
                }

                // finalize the average
                this.Average /= System.Convert.ToUInt32(this.Children.Length);
            }

            public int Get(int x, int y, int z, int originalX, int originalY, int OriginalZ)
            {
                return (z * (originalY * originalX)) + (y * originalX) + x;
            }

            public Point3D GetClosestPoint(int x, int y, int z)
            {
                int ax, ay, az;

                if (x > this.maxX)
                {
                    ax = this.maxX;
                }
                else if (x < this.minX)
                {
                    ax = this.minX;
                }
                else
                {
                    ax = x;
                }

                if (y > this.maxY)
                {
                    ay = this.maxY;
                }
                else if (y < this.minY)
                {
                    ay = this.minY;
                }
                else
                {
                    ay = y;
                }

                if (z > this.maxZ)
                {
                    az = this.maxZ;
                }
                else if (z < this.minZ)
                {
                    az = this.minZ;
                }
                else
                {
                    az = z;
                }
            
                return new Point3D(ax, ay, az);
            }


            public Point3D FindClosestBorderPosition(int x, int y, int z, Point3D closest = null)
            {
                return FindClosestBorderPosition(x, y, z, new UnityEngine.Vector3(1, 1, 1), closest);
            }

            public Point3D FindClosestBorderPosition(int x, int y, int z, UnityEngine.Vector3 correction, Point3D closest = null)
            {
                Point3D point = new Point3D(x, y, z);

                // leaf node behaviour
                if (Children == null || Children.Length == 0)
                {
                    Point3D thisPoint = new Point3D(this.maxX, this.maxY, this.maxZ);
                    
                    if (closest == null)
                    {
                        return thisPoint;
                    }
                    else
                    {
                        if (thisPoint.Distance(point) < closest.Distance(point, correction))
                        {
                            return thisPoint;
                        }
                        else
                        {
                            return closest;
                        }
                    }
                }

                int index = 0;

                // below is the behaviour for the rest of the tree
                DistanceValue[] valid = new DistanceValue[8];

                DistanceValue Closest = null;

                // find the first value
                for (; index++ < Children.Length; index++)
                {
                    if (Children[index] != null)
                    {
                        Closest = new DistanceValue(
                            index,
                            Children[0].GetClosestPoint(x, y, z).Distance(point, correction)
                        );

                        break;
                    }
                }

                // if a value is not set then treat this like a leaf node
                if (closest == null)
                {
                    Point3D thisPoint = new Point3D(this.maxX, this.maxY, this.maxZ);

                    if (closest == null)
                    {
                        return thisPoint;
                    }
                    else
                    {
                        if (thisPoint.Distance(point) < closest.Distance(point, correction))
                        {
                            return thisPoint;
                        }
                        else
                        {
                            return closest;
                        }
                    }
                }

                index++;

                // look for the closest valid object and then look look though the children to work out a logical order
                for (; index < this.Children.Length; index++)
                {
                    if (Children[index] != null && Children[index].amountOfBorderTypes > 0)
                    {

                        // get the sorting data out for the objects
                        DistanceValue other =
                            new DistanceValue(
                               index,
                               Children[index].GetClosestPoint(x, y, z).Distance(point, correction)
                               );

                        // save all the closest and focus on the rest later
                        if (Closest.distance > other.distance)
                        {
                            valid[index] = Closest;
                            Closest = other;
                        }
                        else
                        {
                            valid[index] = other;
                        }
                    }
                }

                // search via the closest index
                Point3D closestPoint = Children[Closest.index].FindClosestBorderPosition(x, y, z);
                float dist = closestPoint.Distance(point);
                
                // sort the array so the closer checks get pirority
                System.Array.Sort<DistanceValue>(valid, 
                    new System.Comparison<DistanceValue>((i1, i2) => i2.distance.CompareTo(i1.distance)));

                for (int i = 0; i < valid.Length; i++)
                {
                    // if the value has been set to a real value
                    if (valid[i] != null)
                    {
                        // if the if it is possible there is a closer match
                        if (valid[i].distance < dist)
                        {
                            Point3D searchResult = Children[valid[i].index].FindClosestBorderPosition(x, y, z);
                            float oDist = searchResult.Distance(point);

                            // if we found a closer point set the new point then continue
                            if (oDist < dist)
                            {
                                closestPoint = searchResult;
                                dist = oDist;
                            }
                        }
                    }
                }

                return closestPoint;
            }

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{");
                sb.Append("level: ");
                sb.Append(Level);
                sb.Append(",Min: ");
                sb.Append(minX);
                sb.Append(",");
                sb.Append(minY);
                sb.Append(",");
                sb.Append(minZ);
                sb.Append(",");
                sb.Append("Max: ");
                sb.Append(maxX);
                sb.Append(",");
                sb.Append(maxY);
                sb.Append(",");
                sb.Append(maxZ);
                sb.Append(",");
                sb.Append("Count: ");
                sb.Append(this.Count);
                sb.AppendLine("}");
                return sb.ToString();
            }
        }
    }

    class DistanceValue
    {
        public int index = -1;
        public float distance = -1f;

        public DistanceValue(int index, float dist)
        {
            this.index = index;
            this.distance = dist;
        }


    }

    public class Point3D
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        public Point3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public UnityEngine.Vector3Int ToVector3Int()
        {
            return new UnityEngine.Vector3Int(this.x, y, z);
        }

        public float Distance(Point3D point)
        {
            return UnityEngine.Vector3Int.Distance(this.ToVector3Int(), point.ToVector3Int());
        }

        public float Distance(Point3D point, UnityEngine.Vector3 correction)
        {
            return UnityEngine.Vector3.Distance(this.Muli(correction), point.Muli(correction));
        }

        private UnityEngine.Vector3 Muli(UnityEngine.Vector3 other)
        {
            return new UnityEngine.Vector3(
                    this.x * other.x,
                    this.y * other.y,
                    this.z * other.z
                );
        }
    }
}

