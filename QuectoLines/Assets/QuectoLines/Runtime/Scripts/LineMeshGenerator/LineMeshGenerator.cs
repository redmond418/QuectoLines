using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

namespace Redmond.QuectoLines
{
    [ExecuteInEditMode]
    public class LineMeshGenerator : MonoBehaviour, ILineGenerator2d
    {
        const int LINE_VERTEX_COUNT = 4;
        const int LINE_TRIANGLES_COUNT = 6;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private int orderInLayer = 0;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float thickness = 0.5f;
        [SerializeField] private bool connectSamePoints = true;
        [SerializeField] private List<Vector2> positions = new() { Vector2.zero, Vector2.right, Vector2.up };
        [SerializeField] private int endCapDivisionCount;
        [SerializeField] private float endCapStretch = 1f;
        [SerializeField] private CornerMode cornerMode;
        [SerializeField, ShowIf(nameof(IsCornerDivision))] private int cornerDivisionCount;
        private readonly List<Vector3> verticesPosition = new();
        private readonly List<int> triangles = new();
        private readonly List<Color> colors = new();
        private Mesh mesh;
        private int previousVerteicesCount;

        private bool IsCornerDivision => cornerMode == CornerMode.Division;

        private int CornerVertexCount => cornerMode switch
        {
            CornerMode.None => 0,
            CornerMode.Intersection => 2,
            CornerMode.Division => cornerDivisionCount >= 1 ? cornerDivisionCount : 1,
            _ => 0,
        };

        private int CornerTrianglesCount => cornerMode switch
        {
            CornerMode.None => 0,
            CornerMode.Intersection => 6,
            CornerMode.Division => cornerDivisionCount >= 1 ? cornerDivisionCount * 3 : 3,
            _ => 0,
        };

        public List<Vector2> Positions => positions;

        public Color Color
        {
            get => color;
            set => color = value;
        }

        public float Thickness
        {
            get => thickness;
            set => thickness = value;
        }

        public int EndCapDivisionCount
        {
            get => endCapDivisionCount;
            set => endCapDivisionCount = value;
        }

        public float EndCapStretch
        {
            get => endCapStretch;
            set => endCapStretch = value;
        }

        public CornerMode CornerMode
        {
            get => cornerMode; 
            set => cornerMode = value;
        }

        public int CornerDivisionCount
        {
            get => cornerDivisionCount;
            set => cornerDivisionCount = value;
        }

        private void Start()
        {
            MeshInitialize();
        }

        [Button]
        private void MeshInitialize()
        {
            if (meshFilter is null) return;
            mesh = new Mesh();
            meshFilter.mesh = mesh;
        }

