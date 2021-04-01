using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorHelper
{

    public struct Matrix2x2
    {
        public Vector2 XCol;
        public Vector2 YCol;

        public Matrix2x2(Vector2 xCol, Vector2 yCol)
        {
            XCol = xCol;
            YCol = yCol;
        }
    }
    public struct Matrix3x2
    {
        public Vector3 XCol;
        public Vector3 YCol;

        public Matrix3x2(Vector3 xCol, Vector3 yCol)
        {
            XCol = xCol;
            YCol = yCol;
        }
    }
    public struct Matrix3x3
    {
        public Vector3 XCol;
        public Vector3 YCol;
        public Vector3 ZCol;

        public Matrix3x3(Vector3 xCol, Vector3 yCol, Vector3 zCol)
        {
            XCol = xCol;
            YCol = yCol;
            ZCol = zCol;
        }

        public static Matrix3x3 zero = new Matrix3x3(Vector3.zero, Vector3.zero, Vector3.zero);

        public static Vector3 operator * (Matrix3x3 m, Vector3 v)
        {
            return VectorHelper.Multiply(m, v);
        }
        public Matrix3x3 Transpose()
        {
            var xCol = new Vector3(XCol.x, YCol.x, ZCol.x);
            var yCol = new Vector3(XCol.y, YCol.y, ZCol.y);
            var zCol = new Vector3(XCol.z, YCol.z, ZCol.z);
            return new Matrix3x3(xCol, yCol, zCol);
        }
    }

    public static Matrix3x2 OrthogonalPlaneBasis(Vector3 zAxis)
    {
        Vector3 orthogonalX = Vector3.Cross(Vector3.right, zAxis).normalized;
        if (orthogonalX.Equals(Vector3.zero))
        {
            orthogonalX = Vector3.Cross(Vector3.up, zAxis).normalized;
        }

        Vector3 orthogonalY = Vector3.Cross(orthogonalX, zAxis).normalized;

        return new Matrix3x2(orthogonalX, orthogonalY);
    }
    public static Vector2 OrthogonalVectorBasis(Vector2 yAxis)
    {
        return new Vector2(yAxis.y, -yAxis.x);
    }
    
    public static Vector2 ProjectOntoBasis(Vector3 worldVector, Matrix3x2 planeBasis)
    {
        Vector2 planeVector;
        planeVector.x = Vector3.Dot(planeBasis.XCol, worldVector);
        planeVector.y = Vector3.Dot(planeBasis.YCol, worldVector);
        return planeVector;
    }
    public static Vector3 ProjectFromBasis(Vector2 planeVector, Matrix3x2 planeBasis)
    {
        Vector3 worldVector;
        worldVector = (planeBasis.XCol * planeVector.x) + (planeBasis.YCol * planeVector.y);
        /*worldVector.x = Vector3.Dot(planeBasis.XCol * planeVector.x, Vector3.right) + Vector3.Dot(planeBasis.YCol * planeVector.y, Vector3.right);
        worldVector.y = Vector3.Dot(planeBasis.XCol * planeVector.x, Vector3.up) + Vector3.Dot(planeBasis.YCol * planeVector.y, Vector3.up);
        worldVector.z = Vector3.Dot(planeBasis.XCol * planeVector.x, Vector3.forward) + Vector3.Dot(planeBasis.YCol * planeVector.y, Vector3.forward);*/
        return worldVector;
    }
    public static Vector3 ProjectOntoBasis(Vector3 worldVector, Matrix3x3 basis)
    {
        Vector3 planeVector;
        planeVector.x = Vector3.Dot(basis.XCol, worldVector);
        planeVector.y = Vector3.Dot(basis.YCol, worldVector);
        planeVector.z = Vector3.Dot(basis.ZCol, worldVector);
        return planeVector;
    }
    public static Vector3 ProjectFromBasis(Vector3 basisVector, Matrix3x3 basis)
    {
        Vector3 worldVector;
        worldVector = (basis.XCol * basisVector.x) + (basis.YCol * basisVector.y) + (basis.ZCol * basisVector.z);
        /*worldVector.x = Vector3.Dot(basis.XCol * basisVector.x, Vector3.right) + Vector3.Dot(basis.YCol * basisVector.y, Vector3.right) + Vector3.Dot(basis.ZCol * basisVector.z, Vector3.right);
        worldVector.y = Vector3.Dot(basis.XCol * basisVector.x, Vector3.up) + Vector3.Dot(basis.YCol * basisVector.y, Vector3.up) + Vector3.Dot(basis.ZCol * basisVector.z, Vector3.up);
        worldVector.z = Vector3.Dot(basis.XCol * basisVector.x, Vector3.forward) + Vector3.Dot(basis.YCol * basisVector.y, Vector3.forward) + Vector3.Dot(basis.ZCol * basisVector.z, Vector3.forward);*/
        return worldVector;
    }
    public static Vector2 ProjectOntoBasis(Vector2 worldVector, Matrix2x2 basis)
    {
        Vector2 planeVector;
        planeVector.x = Vector2.Dot(basis.XCol, worldVector);
        planeVector.y = Vector2.Dot(basis.YCol, worldVector);
        return planeVector;
    }
    public static Vector2 ProjectFromBasis(Vector2 basisVector, Matrix2x2 basis)
    {
        Vector2 worldVector;
        worldVector = (basis.XCol * basisVector.x) + (basis.YCol * basisVector.y);
        /*worldVector.x = Vector2.Dot(basis.XCol * basisVector.x, Vector3.right) + Vector2.Dot(basis.YCol * basisVector.y, Vector3.right);
        worldVector.y = Vector2.Dot(basis.XCol * basisVector.x, Vector3.up) + Vector2.Dot(basis.YCol * basisVector.y, Vector3.up);*/
        return worldVector;
    }

    public static Vector3 CartesianToPolar(Vector3 cartesian)
    {
        Vector3 toRet;
        toRet.x = cartesian.magnitude;
        toRet.y = Mathf.Acos(cartesian.z / toRet.x);
        toRet.z = Mathf.Atan2(cartesian.y, cartesian.x);
        return toRet;
    }
    public static Vector3 PolarToCartesian(Vector3 polar)
    {
        Vector3 toRet;
        toRet.x = polar.x * Mathf.Sin(polar.y) * Mathf.Cos(polar.z);
        toRet.y = polar.x * Mathf.Sin(polar.y) * Mathf.Sin(polar.z);
        toRet.z = polar.x * Mathf.Cos(polar.y);
        return toRet;
    }
    public static Vector2 CartesianToPolar(Vector2 cartesian)
    {
        Vector2 toRet;
        toRet.x = cartesian.magnitude;
        toRet.y = Mathf.Atan2(cartesian.y, cartesian.x);
        return toRet;
    }
    public static Vector2 PolarToCartesian(Vector2 polar)
    {
        Vector2 toRet;
        toRet.x = polar.x * Mathf.Cos(polar.y);
        toRet.y = polar.x * Mathf.Sin(polar.y);
        return toRet;
    }

    public static Vector2 Map(Vector2 initial, FunctionHelper.Func<float, float> func)
    {
        Vector2 final;
        final.x = func(initial.x);
        final.y = func(initial.y);
        return final;
    }
    public static Vector3 Map(Vector3 initial, FunctionHelper.Func<float, float> func)
    {
        Vector3 final;
        final.x = func(initial.x);
        final.y = func(initial.y);
        final.z = func(initial.z);
        return final;
    }
    public static Matrix2x2 Map(Matrix2x2 initial, FunctionHelper.Func<float, float> func)
    {
        return MapCol(initial, (Vector2 initialColumn) =>
        {
            return VectorHelper.Map(initialColumn, func);
        });
    }
    public static Matrix3x2 Map(Matrix3x2 initial, FunctionHelper.Func<float, float> func)
    {
        return MapCol(initial, (Vector3 initialColumn) =>
        {
            return VectorHelper.Map(initialColumn, func);
        });
    }
    public static Matrix3x3 Map(Matrix3x3 initial, FunctionHelper.Func<float, float> func)
    {
        return MapCol(initial, (Vector3 initialColumn) =>
        {
            return VectorHelper.Map(initialColumn, func);
        });
    }
    public static Matrix2x2 MapCol(Matrix2x2 initial, FunctionHelper.Func<Vector2, Vector2> func)
    {
        Matrix2x2 final;
        final.XCol = func(initial.XCol);
        final.YCol = func(initial.YCol);
        return final;
    }
    public static Matrix3x2 MapCol(Matrix3x2 initial, FunctionHelper.Func<Vector3, Vector3> func)
    {
        Matrix3x2 final;
        final.XCol = func(initial.XCol);
        final.YCol = func(initial.YCol);
        return final;
    }
    public static Matrix3x3 MapCol(Matrix3x3 initial, FunctionHelper.Func<Vector3, Vector3> func)
    {
        Matrix3x3 final;
        final.XCol = func(initial.XCol);
        final.YCol = func(initial.YCol);
        final.ZCol = func(initial.ZCol);
        return final;
    }

    public static Vector2 ZipWith(Vector2 first, Vector2 second, FunctionHelper.Func2<float, float, float> func)
    {
        Vector2 toReturn;
        toReturn.x = func(first.x, second.x);
        toReturn.y = func(first.y, second.y);
        return toReturn;
    }
    public static Vector3 ZipWith(Vector3 first, Vector3 second, FunctionHelper.Func2<float, float, float> func)
    {
        Vector3 toReturn;
        toReturn.x = func(first.x, second.x);
        toReturn.y = func(first.y, second.y);
        toReturn.z = func(first.z, second.z);
        return toReturn;
    }

    public static float Reduce(Vector2 value, FunctionHelper.Func2<float, float, float> func)
    {
        return func(value.x, value.y);
    }
    public static float Reduce(Vector3 value, FunctionHelper.Func2<float, float, float> func)
    {
        return func(func(value.x, value.y), value.z);
    }

    public static Vector2 Clamp(Vector2 value, float min, float max)
    {
        if (value.sqrMagnitude > max * max)
        {
            return value.normalized * max;
        }
        else if (value.sqrMagnitude < min * min)
        {
            return value.normalized * min;
        }
        else
        {
            return value;
        }
    }
    public static Vector2 Clamp01(Vector2 value)
    {
        if (value.sqrMagnitude > 1f)
        {
            return value.normalized;
        }
        else
        {
            return value;
        }
    }
    public static Vector2 SoftClamp(Vector2 value, float min, float max, float attenuation)
    {
        if (value.sqrMagnitude > max * max)
        {
            float newMagnitude = (value.magnitude - max) * (1 - attenuation) + max;
            return value.normalized * newMagnitude;
        }
        if (value.sqrMagnitude < min * min)
        {
            float newMagnitude = (value.magnitude - min) * (1 - attenuation) + min;
            return value.normalized * newMagnitude;
        }
        else
        {
            return value;
        }
    }
    public static Vector3 Clamp(Vector3 value, float min, float max)
    {
        if (value.sqrMagnitude > max * max)
        {
            return value.normalized * max;
        }
        else if (value.sqrMagnitude < min * min)
        {
            var potentialVector = value.normalized * min;
            return potentialVector;
        }
        else
        {
            return value;
        }
    }
    public static Vector3 Clamp01(Vector3 value)
    {
        if (value.sqrMagnitude > 1f)
        {
            return value.normalized;
        }
        else
        {
            return value;
        }
    }
    public static Vector3 SoftClamp(Vector3 value, float min, float max, float attenuation)
    {
        if (value.sqrMagnitude > max * max)
        {
            float newMagnitude = (value.magnitude - max) * (1 - attenuation) + max;
            return value.normalized * newMagnitude;
        }
        if (value.sqrMagnitude < min * min)
        {
            float newMagnitude = (value.magnitude - min) * (1 - attenuation) + min;
            return value.normalized * newMagnitude;
        }
        else
        {
            return value;
        }
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        Vector2 lerped;
        lerped.x = Mathf.Lerp(a.x, b.x, t);
        lerped.y = Mathf.Lerp(a.y, b.y, t);
        return lerped;
    }
    public static Vector2 LerpPolar(Vector2 a, Vector2 b, float t)
    {
        Vector2 lerped;
        lerped.x = Mathf.Lerp(a.x, b.x, t);
        lerped.y = Mathf.Deg2Rad * Mathf.LerpAngle(a.y * Mathf.Rad2Deg, b.y * Mathf.Rad2Deg, t);
        return lerped;
    }
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        Vector3 lerped;
        lerped.x = Mathf.Lerp(a.x, b.x, t);
        lerped.y = Mathf.Lerp(a.y, b.y, t);
        lerped.z = Mathf.Lerp(a.z, b.z, t);
        return lerped;
    }
    public static Vector3 LerpPolar(Vector3 a, Vector3 b, float t)
    {
        Vector3 lerped;
        lerped.x = Mathf.Lerp(a.x, b.x, t);
        lerped.y = Mathf.Deg2Rad * Mathf.LerpAngle(a.y * Mathf.Rad2Deg, b.y * Mathf.Rad2Deg, t);
        lerped.z = Mathf.Deg2Rad * Mathf.LerpAngle(a.z * Mathf.Rad2Deg, b.z * Mathf.Rad2Deg, t);
        return lerped;
    }

    public static Vector2 Rotate(Vector2 v, float theta)
    {
        var t = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
        return new Vector2(
            (t.x * v.x) - (t.y * v.y),
            (t.y * v.x) + (t.x * v.y)
        );
    }

    public static Vector3 HadamardProduct(Vector3 v, Vector3 w)
    {
        return new Vector3(v.x * w.x, v.y * w.y, v.z * w.z);
    }

    public static Vector3 Multiply(Matrix3x3 m, Vector3 v)
    {
        return ProjectFromBasis(v, m);
    }

    public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
    {
        var xCol = VectorHelper.Multiply(a, b.XCol);
        var yCol = VectorHelper.Multiply(a, b.YCol);
        var zCol = VectorHelper.Multiply(a, b.ZCol);

        return new Matrix3x3(xCol, yCol, zCol);
    }

    // Can't work for points, because translations aren't a linear transformation in non-projective space
    public static Matrix3x3 InverseVectorMatrixOf(Transform transform)
    {
        var xCol = transform.InverseTransformVector(Vector3.right);
        var yCol = transform.InverseTransformVector(Vector3.up);
        var zCol = transform.InverseTransformVector(Vector3.forward);

        return new Matrix3x3(xCol, yCol, zCol);
    }
    public static Matrix3x3 VectorMatrixOf(Transform transform)
    {
        var xCol = transform.TransformVector(Vector3.right);
        var yCol = transform.TransformVector(Vector3.up);
        var zCol = transform.TransformVector(Vector3.forward);

        return new Matrix3x3(xCol, yCol, zCol);
    }
    public static Matrix3x3 InverseDirectionMatrixOf(Transform transform)
    {
        var xCol = transform.InverseTransformDirection(Vector3.right);
        var yCol = transform.InverseTransformDirection(Vector3.up);
        var zCol = transform.InverseTransformDirection(Vector3.forward);

        return new Matrix3x3(xCol, yCol, zCol);
    }
    public static Matrix3x3 DirectionMatrixOf(Transform transform)
    {
        var xCol = transform.TransformDirection(Vector3.right);
        var yCol = transform.TransformDirection(Vector3.up);
        var zCol = transform.TransformDirection(Vector3.forward);

        return new Matrix3x3(xCol, yCol, zCol);
    }
}
