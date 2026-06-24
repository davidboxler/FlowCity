using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  OBSTACLE VIEW — dibuja la señal/situación frente al auto CON FIGURAS REALES
//  ----------------------------------------------------------------------------
//  Cada señal se arma componiendo formas (círculos, triángulos, barras, anillos,
//  octágono, media luna) en lugar de palabras de texto. Quedan como señales de
//  verdad pero en el estilo geométrico simple del juego.
//
//    - Reglamentarias de PROHIBICIÓN: círculo blanco + aro rojo + símbolo negro
//      + barra roja diagonal (slash).
//    - Reglamentarias de OBLIGACIÓN: círculo azul + símbolo blanco.
//    - Advertencia: diamante amarillo con borde negro + símbolo negro.
//    - PARE: octágono rojo. CEDA: triángulo invertido con borde.
//    - Semáforo y senda peatonal: como antes (quedaron muy bien).
// ============================================================================
public class ObstacleView : MonoBehaviour
{
    private RectTransform area;     // zona frente al auto donde aparece el obstáculo
    private GameObject extra;       // elemento dibujado sobre la calle (ej: senda), fuera de la zona

    static readonly Color ROJO   = new Color(0.75f, 0.22f, 0.17f);  // #C0392B
    static readonly Color AZUL   = new Color(0.14f, 0.44f, 0.64f);  // #2471A3
    static readonly Color VERDE  = new Color(0.15f, 0.68f, 0.38f);  // #27AE60
    static readonly Color NEGRO  = new Color(0.106f, 0.149f, 0.192f); // #1B2631
    static readonly Color AMA    = new Color(0.945f, 0.769f, 0.059f); // #F1C40F

    void Awake()
    {
        area = GetComponent<RectTransform>();
    }

