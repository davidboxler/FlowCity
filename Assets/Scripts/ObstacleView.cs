using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  OBSTACLE VIEW — dibuja el obstáculo/senal detallado frente al auto
//  ----------------------------------------------------------------------------
//  Reconstruye un obstáculo realista (no un simple recuadro) segun el tipo:
//   - Semáforo: caja negra con poste lateral y 3 luces (la encendida segun color)
//   - Señales reglamentarias: círculo rojo/azul con símbolo
//   - Señales de advertencia: diamante amarillo con símbolo
//   - PARE: octágono rojo. CEDA: triángulo invertido.
//   - Senda peatonal: franjas blancas horizontales sobre la calle.
//
//  Todo se arma con UI Images + sprites generados por TextureFactory.
//  El contenedor se limpia y se vuelve a armar en cada pregunta.
// ============================================================================
public class ObstacleView : MonoBehaviour
{
    private RectTransform area;     // zona frente al auto donde aparece el obstáculo

    void Awake()
    {
        area = GetComponent<RectTransform>();
    }

    public void Mostrar(string tipo)
    {
        Limpiar();

        switch (tipo)
        {
            case "semaforo_rojo":      Semaforo(0); break;
            case "semaforo_amarillo":  Semaforo(1); break;
            case "semaforo_verde":     Semaforo(2); break;

            case "pare":               Octagono("PARE"); break;
            case "ceda":               TrianguloInvertido(); break;

            case "senda":              SendaPeatonal(); break;

            // Advertencia (diamante amarillo)
            case "lomo":               Diamante("LOMO"); break;
            case "curva":              Diamante("CURVA"); break;
            case "resbaladizo":        Diamante("MOJADO"); break;
            case "tren":               Diamante("TREN"); break;
            case "escuela":            Diamante("ESCUELA"); break;
            case "peligro":            Diamante("!"); break;
            case "obras":              Diamante("OBRAS"); break;
            case "esquina":            Diamante("CRUCE"); break;
            case "neblina":            Diamante("NIEBLA"); break;

            // Reglamentarias prohibición (círculo rojo)
            case "limite40":           CirculoSenal(Hex("#C0392B"), "40"); break;
            case "no_izquierda":       CirculoSenal(Hex("#C0392B"), "NO <"); break;
            case "contramano":         CirculoSenal(Hex("#C0392B"), "NO"); break;
            case "no_adelantar":       CirculoSenal(Hex("#C0392B"), "NO"); break;
            case "telefono":           CirculoSenal(Hex("#C0392B"), "CEL"); break;
            case "alcohol":            CirculoSenal(Hex("#C0392B"), "OH"); break;

            // Reglamentarias info (círculo azul)
            case "obligatoria_derecha":CirculoSenal(Hex("#2471A3"), ">"); break;
            case "rotonda":            CirculoSenal(Hex("#2471A3"), "( )"); break;
            case "estacionar":         CirculoSenal(Hex("#2471A3"), "P"); break;
            case "distancia":          CirculoSenal(Hex("#2471A3"), "| |"); break;
            case "ciclista":           CirculoSenal(Hex("#2471A3"), "BICI"); break;
            case "cinturon":           CirculoSenal(Hex("#27AE60"), "OK"); break;

            // Otros
            case "ambulancia":         CajaSimple(Hex("#C0392B"), "AMBULANCIA"); break;
            case "agente":             CajaSimple(Hex("#2471A3"), "AGENTE"); break;
            case "noche":              CajaSimple(Hex("#2C3E50"), "NOCHE"); break;

            default:                   CajaSimple(Hex("#566573"), "?"); break;
        }
    }

    void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    // ===================== Obstáculos =====================

