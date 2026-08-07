# LumenLab — petición de revisión externa

**Fecha:** 2026‑08‑07 · **Versión del programa:** 1.0.0 (sin publicar) · **Doc de traspaso:** `docs/REGLAS-NEGOCIO.md` v4.02

## Qué se pide

Revisar el **mecanismo de despliegue y actualización** que se acaba de construir, antes de repartir el programa a seis equipos de un laboratorio de ensayos acreditado. No se busca una revisión de estilo ni de la lógica de negocio de la norma: se busca **que alguien intente romper el reparto y el formato de fichero**.

Al final hay una lista de agujeros que veo yo. La parte útil de la revisión empieza donde acaba esa lista.

---

## 1. Contexto en dos minutos

| | |
|---|---|
| **Qué es** | Sustituye un Excel de toma de notas de ensayos de luminarias (EN/IEC 60598 y otras tres normas) |
| **Escala** | 6 equipos, un laboratorio, ~250 tomas de notas vivas |
| **Stack** | .NET 10 + WPF, MVVM a mano, **cero dependencias externas** |
| **Almacenamiento** | Ficheros `.lmnlab` (JSON) en una carpeta de OneDrive. No hay servidor ni base de datos |
| **Tests** | 620, ninguno toca la interfaz |
| **Estado** | Sin publicar. Ningún ensayo real completo todavía |

**Restricciones que no se negocian y explican casi todas las decisiones:**

- No hay servidor, ni lo habrá a corto plazo. OneDrive es el único canal.
- No hay administrador de sistemas. El instalador no puede pedir permisos en cada actualización.
- **Nunca se puede dejar al laboratorio sin poder trabajar.** Es la regla que gana todos los empates.
- Es un laboratorio acreditado: los `.lmnlab` son registros de ensayo.

---

## 2. Lo construido

### 2.1 Blindaje del fichero contra versiones mixtas

**Problema:** seis equipos se actualizan de uno en uno, así que habrá días con dos versiones conviviendo. Se midió qué pasaba: un `.lmnlab` escrito por una versión posterior, tocado por una anterior, **perdía los campos nuevos sin error y sin aviso**. Y el camino que lo provocaba no era abrir el fichero, sino **arrastrar una barra en el calendario**, que reescribe el documento entero de ficheros que nadie tiene abiertos.

**Solución, en dos capas:**

| Capa | Qué cubre | Dónde |
|---|---|---|
| `[JsonExtensionData]` | Campos nuevos sueltos: se conservan y se devuelven tal cual | `DocumentoProyecto`, `Planificacion`, `Colaborador`, `ValorGuardado` |
| `FormatoDeFichero` | Cambios de **forma**: se corta el guardado | `src/LumNotas.Core/Datos/FormatoDeFichero.cs` |

La segunda existe porque la primera no basta: si un campo conocido cambia de significado, el programa viejo lo interpreta mal **precisamente porque reconoce el nombre**. Ahí lo único correcto es no tocar el fichero.

**Leer un fichero más nuevo sí se permite.** Dejar a un técnico sin consultar un ensayo porque su equipo va una versión por detrás sería peor que el problema.

**Detalle no evidente:** los valores de ensayo se reconstruyen desde memoria al guardar, así que su `Desconocido` hay que **rescatarlo del fichero anterior emparejando por ámbito, campo y muestra** (`RepositorioDeProyectos.DesconocidoDe`).

### 2.2 Reparto del programa

**Problema:** «Publicar» escribía el número de versión y nada más. Los otros cinco equipos leían «hay una versión más nueva» y no tenían de dónde sacarla.

**Diseño:**

```
compartida/
  programa/
    1.0.0/  ← ficheros + manifiesto.json (nombre y tamaño de cada uno)
    1.0.1/
  version.json        ← el marcador. SE ESCRIBE EL ÚLTIMO
```

- El acceso directo y la extensión `.lmnlab` apuntan a un **lanzador** (`src/LumNotas.Lanzador`).
- El lanzador lee el marcador, **verifica el manifiesto**, copia a `%LOCALAPPDATA%\LumenLab\versiones\<versión>` y arranca.
- Publicar se hace **desde el programa** (`Ayuda | Acerca de | Publicar`): copia la instalación que ya funciona, no una compilación que nadie ha abierto.

**Tres decisiones que conviene atacar:**

1. **La compartida es almacén, no sitio de ejecución.** Arrancar desde OneDrive bloquea el `.exe` —y entonces no se puede publicar encima—, falla sin conexión, y con Archivos a Petición puede estar solo en la nube.
2. **OneDrive no sincroniza en orden**, así que escribir el marcador el último no basta: puede llegar antes que los ficheros. De ahí el manifiesto. Si no cuadra, **se arranca con lo de siempre** y se reintenta.
3. **Se verifica el tamaño, no una huella.** Caza el fallo real —fichero ausente o a medio bajar— sin dar a entender una garantía contra manipulación que aquí la dan los permisos de la carpeta.