    public void Mostrar(string tipo)
    {
        Limpiar();
        if (extra != null) { Destroy(extra); extra = null; }

        // Semáforos: SIEMPRE dibujados por código (quedaron muy bien).
        if (tipo == "semaforo_rojo")     { Semaforo(0); return; }
        if (tipo == "semaforo_amarillo") { Semaforo(1); return; }
        if (tipo == "semaforo_verde")    { Semaforo(2); return; }

        // Resto: si existe una imagen real en Resources/Senales/{tipo}.png, se usa esa.
        // Si no hay imagen, cae al dibujo geométrico de abajo (fallback, nada se rompe).
        if (MostrarImagen(tipo)) return;

        switch (tipo)
        {
            // ---- Semáforos (ya resueltos arriba; quedan por compatibilidad) ----
            case "semaforo_rojo":      Semaforo(0); break;
            case "semaforo_amarillo":  Semaforo(1); break;
            case "semaforo_verde":     Semaforo(2); break;

            // ---- PARE / CEDA ----
            case "pare":               Pare(); break;
            case "ceda":               Ceda(); break;

            // ---- Senda peatonal (sobre el asfalto) ----
            case "senda":              SendaPeatonal(); break;

            // ============ PROHIBICIÓN (círculo blanco + aro rojo + slash) ============
            case "limite40":
            {
                var t = Prohibicion();
                Etiqueta(t, "40", 64, NEGRO);          // el "40" ES el símbolo real
                break;
            }
            case "no_izquierda":
            {
                var t = Prohibicion();
                Flecha(t, NEGRO, new Vector2(0, 0), 80f, 90f);   // flecha a la izquierda
                Slash(t);
                break;
            }
            case "no_adelantar":
            {
                var t = Prohibicion();
                Auto(t, ROJO,  new Vector2(-24, -2), 40f);
                Auto(t, NEGRO, new Vector2( 24,  4), 40f);
                Slash(t);
                break;
            }
            case "contramano":         NoEntrar(); break;       // círculo rojo + barra blanca
            case "telefono":
            {
                var t = Prohibicion();
                Img(t, "Tubo", null, NEGRO, new Vector2(0, 0), new Vector2(20, 54), 35f);
                Img(t, "Tubo2", null, NEGRO, new Vector2(-16, 14), new Vector2(26, 16), 35f);
                Img(t, "Tubo3", null, NEGRO, new Vector2(16, -14), new Vector2(26, 16), 35f);
                Slash(t);
                break;
            }
            case "alcohol":
            {
                var t = Prohibicion();
                Img(t, "Copa", TextureFactory.Triangulo(), NEGRO, new Vector2(0, 10), new Vector2(48, 40), 180f);
                Img(t, "Pie",  null, NEGRO, new Vector2(0, -18), new Vector2(7, 30));
                Img(t, "Base", null, NEGRO, new Vector2(0, -34), new Vector2(36, 7));
                Slash(t);
                break;
            }

            // ============ OBLIGACIÓN (círculo azul + símbolo blanco) ============
            case "obligatoria_derecha":
            {
                var t = Obligacion(AZUL);
                Flecha(t, Color.white, new Vector2(0, 0), 84f, -90f);  // flecha a la derecha
                break;
            }
            case "rotonda":            Rotonda(); break;
            case "ciclista":           Bicicleta(AZUL, Color.white); break;
            case "estacionar":         Estacionar(); break;       // cuadrado azul + "P"
            case "cinturon":           Cinturon(); break;         // círculo verde + cinturón
            case "distancia":          Distancia(); break;

            // ============ ADVERTENCIA (diamante amarillo) ============
            case "lomo":               Lomo(); break;
            case "curva":              Curva(); break;
            case "tren":               Tren(); break;
            case "esquina":            Cruce(); break;
            case "neblina":            Neblina(); break;
            case "resbaladizo":        Resbaladizo(); break;
            case "escuela":            Escuela(); break;
            case "obras":              Obras(); break;
            case "peligro":
            {
                var t = DiamanteBase();
                Etiqueta(t, "!", 80, NEGRO);
                break;
            }

            // ============ Situaciones (caja con símbolo) ============
            case "ambulancia":         Ambulancia(); break;
            case "agente":             Agente(); break;
            case "noche":              Noche(); break;

            default:                   CajaSimple(new Color(0.34f,0.4f,0.45f), "?"); break;
        }
    }

    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    // Carga Resources/Senales/{tipo}.png (si existe) y la muestra encuadrada en el área,
    // respetando la proporción original. Devuelve true si encontró y mostró la imagen.
    bool MostrarImagen(string tipo)
    {
        Texture2D tex = Resources.Load<Texture2D>("Senales/" + tipo);
        if (tex == null) return false;

        var go = new GameObject("SenalImg");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Tamaño del área: lo tomamos del propio transform para NO depender de Awake
        // (Mostrar puede llamarse con el panel todavía inactivo y area sería null).
        RectTransform areaRT = transform as RectTransform;
        float areaW = (areaRT != null && areaRT.rect.width  > 1f) ? areaRT.rect.width  : 480f;
        float areaH = (areaRT != null && areaRT.rect.height > 1f) ? areaRT.rect.height : 420f;

        // Encajar dentro del área respetando proporción (con un pequeño margen).
        float maxW = areaW * 0.92f;
        float maxH = areaH * 0.92f;
        float aspecto = (float)tex.width / tex.height;
        float w = maxW, h = w / aspecto;
        if (h > maxH) { h = maxH; w = h * aspecto; }
        rt.sizeDelta = new Vector2(w, h);

        var img = go.AddComponent<Image>();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        img.preserveAspect = true;
        return true;
    }

    // ============================================================================
    //  SEÑALES COMPUESTAS
    // ============================================================================

    // ---- Semáforo: poste + caja negra con 3 luces. luzOn: 0=rojo,1=amarillo,2=verde
    void Semaforo(int luzOn)
    {
        bool ladoIzq = Random.value < 0.5f;
        float xCaja = ladoIzq ? -90f : 90f;

        Rect("Poste", new Color(0.36f,0.43f,0.49f), new Vector2(xCaja, -120), new Vector2(14, 240));
        var caja = Rect("CajaSemaforo", NEGRO, new Vector2(xCaja, 40), new Vector2(70, 180));

        Color rojo    = (luzOn == 0) ? new Color(0.91f,0.30f,0.24f) : new Color(0.36f,0.17f,0.17f);
        Color amarillo= (luzOn == 1) ? AMA                          : new Color(0.36f,0.33f,0.12f);
        Color verde   = (luzOn == 2) ? new Color(0.18f,0.80f,0.44f) : new Color(0.12f,0.30f,0.20f);
        Luz(caja.transform, rojo,     new Vector2(0, 55));
        Luz(caja.transform, amarillo, new Vector2(0, 0));
        Luz(caja.transform, verde,    new Vector2(0, -55));
    }

