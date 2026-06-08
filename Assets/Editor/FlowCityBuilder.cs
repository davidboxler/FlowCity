#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// ============================================================================
//  FLOWCITY BUILDER (Editor)
//  ----------------------------------------------------------------------------
//  Construye TODA la escena del juego con un clic, conectando referencias solo.
//  Menú:  FlowCity > Construir Escena
//
//  Genera: cámara, fondo de "ruta" (primera persona simple), HUD de velocidad,
//  panel de preguntas (señal + enunciado + 2 botones + barra de tiempo),
//  panel final, y el GameManager con todo cableado.
// ============================================================================
public static class FlowCityBuilder
{
    [MenuItem("FlowCity/Construir Escena")]
    public static void Construir()
    {
        // Escena nueva vacía.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Cámara ----------
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexC("#5DADE2"); // cielo
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        // ---------- EventSystem (necesario para botones) ----------
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ---------- Canvas raíz ----------
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---------- Fondo: "ruta" en primera persona (simple) ----------
        // Pasto
        var pasto = Panel(canvasGO.transform, "Pasto", HexC("#7DCEA0"));
        StretchRT(pasto.GetComponent<RectTransform>());
        // Ruta (trapecio simulado con un rectángulo gris central)
        var ruta = Panel(canvasGO.transform, "Ruta", HexC("#566573"));
        var rutaRT = ruta.GetComponent<RectTransform>();
        rutaRT.anchorMin = new Vector2(0.32f, 0f);
        rutaRT.anchorMax = new Vector2(0.68f, 1f);
        rutaRT.offsetMin = Vector2.zero; rutaRT.offsetMax = Vector2.zero;
        // Línea central punteada (una franja amarilla)
        var linea = Panel(ruta.transform, "LineaCentral", HexC("#F4D03F"));
        var lineaRT = linea.GetComponent<RectTransform>();
        lineaRT.anchorMin = new Vector2(0.48f, 0f);
        lineaRT.anchorMax = new Vector2(0.52f, 1f);
        lineaRT.offsetMin = Vector2.zero; lineaRT.offsetMax = Vector2.zero;
        // Capó del auto (primera persona): rectángulo oscuro abajo
        var capo = Panel(canvasGO.transform, "CapoAuto", HexC("#212F3D"));
        var capoRT = capo.GetComponent<RectTransform>();
        capoRT.anchorMin = new Vector2(0f, 0f);
        capoRT.anchorMax = new Vector2(1f, 0.18f);
        capoRT.offsetMin = Vector2.zero; capoRT.offsetMax = Vector2.zero;

        // ---------- PANEL JUEGO ----------
        var panelJuego = new GameObject("PanelJuego");
        panelJuego.transform.SetParent(canvasGO.transform, false);
        var pjRT = panelJuego.AddComponent<RectTransform>();
        StretchRT(pjRT);

        // Señal (arriba centro): fondo de color + símbolo grande
        var signGO = new GameObject("Senal");
        signGO.transform.SetParent(panelJuego.transform, false);
        var signRT = signGO.AddComponent<RectTransform>();
        signRT.anchorMin = new Vector2(0.5f, 1f);
        signRT.anchorMax = new Vector2(0.5f, 1f);
        signRT.pivot = new Vector2(0.5f, 1f);
        signRT.anchoredPosition = new Vector2(0, -60);
        signRT.sizeDelta = new Vector2(260, 260);
        var signFondo = signGO.AddComponent<Image>();
        signFondo.color = HexC("#34495E");
        // símbolo
        var simboloGO = new GameObject("Simbolo");
        simboloGO.transform.SetParent(signGO.transform, false);
        var simboloTxt = simboloGO.AddComponent<Text>();
        simboloTxt.text = "?";
        simboloTxt.font = Font();
        simboloTxt.fontStyle = FontStyle.Bold;
        simboloTxt.fontSize = 44;
        simboloTxt.alignment = TextAnchor.MiddleCenter;
        simboloTxt.color = Color.white;
        simboloTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        simboloTxt.verticalOverflow = VerticalWrapMode.Overflow;
        StretchRT(simboloGO.GetComponent<RectTransform>());
        var signDrawer = signGO.AddComponent<SignDrawer>();
        SetPriv(signDrawer, "fondo", signFondo);
        SetPriv(signDrawer, "simbolo", simboloTxt);

        // Progreso "Pregunta X / 15"
        var progreso = Texto(panelJuego.transform, "Progreso", "Pregunta 1 / 15", 34, TextAnchor.UpperCenter);
        var progRT = progreso.GetComponent<RectTransform>();
        progRT.anchorMin = new Vector2(0.5f, 1f); progRT.anchorMax = new Vector2(0.5f, 1f);
        progRT.pivot = new Vector2(0.5f, 1f);
        progRT.anchoredPosition = new Vector2(0, -10);
        progRT.sizeDelta = new Vector2(500, 50);

        // Enunciado de la pregunta (centro)
        var enunciado = Texto(panelJuego.transform, "Enunciado", "¿Pregunta?", 52, TextAnchor.MiddleCenter);
        var enunRT = enunciado.GetComponent<RectTransform>();
        enunRT.anchorMin = new Vector2(0.5f, 0.5f); enunRT.anchorMax = new Vector2(0.5f, 0.5f);
        enunRT.pivot = new Vector2(0.5f, 0.5f);
        enunRT.anchoredPosition = new Vector2(0, 60);
        enunRT.sizeDelta = new Vector2(1000, 200);
        enunciado.color = Color.white;
        AddOutline(enunciado.gameObject);

        // Barra de tiempo (debajo del enunciado)
        var barraBG = Panel(panelJuego.transform, "BarraTiempoBG", HexC("#1C2833"));
        var barraBGRT = barraBG.GetComponent<RectTransform>();
        barraBGRT.anchorMin = new Vector2(0.5f, 0.5f); barraBGRT.anchorMax = new Vector2(0.5f, 0.5f);
        barraBGRT.pivot = new Vector2(0.5f, 0.5f);
        barraBGRT.anchoredPosition = new Vector2(0, -40);
        barraBGRT.sizeDelta = new Vector2(700, 32);
        var barraFill = Panel(barraBG.transform, "BarraTiempoFill", HexC("#F1C40F"));
        var barraFillImg = barraFill.GetComponent<Image>();
        barraFillImg.type = Image.Type.Filled;
        barraFillImg.fillMethod = Image.FillMethod.Horizontal;
        barraFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        barraFillImg.fillAmount = 1f;
        StretchRT(barraFill.GetComponent<RectTransform>());

        // Botón A (izquierda)
        Button btnA; Text txtA;
        CrearBoton(panelJuego.transform, "BotonA", "Opción A", new Vector2(-260, -160), HexC("#2980B9"), out btnA, out txtA);
        // Botón B (derecha)
        Button btnB; Text txtB;
        CrearBoton(panelJuego.transform, "BotonB", "Opción B", new Vector2(260, -160), HexC("#16A085"), out btnB, out txtB);

        // HUD de velocidad (abajo derecha, sobre el capó)
        var velTxt = Texto(panelJuego.transform, "TextoVelocidad", "40 km/h", 60, TextAnchor.MiddleRight);
        var velRT = velTxt.GetComponent<RectTransform>();
        velRT.anchorMin = new Vector2(1f, 0f); velRT.anchorMax = new Vector2(1f, 0f);
        velRT.pivot = new Vector2(1f, 0f);
        velRT.anchoredPosition = new Vector2(-40, 40);
        velRT.sizeDelta = new Vector2(420, 90);
        velTxt.color = Color.white;
        AddOutline(velTxt.gameObject);
        // barra de velocidad (vertical a la derecha)
        var velBG = Panel(panelJuego.transform, "VelocidadBG", HexC("#1C2833"));
        var velBGRT = velBG.GetComponent<RectTransform>();
        velBGRT.anchorMin = new Vector2(1f, 0f); velBGRT.anchorMax = new Vector2(1f, 0f);
        velBGRT.pivot = new Vector2(1f, 0f);
        velBGRT.anchoredPosition = new Vector2(-40, 140);
        velBGRT.sizeDelta = new Vector2(40, 300);
        var velFill = Panel(velBG.transform, "VelocidadFill", HexC("#E74C3C"));
        var velFillImg = velFill.GetComponent<Image>();
        velFillImg.type = Image.Type.Filled;
        velFillImg.fillMethod = Image.FillMethod.Vertical;
        velFillImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        velFillImg.fillAmount = 0.33f;
        StretchRT(velFill.GetComponent<RectTransform>());

        // Flash de feedback (cubre toda la pantalla, transparente)
        var flash = Panel(panelJuego.transform, "Flash", new Color(0,0,0,0));
        StretchRT(flash.GetComponent<RectTransform>());
        flash.GetComponent<Image>().raycastTarget = false;

        // ---------- PANEL FINAL ----------
        var panelFinal = new GameObject("PanelFinal");
        panelFinal.transform.SetParent(canvasGO.transform, false);
        var pfRT = panelFinal.AddComponent<RectTransform>();
        StretchRT(pfRT);
        var pfBG = panelFinal.AddComponent<Image>();
        pfBG.color = new Color(0.1f, 0.12f, 0.16f, 0.92f);

        var resultado = Texto(panelFinal.transform, "Resultado", "Resultado", 50, TextAnchor.MiddleCenter);
        var resRT = resultado.GetComponent<RectTransform>();
        resRT.anchorMin = new Vector2(0.5f, 0.5f); resRT.anchorMax = new Vector2(0.5f, 0.5f);
        resRT.pivot = new Vector2(0.5f, 0.5f);
        resRT.anchoredPosition = new Vector2(0, 80);
        resRT.sizeDelta = new Vector2(1100, 400);
        resultado.color = Color.white;

        Button btnReiniciar; Text txtReiniciar;
        CrearBoton(panelFinal.transform, "BotonReiniciar", "Volver a empezar", new Vector2(0, -200), HexC("#2980B9"), out btnReiniciar, out txtReiniciar);

        // ---------- GAME MANAGER ----------
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        SetPriv(gm, "textoPregunta", enunciado);
        SetPriv(gm, "textoOpcionA", txtA);
        SetPriv(gm, "textoOpcionB", txtB);
        SetPriv(gm, "botonA", btnA);
        SetPriv(gm, "botonB", btnB);
        SetPriv(gm, "barraTiempo", barraFillImg);
        SetPriv(gm, "textoProgreso", progreso);
        SetPriv(gm, "signDrawer", signDrawer);
        SetPriv(gm, "textoVelocidad", velTxt);
        SetPriv(gm, "velocidadFill", velFillImg);
        SetPriv(gm, "panelJuego", panelJuego);
        SetPriv(gm, "panelFinal", panelFinal);
        SetPriv(gm, "textoResultado", resultado);
        SetPriv(gm, "botonReiniciar", btnReiniciar);
        SetPriv(gm, "flashColor", flash.GetComponent<Image>());

        panelFinal.SetActive(false);

        // Guardar la escena.
        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/FlowCity.unity");
        EditorUtility.DisplayDialog("FlowCity",
            "¡Escena construida y guardada!\n\nAhora apretá PLAY para jugar.\n\n(Assets/Scenes/FlowCity.unity)", "Genial");
        Debug.Log("[FlowCity] Escena construida en Assets/Scenes/FlowCity.unity. Apretá Play.");
    }

