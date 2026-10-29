# Documento de Diseño de Videojuego (GDD) – "Onda S: 60 Segundos"

---

## 1. Ficha Técnica y Resumen Ejecutivo (High Concept)

* **Título del Proyecto:** Onda S: 60 Segundos
* **Género:** Survival / Serious Game de Acción Táctica en Primera Persona (FPS)
* **Plataforma Objetivo:** PC (Windows)
* **Motor Gráfico:** Unity 3D (HDRP / URP)
* **Público Objetivo:** Jóvenes universitarios y ciudadanos residentes en zonas de alta actividad sísmica (15 a 35 años).
* **Clasificación:** Basado en los protocolos oficiales de INDECI / Defensa Civil y alineado con la Agenda 2030 de la ONU.

### 1.1. Concepto General (Logline)

*Onda S: 60 Segundos* es un simulador de supervivencia en primera persona donde el jugador se encuentra en el octavo piso de un edificio multifamiliar cuando suena la alarma de alerta sísmica temprana. Cuenta con exactamente **60 segundos** para armar su mochila de emergencia y mitigar riesgos en su departamento antes del impacto destructivo, para luego emprender un descenso táctico entre escaleras colapsadas, oscuridad absoluta y réplicas impredecibles hasta alcanzar la zona segura en la calle.

---

## 2. Justificación Académica y Alineación con los ODS

El proyecto se fundamenta bajo la categoría de **Serious Game (Juego Serio)**, cuyo propósito principal no es únicamente el entretenimiento, sino el entrenamiento situacional y la retención de protocolos de emergencia mediante el aprendizaje experiencial (ensayo, error y consecuencia directa).

| Objetivo de Desarrollo Sostenible (ODS) | Meta Específica de la ONU | Aplicación y Métrica dentro del Videojuego |
| --- | --- | --- |
| **ODS 11:** Ciudades y Comunidades Sostenibles | **Meta 11.5 y 11.b:** Reducir significativamente el número de muertes y pérdidas causadas por desastres y promover la gestión integral del riesgo de desastres (Marco de Sendai). | Entrena la toma de decisiones bajo estrés en edificaciones verticales. Desincentiva conductas mortales comunes (uso de ascensores, pánico en escaleras) y enseña la mitigación preventiva en el hogar (corte de suministros). |
| **ODS 4:** Educación de Calidad | **Meta 4.7:** Asegurar que los alumnos adquieran los conocimientos teóricos y prácticos necesarios para promover el desarrollo sostenible y estilos de vida seguros. | Sustituye la educación pasiva (folletos o charlas teóricas) por **Aprendizaje Basado en Consecuencias**. Si el jugador olvida un ítem vital en la Fase 1, experimenta la dificultad real de su ausencia en la Fase 3. |
| **ODS 3:** Salud y Bienestar | **Meta 3.d:** Reforzar la capacidad en materia de alerta temprana, reducción de riesgos y gestión de los riesgos para la salud nacional. | Incorpora la gestión del estrés fisiológico (pulso y respiración), prevención de asfixia por polvo estructural y aplicación de primeros auxilios básicos post-impacto. |

---

## 3. Flujo del Juego y Estructura por Fases (Core Loop)

Una partida completa está diseñada para durar entre **5 y 8 minutos**, dividida en tres fases continuas sin pantallas de carga intermedias.

### Fase 1: La Onda P – Preparación (Duración: 60 Segundos)

* **Contexto:** Suena la alerta sísmica en el celular del jugador. El movimiento aún es imperceptible durante los primeros 45 segundos; en los últimos 15 segundos comienza una vibración leve.
* **Escenario:** Departamento en el Piso 8 (sala, cocina, baño, dormitorio).
* **Objetivo:** Recolectar hasta **5 objetos clave** en la mochila de emergencia (generados en ubicaciones aleatorias por partida) y ejecutar las **3 acciones de mitigación de entorno** antes de que el reloj llegue a `0:00`.

### Fase 2: Evacuación Vertical Táctica (Del Piso 8 al Piso 3)

