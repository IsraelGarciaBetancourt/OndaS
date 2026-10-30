# Plan del primer sismo

Fecha: 30 de octubre de 2026. Estado: propuesta; no implementado.

## Resultado buscado

Al terminar los 60 segundos, el departamento familiar se vuelve peligroso: se escucha el edificio, se mueven objetos y el jugador debe protegerse. Cuando termina el movimiento, aparece el objetivo de evacuar. La partida continúa en la misma escena y conserva los objetos que realmente lleva el jugador.

Propongo separar este primer impacto del evento mayor del piso 3. Primero construiremos una secuencia completa en el departamento del piso 8; después conectaremos el descenso.

## Qué se revisó

Revisión de archivos de la escena guardada `Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity`, scripts de OndaS, GDD, backlog, documentación de inventario y capturas existentes de preparación/apagón. `Library/LastSceneManagerSetup.txt` registra esa escena como activa y está habilitada en Build Settings.

No había herramientas MCP de Unity disponibles en esta sesión. No se ejecutó Play ni se inspeccionaron cambios sin guardar. Las capturas consultadas corresponden a una iteración anterior; la transitabilidad y la lectura visual actuales deben comprobarse en el Editor.

| Base existente | Implicación para el sismo |
| --- | --- |
| Ocho pisos bajo `Structure_03/Interior`, con escaleras y mobiliario | Hay escenario para empezar en el piso 8 y conectar el descenso. Falta verificar el recorrido en Play. |
| Jugador colocado a altura del piso 8; `OndaSLookDevWalkthrough` con `CharacterController` | Reutilizar locomoción, sprint con estamina y agachado con C. |
| Cámara retro a 360 píxeles verticales y manos de primera persona | Mantener la estética y agregar un movimiento sísmico que no interfiera con la mirada. |
| `OndaSDoorInteractor` y `OndaSDoor` | Puertas, recogida y mochila ya comparten E. La protección necesita una prioridad de interacción explícita. |
| Inventario: dos espacios rápidos y cuatro dentro de la mochila | Conservar las instancias existentes; no volver al inventario antiguo de cinco espacios del GDD. |
| Prefabs de mochila y agua referenciados en la escena | Es la muestra de recolección encontrada, no una distribución completa de suministros. |
| Dos emisores de polvo ambiental en el piso 8 | Reutilizar material y estilo; crear emisiones de impacto aparte. |
| Apagón con B y linterna con F, ambos de prueba | Separar los controles de prueba de las reglas de fases e inventario. El apagón de prueba enciende una linterna gratuita. |

No encontré controlador de fases, reloj de 60 segundos, sacudida sísmica, salud/daño, zonas de protección ni efectos funcionales de uso de ítems en los scripts propios revisados. El reloj es un requisito previo de integración, aunque se podrá disparar el sismo manualmente para iterarlo.

## Ajuste propuesto respecto del GDD

El GDD plantea evacuar del piso 8 al 3 mientras ocurre el primer sismo, y buscar un “triángulo de vida” en el impacto posterior. Propongo cambiar esa secuencia a **protección durante el movimiento → evacuación al terminar → evento posterior durante el descenso**.

Esto encaja mejor con el propósito educativo: INDECI recomienda dirigirse a la zona segura externa al finalizar el movimiento y preparar zonas seguras internas accesibles. USGS cuestiona el “triángulo de vida” y recoge la recomendación de agacharse, cubrirse y sujetarse bajo una mesa o escritorio. El nivel necesita un punto de protección diseñado y reconocible; una mesa cualquiera del kit no debe otorgar protección automáticamente.

Fuentes consultadas:

