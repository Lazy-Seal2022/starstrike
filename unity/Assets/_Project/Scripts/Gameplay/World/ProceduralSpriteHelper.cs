using UnityEngine;

namespace StarStrike.Gameplay
{
    public static class ProceduralSpriteHelper
    {
        private static Sprite cachedScout;
        private static Sprite cachedFighter;
        private static Sprite cachedMiner;
        private static Sprite cachedDrone;
        private static Sprite cachedInterceptor;
        private static Sprite cachedJuggernaut;
        private static Sprite cachedBoss;
        private static Sprite cachedLaser;
        private static Sprite cachedEnemyLaser;
        private static Sprite cachedAsteroid;
        private static Sprite cachedGem;
        private static Sprite cachedStar;
        private static Sprite cachedRadar;
        private static Sprite cachedPowerUp;
        private static Sprite cachedMissile;
        private static Sprite cachedPellet;

        private static Sprite GetCachedOrGenerate(ref Sprite cache, System.Func<Sprite> generator, bool forceNew = false)
        {
            if (forceNew) return generator();
            if (cache == null)
            {
                cache = generator();
            }
            return cache;
        }

        public static Sprite GenerateShipSprite(string type)
        {
            if (type == "fighter") return GenerateShip(new Color(0.96f, 0.25f, 0.37f), 64, 2);
            if (type == "miner") return GenerateShip(new Color(0.96f, 0.62f, 0.04f), 64, 3);
            if (type == "drone") return GenerateShip(new Color(0.9f, 0.15f, 0.15f), 64, 4);
            if (type == "interceptor") return GenerateShip(new Color(0.45f, 0.35f, 0.98f), 64, 5);
            if (type == "juggernaut") return GenerateShip(new Color(0.95f, 0.45f, 0.15f), 64, 6);
            if (type == "boss") return GenerateShip(new Color(0.95f, 0.12f, 0.22f), 96, 7);
            return GenerateShip(new Color(0.22f, 0.74f, 0.97f), 64, 1);
        }

        public static Sprite GenerateLaserSprite(bool isEnemy = false)
        {
            if (isEnemy) return GenerateCapsule(new Color(1f, 0.25f, 0.25f), 16, 48);
            return GenerateCapsule(new Color(0.2f, 0.9f, 1f), 16, 48);
        }

        public static Sprite GenerateAsteroidSprite()
        {
            return GenerateAsteroid(new Color(0.6f, 0.65f, 0.72f), 64);
        }

        public static Sprite GenerateGemSprite()
        {
            return GenerateDiamond(new Color(0.2f, 0.95f, 0.45f), 32);
        }

        public static Sprite GenerateStarSprite()
        {
            return GenerateCircle(Color.white, 16);
        }

        public static Sprite GeneratePowerUpSprite()
        {
            return GenerateDiamond(new Color(1f, 0.85f, 0.2f), 32);
        }

        public static Sprite GenerateMissileSprite()
        {
            return GenerateCapsule(new Color(1f, 0.4f, 0.1f), 16, 40);
        }

        public static Sprite GeneratePelletSprite()
        {
            return GenerateCircle(new Color(1f, 0.8f, 0.2f), 16);
        }

        public static Sprite GetShipSprite(string type, bool forceProcedural = false)
        {
            if (type == "fighter")
                return GetCachedOrGenerate(ref cachedFighter, () => GenerateShipSprite("fighter"), forceProcedural);
            if (type == "miner")
                return GetCachedOrGenerate(ref cachedMiner, () => GenerateShipSprite("miner"), forceProcedural);
            if (type == "drone")
                return GetCachedOrGenerate(ref cachedDrone, () => GenerateShipSprite("drone"), forceProcedural);
            if (type == "interceptor")
                return GetCachedOrGenerate(ref cachedInterceptor, () => GenerateShipSprite("interceptor"), forceProcedural);
            if (type == "juggernaut")
                return GetCachedOrGenerate(ref cachedJuggernaut, () => GenerateShipSprite("juggernaut"), forceProcedural);
            if (type == "boss")
                return GetCachedOrGenerate(ref cachedBoss, () => GenerateShipSprite("boss"), forceProcedural);

            return GetCachedOrGenerate(ref cachedScout, () => GenerateShipSprite("scout"), forceProcedural);
        }

        public static Sprite GetLaserSprite(bool isEnemy = false, bool forceProcedural = false)
        {
            if (isEnemy)
                return GetCachedOrGenerate(ref cachedEnemyLaser, () => GenerateLaserSprite(true), forceProcedural);

            return GetCachedOrGenerate(ref cachedLaser, () => GenerateLaserSprite(false), forceProcedural);
        }