* **Contexto:** El temporizador llega a cero y comienza el sismo moderado-fuerte. El jugador sale al pasillo y debe descender por la caja de escaleras de emergencia.
* **Objetivo:** Bajar manteniendo el equilibrio corporal, esquivando obstáculos dinámicos (ventanas que estallan, muebles sueltos, vecinos en pánico) y evitando trampas mortales como el ascensor.
* **Evento de Transición (Clímax):** Al llegar al **Piso 3**, entra la **Onda S (el impacto destructivo principal)**. El tramo de escalera hacia el Piso 2 colapsa frente al jugador, obligándolo a ingresar de emergencia a las oficinas/departamentos del Piso 3 y buscar un "Triángulo de Vida" para sobrevivir al pico del terremoto.

### Fase 3: Zona Cero – Oscuridad y Escombros (Del Piso 3 a la Calle)

* **Contexto:** El sismo principal termina, pero el suministro eléctrico se corta por completo (**Oscuridad total**) y el ambiente se llena de polvo en suspensión.
* **Objetivo:** Navegar entre muros derribados, vigas caídas y puertas trabadas haciendo uso de los objetos recolectados en la Fase 1, sobreviviendo a las **réplicas aleatorias** (caída de escombros del techo) hasta encontrar la escalera de emergencia externa y llegar al Punto de Reunión en la planta baja.

---

## 4. Especificación de Mecánicas de Juego (Gameplay Mechanics)

### 4.1. Controles y Movimiento en 1ª Persona

El controlador del personaje (`FirstPersonController`) modifica su física según el estado del edificio:

* **Locomoción Estándar (`W, A, S, D`):** Caminata fluida en la Fase 1. Durante las Fases 2 y 3, el movimiento se ve afectado por fuerzas laterales (`AddForce`) que simulan el movimiento telúrico.
* **Sprint (`Shift`):** Aumenta la velocidad, pero consume la barra de **Estamina** y eleva el **Pulso Cardíaco**. Si el jugador corre mientras ocurre un sacudón fuerte en las escaleras, tiene un 70% de probabilidad de tropezar y caer, perdiendo salud.
* **Agacharse y Cubrirse (`C` / `Ctrl`):** Reduce la altura del `CharacterController`. Es obligatorio para pasar bajo vigas colapsadas en la Fase 3 y reduce un **80% el daño recibido** si el jugador se ubica junto a una columna estructural o marco reforzado durante una réplica.
* **Salto / Franqueo (`Espacio`):** Permite saltar pequeños escombros en el suelo.
* **Sujeción y Equilibrio (`Mantener E`):** En las escaleras o cerca de barandas, permite al jugador aferrarse para anular el empuje del temblor hasta que pase la sacudida.

### 4.2. Sistema de Inventario Limitado (Mochila de Emergencia)

El jugador cuenta con una interfaz rápida en la parte inferior de **5 Slots** y un límite de **Peso Máximo de 5.0 kg**. Esto obliga a priorizar elementos de supervivencia real sobre objetos cotidianos inútiles (como consolas de videojuegos, laptops o adornos pesados que restan espacio y velocidad).

| Objeto | Peso | Función Mecánica en Fase 2 y 3 | Consecuencia de NO llevarlo |
| --- | --- | --- | --- |
| **Linterna de Mano** | 0.8 kg | Genera un cono de luz potente (`Spotlight`) en la Fase 3 para ver grietas y rutas. Consume batería lentamente. | El jugador debe usar el flash de su celular (alcance mínimo de 2 metros y batería que dura solo 45 segundos). |
| **Botiquín Primeros Auxilios** | 1.2 kg | Restaura un 50% de salud y elimina el estado de "Herido" (cojera que reduce la velocidad un 40%). | Si un escombro golpea al jugador, caminará lento y con visión borrosa el resto de la partida. |
| **Mascarilla / Pañuelo** | 0.2 kg | Filtra el polvo en suspensión de la Fase 3, manteniendo la barra de oxígeno y estamina al 100%. | El personaje tose constantemente en zonas de derrumbe; la estamina máxima se reduce a la mitad. |
| **Agua Embotellada** | 1.0 kg | Reduce el nivel de **Estrés/Pulso** y recupera la estamina instantáneamente. | La recuperación de estamina tras correr o saltar es muy lenta; el pulso alto genera temblor en la cámara. |
| **Radio a Pilas** | 0.8 kg | Al activarla en la Fase 3, revela mediante audio e íconos en pantalla qué ruta de salida está despejada. | El jugador debe explorar a ciegas entre los escombros, arriesgándose a entrar en callejones sin salida. |
| **Herramienta / Llaves** | 1.0 kg | Permite destrabar puertas atascadas en 2 segundos sin hacer ruido ni esfuerzo. | Desbloquear puertas requiere un minijuego de fuerza repetitivo que consume tiempo y estamina. |

