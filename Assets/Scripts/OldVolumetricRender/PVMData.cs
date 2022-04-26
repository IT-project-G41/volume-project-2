using UnityEngine;

[System.Serializable]
public class PVMData
{
    public int width;
    public int height;
    public int breath;
    public int scalex;
    public int scaley;
    public int scalez;
    public uint[] data;

    
    private uint maxValue = uint.MinValue;
    private uint minValue = uint.MaxValue;

    /**
     * 
     */
    public uint GetMaxPixelValue() {
        if (maxValue != uint.MinValue)
        {
            setMinAndMax();
        }

        return maxValue;
    }

    /**
     * 
     */
    public uint GetMinPixelValue()
    {
        if (minValue != uint.MaxValue)
        {
            setMinAndMax();
        }

        return minValue;
    }

    private void setMinAndMax()
    {
        if (this.data != null)
        {
            for(int index = 0; index < data.Length; index++)
            {
                if(maxValue < this.data[index])
                {
                    maxValue = this.data[index];
                }

                if (minValue > this.data[index])
                {
                    minValue = this.data[index];
                }
            }
        }
    }
}
