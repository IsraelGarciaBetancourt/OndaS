# Recolección y mochila

Para el estilo visual, los avisos y la barra de stamina, consulta [OndaS_UI_y_Stamina.md](OndaS_UI_y_Stamina.md).

El jugador usa **E** para recoger un objeto al apuntarlo. La mochila tiene cinco espacios y admite hasta 5 kg en total. Cada objeto ocupa un espacio, incluidos los duplicados. Con **1–5** se selecciona un espacio y con **Q** se deja su objeto sobre el suelo, frente al jugador. Si no se encuentra una superficie accesible, el objeto permanece en la mochila. Las puertas siguen usando E mediante el mismo raycast.

El HUD muestra los nombres, el espacio seleccionado y el peso. Utiliza la tipografía y la animación Scribble existentes. La categoría útil o distractora se mantiene en los datos, pero no se señala en pantalla.

Los 71 prefabs del catálogo tienen `OndaSCollectible`, un collider y una definición en `Assets/OndaS/Items/Definitions`. Cada definición contiene nombre, categoría, peso y prefab para devolverlo al mundo. Los valores de agua, linterna, botiquín, mascarillas, radios y llave inglesa siguen el GDD; los restantes son valores provisionales editables en el Inspector. Para volver a procesar el catálogo existe el menú `OndaS/Configurar ítems recolectables`; las definiciones existentes no se sobrescriben.

El sistema se crea en tiempo de ejecución desde `OndaSDoorInteractor`, que debe estar presente en el jugador y tener una cámara hija o asignada. Los prefabs colocados en una escena se pueden recoger sin configuración adicional. La escena de desarrollo ya tiene el interactor y agua colocada; no se han colocado todos los ítems automáticamente.

La mochila vive durante la sesión y aún no implementa guardado entre escenas o partidas, efectos de uso de los ítems, generación aleatoria, temporizador ni puntuación. Esas mecánicas siguen siendo sistemas separados del GDD.
