using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  SIGN DRAWER — dibuja la señal de tránsito de cada pregunta
//  ----------------------------------------------------------------------------
//  Representa cada señal con un COLOR de fondo + una etiqueta de texto corta.
//  Usa texto simple (no emojis) para que se vea bien con la fuente legacy de
//  Unity. El color comunica el tipo de señal (rojo=prohibición/peligro,
//  amarillo=precaución, azul=información, verde=permitido).
//  (Más adelante se puede reemplazar por sprites reales sin tocar el resto.)
// ============================================================================
public class SignDrawer : MonoBehaviour
{
    [SerializeField] private Image fondo;        // panel de color detrás de la señal
    [SerializeField] private Text simbolo;       // etiqueta de texto de la señal

    public void Dibujar(string tipo)
    {
        Color color;
        string s;

        switch (tipo)
        {
            case "semaforo_rojo":        color = HexC("#C0392B"); s = "SEMAFORO\nROJO"; break;
            case "semaforo_amarillo":    color = HexC("#F39C12"); s = "SEMAFORO\nAMARILLO"; break;
            case "semaforo_verde":       color = HexC("#27AE60"); s = "SEMAFORO\nVERDE"; break;
            case "pare":                 color = HexC("#C0392B"); s = "PARE"; break;
            case "ceda":                 color = HexC("#C0392B"); s = "CEDA\nEL PASO"; break;
            case "lomo":                 color = HexC("#E67E22"); s = "LOMO DE\nBURRO"; break;
            case "senda":                color = HexC("#2980B9"); s = "SENDA\nPEATONAL"; break;
            case "escuela":              color = HexC("#E67E22"); s = "ZONA\nESCOLAR"; break;
            case "limite40":             color = HexC("#C0392B"); s = "MAX\n40"; break;
            case "curva":                color = HexC("#E67E22"); s = "CURVA\nPELIGROSA"; break;
            case "resbaladizo":          color = HexC("#2980B9"); s = "CALZADA\nMOJADA"; break;
            case "ambulancia":           color = HexC("#C0392B"); s = "AMBULANCIA"; break;
            case "tren":                 color = HexC("#E67E22"); s = "PASO A\nNIVEL"; break;
            case "ciclista":             color = HexC("#2980B9"); s = "CICLISTA"; break;
            case "no_izquierda":         color = HexC("#C0392B"); s = "NO GIRAR\nIZQUIERDA"; break;
            case "contramano":           color = HexC("#C0392B"); s = "CONTRA\nMANO"; break;
            case "no_adelantar":         color = HexC("#C0392B"); s = "NO\nADELANTAR"; break;
            case "obligatoria_derecha":  color = HexC("#2980B9"); s = "GIRO\nDERECHA"; break;
            case "noche":                color = HexC("#2C3E50"); s = "DE\nNOCHE"; break;
            case "neblina":              color = HexC("#7F8C8D"); s = "NEBLINA"; break;
            case "distancia":            color = HexC("#2980B9"); s = "DISTANCIA"; break;
            case "estacionar":           color = HexC("#2980B9"); s = "ESTACIONAR"; break;
            case "peligro":              color = HexC("#E67E22"); s = "PELIGRO"; break;
            case "agente":               color = HexC("#2980B9"); s = "AGENTE DE\nTRANSITO"; break;
            case "cinturon":             color = HexC("#27AE60"); s = "CINTURON"; break;
            case "telefono":             color = HexC("#C0392B"); s = "CELULAR"; break;
            case "alcohol":              color = HexC("#C0392B"); s = "ALCOHOL"; break;
            case "rotonda":              color = HexC("#2980B9"); s = "ROTONDA"; break;
            case "esquina":              color = HexC("#F39C12"); s = "ESQUINA"; break;
            case "obras":                color = HexC("#E67E22"); s = "OBRAS EN\nLA VIA"; break;
            default:                     color = HexC("#34495E"); s = "?"; break;
        }

        if (fondo != null) fondo.color = color;
        if (simbolo != null) simbolo.text = s;
    }

    private Color HexC(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