    // ================= Helpers de construcción =================

    static GameObject Panel(Transform parent, string nombre, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static Text Texto(Transform parent, string nombre, string contenido, int tam, TextAnchor anchor)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.text = contenido;
        t.font = Font();
        t.fontSize = tam;
        t.alignment = anchor;
        t.color = HexC("#1C2833");
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void CrearBoton(Transform parent, string nombre, string label, Vector2 pos, Color color, out Button btn, out Text txt)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(440, 130);
        var img = go.AddComponent<Image>();
        img.color = color;
        btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(color.r * 1.15f, color.g * 1.15f, color.b * 1.15f, 1f);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f);
        btn.colors = colors;

        var txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Font();
        txt.fontSize = 40;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        StretchRT(txtGO.GetComponent<RectTransform>());
    }

    static void AddOutline(GameObject go)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = new Color(0, 0, 0, 0.7f);
        o.effectDistance = new Vector2(2, -2);
    }

    static void StretchRT(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Font Font()
    {
        // Fuente incorporada de Unity. En Unity 6 es "LegacyRuntime.ttf";
        // en versiones viejas era "Arial.ttf". Probamos ambas por seguridad.
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static Color HexC(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }

    // Asigna un campo privado [SerializeField] por reflexión (para cablear el GameManager).
    static void SetPriv(object target, string field, object value)
    {
        var f = target.GetType().GetField(field,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(target, value);
        else Debug.LogWarning("[FlowCity] No se encontró el campo: " + field);
    }
}
#endif
