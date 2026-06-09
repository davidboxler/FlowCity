#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// ============================================================================
//  FLOWCITY BUILDER (Editor)
//  ----------------------------------------------------------------------------
//  Construye TODA la escena con un clic: FlowCity > Construir Escena
//
//  Ambiente: edificios en ambos laterales, calle en perspectiva al centro,
//  auto visto de espaldas abajo, zona de obstáculo frente al auto.
//  UI: pregunta arriba del auto, 2 respuestas (izq/der), barra de tiempo,
//  HUD de velocidad, flash de feedback, panel final.
// ============================================================================
public static class FlowCityBuilder
{
    [MenuItem("FlowCity/Construir Escena")]
    public static void Construir()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Cámara ----------
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexC("#5DADE2");
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        // ---------- EventSystem ----------
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ---------- Canvas ----------
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ============ AMBIENTE ============
        // Cielo
        var cielo = Panel(canvasGO.transform, "Cielo", HexC("#5DADE2"));
        StretchRT(cielo.GetComponent<RectTransform>());
        // Vereda/piso (parte baja)
        var piso = Panel(canvasGO.transform, "Piso", HexC("#7F8C8D"));
        Anchor(piso, new Vector2(0, 0), new Vector2(1, 0.55f));

        // Calle en perspectiva (trapecio centrado, sube desde abajo)
        var calleGO = new GameObject("Calle");
        calleGO.transform.SetParent(canvasGO.transform, false);
        var calleRT = calleGO.AddComponent<RectTransform>();
        calleRT.anchorMin = new Vector2(0.5f, 0f);
        calleRT.anchorMax = new Vector2(0.5f, 0f);
        calleRT.pivot = new Vector2(0.5f, 0f);
        calleRT.anchoredPosition = new Vector2(0, 0);
        calleRT.sizeDelta = new Vector2(1100, 760);
        var calleImg = calleGO.AddComponent<Image>();
        // sprite generado en runtime (TextureFactory). Como el builder es editor,
        // dejamos un color base y un componente que lo genera al iniciar:
        calleImg.color = HexC("#4A5158");
        calleGO.AddComponent<CalleRuntime>();

        // Edificios laterales (siluetas)
        Edificios(canvasGO.transform, true);   // izquierda
        Edificios(canvasGO.transform, false);  // derecha

        // ---------- PANEL JUEGO ----------
        var panelJuego = new GameObject("PanelJuego");
        panelJuego.transform.SetParent(canvasGO.transform, false);
        StretchRT(panelJuego.AddComponent<RectTransform>());

        // Zona de obstáculo (frente al auto, centro-arriba de la calle)
        var obsGO = new GameObject("ZonaObstaculo");
        obsGO.transform.SetParent(panelJuego.transform, false);
        var obsRT = obsGO.AddComponent<RectTransform>();
        obsRT.anchorMin = new Vector2(0.5f, 0.5f);
        obsRT.anchorMax = new Vector2(0.5f, 0.5f);
        obsRT.pivot = new Vector2(0.5f, 0.5f);
        obsRT.anchoredPosition = new Vector2(0, 80);
        obsRT.sizeDelta = new Vector2(500, 360);
        var obstaculo = obsGO.AddComponent<ObstacleView>();

        // Auto (de espaldas) abajo centro
        AutoDeEspaldas(panelJuego.transform);

        // Progreso arriba
        var progreso = Texto(panelJuego.transform, "Progreso", "Pregunta 1 / 15", 34, TextAnchor.UpperCenter);
        Anchor01(progreso.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(500, 50));
        progreso.color = Color.white; AddOutline(progreso.gameObject);

        // Pregunta (banda arriba del auto)
        var bandaPreg = Panel(panelJuego.transform, "BandaPregunta", new Color(0,0,0,0.55f));
        Anchor01(bandaPreg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(1200, 110));
        var enunciado = Texto(bandaPreg.transform, "Enunciado", "Pregunta", 46, TextAnchor.MiddleCenter);
        StretchRT(enunciado.GetComponent<RectTransform>());
        enunciado.color = Color.white;

