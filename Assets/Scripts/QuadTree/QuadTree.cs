
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class QuadTree : MonoBehaviour
{
    public const int DIMENSION = 64;
    public const float CHUNKSIZE = 12.5f;
    public const int DEPTH = 7;
    public float FineDetailDistance = 50f;
    public float MediumDetialDistance = 100f;
    public float LowDetialDistance = 200f;
    public GameObject Target;
    public Material BufferMaterial;
    private Node _root;
    private List<Node> _nodes = new List<Node>();
    private FlexTileVisual _tileVisual;
    private FlexTileVisual.TileData[] _tileData;

    private float _gridSize;


    private void OnEnable()
    {
        _tileVisual = new FlexTileVisual(BufferMaterial);
        _gridSize = DIMENSION * CHUNKSIZE;

        _tileVisual.Initialize();
    }

    private void OnDisable()
    {
        _tileVisual.Dispose();
        _nodes.Clear();

    }
    void Update()
    {


        UpdateTree();
        _tileVisual.SetData(_tileData);

        _tileVisual.Draw();

    }
    private void UpdateTree() 
    {
        Vector2 pos = new Vector2(Target.transform.position.x, Target.transform.position.z);
        _root = new Node(-_gridSize, _gridSize, _gridSize, -_gridSize, 0);
        _nodes.Clear();
        PropergateQuadTree(_root);

        _tileData = new FlexTileVisual.TileData[_nodes.Count];
        for (int i = 0; i < _tileData.Length; i++)
        {
            _tileData[i].Pos = new Vector2 (_nodes[i].Bound.center.x, _nodes[i].Bound.center.z) ;
            _tileData[i].Size = _nodes[i].Size;
            _tileData[i].Color = 
                _nodes[i].Excluded ?
                new Vector3(1, 0, 0): 
                Vector3.Lerp ( new Vector3 (1,0,0), new Vector3(0,1,0),(float)_nodes[i].Depth / DEPTH);
        }
        print($"Visiable Chunks : {_nodes.Where(x => !x.Excluded).ToList().Count}; " +
            $"Excluded Chunks : {_nodes.Where(x => x.Excluded).ToList().Count}");
    }
    public void PropergateQuadTree(Node current) 
    {
        if (current.Depth >= DEPTH)
            return;
        current.SpawnChild();
        foreach (Node c in current.Children)
        {
            if (c == null) continue;
            if (BoxSDF(c.L, c.R, c.T, c.B, new Vector2(Target.transform.position.x, Target.transform.position.z)) < LowDetialDistance
                && c.Depth < DEPTH - 2)
                    PropergateQuadTree(c);
            else if (BoxSDF(c.L, c.R, c.T, c.B, new Vector2(Target.transform.position.x, Target.transform.position.z)) < MediumDetialDistance
                && c.Depth < DEPTH - 1)
                    PropergateQuadTree(c);
            else if (BoxSDF(c.L, c.R, c.T, c.B, new Vector2(Target.transform.position.x, Target.transform.position.z)) < FineDetailDistance
                && c.Depth < DEPTH)
                    PropergateQuadTree(c);
            else
                _nodes.Add(c);
                c.Excluded = !TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Target.GetComponent<Camera>()), c.Bound);



        }
    }

    public static bool TestPlanesAABB(Plane[] planes, Bounds bounds)
    {
        Vector3[] corners = GetCorners(bounds);
        foreach (Plane plane in planes)
        {
            bool atLeastOneInside = false;
            foreach (Vector3 corner in corners)
            {
                if (plane.GetDistanceToPoint(corner) >= 0)
                {
                    atLeastOneInside = true;
                    break;
                }
            }
            if (!atLeastOneInside)
            {
                return false;
            }
        }
        return true;
    }
    private static Vector3[] GetCorners(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        return new Vector3[]
        {
            new Vector3(min.x, min.y, min.z), 
            new Vector3(max.x, min.y, min.z), 
            new Vector3(min.x, max.y, min.z), 
            new Vector3(max.x, max.y, min.z), 
            new Vector3(min.x, min.y, max.z), 
            new Vector3(max.x, min.y, max.z), 
            new Vector3(min.x, max.y, max.z), 
            new Vector3(max.x, max.y, max.z)  
        };
    }
    public static float BoxSDF(float left, float right, float top, float bot, Vector2 point)
    {
        Vector2 boxCenter = new Vector2((left + right) / 2f, (top + bot) / 2f);
        Vector2 boxHalfSize = new Vector2((right - left) / 2f, (top - bot) / 2f);

        Vector2 distance = new Vector2(
            Mathf.Abs(point.x - boxCenter.x) - boxHalfSize.x,
            Mathf.Abs(point.y - boxCenter.y) - boxHalfSize.y
        );

        Vector2 outside = Vector2.Max(distance, Vector2.zero);
        float inside = Mathf.Min(Mathf.Max(distance.x, distance.y), 0f);
        return outside.magnitude + inside;
    }
    public class Node 
    {
        public float L, R, T, B;
        public Vector2 Position;
        public float Size;
        public int Quadrant;
        public Node[] Children;
        public int Depth;
        public bool Excluded;
        public Bounds Bound;

        public Node(float l, float r,float t, float b, int depth) 
        {
            L = l; R = r; T = t; B = b;
            Position = new Vector2(l + (r - l) / 2, b + (t - b) / 2);
            Size = L - R;
            Bound.center = new Vector3(Position.x, 0, Position.y);
            Bound.size = new Vector3 (Size,1f,Size);
            Depth = depth;
        }

        public void SpawnChild() 
        {

                Children = new Node[4];

                Children[0] = new Node(L, Position.x, Position.y, B, Depth + 1);
                Children[1] = new Node(L, Position.x, T, Position.y, Depth + 1);
                Children[2] = new Node(Position.x, R, T, Position.y, Depth + 1);
                Children[3] = new Node(Position.x, R, Position.y, B, Depth + 1);

        }
    }
}
