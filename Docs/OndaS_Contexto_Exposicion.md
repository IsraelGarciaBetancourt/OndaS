# OndaS: contexto para la exposición

Guía breve para presentar el GDD junto con el estado real del prototipo. El proyecto contiene una visión de juego completa, pero la escena actual implementa solo una parte de esa visión. Las ideas del GDD no deben describirse como funciones ya terminadas.

## De qué trata el juego

**Onda S: 60 Segundos** es un juego de supervivencia en primera persona con propósito educativo. El jugador vive un sismo desde el departamento de un edificio alto. La propuesta busca que practique decisiones de preparación, protección y evacuación, y que vea las consecuencias de sus decisiones. El contexto educativo se relaciona con gestión del riesgo y los ODS 11 y 4; el GDD también menciona el ODS 3.

El GDD plantea una partida en tres momentos: preparar una mochila durante 60 segundos, atravesar el edificio durante el sismo y evacuar entre oscuridad y escombros después del impacto principal. Esa es la visión general; no es la descripción literal del prototipo que hoy se puede jugar.

## Estado actual del proyecto

La escena principal de desarrollo es `Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity`. Es una escena de revisión del edificio con ocho pisos y el departamento del piso 8 preparado como zona de prototipo. La escena explora una dirección visual de terror retro/PSX: resolución interna baja, texturas pixeladas, colores apagados, iluminación y polvo ambiental.

En esa escena ya hay:

- Recorrido en primera persona con caminar, correr con Shift, agacharse con C y stamina. Es un controlador de desarrollo, no todavía un controlador final integrado a todas las fases.
- Interacción con puertas y objetos, recogida y suelta de ítems, dos espacios rápidos y una mochila física con cuatro espacios internos. La capacidad actual es hasta seis ítems en total; el límite de cinco objetos y 5 kg del GDD ya no representa el inventario implementado.
- Modelos de objetos preparados como prefabs y definiciones de ítems. Que un modelo exista no significa que tenga un efecto funcional al usarlo.
- Caja de luz con cinco interruptores y una válvula de gas interactiva. La caja registra si se cortó la corriente, pero todavía no controla peligros eléctricos. La válvula sí se conecta al peligro de fuego del prototipo del sismo.
- Un primer sismo en el piso 8: empieza automáticamente tras 60 segundos o se puede iniciar con F8, dura unos 35 segundos y tiene sacudida de cámara, objetos seleccionados que se mueven/caen, polvo, luces, audio provisional y rotura de algunos paneles de vidrio. Incluye refugio bajo puntos configurados y una condición de derrota si el jugador queda expuesto demasiado tiempo.
- Apagón y linterna para pruebas visuales. Los controles B y F son herramientas de desarrollo y no equivalen todavía a las reglas finales de batería e inventario.

La secuencia del sismo está hecha dentro de un controlador de prototipo; todavía no existe un sistema completo de fases que integre preparación, descenso, impacto del piso 3 y salida a la calle.

## Diferencias importantes con el GDD

| Tema | Lo que plantea el GDD | Estado o dirección actual |
| --- | --- | --- |
| Escala del proyecto | Partida completa del piso 8 a la calle en tres fases | Prototipo centrado en el edificio y el departamento del piso 8; el recorrido completo aún no está implementado |
| Primer sismo | Evacuar por escaleras mientras tiembla | El prototipo actual hace que el jugador busque refugio durante el movimiento; evacuaría cuando termine |
| Protección | “Triángulo de Vida” durante el impacto del piso 3 | La propuesta revisada es usar protección bajo puntos diseñados, agacharse y sujetarse; el término y la secuencia del GDD deben actualizarse |
| Inventario | Cinco objetos, cinco slots y máximo 5 kg | Dos espacios rápidos y mochila de cuatro espacios internos; sin límite de peso activo |
| Objetos | Efectos definidos para linterna, agua, mascarilla, botiquín, radio y herramienta | Modelos e inventario están; los efectos de uso, batería, oxígeno, pulso y guía de rutas aún no están conectados |
| Mitigación | Gas, luz y puerta son tres acciones antes del sismo | Válvula de gas y caja de luz existen como interacciones; la puerta no tiene aún el efecto estratégico del GDD. La caja de luz no activa todavía peligros eléctricos |
| Riesgos y evaluación | Ascensor mortal, réplicas, salud, puntuación ODS e informe final | No forman parte de la secuencia jugable implementada actualmente |
| Aspecto visual | El GDD permite HDRP/URP | El proyecto usa Unity 6.5 y URP 17.5; la prueba retro está en curso y aún requiere validación visual y de rendimiento |

