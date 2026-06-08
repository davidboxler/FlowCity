using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  GAME MANAGER — FlowCity (examen de manejo por reacción)
//  ----------------------------------------------------------------------------
//  Flujo del examen:
//    1) Toma 15 preguntas al azar de las 50.
//    2) Muestra la señal + pregunta + 2 opciones.
//    3) El jugador tiene N segundos para elegir (mide el tiempo de reacción).
//    4) Si acierta suma; si falla o se acaba el tiempo, no.
//    5) Al terminar las 15, muestra resultado + reacción promedio.
//
//  El "auto en primera persona" es visual: una velocidad que sube/baja según
//  las respuestas, mostrada en el HUD. (Por ahora simple, como pidió David.)
// ============================================================================
public class GameManager : MonoBehaviour
{
    [Header("Configuración del examen")]
    [SerializeField] public int cantidadPreguntas = 15;     // cuántas preguntas trae el examen
    [SerializeField] public float segundosPorPregunta = 3f; // tiempo para responder cada una

    [Header("Referencias de UI (las conecta el constructor de escena)")]
    [SerializeField] private Text textoPregunta;
    [SerializeField] private Text textoOpcionA;
    [SerializeField] private Text textoOpcionB;
    [SerializeField] private Button botonA;
    [SerializeField] private Button botonB;
    [SerializeField] private Image barraTiempo;     // se vacía mientras corre el tiempo
    [SerializeField] private Text textoProgreso;    // "Pregunta 3 / 15"
    [SerializeField] private SignDrawer signDrawer;

    [Header("HUD de velocidad (primera persona)")]
    [SerializeField] private Text textoVelocidad;
    [SerializeField] private Image velocidadFill;   // barra/aguja de velocidad

    [Header("Pantallas")]
    [SerializeField] private GameObject panelJuego;
    [SerializeField] private GameObject panelFinal;
    [SerializeField] private Text textoResultado;
    [SerializeField] private Button botonReiniciar;

    [Header("Feedback visual")]
    [SerializeField] private Image flashColor;       // verde/rojo al responder

    // --- Estado interno ---
    private List<Pregunta> examen = new List<Pregunta>();
    private int indiceActual = 0;
    private int aciertos = 0;
    private float tiempoRestante;
    private bool esperandoRespuesta = false;
    private float tiempoInicioPregunta;
    private List<float> tiemposReaccion = new List<float>();

    // --- Velocidad simulada (HUD primera persona) ---
    private float velocidad = 40f;
    private const float VEL_MAX = 120f;
    private const float VEL_MIN = 0f;

    void Start()
    {
        // Conectar los botones por código (a prueba de desconexiones).
        if (botonA != null) botonA.onClick.AddListener(() => Responder(0));
        if (botonB != null) botonB.onClick.AddListener(() => Responder(1));
        if (botonReiniciar != null) botonReiniciar.onClick.AddListener(IniciarExamen);

        IniciarExamen();
    }

    public void IniciarExamen()
    {
        // Elegir N preguntas al azar de todas.
        List<Pregunta> todas = PreguntasDB.ObtenerTodas();
        Barajar(todas);
        examen.Clear();
        int n = Mathf.Min(cantidadPreguntas, todas.Count);
        for (int i = 0; i < n; i++) examen.Add(todas[i]);

        indiceActual = 0;
        aciertos = 0;
        velocidad = 40f;
        tiemposReaccion.Clear();

        if (panelFinal != null) panelFinal.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);