        private void Update()
        {
            if(meshFilter == null) return;
            if (mesh == null) MeshInitialize();
            if (meshRenderer != null) meshRenderer.sortingOrder = orderInLayer;
            if (connectSamePoints && positions.Count >= 2) for (int i = positions.Count - 2; i >= 0; i--)
                {
                    if (positions[i] == positions[i + 1]) positions.RemoveAt(i + 1);
                }
            verticesPosition.Clear();
            triangles.Clear();
            if (positions.Count < 2)
            {
                mesh.SetTriangles(triangles, 0);
                mesh.SetVertices(verticesPosition);
                previousVerteicesCount = verticesPosition.Count;
                return;
            }
            for (int i = 0; i < positions.Count - 1; i++)
            {
                if (positions[i] == positions[i + 1]) continue;
                if(i > 0)
                {
                    for (int j = 0; j < CornerVertexCount; j++)
                    {
                        verticesPosition.Add(Vector3.zero);
                    }
                    for (int j = 0; j < CornerTrianglesCount; j++)
                    {
                        triangles.Add(0);
                    }
                }
                var rightNormal = (positions[i + 1] - positions[i]).normalized;
                rightNormal = new(rightNormal.y, -rightNormal.x);
                int vertexIndex = i * (LINE_VERTEX_COUNT + CornerVertexCount);
                //int triangleIndex = i * (LINE_TRIANGLES_COUNT + CornerTrianglesCount);
                verticesPosition.Add(positions[i] + rightNormal * thickness);
                verticesPosition.Add(positions[i + 1] + rightNormal * thickness);
                verticesPosition.Add(positions[i] - rightNormal * thickness);
                verticesPosition.Add(positions[i + 1] - rightNormal * thickness);
                triangles.Add(vertexIndex + 0);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                if (i > 0) switch (cornerMode)
                    {
                        case CornerMode.Intersection:
                            if (IsLeftCorner(i, vertexIndex))
                            {
                                GenerateCornerByIntersection(i, vertexIndex - CornerVertexCount - 1, vertexIndex + 2, positions[i],
                                    positions[i] - positions[i - 1], positions[i] - positions[i + 1]);
                            }
                            else
                            {
                                GenerateCornerByIntersection(i, vertexIndex, vertexIndex - CornerVertexCount - 3, positions[i],
                                    positions[i] - positions[i + 1], positions[i] - positions[i - 1]);
                            }
                            break;
                        case CornerMode.Division:
                            if(IsLeftCorner(i, vertexIndex))
                            {
                                GenerateCornerByDivision(i, vertexIndex - CornerVertexCount - 1, vertexIndex + 2,
                                    positions[i], cornerDivisionCount);
                            }
                            else
                            {
                                GenerateCornerByDivision(i, vertexIndex, vertexIndex - CornerVertexCount - 3,
                                    positions[i], cornerDivisionCount);
                            }
                            break;
                        case CornerMode.None:
                        default:
                            break;
                    }
            }
            GenerateEndCap(verticesPosition.Count - 1, verticesPosition.Count - 3, positions[^1], positions[^1] - positions[^2], thickness, endCapStretch, endCapDivisionCount);
            GenerateEndCap(0, 2, positions[0], positions[0] - positions[1], thickness, endCapStretch, endCapDivisionCount);
            
            if(colors.Count > 0 && colors[0] != color)
            {
                for (int i = 0; i < colors.Count; i++)
                {
                    colors[i] = color;
                }
            }
            for (int i = colors.Count; i < verticesPosition.Count; i++)
            {
                colors.Add(color);
            }
            for (int i = colors.Count - 1; i >= verticesPosition.Count; i--)
            {
                colors.RemoveAt(colors.Count - 1);
            }

            if(previousVerteicesCount <= verticesPosition.Count)
            {
                mesh.SetVertices(verticesPosition);
                mesh.SetTriangles(triangles, 0);
                mesh.SetColors(colors);
            }
            else
            {
                mesh.SetTriangles(triangles, 0);
                mesh.SetVertices(verticesPosition);
                mesh.SetColors(colors);
            }
            mesh.RecalculateBounds();
            previousVerteicesCount = verticesPosition.Count;
        }

        private bool IsLeftCorner(int i, int vertexIndex) =>
            Vector3.Cross(positions[i] - positions[i - 1], verticesPosition[vertexIndex + 2] - verticesPosition[vertexIndex - CornerVertexCount - 1]).z *
            Vector3.Cross(positions[i] - positions[i - 1], verticesPosition[vertexIndex + 3] - verticesPosition[vertexIndex - CornerVertexCount - 1]).z > 0;

        private void GenerateCornerByIntersection(int index, int vertexIndexL, int vertexIndexR, Vector2 center, Vector2 directionL, Vector2 directionR)
        {
            if(directionL == directionR || directionL == -directionR) return;
            Vector2 positionL = verticesPosition[vertexIndexL];
            Vector2 positionR = verticesPosition[vertexIndexR];
            int vertexIndex = (index - 1) * (LINE_VERTEX_COUNT + CornerVertexCount) + LINE_VERTEX_COUNT;
            int triangleIndex = (index - 1) * (LINE_TRIANGLES_COUNT + CornerTrianglesCount) + LINE_TRIANGLES_COUNT;
            /*float s = ((positionL.y - positionR.y) * directionR.x - (positionL.x - positionR.y) * directionR.y) / 
                (directionL.x * directionR.y - directionL.y * directionR.x);*/
            float x = (directionL.y * directionR.x * positionL.x - directionL.x * directionR.y * positionR.x - 
                directionL.x * directionR.x * (positionL.y - positionR.y)) / (directionL.y * directionR.x - directionL.x * directionR.y);
            float y = directionL.x != 0 ?
                (x - positionL.x) * directionL.y / directionL.x + positionL.y :
                (x - positionR.x) * directionR.y / directionR.x + positionR.y;
            verticesPosition[vertexIndex] = center;
            verticesPosition[vertexIndex + 1] = new(x, y);
            triangles[triangleIndex + 0] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndexL;
            triangles[triangleIndex + 2] = vertexIndex + 1;
            triangles[triangleIndex + 3] = vertexIndex;
            triangles[triangleIndex + 4] = vertexIndex + 1;
            triangles[triangleIndex + 5] = vertexIndexR;
        }

