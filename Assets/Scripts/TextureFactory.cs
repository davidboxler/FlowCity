using System.Collections.Generic;
using UnityEngine;

// ============================================================================
//  TEXTURE FACTORY — genera sprites por código (sin imágenes externas)
//  ----------------------------------------------------------------------------
//  Crea formas que un panel rectangular no puede: círculos, triángulos y la
//  calle en perspectiva (trapecio). Cachea los resultados para no regenerar.
// ============================================================================
public static class TextureFactory
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    // --- Círculo lleno ---
    public static Sprite Circulo(int size = 128)
    {
        string key = "circ_" + size;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(size, size);
        float r = size * 0.5f - 1f;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Triángulo apuntando hacia ARRIBA ---
    public static Sprite Triangulo(int size = 128)
    {
        string key = "tri_" + size;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(size, size);
        for (int y = 0; y < size; y++)
        {
            // En la base (y=0) ancho completo; en la punta (y=size) ancho 0.
            float t = (float)y / size;
            float half = (size * 0.5f) * (1f - t);
            float cx = size * 0.5f;
            for (int x = 0; x < size; x++)
            {
                bool dentro = Mathf.Abs(x + 0.5f - cx) <= half;
                tex.SetPixel(x, y, dentro ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Calle en perspectiva (trapecio): ancha abajo, angosta arriba ---
    public static Sprite Calle(int w = 512, int h = 512)
    {
        string key = "calle_" + w + "x" + h;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(w, h);
        Color asfalto = Hex("#4A5158");
        Color borde = Hex("#D5D8DC");
        Color linea = Hex("#F4D03F");
        float cx = w * 0.5f;

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;                 // 0 abajo, 1 arriba (punto de fuga)
            // El punto de fuga llega casi a un punto arriba => perspectiva real apoyada en el suelo.
            float half = Mathf.Lerp(w * 0.49f, w * 0.012f, t);
            // el asfalto se aclara un poco hacia el fondo (efecto distancia/neblina)
            Color asfaltoY = Color.Lerp(asfalto, Hex("#6B7178"), t * 0.6f);
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - cx);
                if (dx <= half)
                {
                    // borde blanco a los costados (más fino hacia el fondo)
                    if (dx >= half - Mathf.Lerp(12f, 1.5f, t))
                        tex.SetPixel(x, y, borde);
                    // línea central amarilla discontinua UNIFORME: mismo ancho, mismo
                    // largo de raya y mismo hueco en toda la calle (period fijo = recto).
                    else if (dx <= Mathf.Lerp(8f, 4f, t) && (y % 72 < 40))
                        tex.SetPixel(x, y, linea);
                    else
                        tex.SetPixel(x, y, asfaltoY);
                }
                else tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Octágono (PARE) ---
    public static Sprite Octagono(int size = 128)
    {
        string key = "octa_" + size;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(size, size);
        float half = size * 0.5f - 1f;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - c.x);
                float dy = Mathf.Abs(y + 0.5f - c.y);
                bool dentro = dx <= half && dy <= half && (dx + dy) <= half * 1.30f;
                tex.SetPixel(x, y, dentro ? Color.white : Color.clear);
            }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Anillo / aro (para ruedas de bici, rotondas) ---
    public static Sprite Anillo(int size = 128, float grosorRel = 0.16f)
    {
        string key = "anillo_" + size + "_" + grosorRel;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(size, size);
        float r = size * 0.5f - 1f;
        float grosor = size * grosorRel;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                tex.SetPixel(x, y, (d <= r && d >= r - grosor) ? Color.white : Color.clear);
            }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Media luna / domo (lomo de burro): mitad superior de un disco, apoyada abajo ---
    public static Sprite Semicirculo(int size = 128)
    {
        string key = "semi_" + size;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(size, size);
        float r = size * 0.5f - 1f;
        Vector2 c = new Vector2(size * 0.5f, 0f); // centro en el borde inferior
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }
        tex.Apply();
        return Guardar(key, tex);
    }

    // --- Senda peatonal (cebra) EN PERSPECTIVA: franjas verticales que convergen
    //     hacia el punto de fuga. Anchas/separadas abajo (cerca), angostas/juntas arriba. ---
    public static Sprite SendaPeatonal(int w = 512, int h = 256, int franjas = 6)
    {
        string key = "senda_" + w + "x" + h + "_" + franjas;
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = NuevaTextura(w, h);
        float cx = w * 0.5f;
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;                       // 0 abajo (cerca), 1 arriba (lejos)
            float half = Mathf.Lerp(w * 0.47f, w * 0.20f, t); // el ancho de la cebra se cierra al fondo
            for (int x = 0; x < w; x++)
            {
                float dx = x + 0.5f - cx;
                float u = dx / half;                      // -1..1 dentro de la senda
                if (u < -1f || u > 1f) { tex.SetPixel(x, y, Color.clear); continue; }
                // u mapeado a celdas: cada celda = franja blanca + hueco
                float p = (u + 1f) * 0.5f * franjas;       // 0..franjas
                bool blanco = (p - Mathf.Floor(p)) < 0.55f;
                tex.SetPixel(x, y, blanco ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Guardar(key, tex);
    }

    // ---------- helpers ----------
    private static Texture2D NuevaTextura(int w, int h)
    {
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    private static Sprite Guardar(string key, Texture2D tex)
    {
        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        cache[key] = s;
        return s;
    }

    public static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
