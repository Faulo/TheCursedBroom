using System;
using System.Collections.Generic;
using System.Linq;
using Slothsoft.UnityExtensions;
using TheCursedBroom.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheCursedBroom.Level {
    [CreateAssetMenu()]
    public class TilemapLayerAsset : ScriptableObject {
        enum RenderMode {
            None,
            Chunk,
            Individual,
        }
        enum CollisionMode {
            None,
            Collision,
            Trigger,
        }
        enum ColliderBaker {
            Unity,
            Slothsoft,
        }
        const int TILE_MAX_COUNT = 100;

        public static TilemapLayerAsset[] all => Resources
            .LoadAll<TilemapLayerAsset>("TilemapLayers")
            .OrderBy(layer => layer.name)
            .ToArray();

        [Header("Tile settings")]
        [SerializeField, Expandable]
        Grid tilemapPalette = default;
        [SerializeField]
        Color tilemapColor = Color.white;
        [SerializeField, Layer]
        int tilemapLayer = default;
        public IEnumerable<TileBase> allowedTiles {
            get {
                var tilemap = tilemapPalette.GetComponentInChildren<Tilemap>();
                var tiles = new TileBase[TILE_MAX_COUNT];
                int count = tilemap.GetUsedTilesNonAlloc(tiles);
                return new HashSet<TileBase>(tiles.Take(count));
            }
        }

        [SerializeField]
        TileGroup[] synonymousTiles = new TileGroup[0];
        public TileComparer CreateTileComparer() {
            var comparer = new TileComparer();

            for (int i = 0; i < synonymousTiles.Length; i++) {
                foreach (var (one, two) in synonymousTiles[i].allTileCombinations) {
                    comparer.AddSynonym(one, two);
                }
            }

            return comparer;
        }

        [Header("Renderer settings")]
        [SerializeField]
        RenderMode renderMode = RenderMode.None;
        [SerializeField, SortingLayer]
        int renderSortingLayer = default;
        [SerializeField, Range(0, 100)]
        int renderOrderInLayer = 0;
        [SerializeField]
        Material renderMaterial = default;

        public Tilemap InstallTilemap(GameObject obj) {
            if (obj.name != name) {
                obj.name = name;
            }
            if (obj.layer != tilemapLayer) {
                obj.layer = tilemapLayer;
            }
            if (obj.transform.position != Vector3.zero) {
                obj.transform.position = Vector3.zero;
            }

            // Controller
            var controller = obj.GetOrAddComponent<TilemapController>();
            controller.type = this;

            // Tilemap
            var tilemap = obj.GetOrAddComponent<Tilemap>();
            tilemap.color = tilemapColor;

            // Renderer
            if (renderMode == RenderMode.None) {
                obj.DestroyComponent<TilemapRenderer>();
            } else {
                var renderer = obj.GetOrAddComponent<TilemapRenderer>();
                renderer.material = renderMaterial;
                renderer.sortingLayerID = renderSortingLayer;
                renderer.sortingOrder = renderOrderInLayer;
                switch (renderMode) {
                    case RenderMode.Chunk:
                        renderer.mode = TilemapRenderer.Mode.Chunk;
                        break;
                    case RenderMode.Individual:
                        renderer.mode = TilemapRenderer.Mode.Individual;
                        break;
                    default:
                        throw new NotImplementedException(renderMode.ToString());
                }
            }

            return tilemap;
        }
    }
}