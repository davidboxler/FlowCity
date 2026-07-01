#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

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
        // Horizonte: la línea donde el suelo se junta con el cielo (≈48% de la altura).
        const float HORIZONTE = 0.48f;

        // Cielo (de la mitad para arriba) — gradiente para dar profundidad
        var cielo = Panel(canvasGO.transform, "Cielo", Color.white);
        Anchor(cielo, new Vector2(0, HORIZONTE), new Vector2(1, 1f));
        var cieloG = cielo.AddComponent<GradientBackground>();
        cieloG.abajo = FlowTheme.SKY_LOW;   // claro en el horizonte
        cieloG.arriba = FlowTheme.SKY_TOP;  // más profundo arriba
        // Suelo (de la mitad para abajo) — la calle se apoya ACÁ
        var piso = Panel(canvasGO.transform, "Piso", FlowTheme.GROUND);
        Anchor(piso, new Vector2(0, 0), new Vector2(1, HORIZONTE));

        // Edificios al FONDO (sobre el horizonte, lejanos: violeta/negro oscuro)
        EdificiosFondo(canvasGO.transform, HORIZONTE);

        // Calle en perspectiva: arranca abajo y su PUNTO DE FUGA termina justo en el
        // horizonte (no sube hasta el cielo). Por eso la altura llega hasta HORIZONTE.
        var calleGO = new GameObject("Calle");
        calleGO.transform.SetParent(canvasGO.transform, false);
        var calleRT = calleGO.AddComponent<RectTransform>();
        calleRT.anchorMin = new Vector2(0.5f, 0f);
        calleRT.anchorMax = new Vector2(0.5f, 0f);
        calleRT.pivot = new Vector2(0.5f, 0f);
        calleRT.anchoredPosition = new Vector2(0, 0);
        // alto = hasta el horizonte (1080 * 0.48 ≈ 520). Ancho base amplio.
        calleRT.sizeDelta = new Vector2(1300, 1080 * HORIZONTE);
        var calleImg = calleGO.AddComponent<Image>();
        calleImg.color = HexC("#4A5158");
        calleGO.AddComponent<CalleRuntime>();

        // Edificios laterales (siluetas cercanas, negras con ventanas amarillas)
        Edificios(canvasGO.transform, true);   // izquierda
        Edificios(canvasGO.transform, false);  // derecha

        // ---------- PANEL JUEGO ----------
        var panelJuego = new GameObject("PanelJuego");
        panelJuego.transform.SetParent(canvasGO.transform, false);
        StretchRT(panelJuego.AddComponent<RectTransform>());

        // Vehículos: se crean los DOS; el GameManager activa el elegido (auto/moto).
        var auto = VehiculoAuto(panelJuego.transform);
        var moto = VehiculoMoto(panelJuego.transform);
        moto.SetActive(false);

        // Panel "GLASS" detrás de los botones (vidrio esmerilado translúcido).
        var glass = Panel(panelJuego.transform, "GlassBotones", new Color(1f, 1f, 1f, 0.10f));
        Anchor01(glass, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 82), new Vector2(1340, 210));
        Redondear(glass, FlowTheme.R_TARJETA);               // vidrio con esquinas suaves
        glass.GetComponent<Image>().raycastTarget = false;   // que no robe clicks
        var glassOl = glass.AddComponent<Outline>();
        glassOl.effectColor = new Color(1f, 1f, 1f, 0.22f);   // borde claro (brillo del vidrio)
        glassOl.effectDistance = new Vector2(1.5f, -1.5f);
        var glassSh = glass.AddComponent<Shadow>();
        glassSh.effectColor = new Color(0f, 0f, 0f, 0.28f);
        glassSh.effectDistance = new Vector2(0, -6);

        // Botones de respuesta (izq y der, abajo) — sobre el glass. Creados DESPUÉS para ir encima.
        Button btnIzq; TMP_Text txtIzq;
        CrearBoton(panelJuego.transform, "BotonIzq", "Opción Izq", "<", new Vector2(0.5f, 0f), new Vector2(-350, 75), FlowTheme.PRIMARY, out btnIzq, out txtIzq);
        Button btnDer; TMP_Text txtDer;
        CrearBoton(panelJuego.transform, "BotonDer", "Opción Der", ">", new Vector2(0.5f, 0f), new Vector2(350, 75), HexC("#14B8A6"), out btnDer, out txtDer);

        // HUD de velocidad — POPUP FLOTANTE (claro/grisáceo, con sombra, despegado del borde)
        TMP_Text velTxt; Image velFillImg;
        PopupVelocidad(panelJuego.transform, out velTxt, out velFillImg);

        // Flash de feedback
        var flash = Panel(panelJuego.transform, "Flash", new Color(0,0,0,0));
        StretchRT(flash.GetComponent<RectTransform>());
        flash.GetComponent<Image>().raycastTarget = false;

        // ---------- PANEL SEÑAL (pantalla completa, fase de memorización) ----------
        // Se dibuja DESPUÉS del panel de juego para quedar por encima y tapar la ciudad.
        var panelSenal = new GameObject("PanelSenal");
        panelSenal.transform.SetParent(canvasGO.transform, false);
        StretchRT(panelSenal.AddComponent<RectTransform>());
        var psBG = panelSenal.AddComponent<Image>();
        psBG.color = FlowTheme.BG0;   // fondo profundo OPACO (tapa la ciudad detrás)

        // Banda superior institucional
        var psBanda = Panel(panelSenal.transform, "Banda", FlowTheme.PRIMARY);
        Anchor(psBanda, new Vector2(0, 0.92f), new Vector2(1, 1f));
        var psInst = Texto(psBanda.transform, "Inst", "INSTITUTO DE FORMACIÓN VIAL", 24, TextAnchor.MiddleCenter);
        psInst.color = Color.white; StretchRT(psInst.GetComponent<RectTransform>());

        // Progreso (arriba a la izquierda)
        var progreso = Texto(panelSenal.transform, "Progreso", "Pregunta 1 / 15", 32, TextAnchor.MiddleLeft);
        progreso.color = Color.white;
        Anchor01(progreso.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250, -120), new Vector2(420, 50));

        // Título
        var psTitulo = Texto(panelSenal.transform, "Titulo", "MEMORIZÁ LA SEÑAL", 40, TextAnchor.MiddleCenter);
        psTitulo.color = FlowTheme.PRIMARYH;
        Anchor01(psTitulo.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -130), new Vector2(700, 60));

        // Cuenta regresiva (arriba a la derecha: rótulo + número grande)
        var cuentaCap = Texto(panelSenal.transform, "CuentaCap", "TIEMPO", 22, TextAnchor.MiddleCenter);
        cuentaCap.color = FlowTheme.TXT_MD;
        Anchor01(cuentaCap.gameObject, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150, -98), new Vector2(220, 30));
        var cuenta = Texto(panelSenal.transform, "Cuenta", "5", 120, TextAnchor.MiddleCenter);
        cuenta.color = FlowTheme.WARN; AddOutline(cuenta.gameObject);
        Anchor01(cuenta.gameObject, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150, -190), new Vector2(220, 150));

        // Señal grande (centro)
        var zonaSenalGO = new GameObject("ZonaSenal");
        zonaSenalGO.transform.SetParent(panelSenal.transform, false);
        var zsRT = zonaSenalGO.AddComponent<RectTransform>();
        zsRT.anchorMin = new Vector2(0.5f, 0.5f); zsRT.anchorMax = new Vector2(0.5f, 0.5f);
        zsRT.pivot = new Vector2(0.5f, 0.5f);
        zsRT.anchoredPosition = new Vector2(0, 70); zsRT.sizeDelta = new Vector2(540, 480);
        var obstaculoSenal = zonaSenalGO.AddComponent<ObstacleView>();

        // Pregunta (banda inferior)
        var bandaPreg = Panel(panelSenal.transform, "BandaPregunta", FlowTheme.BG1);
        Anchor01(bandaPreg, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 155), new Vector2(1320, 150));
        Redondear(bandaPreg, FlowTheme.R_TARJETA);   // tarjeta de la pregunta
        var enunciadoSenal = Texto(bandaPreg.transform, "PreguntaSenal", "Pregunta", 44, TextAnchor.MiddleCenter);
        enunciadoSenal.color = Color.white;
        var enRT = enunciadoSenal.GetComponent<RectTransform>();
        enRT.anchorMin = Vector2.zero; enRT.anchorMax = Vector2.one;
        enRT.offsetMin = new Vector2(40, 0); enRT.offsetMax = new Vector2(-40, 0);

        // Ayuda
        var psAyuda = Texto(panelSenal.transform, "Ayuda", "Respondé en la ciudad cuando la señal desaparezca", 24, TextAnchor.MiddleCenter);
        psAyuda.color = FlowTheme.TXT_MD;
        Anchor01(psAyuda.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(1100, 40));

        // ---------- PANEL FINAL ----------
        var panelFinal = new GameObject("PanelFinal");
        panelFinal.transform.SetParent(canvasGO.transform, false);
        StretchRT(panelFinal.AddComponent<RectTransform>());
        var pfBG = panelFinal.AddComponent<Image>();
        pfBG.color = new Color(FlowTheme.BG0.r, FlowTheme.BG0.g, FlowTheme.BG0.b, 0.96f);
        var resultado = Texto(panelFinal.transform, "Resultado", "Resultado", 50, TextAnchor.MiddleCenter);
        Anchor01(resultado.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(1100, 400));
        resultado.color = Color.white;
        Button btnReiniciar; TMP_Text txtReiniciar;
        CrearBoton(panelFinal.transform, "BotonReiniciar", "Volver a empezar", "", new Vector2(0.5f, 0f), new Vector2(0, 220), HexC("#2980B9"), out btnReiniciar, out txtReiniciar);

        // ---------- GAME MANAGER ----------
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        SetPriv(gm, "panelSenal", panelSenal);
        SetPriv(gm, "obstaculoSenal", obstaculoSenal);
        SetPriv(gm, "textoPreguntaSenal", enunciadoSenal);
        SetPriv(gm, "textoCuenta", cuenta);
        SetPriv(gm, "textoProgreso", progreso);
        SetPriv(gm, "textoOpcionIzq", txtIzq);
        SetPriv(gm, "textoOpcionDer", txtDer);
        SetPriv(gm, "botonIzq", btnIzq);
        SetPriv(gm, "botonDer", btnDer);
        SetPriv(gm, "vehiculoAuto", auto);
        SetPriv(gm, "vehiculoMoto", moto);
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

    // Edificios CERCANOS a los lados (siluetas negras con ventanas amarillas).
    // Se apoyan en el horizonte y bajan; llenan todo el borde lateral.
    static void Edificios(Transform parent, bool izquierda)
    {
        const float HORIZONTE = 0.48f;
        var cont = new GameObject(izquierda ? "EdificiosIzq" : "EdificiosDer");
        cont.transform.SetParent(parent, false);
        var rt = cont.AddComponent<RectTransform>();
        // ocupan desde el borde hasta donde empieza la calle (~28% de cada lado)
        if (izquierda) { rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0.30f, 1f); }
        else           { rt.anchorMin = new Vector2(0.70f, 0); rt.anchorMax = new Vector2(1f, 1f); }
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        string[] tonos = { "#1B1B22", "#23232E", "#15151B", "#2A2533" };
        // 5 edificios por lado, pegados, distintas alturas, base en el horizonte
        int n = 5;
        for (int i = 0; i < n; i++)
        {
            var ed = Panel(cont.transform, "Edificio", HexC(tonos[i % tonos.Length]));
            var er = ed.GetComponent<RectTransform>();
            float ancho = 1f / n;
            float bx = i * ancho;
            // alturas variadas: la silueta sube desde el horizonte
            float alturas = 0.30f + ((i * 7 + (izquierda ? 0 : 3)) % 5) * 0.07f;
            er.anchorMin = new Vector2(bx + 0.01f, HORIZONTE - 0.02f);
            er.anchorMax = new Vector2(bx + ancho - 0.01f, HORIZONTE - 0.02f + alturas);
            er.offsetMin = Vector2.zero; er.offsetMax = Vector2.zero;
            Ventanas(ed.transform);
        }
    }

    // Edificios LEJANOS al fondo (sobre el horizonte, centro): negro/violeta oscuro,
    // más bajos y sin ventanas (o pocas), para dar profundidad a la ciudad.
    static void EdificiosFondo(Transform parent, float horizonte)
    {
        var cont = new GameObject("EdificiosFondo");
        cont.transform.SetParent(parent, false);
        var rt = cont.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.15f, 0); rt.anchorMax = new Vector2(0.85f, 1f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        string[] tonos = { "#2C2440", "#1E1A2E", "#322A47", "#251F38" };
        int n = 9;
        for (int i = 0; i < n; i++)
        {
            var ed = Panel(cont.transform, "EdificioFondo", HexC(tonos[i % tonos.Length]));
            var er = ed.GetComponent<RectTransform>();
            float ancho = 1f / n;
            float bx = i * ancho;
            float alturas = 0.10f + ((i * 5) % 4) * 0.045f; // bajos (lejanos)
            er.anchorMin = new Vector2(bx + 0.004f, horizonte);
            er.anchorMax = new Vector2(bx + ancho - 0.004f, horizonte + alturas);
            er.offsetMin = Vector2.zero; er.offsetMax = Vector2.zero;
        }
    }

    static void Ventanas(Transform edificio)
    {
        for (int fila = 0; fila < 5; fila++)
            for (int col = 0; col < 3; col++)
            {
                // algunas apagadas para que se vea más natural
                bool encendida = ((fila * 3 + col) % 4) != 0;
                var v = Panel(edificio, "Ventana", encendida ? HexC("#F7DC6F") : HexC("#5A4A1A"));
                var vr = v.GetComponent<RectTransform>();
                float vx = 0.14f + col * 0.28f;
                float vy = 0.10f + fila * 0.17f;
                vr.anchorMin = new Vector2(vx, vy);
                vr.anchorMax = new Vector2(vx + 0.16f, vy + 0.09f);
                vr.offsetMin = Vector2.zero; vr.offsetMax = Vector2.zero;
            }
    }

    // Auto visto de espaldas (centro-abajo). Devuelve el objeto para activarlo/desactivarlo.
    static GameObject VehiculoAuto(Transform parent)
    {
        var auto = new GameObject("Auto");
        auto.transform.SetParent(parent, false);
        var rt = auto.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 150); // levantado del piso para no solaparse con los botones/velocidad
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
        return auto;
    }

    // Moto vista de espaldas (con conductor). Mismo anclaje que el auto.
    static GameObject VehiculoMoto(Transform parent)
    {
        var moto = new GameObject("Moto");
        moto.transform.SetParent(parent, false);
        var rt = moto.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 150);
        rt.sizeDelta = new Vector2(320, 320);

        // rueda trasera (neumático ancho)
        var rueda = Panel(moto.transform, "Rueda", HexC("#1B2631"));
        Anchor01(rueda, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 45), new Vector2(96, 180));
        // luz trasera
        var luz = Panel(moto.transform, "Luz", HexC("#F1948A"));
        Anchor01(luz, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(54, 26));
        // cuerpo/colín (color del vehículo)
        var cuerpo = Panel(moto.transform, "Cuerpo", HexC("#E67E22"));
        Anchor01(cuerpo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(150, 140));
        // espalda del conductor
        var espalda = Panel(moto.transform, "Espalda", HexC("#2C3E50"));
        Anchor01(espalda, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 245), new Vector2(124, 150));
        // hombros
        var hombros = Panel(moto.transform, "Hombros", HexC("#34495E"));
        Anchor01(hombros, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 300), new Vector2(170, 50));
        // casco
        var casco = Panel(moto.transform, "Casco", HexC("#34495E"));
        Anchor01(casco, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 350), new Vector2(92, 92));
        // manubrios
        var manI = Panel(moto.transform, "ManI", HexC("#1B2631"));
        Anchor01(manI, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100, 240), new Vector2(70, 18));
        var manD = Panel(moto.transform, "ManD", HexC("#1B2631"));
        Anchor01(manD, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(100, 240), new Vector2(70, 18));
        return moto;
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

    static TextMeshProUGUI Texto(Transform parent, string nombre, string contenido, int tam, TextAnchor anchor)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = tam;
        t.fontStyle = FontStyles.Bold;
        t.alignment = Alinear(anchor);
        t.color = HexC("#1C2833");
        return t;
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

    // Redondea las esquinas de un GameObject que ya tiene Image.
    static void Redondear(GameObject go, int radio)
    {
        var r = go.AddComponent<RoundedImage>();
        r.radius = radio;
    }

    // Botón mejorado: más grande, con sombra (efecto flotante), bisel inferior,
    // realce al pasar/presionar y una flecha lateral ("<" o ">").
    static void CrearBoton(Transform parent, string nombre, string label, string flecha, Vector2 anchor, Vector2 pos, Color color, out Button btn, out TMP_Text txt)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(580, 150);
        var img = go.AddComponent<Image>();
        img.color = color;
        Redondear(go, FlowTheme.R_BOTON);                 // esquinas redondeadas

        // sombra: hace que el botón "flote" sobre la escena
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.45f);
        sh.effectDistance = new Vector2(0, -7);

        btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = FlowTheme.Mul(color, 1.15f);
        colors.pressedColor = FlowTheme.Mul(color, 0.82f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        // flecha lateral (lado externo según el botón)
        if (!string.IsNullOrEmpty(flecha))
        {
            bool izq = flecha == "<";
            var fl = Texto(go.transform, "Flecha", flecha, 80, TextAnchor.MiddleCenter);
            fl.color = new Color(1, 1, 1, 0.85f);
            Anchor01(fl.gameObject, new Vector2(izq ? 0f : 1f, 0.5f), new Vector2(izq ? 0f : 1f, 0.5f), new Vector2(izq ? 44 : -44, 4), new Vector2(70, 90));
        }

        // texto principal (centrado, con margen lateral para no pisar la flecha)
        var txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 40;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        txt = tmp;
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0); trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(85, 14); trt.offsetMax = new Vector2(-85, -6);
    }

    // Popup flotante de velocidad: tarjeta clara/grisácea con sombra, despegada del
    // borde (arriba a la derecha), número grande y barra de progreso.
    static void PopupVelocidad(Transform parent, out TMP_Text velTxt, out Image velFillImg)
    {
        var pop = Panel(parent, "VelocidadPopup", new Color(0.96f, 0.97f, 0.98f, 0.95f));
        Anchor01(pop, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(185, 120), new Vector2(300, 132));
        Redondear(pop, FlowTheme.R_POPUP);
        var sh = pop.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.38f);
        sh.effectDistance = new Vector2(5, -5);
        var ol = pop.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.10f);
        ol.effectDistance = new Vector2(1, -1);

        var cap = Texto(pop.transform, "Cap", "VELOCIDAD", 20, TextAnchor.UpperCenter);
        cap.color = HexC("#7F8C8D");
        Anchor01(cap.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -14), new Vector2(280, 28));

        velTxt = Texto(pop.transform, "TextoVelocidad", "20 km/h", 46, TextAnchor.MiddleCenter);
        velTxt.color = HexC("#1C2833");
        Anchor01(velTxt.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 8), new Vector2(290, 60));

        var velBG = Panel(pop.transform, "VelTrackBG", HexC("#D5DBDB"));
        Anchor01(velBG, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 18), new Vector2(250, 16));
        Redondear(velBG, 8);
        var velFill = Panel(velBG.transform, "VelFill", FlowTheme.PRIMARY);
        velFillImg = velFill.GetComponent<Image>();
        velFillImg.type = Image.Type.Filled;
        velFillImg.fillMethod = Image.FillMethod.Horizontal;
        velFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        velFillImg.fillAmount = 0.33f;
        StretchRT(velFill.GetComponent<RectTransform>());
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
