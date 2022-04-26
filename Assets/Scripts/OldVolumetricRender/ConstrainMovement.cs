using UnityEngine;

/// <summary>
/// Stops the object it is in form moving away from it's local coordenates.
/// 
/// Rotation was added at a later date but still can be useful
/// </summary>
public class ConstrainMovement : MonoBehaviour
{
    [Header("Behaviour Parameters")]
    public bool ConvertMovementIntoRotation = false;
    public bool UseLocalValues = true;

    [Header("Position Constraints")]
    public bool AllowMovementOnX = false;
    public bool AllowMovementOnY = false;
    public bool AllowMovementOnZ = false;

    private float MovementX = 0;
    private float MovementY = 0;
    private float MovementZ = 0;

    public bool SetRangeOnX = false;
    public float RangeOnX = -1;
    public bool SetRangeOnY = false;
    public float RangeOnY = -1;
    public bool SetRangeOnZ = false;
    public float RangeOnZ = -1;

    [Header("Rotation Constraints")]
    public bool AllowRotationOnX = false;
    public bool AllowRotationOnY = false;
    public bool AllowRotationOnZ = false;

    private float RotationX = 0;
    private float RotationY = 0;
    private float RotationZ = 0;

    /// <summary>
    /// This is only here for telling how many textures there that this object needs to focus on
    /// </summary>
    public ChangeTextureBasedOnMovment textureArray;

    // Start is called before the first frame update
    void Start()
    {
        // set all the values to the same ones that exist on start up
        if (UseLocalValues)
        {
            MovementX = gameObject.transform.localPosition.x;
            MovementY = gameObject.transform.localPosition.y;
            MovementZ = gameObject.transform.localPosition.z;
            RotationX = gameObject.transform.localRotation.x;
            RotationY = gameObject.transform.localRotation.y;
            RotationZ = gameObject.transform.localRotation.z;
        }
        else
        {
            MovementX = gameObject.transform.position.x;
            MovementY = gameObject.transform.position.y;
            MovementZ = gameObject.transform.position.z;
            RotationX = gameObject.transform.position.x;
            RotationY = gameObject.transform.position.y;
            RotationZ = gameObject.transform.position.z;
        }
    }



