#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// ============================================================================
//  MENUS BUILDER (Editor) — FlowCity
//  ----------------------------------------------------------------------------
//  Crea las dos escenas de menú con un clic cada una:
//    FlowCity > Construir Menú Principal
//    FlowCity > Construir Selector de Vehículo
//
//  Diseño sobrio / institucional: fondo azul oscuro, barra superior, título,
//  botones grandes con onda profesional. Los botones quedan cableados al
//  script SeleccionExamen.
//
//  IMPORTANTE: agregar las 3 escenas a File > Build Settings en este orden:
//    MenuPrincipal, SeleccionVehiculo, FlowCity
// ============================================================================
public static class MenusBuilder
{
    // Paleta unificada (lee del tema central FlowTheme)
    static Color FONDO   = FlowTheme.BG0;     // fondo profundo
    static Color FONDO2  = FlowTheme.BG1;     // panel / superficie
    static Color ACENTO  = FlowTheme.PRIMARY; // azul acción
    static Color ACENTO2 = FlowTheme.SUCCESS; // verde acción (CTA)
    static Color MOTO    = FlowTheme.MOTO;    // naranja moto
    static Color TXT     = FlowTheme.TXT_HI;  // texto claro
    static Color TXTGRIS = FlowTheme.TXT_MD;

    // ---------------- MENÚ PRINCIPAL ----------------
    [MenuItem("FlowCity/Construir Menú Principal")]
    public static void ConstruirMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = BaseEscena(out var ctrl);

        // Fondo con gradiente (más claro arriba, profundo abajo) para dar profundidad
        var fondo = FondoGradiente(canvas);
        // Banda superior (institucional)
        var banda = Panel(canvas, "BandaSuperior", ACENTO);
        Anchor(banda, new Vector2(0, 0.92f), new Vector2(1, 1f));
        var bandaTxt = Texto(banda.transform, "Inst", "INSTITUTO DE FORMACIÓN VIAL", 22, TextAnchor.MiddleCenter, TXT);
        Stretch(bandaTxt.gameObject);

        // Título grande del juego
        var titulo = Texto(canvas, "Titulo", "FLOWCITY", 96, TextAnchor.MiddleCenter, TXT);
        AnchorPos(titulo.gameObject, new Vector2(0.5f, 0.72f), new Vector2(0, 0), new Vector2(1200, 130));
        var subt = Texto(canvas, "Subtitulo", "Simulador del Examen de Manejo", 34, TextAnchor.MiddleCenter, ACENTO);
        AnchorPos(subt.gameObject, new Vector2(0.5f, 0.63f), new Vector2(0, 0), new Vector2(1200, 60));

        // Botón JUGAR (grande, verde)
        BotonGrande(canvas, ctrl, "BotonJugar", "COMENZAR", new Vector2(0.5f, 0.42f), ACENTO2, "IrASeleccion");
        // Botón SALIR
        BotonGrande(canvas, ctrl, "BotonSalir", "SALIR", new Vector2(0.5f, 0.27f), FONDO2, "Salir");

        // Pie
        var pie = Texto(canvas, "Pie", "v1.0  ·  Práctica de reacción ante señales de tránsito", 20, TextAnchor.MiddleCenter, TXTGRIS);
        AnchorPos(pie.gameObject, new Vector2(0.5f, 0.06f), new Vector2(0, 0), new Vector2(1400, 40));