    // Semáforo: poste a un costado + caja negra con 3 luces. luzOn: 0=rojo,1=amarillo,2=verde
    void Semaforo(int luzOn)
    {
        bool ladoIzq = Random.value < 0.5f;
        float xCaja = ladoIzq ? -90f : 90f;

        // Poste (va desde abajo hasta la caja)
        var poste = Rect("Poste", Hex("#5D6D7E"), new Vector2(xCaja, -120), new Vector2(14, 240));

        // Caja negra
        var caja = Rect("CajaSemaforo", Hex("#1B2631"), new Vector2(xCaja, 40), new Vector2(70, 180));

        // 3 luces
        Color rojo    = (luzOn == 0) ? Hex("#E74C3C") : Hex("#5B2C2C");
        Color amarillo= (luzOn == 1) ? Hex("#F1C40F") : Hex("#5B541F");
        Color verde   = (luzOn == 2) ? Hex("#2ECC71") : Hex("#1E4D33");
        Luz(caja.transform, rojo,     new Vector2(0, 55));
        Luz(caja.transform, amarillo, new Vector2(0, 0));
        Luz(caja.transform, verde,    new Vector2(0, -55));
    }

    void Luz(Transform parent, Color color, Vector2 pos)
    {
        var go = new GameObject("Luz");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(48, 48);
        var img = go.AddComponent<Image>();
        img.sprite = TextureFactory.Circulo();
        img.color = color;
    }

    // Octágono (PARE) — aproximado con círculo rojo + borde + texto
    void Octagono(string texto)
    {
        Poste(0f);
        var go = NuevoHijo("Pare", new Vector2(0, 30), new Vector2(150, 150));
        var img = go.AddComponent<Image>();
        img.sprite = TextureFactory.Circulo();
        img.color = Hex("#C0392B");
        Etiqueta(go.transform, texto, 38, Color.white);
    }

    void TrianguloInvertido()
    {
        Poste(0f);
        var go = NuevoHijo("Ceda", new Vector2(0, 30), new Vector2(160, 160));
        var img = go.AddComponent<Image>();
        img.sprite = TextureFactory.Triangulo();
        img.color = Hex("#C0392B");
        // girar 180 para que apunte hacia abajo
        go.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 180);
        var lbl = Etiqueta(go.transform, "CEDA", 26, Color.white);
        lbl.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 180);
    }

    void Diamante(string texto)
    {
        Poste(0f);
        var go = NuevoHijo("Diamante", new Vector2(0, 30), new Vector2(150, 150));
        var img = go.AddComponent<Image>();
        img.color = Hex("#F1C40F");
        go.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 45); // cuadrado rotado = diamante
        var lbl = Etiqueta(go.transform, texto, 30, Hex("#1B2631"));
        lbl.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, -45);
    }

    void CirculoSenal(Color color, string texto)
    {
        Poste(0f);
        var go = NuevoHijo("CirculoSenal", new Vector2(0, 30), new Vector2(150, 150));
        var img = go.AddComponent<Image>();
        img.sprite = TextureFactory.Circulo();
        img.color = color;
        Etiqueta(go.transform, texto, 40, Color.white);
    }

    void CajaSimple(Color color, string texto)
    {
        Poste(0f);
        var go = NuevoHijo("Caja", new Vector2(0, 30), new Vector2(220, 120));
        var img = go.AddComponent<Image>();
        img.color = color;
        Etiqueta(go.transform, texto, 30, Color.white);
    }

    // Senda peatonal: franjas blancas horizontales sobre la calle (abajo, ancho)
    void SendaPeatonal()
    {
        for (int i = 0; i < 6; i++)
        {
            float x = -125 + i * 50;
            Rect("Franja", Color.white, new Vector2(x, -120), new Vector2(30, 90));
        }
        var lbl = Etiqueta(transform, "SENDA PEATONAL", 26, Color.white);
        lbl.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
        lbl.gameObject.AddComponent<Outline>().effectColor = Color.black;
    }

    // ===================== helpers =====================

    void Poste(float x)
    {
        Rect("Poste", Hex("#5D6D7E"), new Vector2(x, -120), new Vector2(14, 200));
    }

    GameObject Rect(string nombre, Color color, Vector2 pos, Vector2 size)
    {
        var go = NuevoHijo(nombre, pos, size);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    GameObject NuevoHijo(string nombre, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
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

    Color Hex(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }
}