    // Update is called once per frame
    void Update()
    {
        // So this is a big set of if statments that don't really do that much. 
        // the first thing they all do is see if movment contraints are on if 
        // they are they will check if the object has moved. if they have been 
        //they will move them back in to place 
        // if the object is allowed to move but has a range specification these
        // statements will check to see if the object has exceeded that specificiation
        // and place them in at the end position in thier range.

        if (UseLocalValues)
        {
            // Movement Logic
            float valueX = gameObject.transform.localPosition.x - MovementX;
            if (!AllowMovementOnX &&
                valueX != 0)
            {
                gameObject.transform.localPosition =
                    new Vector3(MovementX,
                        gameObject.transform.localPosition.y,
                        gameObject.transform.localPosition.z);
            }
            else if (SetRangeOnX &&
                System.Math.Abs(gameObject.transform.localPosition.x) > RangeOnX)
            {
                if (gameObject.transform.localPosition.x < 0)
                {
                    gameObject.transform.localPosition =
                    new Vector3(-RangeOnX,
                        gameObject.transform.localPosition.y,
                        gameObject.transform.localPosition.z);
                    MovementX = -RangeOnX;
                }
                else
                {
                    gameObject.transform.localPosition =
                    new Vector3(RangeOnX,
                        gameObject.transform.localPosition.y,
                        gameObject.transform.localPosition.z);
                    MovementX = RangeOnX;
                }
            }
            else
            {
                MovementX = gameObject.transform.localPosition.x;
            }

            float valueY = gameObject.transform.localPosition.y - MovementY;
            if (!AllowMovementOnY && valueY != 0)
            {
                gameObject.transform.localPosition =
                    new Vector3(gameObject.transform.localPosition.x, MovementY,
                        gameObject.transform.localPosition.z);
            }
            else if (SetRangeOnY &&
                System.Math.Abs(gameObject.transform.localPosition.y) > RangeOnY)
            {
                if (gameObject.transform.localPosition.y < 0)
                {
                    gameObject.transform.localPosition =
                    new Vector3(gameObject.transform.localPosition.x, -RangeOnY,
                        gameObject.transform.localPosition.z);
                    MovementY = -RangeOnY;
                }
                else
                {
                    gameObject.transform.localPosition =
                    new Vector3(gameObject.transform.localPosition.x, RangeOnY,
                        gameObject.transform.localPosition.z);
                    MovementY = RangeOnY;
                }
            }
            else
            {
                MovementY = gameObject.transform.localPosition.y;
            }

            float valueZ = gameObject.transform.localPosition.z - MovementZ;
            if (!AllowMovementOnZ && valueZ != 0)
            {
                gameObject.transform.localPosition =
                    new Vector3(gameObject.transform.localPosition.x,
                        gameObject.transform.localPosition.y,
                        MovementZ);
            }
            else if (SetRangeOnZ &&
                System.Math.Abs(gameObject.transform.localPosition.z) > RangeOnZ)
            {
                if (gameObject.transform.localPosition.z < 0)
                {
                    gameObject.transform.localPosition =
                        new Vector3(gameObject.transform.localPosition.x,
                            gameObject.transform.localPosition.y,
                            -RangeOnZ);
                    MovementZ = -RangeOnZ;
                }
                else
                {
                    gameObject.transform.localPosition =
                        new Vector3(gameObject.transform.localPosition.x,
                            gameObject.transform.localPosition.y,
                            RangeOnZ);
                    MovementZ = RangeOnZ;
                }
            }
            else
            {
                MovementZ = gameObject.transform.localPosition.z;
            }

            // Rotation Logic
            if (AllowRotationOnX)
            {
                // if were supposed to be moving the object update the vector
                RotationX = gameObject.transform.localRotation.x;
            }
            else if (gameObject.transform.localRotation.x != RotationX)
            {
                // if your not allowed be here update the vector
                gameObject.transform.localRotation = new Quaternion(RotationX,
                    gameObject.transform.localRotation.y,
                    gameObject.transform.localRotation.z,
                    gameObject.transform.localRotation.w);
            }

            if (AllowRotationOnY)
            {
                // if were supposed to be moving the object update the vector
                RotationY = gameObject.transform.localRotation.y;
            }
            else if (gameObject.transform.localRotation.y != RotationY)
            {
                // if your not allowed be here update the vector
                gameObject.transform.localRotation = new Quaternion(
                    gameObject.transform.localRotation.x, RotationY,
                    gameObject.transform.localRotation.z,
                    gameObject.transform.localRotation.w);
            }

            if (AllowRotationOnZ)
            {
                // if were supposed to be moving the object update the vector
                RotationZ = gameObject.transform.localRotation.z;
            }
            else if (gameObject.transform.localRotation.z != RotationZ)
            {
                // if your not allowed be here update the vector
                gameObject.transform.localRotation = new Quaternion(
                    gameObject.transform.localRotation.x,
                    gameObject.transform.localRotation.y,
                    RotationZ, gameObject.transform.localRotation.w);
            }

            if (ConvertMovementIntoRotation)
            {
                Debug.Log("X:" + valueX + "Y:" + valueY + "Z:" + valueZ);

                // adjust the users rotation based on the movement vectors
                if (AllowRotationOnX && !AllowMovementOnX)
                {
                    valueX += gameObject.transform.localRotation.x;
                    if (!AllowRotationOnY && !AllowRotationOnZ)
                    {
                        //valueX += valueY + valueZ;
                    }
                    else if (AllowRotationOnY && !AllowRotationOnZ)
                    {
                        //valueX += valueZ / 2;
                    }
                    else if (AllowRotationOnZ && !AllowRotationOnY)
                    {
                        //valueX += valueY / 2;
                    }
                }
                else
                {
                    valueX = gameObject.transform.localRotation.x;
                }


                if (AllowRotationOnY && !AllowMovementOnY)
                {
                    valueY += gameObject.transform.localRotation.y;

                    if (!AllowRotationOnX && !AllowRotationOnZ)
                    {
                        //valueY += valueX + valueZ;
                    }
                    else if (AllowRotationOnX && !AllowRotationOnZ)
                    {
                        // no rotation on the Z axis will be taking place so this will need to be concidered
                        //valueY += valueZ / 2;
                    }
                    else if (AllowRotationOnZ && !AllowRotationOnX)
                    {
                        //
                        //valueY += valueZ / 2;
                    }
                }
                else
                {
                    valueY += gameObject.transform.localRotation.y;
                }


                if (AllowRotationOnZ && !AllowMovementOnZ)
                {
                    valueZ += gameObject.transform.localRotation.z;

                    if (!AllowRotationOnX && !AllowRotationOnY)
                    {
                        //valueZ += valueY + valueX;
                    }
                    else if (AllowRotationOnY && !AllowRotationOnY)
                    {
                        // no rotation on the Z axis will be taking place so this will need to be concidered
                        //valueZ += valueX;
                    }
                    else if (AllowRotationOnX && !AllowRotationOnX)
                    {
                        //
                        //valueZ += valueY;
                    }
                }
                else
                {
                    valueZ = gameObject.transform.localRotation.z;
                }

                // Due to a bug in angle mathmatics it looks like you can't use a 1 to 1 method for adding these


                // compile all the values determined above into the machine
                gameObject.transform.rotation = new Quaternion(
                    valueX,
                    valueY,
                    valueZ,
                    gameObject.transform.localRotation.w);
            }
        }
        else
        {
            // use global values rather than local

            // Movement Logic
            float valueX = gameObject.transform.position.x - MovementX;
            if (!AllowMovementOnX && valueX != 0)
            {
                gameObject.transform.position =
                    new Vector3(MovementX,
                        gameObject.transform.position.y,
                        gameObject.transform.position.z);
            }
            else if (SetRangeOnX &&
                System.Math.Abs(gameObject.transform.position.x) > RangeOnX)
            {
                if (gameObject.transform.position.x < 0)
                {
                    gameObject.transform.position =
                    new Vector3(-RangeOnX,
                        gameObject.transform.position.y,
                        gameObject.transform.position.z);
                    MovementX = -RangeOnX;
                }
                else
                {
                    gameObject.transform.position =
                    new Vector3(RangeOnX,
                        gameObject.transform.position.y,
                        gameObject.transform.position.z);
                    MovementX = RangeOnX;
                }
            }
            else
            {
                MovementX = gameObject.transform.position.x;
            }

            float valueY = gameObject.transform.position.y - MovementY;
            if (!AllowMovementOnY && valueY != 0)
            {
                gameObject.transform.position =
                    new Vector3(gameObject.transform.position.x, MovementY,
                        gameObject.transform.position.z);
            }
            else if (SetRangeOnY &&
                System.Math.Abs(gameObject.transform.position.y) > RangeOnY)
            {
                if (gameObject.transform.position.y < 0)
                {
                    gameObject.transform.position =
                    new Vector3(gameObject.transform.position.x, -RangeOnY,
                        gameObject.transform.position.z);
                    MovementY = -RangeOnY;
                }
                else
                {
                    gameObject.transform.position =
                    new Vector3(gameObject.transform.position.x, RangeOnY,
                        gameObject.transform.position.z);
                    MovementY = RangeOnY;
                }
            }
            else
            {
                MovementY = gameObject.transform.position.y;
            }

            float valueZ = gameObject.transform.position.z - MovementZ;
            if (!AllowMovementOnZ && valueZ != 0)
            {
                gameObject.transform.position =
                    new Vector3(gameObject.transform.position.x,
                        gameObject.transform.position.y,
                        MovementZ);
            }
            else if (SetRangeOnZ &&
                System.Math.Abs(gameObject.transform.position.z) > RangeOnZ)
            {
                if (gameObject.transform.position.z < 0)
                {
                    gameObject.transform.position =
                        new Vector3(gameObject.transform.position.x,
                            gameObject.transform.position.y,
                            -RangeOnZ);
                    MovementZ = -RangeOnZ;
                }
                else
                {
                    gameObject.transform.position =
                        new Vector3(gameObject.transform.position.x,
                            gameObject.transform.position.y,
                            RangeOnZ);
                    MovementZ = RangeOnZ;
                }
            }
            else
            {
                MovementZ = gameObject.transform.position.z;
            }

            // Rotation Logic
            if (AllowRotationOnX)
            {
                // if were supposed to be moving the object update the vector
                RotationX = gameObject.transform.rotation.x;
            }
            else if (gameObject.transform.rotation.x != RotationX)
            {
                // if your not allowed be here update the vector
                gameObject.transform.rotation = new Quaternion(RotationX,
                    gameObject.transform.rotation.y,
                    gameObject.transform.rotation.z,
                    gameObject.transform.rotation.w);
            }

            if (AllowRotationOnY)
            {
                // if were supposed to be moving the object update the vector
                RotationY = gameObject.transform.rotation.y;
            }
            else if (gameObject.transform.rotation.y != RotationY)
            {
                // if your not allowed be here update the vector
                gameObject.transform.rotation = new Quaternion(
                    gameObject.transform.rotation.x, RotationY,
                    gameObject.transform.rotation.z,
                    gameObject.transform.rotation.w);
            }

            if (AllowRotationOnZ)
            {
                // if were supposed to be moving the object update the vector
                RotationZ = gameObject.transform.rotation.z;
            }
            else if (gameObject.transform.rotation.z != RotationZ)
            {
                // if your not allowed be here update the vector
                gameObject.transform.rotation = new Quaternion(
                    gameObject.transform.rotation.x,
                    gameObject.transform.rotation.y,
                    RotationZ, gameObject.transform.rotation.w);
            }

            if (ConvertMovementIntoRotation)
            {
                Debug.Log("X:" + valueX + "Y:" + valueY + "Z:" + valueZ);

                // adjust the users rotation based on the movement vectors
                if (AllowRotationOnX && !AllowMovementOnX)
                {
                    valueX += gameObject.transform.rotation.x;
                    if (!AllowRotationOnY && !AllowRotationOnZ)
                    {
                        //valueX += valueY + valueZ;
                    }
                    else if (AllowRotationOnY && !AllowRotationOnZ)
                    {
                        //valueX += valueZ / 2;
                    }
                    else if (AllowRotationOnZ && !AllowRotationOnY)
                    {
                        //valueX += valueY / 2;
                    }
                }
                else
                {
                    valueX = gameObject.transform.rotation.x;
                }


                if (AllowRotationOnY && !AllowMovementOnY)
                {
                    valueY += gameObject.transform.rotation.y;

                    if (!AllowRotationOnX && !AllowRotationOnZ)
                    {
                        //valueY += valueX + valueZ;
                    }
                    else if (AllowRotationOnX && !AllowRotationOnZ)
                    {
                        // no rotation on the Z axis will be taking place so this will need to be concidered
                        //valueY += valueZ / 2;
                    }
                    else if (AllowRotationOnZ && !AllowRotationOnX)
                    {
                        //
                        //valueY += valueZ / 2;
                    }
                }
                else
                {
                    valueY = gameObject.transform.rotation.y;
                }


                if (AllowRotationOnZ && !AllowMovementOnZ)
                {
                    valueZ += gameObject.transform.rotation.z;

                    if (!AllowRotationOnX && !AllowRotationOnY)
                    {
                        //valueZ += valueY + valueX;
                    }
                    else if (AllowRotationOnY && !AllowRotationOnY)
                    {
                        // no rotation on the Z axis will be taking place so this will need to be concidered
                        //valueZ += valueX;
                    }
                    else if (AllowRotationOnX && !AllowRotationOnX)
                    {
                        //
                        //valueZ += valueY;
                    }
                }
                else
                {
                    valueZ = gameObject.transform.rotation.z;
                }

                // Due to a bug in angle mathmatics it looks like you can't use a 1 to 1 method for adding these


                // compile all the values determined above into the machine
                gameObject.transform.rotation = new Quaternion(
                    valueX,
                    valueY,
                    valueZ,
                    gameObject.transform.rotation.w);
            }
        }
    }

