using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DicomGrid
{
    #region veribles
    /// <summary>
    /// The phyical location of the slices in the image.
    /// This can be used to tell the distance that exists between slices
    /// and how the slices should be ordered
    /// </summary>
    public float[] sliceLocations;

    /// <summary>
    /// The width of the grid in voxels
    /// </summary>
    public int width;

    /// <summary>
    /// the height of the grid in voxels
    /// </summary>
    public int height;

    /// <summary>
    /// the breath of the grid in voxels
    /// </summary>
    public int breath;

    /// <summary>
    /// This tells the system how much space is inbetween the slices by millimeters
    /// </summary>
    public float spacingBetweenSlices;

    /// <summary>
    /// The thinkness of the slice in Millimeters
    /// </summary>
    public float sliceThickness;

    /// <summary>
    /// The size of each voxel 
    /// </summary>
    public float pixelSpacingX;

    /// <summary>
    /// The size of each voxel
    /// </summary>
    public float pixelSpacingY;

    /// <summary>
    /// A generic identifier that all the dicoms should have in common
    /// </summary>
    public string frameOfReferenceId;

    /// <summary>
    /// this is the buffer object it holds all of the voxel data that is stored within
    /// the image
    /// </summary>
    public uint[] buffer;

    #endregion

    #region bufferBasedCommands

    // tells the system if this object was created successfully
    public bool Exists()
    {
        return this.buffer == null;
    }

    /// <summary>
    /// The amount of pixels there are in the buffer
    /// </summary>
    /// <returns>0</returns>
    public int Count()
    {
        if (this.buffer != null)
            return this.buffer.Length;
        else
            return 0;
    }

    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Min()
    {
        uint min = uint.MaxValue;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] < min)
            {
                min = buffer[index];
            }
        }

        return min;
    }

    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Max()
    {
        uint max = uint.MinValue;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] > max)
            {
                max = buffer[index];
            }
        }

        return max;
    }



    /// <summary>
    /// finds the smallest value in the dicom grid buffer object
    /// </summary>
    /// <returns>a unit that represents the minium value</returns>
    public uint Avg()
    {
        uint avg = 0;

        for (int index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] > avg)
            {
                avg = (avg + buffer[index]) / 2;
            }
        }

        return avg;
    }

    #endregion

    #region olderDemoCode

    public int GetThicknessValue()
    {
        return (int)Math.Round(sliceThickness);
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getXYResultion()
    {
        return new float[] {
            width,
            height
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>
    /// A float represent real world distance of the Z axis
    /// s</returns>
    public float PhysicalDistanceAlongZ()
    {
        return breath * GetThicknessValue();
    }

    public int[] GetXYSliceImage(int zIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (zIndex < 0 || zIndex >= breath) return null;

        // get the out put array intialized
        int[] output = new int[height * width];

        // set the starting point for the output array
        int indexOuter = zIndex * output.Length;

        // just set up the value verible that will dictate the value of the element
        byte value = 0;

        // loop though the arrays and set the values as needed
        for (int index = 0; index < output.Length; index++, indexOuter++)
        {
            if (buffer[indexOuter] < lowValue)
                value = 0;
            else if (buffer[indexOuter] > highValue)
                value = 255;
            else
                value = (byte)Math.Round((buffer[indexOuter] - lowValue) / (double)highValue * 255);

            output[index] = ((255 - value) << 24) + (value << 16) + (value << 8) + value;
        }

        return output;
    }

    public Color32[] GetXYSliceColorArray(int zIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (zIndex < 0 || zIndex >= breath) return null;

        // get the out put array intialized
        Color32[] output = new Color32[height * width];

        // set the starting point for the output array
        int indexOuter = zIndex * output.Length;

        // just set up the value verible that will dictate the value of the element
        byte value = 0;

        // loop though the arrays and set the values as needed
        for (int index = 0; index < output.Length; index++, indexOuter++)
        {
            if (buffer[indexOuter] < lowValue)
                value = 0;
            else if (buffer[indexOuter] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[indexOuter] - lowValue) / (double)highValue) * 255));

            output[index] = new Color32(value, value, value, (byte)(255 - value));
        }

        return output;
    }

    public int[] GetZYSliceImage(int xIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (xIndex < 0 || xIndex > width) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the out put array intialized
        int[] output = new int[height * breath * thinknessValue];

        // the amount to grow the array by each round
        int incrementalAmount = width * height;

        int zIndex = xIndex;
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            if (buffer[zIndex] < lowValue)
                value = 0;
            else if (buffer[zIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[zIndex] - highValue) / (double)lowValue) * 255));

            int colorVal = ((255 - value) << 24) + (value << 16) + (value << 8) + value;

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            zIndex += incrementalAmount;
            if (zIndex >= buffer.Length)
            {
                xIndex += width;
                zIndex = xIndex;
            }
        }

        return output;
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getZYResultion()
    {
        return new float[] {
            breath * GetThicknessValue(),
            height
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>
    /// A float represent real world distance of the X axis
    /// s</returns>
    public float PhysicalDistanceAlongX()
    {
        return width;
    }

    public Color32[] GetZYSliceImageColorArray(int xIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (xIndex < 0 || xIndex > width) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the out put array intialized
        Color32[] output = new Color32[height * breath * thinknessValue];

        // the amount to grow the array by each round
        int incrementalAmount = width * height;

        int zIndex = xIndex;
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            if (buffer[zIndex] < lowValue)
                value = 0;
            else if (buffer[zIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[zIndex] - highValue) / (double)lowValue) * 255));

            Color32 colorVal = new Color32((byte)(255 - value), value, value, value);

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            zIndex += incrementalAmount;
            if (zIndex >= buffer.Length)
            {
                xIndex += width;
                zIndex = xIndex;
            }
        }

        return output;
    }

    public int[] GetXZSliceImage(int yIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (yIndex < 0 || yIndex > height) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the output array intialized
        int[] output = new int[width * breath * thinknessValue];

        // get the note the starting location of the first index
        int xIndex = yIndex * width;

        // the amount to grow the array by each round
        int carriageReturn = xIndex;

        int incrementalValue = width * height;

        // veribles that will be itterated on thoughout the loop
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            //Console.WriteLine(buffer[yIndex]);
            // get the color out of the array
            if (buffer[yIndex] < lowValue)
                value = 0;
            else if (buffer[yIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round(((buffer[yIndex] - highValue) / (double)lowValue) * 255));

            int colorVal = ((255 - value) << 24) + (value << 16) + (value << 8) + value;

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            // increment the rest of the indexes as required
            yIndex += incrementalValue;
            if (yIndex >= buffer.Length)
            {
                xIndex++;
                yIndex = xIndex;
            }
        }

        return output;
    }

    /// <summary>
    /// This function returns an array with the size in mm of the 
    /// resultion frame of the image
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] getXZResultion()
    {
        return new float[] {
            width,
            breath * GetThicknessValue()
        };
    }

    /// <summary>
    /// returns the amount of distance the object will need to travel in mm
    /// </summary>
    /// <returns>a float represent real world distance of the Y axis</returns>
    public float PhysicalDistanceAlongY()
    {
        return height;
    }

    public Color32[] GetXZSliceImageColorArray(int yIndex, int highValue, int lowValue)
    {
        // if invalid parmaters are addded return null
        if (yIndex < 0 || yIndex > height) return null;

        // this has the amount of pixels each pixel is worth from this persepective
        int thinknessValue = GetThicknessValue();

        // get the output array intialized
        Color32[] output = new Color32[width * breath * thinknessValue];

        // get the note the starting location of the first index
        int xIndex = yIndex * width;

        // the amount to grow the array by each round
        int carriageReturn = xIndex;

        int incrementalValue = width * height;

        // veribles that will be itterated on thoughout the loop
        int index = 0;
        byte value = 0;

        while (index < output.Length)
        {
            //Console.WriteLine(buffer[yIndex]);
            // get the color out of the array
            if (buffer[yIndex] < lowValue)
                value = 0;
            else if (buffer[yIndex] > highValue)
                value = 255;
            else
                value = (byte)(Math.Round((buffer[yIndex] - highValue) / (double)lowValue) * 255);

            Color32 colorVal = new Color32((byte)(255 - value), value, value, value);

            for (int i = 0; i < thinknessValue; i++)
                output[index++] = colorVal;

            // increment the rest of the indexes as required
            yIndex += incrementalValue;
            if (yIndex >= buffer.Length)
            {
                xIndex++;
                yIndex = xIndex;
            }
        }

        return output;
    }

    #endregion

    public string GetJSON()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    #region 3DImageBufferCode

    public Vector3 GetPosition(int index)
    {
        return new Vector3(index % height, (index / width) % height, index / (width * height));
    }

    public Vector3Int GetPositionAsInt(int index)
    {
        return new Vector3Int(index % height, (index / width) % height, index / (width * height));
    }

    public uint Get(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
            return 0;

        try
        {
            return this.buffer[(z * (height * width)) + (y * width) + x];
        }
        catch (Exception)
        {
            Debug.LogWarning(new Vector3(x, y, z));
        }
        return 0;

    }

    public int GetIndex(Vector3 position)
    {
        return Mathf.RoundToInt((position.z * (height * width)) + (position.y * width) + position.x);
    }

    public int GetIndex(Vector3Int position)
    {
        return Mathf.RoundToInt((position.z * (height * width)) + (position.y * width) + position.x);
    }

    public int GetIndex(int x, int y, int z)
    {
        return (z * (height * width)) + (y * width) + x;
    }

    public uint Get(Vector3 position)
    {
        return this.Get(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
    }

    #endregion

    #region BreathFirstSearch

    /// <summary>
    /// Brute force search for the closest point
    /// </summary>
    /// <param name="position"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public float MiniumDistanceToTheExit(Vector3 position, uint tolerance)
    {
        position = new Vector3(position.x * width, position.y * height, position.z * breath);

        float smallestDistance = 999999f;
        float temp;

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        temp = Vector3.Distance(new Vector3(x, y, z), position);

                        if (smallestDistance > temp)
                        {
                            smallestDistance = temp;
                        }
                    }
                }

        if (smallestDistance < 0.001)
            smallestDistance = 0.0001f;

        return smallestDistance;
    }

    public KeyValuePair<float, Vector3> MiniumDistanceAndPosToTheExit(Vector3 position, uint tolerance)
    {
        position = new Vector3(position.x * width, position.y * height, position.z * breath);

        KeyValuePair<float, Vector3> result = new KeyValuePair<float, Vector3>();
        float smallestDistance = 999999f;
        float temp;

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        temp = Vector3.Distance(new Vector3(x, y, z), position);
                        if (temp < smallestDistance)
                        {
                            smallestDistance = temp;
                            result = new KeyValuePair<float, Vector3>(smallestDistance, new Vector3((float)x / (float)this.width, (float)y / (float)this.height, (float)z / (float)this.breath));
                        }
                    }
                }

        if (smallestDistance < 0.001)
            smallestDistance = 0.0001f;

        return result;
    }


    // Writes the closest point to a database
    public ClosetPointOnVolumeData[] BuildMinimalDistanceDataBase(Vector3 position, uint tolerance)
    {
        // work out the orgin info
        Vector3 RPos = position;
        position = new Vector3(position.x * width, position.y * height, position.z * breath);
        Vector3Int vPos = new Vector3Int((int)Math.Round(position.x), (int)Math.Round(position.y), (int)Math.Round(position.z));

        // create a data stucture where you can hold that data
        List<ClosetPointOnVolumeData> result = new List<ClosetPointOnVolumeData>();

        for (int z = 0; z < this.breath; z++)
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                {
                    if (this.Get(x, y, z) < tolerance)
                    {
                        result.Add(new ClosetPointOnVolumeData(
                        new Vector3Int(x, y, z),
                        new Vector3((float)x / (float)this.width, (float)y / (float)this.height, (float)z / (float)this.breath),
                        Vector3.Distance(new Vector3(x, y, z), position),
                        vPos,
                        RPos
                        ));
                    }
                }

        // format the array to  be inserted
        return result.ToArray();
    }

    public Vector3? GetClosestPointWithAThreashold(Vector3 start, uint greaterThan, uint lessThan = uint.MaxValue)
    {
        return GetClosestPointWithAThreashold(start, new Vector3(1, 1, 1), greaterThan, lessThan);
    }

    public static Vector3 Multi(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(
            lhs.x * rhs.x,
            lhs.y * rhs.y,
            lhs.z * rhs.z
            );
    }

   public Vector3? GetClosestPointWithAThreashold(Vector3 start, Vector3 correction, uint greaterThan, uint lessThan = uint.MaxValue)
    {
        Queue<Vector3> toSee = new Queue<Vector3>();
        bool[] seen = new bool[this.width * this.breath * this.height];
        float ClosestPointDistance = float.MaxValue;
        Vector3? ClosetPoint = null;

        toSee.Enqueue(start);

        do
        {
            // Get and move the current point of memory
            Vector3 current = toSee.Dequeue();
            float dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));

            if (dist < ClosestPointDistance && dist > 0.5)
            {
                // add all of the naubors to the to see list
                // or else it will see if they are closer than the currrent closest
                Vector3 next;
                if (current.x < this.width)
                {
                    next = current + new Vector3(1, 0, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            } 
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.x > 0)
                {
                    next = current - new Vector3(1, 0, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }

                if (current.y < this.height)
                {
                    next = current + new Vector3(0, 1, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                            seen[this.GetIndex(next)] = true;
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.y > 0)
                {
                    next = current - new Vector3(0, 1, 0);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.z < this.breath)
                {
                    next = current + new Vector3(0, 0, 1);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }


                if (current.z > 0)
                {
                    next = current - new Vector3(0, 0, 1);
                    if (this.definition[this.GetIndex(next)] == TypeOfVoxel.inside)
                    {
                        uint v = this.Get(next);
                        if (v > greaterThan && v < lessThan)
                        {
                            seen[this.GetIndex(next)] = true;
                            toSee.Enqueue(next);
                        }
                        else if (!seen[this.GetIndex(next)])
                        {
                            dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                            if (dist > 0.5f && ClosestPointDistance > dist)
                            {
                                ClosestPointDistance = dist;
                                ClosetPoint = current;
                                seen[this.GetIndex(next)] = true;
                            }
                        }
                    }
                    else if (!seen[this.GetIndex(next)])
                    {
                        dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                        if (dist > 0.5f && ClosestPointDistance > dist)
                        {
                            ClosestPointDistance = dist;
                            ClosetPoint = current;
                            seen[this.GetIndex(next)] = true;
                        }
                    }
                }
                else
                {
                    dist = Vector3.Distance(Multi(start, correction), Multi(current, correction));
                    if (dist > 0.5f && ClosestPointDistance > dist)
                    {
                        ClosestPointDistance = dist;
                        ClosetPoint = current;
                    }
                }
            }

        } while (toSee.Count > 0);

        return ClosetPoint;
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolume(Vector3 start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3> toSee = new Queue<Vector3>();

        toSee.Enqueue(start);

        do
        {
            // Get and move the current point of memory
            Vector3 current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3 next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3(1, 0, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3(1, 0, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3(0, 1, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3(0, 1, 0);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3(0, 0, 1);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3(0, 0, 1);
                DetermineSegment(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    private void DetermineSegment(Vector3 next, int segmentVolume, ref bool[] seen, ref Queue<Vector3> toSee)
    {
        int i = this.GetIndex(next);

        // if we haven't seen it then append it
        if (seen[i])
        {
            seen[i] = false;
            toSee.Enqueue(next);

            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
        else if (definition[i] == TypeOfVoxel.border)
        {
            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
    }



    private void DetermineSegmentInt(Vector3Int next, int segmentVolume, ref bool[] seen, ref Queue<Vector3Int> toSee)
    {
        int i = this.GetIndex(next.x, next.y, next.z);

        // if we haven't seen it then append it
        if (seen[i])
        {
            seen[i] = false;
            toSee.Enqueue(next);

            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
        else if (definition[i] == TypeOfVoxel.border)
        {
            RelatedSegments[i] = ArrayAppend(segmentVolume, RelatedSegments[i]);
        }
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeInt(Vector3 start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3Int> toSee = new Queue<Vector3Int>();
        int x = Mathf.RoundToInt(start.x);
        int y = Mathf.RoundToInt(start.y);
        int z = Mathf.RoundToInt(start.z);

        toSee.Enqueue(new Vector3Int(x, y, z));

        do
        {
            // Get and move the current point of memory
            Vector3Int current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeInt(Vector3Int start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3Int> toSee = new Queue<Vector3Int>();
        int x = start.x;
        int y = start.y;
        int z = start.z;

        toSee.Enqueue(new Vector3Int(x, y, z));

        do
        {
            // Get and move the current point of memory
            Vector3Int current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }



    /// <summary>
    /// Used to work out what volumes are attached to this volume
    /// </summary>
    /// <param name="start">the start of the search</param>
    /// <param name="segmentVolumeId">an Id that gets placed on these</param>
    /// <param name="seen">A list of still valid pixels</param>
    /// <returns></returns>
    private int[] DetermineInteriorVolumeIntTwelveDirection(Vector3Int start, int segmentVolumeId, ref bool[] seen)
    {
        List<int> output = new List<int>();
        Queue<Vector3Int> toSee = new Queue<Vector3Int>();
        int x = start.x;
        int y = start.y;
        int z = start.z;

        toSee.Enqueue(new Vector3Int(x, y, z));

        do
        {
            // Get and move the current point of memory
            Vector3Int current = toSee.Dequeue();
            output.Add(this.GetIndex(current));

            // add all of the naubors to the to see list
            Vector3Int next;
            if (current.x < this.width - 1)
            {
                next = current + new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x > 0)
            {
                next = current - new Vector3Int(1, 0, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1)
            {
                next = current + new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y > 0)
            {
                next = current - new Vector3Int(0, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1)
            {
                next = current + new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z > 0)
            {
                next = current - new Vector3Int(0, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < this.width - 1 && current.y > 0)
            {
                next = current + new Vector3Int(1, -1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.x < this.width - 1 && current.z > 0)
            {
                next = current + new Vector3Int(1, 0, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 1, 0);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.y < this.height - 1 && current.z > 0)
            {
                next = current + new Vector3Int(0, 1, -1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1 && current.x > 0)
            {
                next = current + new Vector3Int(-1, 0, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }
            if (current.z < this.breath - 1 && current.y > 0)
            {
                next = current + new Vector3Int(0, -1, 1);
                DetermineSegmentInt(next, segmentVolumeId, ref seen, ref toSee);
            }

        } while (toSee.Count > 0);

        return output.ToArray();
    }

    #endregion

    #region segmentionParameters

    public enum TypeOfVoxel
    {
        outside = 1,
        inside = 2,
        border = 3,
        unknown = 0
    };

    public TypeOfVoxel[] definition;

    public Vector3Int[] borderPixels;

    public Vector3Int[] borderPixelsOfLargestSegment;

    public int[][] RelatedSegments;

    public int largestSegment;

    public int amountOfSegments;

    public int sizeOfLargestSegment;

    private bool segmented = false;
    public bool Segmented { get => segmented; }

    // used to create a mininmum bounding volume hyeriacty
    public Vector3 minAABB = Vector3.positiveInfinity, maxAABB = Vector3.negativeInfinity;

    #endregion

    #region segmentionBasedCode

    /// <summary>
    /// Performs a 2 pass Segmention over the dicom to
    /// segment various sets are not 
    /// </summary>
    /// <param name="tolerance"></param>
    public void SegmentDicom(uint tolerance)
    {
        // make sure this can run
        if (this.buffer == null || this.buffer.Length < 1 || segmented)
            return;


        definition = new TypeOfVoxel[this.buffer.Length];

        int index = 0;
        List<Vector3Int> border = new List<Vector3Int>();

        // find the starting point
        for (; index < this.buffer.Length; index++)
        {
            definition[index] = TypeOfVoxel.inside;
            if (index < tolerance)
            {
                definition[index] = TypeOfVoxel.outside;

                // get all of the surronding 
                Vector3 VIndex = this.GetPosition(index);

                if (index > 0)
                    SetAllOutsideToBoundry(VIndex);

                // we need to increment because we wont for a while
                index++;

                // once we have a starting point then we want to 
                break;
            }
            else
            {
                definition[index] = TypeOfVoxel.inside;
            }
        }

        // Work out what type the rest of the pixels are now we have one outside now
        for (; index < this.buffer.Length; index++)
        {
            // work out what type this object is
            definition[index] = GetTypeSixDirection(this.GetPositionAsInt(index), tolerance);
        }

        // used for later on parameters to work out differnt segments
        bool[] isInside = new bool[buffer.Length];

        index--;

        // 2nd Pass over all the pixels to ensure they where allocated correctly
        for (; index >= 0; index--)
        {
            // work out what type this object is
            definition[index] = this.GetTypeTweleveDirection(this.GetPositionAsInt(index), tolerance);

            // depending on the result then we need to filter results by a way that makes sense
            switch (definition[index])
            {
                case TypeOfVoxel.border:
                    // add the position to the border array to fit it in
                    border.Add(this.GetPositionAsInt(index));
                    isInside[index] = false;
                    break;
                case TypeOfVoxel.inside:
                    isInside[index] = true;
                    break;
                case TypeOfVoxel.outside:
                default:
                    isInside[index] = false;
                    break;
            }
        }

        // set the border pixels to the array
        //borderPixels = border.ToArray();

        // count the amount of segments that are found
        int counter = 0;

        this.RelatedSegments = new int[this.buffer.Length][];

        int increment = isInside.Length / 2;
        List<int[]> segments = new List<int[]>();
        // work out what segments belong to what segmnets
        for (index = this.buffer.Length - 1; index >= 0; index--)
        {
            int altIndex = (index + (this.buffer.Length / 2)) % this.buffer.Length;

            if (isInside[altIndex] && this.definition[altIndex] == TypeOfVoxel.inside)
            {
                // run a bfs to work detertine what pixels belong to this segmnet 
                int[] segment = DetermineInteriorVolumeIntTwelveDirection(this.GetPositionAsInt(altIndex), counter, ref isInside);

                int sizeOfSegment = segment.Length;

                if (sizeOfLargestSegment < sizeOfSegment)
                {
                    this.sizeOfLargestSegment = sizeOfSegment;
                    this.largestSegment = counter;
                }

                // set the next segment if it has more than just a small portion of data
                if (segment.Length > 8)
                {
                    segments.Add(segment);
                    counter++;
                }
            }
        }
        this.amountOfSegments = counter;

        // check the flag so this isn't called twice
        segmented = true;

        // set the border pixels to the array
        borderPixels = border.ToArray();
        Queue<Vector3Int> largest = new Queue<Vector3Int>();

        for (index = 0; index < this.borderPixels.Length; index++)
        {
            // find the AABB bounidng box for all of the border pixels
            if (minAABB.x > this.borderPixels[index].x)
                minAABB.x = this.borderPixels[index].x;
            if (minAABB.y > this.borderPixels[index].y)
                minAABB.y = this.borderPixels[index].y;
            if (minAABB.z > this.borderPixels[index].z)
                minAABB.z = this.borderPixels[index].z;
            if (maxAABB.x < this.borderPixels[index].x)
                maxAABB.x = this.borderPixels[index].x;
            if (maxAABB.y < this.borderPixels[index].y)
                maxAABB.y = this.borderPixels[index].y;
            if (maxAABB.z < this.borderPixels[index].z)
                maxAABB.z = this.borderPixels[index].z;

            int[] temp = this.RelatedSegments[this.GetIndex(this.borderPixels[index])];
            if (temp != null && ArrayContains(this.largestSegment, temp))
            {
                largest.Enqueue(this.borderPixels[index]);
            }
        }

        this.borderPixelsOfLargestSegment = largest.ToArray();

        // fix the x values I removed them becuse I was in a rush and I knew what I wanted to see from them
        // find the AABB bounidng box for all of the border pixels as percentages
        minAABB.x = minAABB.x / (float)this.width;
        minAABB.y = minAABB.y / (float)this.height;
        minAABB.z = minAABB.z / (float)this.breath;
        maxAABB.x = maxAABB.x / (float)this.width;
        maxAABB.y = maxAABB.y / (float)this.height;
        maxAABB.z = maxAABB.z / (float)this.breath;
    }

    /// <summary>
    /// Not used Currently...
    /// This was designed to work out if any segments potentially naugbored each other
    /// </summary>
    /// <param name="index">the current point in the buffer that needs to be checked</param>
    /// <returns>a list of segments that need to be merged</returns>
    private List<int> MergeNauborSegments(int index)
    {
        List<int> output = new List<int>();
        Vector3 current = this.GetPosition(index);
        Vector3 next;
        int segment = this.RelatedSegments[index][0];

        int nextIndex;
        if (current.x < this.width)
        {
            next = current + new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }

        if (current.y < this.height - 1)
        {
            next = current + new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.z < this.breath - 1)
        {
            next = current + new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (definition[nextIndex] == TypeOfVoxel.inside && segment != this.RelatedSegments[nextIndex][0])
            {
                output.AddRange(this.RelatedSegments[nextIndex]);
                ChangeValue(this.RelatedSegments[nextIndex][0], segment, this.RelatedSegments[nextIndex]);
            }
        }

        return output;
    }

    private void SetAllOutsideToBoundry(Vector3 current, int minIndex = int.MaxValue)
    {
        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width)
        {
            next = current + new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }

        if (current.y < this.height)
        {
            next = current + new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z < this.breath)
        {
            next = current + new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < minIndex)
            {
                definition[nextIndex] = TypeOfVoxel.border;
            }
        }
    }

    /// <summary>
    /// Gets teh type of the current pixel, it is either 
    /// outside, inside or within
    /// </summary>
    /// <param name="current"></param>
    /// <param name="minIndex"></param>
    private TypeOfVoxel GetTypeSixDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = this.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0)
        {
            next = current - new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.y > 0)
        {
            next = current - new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z < this.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z > 0)
        {
            next = current - new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        return output;
    }



    /// <summary>
    /// Gets teh type of the current pixel, it is either 
    /// outside, inside or within
    /// </summary>
    /// <param name="current"></param>
    /// <param name="minIndex"></param>
    private TypeOfVoxel GetTypeTweleveDirection(Vector3Int current, uint tolerance = 0, int minIndex = int.MaxValue)
    {
        // make a educated guess about the pixel we are looking at by its tolerance
        TypeOfVoxel output = TypeOfVoxel.unknown;
        bool isGreaterThanTolerance = this.Get(current) > tolerance;

        // add all of the naubors to the to see list
        Vector3 next;
        int nextIndex;
        if (current.x < this.width - 1)
        {
            next = current + new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }
        if (current.x > 0)
        {
            next = current - new Vector3Int(1, 0, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.y > 0)
        {
            next = current - new Vector3Int(0, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.z < this.breath - 1)
        {
            next = current + new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        if (current.z > 0)
        {
            next = current - new Vector3Int(0, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        else if (isGreaterThanTolerance)
        {
            // if it is inside then it should be a border since it borders on the outside
            return TypeOfVoxel.border;
        }

        // diagonal Concerns
        if (current.x > 0 && current.y > 0)
        {
            next = current - new Vector3Int(1, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.x > 0 && current.z > 0)
        {
            next = current - new Vector3Int(1, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        if (current.y > 0 && current.z > 0)
        {
            next = current - new Vector3Int(0, 1, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < this.width - 1 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(1, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < this.width - 1 && current.z < this.breath - 1)
        {
            next = current + new Vector3Int(1, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(-1, 1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x > 0 && current.z < this.breath - 1)
        {
            next = current + new Vector3Int(-1, 0, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < this.width - 1 && current.z > 0)
        {
            next = current + new Vector3Int(1, 0, -1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.x < this.width - 1 && current.y > 0)
        {
            next = current + new Vector3Int(1, -1, 0);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z > 0 - 1 && current.y < this.height - 1)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }
        if (current.z < this.breath - 1 && current.y > 0)
        {
            next = current + new Vector3Int(0, -1, 1);
            nextIndex = this.GetIndex(next);
            if (nextIndex < definition.Length && definition[nextIndex] != TypeOfVoxel.unknown)
            {
                if (isGreaterThanTolerance)
                {
                    //if (definition[nextIndex] == TypeOfVoxel.outside) // don't do anything
                    if (definition[nextIndex] == TypeOfVoxel.inside || definition[nextIndex] == TypeOfVoxel.border)
                    {
                        output = TypeOfVoxel.inside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.border;
                    }
                }
                else
                {
                    if (definition[nextIndex] == TypeOfVoxel.outside)
                    {
                        return TypeOfVoxel.outside;
                    }
                    else if (definition[nextIndex] == TypeOfVoxel.inside)
                    {
                        output = TypeOfVoxel.inside;
                    }
                }
            }
        }

        return output;
    }

    public Vector3 BruteForceGetNearestBorder(Vector3Int point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;

        for (int index = 0; index < this.borderPixels.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixels[index], point);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }

        return this.borderPixels[closest];
    }

    public Vector3 BruteForceGetNearestBorderAsPercenatage(Vector3Int point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;
        int index;
        for (index = 0; index < this.borderPixels.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixels[index], point);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }
        Debug.Log("Amount of Itterations: " + closest);
        return GetAsPercentage(this.borderPixels[closest]);
    }


    /// <summary>
    /// Should Rename...
    /// 
    /// Used as magnetic type of collision for volumetric collision. but is very slow
    /// </summary>
    /// <param name="point">
    /// A percenatage of the direction of the movement noticed by the user
    /// </param>
    /// <returns>
    /// The nearest valid point 
    /// </returns>
    public Vector3 BruteForceGetNearestBorderAsPercenatageUsingOnlyLargestBorderUsingAPointFormedAsPercenatage(Vector3 point)
    {
        int closest = 0;
        float closestDist = float.MaxValue;

        Vector3Int p = this.GetFromPercentage(point);

        for (int index = 0; index < this.borderPixelsOfLargestSegment.Length; index++)
        {
            float dist = Vector3.Distance(this.borderPixelsOfLargestSegment[index], p);

            if (closestDist > dist)
            {
                closestDist = dist;
                closest = index;
            }
        }

        return GetAsPercentage(this.borderPixels[closest]);
    }
    

    #endregion

    public Vector3 GetAsPercentage(Vector3 vector)
    {
        return new Vector3(
            vector.x / this.width,
            vector.y / this.height,
            vector.z / this.breath
            );
    }

    public Vector3 GetAsPercentage(Vector3Int vector)
    {
        return new Vector3(
            System.Convert.ToSingle(vector.x) / this.width,
            System.Convert.ToSingle(vector.y) / this.height,
            System.Convert.ToSingle(vector.z) / this.breath
            );
    }

    public Vector3Int GetFromPercentage(Vector3 vector)
    {
        return new Vector3Int(
            this.width - Mathf.RoundToInt(vector.x * this.width),
            this.height - Mathf.RoundToInt(vector.y * this.height),
            this.breath - Mathf.RoundToInt(vector.z * this.breath)
            );
    }

    #region OctTree

    public octTree.OctTreeMedicalData GetAsOctTree()
    {
        return new octTree.OctTreeMedicalData(this.buffer, this.definition, this.largestSegment, this.RelatedSegments, width, height, breath);
    }

    #endregion

    public static int[] ArrayAppend(int value, int[] array)
    {
        int[] newArray;

        if (array != null)
        {
            newArray = new int[array.Length + 1];

            for (int index = 0; index < array.Length; index++)
            {
                if (array[index] == value)
                    return array;

                newArray[index] = array[index];
            }
        }
        else
        {
            newArray = new int[1];
        }

        newArray[newArray.Length - 1] = value;

        return newArray;
    }

    public static int[] Merge(int[] left, int[] right)
    {
        if (left == null)
        {
            return right;
        }
        else if (right == null)
        {
            return left;
        }

        int[] array = new int[left.Length + right.Length];

        for (int index = 0; index < left.Length; index++)
        {
            array[index] = left[index];
        }
        for (int index = 0; index < right.Length; index++)
        {
            array[left.Length + index] = right[index];
        }

        return array;
    }

    public static bool ArrayContains(int value, int[] array)
    {
        for(int index = 0; index < array.Length; index++)
        {
            if (array[index] == value)
            {
                return true;
            }
        }
        return false;
    }


    public static void ChangeValue(int value, int changeTo, int[] array)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index] == value)
            {
                array[index] = changeTo;
            }
        }
    }

}


// 
// below is old code that is used to merge sections when they where created.
// It has been optimised a fair amount but it is really slow
// this was later found to be a caused by a floating point error by the lower loop
//

/*
        List<int[]> finalSegementionResults = new List<int[]>();
        List<int> merged = new List<int>();
        for (index = 0; index < segments.Count; index++)
        {
            // if it wont exist after this dont' do anything about it
            if (!merged.Contains(index))
            {
                // all of the segments
                int[] segment = segments[index];

                // loop though the pixels assoiated with this to see if they can be merged
                for(int i = 0; i < segment.Length; i++)
                {
                    if (this.definition[segment[i]] == TypeOfVoxel.inside && this.RelatedSegments[segment[i]] != null)
                    {
                        // fix the largest range
                        List<int> arraysToMerge = MergeNauborSegments(segment[i]);

                        // remove the duplicates
                        if (arraysToMerge.Count > 0)
                        {
                            // merge all the arrays found
                            for(int j = 0; j < arraysToMerge.Count; j++)
                            {
                                int[] other = segments[arraysToMerge[j]];

                                segment = Merge(segment, segments[arraysToMerge[j]]);

                                for(int k = 0; k < other.Length; k++)
                                {
                                    this.RelatedSegments[other[k]][0] = index;
                                }
                            }

                            //segment = segment.Distinct().ToArray();
                            // add the new values the the merged array
                            merged.AddRange(arraysToMerge);
                        }
                    }
                }

                // remove duplicates
                segment = segment.Distinct().ToArray();

                // add the new segment to the final output
                finalSegementionResults.Add(segment);

                // size of the largst segment
                if (sizeOfLargestSegment < segment.Length)
                {
                    this.sizeOfLargestSegment = segment.Length;
                    this.largestSegment = index;
                }
            }
        }

        this.amountOfSegments = finalSegementionResults.Count;
        */
/*
int index = 0;
int i = 0;
for(int z = 0; z < this.breath; z++)
    for (int y = 0; y < this.height; y++)
        for (int x = 0; x < this.width; x++)
        {
            if (!this.GetPosition(index).Equals(new Vector3(x, y, z)))
            {
                Debug.LogError("index: " + index + ", Vector: " + new Vector3(x, y, z));
                i++;
            }
            else if (!this.GetIndex(x, y, z).Equals(index))
            {
                Debug.LogError("2nd Option: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(x, y, z));

            i++;
        }
            if (i > 20)
            {
                return;
            }

            /*if (index > 16777255)
            {
                //Debug.Log("Test: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(new Vector3(x, y, z)));
                Debug.Log("Test: " + "index: " + index + ", Vector: " + new Vector3(x, y, z) + ",Expected: " + this.GetPosition(index) + "index: " + this.GetIndex(x, y, z));
                i++;
            index++;
        }

            }*/
