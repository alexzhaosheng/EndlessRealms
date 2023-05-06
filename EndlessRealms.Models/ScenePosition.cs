using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public struct ScenePosition
{
    public static ScenePosition operator +(ScenePosition p1, ScenePosition p2)
    {
        return new ScenePosition() { X = p1.X + p2.X, Y = p1.Y + p2.Y };
    }
    public static ScenePosition operator -(ScenePosition p1, ScenePosition p2)
    {
        return new ScenePosition() { X = p1.X - p2.X, Y = p1.Y - p2.Y };
    }
    public static ScenePosition operator *(ScenePosition p1, double number)
    {
        return new ScenePosition() { X = (int)(p1.X * number), Y = (int)(p1.Y * number) };
    }

    public int X { get; set; }
    public int Y { get; set; }

    public ScenePosition(int x, int y)
    {
        X = x; Y = y;
    }

    public ScenePosition Extend(int x, int y)
    {
        if (this.X != 0) 
        {
            this.X *= x; 
        }
        if(this.Y != 0)
        {
            this.Y *= y;
        }
        return this;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is ScenePosition)
        {
            ScenePosition p = (ScenePosition)obj;
            return X == p.X && Y == p.Y;
        }
        return false;        
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }
}