        private void GenerateCornerByDivision(int index, int vertexIndexL, int vertexIndexR, Vector2 center, int division)
        {
            if (division <= 0) return;
            Vector2 positionL = verticesPosition[vertexIndexL];
            Vector2 positionR = verticesPosition[vertexIndexR];
            var vertexIndex = (index - 1) * (LINE_VERTEX_COUNT + CornerVertexCount) + LINE_VERTEX_COUNT;
            var triangleIndex = (index - 1) * (LINE_TRIANGLES_COUNT + CornerTrianglesCount) + LINE_TRIANGLES_COUNT;
            var angleR = Mathf.Atan2(positionR.y - center.y, positionR.x - center.x);
            var angleL = Mathf.Atan2(positionL.y - center.y, positionL.x - center.x);
            var fixedAngle = (angleL - angleR) / (2 * Mathf.PI);
            angleL = angleR + (fixedAngle - Mathf.Floor(fixedAngle)) * 2 * Mathf.PI;
            var distanceR = (positionR - center).magnitude;
            var distanceL = (positionL - center).magnitude;
            verticesPosition[vertexIndex] = center;
            for (int i = 0; i < division; i++)
            {
                triangles[triangleIndex + i * 3 + 0] = vertexIndex;
                triangles[triangleIndex + i * 3 + 1] = i == 0 ? vertexIndexL : vertexIndex + i;
                triangles[triangleIndex + i * 3 + 2] = i == division - 1 ? vertexIndexR : vertexIndex + i + 1;
                if (i == division - 1) break;
                var ratio = (float)(i + 1) / division;
                var distance = Mathf.Lerp(distanceL, distanceR, (-Mathf.Cos(ratio * Mathf.PI) + 1) / 2);
                var angle = Mathf.Lerp(angleL, angleR, ratio);
                Vector2 pos = new(Mathf.Cos(angle) * distance + center.x, Mathf.Sin(angle) * distance + center.y);
                verticesPosition[vertexIndex + 1 + i] = pos;
            }
        }

        private void GenerateEndCap(int vertexIndexL, int vertexIndexR, Vector2 center, Vector2 direction, float radius, float stretch, int division)
        {
            if (division <= 1 || direction == Vector2.zero) return;
            direction = direction.normalized;
            Vector2 directionRight = new(direction.y, -direction.x);
            int vertexIndex = verticesPosition.Count;
            verticesPosition.Add(center);
            for (int i = 0; i < division; i++)
            {
                triangles.Add(vertexIndex);
                triangles.Add(i == 0 ? vertexIndexL : vertexIndex + i);
                triangles.Add(i == division - 1 ? vertexIndexR : vertexIndex + i + 1);
                if (i == division - 1) break;
                var ratio = (float)(i + 1) / division;
                verticesPosition.Add(center - directionRight * radius * Mathf.Cos(ratio * Mathf.PI) +
                    radius * Mathf.Sin(ratio * Mathf.PI) * stretch * direction);
            }
        }

        public void SetPosition(int index, Vector2 position)
        {
            if(index >=  verticesPosition.Count)
            {
                positions.Add(position);
                Debug.LogWarning("Index was positions count or more.");
            }
            else positions[index] = position;
        }

        public void SetPositions(Vector2[] positions)
        {
            this.positions.Clear();
            if (positions is null) return;
            foreach (var pos in positions)
            {
                this.positions.Add(pos);
            }
        }

        public void SetPositions(IEnumerable<Vector2> positions)
        {
            this.positions.Clear();
            if(positions is null) return;
            foreach (var pos in positions)
            {
                this.positions.Add(pos);
            }
        }
    }

    public enum CornerMode
    {
        None = 0,
        Intersection = 1,
        Division = 2,
    }
}
