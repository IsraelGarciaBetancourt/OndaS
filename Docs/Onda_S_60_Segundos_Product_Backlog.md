Para que puedas copiar y pegar directamente en **Trello, Jira o Notion**, he estructurado las tareas como tarjetas listas para tu **Product Backlog**, divididas por roles técnicos de **Unity 3D** y organizadas en 4 Sprints lógicos.

---

## 📋 Estructura de Columnas para tu Tablero

1. **Backlog (Por hacer):** Todas las tarjetas listadas abajo.
2. **Sprint Actual (Seleccionadas para la semana):** Lo que van a atacar ahora.
3. **In Progress (En desarrollo):** Lo que cada miembro está programando/modelando hoy.
4. **Testing / Integración en Unity:** Tareas listas que deben probarse en la escena principal sin que tiren errores de consola ni rompan el repositorio (GitHub).
5. **Done (Terminado):** Funcional y aprobado.

---

## 🛠️ Tarjetas del Backlog por Área (Para repartir en el equipo)

### 💻 Épica 1: Programación Core y Jugador (C# / Unity)

*Ideal para el programador principal de gameplay.*

* **[PROG-01] Configuración del Repositorio y Proyecto Base:** Crear el proyecto en Unity 3D (URP), configurar el archivo `.gitignore` para Unity y subir la escena base a GitHub/GitLab.
* **[PROG-02] Controlador en 1ª Persona (`FirstPersonController`):** Programar movimiento con `CharacterController` (WASD, salto, agacharse reduciendo el `height` del collider y sprint con consumo de barra de estamina).
* **[PROG-03] Sistema de Raycast e Interacción (`InteractionSystem`):** Lanzar un `Physics.Raycast` desde el centro de la cámara que detecte objetos con la interfaz `IInteractable` (mostrar prompt `[E]` al mirar ítems, puertas o llaves de gas).
* **[PROG-04] Sistema de Inventario por Slots y Peso:** Crear estructura con `ScriptableObjects` para los ítems. Programar un array de 5 slots máximos y validación de límite de 5.0 kg.
* **[PROG-05] Controlador de Cámara Sísmica (`CameraShake`):** Usar *Cinemachine* (`CinemachineImpulseSource`) o una corrutina de *Perlin Noise* para agitar la cámara según la intensidad de la fase y hacer que el jugador pierda estabilidad si corre.
* **[PROG-06] Mecánica de Linterna y Celular:** Programar el encendido/apagado de un componente `SpotLight` adherido a la mano/cámara, con un script que drene la variable `batteryLife` con `Time.deltaTime`.

---

### ⚡ Épica 2: Sistemas de Entorno, Fases y Peligros (C#)

*Ideal para el segundo programador o encargado de lógica de nivel.*

* **[SYS-01] Game Manager y Reloj de 60 Segundos:** Controlar los estados del juego (`Fase1_Preparacion`, `Fase2_Evacuacion`, `Fase3_Escombros`, `GameOver`, `Victoria`) y el temporizador regresivo.
* **[SYS-02] Spawner Aleatorio de Ítems (Estilo *60 Seconds!*):** Crear un script con un array de `Transform[] spawnPoints` en el departamento para instanciar la linterna, agua y botiquín en lugares distintos en cada partida.
* **[SYS-03] Acciones de Mitigación (Variables Globales):** Programar la interacción mantenida (`Hold [E]`) para la llave de gas, tablero eléctrico y puerta. Guardar booleanos (`gasCerrado`, `luzCortada`) que activen o desactiven el fuego y los cables con chispas en el Piso 3.
* **[SYS-04] Trampa del Ascensor (Trigger de Game Over):** Crear un `BoxCollider` (`isTrigger`) dentro del ascensor del Piso 8 que cierre las puertas, reproduzca sonido de caída y dispare la pantalla educativa de derrota.
* **[SYS-05] Generador de Réplicas y Escombros (`AftershockManager`):** Corrutina en la Fase 3 que cada 20 segundos active un `ParticleSystem` de polvo en el techo por 2.5 segundos y luego instancie un prefab de escombro con `Rigidbody` y un `OnCollisionEnter` que reste salud al jugador si no está bajo una zona con el tag `"ZonaSegura"`.
* **[SYS-06] Calculadora de Puntaje ODS (Reporte Final):** Script que evalúe qué ítems llevó el jugador, si cerró el gas/luz y su vida restante para calcular una nota de 0 a 100 al tocar el collider de la calle.

---

### 🏢 Épica 3: Nivel 3D, Iluminación y Efectos (Level Design & 3D)

