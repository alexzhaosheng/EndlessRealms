using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility.Extensions;
public static class DirectionExtension
{
    public static Direction GetOpposite(this Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }

    public static Vector2 ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector2(0, 1);
            case Direction.South:
                return new Vector2(0, -1);
            case Direction.East:
                return new Vector2(1, 0);
            case Direction.West:
                return new Vector2(-1, 0);

            default:
                throw new ArgumentException();
        }
    }

    public static Direction ToDirection(this string directionStr)
    {
        return (Direction)Enum.Parse(typeof(Direction), directionStr);
    }

    public static Direction ToDirection(this Vector2 vector)
    {
        if(vector.X > 0)
        {
            return Direction.East;
        }
        else if(vector.X < 0)
        {
            return Direction.West;
        }
        else if(vector.Y > 0)
        {
            return Direction.North;
        }
        else if(vector.Y < 0)
        {
            return Direction.South;
        }

        throw new EndlessRealmsException("Can't convert vector to direction.");
    }


}