        public static Sprite GetAsteroidSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedAsteroid, () => GenerateAsteroidSprite(), forceProcedural);
        }

        public static Sprite GetGemSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedGem, () => GenerateGemSprite(), forceProcedural);
        }

        public static Sprite GetStarSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedStar, () => GenerateStarSprite(), forceProcedural);
        }

        public static Sprite GetPowerUpSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedPowerUp, () => GeneratePowerUpSprite(), forceProcedural);
        }

        public static Sprite GetMissileSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedMissile, () => GenerateMissileSprite(), forceProcedural);
        }

        public static Sprite GetPelletSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedPellet, () => GeneratePelletSprite(), forceProcedural);
        }

        private static Sprite GenerateCircle(Color color, int size)
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
                    if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist / r));
                        cols[y * size + x] = new Color(color.r, color.g, color.b, alpha * alpha);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
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
                    else if (style == 4) // Drone (Diamond saucer)
                    {
                        float nx = Mathf.Abs(p.x) / (r * 0.8f);
                        float ny = Mathf.Abs(p.y) / (r * 0.8f);
                        inside = (nx + ny) <= 1.0f;
                    }
                    else if (style == 5) // Interceptor (Viper Interceptor - sleek needle + swept wings)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool needle = (nx >= -0.7f && nx <= 0.95f && ny <= (0.95f - nx) * 0.35f);
                        bool sweptWings = (nx >= -0.5f && nx <= 0.25f && ny <= 0.85f && ny >= 0.3f && (nx - ny * 0.35f) >= -0.45f);
                        bool engines = (nx >= -0.85f && nx <= -0.3f && ny >= 0.2f && ny <= 0.45f);
                        inside = needle || sweptWings || engines;
                    }
                    else if (style == 6) // Juggernaut (Titan Juggernaut - armored tank chassis + prongs)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool mainBlock = (Mathf.Abs(nx) <= 0.75f && ny <= 0.75f);
                        bool frontProngs = (nx >= 0.2f && nx <= 0.95f && ny >= 0.25f && ny <= 0.7f);
                        bool centerRam = (nx >= 0.6f && nx <= 0.9f && ny <= 0.25f);
                        bool rearPlates = (nx <= -0.4f && nx >= -0.9f && ny <= 0.85f);
                        inside = mainBlock || frontProngs || centerRam || rearPlates;
                    }
                    else if (style == 7) // Boss (Dreadnought flagship)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool core = (nx >= -0.85f && nx <= 0.9f && ny <= (0.95f - nx) * 0.75f);
                        bool claws = (nx >= -0.2f && nx <= 0.85f && ny >= 0.5f && ny <= 0.95f);
                        bool rearEngines = (nx <= -0.4f && nx >= -0.95f && ny >= 0.3f && ny <= 0.85f);
                        inside = core || claws || rearEngines;
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

        private static Sprite cachedWhite;
        private static Sprite cachedCircle;
        private static Sprite cachedRoundedBox;

        public static Sprite GetWhiteSprite()
        {
            if (cachedWhite == null)
            {
                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] cols = new Color[16];
                for (int i = 0; i < 16; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
                cachedWhite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100);
            }
            return cachedWhite;
        }

        public static Sprite GetCircleSprite(int size = 64, bool forceProcedural = false)
        {
            if (forceProcedural) return GenerateCircle(Color.white, size);
            if (cachedCircle == null)
            {
                cachedCircle = GenerateCircle(Color.white, size);
            }
            return cachedCircle;
        }

        public static Sprite GetRadarSprite(int size = 128, bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedRadar, () => GenerateRadarInternal(size), forceProcedural);
        }

        private static Sprite GenerateRadarInternal(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;
            float innerRingRadius = radius * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        Color col = new Color(0.02f, 0.05f, 0.12f, 0.75f);

                        if (dist >= radius - 2.5f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.85f);
                        }
                        else if (Mathf.Abs(dist - innerRingRadius) <= 1.0f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.3f);
                        }
                        else if ((Mathf.Abs(x - center.x) <= 0.6f || Mathf.Abs(y - center.y) <= 0.6f) && dist > 4f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.25f);
                        }

                        float aa = Mathf.Clamp01((radius - dist) + 0.5f);
                        cols[y * size + x] = new Color(col.r, col.g, col.b, col.a * aa);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }

        public static Sprite GetRoundedBoxSprite(int size = 32, int cornerRadius = 6)
        {
            if (cachedRoundedBox == null)
            {
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                Color[] cols = new Color[size * size];

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int cx = x < cornerRadius ? cornerRadius : (x >= size - cornerRadius ? size - 1 - cornerRadius : x);
                        int cy = y < cornerRadius ? cornerRadius : (y >= size - cornerRadius ? size - 1 - cornerRadius : y);
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        float alpha = dist > cornerRadius ? 0f : 1f;
                        cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                }
                tex.SetPixels(cols);
                tex.Apply();
                cachedRoundedBox = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
            }
            return cachedRoundedBox;
        }
    }
}