*Ideal para quien arme los escenarios, busque assets o modele en Blender.*

* **[3D-01] Whiteboxing (Bloqueo con ProBuilder):** Armar la geometría gris básica del Departamento (Piso 8), la caja de escaleras modular y el Piso 3 colapsado para probar escalas y colisiones.
* **[3D-02] Vestido del Departamento (Piso 8):** Colocar modelos 3D (muebles, cocina, camas, válvula de gas, caja de luz) y configurar sus `Colliders`. Añadir `Rigidbody` a objetos pequeños (cuadros, vasos) para que caigan cuando empiece el temblor.
* **[3D-03] Construcción de Escalera Modular y Piso 3 (Destruido):** Duplicar el prefab de escaleras del piso 8 al 3. En el Piso 3, bloquear la escalera hacia el piso 2 con mallas de escombros y colocar vigas a media altura que obliguen a agacharse (`C`).
* **[3D-04] Iluminación y Apagón Dinámico:** Configurar luces *Realtime/Mixed*. Crear el script o animación que apague las luces del edificio al entrar a la Fase 3, dejando solo luces rojas de emergencia parpadeando y *Fog* (niebla densa de polvo) en el `Volume Profile`.
* **[3D-05] VFX de Partículas (Fuego, Chispas y Polvo):** Crear sistemas de partículas en Unity para el polvo cayendo del techo, humo ambiental, chispas de cables rotos y fuego por fuga de gas.

---

### 🎨 Épica 4: UI/UX (Figma a Unity Canvas) y Audio

*Ideal para el diseñador de interfaz y encargado de documentación.*

* **[UI-01] Diseño de Wireframes y Mockups en Figma:** Diseñar el HUD (reloj, 5 slots de mochila, indicador de peso, barra de pulso/estamina), menú principal y pantalla de "Reporte de Supervivencia ODS".
* **[UI-02] Maquetación del HUD en Unity (`Canvas`):** Exportar sprites desde Figma en PNG, configurar el `Canvas Scaler` (*Scale With Screen Size* 1920x1080) y conectar las barras e íconos con los scripts del jugador usando `TextMeshPro`.
* **[UI-03] Pantalla de Game Over y Reporte Final:** Maquetar los paneles que aparecen al morir (ej. por usar el ascensor) o al ganar, mostrando los consejos oficiales de INDECI.
* **[AUD-01] Implementación de Audio 3D y Alarma:** Integrar `AudioSource` para la sirena del SASPe (celular), sonido subterráneo del sismo, crujidos de concreto en 3D espacial (para avisar dónde caerá un escombro) y tos/latidos del personaje.

---

### 📄 Épica 5: Entregables de Universidad (ODS / Documentación)

*Para repartir entre todos o asignar al encargado de gestión.*

* **[DOC-01] Redacción de Matriz y Justificación ODS 11 y 4:** Pasar en limpio el GDD al formato oficial del curso/tesis.
* **[DOC-02] Diseño de Encuesta Pre-Test y Post-Test:** Crear un formulario de Google Forms de 5 preguntas sobre qué hacer en un sismo para demostrar con gráficos ante el jurado que los usuarios aprendieron después de jugar tu prototipo.

---

## 🗓️ Sugerencia de Sprints (Cómo ordenarlo por semanas)

| Sprint | Meta del Sprint (Deliverable) | Tarjetas a incluir |
| --- | --- | --- |
| **Sprint 1** | **Prototipo Gris Jugable (Fase 1):** Caminar por el departamento con cajas grises, agarrar 5 cubos con `[E]` y que el reloj baje de 60 a 0. | `PROG-01`, `PROG-02`, `PROG-03`, `PROG-04`, `SYS-01`, `3D-01`, `UI-01` |
| **Sprint 2** | **Evacuación y Físicas (Fase 2):** Cámara que tiembla, bajar escaleras, trampa del ascensor y acciones de cerrar gas/luz funcionando. | `PROG-05`, `SYS-02`, `SYS-03`, `SYS-04`, `3D-02`, `UI-02` |
| **Sprint 3** | **Zona Cero y Oscuridad (Fase 3):** Apagón de luces, linterna funcionando, caída de escombros en el Piso 3 y llegada a la meta. | `PROG-06`, `SYS-05`, `3D-03`, `3D-04`, `3D-05` |
| **Sprint 4** | **Pulido, Audio y Sustentación:** Sonidos 3D, pantalla de puntaje final ODS, pruebas de errores y documento listo. | `SYS-06`, `UI-03`, `AUD-01`, `DOC-01`, `DOC-02` |
