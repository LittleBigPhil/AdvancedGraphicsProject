using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class TreeInterpreter : MonoBehaviour
{


    [Range(0.25f,2f)]
    public float LeafScale = .8f;
    [Range(0.125f,2f)]
    public float MaxLeafSize = .5f;
    [Range(0.25f,2f)]
    public float ContinueStep = 1f;
    [Range(0f,1f)]
    public float MinContinueScaleFactor = .9f;
    [Range(0f,2f)]
    public float MaxContinueScaleFactor = 1f;
    [Range(0f,1f)]
    public float BranchScaleFactor = .7f;
    [Range(0f,120f)]
    public float BranchMinAngle = 30f;
    [Range(0f,120f)]
    public float BranchMaxAngle = 90f;
    [Range(0f,90f)]
    public float ContinueMaxAngle = 15f;
    [Range(0f,30f)]
    public float BiasAngle = 5f;
    public Vector3 BiasDirection = Vector3.up;

    public bool SaveMesh = false;


    private System.Random Rand; 

    private Dictionary<string, Action> CommandsD;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, transform.lossyScale.x * .4f); 
    }

    private void Awake()
    {
        Rand = new System.Random(DateTime.Now.Millisecond + gameObject.GetInstanceID());

        CommandsD = new Dictionary<string, Action>()
        {
            ["-"] = () => DrawStack.Peek().Scale *= MinContinueScaleFactor,
            ["+"] = () => DrawStack.Peek().Scale /= MinContinueScaleFactor,
            ["Continue"] = () =>
            {
                var s = DrawStack.Peek();
                s.Scale *= Mathf.Lerp(MinContinueScaleFactor, MaxContinueScaleFactor, (float)Rand.NextDouble());
                DrawLine(s.Direction);

                var rotation = RandomRotationFrom(0f, ContinueMaxAngle, s.Direction);
                s.BranchDirection = rotation * s.BranchDirection;
                s.Direction = rotation * s.Direction;

                rotation = Quaternion.FromToRotation(s.Direction, BiasDirection);
                rotation.ToAngleAxis(out var angle, out var axis);
                rotation = Quaternion.AngleAxis(Mathf.Min(BiasAngle, angle), axis);

                s.BranchDirection = rotation * s.BranchDirection;
                s.Direction = rotation * s.Direction;
            },
            ["("] = () =>
            {
                var s = DrawStack.Peek().Clone();
                DrawStack.Push(s);
            },
            ["Branch("] = () =>
            {
                var s = DrawStack.Peek().Clone();
                var rotation = RandomRotationFrom(BranchMinAngle, BranchMaxAngle, s.Direction);

                var branchDirection = rotation * s.Direction;
                DrawStack.Peek().BranchDirection = branchDirection;
                s.Direction = branchDirection;

                s.Scale *= BranchScaleFactor;
                DrawStack.Push(s);
            },
            ["Oppose("] = () =>
            {
                var s = DrawStack.Peek().Clone();
                var branchDirection = Quaternion.AngleAxis(180f, s.Direction) * s.BranchDirection;
                DrawStack.Peek().BranchDirection = branchDirection;
                s.Direction = branchDirection;
                s.Scale *= BranchScaleFactor;
                DrawStack.Push(s);
            },
            ["R120("] = () =>
            {
                var s = DrawStack.Peek().Clone();
                var branchDirection = Quaternion.AngleAxis(120f, s.Direction) * s.BranchDirection;
                DrawStack.Peek().BranchDirection = branchDirection;
                s.Direction = branchDirection;
                s.Scale *= BranchScaleFactor;
                DrawStack.Push(s);
            },
            ["RG("] = () =>
            {
                var s = DrawStack.Peek().Clone();
                var angle = .618f * 360f; // Golden ratio (by percentage) in degrees
                var branchDirection = Quaternion.AngleAxis(angle, s.Direction) * s.BranchDirection;
                DrawStack.Peek().BranchDirection = branchDirection;
                s.Direction = branchDirection;
                s.Scale *= BranchScaleFactor;
                DrawStack.Push(s);
            },
            ["Leaf"] = () => DrawLeaf(DrawStack.Peek().Direction),
            [")"] = () =>
            {
                DrawClose();
                DrawStack.Pop();
            }
        };

        GetComponent<LSystem>().ConsumeGenerated += (List<string> generated) => CreateRenderers(generated);
    }

    private class DrawingState
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 BranchDirection;
        public int Index;
        public float Scale;
        public int UVOffset;

        public DrawingState Clone()
        {
            return new DrawingState()
            {
                Position = Position,
                Direction = Direction,
                BranchDirection = BranchDirection,
                Index = Index,
                Scale = Scale,
                UVOffset = UVOffset
            };
        }
    }
    private Stack<DrawingState> DrawStack = new Stack<DrawingState>();
    private List<Vector3> Vertices = new List<Vector3>();
    private List<Vector2> UVs = new List<Vector2>();
    private List<int> MainIndices = new List<int>();
    private List<int> LeafIndices = new List<int>();

    private void CreateRenderers(List<string> generated)
    {

        DrawStack.Push(new DrawingState()
        {
            Position = Vector3.zero,
            Direction = Vector3.up,
            Index = 0,
            Scale = 1f,
            UVOffset = 0
        });

        var count = AddMainVertices(Vector3.zero, Vector3.up, 1f);
        AddLineUVs(0, count);


        foreach (var s in generated)
        {
            var isCommand = CommandsD.TryGetValue(s, out var interpetation);
            if (isCommand)
            {
                interpetation();
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(Vertices);
        mesh.subMeshCount = 2;
        mesh.SetIndices(MainIndices, MeshTopology.Triangles, 0);
        mesh.SetIndices(LeafIndices, MeshTopology.Triangles, 1);
        mesh.SetUVs(0, UVs);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshFilter>().mesh = mesh;
        var collider = GetComponent<MeshCollider>();
        if (collider) {
            collider.sharedMesh = mesh;
        }
        if (SaveMesh)
        {
            AssetDatabase.CreateAsset(mesh, "Assets/Meshes/" + gameObject.name + gameObject.GetInstanceID() + ".asset");
        }
    }

    private void DrawLine(Vector3 direction)
    {
        var current = DrawStack.Peek();
        current.UVOffset++;

        var nextPosition = current.Position + direction * current.Scale * ContinueStep;
        var nextIndex = Vertices.Count;
        var count = AddMainVertices(nextPosition, direction, current.Scale);
        AddLineUVs(current.UVOffset, count);
        AddAllLineIndices(current.Index, nextIndex, count);

        current.Position = nextPosition;
        current.Index = nextIndex;
        current.Direction = direction;
    }

    private void DrawLeaf(Vector3 direction)
    {
        var current = DrawStack.Peek();

        var nextPosition = current.Position + direction * Mathf.Min(current.Scale * 10f, MaxLeafSize) * LeafScale;
        var nextIndex = Vertices.Count;
        AddBillboardVertices(current.Position, nextPosition, direction, current.Scale);
        AddBillboardIndices(nextIndex);
        AddLeafUVs();
    }

    private void DrawClose()
    {
        var current = DrawStack.Peek();
        current.UVOffset++;
        var count = 2;
        AddAllLineIndices(current.Index, current.Index + 2, count);
    }

    private List<Vector3> ThicknessOffsets(float scale, Vector3 direction)
    {
        var plane = VectorHelper.OrthogonalPlaneBasis(direction);
        return new List<Vector3>() {
            scale * .2f * ( plane.XCol - plane.YCol),
            scale * .2f * ( plane.XCol + plane.YCol),
            scale * .2f * (-plane.XCol + plane.YCol),
            scale * .2f * (-plane.XCol - plane.YCol)
        };
    }
    private int AddMainVertices(Vector3 position, Vector3 direction, float scale)
    {
        var perturbances = ThicknessOffsets(scale, direction);
        foreach (var perturbance in perturbances)
        {
            Vertices.Add(position + perturbance);
        }
        return perturbances.Count;
    }

    private List<Vector3> BillboardOffsets(float scale, Vector3 direction)
    {
        var plane = VectorHelper.OrthogonalPlaneBasis(direction);
        var projectedUp = VectorHelper.ProjectOntoBasis(BiasDirection, plane).normalized;
        var projectedFlat = new Vector2(projectedUp.y, -projectedUp.x);
        var flat = VectorHelper.ProjectFromBasis(projectedFlat, plane);
        return new List<Vector3>() {
            Mathf.Min(scale * 10f, MaxLeafSize) * 1f * ( flat) * LeafScale,
            Mathf.Min(scale * 10f, MaxLeafSize) * 1f * (-flat) * LeafScale,
        };
    }
    private void AddBillboardVertices(Vector3 position, Vector3 nextPosition, Vector3 direction, float scale)
    {
        var perturbances = BillboardOffsets(scale, direction);
        foreach (var perturbance in perturbances)
        {
            Vertices.Add(position + perturbance);
        }
        foreach (var perturbance in perturbances)
        {
            Vertices.Add(nextPosition + perturbance);
        }
    }
    private void AddBillboardIndices(int start)
    {
        var oldStart = start;
        var newStart = start + 2;

        LeafIndices.Add(oldStart);
        LeafIndices.Add(newStart);
        LeafIndices.Add(oldStart + 1);

        LeafIndices.Add(newStart);
        LeafIndices.Add(newStart + 1);
        LeafIndices.Add(oldStart + 1);
    }

    private void AddAllLineIndices(int oldStart, int newStart, int n)
    {
        for (int i = 0; i < n; i++)
        {
            AddLineIndices(oldStart, newStart, i, n);
        }
    }
    private void AddLineIndices(int oldStart, int newStart, int offset, int n)
    {
        var biggerOffset = (offset + 1) % n;

        MainIndices.Add(oldStart + offset);
        MainIndices.Add(newStart + offset);
        MainIndices.Add(oldStart + biggerOffset);

        MainIndices.Add(newStart + offset);
        MainIndices.Add(newStart + biggerOffset);
        MainIndices.Add(oldStart + biggerOffset);
    }

    private void AddLeafUVs()
    {
        UVs.Add(new Vector2(0f, 0f));
        UVs.Add(new Vector2(1f, 0f));
        UVs.Add(new Vector2(0f, 1f));
        UVs.Add(new Vector2(1f, 1f));
    }

    private void AddLineUVs(int yOffset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var xOffset = ((float)i) / (count - 1);
            UVs.Add(new Vector2(xOffset, yOffset));
        }
    }


    private Quaternion RandomRotationFrom(float minAngle, float maxAngle, Vector3 fromDirection)
    {
        var basis = VectorHelper.OrthogonalPlaneBasis(fromDirection);
        var planeTheta = Mathf.PI * (float)Rand.NextDouble() / 2f;
        var planeVec = new Vector2(Mathf.Cos(planeTheta), Mathf.Sin(planeTheta));
        var rotationAxis = VectorHelper.ProjectFromBasis(planeVec, basis);
        var rotationSign = Mathf.Sign((float)Rand.NextDouble() - .5f);
        var rotationTheta = rotationSign * Mathf.Lerp(minAngle, maxAngle, (float)Rand.NextDouble());
        var rotation = Quaternion.AngleAxis(rotationTheta, rotationAxis);
        return rotation;
    }
}