## Dirección de diseño propuesta para explicar

La propuesta más reciente para el ritmo educativo es:

1. **Preparación:** usar el tiempo disponible para recoger suministros y, en futuras iteraciones, completar acciones de mitigación.
2. **Sismo en el departamento:** buscar refugio, agacharse y sujetarse durante el movimiento. El primer prototipo dura aproximadamente 35 segundos.
3. **Evacuación posterior:** salir cuando termine el movimiento y conectar más adelante la ruta por escaleras.
4. **Evento mayor y salida:** el impacto del piso 3, los escombros, las réplicas y el punto de reunión siguen siendo objetivos de diseño, no sistemas completos del prototipo.

Esta secuencia modifica el orden del GDD, que pone la evacuación durante el primer sismo y usa el “triángulo de vida” para el impacto posterior. La modificación aparece como propuesta en `OndaS_Plan_Primer_Sismo.md`; no se ha consolidado aún en una nueva versión del GDD. Si durante la exposición preguntan cuál es la versión oficial, conviene decir que el GDD está pendiente de actualizar y presentar la secuencia anterior como dirección revisada, no como decisión ya aprobada en el documento.

## Qué falta para llegar a la visión completa

- Integrar el reloj de preparación, las acciones previas y las interacciones con un sistema de fases común.
- Conectar el final del primer sismo con el descenso por las escaleras.
- Definir e implementar el evento del piso 3, los obstáculos, las réplicas y la salida.
- Programar los efectos reales de los ítems y los peligros eléctricos; integrar linterna y batería.
- Definir si se conservarán el ascensor con Game Over educativo, el puntaje y el reporte final, y actualizar el GDD en consecuencia.
- Probar la secuencia completa, accesibilidad de los refugios, lectura visual y rendimiento.

## Resumen oral sugerido

> “Onda S: 60 Segundos es un prototipo de supervivencia en primera persona con enfoque educativo sobre decisiones ante un sismo. El GDD describe la meta completa: prepararse, sobrevivir y evacuar desde un edificio alto. En el prototipo ya estamos trabajando el departamento del piso 8, el inventario y un primer sismo con refugio, objetos que reaccionan y efectos visuales retro. La evacuación completa, el evento del piso 3, las réplicas, los efectos de todos los objetos y la evaluación educativa todavía están por desarrollarse. También estamos revisando el GDD: el prototipo actual propone refugiarse durante el movimiento y evacuar después, así que algunos detalles del documento aún no coinciden con la implementación.”

## Documentos de referencia

- [GDD actual](Onda_S_60_Segundos_GDD.md): visión general y especificaciones originales; varias partes son metas, no funciones implementadas.
- [Backlog](Onda_S_60_Segundos_Product_Backlog.md): propuesta inicial de tareas; algunas tarjetas ya cambiaron con el trabajo posterior.
- [Revisión de efectos del sismo](OndaS_Revision_Efectos_Sismo.md): registra la implementación del primer prototipo de sismo.
- [Plan del primer sismo](OndaS_Plan_Primer_Sismo.md): propuesta de secuencia revisada; su encabezado y algunas secciones describen un estado anterior a la implementación y deben leerse con la actualización de la revisión.
- [Estado visual](OndaS_Implementacion_Visual.md), [inventario](OndaS_Inventario.md) e [interfaz y stamina](OndaS_UI_y_Stamina.md): detalle del prototipo actual.
