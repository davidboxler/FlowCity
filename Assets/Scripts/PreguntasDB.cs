using System.Collections.Generic;

// ============================================================================
//  BANCO DE PREGUNTAS — FlowCity (examen de manejo)
//  ----------------------------------------------------------------------------
//  Cada pregunta es una señal/situación de tránsito con DOS opciones.
//  El jugador debe reaccionar rápido eligiendo la correcta.
//
//  "iconoTipo" identifica qué dibujar en pantalla (el SignDrawer lo usa para
//  pintar la señal sin necesidad de imágenes externas).
// ============================================================================

[System.Serializable]
public class Pregunta
{
    public string enunciado;     // texto/pregunta que se muestra
    public string opcionA;       // texto del botón izquierdo
    public string opcionB;       // texto del botón derecho
    public int correcta;         // 0 = opcionA correcta, 1 = opcionB correcta
    public string iconoTipo;     // qué señal dibujar (ver SignDrawer)

    public Pregunta(string enunciado, string opcionA, string opcionB, int correcta, string iconoTipo)
    {
        this.enunciado = enunciado;
        this.opcionA = opcionA;
        this.opcionB = opcionB;
        this.correcta = correcta;
        this.iconoTipo = iconoTipo;
    }
}

public static class PreguntasDB
{
    // Las 50 preguntas del examen. El examen toma 15 al azar de acá.
    public static List<Pregunta> ObtenerTodas()
    {
        return new List<Pregunta>
        {
            // --- Semáforos ---
            new Pregunta("Semáforo en AMARILLO", "Avanzar", "Detenerse", 1, "semaforo_amarillo"),
            new Pregunta("Semáforo en ROJO", "Detenerse", "Avanzar", 0, "semaforo_rojo"),
            new Pregunta("Semáforo en VERDE", "Avanzar", "Detenerse", 0, "semaforo_verde"),
            new Pregunta("Semáforo en rojo y querés doblar", "Frenar y esperar", "Doblar igual", 0, "semaforo_rojo"),

            // --- Señales de PARE / ceder ---
            new Pregunta("Cartel de PARE", "Detenerse por completo", "Reducir y seguir", 0, "pare"),
            new Pregunta("Cartel CEDA EL PASO", "Ceder el paso", "Acelerar para pasar", 0, "ceda"),
            new Pregunta("Llegás a una esquina sin señales", "Ceder a quien viene por derecha", "Pasar primero siempre", 0, "esquina"),

            // --- Velocidad / obstáculos ---
            new Pregunta("Lomo de burro adelante", "Reducir la velocidad", "Mantener la velocidad", 0, "lomo"),
            new Pregunta("Senda peatonal con gente cruzando", "Ceder el paso al peatón", "Avanzar rápido", 0, "senda"),
            new Pregunta("Zona de escuela", "Reducir la velocidad", "Acelerar", 0, "escuela"),
            new Pregunta("Cartel de velocidad máxima 40", "Respetar el límite", "Ir más rápido", 0, "limite40"),
            new Pregunta("Curva peligrosa adelante", "Reducir la velocidad", "Acelerar", 0, "curva"),
            new Pregunta("Camino resbaladizo por lluvia", "Reducir y aumentar distancia", "Frenar de golpe", 0, "resbaladizo"),
            new Pregunta("Calle con badén o pozo", "Reducir la velocidad", "Pasar rápido", 0, "lomo"),

            // --- Prioridades y peatones ---
            new Pregunta("Un peatón espera en la esquina", "Cederle el paso", "Tocar bocina y seguir", 0, "senda"),
            new Pregunta("Una ambulancia con sirena se acerca", "Ceder el paso y orillarse", "Seguir normal", 0, "ambulancia"),
            new Pregunta("Cruce de tren con barrera baja", "Detenerse y esperar", "Cruzar rápido", 0, "tren"),
            new Pregunta("Ves un ciclista adelante", "Adelantar con distancia segura", "Pasar muy cerca", 0, "ciclista"),

            // --- Señales reglamentarias ---
            new Pregunta("Señal PROHIBIDO GIRAR a la izquierda", "Seguir derecho", "Girar a la izquierda", 0, "no_izquierda"),
            new Pregunta("Señal de CONTRAMANO", "No ingresar", "Ingresar igual", 0, "contramano"),
            new Pregunta("Señal PROHIBIDO ADELANTAR", "No adelantar", "Adelantar igual", 0, "no_adelantar"),
            new Pregunta("Señal de DIRECCIÓN OBLIGATORIA a la derecha", "Girar a la derecha", "Seguir derecho", 0, "obligatoria_derecha"),

            // --- Luces y condiciones ---
            new Pregunta("Está oscureciendo", "Encender las luces", "Manejar sin luces", 0, "noche"),
            new Pregunta("Hay neblina espesa", "Reducir y usar luces bajas", "Acelerar para salir rápido", 0, "neblina"),
            new Pregunta("Vas a doblar en la esquina", "Poner el guiño antes", "No avisar", 0, "esquina"),

            // --- Distancia y maniobras ---
            new Pregunta("El auto de adelante frena", "Mantener distancia de seguridad", "Pegarse más", 0, "distancia"),
            new Pregunta("Querés estacionar", "Señalizar y reducir", "Frenar de golpe", 0, "estacionar"),
            new Pregunta("Una pelota cruza la calle", "Frenar (puede venir un niño)", "Seguir igual", 0, "peligro"),
            new Pregunta("Animal cruzando la ruta", "Reducir la velocidad", "Acelerar", 0, "peligro"),

            // --- Más semáforos / situaciones ---
            new Pregunta("Semáforo amarillo y ya estás cruzando", "Completar el cruce", "Frenar en el medio", 0, "semaforo_amarillo"),
            new Pregunta("Semáforo intermitente amarillo", "Avanzar con precaución", "Detenerse siempre", 0, "semaforo_amarillo"),
            new Pregunta("Agente de tránsito te indica parar", "Detenerse", "Ignorarlo", 0, "agente"),

            // --- Reglas generales ---
            new Pregunta("Vas a arrancar el auto", "Ponerte el cinturón", "Arrancar sin cinturón", 0, "cinturon"),
            new Pregunta("Suena tu teléfono manejando", "No atender / usar manos libres", "Atender con la mano", 0, "telefono"),
            new Pregunta("Querés tomar una bebida alcohólica y manejar", "No manejar", "Manejar igual", 0, "alcohol"),
            new Pregunta("Entrás a una rotonda", "Ceder a los que ya circulan", "Entrar sin ceder", 0, "rotonda"),
            new Pregunta("Cartel de SOLO PEATONES", "No ingresar con el auto", "Ingresar", 0, "senda"),

            // --- Velocidad en contexto ---
            new Pregunta("Avenida amplia y despejada", "Respetar el máximo permitido", "Ir a toda velocidad", 0, "limite40"),
            new Pregunta("Calle angosta de barrio", "Circular despacio", "Circular rápido", 0, "escuela"),
            new Pregunta("Bajada pronunciada", "Usar freno motor / reducir", "Acelerar", 0, "curva"),
            new Pregunta("Hay un colectivo detenido en la parada", "Reducir, pueden bajar pasajeros", "Pasar rápido al lado", 0, "senda"),

            // --- Cruces y prioridad ---
            new Pregunta("Cruce con prioridad para el de la derecha", "Cederle si viene por derecha", "Pasar primero", 0, "esquina"),
            new Pregunta("Querés adelantar en doble línea amarilla", "No adelantar", "Adelantar", 0, "no_adelantar"),
            new Pregunta("Semáforo en rojo a punto de cambiar", "Esperar el verde", "Arrancar antes", 0, "semaforo_rojo"),
            new Pregunta("Charco grande en la calle", "Reducir la velocidad", "Pasar a toda velocidad", 0, "resbaladizo"),

            // --- Situaciones de cierre ---
            new Pregunta("Niños jugando cerca de la calle", "Extremar precaución y reducir", "Seguir igual", 0, "escuela"),
            new Pregunta("Llegás a un paso a nivel sin barrera", "Mirar y cruzar con precaución", "Cruzar sin mirar", 0, "tren"),
            new Pregunta("La luz del semáforo está apagada", "Cruzar como esquina sin señales", "Pasar a toda velocidad", 0, "esquina"),
            new Pregunta("Hay obras en la calle", "Reducir y seguir señalización", "Acelerar", 0, "obras"),
            new Pregunta("Cartel de DESPACIO", "Reducir la velocidad", "Mantener velocidad", 0, "limite40"),
        };
    }
}