    public void EnableEverything()
    {
        // set all the allows to true
        AllowMovementOnX = true;
        AllowMovementOnY = true;
        AllowMovementOnZ = true;
        AllowRotationOnX = true;
        AllowRotationOnY = true;
        AllowRotationOnZ = true;
    }

    public enum RangePoints
    {
        positiveX,
        positiveY,
        positiveZ,
        negitiveX,
        negitiveY,
        negitiveZ
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public float DistanceToRange(RangePoints range)
    {
        switch (range)
        {
            case RangePoints.negitiveX:
                return System.Math.Abs(-RangeOnX - gameObject.transform.localPosition.x);
            case RangePoints.negitiveY:
                return System.Math.Abs(-RangeOnY - gameObject.transform.localPosition.y);
            case RangePoints.negitiveZ:
                return System.Math.Abs(-RangeOnZ - gameObject.transform.localPosition.z);
            case RangePoints.positiveX:
                return RangeOnX - gameObject.transform.localPosition.x;
            case RangePoints.positiveY:
                return RangeOnY - gameObject.transform.localPosition.y;
            case RangePoints.positiveZ:
                return RangeOnZ - gameObject.transform.localPosition.z;
            default:
                return -1;
        }
    }

    /// <summary>
    /// Returns a value as percentage between 1 and 0
    /// </summary>
    /// <param name="rangePoint"></param>
    /// <returns>A floating point primitive represenive of a percent as a decimal figure (1 == 100%)</returns>
    public float GetDistanceToRangeAsPercentage(RangePoints rangePoint)
    {
        // inisalize veriables
        float start;
        float end;
        float currentPos;

        // set up the values these are the same regardless of positive or negitive
        switch (rangePoint)
        {
            case RangePoints.positiveX:
            case RangePoints.negitiveX:
                start = RangeOnX;
                end = -RangeOnX;
                currentPos = gameObject.transform.localPosition.x;
                break;
            case RangePoints.negitiveY:
            case RangePoints.positiveY:
                start = RangeOnY;
                end = -RangeOnY;
                currentPos = gameObject.transform.localPosition.y;
                break;
            case RangePoints.negitiveZ:
            case RangePoints.positiveZ:
                start = RangeOnZ;
                end = -RangeOnZ;
                currentPos = gameObject.transform.localPosition.z;
                break;
            default:
                return -1;
        }

        // because C# :/ 
        bool isPositive = rangePoint == RangePoints.positiveX || rangePoint == RangePoints.positiveY || rangePoint == RangePoints.positiveZ;

        if (start < 0)
        {
            end -= start;
            currentPos -= start;
            start = 0;
        }

        if (end < 0)
        {
            start -= end;
            currentPos -= end;
            end = 0;
        }

        // make sure the order is what the end user will expect
        if (end < start)
        {
            // make the start more then the end
            float temp = start;
            start = end;
            end = temp;
        }

        return (currentPos - start) / (end - start);
    }

    public int GetResultAsSlideNumber(RangePoints rangePoint)
    {
        if (textureArray == null) return 0;
        float percentageValue = this.GetDistanceToRangeAsPercentage(rangePoint);
        if (percentageValue > 1)
        {
            percentageValue = 1f;
        }
        else if (percentageValue < 0)
        {
            percentageValue = 0;
        }
        return (int)System.Math.Round((1 - percentageValue) * (textureArray.AmountOfTexturesAvalible() - 1));
    }
}
