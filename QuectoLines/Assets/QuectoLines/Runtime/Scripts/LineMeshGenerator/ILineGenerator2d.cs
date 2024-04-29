using System.Collections.Generic;
using UnityEngine;

namespace Redmond.QuectoLines
{
    public interface ILineGenerator2d
    {
        void SetPosition(int index, Vector2 position);
        void SetPositions(Vector2[] positions);
        void SetPositions(IEnumerable<Vector2> positions);
    }
}