        Guardar(scene, "MenuPrincipal");
    }

    // ---------------- SELECTOR DE VEHÍCULO ----------------
    [MenuItem("FlowCity/Construir Selector de Vehículo")]
    public static void ConstruirSelector()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = BaseEscena(out var ctrl);

        var fondo = FondoGradiente(canvas);
        var banda = Panel(canvas, "BandaSuperior", ACENTO);
        Anchor(banda, new Vector2(0, 0.92f), new Vector2(1, 1f));
        Stretch(Texto(banda.transform, "Inst", "INSTITUTO DE FORMACIÓN VIAL", 22, TextAnchor.MiddleCenter, TXT).gameObject);

        var titulo = Texto(canvas, "Titulo", "ELEGÍ TU EXAMEN", 64, TextAnchor.MiddleCenter, TXT);
        AnchorPos(titulo.gameObject, new Vector2(0.5f, 0.78f), new Vector2(0, 0), new Vector2(1200, 100));

        // Dos tarjetas grandes: AUTO y MOTO
        TarjetaVehiculo(canvas, ctrl, "TarjetaAuto", "AUTO", "Examen para licencia de automóvil",
            new Vector2(0.30f, 0.45f), ACENTO, "ElegirAuto", dibujarAuto: true);
        TarjetaVehiculo(canvas, ctrl, "TarjetaMoto", "MOTO", "Examen para licencia de motocicleta",
            new Vector2(0.70f, 0.45f), MOTO, "ElegirMoto", dibujarAuto: false);

        // Botón volver
        BotonChico(canvas, ctrl, "BotonVolver", "← Volver", new Vector2(0.12f, 0.92f), FONDO2, "VolverAlMenu");

        Guardar(scene, "SeleccionVehiculo");
    }

    // ============ piezas ============

    static Transform BaseEscena(out SeleccionExamen ctrl)
    {
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera"; cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = FONDO; cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var ctrlGO = new GameObject("MenuController");
        ctrl = ctrlGO.AddComponent<SeleccionExamen>();
        return canvasGO.transform;
    }

    static void BotonGrande(Transform p, SeleccionExamen ctrl, string nombre, string label, Vector2 anchor, Color color, string metodo)
    {
        var go = CrearBoton(p, ctrl, nombre, label, anchor, new Vector2(560, 110), 44, color, metodo);
    }
    static void BotonChico(Transform p, SeleccionExamen ctrl, string nombre, string label, Vector2 anchor, Color color, string metodo)
    {
        CrearBoton(p, ctrl, nombre, label, anchor, new Vector2(220, 64), 26, color, metodo);
    }

    // Tarjeta de vehículo: panel grande clickeable con dibujo + título + descripción
    static void TarjetaVehiculo(Transform p, SeleccionExamen ctrl, string nombre, string titulo, string desc,
                                Vector2 anchor, Color color, string metodo, bool dibujarAuto)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(440, 460);
        var img = go.AddComponent<Image>(); img.color = FONDO2;
        Redondear(go, FlowTheme.R_TARJETA);               // tarjeta redondeada
        SombraSuave(go, new Vector2(0, -8), 0.40f);       // flota sobre el fondo
        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = FlowTheme.BG2; cols.pressedColor = FlowTheme.Mul(FlowTheme.BG1, 0.8f);
        cols.fadeDuration = 0.08f;
        btn.colors = cols;
        CablearBoton(btn, ctrl, metodo);

        // barra de acento del vehículo arriba (redondeada, integrada a la tarjeta)
        var top = Panel(go.transform, "Top", color);
        Redondear(top, 8);
        AnchorPos(top, new Vector2(0.5f, 1f), new Vector2(0, -26), new Vector2(360, 10));

        // dibujo del vehículo con figuras (auto o moto, simple)
        var dibujo = new GameObject("Dibujo");
        dibujo.transform.SetParent(go.transform, false);
        var drt = dibujo.AddComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.5f, 0.62f); drt.anchorMax = new Vector2(0.5f, 0.62f);
        drt.pivot = new Vector2(0.5f, 0.5f); drt.anchoredPosition = Vector2.zero; drt.sizeDelta = new Vector2(260, 200);
        if (dibujarAuto) DibujoAuto(dibujo.transform, color); else DibujoMoto(dibujo.transform, color);

        // título y descripción
        var t = Texto(go.transform, "Titulo", titulo, 48, TextAnchor.MiddleCenter, TXT);
        AnchorPos(t.gameObject, new Vector2(0.5f, 0.28f), new Vector2(0, 0), new Vector2(400, 70));
        var d = Texto(go.transform, "Desc", desc, 22, TextAnchor.UpperCenter, TXTGRIS);
        AnchorPos(d.gameObject, new Vector2(0.5f, 0.14f), new Vector2(0, 0), new Vector2(380, 80));
    }

    // Auto de atrás simple (con figuras), centrado en su contenedor
    static void DibujoAuto(Transform p, Color color)
    {
        var cuerpo = Panel(p, "Cuerpo", color);
        AnchorPos(cuerpo, new Vector2(0.5f, 0.4f), new Vector2(0, 0), new Vector2(200, 110));
        var techo = Panel(p, "Techo", Hex("#2C3E50"));
        AnchorPos(techo, new Vector2(0.5f, 0.62f), new Vector2(0, 0), new Vector2(140, 70));
        var luzI = Panel(p, "LuzI", Hex("#F1948A")); AnchorPos(luzI, new Vector2(0.5f, 0.42f), new Vector2(-75, 0), new Vector2(34, 26));
        var luzD = Panel(p, "LuzD", Hex("#F1948A")); AnchorPos(luzD, new Vector2(0.5f, 0.42f), new Vector2(75, 0), new Vector2(34, 26));
    }

    // Moto de atrás simple (con figuras)
    static void DibujoMoto(Transform p, Color color)
    {
        var rueda = Panel(p, "Rueda", Hex("#1B2631"));
        AnchorPos(rueda, new Vector2(0.5f, 0.28f), new Vector2(0, 0), new Vector2(70, 120));
        var cuerpo = Panel(p, "Cuerpo", color);
        AnchorPos(cuerpo, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(110, 90));
        var manubrioI = Panel(p, "ManI", Hex("#2C3E50")); AnchorPos(manubrioI, new Vector2(0.5f, 0.72f), new Vector2(-70, 0), new Vector2(60, 18));
        var manubrioD = Panel(p, "ManD", Hex("#2C3E50")); AnchorPos(manubrioD, new Vector2(0.5f, 0.72f), new Vector2(70, 0), new Vector2(60, 18));
        var luz = Panel(p, "Luz", Hex("#F1948A")); AnchorPos(luz, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(40, 30));
    }

    // ============ helpers ============

    static Button CrearBoton(Transform parent, SeleccionExamen ctrl, string nombre, string label, Vector2 anchor, Vector2 size, int fontSize, Color color, string metodo)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        Redondear(go, FlowTheme.R_BOTON);                 // esquinas redondeadas
        SombraSuave(go, new Vector2(0, -5), 0.30f);       // profundidad
        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = FlowTheme.Mul(color, 1.15f);
        cols.pressedColor     = FlowTheme.Mul(color, 0.85f);
        cols.fadeDuration = 0.08f;
        btn.colors = cols;
        CablearBoton(btn, ctrl, metodo);
        var txt = Texto(go.transform, "Texto", label, fontSize, TextAnchor.MiddleCenter, TXT);
        Stretch(txt.gameObject);
        return btn;
    }

    static void CablearBoton(Button btn, SeleccionExamen ctrl, string metodo)
    {
        var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), ctrl, metodo);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    static GameObject Panel(Transform parent, string nombre, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static TextMeshProUGUI Texto(Transform parent, string nombre, string contenido, int tam, TextAnchor anchor, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = tam; t.fontStyle = FontStyles.Bold;
        t.alignment = Alinear(anchor); t.color = color;
        return t;
    }

    // ----- helpers de estilo moderno -----

    // Fondo a pantalla completa con gradiente vertical (profundo abajo, claro arriba).
    static GameObject FondoGradiente(Transform canvas)
    {
        var go = Panel(canvas, "Fondo", Color.white);
        Stretch(go);
        var g = go.AddComponent<GradientBackground>();
        g.abajo = FONDO;
        g.arriba = FlowTheme.Mul(FONDO, 1.7f);   // un poco más claro arriba
        return go;
    }

    // Redondea las esquinas de un GameObject que ya tiene Image.
    static void Redondear(GameObject go, int radio)
    {
        var r = go.AddComponent<RoundedImage>();
        r.radius = radio;
    }

    // Sombra suave (sensación de elevación).
    static void SombraSuave(GameObject go, Vector2 offset, float alpha)
    {
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, alpha);
        sh.effectDistance = offset;
    }

    // TextAnchor (legacy) -> TextAlignmentOptions (TMP)
    static TextAlignmentOptions Alinear(TextAnchor a)
    {
        switch (a)
        {
            case TextAnchor.UpperLeft:    return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:  return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:   return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:   return TextAlignmentOptions.Left;
            case TextAnchor.MiddleRight:  return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:    return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:  return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:   return TextAlignmentOptions.BottomRight;
            default:                      return TextAlignmentOptions.Center;
        }
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
    static void Anchor(GameObject go, Vector2 min, Vector2 max)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
    static void AnchorPos(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }

    static void Guardar(UnityEngine.SceneManagement.Scene scene, string nombre)
    {
        if (!System.IO.Directory.Exists("Assets/Scenes")) System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + nombre + ".unity");
        EditorUtility.DisplayDialog("FlowCity", "Escena '" + nombre + "' construida.\n\nRecordá agregarla a File > Build Settings.", "OK");
        Debug.Log("[FlowCity] Escena " + nombre + " creada.");
    }
}
#endif