- [INDECI: medidas de preparación ante sismos](https://www.gob.pe/institucion/indeci/noticias/576089-indeci-recomienda-medidas-de-preparacion-ante-sismos).
- [USGS: qué es el “triángulo de vida”](https://www.usgs.gov/faqs/what-triangle-life-and-it-legitimate).

Es una propuesta de cambio de diseño. El GDD existente no se modificó. Los tiempos siguientes son valores de jugabilidad para probar, no predicciones de duración real de un sismo.

## Secuencia jugable

Tiempo contado desde el final de la recolección. Duración inicial propuesta: 35 segundos, ajustable tras probar.

| Momento | Experiencia | Acción del jugador |
| --- | --- | --- |
| 0–3 s: inicio | El reloj llega a cero y cambia a “SISMO — PROTÉGETE”. Rumor grave, vibración creciente, vajilla que suena. | Reconocer el peligro y dirigirse a un punto de protección cercano. Mantiene el control. |
| 3–10 s: aumento | Crujidos localizados, polvo puntual, un cuadro cae y algunos objetos se desplazan. | Agacharse con C y mantener E para sujetarse en un punto válido. |
| 10–25 s: tramo fuerte | Dos o tres sacudidas con intensidad variable; golpes de objetos y oscilación de una luminaria. | Permanecer protegido. Puede mirar alrededor; soltarse o alejarse termina la protección. |
| 25–35 s: descenso de intensidad | Disminuyen cámara y estruendo; quedan polvo y sonidos aislados. | Esperar el final. El mensaje cambia a “El movimiento terminó. Evacúa por las escaleras”. |
| Desde 35 s | El departamento conserva los cambios producidos y el inventario sigue intacto. | Salir al pasillo; la siguiente entrega conecta la ruta de descenso. |

La breve anticipación puede empezar en los últimos 15 segundos de preparación, como plantea el GDD, pero no debe retrasar la transición de 0:00 ni adelantar daño.

## Montaje en el piso 8

- **Sala/comedor:** preparar un punto de protección principal, retirando sillas u otros obstáculos que impidan entrar agachado. Revisar altura y colisiones con el controlador real. La mesa visible en las capturas no está validada como refugio.
- **Otra habitación:** agregar un segundo punto de protección para que comenzar lejos de la sala no produzca una derrota inevitable. Medir el recorrido desde cocina, baño y dormitorios.
- **Cocina:** sonido de vajilla y apertura controlada de una puerta de armario; caída de pocos objetos decorativos. Distinguirlos de los objetos recolectables.
- **Ventanas:** vibración y audio localizado para comunicar peligro. Reservar rotura de cristales y daño para una iteración posterior.
- **Salida al pasillo:** conservar un recorrido disponible al terminar. Los escombros del primer prototipo no deben cerrar la única salida.
- **Iluminación:** variaciones breves y una luminaria oscilante. Mantener suficiente luz para leer la protección y la salida. Reservar el apagón total para el evento posterior del piso 3, conforme al ritmo general del GDD.

## Reglas de interacción y consecuencias

La condición de protección se calcula con posición dentro de un volumen válido, postura agachada y sujeción mantenida. El HUD confirma “PROTEGIDO” solo si se cumplen las tres. Salir del volumen o soltar E retira esa condición. El volumen debe detectar al jugador sin activar daño ni bloquear los raycasts existentes.

El cambio de fase cierra el radial de mochila y cancela cualquier transferencia pendiente. Durante el tramo fuerte, priorizar la protección; los mensajes de recogida no deben tapar el objetivo del sismo. Propuesta inicial: suspender la recogida y gestión del radial durante el sismo, manteniendo puertas y protección, y reactivarlas tras él. Esto es una regla de interfaz para el prototipo, no una desaparición de los objetos del mundo.

No eliminar, rellenar ni vaciar el inventario al comenzar el sismo. La mochila que quedó en el suelo permanece allí con su contenido. No condicionar la supervivencia inmediata a haber recogido un ítem concreto: el beneficio de la preparación aparece durante la evacuación posterior.

Primera entrega: registrar si el jugador se protegió y dar feedback al terminar. Segunda entrega: añadir salud y daño por peligros concretos, anunciados con sonido y polvo, con tiempo de reacción medido en Play. Evitar caídas aleatorias por correr y golpes inevitables al llegar a cero.

## Implementación propuesta

Los siguientes nombres son componentes nuevos propuestos.

| Componente | Responsabilidad |
| --- | --- |
| `OndaSPhaseController` | Estados `Preparation → FirstQuake → Evacuation`; reloj de preparación; transición única y eventos de fase. Más adelante conectará el evento del piso 3. |
| `OndaSEarthquakeController` | Reloj local del sismo, curva de intensidad y eventos programados. Emite una intensidad común para cámara, audio y decoración. |
| `OndaSCameraShake` | Desplazamiento y rotación limitados en un pivote dedicado; intensidad ajustable, opción de reducir movimiento y retorno exacto a reposo. |
| `OndaSProtectionZone` | Punto de protección, volumen, postura y sujeción; información que consume el interactor y el HUD. |
| `OndaSQuakeProp` | Oscilación o caída de objetos seleccionados; parámetros por objeto y activación una sola vez. |
| `OndaSPhaseHud` | Reloj, objetivo, indicación de protección y cambio a evacuación, usando la tipografía existente. |

Cambios de integración necesarios:

1. **Cámara:** `OndaSLookDevWalkthrough` escribe rotación y altura de cámara cada frame. Introducir una jerarquía con pivotes separados de mirada/postura y sacudida, conservando cámara, manos y visual del ítem. Dos scripts no deben escribir el mismo transform.
2. **Movimiento:** cualquier desplazamiento sísmico del jugador debe entrar por su `CharacterController` y respetar colisiones. No aplicar `AddForce` a este controlador ni mover físicamente todo el edificio. La primera entrega puede usar solo sacudida visual.
3. **Interacción:** añadir la prioridad de protección y controles por fase dentro del interactor. Desactivar todo el componente también ocultaría inventario y puertas.
4. **Iluminación:** extraer la gestión definitiva de luces del controlador de recorrido. B/F quedan como herramientas de desarrollo; la linterna definitiva comprobará inventario y, más adelante, batería.
5. **Objetos:** aplicar física solo a una lista pequeña de decoración elegida. Algunos cuerpos están heredados de prefabs: comprobar componentes finales en Unity. No activar física indiscriminadamente en muebles, arquitectura, objetos guardados o la visual del objeto en mano.
6. **Disparadores:** usar referencias serializadas e identificadores de piso; los nombres actuales incluyen `3nd Floor`, `4rd Floor`, etc. No deducir el piso a partir de texto o de la altura de la cámara.

## Orden de trabajo

1. **Recorrido y protección:** abrir la escena, recorrer el departamento y la salida, medir acceso a dos puntos de protección y ajustar colisiones.
2. **Transición funcional:** reloj 60 → 0, fase de sismo con disparo único, HUD y continuación a evacuación, conservando inventario.
3. **Sensación del sismo:** curva de cámara, rumor, crujidos espaciales, cinco a ocho objetos decorativos y emisiones breves de polvo. Ajustar primero un solo departamento.
4. **Protección completa:** C + mantener E, feedback claro y registro del resultado. Validar conflicto con puertas y mochila.
5. **Integración del descenso:** continuar hacia escaleras, verificar pisos 8 a 3 y preparar por separado el evento posterior. Salud, daños, puertas trabadas y efectos de uso de ítems se incorporan después de que la secuencia base funcione.

## Criterios de aceptación de la primera entrega

- El reloj dispara el sismo exactamente una vez al alcanzar cero, incluso si E está pulsada o el radial estaba abierto. Puede empezarse directamente en el sismo desde una opción de desarrollo.
- No hay teletransporte, carga de escena ni reinicio del inventario. Se conservan los dos espacios rápidos y el contenido de cada mochila, equipada o en el suelo.
- La cámara permite orientar la vista, restaura su posición al terminar y respeta el agachado. El ajuste de movimiento reducido mantiene el audio y las señales del HUD.
- Hay un punto de protección alcanzable desde cada habitación; no es posible quedar atrapado bajo la mesa al intentar levantarse.
- Los eventos decorativos ocurren una sola vez, no desplazan arquitectura y dejan transitable la salida.
- El HUD distingue buscar protección, estar protegido y comenzar la evacuación. El apagón de prueba no regala una linterna durante la partida definitiva.
- Reiniciar Play restaura objetos, luces y controles; no aparecen errores de consola. Confirmar todo en Play antes de considerar implementada la fase.

El prototipo se considera completo cuando se puede jugar **preparación de 60 s → primer sismo de aproximadamente 35 s → salida al pasillo**, con protección legible e inventario conservado.
