using UnityEngine;

namespace U1W
{
    [ExecuteInEditMode]
    public class LineControllerTwoPoints : MonoBehaviour
    {
        [SerializeField] private LineMeshGenerator line;
        [SerializeField] private Vector2 endA = Vector2.zero;
        [SerializeField] private Vector2 endB = Vector2.right;
        private readonly Vector2[] ends = new Vector2[2];

        public Vector2 EndA
        {
            get => endA; 
            set => endA = value;
        }
        public Vector2 EndB
        {
            get => endB;
            set => endB = value;
        }

        private void Update()
        {
            if (line == null) return;
            ends[0] = endA;
            ends[1] = endB;
            line.SetPositions(ends);
        }
    }
}
