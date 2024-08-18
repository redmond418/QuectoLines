using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Splines;

namespace Redmond.QuectoLines
{
    [ExecuteInEditMode]
    public class LineControllerSpline : MonoBehaviour
    {
        [SerializeField] private LineMeshGenerator line;
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private int splineIndex;
        [SerializeField] private bool isLoop;
        [SerializeField] private bool useSplinePosition = true;
        [SerializeField] private bool useSplineRotation = true;
        [SerializeField/*, HorizontalGroup("ends"), Range(0, 2)*/] private float endA = 0;
        [SerializeField/*, HorizontalGroup("ends"), Range(0, 2)*/] private float endB = 1;
        [SerializeField] private DivideType divideMode;
        [SerializeField, ShowIf(nameof(IsEquidistantDivide))] private int divideCount;
        private readonly List<Vector2> positions = new();
        private readonly List<float> values = new();
        private readonly List<float> mediumPositionsCache = new();

        private bool IsEquidistantDivide() => divideMode == DivideType.Equidistant;

        public float EndA
        {
            get => endA;
            set => endA = value;
        }
        public float EndB
        {
            get => endB;
            set => endB = value;
        }

        private void Update()
        {
            if (line == null || splineContainer == null) return;
            switch (divideMode)
            {
                case DivideType.Equidistant:
                    if (divideCount <= 0) divideCount = 1;
                    values.Clear();
                    values.Add(endA);
                    for (int i = 1; i < divideCount; i++)
                    {
                        values.Add(Mathf.Lerp(endA, endB, (float)i / divideCount));
                    }
                    values.Add(endB);
                    break;
                case DivideType.Knots:
                    mediumPositionsCache.Clear();
                    float currentT = 0;
                    float length = splineContainer.Splines[splineIndex].GetLength();
                    float endA2 = isLoop ? endA - (int)endA : Mathf.Clamp01(endA);
                    float endB2 = isLoop ? endB - (int)endB : Mathf.Clamp01(endB);
                    bool inRange = endA2 <= endB2;
                    if (!inRange && !isLoop) endB2 = endA2;
                    int startIndex = -1;
                    for (int i = 0; i < splineContainer.Splines[splineIndex].GetCurveCount(); i++)
                    {
                        currentT += splineContainer.Splines[splineIndex].GetCurveLength(i) / length;
                        if (inRange && endA2 < currentT && currentT < endB2)
                        {
                            mediumPositionsCache.Add(currentT);
                        }
                        else if (isLoop && !inRange && (endA2 < currentT || currentT < endB2))
                        {
                            mediumPositionsCache.Add(currentT);
                            if(startIndex < 0 && endA2 < currentT) startIndex = mediumPositionsCache.Count - 1;
                        }
                    }
                    var count = mediumPositionsCache.Count;
                    values.Clear();
                    values.Add(endA2);
                    if(startIndex < 0) startIndex = 0;
                    for (int i = 0; i < count; i++)
                    {
                        values.Add(mediumPositionsCache[(i + startIndex) % count]);
                    }
                    values.Add(endB2);
                    break;
                default:
                    return;
            }
            for (int i = 0; i < values.Count; i++)
            {
                Vector2 pos = (Vector3)splineContainer.EvaluatePosition(splineIndex, isLoop ? values[i] - Mathf.Floor(values[i]) : Mathf.Clamp01(values[i]));
                if(!useSplinePosition) pos -= (Vector2)splineContainer.transform.position;
                if(!useSplineRotation) pos = Quaternion.Inverse(splineContainer.transform.rotation) * pos;
                if (i < positions.Count) positions[i] = pos;
                else positions.Add(pos);
            }
            for (int i = positions.Count - 1; i >= values.Count; i--)
            {
                positions.RemoveAt(i);
            }
            line.SetPositions(positions);
        }
    }

    public enum DivideType
    {
        Equidistant, Knots
    }
}