**Volver atrás** es reescribir `version.json` con el número anterior. Se conservan las dos últimas versiones a los dos lados.

### 2.3 Una sola instancia

Dos ventanas podían tener abierta la misma toma de notas y ganaba el último que guardaba. Mutex por sesión + tubería con nombre: el segundo arranque le pasa la ruta al primero y se cierra.

No basta con cerrarse: si lo hiciera, el doble clic sobre un `.lmnlab` no haría nada.

### 2.4 Cabos sueltos

- Caché del escaneo: escritura atómica (antes se cortaba con dos instancias).
- `errores.log`: rota a medio mega, conserva el tramo anterior.
- Versión publicada: se relee cada 30 min (antes, una vez por sesión).
- Cultura fijada a `es-ES`, formato **e interfaz**. De ahí salen cómo se leen las fechas de un registro de ensayo.
- Identidad del ejecutable: `LumenLab.exe`, icono propio, autoría DMP.

---

## 3. Qué se verificó, y cómo

Todo lo de abajo se **midió**, no se dedujo.

| Qué | Cómo | Resultado |
|---|---|---|
| Pérdida de campos en versiones mixtas | Fichero escrito a mano + arrastre real por `ActualizarPlanificacion` | Antes: 3 campos perdidos. Después: conservados |
| Corte por formato posterior | Ídem con `lmnlab/2` | No se escribe; fichero **byte a byte idéntico** |
| Profundidad de la protección | Campos dentro de `colaboradores[]` y `valores[]` | Primera versión los perdía. Corregido y vuelto a medir |
| Reparto con ficheros reales | `publicar` + `PonerAlDia` sobre el publish de verdad (17 ficheros, 2,1 MB) | Copia, verifica, segunda pasada no copia |
| Lanzador de punta a punta | Equipo simulado sin nada + compartida temporal | Se trajo la 1.0.0, arrancó el programa, anotó en su registro |
| Una sola instancia | Dos arranques reales | La segunda se cierra sola |
| Origen del «Select a date» | Tres hipótesis probadas por separado | **No es el `Language`**, es la cultura de interfaz |
| Identidad del ejecutable | `FileVersionInfo` sobre el publish | Correcta en programa y lanzador |
| Icono | Renderizado a 5 tamaños e inspeccionado | Legible a 16 px |

**620 tests en verde.** Los nuevos: `VersionesMixtasTests` (7) y `RepartoDelProgramaTests` (15), casi todos de *qué pasa cuando algo va mal*.

---

## 4. Lo que NO está verificado

Esto es lo que un revisor debería asumir como no probado:

1. **Nada se ha probado sobre OneDrive real.** Todo el reparto se ha ejercitado sobre carpetas locales. `File.Replace` y `Directory.Move` sobre una carpeta sincronizada pueden comportarse de otra forma.
2. **El instalador nunca se ha compilado.** `entrega/LumenLab.iss` está escrito y no ha pasado por Inno Setup. Incluye código Pascal (detección del runtime) sin ejecutar jamás.
3. **Los diálogos modales no se han podido ver.** En el entorno de desarrollo no se materializan (comprobado por tres vías). Lo que se garantiza de ellos es lo que exige el compilador.
4. **Ningún ensayo real completo.** Lo está haciendo el laboratorio ahora.
5. **Sin firma de código.** Decisión tomada: se asume el aviso de SmartScreen.
6. **La rotación del registro y la caducidad de 30 min** no tienen test.

---

## 5. Agujeros que veo yo y no he cerrado

Ordenados por lo que me preocupa.

### 5.1 El lanzador lee un fichero de la aplicación sin contrato

`src/LumNotas.Lanzador/Programa.cs` abre `%APPDATA%\LumNotas\ajustes.json` y busca las claves `CarpetaCompartida` y `CarpetaDeProyectos` **por su nombre en texto**. La clase `Ajustes` vive en `LumNotas.App`, a la que el lanzador no puede referenciar.

Si alguien renombra esas claves, el lanzador deja de encontrar la carpeta y **arranca la versión vieja para siempre, en silencio** — el peor modo de fallo posible en esta pieza. No hay ningún test que ate las dos partes.

*Arreglo que propondría:* mover `Ajustes` al núcleo, o al menos las dos constantes, y un test que compare.

### 5.2 La disciplina del número de formato es humana

`FormatoDeFichero.VersionQueSeEntiende` hay que subirlo **a mano** cuando cambie la forma del fichero. Nada lo obliga. Es exactamente el fallo que ya mordió una vez en este proyecto con `CacheDeResumenes.Formato`, y allí se resolvió con un test por reflexión que falla al añadir un campo.

Aquí no existe ese test, y el fallo sería peor: silencioso y sobre registros de ensayo.

### 5.3 Interacción lanzador / instancia única, no comunicada