    void Luz(Transform parent, Color color, Vector2 pos)
    {
        Img(parent, "Luz", TextureFactory.Circulo(), color, pos, new Vector2(48, 48));
    }

    // ---- PARE: octágono rojo con borde blanco + "PARE"
    void Pare()
    {
        Poste(0f);
        Img(transform, "Par008Borde", TextureFactory.Octagono(), Color.white, new Vector2(0, 30), new Vector2(160, 160));
        var go = Img(transform, "Pare", TextureFactory.Octagono(), ROJO, new Vector2(0, 30), new Vector2(140, 140));
        Etiqueta(go.transform, "PARE", 40, Color.white);
    }

    // ---- CEDA: triángulo invertido rojo con interior blanco (borde rojo)
    void Ceda()
    {
        Poste(0f);
        Img(transform, "CedaBorde", TextureFactory.Triangulo(), ROJO, new Vector2(0, 34), new Vector2(170, 170), 180f);
        Img(transform, "CedaInt",   TextureFactory.Triangulo(), Color.white, new Vector2(0, 22), new Vector2(118, 118), 180f);
    }

    // ---- Senda peatonal: dentro de la zona de señal (NO se ancla al Canvas, así no se
    //      "filtra" a la pantalla de ciudad y desaparece junto con el panel de la señal).
    void SendaPeatonal()
    {
        extra = new GameObject("SendaPeatonal");
        extra.transform.SetParent(transform, false);   // hija de la ZonaSenal
        var rt = extra.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -40);
        rt.sizeDelta = new Vector2(460, 220);