### 4.3. Mecánica de Acciones de Entorno (Mitigación en Fase 1)

Interactuar con elementos del departamento (`Mantener tecla E` con barra de progreso circular) altera directamente el estado del nivel en la Fase 3:

1. **Cerrar Válvula Principal de Gas:** Si el jugador **NO** la cierra en la Fase 1, durante la Fase 3 habrá una fuga de gas e incendios bloqueando el pasillo central, forzando una ruta más larga y peligrosa.
2. **Bajar Llave General de Luz (Tablero Eléctrico):** Si **NO** se baja, los cables rotos colgarán del techo soltando chispas en la Fase 3, causando daño eléctrico al pasar cerca de ellos.
3. **Destrabar Puerta Principal del Departamento:** Dejar la puerta entreabierta antes de que empiece el sismo evita que el marco se deforme. Si no se hace, el jugador perderá 10 segundos valiosos golpeando la puerta para salir al iniciar la Fase 2.
4. **La Trampa del Ascensor (Game Over Educativo):** En el pasillo del Piso 8, el ascensor tendrá las puertas abiertas. Si el jugador entra y presiona el botón, las puertas se cierran, el sismo corta los tensores y se muestra una pantalla de derrota inmediata explicando la normativa de seguridad sobre ascensores en sismos.

### 4.4. Sistema de Peligros Dinámicos y Réplicas (Fase 3)

Para mantener la tensión mientras el jugador navega a oscuras, un controlador de eventos (`AftershockManager`) ejecuta réplicas cada **15 a 25 segundos**:

* **Fase de Advertencia (2.5 segundos):** Se escucha un crujido estructural en audio 3D posicional sobre la cabeza del jugador y cae un emisor de partículas de polvo blanco desde el techo.
* **Fase de Impacto:** Si el jugador no se mueve de ese radio de 2 metros o no se coloca bajo un dintel/columna segura, se instancia un bloque de concreto (`Rigidbody`) que cae del techo, restándole **40% de vida** y aturdiéndolo por 3 segundos.

---

## 5. Interfaz de Usuario (HUD) y Sistema de Evaluación

### 5.1. HUD en Partida (Diegético y Minimalista)

* **Esquina Superior Izquierda:** Cronómetro regresivo (`0:60` a `0:00`) en Fase 1, que cambia a indicador de alerta de ondas (`"ALERTA: RÉPLICA EN CAMINO"`) en las Fases 2 y 3.
* **Centro Inferior:** Barra de Inventario de 5 casillas con indicador de peso (`PESO: X.X / 5.0 kg`) y porcentaje de batería sobre el ícono de la linterna.
* **Esquina Inferior Derecha:** Indicador biométrico de **Pulso/Estrés** (Normal, Elevado, Crítico) acompañado de la barra de estamina y el ícono de postura del jugador (De pie / Agachado).
* **Centro de Pantalla:** Retícula dinámica que cambia a un ícono de mano o tecla `[E]` al apuntar a objetos interactuables.

### 5.2. Pantalla de Reporte Final (Métrica Pedagógica)

Al cruzar la puerta de salida hacia el círculo de seguridad en la calle, el juego muestra un **"Informe de Gestión de Riesgo"** con una calificación de **0 a 100 puntos** (Rangos S, A, B, C, F):

* **Preparación de Mochila (+30 pts):** Puntaje según la utilidad real de los ítems seleccionados.
* **Prevención en el Hogar (+30 pts):** +10 pts por cerrar el gas, +10 pts por cortar la luz, +10 pts por asegurar la puerta.
* **Protocolo de Evacuación (+40 pts):** Puntos por no correr en las escaleras durante el pico sísmico, usar el triángulo de vida en la transición de la Onda S y salir con más del 50% de salud.
* **Feedback Educativo:** Lista personalizada de errores cometidos en la partida con la recomendación oficial de INDECI para corregirlos en la vida real.