using UnityEngine;

// ============================================================================
//  FLOW THEME — design tokens centralizados (paleta + radios + tiempos)
//  ----------------------------------------------------------------------------
//  Un solo lugar con TODOS los colores y medidas, para que las 3 escenas
//  (menú, selector, juego) se sientan un único producto. Los builders y los
//  scripts de runtime leen de acá en vez de hardcodear HEX sueltos.
//
//  Estilo: "sobrio-tech" — fondo profundo azul-negro, superficies elevadas,
//  un acento azul vivo para la acción y semánticos para éxito/error/atención.
// ============================================================================
public static class FlowTheme
{
    // ---- Superficies ----
    public static readonly Color BG0      = Hex("#0F1623"); // fondo profundo
    public static readonly Color BG1      = Hex("#1A2332"); // panel / tarjeta
    public static readonly Color BG2      = Hex("#243044"); // superficie elevada / hover

    // ---- Acento y semánticos ----
    public static readonly Color PRIMARY  = Hex("#3B82F6"); // azul acción
    public static readonly Color PRIMARYH = Hex("#60A5FA"); // azul hover
    public static readonly Color SUCCESS  = Hex("#22C55E"); // acierto
    public static readonly Color DANGER   = Hex("#EF4444"); // error
    public static readonly Color WARN     = Hex("#F59E0B"); // timer / atención

    // ---- Vehículos ----
    public static readonly Color AUTO     = Hex("#3B82F6");
    public static readonly Color MOTO     = Hex("#F97316");

    // ---- Texto ----
    public static readonly Color TXT_HI   = Hex("#F8FAFC"); // texto principal
    public static readonly Color TXT_MD   = Hex("#94A3B8"); // texto secundario

    // ---- Ambiente del juego (cielo en gradiente, piso) ----
    public static readonly Color SKY_TOP  = Hex("#2E6FA8");
    public static readonly Color SKY_LOW  = Hex("#7FC0E8");
    public static readonly Color GROUND   = Hex("#2B3340");
    public static readonly Color ASFALTO  = Hex("#3A4150");

    // ---- Radios (px en resolución de referencia 1920x1080) ----
    public const int R_BOTON   = 18;
    public const int R_TARJETA = 26;
    public const int R_POPUP   = 14;
    public const int R_BANDA   = 0;   // las bandas full-width quedan rectas

    public static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }

    // Aclara/oscurece un color manteniendo alpha (para estados hover/pressed).
    public static Color Mul(Color c, float f)
        => new Color(Mathf.Clamp01(c.r * f), Mathf.Clamp01(c.g * f), Mathf.Clamp01(c.b * f), c.a);
}
