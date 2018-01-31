using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntVector3 {

    public int x, y, z;

    public IntVector3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public float magnitude {
        get { return Mathf.Sqrt((float)(x * x + y * y + z * z)); }
    }

    public int sqrMagnitude {
        get { return x * x + y * y + z * z; }
    }

    public static float Angle(IntVector3 from, IntVector3 to) {
        return Vector3.Angle(from, to);
    }

    public static float Distance(IntVector3 a, IntVector3 b) {
        return Vector3.Distance(a, b);
    }

    public static int ManhattanDistance(IntVector3 a, IntVector3 b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
    }

    public static float Dot(IntVector3 lhs, IntVector3 rhs) {
        return Vector3.Dot(lhs, rhs);
    }

    public static Vector3 Lerp(IntVector3 a, IntVector3 b, float t) {
        return Vector3.Lerp(a, b, t);
    }

    public static Vector3 LerpUnclamped(IntVector3 a, IntVector3 b, float t) {
        return Vector3.LerpUnclamped(a, b, t);
    }

    public static IntVector3 Max(IntVector3 lhs, IntVector3 rhs) {
        return new IntVector3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
    }

    public static IntVector3 Min(IntVector3 lhs, IntVector3 rhs) {
        return new IntVector3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
    }

    public static IntVector3 Scale(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public override bool Equals(object other) {
        if (!typeof(IntVector3).IsAssignableFrom(other.GetType())) {
            return false;
        } else {
            IntVector3 otherVector = (IntVector3)other;
            return otherVector.x == x && otherVector.y == y && otherVector.z == z;
        }
    }

    public override int GetHashCode() {
        return x.GetHashCode() * 769 + y.GetHashCode() * 1543 + z.GetHashCode() * 7717;
    }

    public void Scale(IntVector3 scale) {
        x *= scale.x;
        y *= scale.y;
        z *= scale.z;
    }

    public void Set(int newX, int newY, int newZ) {
        this.x = newX;
        this.y = newY;
        this.z = newZ;
    }

    public int SqrMagnitude() {
        return sqrMagnitude;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    public static IntVector3 operator +(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static IntVector3 operator -(IntVector3 a) {
        return a * -1;
    }

    public static IntVector3 operator -(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator *(float d, IntVector3 a) {
        return new Vector3((float)a.x * d, (float)a.y * d, (float)a.z * d);
    }

    public static IntVector3 operator *(int d, IntVector3 a) {
        return new IntVector3(a.x * d, a.y * d, a.z * d);
    }

    public static Vector3 operator *(IntVector3 a, float d) {
        return d * a;
    }

    public static IntVector3 operator *(IntVector3 a, int d) {
        return d * a;
    }

    public static IntVector3 operator /(IntVector3 a, int d) {
        return new IntVector3(a.x / d, a.y / d, a.z / d);
    }

    public static bool operator ==(IntVector3 lhs, IntVector3 rhs) {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(IntVector3 lhs, IntVector3 rhs) {
        return !lhs.Equals(rhs);
    }

    public static implicit operator Vector3(IntVector3 v) {
        return new Vector3((float)v.x, (float)v.y, (float)v.z);
    }
}