        MostrarPregunta();
    }

    void MostrarPregunta()
    {
        if (indiceActual >= examen.Count)
        {
            TerminarExamen();
            return;
        }

        Pregunta p = examen[indiceActual];

        if (textoPregunta != null) textoPregunta.text = p.enunciado;
        if (textoOpcionA != null) textoOpcionA.text = p.opcionA;
        if (textoOpcionB != null) textoOpcionB.text = p.opcionB;
        if (textoProgreso != null) textoProgreso.text = "Pregunta " + (indiceActual + 1) + " / " + examen.Count;
        if (signDrawer != null) signDrawer.Dibujar(p.iconoTipo);

        tiempoRestante = segundosPorPregunta;
        tiempoInicioPregunta = Time.time;
        esperandoRespuesta = true;
        SetBotones(true);
    }

    void Update()
    {
        // Suavizar la aguja de velocidad hacia su valor objetivo.
        ActualizarHUDVelocidad();

        if (!esperandoRespuesta) return;

        tiempoRestante -= Time.deltaTime;
        if (barraTiempo != null)
            barraTiempo.fillAmount = Mathf.Clamp01(tiempoRestante / segundosPorPregunta);

        if (tiempoRestante <= 0f)
        {
            // Se acabó el tiempo => cuenta como error (no reaccionó a tiempo).
            esperandoRespuesta = false;
            SetBotones(false);
            velocidad = Mathf.Max(VEL_MIN, velocidad - 15f); // pierde velocidad por no reaccionar
            Flash(false);
            tiemposReaccion.Add(segundosPorPregunta); // reacción = tiempo máximo
            Invoke(nameof(Siguiente), 0.6f);
        }
    }

    void Responder(int opcion)
    {
        if (!esperandoRespuesta) return;

        esperandoRespuesta = false;
        SetBotones(false);

        float reaccion = Time.time - tiempoInicioPregunta;
        tiemposReaccion.Add(reaccion);

        Pregunta p = examen[indiceActual];
        bool correcto = (opcion == p.correcta);

        if (correcto)
        {
            aciertos++;
            velocidad = Mathf.Min(VEL_MAX, velocidad + 10f); // acelera al acertar
            Flash(true);
        }
        else
        {
            velocidad = Mathf.Max(VEL_MIN, velocidad - 20f); // frena al fallar
            Flash(false);
        }

        Invoke(nameof(Siguiente), 0.6f);
    }

    void Siguiente()
    {
        indiceActual++;
        MostrarPregunta();
    }

    void TerminarExamen()
    {
        esperandoRespuesta = false;
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelFinal != null) panelFinal.SetActive(true);

        float promedio = 0f;
        foreach (float t in tiemposReaccion) promedio += t;
        if (tiemposReaccion.Count > 0) promedio /= tiemposReaccion.Count;

        bool aprobado = aciertos >= Mathf.CeilToInt(examen.Count * 0.6f); // 60% para aprobar

        if (textoResultado != null)
        {
            string estado = aprobado ? "APROBADO ✅" : "DESAPROBADO ❌";
            textoResultado.text =
                estado + "\n\n" +
                "Respuestas correctas: " + aciertos + " / " + examen.Count + "\n" +
                "Tiempo de reacción promedio: " + promedio.ToString("F2") + " s";
        }
    }

    // ---------- Helpers ----------

    void SetBotones(bool activos)
    {
        if (botonA != null) botonA.interactable = activos;
        if (botonB != null) botonB.interactable = activos;
    }

    void ActualizarHUDVelocidad()
    {
        if (textoVelocidad != null)
            textoVelocidad.text = Mathf.RoundToInt(velocidad) + " km/h";
        if (velocidadFill != null)
            velocidadFill.fillAmount = Mathf.Lerp(velocidadFill.fillAmount, velocidad / VEL_MAX, Time.deltaTime * 5f);
    }

    void Flash(bool acierto)
    {
        if (flashColor == null) return;
        Color c = acierto ? new Color(0.15f, 0.8f, 0.3f, 0.45f) : new Color(0.9f, 0.2f, 0.2f, 0.45f);
        flashColor.color = c;
        CancelInvoke(nameof(ApagarFlash));
        Invoke(nameof(ApagarFlash), 0.4f);
    }

    void ApagarFlash()
    {
        if (flashColor != null) flashColor.color = new Color(0, 0, 0, 0);
    }

    void Barajar(List<Pregunta> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Pregunta tmp = lista[i];
            lista[i] = lista[j];
            lista[j] = tmp;
        }
    }
}
