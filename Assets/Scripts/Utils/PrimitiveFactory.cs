using UnityEngine;

namespace CrashMissileCrash.Utils
{
    public enum PlaceholderShape { Capsule, Cube, Sphere, Cylinder }

    /// <summary>
    /// Builds simple, colorful "toy-like" primitive placeholder visuals at runtime so the game
    /// is fully playable before final art is authored. Art can later replace any visualKey by
    /// registering a real prefab with VisualRegistry - no gameplay code needs to change.
    /// </summary>
    public static class PrimitiveFactory
    {
        private static Shader _urpLitShader;

        private static Shader LitShader
        {
            get
            {
                if (_urpLitShader == null)
                {
                    _urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (_urpLitShader == null) _urpLitShader = Shader.Find("Standard");
                }
                return _urpLitShader;
            }
        }

        public static GameObject CreatePlaceholder(string name, PlaceholderShape shape, Color color, Vector3 localScale)
        {
            PrimitiveType primitiveType = shape switch
            {
                PlaceholderShape.Cube => PrimitiveType.Cube,
                PlaceholderShape.Sphere => PrimitiveType.Sphere,
                PlaceholderShape.Cylinder => PrimitiveType.Cylinder,
                _ => PrimitiveType.Capsule
            };

            var go = GameObject.CreatePrimitive(primitiveType);
            go.name = name;
            go.transform.localScale = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(LitShader) { color = color };
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            return go;
        }

        public static Color ColorForRarity(int rarity)
        {
            return rarity switch
            {
                0 => new Color(0.75f, 0.75f, 0.78f), // Common - silver
                1 => new Color(0.30f, 0.65f, 1.00f),  // Rare - blue
                2 => new Color(0.68f, 0.36f, 0.95f),  // Epic - purple
                3 => new Color(1.00f, 0.72f, 0.20f),  // Legendary - gold
                _ => new Color(1.00f, 0.30f, 0.55f)   // Mythic - pink
            };
        }
    }
}
