using UnityEngine;

namespace StarStrike.Gameplay
{
    public static class ProceduralSpriteHelper
    {
        private static Sprite cachedScout;
        private static Sprite cachedFighter;
        private static Sprite cachedMiner;
        private static Sprite cachedDrone;
        private static Sprite cachedLaser;
        private static Sprite cachedEnemyLaser;
        private static Sprite cachedAsteroid;
        private static Sprite cachedGem;

        public static Sprite GetShipSprite(string type)
        {
            if (type == "fighter")
            {
                if (cachedFighter == null) cachedFighter = GenerateShip(new Color(0.96f, 0.25f, 0.37f), 64, 2);
                return cachedFighter;
            }
            if (type == "miner")
            {
                if (cachedMiner == null) cachedMiner = GenerateShip(new Color(0.96f, 0.62f, 0.04f), 64, 3);
                return cachedMiner;
            }
            if (type == "drone")
            {
                if (cachedDrone == null) cachedDrone = GenerateShip(new Color(0.9f, 0.15f, 0.15f), 48, 4);
                return cachedDrone;
            }
            if (cachedScout == null) cachedScout = GenerateShip(new Color(0.22f, 0.74f, 0.97f), 64, 1);
            return cachedScout;
        }

        public static Sprite GetLaserSprite(bool isEnemy = false)
        {
            if (isEnemy)
            {
                if (cachedEnemyLaser == null) cachedEnemyLaser = GenerateCapsule(new Color(1f, 0.25f, 0.25f), 16, 48);
                return cachedEnemyLaser;
            }
            if (cachedLaser == null) cachedLaser = GenerateCapsule(new Color(0.2f, 0.9f, 1f), 16, 48);
            return cachedLaser;
        }

        public static Sprite GetAsteroidSprite()
        {
            if (cachedAsteroid == null) cachedAsteroid = GenerateAsteroid(new Color(0.6f, 0.65f, 0.72f), 64);
            return cachedAsteroid;
        }

        public static Sprite GetGemSprite()
        {
            if (cachedGem == null) cachedGem = GenerateDiamond(new Color(0.2f, 0.95f, 0.45f), 32);
            return cachedGem;
        }

        private static Sprite GenerateShip(Color color, int size, int style)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center.x, y - center.y);
                    float dist = p.magnitude;
                    bool inside = false;

                    if (style == 1) // Scout (Delta Arrow)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = nx >= -0.5f && nx <= 0.85f && ny <= (0.85f - nx) * 0.65f;
                    }
                    else if (style == 2) // Fighter (Twin Wings)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = (nx >= -0.6f && nx <= 0.9f && ny <= (0.9f - nx) * 0.7f) ||
                                 (nx >= -0.8f && nx <= 0.2f && ny >= 0.4f && ny <= 0.85f);
                    }
                    else if (style == 3) // Miner (Heavy Cruiser)
                    {
                        float nx = Mathf.Abs(p.x) / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = (nx <= 0.7f && ny <= 0.7f) || (p.x >= 0 && p.x <= r * 0.95f && ny <= 0.45f);
                    }
                    else // Drone (Diamond saucer)
                    {
                        float nx = Mathf.Abs(p.x) / (r * 0.8f);
                        float ny = Mathf.Abs(p.y) / (r * 0.8f);
                        inside = (nx + ny) <= 1.0f;
                    }

                    if (inside)
                    {
                        float edge = 1f - Mathf.Clamp01(dist / r);
                        cols[y * size + x] = Color.Lerp(color, Color.white, edge * 0.5f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateCapsule(Color color, int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x - w * 0.5f) / (w * 0.5f);
                    float ny = (y - h * 0.5f) / (h * 0.5f);
                    float d = Mathf.Sqrt(nx * nx + ny * ny * 0.25f);
                    if (d < 1f)
                    {
                        float glow = Mathf.Pow(1f - d, 1.5f);
                        cols[y * w + x] = Color.Lerp(color, Color.white, glow);
                    }
                    else
                    {
                        cols[y * w + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateAsteroid(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float baseR = size * 0.38f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center.x, y - center.y);
                    float angle = Mathf.Atan2(p.y, p.x);
                    float r = baseR * (1f + 0.18f * Mathf.Sin(angle * 5f) + 0.12f * Mathf.Cos(angle * 3f + 1f));

                    if (p.magnitude <= r)
                    {
                        float shade = 0.7f + 0.3f * Mathf.Sin(p.x * 0.2f + p.y * 0.15f);
                        cols[y * size + x] = new Color(color.r * shade, color.g * shade, color.b * shade, 1f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateDiamond(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = size * 0.4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - center.x) / r;
                    float ny = Mathf.Abs(y - center.y) / r;
                    if (nx + ny <= 1f)
                    {
                        float shine = 1f - (nx + ny);
                        cols[y * size + x] = Color.Lerp(color, Color.white, shine * 0.7f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