Si un técnico tiene el programa abierto (v1) y se publica la v2, al pulsar el acceso directo el lanzador **copia la v2**, arranca su `.exe`, y este ve el mutex y se cierra pasando el argumento a la v1. Resultado: la actualización está en disco pero **no surte efecto hasta cerrar todo**, y nadie se lo dice al usuario.

Es defendible —no puedes tener dos versiones a la vez— pero está sin comunicar y sin decidir conscientemente.

### 5.4 Una versión rota se reinstala en bucle

Si la v2 se copia bien pero **revienta al arrancar**, el lanzador la seguirá eligiendo cada día. No hay «marcar versión como mala» ni detección de arranque fallido. La salida es manual: reescribir `version.json`.

### 5.5 Publicación concurrente

`RepartoDelPrograma.Publicar` hace `Directory.Delete` + copia sobre la compartida sin ningún cerrojo. Dos personas publicando el mismo número a la vez se pisan. Poco probable (publica una persona), pero no está impedido.

### 5.6 La tubería con nombre no lleva ACL explícita

Cualquier proceso del mismo usuario puede conectarse y mandar una ruta, que se abriría. Riesgo bajo —mismo usuario, misma sesión— pero es una superficie que no estaba antes.

### 5.7 Fijar la cultura a `es-ES` toca el parseo de números en todo el programa

El lector de importes prueba cultura actual y luego invariante, así que sigue admitiendo `2000.50`. **No se ha auditado exhaustivamente el resto** de conversiones del programa.

### 5.8 Compatibilidad entre lanzador y manifiesto

El lanzador lleva su propia copia de `LumNotas.Core.dll`, congelada en la instalación. Si el formato del manifiesto cambia, los lanzadores viejos no lo entenderán y habrá que volver a los seis equipos. El contrato se ha dejado deliberadamente simple (nombre + tamaño) por eso, pero no está declarado como contrato ni versionado.

### 5.9 Integridad de los registros

Los `.lmnlab` son JSON en texto plano en una carpeta compartida: cualquiera con acceso los edita con el Bloc de notas. La respuesta acordada son **permisos de carpeta**, y todavía no se han puesto. Tampoco hay trazabilidad de quién guardó qué — decisión del laboratorio: el autor es el Técnico 1 y la firma es la del director técnico sobre el HTML exportado.

### 5.10 Copia de seguridad nunca ensayada

La única red es el historial de versiones de OneDrive. Nadie ha probado a restaurar un `.lmnlab` desde ahí.

---

## 6. Preguntas concretas

1. ¿El tamaño es verificación suficiente contra una sincronización parcial de OneDrive, o hay modos de fallo que dan el tamaño correcto con contenido incorrecto?
2. ¿`File.Replace` y `Directory.Move` se comportan de forma fiable dentro de una carpeta sincronizada por OneDrive? Es la base de toda la escritura atómica del programa, no solo del reparto.
3. ¿Hay una forma razonable de **obligar** a subir el número de formato cuando cambia la forma del fichero (5.2)?
4. Instalación por usuario en `%LOCALAPPDATA%` para no pedir administrador en cada actualización: ¿es la elección correcta, o hay algo mejor sin servidor ni dominio?
5. ¿Qué falta aquí para que un auditor de ISO/IEC 17025 dé por válida la validación del software?
6. ¿El reparto tiene algún modo de fallo que deje al laboratorio sin poder abrir el programa? Es la única consecuencia que se considera inaceptable.

---

## 7. Cómo reproducirlo

```bash
dotnet build LumNotas.sln
dotnet test LumNotas.sln
dotnet run --project src/LumNotas.App
```

`dotnet test` **no reconstruye** la aplicación: `LumNotas.App` no es un proyecto de pruebas.

Generar lo que se repartiría: `entrega/publicar.ps1`. El guion del instalador está en `entrega/LumenLab.iss`.

### Ficheros que concentran lo revisable

| Fichero | Qué |
|---|---|
| `src/LumNotas.Core/Despliegue/RepartoDelPrograma.cs` | Todo el reparto |
| `src/LumNotas.Core/Despliegue/ControlDeVersion.cs` | El marcador |
| `src/LumNotas.Core/Datos/FormatoDeFichero.cs` | El corte por formato |
| `src/LumNotas.Storage/RepositorioDeProyectos.cs` | Lectura y escritura del `.lmnlab` |
| `src/LumNotas.Lanzador/Programa.cs` | El lanzador |
| `src/LumNotas.App/UnaSolaInstancia.cs` | Mutex y tubería |
| `tests/.../RepartoDelProgramaTests.cs` | 15 tests, casi todos de fallos |
| `tests/.../VersionesMixtasTests.cs` | 7 tests de versiones conviviendo |

El **porqué** de cada decisión está en `docs/REGLAS-NEGOCIO.md`, entradas DD‑151 a DD‑155.
