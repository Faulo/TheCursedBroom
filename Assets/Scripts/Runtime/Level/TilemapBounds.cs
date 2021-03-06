using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheCursedBroom.Level {
    [Serializable]
    public class TilemapBounds {
        public event Action<Vector3Int> onLoadTiles;
        public event Action<Vector3Int> onDiscardTiles;

        [SerializeField]
        public bool enabled = true;
        [SerializeField, Range(1, 100)]
        public int width = 10;
        [SerializeField, Range(1, 100)]
        public int height = 10;
        public int tileCount => width * height * 4;
        public Vector3Int extends => new Vector3Int(width, height, 0);

        Vector3Int center;
        BoundsInt bounds;
        BoundsInt oldBounds;

        public void PrepareTiles(Vector3Int position) {
            center = position;
            UpdateBounds();

            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                position.x = x;
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    position.y = y;
                    onLoadTiles?.Invoke(position);
                }
            }
        }

        public int UpdateTiles(Vector3Int position) {
            int tilesChangedCount = 0;
            oldBounds = bounds;
            center = position;
            UpdateBounds();

            for (int x = oldBounds.xMin; x < oldBounds.xMax; x++) {
                position.x = x;
                for (int y = oldBounds.yMin; y < oldBounds.yMax; y++) {
                    position.y = y;
                    if (!bounds.Contains(position)) {
                        onDiscardTiles?.Invoke(position);
                        tilesChangedCount++;
                    }
                }
            }

            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                position.x = x;
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    position.y = y;
                    if (!oldBounds.Contains(position)) {
                        onLoadTiles?.Invoke(position);
                        tilesChangedCount++;
                    }
                }
            }

            return tilesChangedCount;
        }

        void UpdateBounds() {
            bounds.position = center - extends;
            bounds.size = enabled
                ? (2 * extends) + new Vector3Int(0, 0, 1)
                : Vector3Int.zero;
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        public static int TryGetShapes(HashSet<Vector3Int> positions, ref TileShape[] shapes) {
            int shapeCount = 0;
            foreach (var position in positions) {
                if (!positions.Contains(position + Vector3Int.left) && !positions.Contains(position + Vector3Int.down)) {
                    bool contains = false;
                    for (int i = 0; i < shapeCount; i++) {
                        if (shapes[i].ContainsPosition(position)) {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains) {
                        shapes[shapeCount++] = CreateTileShape(positions.Contains, position, Vector3Int.up);
                        if (shapeCount == shapes.Length) {
                            break;
                        }
                    }
                }
            }
            return shapeCount;
        }
        public static bool TryGetBounds(HashSet<Vector3Int> positions, out Vector3 offset, out Vector3 size) {
            if (positions.Count == 0) {
                offset = size = Vector3.zero;
                return false;
            }
            var bottomLeft = (Vector3)positions.First();
            var topRight = bottomLeft;
            foreach (var position in positions) {
                if (topRight.x < position.x) {
                    topRight.x = position.x;
                }
                if (topRight.y < position.y) {
                    topRight.y = position.y;
                }
                if (bottomLeft.x > position.x) {
                    bottomLeft.x = position.x;
                }
                if (bottomLeft.y > position.y) {
                    bottomLeft.y = position.y;
                }
            }
            offset = (topRight + bottomLeft + Vector3.one) / 2;
            size = topRight - bottomLeft + Vector3.one;
            return true;
        }
        static readonly Dictionary<Vector3Int, Vector2> offsets = new Dictionary<Vector3Int, Vector2> {
            [Vector3Int.right] = new Vector2(0, 1),
            [Vector3Int.down] = new Vector2(1, 1),
            [Vector3Int.left] = new Vector2(1, 0),
            [Vector3Int.up] = new Vector2(0, 0),
        };
        static readonly Dictionary<Vector3Int, Vector3Int> forwardRotation = new Dictionary<Vector3Int, Vector3Int> {
            [Vector3Int.right] = Vector3Int.down,
            [Vector3Int.down] = Vector3Int.left,
            [Vector3Int.left] = Vector3Int.up,
            [Vector3Int.up] = Vector3Int.right,
        };
        static readonly Dictionary<Vector3Int, Vector3Int> backwardRotation = new Dictionary<Vector3Int, Vector3Int> {
            [Vector3Int.right] = Vector3Int.up,
            [Vector3Int.down] = Vector3Int.right,
            [Vector3Int.left] = Vector3Int.down,
            [Vector3Int.up] = Vector3Int.left,
        };
        static TileShape CreateTileShape(Func<Vector3Int, bool> inBounds, Vector3Int startPosition, Vector3Int startDirection) {
            var shape = new TileShape();
            var position = startPosition;
            var direction = startDirection;
            do {
                if (inBounds(position + direction + backwardRotation[direction])) {
                    position += direction + backwardRotation[direction];
                    direction = backwardRotation[direction];
                    shape.AddCorner(position, offsets[direction]);
                } else if (inBounds(position + direction)) {
                    position += direction;
                } else {
                    direction = forwardRotation[direction];
                    shape.AddCorner(position, offsets[direction]);
                }
            } while (!(position == startPosition && direction == startDirection));
            return shape;
        }
    }
}