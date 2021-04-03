using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FunctionHelper  {

    public delegate Y Func<T, Y>(T t);
    public delegate U Func2<T, Y, U>(T t, Y y);
    public delegate I Func3<T, Y, U, I>(T t, Y y, U u);

    /*
     * Preconditions: steepness != 1f
     * 
    */
    public static Func<float, float> SFunction01(float threshold, float steepnessBefore, float steepnessAfter)
    {
        Func<float, float> c = (float steepness) => 2f / (1f - steepness) - 1f;
        Func3<float, float, float, float> func = (float x, float n, float steepness) => Mathf.Pow(x, c(steepness)) / (Mathf.Pow(n, c(steepness) - 1));

        Func<float, float> sFunc = (float x) =>
        {
            float toRet = 0f;

            if (!(x < 1f))
            {
                toRet =  1f;
            }
            else if (!(x > 0f))
            {
                toRet =  0f;
            }
            else if (x <= threshold)
            {
                toRet =  func(x, threshold, steepnessBefore);
            }
            else
            {
                toRet =  1f - func(1f - x, 1f - threshold, steepnessAfter);
            }
;
            return toRet;
         
        };
        
       return sFunc;
    }

    // In Degrees
    public static float AngleOfDirection(Vector2 direction)
    {
        var cosSolution1 = (Mathf.Acos(direction.x) * 180f / Mathf.PI);
        var sinSolution1 = (Mathf.Asin(direction.y) * 180f / Mathf.PI);
        var sinSolution2 = Mathf.Sign(sinSolution1) * (90f - Mathf.Abs(sinSolution1) + 90f);

        //Debug.Log("Acos = " + cosSolution1 + ", " + -cosSolution1 + ", Asin = " + sinSolution1 + ", " + sinSolution2 + ", Direction = (" + direction.x + ", " + direction.y + ")");
        var distance1 = Mathf.Abs(Mathf.Abs(cosSolution1) - Mathf.Abs(sinSolution1));
        var distance2 = Mathf.Abs(Mathf.Abs(cosSolution1) - Mathf.Abs(sinSolution2));
        if (distance1 < distance2)
        {
            return sinSolution1;
        }
        else
        {
            return sinSolution2;
        }
    }
    public static float MoveAngleTowardsAngle(float angle1, float angle2, float angleDelta)
    {
        var tempAngle1 = angle1;
        while (tempAngle1 > angle2)
        {
            tempAngle1 -= 360f;
        }
        while (angle2 - tempAngle1 > 360f)
        {
            tempAngle1 += 360f;
        }

        if (angle2 - tempAngle1 > 180f)
        {
            return tempAngle1 -= angleDelta;
        }
        else
        {
            return tempAngle1 += angleDelta;
        }

    }

    public static B DoWhileAnd<A,B>(A seed, Func<A,B> doBody, Func<B, bool> whileCondition, Func<B,A> whileBody)
    {
        var current = seed;
        B toReturn = default(B);
        while (true)
        {
            toReturn = doBody(current);
            if (!whileCondition(toReturn))
            {
                break;
            }
            current = whileBody(toReturn);
        }
        return toReturn;
    }

    public interface Maybe<T>
    {
        T GetUnsafe();
        bool IsNull();
        Maybe<Y> Map<Y>(Func<T, Y> func);
        Maybe<Y> FlatMap<Y>(Func<T, Maybe<Y>> func);
    }
    public class Just<T> : Maybe<T>
    {
        private T Value;
        public Just(T t)
        {
            Value = t;
        }

        public T GetUnsafe()
        {
            return Value;
        }
        public bool IsNull()
        {
            return false;
        }
        public Maybe<Y> Map<Y>(Func<T, Y> func)
        {
            return new Just<Y>(func(Value));
        }
        public Maybe<Y> FlatMap<Y>(Func<T, Maybe<Y>> func)
        {
            return func(Value);
        }
    }
    public class Nothing<T> : Maybe<T>
    {
        public T GetUnsafe()
        {
            return default(T);
        }
        public bool IsNull()
        {
            return true;
        }
        public Maybe<Y> Map<Y>(Func<T, Y> func)
        {
            return new Nothing<Y>();
        }
        public Maybe<Y> FlatMap<Y>(Func<T, Maybe<Y>> func)
        {
            return new Nothing<Y>();
        }
    }
    public static Maybe<T> NullToNothing<T>(T t)
    {
        if (t != null)
        {
            return new Just<T>(t);
        }
        else
        {
            return new Nothing<T>();
        }
    }

    public class Lazy<T>
    {
        private Maybe<T> Result;

        public delegate T Producer();
        private Producer Produce;

        public Lazy(Producer produce)
        {
            Result = new Nothing<T>();
            Produce = produce;
        }

        public T Get()
        {
            if (Result.IsNull())
            {
                Result = new Just<T>(Produce());
            }
            return Result.GetUnsafe();
        }
    }

    /*
     * Input:
     * seed is the starting point for the iteration
     *    optimal seed picking depends on the nature of the function f(x) = x - backwards(forwards(x))
     *    concave up functions should have a seed less than the expected solution
     *    concave down functions should have a seed greater tahn the expected solution
     *    if the function is neither concave up nor concave down on the large scale
     *       then this algorithm is not well suited for finding a solution
     *    also it works better with not very functions that are less steep than f(x) = x
     * forwards uses a guess at part of a solution to calculate the other parts of that solution (if it were true)
     * backwards uses the parts of the solution to calculate what the guess must have been
     * tolerance is how close the guess and the derivation need to be to conclude
     * 
     * Output:
     * returns the solution that has a consistency between the guess and the derivation of what the guess must have been
     *
     * Algorithm:
     * Uses fixed point iteration to find bounds on the solution
     * And then uses the bisection method to converge on the solution
    */
    public static Maybe<Output> FindConsistentSolution<Output>(float seed, Func<float, Maybe<Output>> forwards, Func<Output, Maybe<float>> backwards, float tolerance, int maxIterations = 20)
    {
        Maybe<Output> toReturn = new Nothing<Output>();

        var inputUsed = seed;
        Maybe<float> inputGotBackMaybe = new Just<float>(seed);
        int iterations = 0;

        Maybe<float> inputUsedToGetBackSmallestPositiveDifference = new Nothing<float>();
        Maybe<float> inputUsedToGetBackLargestNegativeDifference = new Nothing<float>();
        do
        {
            var inputGotBack = inputGotBackMaybe.GetUnsafe();
            var difference = inputGotBack - inputUsed;
            if (difference > 0f)
            {
                inputUsedToGetBackSmallestPositiveDifference = new Just<float>(inputUsed);
            }
            else if (difference < 0f)
            {
                inputUsedToGetBackLargestNegativeDifference = new Just<float>(inputUsed);
            }

            if (inputUsedToGetBackSmallestPositiveDifference.IsNull() || inputUsedToGetBackLargestNegativeDifference.IsNull())
            {
                inputUsed = inputGotBack; // Follow towards fixed point
            }
            else
            {
                return FindConsistentSolutionBisectionMethod(
                    inputUsedToGetBackLargestNegativeDifference.GetUnsafe(),
                    inputUsedToGetBackSmallestPositiveDifference.GetUnsafe(),
                    forwards, backwards, maxIterations - iterations);
            }

            var result = forwards(inputUsed);
            inputGotBackMaybe = result.FlatMap(backwards);
            if (!inputGotBackMaybe.IsNull())
            {
                toReturn = result;
            }

            iterations++;
        } while (!inputGotBackMaybe.IsNull() && Mathf.Abs(inputUsed - inputGotBackMaybe.GetUnsafe()) > tolerance && iterations < maxIterations);

        return toReturn;
    }
    public static Maybe<Output> FindConsistentSolutionBisectionMethod<Output>(float negativeStartingBound, float positiveStartingBound, Func<float, Maybe<Output>> forwards, Func<Output, Maybe<float>> backwards, float tolerance, int maxIterations = 20)
    {
        Maybe<Output> toReturn = new Nothing<Output>();

        var inputUsed = 0f;
        Maybe<float> inputGotBackMaybe = new Just<float>(0f);
        int iterations = 0;

        var negativeBound = negativeStartingBound;
        var positiveBound = positiveStartingBound;
        do
        {
            var inputGotBack = inputGotBackMaybe.GetUnsafe();
            var difference = inputGotBack - inputUsed;
            if (difference > 0f)
            {
                positiveBound = inputUsed;
            }
            else if (difference < 0f)
            {
                negativeBound = inputUsed;
            }

            inputUsed = (negativeBound + positiveBound) / 2f;

            var result = forwards(inputUsed);
            inputGotBackMaybe = result.FlatMap(backwards);
            if (!inputGotBackMaybe.IsNull())
            {
                toReturn = result;
            }

            iterations++;
        } while (!inputGotBackMaybe.IsNull() && Mathf.Abs(inputUsed - inputGotBackMaybe.GetUnsafe()) > tolerance && iterations < maxIterations);

        return toReturn;
    }

    public static float DeLerp(float min, float max, float lerped)
    {
        return (lerped - min) / (max - min);
    }
    public static float ReLerp(float lerped, float originalMin, float originalMax, float newMin, float newMax)
    {
        return Mathf.Lerp(newMin, newMax, DeLerp(originalMin, originalMax, lerped));
    }

    public static float ClampAbs(float value, float min, float max)
    {
        var mag = Mathf.Abs(value);
        if (mag > max)
        {
            return value / mag * max;
        }
        else if (mag < min)
        {
            return value / mag * min;
        }
        else
        {
            return value;
        }
    }
    public static float SoftClamp(float value, float min, float max, float attenuation)
    {
        if (value > max)
        {
            return (value - max) * (1 - attenuation) + max;
        }
        if (value < min)
        {
            return (value - min) * (1 - attenuation) + min;
        }
        else
        {
            return value;
        }
    }

    public static float Remainder(float value, float divisor)
    {
        if (value >= 0f)
        {
            return value - Mathf.Floor(value / divisor) * divisor;
        }
        else
        {
            return value + Mathf.Ceil(value / divisor) * divisor;
        }
    }
    public static bool SameSign(float a, float b)
    {
        return (a * b) > 0f;
    }

    public static float SolveQuadratic(float a, float b, float c, out float solution2)
    {
        var discriminant = Mathf.Sqrt(b * b - 4f * a * c);
        solution2 = (-b - discriminant) / (2f * a);
        return (-b + discriminant) / (2f * a);
    }
    public static double SolveQuadratic(double a, double b, double c, out double solution2)
    {
        var discriminant = Math.Sqrt(b * b - 4d * a * c);
        solution2 = (-b - discriminant) / (2d * a);
        return (-b + discriminant) / (2d * a);
    }

    public static List<float> SolveCubic(float a, float b, float c, float d)
    {
        var del0 = b * b - 3 * a * c;
        var del1 = 2 * b * b * b - 9 * a * b * c + 27 * a * a * d;
        var bigC = Mathf.Pow((del1 +- Mathf.Sqrt(del1 * del1 - 4 * del0 * del0 * del0)) / 2, 1 / 3);

        var root1 = -1 / (3 * a) * (b + bigC + del0 / bigC);
        return new List<float>();
    }
    public static List<float> SolveDepressedCubic(float p, float q)
    {

        return new List<float>();
    }
    public static List<float> SolveDepressedQuartic(float p, float q, float r)
    {
        //using ferraris method
        return new List<float>();
    }

    public static bool RemoveNullObjects<T>(List<T> objects)
    {
        var removedAny = false;
        objects.RemoveAll((T objectToRemove) => {
            var isNull = objectToRemove == null || objectToRemove.Equals(null);
            removedAny = isNull || removedAny;
            return isNull;
        });
        return removedAny;
    }

    public static bool CastCollision(CapsuleCollider collider, Vector3 movement, int layerMask)
    {
        var point1 = collider.center;
        point1.y += (collider.height - collider.radius * 2) / 2;
        point1 = collider.transform.TransformPoint(point1); 

        var point2 = collider.center;
        point2.y -= (collider.height - collider.radius * 2) / 2;
        point2 = collider.transform.TransformPoint(point2);

        var worldRadius = collider.transform.lossyScale.x * collider.radius;

        return Physics.CapsuleCast(point1, point2, worldRadius, movement.normalized, movement.magnitude, layerMask);
    }
    public static bool CheckCollision(CapsuleCollider collider, Vector3 movement, int layerMask)
    {
        var point1 = collider.center;
        point1.y += (collider.height - collider.radius * 2) / 2;
        point1 = collider.transform.TransformPoint(point1) + movement; 

        var point2 = collider.center;
        point2.y -= (collider.height - collider.radius * 2) / 2;
        point2 = collider.transform.TransformPoint(point2) + movement;

        var worldRadius = collider.transform.lossyScale.x * collider.radius;

        return Physics.CheckCapsule(point1, point2, worldRadius, layerMask);
    }
    public static List<Collider> OverlappingCollision(CapsuleCollider collider, Vector3 movement, int layerMask)
    {
        var point1 = collider.center;
        point1.y += (collider.height - collider.radius * 2) / 2;
        point1 = collider.transform.TransformPoint(point1) + movement; 

        var point2 = collider.center;
        point2.y -= (collider.height - collider.radius * 2) / 2;
        point2 = collider.transform.TransformPoint(point2) + movement;

        var worldRadius = collider.transform.lossyScale.x * collider.radius;

        return Physics.OverlapCapsule(point1, point2, worldRadius, layerMask).ToList();
    }
    public static bool CheckCollision(SphereCollider collider, Vector3 movement, int layerMask)
    {
        //var worldRadius = collider.transform.lossyScale.x * collider.radius;
        var newCenter = collider.transform.TransformPoint(collider.center) + movement;
        return Physics.CheckSphere(newCenter, collider.radius, layerMask);
    }

    public class AnonymousComparer<T> : IComparer<T>
    {
        private Func2<T, T, int> F; 

        public AnonymousComparer(Func2<T, T, int> f)
        {
            F = f;
        }

        public int Compare(T x, T y)
        {
            return F(x, y);
        }
    }
    public static IComparer<T> Lexicographicly<T>(Func2<T, T, int> first, Func2<T, T, int> second)
    {
        return new AnonymousComparer<T>((a, b) => {
            var firstComparison = first(a, b);
            if (firstComparison == 0)
            {
                return second(a, b);
            }
            return firstComparison;
        });
    }
    public static IComparer<T> Lexicographicly<T>(List<Func2<T, T, int>> funcs)
    {
        return new AnonymousComparer<T>((a, b) => {
            foreach (var f in funcs)
            {
                var comparison = f(a, b);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return 0;
        });
    }

    public class Reference<T>
    {
        private Func<T> Getter;
        private Action<T> Setter;

        public Reference(Func<T> getter, Action<T> setter) 
        {
            Getter = getter;
            Setter = setter;
        }

        public T Value
        {
            get { return Getter(); }
            set { Setter(value); }
        }

        public static implicit operator T(Reference<T> r) { return r.Value; }
    }

    public class BiFunction<T>
    {
        public Func<T, T> Forward;
        public Func<T, T> Backward;
        public BiFunction(Func<T, T> forward, Func<T, T> backward)
        {
            Forward = forward;
            Backward = backward;
        }
        public BiFunction<T> Reverse
        {
            get { return new BiFunction<T>(Backward, Forward); }
        }

        public static BiFunction<T> Identity()
        {
            return new BiFunction<T>((a) => a, (b) => b);
        } 

        public static BiFunction<float> Exponential(float baseOf)
        {
            return new BiFunction<float>(
                (a) => Mathf.Pow(baseOf, a),
                (b) => Mathf.Log(b) / Mathf.Log(baseOf)
            );
        }
        public static BiFunction<float> Logarithmic(float power)
        {
            return BiFunction<float>.Exponential(power).Reverse;
        }

    }

    public static float Round(float number, int decimalPlaces)
    {
        if (decimalPlaces >= 0)
        {
            var shiftFactor = Mathf.Pow(10, decimalPlaces);
            return Mathf.Round(number * shiftFactor) / shiftFactor;
        }
        return number;
    }

    /*public static Tuple<float,float> ElasticCollision(float massA, float velocityAOriginal, float massB, float velocityBOriginal)
    {
        var massTotal = massA + massB;
        var momentumTotal = massA * velocityAOriginal + massB * velocityBOriginal;
        var velocityDeltaOriginal = velocityBOriginal - velocityAOriginal;
        var velocityBAfter = (momentumTotal - massA * velocityDeltaOriginal) / massTotal;
        var velocityAAfter = velocityDeltaOriginal + velocityBAfter;
        return new Tuple<float, float>(velocityAAfter, velocityBAfter);
    }*/
    public static Tuple<float,float> ElasticCollision(float massA, float velocityAOriginal, float massB, float velocityBOriginal)
    {
        // Swap momentums in center of momentum reference frame
        // Works because of conservation of momentum
        var velocityTotal = velocityAOriginal / massB + velocityBOriginal / massA;
        var toCOMFrame = new BiFunction<float>((a) => a - velocityTotal, (b) => b + velocityTotal);
        var velocityAAfter = toCOMFrame.Backward( toCOMFrame.Forward(velocityBOriginal) * (massB / massA) ); 
        var velocityBAfter = toCOMFrame.Backward( toCOMFrame.Forward(velocityAOriginal) * (massA / massB) );
        return new Tuple<float, float>(velocityAAfter, velocityBAfter);
    }
    public static Tuple<Vector3, Vector3> ElasticCollision(Vector3 normal, float massA, Vector3 velocityAOriginal, float massB, Vector3 velocityBOriginal)
    {
        var planeBasis = VectorHelper.OrthogonalPlaneBasis(normal);
        var basis = new VectorHelper.Matrix3x3(planeBasis.XCol, planeBasis.YCol, normal);
        var projectedVelocityA = VectorHelper.ProjectOntoBasis(velocityAOriginal, basis);
        var projectedVelocityB = VectorHelper.ProjectOntoBasis(velocityBOriginal, basis);
        var velocitiesNormalAfter = ElasticCollision(massA, projectedVelocityA.z, massB, projectedVelocityB.z);
        projectedVelocityA.z = velocitiesNormalAfter.Item1;
        projectedVelocityB.z = velocitiesNormalAfter.Item2;
        var velocityAAfter = VectorHelper.ProjectFromBasis(projectedVelocityA, basis);
        var velocityBAfter = VectorHelper.ProjectFromBasis(projectedVelocityB, basis);
        return new Tuple<Vector3, Vector3>(velocityAAfter, velocityBAfter);
    }

}
