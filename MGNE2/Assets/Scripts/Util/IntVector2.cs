using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntVector2 {

    public int x, y;

    public IntVector2(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public float magnitude {
        get { return Mathf.Sqrt((float)(x * x + y * y)); }
    }

    public int sqrMagnitude {
        get { return x * x + y * y; }
    }

    public static float Angle(IntVector2 from, IntVector2 to) {
        return Vector2.Angle(from, to);
    }

    public static float Distance(IntVector2 a, IntVector2 b) {
        return Vector2.Distance(a, b);
    }

    public static float Dot(IntVector2 lhs, IntVector2 rhs) {
        return Vector2.Dot(lhs, rhs);
    }
    
    public static Vector2 Lerp(IntVector2 a, IntVector2 b, float t) {
        return Vector2.Lerp(a, b, t);
    }
    
    public static Vector2 LerpUnclamped(IntVector2 a, IntVector2 b, float t) {
        return Vector2.LerpUnclamped(a, b, t);
    }

    public static IntVector2 Max(IntVector2 lhs, IntVector2 rhs) {
        return new IntVector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
    }

    public static IntVector2 Min(IntVector2 lhs, IntVector2 rhs) {
        return new IntVector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
    }

    public static IntVector2 Scale(IntVector2 a, IntVector2 b) {
        return new IntVector2(a.x * b.x, a.y * b.y);
    }
    
    public override bool Equals(object other) {
        if (!typeof(IntVector2).IsAssignableFrom(other.GetType())) {
            return false;
        } else {
            IntVector2 otherVector = (IntVector2)other;
            return otherVector.x == x && otherVector.y == y;
        }
    }

    public override int GetHashCode() {
        return x.GetHashCode() * 769 + y.GetHashCode() * 1543;
    }
    
    public void Scale(IntVector2 scale) {
        x *= scale.x;
        y *= scale.y;
    }

    public void Set(int newX, int newY) {
        this.x = newX;
        this.y = newY;
    }

    public int SqrMagnitude() {
        return sqrMagnitude;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }

    public static IntVector2 operator +(IntVector2 a, IntVector2 b) {
        return new IntVector2(a.x + b.x, a.y + b.y);
    }

    public static IntVector2 operator -(IntVector2 a) {
        return a * -1;
    }

    public static IntVector2 operator -(IntVector2 a, IntVector2 b) {
        return new IntVector2(a.x - b.x, a.y - b.y);
    }

    public static Vector2 operator *(float d, IntVector2 a) {
        return new Vector2((float)a.x * d, (float)a.y * d);
    }

    public static IntVector2 operator *(int d, IntVector2 a) {
        return new IntVector2(a.x * d, a.y * d);
    }

    public static Vector2 operator *(IntVector2 a, float d) {
        return d * a;
    }

    public static IntVector2 operator *(IntVector2 a, int d) {
        return d * a;
    }

    public static IntVector2 operator /(IntVector2 a, int d) {
        return new IntVector2(a.x / d, a.y / d);
    }

    public static bool operator ==(IntVector2 lhs, IntVector2 rhs) {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(IntVector2 lhs, IntVector2 rhs) {
        return !lhs.Equals(rhs);
    }

    public static implicit operator Vector2(IntVector2 v) {
        return new Vector2((float)v.x, (float)v.y);
    }
}
