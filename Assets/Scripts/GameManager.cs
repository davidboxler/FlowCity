using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  GAME MANAGER — FlowCity (examen de manejo por reacción)
//  ----------------------------------------------------------------------------
//  Flujo: toma 15 preguntas al azar de 50, muestra el obstáculo + pregunta +
//  2 respuestas (que CAMBIAN de lado izq/der), da N segundos, mide reacción,
//  feedback verde/rojo, y al final aprobado/desaprobado + reacción promedio.
//
//  La "velocidad" sube lento y no pasa de 60 (estética de juego de carreras).
// ============================================================================
public class GameManager : MonoBehaviour
{
    [Header("Configuración del examen")]
    [SerializeField] public int cantidadPreguntas = 15;
    [SerializeField] public float segundosPorPregunta = 3f;

    [Header("UI de pregunta")]
    [SerializeField] private Text textoPregunta;
    [SerializeField] private Text textoOpcionIzq;
    [SerializeField] private Text textoOpcionDer;
    [SerializeField] private Button botonIzq;
    [SerializeField] private Button botonDer;
    [SerializeField] private Image barraTiempo;
    [SerializeField] private Text textoProgreso;
    [SerializeField] private ObstacleView obstaculo;

    [Header("HUD de velocidad")]
    [SerializeField] private Text textoVelocidad;
    [SerializeField] private Image velocidadFill;

    [Header("Pantallas")]
    [SerializeField] private GameObject panelJuego;
    [SerializeField] private GameObject panelFinal;
    [SerializeField] private Text textoResultado;
    [SerializeField] private Button botonReiniciar;

    [Header("Feedback")]
    [SerializeField] private Image flashColor;

    // Velocidad estética
    private const float VEL_MAX = 60f;
    private float velocidad = 20f;

    // Estado
    private List<Pregunta> examen = new List<Pregunta>();
    private int indiceActual = 0;
    private int aciertos = 0;
    private float tiempoRestante;
    private bool esperandoRespuesta = false;
    private float tiempoInicioPregunta;
    private List<float> tiemposReaccion = new List<float>();

    // ¿En qué lado está la opción A de la pregunta? (para saber qué botón es correcto)
    private bool opcionAenIzquierda = true;

    void Start()
    {
        if (botonIzq != null) botonIzq.onClick.AddListener(() => ResponderLado(true));
        if (botonDer != null) botonDer.onClick.AddListener(() => ResponderLado(false));
        if (botonReiniciar != null) botonReiniciar.onClick.AddListener(IniciarExamen);
        IniciarExamen();
    }

    public void IniciarExamen()
    {
        List<Pregunta> todas = PreguntasDB.ObtenerTodas();
        Barajar(todas);
        examen.Clear();
        int n = Mathf.Min(cantidadPreguntas, todas.Count);
        for (int i = 0; i < n; i++) examen.Add(todas[i]);

        indiceActual = 0;
        aciertos = 0;
        velocidad = 20f;
        tiemposReaccion.Clear();

        if (panelFinal != null) panelFinal.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);

        MostrarPregunta();
    }

    void MostrarPregunta()
    {
        if (indiceActual >= examen.Count) { TerminarExamen(); return; }

        Pregunta p = examen[indiceActual];

        // Elegir al azar de qué lado va la opción A (así la correcta varía de lado).
        opcionAenIzquierda = Random.value < 0.5f;
        if (opcionAenIzquierda)
        {
            if (textoOpcionIzq != null) textoOpcionIzq.text = p.opcionA;
            if (textoOpcionDer != null) textoOpcionDer.text = p.opcionB;
        }
        else
        {
            if (textoOpcionIzq != null) textoOpcionIzq.text = p.opcionB;
            if (textoOpcionDer != null) textoOpcionDer.text = p.opcionA;
        }

        if (textoPregunta != null) textoPregunta.text = p.enunciado;
        if (textoProgreso != null) textoProgreso.text = "Pregunta " + (indiceActual + 1) + " / " + examen.Count;
        if (obstaculo != null) obstaculo.Mostrar(p.iconoTipo);

        tiempoRestante = segundosPorPregunta;
        tiempoInicioPregunta = Time.time;
        esperandoRespuesta = true;
        SetBotones(true);
    }

    void Update()
    {
        ActualizarHUDVelocidad();
        if (!esperandoRespuesta) return;

        tiempoRestante -= Time.deltaTime;
        if (barraTiempo != null)
            barraTiempo.fillAmount = Mathf.Clamp01(tiempoRestante / segundosPorPregunta);

        if (tiempoRestante <= 0f)
        {
            esperandoRespuesta = false;
            SetBotones(false);
            tiemposReaccion.Add(segundosPorPregunta);
            Flash(false);
            Invoke(nameof(Siguiente), 0.7f);
        }
    }

    // El jugador tocó un lado. Traducimos lado -> opción (A o B) -> correcto o no.
    void ResponderLado(bool tocoIzquierda)
    {
        if (!esperandoRespuesta) return;
        esperandoRespuesta = false;
        SetBotones(false);

        float reaccion = Time.time - tiempoInicioPregunta;
        tiemposReaccion.Add(reaccion);

        // ¿Qué opción (0=A,1=B) representa el lado tocado?
        int opcionElegida;
        if (tocoIzquierda) opcionElegida = opcionAenIzquierda ? 0 : 1;
        else               opcionElegida = opcionAenIzquierda ? 1 : 0;

        bool correcto = (opcionElegida == examen[indiceActual].correcta);

        if (correcto)
        {
            aciertos++;
            velocidad = Mathf.Min(VEL_MAX, velocidad + 6f); // sube lento
            Flash(true);
        }
        else
        {
            velocidad = Mathf.Max(0f, velocidad - 10f);
            Flash(false);
        }

        Invoke(nameof(Siguiente), 0.7f);
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

        bool aprobado = aciertos >= Mathf.CeilToInt(examen.Count * 0.6f);

        if (textoResultado != null)
        {
            string estado = aprobado ? "APROBADO" : "DESAPROBADO";
            textoResultado.text =
                estado + "\n\n" +
                "Respuestas correctas: " + aciertos + " / " + examen.Count + "\n" +
                "Tiempo de reaccion promedio: " + promedio.ToString("F2") + " s";
        }
    }

    // ---------- Helpers ----------
    void SetBotones(bool activos)
    {
        if (botonIzq != null) botonIzq.interactable = activos;
        if (botonDer != null) botonDer.interactable = activos;
    }

    void ActualizarHUDVelocidad()
    {
        // sube suavemente hacia su valor (efecto velocímetro)
        if (textoVelocidad != null)
            textoVelocidad.text = Mathf.RoundToInt(velocidad) + " km/h";
        if (velocidadFill != null)
            velocidadFill.fillAmount = Mathf.Lerp(velocidadFill.fillAmount, velocidad / VEL_MAX, Time.deltaTime * 4f);
    }

    void Flash(bool acierto)
    {
        if (flashColor == null) return;
        flashColor.color = acierto ? new Color(0.15f, 0.8f, 0.3f, 0.5f)
                                   : new Color(0.9f, 0.2f, 0.2f, 0.5f);
        CancelInvoke(nameof(ApagarFlash));
        Invoke(nameof(ApagarFlash), 0.5f);
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
            var tmp = lista[i]; lista[i] = lista[j]; lista[j] = tmp;
        }
    }
}