        // Barra de tiempo (debajo de la pregunta)
        var barraBG = Panel(panelJuego.transform, "BarraTiempoBG", HexC("#1C2833"));
        Anchor01(barraBG, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(760, 26));
        var barraFill = Panel(barraBG.transform, "BarraTiempoFill", HexC("#F1C40F"));
        var barraFillImg = barraFill.GetComponent<Image>();
        barraFillImg.type = Image.Type.Filled;
        barraFillImg.fillMethod = Image.FillMethod.Horizontal;
        barraFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        barraFillImg.fillAmount = 1f;
        StretchRT(barraFill.GetComponent<RectTransform>());

        // Botones de respuesta (izq y der, abajo)
        Button btnIzq; Text txtIzq;
        CrearBoton(panelJuego.transform, "BotonIzq", "Opción Izq", new Vector2(0.5f, 0f), new Vector2(-330, 60), HexC("#2980B9"), out btnIzq, out txtIzq);
        Button btnDer; Text txtDer;
        CrearBoton(panelJuego.transform, "BotonDer", "Opción Der", new Vector2(0.5f, 0f), new Vector2(330, 60), HexC("#16A085"), out btnDer, out txtDer);

        // HUD de velocidad (abajo izquierda)
        var velTxt = Texto(panelJuego.transform, "TextoVelocidad", "20 km/h", 54, TextAnchor.MiddleLeft);
        Anchor01(velTxt.gameObject, new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 60), new Vector2(360, 80));
        velTxt.color = Color.white; AddOutline(velTxt.gameObject);
        var velBG = Panel(panelJuego.transform, "VelocidadBG", HexC("#1C2833"));
        Anchor01(velBG, new Vector2(0, 0), new Vector2(0, 0), new Vector2(50, 130), new Vector2(260, 24));
        var velFill = Panel(velBG.transform, "VelocidadFill", HexC("#E74C3C"));
        var velFillImg = velFill.GetComponent<Image>();
        velFillImg.type = Image.Type.Filled;
        velFillImg.fillMethod = Image.FillMethod.Horizontal;
        velFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        velFillImg.fillAmount = 0.33f;
        StretchRT(velFill.GetComponent<RectTransform>());

        // Flash de feedback
        var flash = Panel(panelJuego.transform, "Flash", new Color(0,0,0,0));
        StretchRT(flash.GetComponent<RectTransform>());
        flash.GetComponent<Image>().raycastTarget = false;

        // ---------- PANEL FINAL ----------
        var panelFinal = new GameObject("PanelFinal");
        panelFinal.transform.SetParent(canvasGO.transform, false);
        StretchRT(panelFinal.AddComponent<RectTransform>());
        var pfBG = panelFinal.AddComponent<Image>();
        pfBG.color = new Color(0.08f, 0.1f, 0.14f, 0.94f);
        var resultado = Texto(panelFinal.transform, "Resultado", "Resultado", 50, TextAnchor.MiddleCenter);
        Anchor01(resultado.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(1100, 400));
        resultado.color = Color.white;
        Button btnReiniciar; Text txtReiniciar;
        CrearBoton(panelFinal.transform, "BotonReiniciar", "Volver a empezar", new Vector2(0.5f, 0f), new Vector2(0, 220), HexC("#2980B9"), out btnReiniciar, out txtReiniciar);

        // ---------- GAME MANAGER ----------
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        SetPriv(gm, "textoPregunta", enunciado);
        SetPriv(gm, "textoOpcionIzq", txtIzq);
        SetPriv(gm, "textoOpcionDer", txtDer);
        SetPriv(gm, "botonIzq", btnIzq);
        SetPriv(gm, "botonDer", btnDer);
        SetPriv(gm, "barraTiempo", barraFillImg);
        SetPriv(gm, "textoProgreso", progreso);
        SetPriv(gm, "obstaculo", obstaculo);
        SetPriv(gm, "textoVelocidad", velTxt);
        SetPriv(gm, "velocidadFill", velFillImg);
        SetPriv(gm, "panelJuego", panelJuego);
        SetPriv(gm, "panelFinal", panelFinal);
        SetPriv(gm, "textoResultado", resultado);
        SetPriv(gm, "botonReiniciar", btnReiniciar);
        SetPriv(gm, "flashColor", flash.GetComponent<Image>());

        panelFinal.SetActive(false);

        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/FlowCity.unity");
        EditorUtility.DisplayDialog("FlowCity",
            "Escena construida y guardada.\n\nApretá PLAY para jugar.\n(Assets/Scenes/FlowCity.unity)", "Genial");
        Debug.Log("[FlowCity] Escena lista. Apretá Play.");
    }

    // ============ piezas del ambiente ============

    static void Edificios(Transform parent, bool izquierda)
    {
        var cont = new GameObject(izquierda ? "EdificiosIzq" : "EdificiosDer");
        cont.transform.SetParent(parent, false);
        var rt = cont.AddComponent<RectTransform>();
        if (izquierda) { rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0.34f, 1f); }
        else           { rt.anchorMin = new Vector2(0.66f, 0); rt.anchorMax = new Vector2(1f, 1f); }
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        string[] tonos = { "#34495E", "#2C3E50", "#3B5266", "#283747" };
        float x = izquierda ? 0.1f : 0.0f;
        for (int i = 0; i < 4; i++)
        {
            var ed = Panel(cont.transform, "Edificio", HexC(tonos[i % tonos.Length]));
            var er = ed.GetComponent<RectTransform>();
            float ancho = 0.26f;
            float bx = (izquierda ? 0.04f : 0.06f) + i * 0.24f;
            float alto = 0.45f + (i % 3) * 0.12f;
            er.anchorMin = new Vector2(bx, 0.42f);
            er.anchorMax = new Vector2(bx + ancho, 0.42f + alto * 0.5f);
            er.offsetMin = Vector2.zero; er.offsetMax = Vector2.zero;
            Ventanas(ed.transform);
        }
    }

    static void Ventanas(Transform edificio)
    {
        for (int fila = 0; fila < 4; fila++)
            for (int col = 0; col < 2; col++)
            {
                var v = Panel(edificio, "Ventana", HexC("#F7DC6F"));
                var vr = v.GetComponent<RectTransform>();
                float vx = 0.2f + col * 0.4f;
                float vy = 0.12f + fila * 0.22f;
                vr.anchorMin = new Vector2(vx, vy);
                vr.anchorMax = new Vector2(vx + 0.18f, vy + 0.12f);
                vr.offsetMin = Vector2.zero; vr.offsetMax = Vector2.zero;
            }
    }

    static void AutoDeEspaldas(Transform parent)
    {
        var auto = new GameObject("Auto");
        auto.transform.SetParent(parent, false);
        var rt = auto.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, -10);
        rt.sizeDelta = new Vector2(420, 240);

        // cuerpo
        var cuerpo = Panel(auto.transform, "Cuerpo", HexC("#C0392B"));
        Anchor01(cuerpo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(360, 170));
        // techo/luneta
        var luneta = Panel(auto.transform, "Luneta", HexC("#2C3E50"));
        Anchor01(luneta, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(260, 90));
        // paragolpes
        var paragolpe = Panel(auto.transform, "Paragolpe", HexC("#1B2631"));
        Anchor01(paragolpe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(380, 40));
        // luces traseras
        var luzIzq = Panel(auto.transform, "LuzIzq", HexC("#F1948A"));
        Anchor01(luzIzq, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150, 80), new Vector2(50, 36));
        var luzDer = Panel(auto.transform, "LuzDer", HexC("#F1948A"));
        Anchor01(luzDer, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150, 80), new Vector2(50, 36));
    }

    // ============ helpers UI ============

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
        t.fontStyle = FontStyle.Bold;
        t.alignment = anchor;
        t.color = HexC("#1C2833");
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void CrearBoton(Transform parent, string nombre, string label, Vector2 anchor, Vector2 pos, Color color, out Button btn, out Text txt)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(520, 130);
        var img = go.AddComponent<Image>();
        img.color = color;
        btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(Mathf.Min(1,color.r*1.2f), Mathf.Min(1,color.g*1.2f), Mathf.Min(1,color.b*1.2f), 1f);
        colors.pressedColor = new Color(color.r*0.8f, color.g*0.8f, color.b*0.8f, 1f);
        btn.colors = colors;

        var txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Font();
        txt.fontSize = 38;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        StretchRT(txtGO.GetComponent<RectTransform>());
    }

    static void AddOutline(GameObject go)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = new Color(0, 0, 0, 0.75f);
        o.effectDistance = new Vector2(2, -2);
    }

    static void Anchor(GameObject go, Vector2 min, Vector2 max)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Anchor01(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void StretchRT(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static Font Font()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static Color HexC(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }

    static void SetPriv(object target, string field, object value)
    {
        var f = target.GetType().GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(target, value);
        else Debug.LogWarning("[FlowCity] Campo no encontrado: " + field);
    }
}
#endif