        Img(extra.transform, "Cebra", TextureFactory.SendaPeatonal(512, 256, 6), Color.white,
            new Vector2(0, 0), new Vector2(440, 200));
        Peaton(extra.transform, new Vector2(0, 30), 1.15f, NEGRO);
    }

    // ---- No entrar / contramano: círculo rojo lleno + barra blanca horizontal
    void NoEntrar()
    {
        Poste(0f);
        var go = Img(transform, "NoEntrar", TextureFactory.Circulo(), ROJO, new Vector2(0, 30), new Vector2(150, 150));
        Img(go.transform, "Barra", null, Color.white, new Vector2(0, 0), new Vector2(86, 24));
    }

    // ---- Rotonda: círculo azul + aro blanco + 3 flechitas girando
    void Rotonda()
    {
        var t = Obligacion(AZUL);
        Img(t, "Aro", TextureFactory.Anillo(128, 0.13f), Color.white, Vector2.zero, new Vector2(92, 92));
        Img(t, "f1", TextureFactory.Triangulo(), Color.white, new Vector2(0, 46),  new Vector2(28, 24), -90f);
        Img(t, "f2", TextureFactory.Triangulo(), Color.white, new Vector2(40, -22), new Vector2(28, 24), 150f);
        Img(t, "f3", TextureFactory.Triangulo(), Color.white, new Vector2(-40, -22), new Vector2(28, 24), 30f);
    }

    // ---- Bicicleta (ciclista): 2 ruedas (anillos) + cuadro
    void Bicicleta(Color fondo, Color trazo)
    {
        var t = Obligacion(fondo);
        Img(t, "R1", TextureFactory.Anillo(128, 0.16f), trazo, new Vector2(-32, -16), new Vector2(48, 48));
        Img(t, "R2", TextureFactory.Anillo(128, 0.16f), trazo, new Vector2(32, -16),  new Vector2(48, 48));
        Img(t, "c1", null, trazo, new Vector2(0, 0),   new Vector2(56, 6), 18f);
        Img(t, "c2", null, trazo, new Vector2(-12, 2), new Vector2(6, 42), -22f);
        Img(t, "c3", null, trazo, new Vector2(18, 4),  new Vector2(6, 44), 14f);
        Img(t, "man", null, trazo, new Vector2(34, 20), new Vector2(24, 5));
        Img(t, "asiento", null, trazo, new Vector2(-14, 24), new Vector2(22, 6));
    }

    // ---- Estacionar: cuadrado azul + "P" blanca
    void Estacionar()
    {
        Poste(0f);
        var go = Img(transform, "Estac", null, AZUL, new Vector2(0, 30), new Vector2(140, 140));
        Etiqueta(go.transform, "P", 80, Color.white);
    }

    // ---- Cinturón: círculo verde + banda diagonal + hebilla
    void Cinturon()
    {
        Poste(0f);
        var go = Img(transform, "Cint", TextureFactory.Circulo(), VERDE, new Vector2(0, 30), new Vector2(150, 150));
        Img(go.transform, "Banda", null, Color.white, new Vector2(0, 0), new Vector2(22, 150), -25f);
        Img(go.transform, "Hebilla", null, NEGRO, new Vector2(-8, -16), new Vector2(28, 20));
    }

    // ---- Distancia entre vehículos (diamante): dos autos + flecha doble
    void Distancia()
    {
        var t = DiamanteBase();
        Auto(t, NEGRO, new Vector2(0, 30), 56f);
        Auto(t, NEGRO, new Vector2(0, -34), 56f);
        Img(t, "lin", null, NEGRO, new Vector2(0, -2), new Vector2(5, 30));
        Img(t, "pa", TextureFactory.Triangulo(), NEGRO, new Vector2(0, 12),  new Vector2(20, 14));
        Img(t, "pb", TextureFactory.Triangulo(), NEGRO, new Vector2(0, -16), new Vector2(20, 14), 180f);
    }

    // ---- Lomo de burro: media luna negra apoyada en el piso
    void Lomo()
    {
        var t = DiamanteBase();
        Img(t, "Piso", null, NEGRO, new Vector2(0, -30), new Vector2(110, 7));
        Img(t, "Lomo", TextureFactory.Semicirculo(), NEGRO, new Vector2(0, -26), new Vector2(96, 48));
    }

    // ---- Curva: flecha que dobla
    void Curva()
    {
        var t = DiamanteBase();
        Img(t, "tramo1", null, NEGRO, new Vector2(-12, -30), new Vector2(16, 44));
        Img(t, "tramo2", null, NEGRO, new Vector2(2, -2),    new Vector2(16, 46), -40f);
        Img(t, "punta", TextureFactory.Triangulo(), NEGRO, new Vector2(22, 26), new Vector2(36, 32), -25f);
    }

    // ---- Paso a nivel (tren): cruz de San Andrés (X negra)
    void Tren()
    {
        var t = DiamanteBase();
        Img(t, "x1", null, NEGRO, new Vector2(0, 0), new Vector2(20, 120), 45f);
        Img(t, "x2", null, NEGRO, new Vector2(0, 0), new Vector2(20, 120), -45f);
    }

    // ---- Cruce / esquina: cruz (+) negra
    void Cruce()
    {
        var t = DiamanteBase();
        Img(t, "v", null, NEGRO, new Vector2(0, 0), new Vector2(22, 112));
        Img(t, "h", null, NEGRO, new Vector2(0, 0), new Vector2(112, 22));
    }

    // ---- Neblina: líneas horizontales cortadas por una vertical
    void Neblina()
    {
        var t = DiamanteBase();
        Img(t, "v",  null, NEGRO, new Vector2(0, 2),  new Vector2(10, 86));
        Img(t, "l1", null, NEGRO, new Vector2(0, 30), new Vector2(96, 10));
        Img(t, "l2", null, NEGRO, new Vector2(0, 2),  new Vector2(96, 10));
        Img(t, "l3", null, NEGRO, new Vector2(0, -26),new Vector2(96, 10));
    }

    // ---- Camino resbaladizo: auto + dos marcas de derrape
    void Resbaladizo()
    {
        var t = DiamanteBase();
        Auto(t, NEGRO, new Vector2(0, 20), 62f);
        Img(t, "d1", null, NEGRO, new Vector2(-24, -28), new Vector2(8, 34), 35f);
        Img(t, "d2", null, NEGRO, new Vector2(24, -28),  new Vector2(8, 34), -35f);
    }

    // ---- Escuela / niños: dos peatones
    void Escuela()
    {
        var t = DiamanteBase();
        Peaton(t, new Vector2(-16, -4), 0.95f, NEGRO);
        Peaton(t, new Vector2(18, -10), 0.75f, NEGRO);
    }

    // ---- Obras: trabajador + montículo
    void Obras()
    {
        var t = DiamanteBase();
        Peaton(t, new Vector2(-10, 4), 0.85f, NEGRO);
        Img(t, "Pala", null, NEGRO, new Vector2(20, 2), new Vector2(8, 48), -35f);
        Img(t, "Monticulo", TextureFactory.Semicirculo(), NEGRO, new Vector2(20, -34), new Vector2(64, 28));
    }

    // ---- Ambulancia: caja blanca + cruz roja
    void Ambulancia()
    {
        Poste(0f);
        var go = Img(transform, "Amb", null, Color.white, new Vector2(0, 30), new Vector2(160, 120));
        Img(go.transform, "cv", null, ROJO, new Vector2(0, 0), new Vector2(30, 84));
        Img(go.transform, "ch", null, ROJO, new Vector2(0, 0), new Vector2(84, 30));
    }

    // ---- Agente / policía: figura con brazo extendido sobre caja azul
    void Agente()
    {
        Poste(0f);
        var go = Img(transform, "Agente", null, AZUL, new Vector2(0, 30), new Vector2(150, 150));
        Img(go.transform, "Cabeza", TextureFactory.Circulo(), Color.white, new Vector2(0, 46), new Vector2(34, 34));
        Img(go.transform, "Cuerpo", null, Color.white, new Vector2(0, 2), new Vector2(28, 70));
        Img(go.transform, "Brazo", null, Color.white, new Vector2(8, 20), new Vector2(86, 14));
    }

    // ---- Noche: caja oscura + luna creciente + estrellas
    void Noche()
    {
        Poste(0f);
        var go = Img(transform, "Noche", null, new Color(0.11f,0.16f,0.23f), new Vector2(0, 30), new Vector2(150, 150));
        Img(go.transform, "LunaLlena", TextureFactory.Circulo(), AMA, new Vector2(-6, 4), new Vector2(82, 82));
        Img(go.transform, "Sombra",    TextureFactory.Circulo(), new Color(0.11f,0.16f,0.23f), new Vector2(16, 12), new Vector2(72, 72));
        Img(go.transform, "e1", TextureFactory.Circulo(), Color.white, new Vector2(40, 40), new Vector2(8, 8));
        Img(go.transform, "e2", TextureFactory.Circulo(), Color.white, new Vector2(48, -30), new Vector2(6, 6));
        Img(go.transform, "e3", TextureFactory.Circulo(), Color.white, new Vector2(-44, -40), new Vector2(7, 7));
    }

    void CajaSimple(Color color, string texto)
    {
        Poste(0f);
        var go = Img(transform, "Caja", null, color, new Vector2(0, 30), new Vector2(220, 120));
        Etiqueta(go.transform, texto, 30, Color.white);
    }

    // ============================================================================
    //  BASES REUTILIZABLES (devuelven el Transform donde poner el símbolo)
    // ============================================================================

    // Círculo blanco + aro rojo (prohibición). El símbolo va negro encima.
    Transform Prohibicion()
    {
        Poste(0f);
        var go = Img(transform, "Prohibicion", TextureFactory.Circulo(), Color.white, new Vector2(0, 30), new Vector2(150, 150));
        Img(go.transform, "Aro", TextureFactory.Anillo(128, 0.15f), ROJO, Vector2.zero, new Vector2(150, 150));
        return go.transform;
    }

    // Barra roja diagonal sobre una señal de prohibición
    void Slash(Transform t)
    {
        Img(t, "Slash", null, ROJO, Vector2.zero, new Vector2(134, 18), -45f);
    }

    // Círculo de color sólido (obligación). El símbolo va blanco encima.
    Transform Obligacion(Color fondo)
    {
        Poste(0f);
        var go = Img(transform, "Obligacion", TextureFactory.Circulo(), fondo, new Vector2(0, 30), new Vector2(150, 150));
        return go.transform;
    }

    // Diamante amarillo con borde negro (advertencia). Devuelve contenedor recto centrado.
    Transform DiamanteBase()
    {
        Poste(0f);
        Img(transform, "DiamBorde", null, NEGRO, new Vector2(0, 30), new Vector2(164, 164), 45f);
        Img(transform, "DiamFondo", null, AMA,   new Vector2(0, 30), new Vector2(140, 140), 45f);
        var simb = NuevoHijoEn(transform, "Simbolos", new Vector2(0, 30), new Vector2(118, 118));
        return simb.transform;
    }

    // ============================================================================
    //  PIEZAS (figuras reutilizables)
    // ============================================================================

    // Flecha apuntando hacia arriba por defecto. angulo en grados (Unity Z, antihorario).
    // 0 = arriba, 90 = izquierda, -90 = derecha, 180 = abajo.
    void Flecha(Transform parent, Color color, Vector2 pos, float escala, float angulo)
    {
        var cont = NuevoHijoEn(parent, "Flecha", pos, new Vector2(escala, escala));
        cont.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angulo);
        Img(cont.transform, "Astil", null, color, new Vector2(0, -escala * 0.08f), new Vector2(escala * 0.26f, escala * 0.62f));
        Img(cont.transform, "Punta", TextureFactory.Triangulo(), color, new Vector2(0, escala * 0.34f), new Vector2(escala * 0.6f, escala * 0.5f));
    }

    // Autito visto de frente: cuerpo + techo
    void Auto(Transform parent, Color color, Vector2 pos, float esc)
    {
        Img(parent, "AutoCuerpo", null, color, pos + new Vector2(0, -esc * 0.12f), new Vector2(esc, esc * 0.46f));
        Img(parent, "AutoTecho",  null, color, pos + new Vector2(0, esc * 0.2f),   new Vector2(esc * 0.62f, esc * 0.36f));
    }

    // Peatón: cabeza + cuerpo + dos piernas
    void Peaton(Transform parent, Vector2 pos, float s, Color color)
    {
        var cont = NuevoHijoEn(parent, "Peaton", pos, new Vector2(44 * s, 88 * s));
        Img(cont.transform, "Cabeza", TextureFactory.Circulo(), color, new Vector2(0, 32 * s), new Vector2(20 * s, 20 * s));
        Img(cont.transform, "Cuerpo", null, color, new Vector2(0, 4 * s), new Vector2(18 * s, 38 * s));
        Img(cont.transform, "PieI", null, color, new Vector2(-7 * s, -28 * s), new Vector2(9 * s, 30 * s), 12f);
        Img(cont.transform, "PieD", null, color, new Vector2(8 * s, -28 * s),  new Vector2(9 * s, 30 * s), -12f);
    }

    // ============================================================================
    //  HELPERS BÁSICOS
    // ============================================================================

    void Poste(float x)
    {
        Img(transform, "Poste", null, new Color(0.36f, 0.43f, 0.49f), new Vector2(x, -120), new Vector2(14, 200));
    }

    // Crea una Image hija con sprite opcional, color, posición, tamaño y rotación.
    Image Img(Transform parent, string nombre, Sprite sprite, Color color, Vector2 pos, Vector2 size, float angulo = 0f)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        if (angulo != 0f) rt.localEulerAngles = new Vector3(0, 0, angulo);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        return img;
    }

    GameObject Rect(string nombre, Color color, Vector2 pos, Vector2 size)
    {
        return Img(transform, nombre, null, color, pos, size).gameObject;
    }

    GameObject NuevoHijoEn(Transform parent, string nombre, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    Text Etiqueta(Transform parent, string texto, int tam, Color color)
    {
        var go = new GameObject("Texto");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        t.text = texto;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = tam;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
