# FlowCity — Juego del examen de manejo (versión simple)

Hola David! Dejé el juego armado y listo. Cuando vuelvas de cocinar, son **2 clicks** para verlo andando.

## Cómo abrirlo y jugar (pasos)

1. **Unity Hub** → **Add** → **Add project from disk** → elegí la carpeta:
   `C:\Users\david\OneDrive\Escritorio\FlowCity`
   (Usa la misma versión de Unity que el juego de trompos: 6000.3.14f1)

2. Abrí el proyecto. La primera vez tarda un poco (importa todo).

3. En la barra de menú de arriba va a aparecer un menú nuevo: **FlowCity**.
   Hacé clic en **FlowCity → Construir Escena**.
   - Esto arma TODA la escena sola (cámara, ruta, HUD, preguntas, botones) y la guarda en `Assets/Scenes/FlowCity.unity`.
   - Va a aparecer un cartelito "¡Escena construida!". Dale "Genial".

4. Apretá **PLAY** (el triángulo de arriba). ¡A jugar!

## Cómo se juega

- Aparece una **señal de tránsito** (semáforo, PARE, lomo de burro, senda peatonal, etc.) con una **pregunta** y **2 opciones**.
- Tenés **3 segundos** para elegir la correcta (clic en el botón).
- Acertar **sube tu velocidad**; fallar o tardar demasiado la **baja**.
- Son **15 preguntas** al azar (de un total de 50).
- Al final: pantalla con **aprobado/desaprobado** + tu **tiempo de reacción promedio**.

## Lo que se puede ajustar fácil (sin tocar código)

En la escena, seleccioná el objeto **GameManager** y en el Inspector vas a ver:
- **Cantidad Preguntas**: 15 (podés cambiarlo).
- **Segundos Por Pregunta**: 3 (subilo/bajalo para más o menos tiempo).

## Qué está hecho y qué falta

✅ Hecho:
- Auto en primera persona (ruta + capó), HUD de velocidad.
- 50 preguntas de tránsito, examen de 15 al azar.
- Timer de 3s por pregunta + medición de reacción.
- Pantalla de resultado (aprobado/desaprobado + reacción promedio).
- Saqué mapa, estrés y eventos live (como pediste). Solo quedó la velocidad.

🔜 Para más adelante (cuando lo hables con el grupo):
- Reemplazar los carteles de texto por imágenes/sprites reales de señales.
- Más realismo en el auto / movimiento.
- Sonido, animaciones, etc.

## Nota técnica

El juego está hecho 100% con UI de Unity y construido por código, así no hay que
conectar nada a mano ni depende de imágenes externas. Las señales por ahora se
muestran como un recuadro de color + texto (rojo=prohibición/peligro,
amarillo=precaución, azul=info, verde=permitido).
