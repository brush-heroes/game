# Plan de contenido — Manual de Usuario BrushHeroes

## Estructura de secciones

| # | Sección | ¿Implementado? |
|---|---------|---------------|
| 1 | Introducción — qué es el juego, a quién va dirigido | ✅ |
| 2 | Requisitos del sistema (Android, ARCore, cámara frontal) | ✅ |
| 3 | Instalación desde APK | ✅ (pendiente generar APK final) |
| 4 | Menú principal — pestaña Minijuegos | ✅ |
| 4.1 | Panel del héroe (racha, estrellas, misiones mañana/tarde) | ✅ |
| 4.2 | Tarjetas de minijuegos (AR, Cepillado, Hilo Dental) | ✅ (3 cards visibles) |
| 4.3 | Pestaña Calendario | ✅ |
| 4.4 | Leyenda del calendario (colores) | ✅ |
| 5 | Minijuego AR — Cepillado con Realidad Aumentada | ✅ |
| 5.1 | Acceso desde el menú | ✅ |
| 5.2 | Pantalla de inicio + detección de rostro | ✅ (FaceTrackingManager) |
| 5.3 | Selección de modo (Dinámico / Solo guía) | ✅ |
| 5.4 | Modo Dinámico — cómo jugar, zonas, bacterias | ✅ |
| 5.5 | Modo Solo Guía — cómo funciona, sin tiempo | ✅ |
| 5.6 | HUD durante el juego (puntos, puntos de zona, cronómetro) | ✅ |
| 5.7 | Notificación de zona (al pasar de una zona a otra) | ✅ |
| 5.8 | Pausa y reanudación | ✅ (PauseScreen implementado) |
| 5.9 | Pantalla de resultado — éxito ("¡Excelente!") | ✅ |
| 5.10 | Pantalla de resultado — tiempo agotado | ✅ |
| 6 | Minijuego Cepillado (BrushingGame) | ⚠️ VERIFICAR con tu equipo |
| 7 | Minijuego Hilo Dental (DentalFlossGame) | ⚠️ VERIFICAR con tu equipo |
| 8 | Sistema de progreso (racha, estrellas, misiones) | ✅ |
| 9 | Preguntas frecuentes / Solución de problemas | ✅ |

---

## Imágenes requeridas (15 capturas)

Carpeta destino sugerida: `Docs/img/`

### Portada e instalación
| Archivo | Qué capturar | Cómo llegar |
|---------|-------------|-------------|
| `portada_logo.png` | Logo o splash art del juego | Asset del proyecto o captura de la pantalla de carga |
| `install_step1.png` | Ajustes Android → fuentes desconocidas | Captura del dispositivo real (ajustes del sistema) |
| `install_step2.png` | Diálogo de instalación del APK | Al abrir el APK en Android |
| `install_step3.png` | Ícono de la app en el lanzador | Pantalla de inicio del dispositivo tras instalar |

### Menú principal
| Archivo | Qué capturar | Cómo llegar |
|---------|-------------|-------------|
| `menu_minijuegos_page.png` | Pestaña Minijuegos completa | Abrir la app → pestaña inferior izquierda |
| `menu_hero_panel.png` | Primer plano del panel superior (racha, estrellas, misiones, mascota) | Mismo estado, scroll al tope |
| `menu_cards.png` | Las 3 tarjetas de minijuegos | Scroll down en la pestaña Minijuegos |
| `menu_calendario.png` | Pestaña Calendario con el mes actual | Pestaña inferior derecha |
| `menu_calendario_leyenda.png` | Leyenda de colores del calendario | Parte inferior de la vista del calendario |

### Minijuego AR
| Archivo | Qué capturar | Cómo llegar |
|---------|-------------|-------------|
| `ar_buscando_rostro.png` | Pantalla de inicio AR con overlay "Buscando rostro..." | Abrir ARGame SIN cara apuntando a la cámara |
| `ar_rostro_detectado.png` | Misma pantalla pero con cara detectada y botones activos | Abrir ARGame apuntando la cámara al rostro |
| `ar_seleccion_modo.png` | Botones "Dinámico" y "Solo guía" visibles y activos | Mismo estado que arriba |
| `ar_dinamico_gameplay.png` | Modo Dinámico en juego: cara con bacterias, cepillo virtual, HUD | Pulsar Dinámico → durante la sesión |
| `ar_guiado_gameplay.png` | Modo Solo Guía: cepillo moviéndose automáticamente | Pulsar Solo guía → durante la sesión |
| `ar_hud_detalle.png` | Primer plano del HUD superior (score, 6 puntos de zona, cronómetro) | Cualquier modo durante la sesión |
| `ar_zona_notificacion.png` | Overlay de notificación de zona (nombre + imagen de zona) | Al completar una zona |
| `ar_pausa.png` | Pantalla de pausa | Pulsar botón pausa durante el juego |
| `ar_resultado_exito.png` | Pantalla de fin — "¡Excelente! ¡Limpiaste todos tus dientes!" | Completar las 6 zonas en modo Dinámico |
| `ar_resultado_fallo.png` | Pantalla de fin — "¡Tiempo agotado! ¡Inténtalo de nuevo!" | Dejar que expire el cronómetro en modo Dinámico |

### Minijuegos adicionales (BrushingGame y DentalFlossGame)
> Estas capturas dependen de lo que tu equipo haya implementado.
> Confirmar antes de incluir estas secciones en el manual.

| Archivo | Qué capturar |
|---------|-------------|
| `brush_gameplay.png` | Gameplay del minijuego de cepillado |
| `floss_gameplay.png` | Gameplay del minijuego de hilo dental |

---

## Preguntas para confirmar antes de generar el documento final

1. **¿Los minijuegos BrushingGame y DentalFlossGame ya tienen gameplay jugable?**
   Si no, puedo omitir sus secciones o poner una nota de "próximamente".

2. **¿El juego ya genera un APK instalable?** (para la sección de instalación)
   Si no, esa sección puede describir la instalación desde Unity/USB en modo developer.

3. **¿Hay nombre oficial de tu universidad y carrera para la portada?**

4. **¿Tiene el juego una pantalla de carga/splash?** (para `portada_logo.png`)

5. **¿El calendario muestra datos reales o mock?** (el código actual usa mock data automática)
   Solo afecta cómo redactar esa sección.
