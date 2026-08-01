# Reglas de negocio — Toma de notas de ensayos de luminarias (EN/IEC 60598)

**Origen:** `TomaDeNotasExcel.xlsx`, plantilla v2.1
**Extraído el:** 2026-07-29 (última modificación del libro: 2026-07-29)
**Versión del documento:** 2.8 — calendario con arrastre
**Actualizado:** 2026‑08‑06
**Propósito:** documento de revisión previo a la construcción de la aplicación de escritorio. Cada regla debe ser **confirmada, corregida o eliminada** por el laboratorio antes de programarla.

## Alcance del borrador

| | |
|---|---|
| **Dentro** | EN/IEC 60598‑1 y sus partes ‑2 (‑2‑1, ‑2‑2, ‑2‑3, ‑2‑4, ‑2‑5, ‑2‑10, ‑2‑13, ‑2‑18, ‑2‑22 y «OTRO») |
| **Fuera** | **IK** (IEC 62262 + IEC TR 62696) y **módulos LED** (IEC 62031) |

Decisión del laboratorio (2026‑07‑29): para valorar la viabilidad, el borrador cubre solo la 60598.

> **Supuesto declarado:** «la 60598» incluye las partes ‑2, porque el motor las trata como parte del mismo árbol de aplicabilidad y comparten la hoja de toma de notas. Si el borrador debe limitarse a la ‑1, se retiran además los 8 bloques de la [sección 5.14](#514-partes-2) y el documento se reduce en otro ~15 %.

**Efecto directo de la exclusión:** desaparecen **las 26 celdas `#REF!` del libro** y los 5 controles huérfanos. Todas pertenecían a IK o a 62031, incluidas —de rebote— las que rompían el texto de normas aplicadas y el panel de avance. El borrador arranca, por tanto, sin lógica rota que reconstruir. Detalle en [D‑01](#11-defectos-incoherencias-y-preguntas-abiertas).

Las partes excluidas se conservan en este documento marcadas como 🚫 **fuera de alcance**, para no perder la información cuando se retomen.

## Registro de decisiones

| Fecha | Punto | Decisión |
|---|---|---|
| 2026‑07‑29 | Alcance | Solo EN/IEC 60598 (‑1 y partes ‑2). IK y 62031 fuera |
| 2026‑07‑29 | Edición | Varios técnicos, **por turnos**: un fichero por proyecto con registro de autoría, sin edición simultánea |
| 2026‑07‑29 | D‑02 | **Numeración canónica: edición 2024** (7 = Construcción, 11 = IP). La antigua queda solo como alias de visualización |
| 2026‑07‑29 | D‑04 | Se acepta el comportamiento actual: **un 0 cuenta como dato introducido** |
| 2026‑07‑29 | D‑05 | Corregido el diagnóstico: los acondicionamientos son correctos. **Se arregla el ensayo de bola** (comparaciones de solo hora) |
| 2026‑07‑29 | D‑10 | Se acepta para el borrador: el agregador de partes ‑2 **no incluye ‑2‑5 ni ‑2‑22** |
| 2026‑07‑29 | D‑19 | Se acepta: ‑2‑7 sigue sin casilla |
| 2026‑07‑29 | D‑20 | Se acepta: «OTRO» sigue fuera de la comprobación de normas |
| 2026‑07‑29 | D‑18 | **Resuelto:** se mantienen los pesos y se añade un contador de apartados completados |
| 2026‑07‑29 | D‑11 | **Resuelto:** la aplicación no firma. Se firma el PDF impreso, fuera del programa |
| 2026‑07‑29 | DD‑01 | **Stack: .NET 8 + WPF** |
| 2026‑07‑29 | DD‑10 | El catálogo de equipos se importa tal cual está |
| 2026‑07‑30 | DD‑20 | **El informe se genera en HTML**, no con librería de PDF. Word lo abre como documento y el navegador lo imprime a PDF con Ctrl+P. Cero dependencias externas |
| 2026‑07‑30 | DD‑21 | **Máximo de muestras: 30.** El Excel admitía 8; ampliación pedida por el laboratorio |
| 2026‑07‑30 | DD‑22 | **Numeración de muestras editable.** El identificador es `EBP_SAFE<código><NN>`; cambiar un número renumera consecutivamente las siguientes |
| 2026‑07‑30 | DD‑23 | **La cabecera del proyecto es obligatoria.** Hasta que esté completa (todo menos técnico 2) no se muestran las secciones de ensayo |
| 2026‑07‑30 | DD‑24 | **Los apartados que no aplican desaparecen**, no se quedan en gris: ensayos de partes ‑2 no marcadas, tierra en Clase II, doble aislamiento en Clase I |
| 2026‑07‑31 | DD‑25 | **Se retiran el porcentaje ponderado y la barra de progreso** de la cabecera por no aportar nada al técnico. Queda solo «X/Y apartados completados». Los pesos se siguen calculando en `IndicadorDeAvance` por si vuelven a hacer falta |
| 2026‑07‑31 | DD‑26 | **Tablero de gestión de proyectos** en su propia pestaña: una columna por proyecto y una tarjeta por sección pendiente |
| 2026‑07‑31 | DD‑27 | **Los proyectos se detectan escaneando una carpeta**, no con un índice. El fichero es la única verdad: con OneDrive y varios técnicos un índice se desincronizaría y mentiría |
| 2026‑07‑31 | DD‑28 | **El avance del tablero se cuenta por secciones, no por apartados.** La sección 7 vale 1 aunque tenga 13 apartados dentro |
| 2026‑08‑01 | DD‑29 | **Cuatro tomas de notas independientes**: 60598‑1, 62031, 60529 (IP) y 62262 (IK). En el Excel el IP y el IK comparten hoja; el laboratorio pidió separarlos |
| 2026‑08‑01 | DD‑30 | **Un proyecto puede llevar varias normas.** El modelo de datos lo soporta ya (`DatosProyecto.Normas`); el panel de inicio donde se eligen queda para más adelante |
| 2026‑08‑01 | DD‑31 | **La toma de notas de luminarias no se toca.** La hoja del libro nuevo no cambió respecto a la ya volcada |
| 2026‑08‑01 | DD‑32 | **30 muestras como máximo en todas las normas**, no los 8 y 3 del Excel |
| 2026‑08‑01 | DD‑33 | **El identificador de muestra sale de la plantilla.** En IK 62262 el laboratorio usa `EBP_CLIM…` en lugar de `EBP_SAFE…` |
| 2026‑08‑01 | DD‑34 | **Los catálogos de equipos se importan por separado**, uno por norma |
| 2026‑08‑01 | DD‑35 | **Qué es obligatorio en la cabecera lo decide la plantilla**, no el código: se leen los campos con `obligatorio: true` |
| 2026‑08‑06 | DD‑57 | **El gesto de arrastre vive en el núcleo** (`BarraDePlanificacion`), no en el modelo de vista. Los eventos de ratón no se pueden automatizar en este equipo, así que la única forma de comprobar el gesto es que su lógica esté fuera de la interfaz |
| 2026‑08‑06 | DD‑56 | **El arrastre se ajusta a días, no a semanas.** Se planifica por semanas, pero un servicio empieza el día que empieza; el número de semana se enseña como ayuda, no como rejilla obligatoria |
| 2026‑08‑06 | DD‑55 | **El calendario mide en semanas ISO**, no en días ni en meses: es la unidad con la que planifica el laboratorio («entra en la S32»). La aritmética vive en `EjeDeSemanas`, dentro del núcleo, para poder probarla |
| 2026‑08‑06 | DD‑54 | **Una tarjeta por toma de notas.** Un servicio con 60598‑1 + ‑2‑3 + IK + 62031 es **una sola tarjeta**: todo cuelga de la toma de notas principal |
| 2026‑08‑06 | DD‑53 | **La planificación vive en el `.lumproj` pero no la gestiona la toma de notas.** Solo la escribe el calendario; al guardar desde una pestaña se conserva releyéndola del disco. Sin esto, el técnico que tuviera el proyecto abierto pisaría al guardar las fechas que otro acababa de mover |
| 2026‑08‑06 | DD‑52 | **Archivar, no ocultar.** Quitar una tarjeta del calendario se guarda en el fichero, no en los ajustes de cada usuario: si cada técnico ocultara lo suyo, el calendario dejaría de ser una foto común. Es reversible y **no borra nada** |
| 2026‑08‑06 | DD‑51 | **Estados: por hacer, en curso, pendiente cliente, terminado.** Es un dato manual y **distinto del avance** que calcula el motor: un proyecto puede estar relleno del todo y seguir «pendiente cliente» |
| 2026‑08‑06 | DD‑50 | **«Muestras recibidas» se guarda como fecha, no como sí/no.** Con la fecha se ve «llegaron hace tres semanas y sigue sin empezar»; con un booleano, no |
| 2026‑08‑05 | DD‑49 | **Las comprobaciones de unificación recorren la carpeta de plantillas**, no una lista de normas escrita en el test. Una lista se queda obsoleta en cuanto se añade una norma, y eso ya pasó con la 62262 |
| 2026‑08‑04 | DD‑48 | **La fila de muestra es idéntica en las tres normas que la usan**, con un test que lo vigila. Lo mismo con los datos de inmersión |
| 2026‑08‑04 | DD‑47 | **Solo la norma principal decide el prefijo de las muestras.** Añadir otra norma ya no las renombra |
| 2026‑08‑04 | DD‑46 | **La clase de aislamiento arranca vacía y hay que elegirla.** Antes el desplegable enseñaba «I» sin que nadie lo hubiera elegido, y un proyecto de Clase II se guardaba como I |
| 2026‑08‑03 | DD‑45 | **Nada de pestañas de dos niveles.** El tablero de gestión es una pestaña más, al lado de los proyectos, y solo puede haber una |
| 2026‑08‑03 | DD‑44 | **Una pestaña por proyecto abierto**, como un navegador. El técnico lleva varios servicios a la vez y antes había que cerrar uno para mirar otro |
| 2026‑08‑02 | DD‑43 | **La aplicación arranca en una portada**: a la izquierda las normas para tomar notas, a la derecha el tablero de gestión. Fuera el selector de norma de la cabecera |
| 2026‑08‑02 | DD‑42 | **El grado IP e IK se elige por muestra** en todas las normas que los piden, con el atajo «Luminaria ordinaria» (IP20). Un mismo servicio puede traer productos con objetivos distintos |
| 2026‑08‑02 | DD‑41 | **El IK deja de ser una toma de notas aparte para luminarias**: se elige por muestra —con «No IK» por defecto— y su sección aparece sola si alguna lo lleva |
| 2026‑08‑02 | DD‑40 | **Lo que se marca como no aplicable no se puede rellenar**: los campos de un apartado o subapartado en N/A se desactivan |
| 2026‑08‑01 | DD‑39 | **La pantalla de luminarias no se toca.** Los campos propios de las demás normas se pintan como tarjetas aparte; en luminarias esa lista está vacía |
| 2026‑08‑01 | DD‑38 | **En la 62262 el grado IP es opcional pero hay que pronunciarse**: o cifras o «Sin grado IP objetivo», excluyentes entre sí |
| 2026‑08‑01 | DD‑37 | **Qué normas se pueden combinar lo declara la plantilla** (`meta.normasCompatibles`). Luminarias no admite la 60529 porque ya lleva el IP dentro; el IP y el IK solo se admiten entre sí |
| 2026‑08‑01 | DD‑36 | **Los ids de bloque de las normas nuevas van prefijados** (`62031.6`, `60529.primeraCifra`). Con varias normas en un proyecto los datos comparten almacén y dos bloques homónimos se pisarían |

Las decisiones de desarrollo (DD‑xx) están en la [sección 13](#13-decisiones-de-desarrollo).

---

## 0. Cómo usar este documento

- Cada regla tiene un **ID estable** (`R-<sección>-<n>`). Ese ID será el que use el código y los tests de la futura aplicación.
- La columna **Origen** indica la celda exacta del libro, para poder auditar la traducción.
- La columna **Expresión original** es la fórmula tal cual está en el Excel.
- ⚠️ marca reglas rotas, ambiguas o sospechosas. Todas están recogidas también en la [sección 11](#11-defectos-incoherencias-y-preguntas-abiertas), que es la lista de trabajo para la revisión.
- Notación usada en las reglas: `V` = verdadero, `F` = falso, `∅` = celda vacía o cero.

---

## 1. Arquitectura funcional del libro

El libro no es una hoja de cálculo: es una aplicación con tres capas mezcladas.

| Capa | Hoja | Contenido |
|---|---|---|
| **Presentación / entrada** | `Toma de notas 60598` | ~56 bloques de ensayo, 161 casillas, campos de medida por muestra, comentarios |
| **Presentación / cabecera** | `RESUMEN PROYECTO LUM` | Datos del proyecto, normas, IP/IK objetivo, identificación de muestras |
| **Lógica** | `Datos ensayos LUM.` | 597 fórmulas: aplicabilidad, validación, cálculos, ponderación de avance |
| **Datos maestros** | `BBDD Equipos 60598` | Catálogo de equipos por ensayo |
| **Vistas derivadas** | `Índice`, `Equipos` | Checklist de apartados aplicables y listado de equipos filtrado |

**Regla estructural:** ninguna celda de `Toma de notas` calcula nada relevante; toda la lógica vive en `Datos ensayos LUM.` y vuelve a la vista como textos de aviso. La aplicación debe conservar esta separación (motor de reglas ≠ formulario).

---

## 2. Modelo de datos

### 2.1 Proyecto

| Campo | Origen | Tipo | Notas |
|---|---|---|---|
| Código de servicio | `RESUMEN!D3` | texto `NNNNNAAAA` | Alimenta `EBP_SAFE<código>` y la cabecera de todas las hojas |
| Técnico 1 / Técnico 2 | `RESUMEN!C5`, `C6` | texto | |
| Nº de muestras | `RESUMEN!D8` | entero 1–8 | Propaga a `Datos!D7` |
| Ta | `RESUMEN!D9` | °C | |
| Clase | `Datos!D8` | 1 / 2 / 3 | Radio; condiciona tierra y doble aislamiento |
| Partes ‑2 aplicables | `Datos!D6:N6` | 10 casillas | ‑2‑1, ‑2‑2, ‑2‑3, ‑2‑4, ‑2‑5, ‑2‑10, ‑2‑13, ‑2‑18, ‑2‑22, OTRO |
| Grado IP objetivo | `Datos!C9:K12` | casillas | 2ª cifra: IPX0…IPX9; 1ª cifra: IP1X…IP6X |
| ~~Grado IK objetivo~~ | `Datos!C14:G15` | casillas | 🚫 fuera de alcance |
| Numeración de muestras | `Datos!G7` (auto) / `I7` (personalizada) | casillas | |
| Inicio de numeración | `RESUMEN!F38` | entero | |
| Firma del registro | comentario en `RESUMEN!D4` | — | ⚠️ Ver [D‑11](#11-defectos-incoherencias-y-preguntas-abiertas) |

### 2.2 Muestras

Máximo 8. Se numeran automáticamente desde `RESUMEN!F38` o de forma manual.

| Regla | Origen | Definición |
|---|---|---|
| `R-MUE-01` | `RESUMEN!B40:B47` | Etiqueta de muestra = `"MUESTRA " & n` si `n ≤ nº muestras`, si no vacío |
| `R-MUE-02` | `RESUMEN!C40:C47` | Identificador = `"EBP_SAFE" & código de servicio` |
| `R-MUE-03` | `RESUMEN!E40:E47` | Nº mostrado = numeración personalizada si `Datos!I7 = V`, si no la automática |
| `R-MUE-04` | `Toma de notas!G1:N1` | La columna de una muestra solo se muestra si `nº muestras ≥ índice de la columna` |

En la aplicación esto deja de ser una regla: las columnas se generan a partir de la lista de muestras.

### 2.3 Bloque de ensayo — patrón común

Todos los bloques de la toma de notas siguen la misma estructura física:

```
fila n     : [código apartado] [título]
fila n+1   : T(ºC)  H(%)  FECHA            ← etiquetas (columnas D, E, F)
fila n+2   : valor  valor  valor            ← datos generales (columnas D, E, F)
fila n+1   :                      G..N      ← fecha por muestra (opcional)
filas ...  : etiqueta de medida (col. F)  →  valores por muestra (col. G..N)
filas ...  : casillas de verificación (col. B/C) enlazadas al motor
última fila: "Comentarios:"  → texto libre
```

**Modelo objetivo:** `BloqueEnsayo { codigo, titulo, aplica, na, condiciones_ambientales{T,H,fecha}, fechas_por_muestra[], campos[], checklists[], equipos[], comentarios }`.

### 2.4 Equipos

Catálogo en `BBDD Equipos 60598`, agrupado por sección de ensayo, con dos columnas por equipo: **código** (`EQ-SAFE-xxx`, `RC-SAFE-xxx`, `EA-CERT-xxx`) y **descripción**. Cada grupo termina en una fila `Otros:` de texto libre.

---

## 3. Convenciones y patrones de reglas

Ocho patrones cubren el 80 % de las 597 fórmulas del motor. Programándolos una vez, el resto es configuración.

### P1 — N/A en cascada

```
na_apartado = na_seccion  OR  na_subapartado  [OR condición_estructural]
```
Ejemplos: `Datos!D55 = OR(D53,D54)`, `Datos!BE39 = OR(BE38,BE20)`, `Datos!CF23 = OR(CF22, NOT(CF20))`.

Tres niveles reales: **sección → subapartado → ensayo concreto**. En las partes ‑2 se añade un cuarto nivel: *no aplica si la parte ‑2 no está marcada en el proyecto*.

### P2 — Aviso de fecha

```
warn_fecha = 1 si la celda de fecha está vacía, si no 0
```
Variante con muestras (`Datos!AJ42`, `AJ47`, `AJ52`, `AJ57`):
```
warn_fecha = 1 si (fecha general = ∅ Y fecha de la muestra 1 = ∅)
```

### P3 — Faltan datos en el apartado

```
faltan_datos = SI na_apartado ENTONCES F
               SI NO (warn_fecha = 1) O (cualquier otra verificación = F)
```
Es el patrón más repetido del libro (≈45 apariciones).

### P4 — Al menos una casilla marcada

```
resultado = OR(casilla_1 … casilla_n)
```
Se usa para: equipos utilizados, opciones excluyentes de checklist y selección de lugar de ensayo.

### P5 — Exactamente una casilla marcada

```
resultado = OR(casillas) Y CONTAR.SI(casillas,"VERDADERO") = 1
```
Origen: `Datos!BU83` (origen de la muestra del ensayo de bola), `Datos!CF27` (altura de montaje), `Datos!BU137` (disposición de la llama).

### P6 — Recuento de datos introducidos

```
introducidos = CONTAR.SI(rango de muestras; ">0")
completo = introducidos ≥ umbral
```
El umbral suele ser `3 × nº de muestras` (P6a) o un número fijo (P6b).
⚠️ Varias fórmulas usan `">=0"` en lugar de `">0"`, lo que cuenta también los ceros → ver [D‑04](#11-defectos-incoherencias-y-preguntas-abiertas).

### P7 — Verificación de duración de acondicionamiento

```
dias_ok  = (fecha_fin − fecha_inicio) ≥ N
horas_ok = (hora_fin − hora_inicio) ≥ 0
verificado = (dias_ok Y horas_ok) O (fecha_fin − fecha_inicio) > N
```
Con `N = 2` para 48 h, `N = 1` para 24 h, `N = 10` para 240 h.

**Es correcto.** Aunque separa días y horas, el resultado es equivalente a `duración ≥ N días`:

| ΔD | ΔH | Duración | P7 | Correcto |
|---|---|---|---|---|
| = N | ≥ 0 | ≥ N días | acepta | ✔ |
| = N | < 0 | < N días | rechaza | ✔ |
| > N | cualquiera | > N días (porque \|ΔH\| < 24 h) | acepta | ✔ |
| < N | cualquiera | < N días | rechaza | ✔ |

El mínimo que acepta son exactamente N días. **En la aplicación se sustituye por una comparación directa de `datetime`**, que da el mismo resultado y se lee mejor:

```
verificado = (fin − inicio) ≥ N días
```

⚠️ El defecto real de comparación temporal está en el ensayo de bola, no aquí. Ver [D‑05](#11-defectos-incoherencias-y-preguntas-abiertas).

### P8 — Peso de avance

```
peso_en_proyecto = peso_general × ¿aplica?
peso_finalizado  = peso_en_proyecto × ¿terminado?
¿aplica?    = NO(na_apartado)
¿terminado? = NO(faltan_datos)
```

---

## 4. Catálogo de entradas

El motor tiene **179 celdas de entrada** enlazadas a casillas/opciones, más los campos numéricos de la toma de notas. Ninguna otra celda del motor es editable.

| Origen del control | Nº | Destino |
|---|---|---|
| `Toma de notas` | 143 | `Datos ensayos LUM.` |
| `RESUMEN` | 36 | `Datos ensayos LUM.` |
| `Índice` | 47 | la propia hoja `Índice` |
| `Toma de notas` / `RESUMEN` | 1 / 4 | `#REF!` — controles huérfanos de IK/62031, 🚫 fuera de alcance |

Reparto por tipo: 233 casillas, 45 opciones (radio) en 19 grupos, 19 marcos de agrupación.

**En el alcance del borrador:** 171 entradas del motor (las 179 menos las 8 de IK: `U6`, `C14`, `E14`, `G14`, `I14`, `C15`, `E15`, `G15`) y ninguno de los 5 controles huérfanos.

### Entradas que no son controles de formulario ⚠️ D‑22

El inventario anterior cubre solo los controles heredados. El libro usa **además otros dos mecanismos**, descubiertos al modelar el MVP:

| Mecanismo | Nº en `Toma de notas` | Descripción |
|---|---|---|
| **Casillas nativas de celda** (función de Excel 2024) | 27 | Celdas booleanas leídas directamente por el motor: `B849`‑`B851`, `B854`, `B861`, `B875`, `B876`, `B880`, `B884` (ensayo de bola), `B919` (aguja), `C927`/`D927` (disposición de la llama), `G930:N930` (no combustión), `B988` (caminos en papel), `B116`/`B118` (7.9), `B1069:B1072` (método IK) |
| **Casillas sin enlazar** | 17 | Se marcan pero **no alimentan ninguna regla**: `15s agua` y `15s hexano` (marcado), `No blando` / `No aislante` de cada tornillo, `Sección mayor especificada en sec. 5`, `En envolvente de la luminaria…`, `RESERVADO A AMPLIACIÓN` |

Total real de entradas en la hoja de trabajo: **188**, de las cuales 44 eran invisibles contando solo controles de formulario. Las 17 sin enlazar son datos de ensayo que hoy no se validan ni se agregan: en la aplicación pasan a ser campos normales.

---

## 5. Reglas por sección

> **Numeración canónica: edición 2024** (decisión de 2026‑07‑29), la misma que usa la hoja de toma de notas. La numeración antigua que aparece en `Índice`, en el panel de pesos y en parte de las hojas de equipos se conserva **solo como alias de visualización** y se traduce con la tabla siguiente.

### Equivalencia de numeraciones

| Antigua (Índice, pesos) | Canónica 2024 | Apartado |
|---|---|---|
| 3 | **6** | Marcado |
| 4 | **7** | Construcción |
| 4.4 / 4.6 / 4.7.2 / 4.9 / 4.10 | 7.4 / 7.6 / 7.7.2 / 7.9 / 7.10 | Portalámparas, bloques de conexión, bornes de red, revestimientos, doble aislamiento |
| 4.12 / 4.13 / 4.14 / 4.18 | 7.12 / 7.13 / 7.14 / 7.18 | Tornillos, impacto, suspensiones y carga, corrosión |
| 4.24.2 / 4.28 | 7.24.2 / 7.28 | Luz azul, fijación de controles térmicos |
| 5 / 5.2 | **8** / 8.2 | Cableado externo e interno ⚠️ el rótulo antiguo dice «interno» y el nuevo «externo»: confirmar |
| 7 | **9** | Continuidad del circuito de tierra |
| 8 | **10** | Protección contra choque eléctrico |
| 9 / 9.3 | **11** / 11.2, 11.3 | IP y humedad |
| 10 / 10.2.1 / 10.2.2 / 10.3 | **12** / 12.2.2 / 12.2.3 / 12.3 | Resistencia de aislamiento, rigidez, corrientes de fuga |
| 11 / 11.2 | **13** / 13.2 | Líneas de fuga y distancias en el aire |
| 12 | **14** | Endurancia y calentamientos |
| 13 / 13.2 / 13.3 | **15** / 15.2 / 15.3 | Resistencia al calor, la llama y los caminos conductores |
| *(sin equivalente antiguo)* | **16 / 17** | Bornes con y sin tornillo |
| Anexo A | Anexo A | Partes activas |

Regla práctica: secciones 3–5 → **+3**; secciones 7–13 → **+2**. La aplicación almacena únicamente el código canónico; si el laboratorio necesita ver la numeración antigua en algún informe, se resuelve con esta tabla, nunca duplicando datos.

### 5.1 Ratings y datos generales

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-GEN-01` | `Datos!D21` | `=OR(E9,E10)` | Hace falta el tamaño de la muestra si el objetivo es **IPX3 o IPX4** |
| `R-GEN-02` | `Datos!D22` | `=IF(OR(D21=FALSE),0,3*D7)` | Datos de tamaño necesarios = 3 por muestra (alto, ancho, profundo); 0 si no hace falta tamaño |
| `R-GEN-03` | `Datos!D23` | `=COUNTIF('Toma de notas'!G17:N19,">0")` | Datos de tamaño introducidos |
| `R-GEN-04` | `Datos!D25` | `=IF(AND(D24<>0,D21=TRUE),1,0)` | Mostrar aviso si faltan datos de tamaño **y** son necesarios |
| `R-GEN-05` | `Datos!D27` | `=IF('Toma de notas'!F5>0,0,1)` | P2 sobre la fecha de ratings |
| `R-GEN-06` | `Datos!D29` | `=OR(D25,D27)` | Faltan datos en generales |

Campos de medida asociados: tensión/corriente/potencia/factor de potencia para **tensión 1** (`G7:N10`) y **tensión 2** si procede (`G12:N15`); tamaño de muestra (`G17:N19`).

### 5.2 Partes activas (Anexo A)

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-PA-01` | `Datos!M21` | `=IF(L13=L14,1,0)` | Incoherencia SELV: las casillas «SELV SÍ» y «SELV NO» valen lo mismo (ambas marcadas o ninguna) |
| `R-PA-02` | `Datos!M22` | `=IF(L21=TRUE,0,IF(L15=L16,1,0))` | Si no es SELV, comprobar la coherencia de «¿es parte activa?» |
| `R-PA-03` | `Datos!L24` | `=IF(L21=TRUE,0,3*D7)` | Si es SELV no hacen falta medidas; si no, 3 por muestra (Vac, Vdc, Uout) |
| `R-PA-04` | `Datos!L25` | `=COUNTIF('Toma de notas'!G31:N33,">0")` | Medidas introducidas |
| `R-PA-05` | `Datos!L31` | `=IF(L20=TRUE,FALSE,OR(L27,L29))` | P3 de la sección |

### 5.3 Sección 6 — Marcado

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-06-01` | `Datos!D40` | `=IF('Toma de notas'!F45=0,1,0)` | P2 |
| `R-06-02` | `Datos!D42` | `=IF(D38=TRUE,FALSE,IF(D40=1,TRUE,FALSE))` | P3. Único requisito: la fecha. Los datos van directos al TRF |

### 5.4 Sección 7 — Construcción

Cada subapartado repite P1 + P2 + P3. `Datos!K39` es el N/A de toda la sección y entra en el N/A de **todos** los subapartados.

#### 7.4 Portalámparas

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.04-01` | `Datos!D55` | `=OR(D53,D54)` | P1 |
| `R-07.04-02` | `Datos!D64` | `=OR(B60,B61,B62,B63,C60,C61,C62,C63)` | P4 sobre 8 casillas de tipo de portalámparas |
| `R-07.04-03` | `Datos!D66` | `=IF(D55=TRUE,FALSE,OR(D57,IF(D64=FALSE,TRUE,FALSE)))` | P3 |

#### 7.6 y 7.7.2 Bornes y conexiones

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.06-01` | `Datos!D84` | `=OR(D81,B83,C83)` | Ensayo de vena: marcada la opción N/A o alguna de las dos opciones de resultado |
| `R-07.06-02` | `Datos!D85` | `=IF(D77=TRUE,FALSE,OR(IF(D84=FALSE,TRUE,FALSE)))` | Faltan datos del ensayo de vena |
| `R-07.06-03` | `Datos!L78` | `=IF(COUNTIF('Toma de notas'!G96:N98,">=0")>=3,TRUE,FALSE)` | Hay dimensiones del bloque de conexión del fabricante ⚠️ D‑04 |
| `R-07.06-04` | `Datos!L79` | `=AND(L77,L78)` | Opción «dimensiones de fabricante» completa = opción marcada **y** dimensiones introducidas |
| `R-07.06-05` | `Datos!L80` | `=OR(L75,L76,L79)` | Bloque de conexión resuelto por una de las tres vías: N/A, dimensiones por defecto, o dimensiones de fabricante completas |
| `R-07.06-06` | `Datos!L84` | `=IF(D84+L80=2,TRUE,FALSE)` | Apartado completo = vena resuelta **y** bloque de conexión resuelto |
| `R-07.06-07` | `Datos!L85` | `=IF(D77=TRUE,FALSE,OR(D79,IF(L84=FALSE,TRUE,FALSE)))` | P3 |

#### 7.9 Revestimientos y manguitos aislantes

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.09-01` | `Datos!D103` | `='Toma de notas'!B116` | Casilla «no son necesarios los ensayos a) ni b)» |
| `R-07.09-02` | `Datos!D110` | `=IF(D108+D109=0,FALSE,TRUE)` | P4 sobre los equipos EQ‑CERT‑305 / EQ‑CERT‑304 ⚠️ D‑07 |
| `R-07.09-03` | `Datos!B117:B119` | P7 con N = 2 | Acondicionamiento de humedad de 48 h (`G122`–`G125`) |
| `R-07.09-04` | `Datos!B123:B125` | P7 con N = 10 | Permanencia en estufa 240 h (10 días) (`G132`–`G135`) |
| `R-07.09-05` | `Datos!B119` / `B125` | `=IF(OR(D103, AND(...)),TRUE,...)` | Si a) y b) no aplican, ambas verificaciones se dan por correctas |
| `R-07.09-06` | `Datos!D112` | `=IF(D99,F,OR(D101,IF(OR(D110=F,B119=F,B125=F),V,F)))` | P3: exige fecha, equipo y las dos verificaciones de tiempo |

Parámetros fijos del ensayo: temperatura programada 25 °C, humedad programada 93 %.

#### 7.10 Aislamiento doble o reforzado

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.10-01` | `Datos!L99` | `=IF(D8=1,TRUE,FALSE)` | **No aplica si la luminaria es Clase I** |
| `R-07.10-02` | `Datos!L100` | `=OR(L97,L98,L99)` | P1 con la condición estructural anterior |
| `R-07.10-03` | `Datos!L105` | `=IF(L100=TRUE,FALSE,OR(L102))` | P3, solo fecha |

#### 7.12 Tornillos, uniones y prensaestopas

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.12-01` | `Datos!D141` | `=IF(COUNTIF('Toma de notas'!G169:N178,">0")>=1,TRUE,FALSE)` | Hay datos de tornillos (hasta 5 tornillos × parte y diámetro) |
| `R-07.12-02` | `Datos!D143` | `=IF(D140=TRUE,FALSE,IF(D141=TRUE,FALSE,TRUE))` | Faltan datos de tornillos salvo que se marque «luminaria sin tornillos» |
| `R-07.12-03` | `Datos!D147` / `D149` | ídem con `G190:N195` | Uniones atornilladas (hasta 3) |
| `R-07.12-04` | `Datos!L138` | `=OR(J135,J136,J137,K135,K136)` | P4 sobre las opciones de portalámparas de 7.12 |
| `R-07.12-05` | `Datos!L139` | `=NOT(L138)` | Faltan datos de portalámparas |
| `R-07.12-06` | `Datos!L147` | `=OR(L142,J144,K144,L144,J145,K145,L145)` | Prensaestopas: marcado «sin prensaestopas» o al menos un material (metal/plástico × 3 posiciones) |
| `R-07.12-07` | `Datos!L148` | `=COUNTIF(J144:L145,"VERDADERO")` | Nº de prensaestopas declarados |
| `R-07.12-08` | `Datos!L149` | `=COUNTIF('Toma de notas'!G220:N222,">0")` | Diámetros introducidos |
| `R-07.12-09` | `Datos!L151` | `=IF(L142,F,IF(AND(L147,L149>=L148),F,V))` | Faltan datos si los diámetros introducidos no cubren los prensaestopas declarados |
| `R-07.12-10` | `Datos!L153` | `=IF(D143+D149+L139+L151=4,FALSE,TRUE)` | ⚠️ D‑06: suma de cuatro booleanos comparada con 4 |
| `R-07.12-11` | `Datos!L154` | `=IF(D136,F,OR(D138,IF(L153=F,V,F)))` | P3 del apartado completo |

#### 7.13 Resistencia mecánica (impacto) y 7.14.1 Carga

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.13-01` | `Datos!D171` | `=IF(D167,F,IF(D169=1,V,F))` | P3, solo fecha (datos al TRF) |
| `R-07.14-01` | `Datos!L171` | `=IF(L167,F,IF(L169=1,V,F))` | P3, solo fecha |

Campos: compresión según tabla 4.13, 3 partes frágiles + 3 otras partes, peso de luminaria y de parte.

#### 7.18.1 Resistencia a la corrosión

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.18.1-01` | `Datos!C190/E190/G190` | radio | Lugar del ensayo: estufa / cámara / otro |
| `R-07.18.1-02` | `Datos!D194` | `=OR(D192,D193)` | Solución: al 10 % u otra |
| `R-07.18.1-03` | `Datos!D196` | `=IF(COUNTIF('Toma de notas'!G278:N280,">=0")>=3,TRUE,FALSE)` | Hay 3 temperaturas (solución 20±5 °C, aire en caja 20±5 °C, estufa 100±5 °C) ⚠️ D‑04 |
| `R-07.18.1-04` | `Datos!D198` | `=IF(D186,F,IF(OR(D188=1,D196=F),V,F))` | P3 |

⚠️ Las tolerancias (20 ± 5 °C, 100 ± 5 °C) **no se validan**; solo se comprueba que haya un número. Ver [D‑08](#11-defectos-incoherencias-y-preguntas-abiertas).

#### 7.18.2 Season cracking (cobre)

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.18.2-01` | `Datos!L190` | `=IF(COUNTIF('Toma de notas'!G293:N294,">0")>=2,TRUE,FALSE)` | Hay preacondicionamiento (24 h) y pH de la solución de NH₄Cl |
| `R-07.18.2-02` | `Datos!L192` | `=IF(L186,F,IF(OR(L188=1,L190=F),V,F))` | P3 |

#### 7.24.2 Riesgo por luz azul

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.24-01` | `Datos!D220` | `=OR(D216,D217)` | ¿Es RG0 o RG1? |
| `R-07.24-02` | `Datos!D223` | `=IF(D218=FALSE,TRUE,IF(OR(D221,D222),TRUE,FALSE))` | Si es **RG2** hace falta o datos externos del LED o ensayo propio |
| `R-07.24-03` | `Datos!L210` | `=IF(COUNTIF(D216:D218,"VERDADERO")>0,TRUE,FALSE)` | Se ha marcado algún grupo de riesgo |
| `R-07.24-04` | `Datos!L211` | `=AND(L210,D223)` | Selección coherente |
| `R-07.24-05` | `Datos!L213` | `=IF(D212,F,IF(OR(D214=1,L211=F),V,F))` | P3 |

Campos para RG2: CCT (K), iluminancia umbral RG1/RG2 (lux), d_min (m).

#### 7.28 Fijación de controles sensibles a la temperatura

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.28-01` | `Datos!D241` | `=IF(COUNTIF('Toma de notas'!G327:N329,">=0")+COUNTIF('Toma de notas'!G334:N336,">=0")>=6,TRUE,FALSE)` | Hay ≥ 6 datos de configuración: severidad (T superior, T inferior 0 °C, 100 ciclos) y condiciones (30 min exposición, tiempo de transferencia, nº de ciclos) ⚠️ D‑04 |
| `R-07.28-02` | `Datos!D244` | `=OR(B243,C243)` | Se ha indicado si el equipo está en funcionamiento durante el ensayo |
| `R-07.28-03` | `Datos!D246` | `=IF(D237,F,IF(OR(D239=1,D244=F,D241=F),V,F))` | P3 |

#### 7.33 y 7.35

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07.33-01` | `Datos!L242` | `=IF(L237,F,OR(L239))` | P3, solo fecha (alimentación por cableado de comunicación) |
| `R-07.35-01` | `Datos!D265` | `=IF(D260,F,OR(D262))` | P3, solo fecha (protección de aspas de ventilador) |

#### Resumen de la sección

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-07-RES` | `Datos!L39` | `=OR(L40,L41,L42,L43,L44,L48,L50,L51,L52,L53,L54,L56,L58)` | Faltan datos en la sección si faltan en cualquiera de sus 13 subapartados. ⚠️ Comentario del autor en `M39`: *«cuando se incorporen las partes ‑2 habrá que añadir más casillas»* |

### 5.5 Sección 8 — Cableado

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-08-01` | `Datos!AB25` | `=IF(AB22,F,IF(AB23=1,V,F))` | Dispositivo de anclaje: P3 sobre la fecha |
| `R-08-02` | `Datos!AB31` | `=AND(Z30,AA30)` | Conductores de sección reducida: **ambas** casillas de resultado marcadas |
| `R-08-03` | `Datos!AB32` | `=IF(AB27,F,IF(OR(AB28=1,AB31=F),V,F))` | P3 del subapartado 8.4 |
| `R-08-04` | `Datos!AB36` | `=OR(AB25,AB32)` | Faltan datos en algún subapartado |
| `R-08-05` | `Datos!AB37` | `=IF(AB20,F,IF(AB36,V,F))` | P3 de la sección |

Campo asociado: sección nominal total de conductores para hasta 3 dispositivos (`G597:N599`).

### 5.6 Sección 9 — Tierra

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-09-01` | `Datos!T21` | `=IF(D8<>1,TRUE,FALSE)` | **No aplica si la luminaria no es Clase I** |
| `R-09-02` | `Datos!T22` | `=OR(T20,T21)` | P1 |
| `R-09-03` | `Datos!T31` | `=OR(T28,T29)` | P4: EQ‑SAFE‑442 (Elabo) o EQ‑SAFE‑403 (GW Instek) |
| `R-09-04` | `Datos!T34` | `=IF(T22,F,IF(OR(T24=1,T31=F),V,F))` | P3 |
| `R-09-05` | `Datos!T26` | `=IF(COUNTIF('Toma de notas'!G577:N577,">0")>1,TRUE,FALSE)` | ⚠️ Calculada pero **no usada**; el autor anota que el valor (Ω) puede ir directo al TRF |

Condiciones fijas documentadas: corriente 10 A, duración 1 min.

### 5.7 Sección 10 — Protección contra choque eléctrico

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-10-01` | `Datos!AB53` | `=OR(AB49,AB50,AB51,AB52)` | P4 sobre EQ‑SAFE‑437 / ‑428 / ‑430 / ‑409 |
| `R-10-02` | `Datos!AB56` | `=IF(AB44,F,IF(OR(AB46=1,AB53=F),V,F))` | P3 |

### 5.8 Sección 11 — IP y humedad

Es la sección con más lógica propia. Se divide en tres apartados independientes: **2ª cifra** (agua), **1ª cifra** (polvo/sólidos) y **humedad 48 h**.

#### Cálculo del arco de lluvia (por muestra)

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-11-01` | `Datos!AQ25` | `=IF($E$10,'Toma de notas'!G17/2,'Toma de notas'!G17)` | Altura H: **se toma la mitad si el objetivo es IPX4** |
| `R-11-02` | `Datos!AQ29` | `=SQRT((A/2)^2+(P/2)^2)` | Semidiagonal de la base `d1` |
| `R-11-03` | `Datos!AQ30` | `=SQRT(H^2+d1^2)` | Distancia `d` desde el centro del arco |
| `R-11-04` | `Datos!AQ32` | `=d+20` | Radio máximo necesario `Rmáx` (cm) |
| `R-11-05` | `Datos!AQ33` | `IFS` sobre 20/40/60/80/100/120/140 cm | Radio de ensayo = el arco disponible que cubre `Rmáx`; si `Rmáx > 140` ⇒ **«Cab. Regadera»**; si `Rmáx = 20` (sin dimensiones) ⇒ N/A |
| `R-11-06` | `Datos!AV35` | `=IF(COUNTIF(AQ33:AX33,"Cab. Regadera")>0,TRUE,FALSE)` | Alguna muestra requiere cabeza de regadera |
| `R-11-07` | `Datos!AQ41` | `=IF($AR$35=TRUE,AQ33,"N/A")` | El radio solo se traslada a la toma de notas si el objetivo es IPX3/IPX4 |
| `R-11-08` | `Toma de notas!C731` | `=IF('Datos'!AR46=TRUE,"Ojo, equipo muy grande…DESVIACIÓN AL MÉTODO","")` | **Aviso de desviación al método** cuando se usa cabeza de regadera |

#### Selección automática de equipo de ensayo IP

| ID | Origen | Condición | Equipo |
|---|---|---|---|
| `R-11-09` | `Datos!AR45` | `=OR(C9,C10)` → IPX1 o IPX2 | EQ‑SAFE‑615 (caja de goteo) |
| `R-11-10` | `Datos!AR46` | IPX3/IPX4 **y** muestra grande | EQ‑SAFE‑111 (cabeza de regadera) |
| `R-11-11` | `Datos!AR47` | IPX3 o IPX4 | EQ‑SAFE‑174 (arcos) |
| `R-11-12` | `Datos!AR48` | IPX5 o IPX6 | EQ‑SAFE‑105 (boquillas) |
| `R-11-13` | `Datos!AR49` | IPX7 o IPX8 | EA‑CERT‑176+177 (depósitos) |
| `R-11-14` | `Datos!AR50` | IPX9 | EQ‑SAFE‑232 |
| `R-11-15` | `Datos!AV70` | `=OR(G11,G12)` → IP5X o IP6X | Cámara de polvo |

#### Validación de datos

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-11-16` | `Datos!AR59` | `=IF(COUNTIF(B9:K10,"VERDADERO")>0,TRUE,FALSE)` | Hay grado marcado para la 2ª cifra |
| `R-11-17` | `Datos!AR36` | `=IF(COUNTIF(AQ25:AX27,">0")>=3,TRUE,FALSE)` | Hay dimensiones de al menos una muestra |
| `R-11-18` | `Datos!AR60` | `=IF(AR35,IF(AR36,V,F),V)` | Si es IPX3/IPX4 son obligatorias las dimensiones; en otro caso se da por bueno |
| `R-11-19` | `Datos!AR62` | `=IF(AR55,F,IF(OR(AR57=1,AR59=F,AR60=F),V,F))` | Faltan datos de la 2ª cifra ⚠️ D‑03 (etiquetas cruzadas) |
| `R-11-20` | `Datos!AR70` | `=IF(COUNTIF(B11:G12,"=VERDADERO")>0,TRUE,FALSE)` | Hay grado marcado para la 1ª cifra |
| `R-11-21` | `Datos!AR72` | `=IF(AR67,F,IF(OR(AR69=1,AR70=F),V,F))` | Faltan datos de la 1ª cifra ⚠️ D‑03 |
| `R-11-22` | `Datos!AP86:AP88` | P7 con N = 2 | Ensayo de humedad 48 h (`G753`–`G756`); 25 °C y 93 % programados |
| `R-11-23` | `Datos!AR80` | `=IF(AR76,F,IF(OR(AR78=1,AP88=F),V,F))` | Faltan datos de humedad |
| `R-11-24` | `Datos!AR83` | `=IF(AR20,F,IF(OR(AR72,AR62,AR80),V,F))` | Faltan datos en la sección |

### 5.9 Sección 12 — Aislamiento, rigidez y corrientes de fuga

Tres subapartados con la misma estructura, cada uno con su propia selección de equipo:

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-12-01` | `Datos!BH24:BH30` | `=OR(BE_,BF_,BG_)` | Un equipo está «en uso» si se marca para aislamiento, rigidez o fuga |
| `R-12-02` | `Datos!BE32/BF32/BG32` | `=OR(equipos de la columna)` | P4 por subapartado |
| `R-12-03` | `Datos!BE34` | `=AND(G6,D8=1)` | Si es **‑2‑4 (portátil) y Clase I** ⇒ avisar de que hay que usar la red especial Kikusui para corrientes de fuga |
| `R-12-04` | `Datos!BE42` | `=IF(BE39,F,IF(OR(BE40=1,BE32=F),V,F))` | P3 resistencia de aislamiento (500 V, 1 min) |
| `R-12-05` | `Datos!BE50` | `=IF(BE47,F,IF(OR(BE48=1,BF32=F),V,F))` | P3 rigidez dieléctrica |
| `R-12-06` | `Datos!BE58` | `=IF(BE55,F,IF(OR(BE56=1,BG32=F),V,F))` | P3 corrientes de fuga |
| `R-12-07` | `Datos!BE61` | `=OR(BE42,BE50,BE58)` | Faltan datos en la sección |

Equipos del grupo: EQ‑SAFE‑403 (GPT‑15004), ‑404 (GLC‑1000), ‑402 (GPT‑15002), ‑441 (AINUO), reservado COPPER‑BS y dos previsiones.

**Regla documental** (texto fijo, no calculado): los puntos de aplicación de rigidez son los mismos que los de resistencia de aislamiento; para SELV/PELV se asume polaridad entre polos 1 y 2; para el resto, L1‑N (o L1/L2/L3‑N).

### 5.10 Sección 13 — Líneas de fuga y distancias en el aire

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-13-01` | `Datos!BN28` | `=IF(BN20,F,IF(AND(BN22,BN23),F,IF(BN25=1,V,F)))` | No faltan datos si se marcan N/A **las dos tablas** (tabla 1 y tabla 2); si no, exige fecha |

### 5.11 Sección 14 — Calentamiento y endurancia

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-14-01` | `Datos!AJ29` | `=OR(AJ23,AJ24,AJ25,AJ26,AJ27)` | P4 sobre los **lugares** de ensayo (EQ‑SAFE‑301, ‑302, ‑328, ‑303, ampliación 2) |
| `R-14-02` | `Datos!AJ32/AJ33` | `=AJ23` / `=AJ24` | Los equipos EQ‑SAFE‑327 y EQ‑SAFE‑3xx se seleccionan **automáticamente** al elegir su lugar |
| `R-14-03` | `Datos!AJ39` | `=OR(AJ32…AJ37)` | P4 sobre los **equipos** de ensayo |
| `R-14-04` | `Datos!AJ42/47/52/57` | P2 con muestras | Fecha de endurancia / calentamiento normal (tc) / anormal / condiciones de fallo |
| `R-14-05` | `Datos!AJ43/48/53/58` | P3 por subapartado | |
| `R-14-06` | `Datos!AJ61` | `=IF(AJ20,F,OR(AJ43,AJ48,AJ53,AJ58,NOT(AJ29),NOT(AJ39)))` | Faltan datos en la sección: cualquier subapartado incompleto **o** sin lugar **o** sin equipo |

Nota del libro: los registros primarios de estos apartados están en los *excel de calentamiento* separados.

### 5.12 Sección 15 — Resistencia al fuego

La sección con las verificaciones más detalladas: 4 ensayos (bola, llama de aguja, hilo incandescente, caminos conductores).

#### 15.2.1 Ensayo de bola

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-15.2-01` | `Datos!BU83` | P5 sobre `B849:B851` | Origen de la muestra: corte del EBP / muestra separada / producto acabado — **exactamente uno** |
| `R-15.2-02` | `Datos!BU85` | `='Toma de notas'!B854` | Se verifica el acondicionamiento de 24 h en laboratorio (15–35 °C) |
| `R-15.2-03` | `Datos!BU88` | `=IF(BU87=F,V,IF(AND(BU87,G862<>0),V,F))` | Si se apilan capas, hay que indicar el número de capas |
| `R-15.2-04` | `Datos!BU91` | `=IF(G864>=2.5,TRUE,FALSE)` | Espesor ≥ 2,5 mm |
| `R-15.2-05` | `Datos!BU92/93` | `=IF(G865>=10,…)` / `=IF(G866>=10,…)` | Largo y ancho ≥ 10 mm |
| `R-15.2-06` | `Datos!BU94` | `=AND(BU91:BU93)` | Dimensiones correctas |
| `R-15.2-07` | `Datos!BU99` | `=IF((G870−G869)*1440<180,FALSE,TRUE)` | Acondicionamiento en horno **≥ 180 min** ⚠️ D‑05: comparación de solo hora |
| `R-15.2-08` | `Datos!BU103` | `=IF(OR((fin−inicio)<58,(fin−inicio)>62),FALSE,TRUE)` | Duración del ensayo **60 ± 2 min** ⚠️ D‑05: comparación de solo hora |
| `R-15.2-09` | `Datos!BU105` | `=IF(G868<>0,TRUE,FALSE)` | Hay temperatura de termopar (tolerancia documental ± 2 °C, no validada) |
| `R-15.2-10` | `Datos!BU107` | `=AND(BU99,BU103,BU105)` | Tiempos y temperatura correctos |
| `R-15.2-11` | `Datos!BU111` | `=AND(B875,B876,B880,B884)` | Checklist de procedimiento: introducción < 30 s, recuperación de temperatura ≤ 5 min, inmersión en agua, 4–8 min sumergida |
| `R-15.2-12` | `Datos!BU114` | `=IF(OR(G886<15,G886>25),FALSE,TRUE)` | Temperatura del agua **15–25 °C** |
| `R-15.2-13` | `Datos!BU115` | `=IF(OR(G887<4,G887>8),FALSE,TRUE)` | Tiempo en agua **4–8 min** |
| `R-15.2-14` | `Datos!BU121` | P7 con N = 1 | Acondicionamiento previo de 24 h |
| `R-15.2-15` | `Datos!BU125` | `=AND(BU116,BU111,BU107,BU105,BU103,BU99,BU94,BU88,BU85,BU83,BU121)` | **Verificación global**: todas las anteriores |
| `R-15.2-16` | `Datos!BW27` | `=IF(BW23,F,IF(OR(BW25=1,NOT(BU125)),V,F))` | P3 del ensayo de bola |

#### 15.3.1 Llama de aguja

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-15.3.1-01` | `Datos!BU134` | P7 con N = 1 | Acondicionamiento 24 h |
| `R-15.3.1-02` | `Datos!BU137` | P5 sobre `C927`/`D927:E927` | Disposición de la llama: una sola opción |
| `R-15.3.1-03` | `Datos!BU139/140/141` | `=IF(BU139<>BU140,TRUE,FALSE)` | Coherencia: o hay tiempo de combustión `tb` o se marca «no combustión», nunca ambos ni ninguno |
| `R-15.3.1-04` | `Datos!BW41` | `=AND(BW36…BW40)` | Checklist de 5 verificaciones previas completa |
| `R-15.3.1-05` | `Datos!BU145` | `=AND(BU134,BU137,BU141)` | Verificación global |
| `R-15.3.1-06` | `Datos!BW43` | `=IF(BW31,F,IF(OR(BW33=1,BW41=F,NOT(BU145)),V,F))` | P3 |

Criterios documentales (texto fijo): aplicación 10 s; sin inflamación del papel; extinción < 30 s tras retirar la llama.

#### 15.3.2 Hilo incandescente

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-15.3.2-01` | `Datos!BX53` | `=IF(AND(G960>=4−0.07,G960<=4+0.07),TRUE,FALSE)` | Diámetro del hilo **4 ± 0,07 mm** |
| `R-15.3.2-02` | `Datos!BW58` | `=IF(AND(G964<>0,G965<>0,G966<>0),TRUE,FALSE)` | Hay espesor, ancho y largo de la muestra |
| `R-15.3.2-03` | `Datos!BU168` | P7 con N = 1 | Acondicionamiento 24 h |
| `R-15.3.2-04` | `Datos!BW57` | `=AND(BW52…BW56,BX53,BW58)` | Verificación global |
| `R-15.3.2-05` | `Datos!BW60` | `=IF(BW47,F,IF(OR(BW49=1,BW57=F),V,F))` | P3 |

#### 15.4 Formación de caminos conductores

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-15.4-01` | `Datos!BW68` | `=IF(AND(BW63=FALSE,'Toma de notas'!B988),TRUE,FALSE)` | Si el ensayo aplica, hay que marcar que las notas se toman **en papel** |
| `R-15.4-02` | `Datos!BW67` | `=IF(BW63,F,IF(OR(BW65=1,BW68=F),V,F))` | P3 |

| `R-15-TOT` | `Datos!BW70` | `=OR(BW27,BW43,BW60,BW67)` | Faltan datos en la sección de fuego |

### 5.13 Secciones 16 y 17 — Bornes

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-16-01` | `Datos!BN45` | `=OR(BN41,BN44)` | P1 (N/A conjunto de 16 y 17, más N/A de bornes de tornillo) |
| `R-16-02` | `Datos!BN48` / `BN51` | P3 | Requisitos generales (16.1‑16.2) y ensayos mecánicos (16.3): solo fecha |
| `R-16-03` | `Datos!BN54` | `=OR(BN48,BN51)` | Faltan datos en la sección 16 |
| `R-17-01` | `Datos!BN63` / `BN69` / `BN75` | P3 | Requisitos generales (17.1‑17.2), cableado interno (17.4) y externo (17.5) ⚠️ D‑09 |
| `R-17-02` | `Datos!BN77` | `=OR(BN63,BN69,BN75)` | Faltan datos en la sección 17 |
| `R-1617-01` | `Datos!BR41` | `=OR(BN54,BN77)` | Faltan datos en 16 o 17 |

### 5.14 Partes ‑2

Todas las partes ‑2 comparten el patrón: `aplica_parte = casilla de la parte en el proyecto` y cada ensayo añade su propio N/A.

| Parte | Ensayos cubiertos | Celda «aplica» | Resultado |
|---|---|---|---|
| ‑2‑1, ‑2‑2 | *(sin requisitos adicionales)* | `D6`, `E6` | — |
| ‑2‑3 | Carga estática (3.6.3.1), rotura en fragmentos (3.6.5.1), cristal de gran resistencia (3.6.5.2), puerta de acceso (3.6.8) | `CF20 = F6` | `CF60` |
| ‑2‑4 | Estabilidad plano inclinado 6°/15° (4.7.3) | `CF68 = G6` | `CF75` |
| ‑2‑5 | Carga estática (5.6.5), rotura (5.6.8.1), cristal (5.6.8.2) | `CM35 = H6` | `CM63` |
| ‑2‑10 | Estabilidad 15° (10.6.2), impacto y caída (10.6.3) | `CM68 = J6` | `CM85` |
| ‑2‑13 | Carga estática (13.6.1) | `CM89 = K6` | `CM96` |
| ‑2‑18 | Impacto mecánico (18.7.2), corrosión (18.7.3) | `CF89 = L6` | `CF108` |
| ‑2‑22 | Mantenimiento del nivel (22.17.3), funcionamiento a alta temperatura (22.19) | `CF132 = M6` | `CF154` |

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-P2-01` | `Datos!CF23` (y análogas) | `=OR(CF22,NOT(CF20))` | Un ensayo de parte ‑2 no aplica si se marca su N/A **o si la parte ‑2 no está en el proyecto** |
| `R-P2-02` | `Datos!CF27` | `=COUNTIF(CF26:CH26,"VERDADERO")` | Altura de montaje: exactamente una de < 8 m / 8‑15 m / > 15 m |
| `R-P2-03` | `Datos!CF33` | `=IF(CF23,F,IF(OR(CF31=1,CF27<>1),V,F))` | Faltan datos de carga estática si no hay fecha o no hay exactamente una altura marcada |
| `R-P2-04` | `Datos!CM41` | `=IF(COUNTIF('Toma de notas'!G427:N427,">0")=0,1,0)` | ‑2‑5: hace falta el área de la muestra |
| `R-P2-05` | `Datos!CF135` | casilla | ‑2‑22: el mantenimiento del nivel **puede subcontratarse**, y entonces se trata como N/A |
| `R-P2-06` | `Datos!CF140` | `=IF(COUNTIF('Toma de notas'!G511:N514,">=0")>=4,0,1)` | ‑2‑22: hacen falta 4 medidas en modo emergencia (duración de batería, iluminancia a 5 s, 60 s y tras la duración) ⚠️ D‑04 |
| `R-P2-07` | `Datos!CF149` | `=IF(COUNTIF('Toma de notas'!G526:N528,">=0")>=3,0,1)` | ‑2‑22: hacen falta 3 medidas a alta temperatura ⚠️ D‑04 |
| `R-P2-08` | `Datos!CO20` | `=OR(CF60,CF75,CM85,CM96,CF108)` | Faltan datos en alguna parte ‑2 ⚠️ **no incluye ‑2‑5 (`CM63`) ni ‑2‑22 (`CF154`)** → D‑10 |

### 5.15 IK — 🚫 FUERA DE ALCANCE

> Excluido del borrador por decisión del laboratorio. Esta sección se conserva **solo como inventario** de lo que habrá que reconstruir si se incorpora más adelante. No se implementa nada de aquí.

⚠️ **La lógica de IK está rota en el libro actual.** Las celdas `Datos!AI14`, `AJ14`, `AK14`, `AL14` contienen `=NOT(#REF!)`, y en `Toma de notas!C1058` y `C1065` los avisos son `=IF(#REF!=FALSE,…)`. Lo que se conserva del diseño original:

- Grado IK objetivo: casillas `Datos!C14:G15` (N/A, IK06, IK07, IK08, IK09, IK10, IK11).
- `Datos!O15 = OR(C14,E14,G14,I14,C15,E15,G15)` → ¿hay información de IK?
- Peso en el avance del proyecto: **30** (el segundo más alto tras endurancia).
- Método de ensayo (`Toma de notas!B1069:B1072`): martillo de resorte EQ‑SAFE‑101 / péndulo EQ‑SAFE‑102 / caída vertical EQ‑SAFE‑103 / otros. Formato condicional en `B1068:E1072` que resalta si no se elige ninguno.
- Campos: hasta 3 puntos de aplicación (descripción + nº de impactos), grado IK alcanzado en luminaria y en grupo óptico.
- Norma asociada: IEC 62262:2002+AMD1:2021 + IEC TR 62696:2011.
- Regla documental: no más de tres golpes en las inmediaciones de un mismo punto salvo indicación expresa.

**Si se reincorpora, hay que reconstruir**: qué condicionaba `AI14`/`AK14` (presumiblemente `N/A IK` y `faltan datos IK`) y a qué celda apuntaban los avisos de `C1058`/`C1065`.

Bloques físicos que quedan fuera del formulario del borrador: `Toma de notas!B1054:C1113` (ensayo de IK completo, incluida la información adicional plegable) y el cuadro «GRADOS IK (OBJETIVO)» de `RESUMEN!O14:P30`.

### 5.16 Módulos LED 62031 — 🚫 FUERA DE ALCANCE

Excluido igualmente. Inventario de lo que existe hoy:

- Casillas de clasificación del módulo en `Datos!Q6`, `R6`, `S6`, `T6`: *built‑in sin cubierta*, *built‑in con cubierta*, *independiente*, *integrado*. **Las cuatro están rotas** (`=#REF!`), es decir, ya no hay casilla que las alimente.
- `Datos!BQ4 = OR(Q6,S6,T6)` → la norma 62031 aplica si el módulo es built‑in, independiente o integrado (obsérvese que `R6`, *built‑in con cubierta*, queda fuera del OR).
- Ediciones: IEC 62031:2018 / EN 62031:2020 + A11:2021.
- `Datos!BN14` compone el texto «62031 + IK» para la cabecera del proyecto.

**Consecuencia para el borrador:** `Datos!O6` (¿hay alguna parte ‑2 marcada?) deja de estar roto en cuanto se elimina `S6` de su expresión, y con él se arregla el aviso «FALTA POR MARCAR NORMAS A APLICAR» de `RESUMEN!D25`.

---

## 6. Cálculos de ingeniería

| ID | Origen | Fórmula | Descripción |
|---|---|---|---|
| `C-01` | `Datos!CF28` | `IFS(<8m→45; 8‑15m→52; >15m→57)` | Velocidad de viento (m/s) según altura de montaje |
| `C-02` | `Datos!CF29:CM29` | `0,5 × 1,225 × A × 1,2 × v²` | Fuerza de carga estática (N) para ‑2‑3, por muestra. `A` = área en m² (`Toma de notas!G382:N382`); 1,225 = densidad del aire; 1,2 = coeficiente de forma |
| `C-03` | `Toma de notas!G428:N428` | `= área × 2400` | Fuerza de carga estática (N) para ‑2‑5 ⚠️ criterio distinto de `C-02`, confirmar |
| `C-04` | `Datos!AQ29` | `√((A/2)² + (P/2)²)` | Semidiagonal de la base de la muestra |
| `C-05` | `Datos!AQ30` | `√(H² + d1²)` | Distancia `d` para el arco de lluvia |
| `C-06` | `Datos!AQ32` | `d + 20` | Radio máximo del arco (cm) |
| `C-07` | `Datos!AQ33` | selección discreta | Arco a utilizar entre {20, 40, 60, 80, 100, 120, 140} cm o cabeza de regadera |
| `C-08` | `Datos!AQ25` | `H/2` si IPX4 | Altura efectiva para el cálculo del arco |
| `C-09` | `Datos!BY101:BY102` | `hora × 1440` | Conversión de hora a minutos para el ensayo de bola |

---

## 7. Ediciones de normas

| ID | Origen | Regla |
|---|---|---|
| `R-NOR-01` | `Datos!BI4:BI14`, `BJ4:BJ14` | Para cada norma, si su factor es verdadero se emite su texto IEC y su texto EN; si no, un espacio |
| `R-NOR-02` | `Datos!BH5:BH14` | El factor de cada parte ‑2 es su casilla de proyecto (`D6`…`N6`) |
| `R-NOR-03` | `Datos!BN10` / `BN12` | Texto final = concatenación de los 11 fragmentos IEC / EN |
| ~~`R-NOR-04`~~ | `Datos!BQ4` | 🚫 62031 — fuera de alcance |
| ~~`R-NOR-05`~~ | `Datos!BQ5` | 🚫 IK — fuera de alcance |
| `R-NOR-06` | `Datos!BF14` | La norma «Otro» toma su texto de `RESUMEN!D27` |

En el borrador, el texto de normas aplicadas se compone únicamente de `R-NOR-01` a `R-NOR-03`, que no dependen de ninguna celda rota.

Catálogo de ediciones vigentes en la plantilla (dato maestro que debe ser editable sin tocar código):

```
60598-1     IEC 60598-1:2024                          EN IEC 60598-1:2024 + A11:2024
60598-2-1   IEC 60598-2-1:2020                        EN IEC 60598-2-1:2021
60598-2-2   IEC 60598-2-2:2011 / :2023                EN 60598-2-2:2012 / EN IEC 60598-2-2:2024
60598-2-3   IEC 60598-2-3:2002+AMD1:2011              EN 60598-2-3:2003+A1:2011
60598-2-4   IEC 60598-2-4:2017                        EN 60598-2-4:2018
60598-2-5   IEC 60598-2-5:2015                        EN 60598-2-15:2015   ⚠️ ¿errata? (2-15 vs 2-5)
60598-2-10  IEC 60598-2-10:2003                       EN 60598-2-10:2003
60598-2-13  IEC 60598-2-13:2006+AMD1:2011+A2:2016     EN 60598-2-13:2006+A1:2012+A2:2016+A11:2021
60598-2-18  IEC 60598-2-18:1993+AMD1:2011 / :2022     EN 60598-2-18:1994+A1:2012 / :2022  (ver TRF)
60598-2-22  IEC 60598-2-22:2021                       EN IEC 60598-2-22:2022
```

🚫 Fuera de alcance del borrador, se conservan para más adelante:

```
62031       IEC 62031:2018                            EN 62031:2020 + A11:2021
IK          IEC 62262:2002+AMD1:2021 + IEC TR 62696:2011   EN 62262:2002 + A1:2021 + IEC TR 62696:2011
```

---

## 8. Estado del proyecto (ponderación de avance)

Marcado en el libro como **«EN FASE DE IMPLEMENTACIÓN»**. Aplica el patrón P8 sobre 21 conceptos dentro del alcance (22 con IK).

| Concepto | Peso | Concepto | Peso | Concepto | Peso |
|---|---|---|---|---|---|
| 3. Marcado | 3 | 11. LF y DA | 5 | IP5X/IP6X | 5 |
| 4.6/4.7.2 Ensayo vena | 3 | 7. Tierra | 3 | IPX_ | 5 |
| 4.12 Tornillos | 3 | 8. Choque eléctrico | 3 | 10.2 Rigidez y R. aisl. | 5 |
| 4.13 Impacto | 3 | 12.3 Endurancia | **100** | 10.3 Corrientes de fuga | 3 |
| 4.14 Carga | 3 | 12.4 Calentamiento + tc | 10 | Bola | 5 |
| 4.24 Luz azul | 5 | 12.5 Anormal | 10 | Llama | 5 |
| Partes activas | 5 | 12.6+12.7 Cond. fallo | 10 | Hilo | 5 |
| | | ~~IK~~ | ~~30~~ 🚫 | IRC (caminos) | 10 |

**Peso total del proyecto en el borrador: 209** (25 de construcción/generales + 141 de tierra‑choque‑LF‑calentamientos + 43 de IP‑aislamiento‑fuego). Con IK serían 239.

| ID | Origen | Expresión original | Regla |
|---|---|---|---|
| `R-EST-01` | `Datos!AX5` | `=SUM(AB6:AB14,AJ6:AJ14,AR6:AR15)` | Peso total del proyecto = suma de pesos de lo que aplica |
| `R-EST-02` | `Datos!AX6` | `=SUM(AD6:AD14,AL6:AL14,AT6:AT15)` | Peso ejecutado |
| `R-EST-03` | `Datos!AX7` | `=AX5−AX6` | Peso pendiente |
| `R-EST-04` | `Datos!AY5:AY7` | barra de datos | % de avance |

Las tres primeras estaban rotas **solo por la fila de IK** (`AJ14`/`AL14`); al excluirla, el panel de avance queda operativo sin más cambios.

⚠️ El bloque de pesos usa la **numeración antigua** de la norma mientras el resto del motor usa la nueva. Ver D‑02.
⚠️ **Endurancia pesa 100 sobre 209, casi la mitad del proyecto.** Ver D‑18.

---

## 9. Índice y selección de equipos

### 9.1 Hoja `Índice`

47 controles propios: casillas `F3`…`F96` (¿aplica este apartado?) y 16 grupos de opciones `G11`, `G14`, `G17`, `G28`, `G36`, `G43`, `G46`, `G53`, `G59`, `G62`, `G68`, `G75`, `G81`, `G91`, `G99`, `G102` (variante de equipo utilizado, valores 1/2/3).

| ID | Origen | Regla |
|---|---|---|
| `R-IDX-01` | `Índice!F6:F9`, `F20:F24`, `F31:F32` | Los subapartados de construcción heredan la casilla de la sección: `=F$5` |
| `R-IDX-02` | `Índice!F40` | `=F39` — el subapartado 9.3 hereda de la sección 9 |
| `R-IDX-03` | `Índice!F48`, `F56`, `F72` | Herencia análoga en cableado, aislamiento y LF/DA |

### 9.2 Hoja `Equipos`

| ID | Origen | Regla |
|---|---|---|
| `R-EQ-01` | `Equipos!C8` y análogas | `=IF(<fecha del ensayo> <> 0, <lista de equipos de BBDD>, "NO ENSAYADO")` — el listado se rellena solo si el ensayo se ha realizado |
| `R-EQ-02` | `Equipos!G9` y análogas | `=IF(Índice!F<n> = TRUE, <rango de BBDD>, "NO ENSAYADO")` — el listado depende de la casilla del índice |
| `R-EQ-03` | `Equipos!N9`, `R9`, `V9`… | `=IF('Datos'!<celda> = TRUE, "X", "")` — marca con **X** el equipo concreto seleccionado en el motor |
| `R-EQ-04` | `Equipos!AT11`, `AX12`… | `=IF(Índice!$G$91 = <n>, "X", "")` — marca el equipo según el grupo de opciones del índice |

---

## 10. Mensajes de aviso de la toma de notas

Los 60 avisos de la hoja de trabajo son la única salida visible del motor. Todos siguen la forma `=IF(<condición del motor>, "<texto>", "")`.

| Texto | Condición |
|---|---|
| `IMPRESCINDIBLE RELLENAR ESTOS DATOS` | `Datos!D25 = 1` (faltan dimensiones y son necesarias) |
| `SELECCIONA UNA OPCIÓN` | `Datos!M21 = 1` / `M22 = 1` (SELV / parte activa incoherentes) |
| `ELEGIR UNA OPCIÓN` | `Datos!D85 ≠ FALSO` (ensayo de vena) |
| `ELEGIR UNA OPCIÓN` | `OR(L75,L76,L77) ≠ V` (dimensiones del bloque de conexión) |
| `INTRODUCIR DIMENSIONES ->` | `L77 = V` y `L79 = F` |
| `COMPLETAR ENSAYO DE HUMEDAD 48H` | `Datos!B119 = F` / `AP88 = F` |
| `COMPLETAR CALENTAMIENTO` | `Datos!B125 = F` |
| `SELECCIONAR UNO` | `Datos!D110 = F` (lugar de acondicionamiento de manguitos) |
| `INTRODUCIR DATOS` | `Datos!D143 = V` / `D149 = V` (tornillos / uniones) |
| `MARCAR UNA OPCIÓN` | `Datos!L139` (portalámparas 7.12) |
| `SELECCIONA OPCIONES` | `Datos!L147 = F` (prensaestopas) |
| `INTRODUCIR DATOS REVISANDO TABLA DEL ANEXO F` | `Datos!L192 = V` (season cracking) |
| `MARCAR UNA DE LAS OPCIONES` | `Datos!CF27 ≠ 1` (altura de montaje ‑2‑3) |
| `INDICAR DATOS MEDIDOS` | `Datos!CF140 = 1` / `CF149 = 1` (‑2‑22) |
| `FALTA INFORMACIÓN SOBRE DIMENSIONES. IMPRESCINDIBLE` | `Datos!AR60 = F` |
| `FALTA POR MARCAR GRADO IP EN 'RESUMEN PROYECTO'` | `AR59 = F` y `AR55 = F` |
| `Ojo, equipo muy grande… DESVIACIÓN AL MÉTODO` | `Datos!AR46 = V` |
| `SELECCIONA EQUIPO(s) UTILIZADOS` | `Datos!AB53 = F` |
| `SELECCIONA LUGARES DE ENSAYO` / `SELECCIONA EQUIPOS DE ENSAYO` | `Datos!AJ29 = F` / `AJ39 = F` |
| `SELECCIONA EQUIPO` | `BE39/BE32 = F`, `BE47/BF32 = F`, `BE55/BG32 = F` |
| `PORTÁTIL CLASE I, OJO A RED especial de Kikusui` | `Datos!BE34 = V` |
| `SELECCIONAR EL ORIGEN DE LA MUESTRA` | `Datos!BU83 = F` |
| `VERIFICAR SI LA MUESTRA HA ESTADO EN CONDICIONES CORRECTAS…` | `Datos!BU121 = F` |
| `ESPESOR O DIMENSIONES INFERIORES A LAS NECESARIAS` | `Datos!BU94 = F` |
| `ESPECIFICAR TIEMPOS DE ACONDICIONAMIENTO Y/O ENSAYO` | `Datos!BU107 = F` |
| `DATOS DE AGUA Y POST ACONDICIONAMIENTO` | `Datos!BU116 = F` |
| `SELECCIONAR LA DISPOSICIÓN DE LA LLAMA` | `Datos!BU137 = F` |
| `COMPLETAR LOS RESULTADOS` | `Datos!BU141 = F` |
| `REVISAR EL TAMAÑO DE LA MUESTRA Y/O EL DIÁMETRO DEL HILO` | `NO(BW58 y BX53)` |
| `INTRODUCIR DATOS SOBRE EL ACONDICIONAMIENTO DE MUESTRAS DURANTE 24H` | `Datos!BU168 = F` |
| `FALTA POR MARACAR INFORMACIÓN DE PROYECTO` | `D4 = 0` o `D8 = 0` o `D9 = 0` (código, nº de muestras o Ta) |
| `RELLENAR INFORMACIÓN SOBRE IMPACTOS` / `RELLENAR RESULTADO OBTENIDO` | ⚠️ `#REF!` — IK |

**Formato condicional adicional** (resaltado de cabeceras): cada título de sección del motor se colorea cuando su regla «faltan datos» es verdadera — `B36←D42`, `B51←D66`, `B73←L85`, `B95←D112`, `B132←L154`, `B163←D171`, `B182←D198`, `B208←L213`, `B233←D246`, `B256←D265`, `J18←L31`, `J95←L105`, `J163←L171`, `J182←L192`, `J233←L242`, `R18←T34`, `Z18←AB37`, `Z42←AB56`, `AH18←AJ61`, `AP18←AR83`, `BC18←BE61`, `BL18←BN28`, `BL39←BR41`, `BU18←BW70`.

---

## 11. Defectos, incoherencias y preguntas abiertas

Lista de trabajo para la revisión con el laboratorio. **Ninguno de estos puntos debe programarse tal cual sin decisión previa.**

Los IDs son estables: los cerrados se conservan tachados en lugar de renumerar la lista.

| ID | Gravedad | Descripción | Decisión necesaria |
|---|---|---|---|
| ~~**D‑01**~~ | ✅ **Cerrado por alcance** | Las **26 celdas `#REF!`** y las 5 casillas huérfanas pertenecen todas a IK o a 62031: lógica de IK (`AI14`, `AK14`, `AJ14`, `AL14`, `C1058`, `C1065`), aplicabilidad de 62031 (`Q6`, `R6`, `S6`, `T6`, `BQ4`, `BR4`, `BS4`, `BN14`, `O6`), texto de normas (`RESUMEN!B34`, `D21`, `D25`) y panel de avance (`AX5:AY7`). Los dos últimos grupos estaban rotos **de rebote**, no por sí mismos | Ninguna. Al excluir IK y 62031 el borrador queda **sin lógica rota**. Reabrir si se reincorpora alguna de las dos |
| ~~**D‑02**~~ | ✅ **Resuelto** | **Dos numeraciones de norma conviven**: `Índice`, el panel de pesos y parte de las hojas de equipos usan la antigua; la toma de notas y el resto del motor, la de 2024 | **Canónica: edición 2024.** La antigua queda como alias de visualización. Tabla de equivalencia en la [sección 5](#equivalencia-de-numeraciones). Queda un punto menor por confirmar: el rótulo 5.2 «cableado interno» ↔ 8.2 «cableado externo» |
| **D‑03** | 🟡 Baja | En la sección IP, las etiquetas internas «primera cifra» y «segunda cifra» del motor (`AP54`–`AP72`) están **cruzadas** respecto a su contenido. Funcionalmente los grupos son correctos (cada uno usa su fecha y sus casillas), pero inducen a error | Renombrar al trasladar. Verificar además `AQ8`, que usa el N/A del grupo de polvo para la fila de peso «IPX_» (agua) |
| ~~**D‑04**~~ | ✅ **Aceptado sin cambio** | Ocho recuentos usan `">=0"` en lugar de `">0"`, de modo que **una celda con 0 cuenta como dato introducido**: `L78` (dimensiones de bloque), `D196` (temperaturas de corrosión), `D241` (7.28), `CF140` y `CF149` (‑2‑22) | Decisión del laboratorio: se replica el comportamiento actual. **Queda documentado como intencionado**, no como defecto, para que nadie lo «arregle» después |
| **D‑05** | 🟠 Media | **Corregido el diagnóstico inicial.** El patrón P7 de los acondicionamientos (24 h / 48 h / 240 h) **es correcto**: equivale a `duración ≥ N días` (demostración en la [sección 3](#p7--verificación-de-duración-de-acondicionamiento)). El defecto real está en el **ensayo de bola**: `Datos!BU99` (acondicionamiento ≥ 180 min) y `BU103` (ensayo 60 ± 2 min) comparan campos **de solo hora, sin fecha**. Si el ensayo cruza la medianoche la resta sale negativa y la verificación falla, marcando como incompletos ensayos correctos | ✅ **A corregir en la aplicación:** los cuatro campos de tiempo del ensayo de bola (`G869`–`G872`) pasan a ser instante completo (fecha + hora) y las comparaciones se hacen sobre `datetime`. No hay riesgo de aceptar datos malos, solo de rechazar buenos |
| **D‑06** | 🟡 Baja | `Datos!L153` = `IF(D143+D149+L139+L151=4,FALSE,TRUE)` suma cuatro booleanos y compara con 4; equivale a `NOT(AND(...))` pero es frágil | Reescribir como expresión booleana explícita |
| **D‑07** | 🟡 Baja | En 7.9 los equipos se llaman `EQ-CERT-305` / `EQ-CERT-304` mientras que en el resto del libro y en la BBDD son `EQ-SAFE-305` / `EQ-SAFE-304` | Unificar los códigos de equipo |
| **D‑08** | 🟠 Media | Muchas tolerancias del procedimiento (20 ± 5 °C de la solución, 100 ± 5 °C de la estufa, ± 2 °C del termopar) están escritas como **texto informativo** y no se validan; solo se comprueba que el campo no esté vacío | Decidir cuáles deben ser validaciones reales de la aplicación |
| **D‑09** | 🟡 Baja | Los subapartados 17.4 y 17.5 tienen **lógica duplicada e idéntica** (`BN66:BN69` y `BN72:BN75`), con la misma etiqueta «Cableado interno» en ambos | Confirmar que el segundo bloque corresponde a cableado **externo** |
| ~~**D‑10**~~ | ✅ **Aceptado para el borrador** | `Datos!CO20` («faltan datos en alguna parte ‑2») **omite ‑2‑5 (`CM63`) y ‑2‑22 (`CF154`)** | Se replica tal cual. **Consecuencia asumida:** un proyecto al que solo le falten datos de ‑2‑5 o ‑2‑22 no activará el aviso global de partes ‑2, aunque sí el suyo propio. Revisar antes de pasar de borrador a producción |
| ~~**D‑11**~~ | ✅ **Resuelto** | La firma del registro se hace **respondiendo a un comentario** de Excel (`RESUMEN!D4`) | **La aplicación no gestiona firmas.** Se firma el PDF impreso, fuera del programa. En consecuencia, los registros son modificables y no hay bloqueo tras firmar (ver DD‑07 y DD‑19) |
| **D‑12** | 🟡 Baja | `Datos!T26` (datos de ensayo de tierra) se calcula pero **no se usa**; el propio autor anota que los datos van directos al TRF | Decidir si el campo desaparece o se convierte en validación real |
| **D‑13** | 🟡 Baja | Dos criterios distintos de carga estática: ‑2‑3 usa la fórmula de presión dinámica del viento y ‑2‑5 usa `área × 2400` | Confirmar ambos con la norma |
| **D‑14** | 🟡 Baja | La edición EN de la parte ‑2‑5 figura como `EN 60598-2-15:2015` (posible errata por `2-5`) | Verificar contra el catálogo de normas |
| **D‑15** | 🟠 Media | `Datos!L39` (resumen de la sección 7) no cubre las partes ‑2, según nota del propio autor | Definir el árbol de agregación completo de una vez |
| **D‑16** | 🟡 Baja | Hay equipos y lugares marcados como `AMPLIACIÓN 1`, `AMPLIACIÓN 2`, `PREVISIÓN 1/2`, `EQ-SAFE-3xx`, `RESERVADO COPPER-BS`, con la nota «no está asignado todavía» | Limpiar el catálogo de equipos antes de migrarlo |
| **D‑17** | 🟡 Baja | El nombre de la hoja `Índice` está corrupto en el fichero (`Ãndice`, problema de codificación) | Sin impacto funcional; se corrige en la migración |
| ~~**D‑18**~~ | ✅ **Resuelto** | **Endurancia pesa 100 sobre 209.** El panel no cuenta apartados, los pondera: con todo terminado salvo endurancia la barra marca **52 %**; solo con endurancia hecha marca 48 %. Como los pesos solo cuentan lo aplicable, en proyectos con pocas secciones endurancia puede suponer el 70‑80 % | **Se mantienen los pesos tal cual** (endurancia dura semanas, es un proxy razonable de esfuerzo) **y se añade un segundo indicador**: «apartados completados: X/Y» |
| ~~**D‑19**~~ | ✅ **Aceptado sin cambio** | La parte **‑2‑7 tiene rótulo (`Datos!I5`) pero no tiene casilla**: no hay ningún control enlazado a `I6`, así que no se puede marcar | Se replica: ‑2‑7 aparece en la lista pero no es seleccionable |
| ~~**D‑20**~~ | ✅ **Aceptado sin cambio** | `Datos!O6` («¿hay alguna parte ‑2 marcada?») **no incluye `N6` (OTRO)**: si el proyecto solo aplica una parte ‑2 declarada como «OTRO», el aviso «FALTA POR MARCAR NORMAS A APLICAR» sigue apareciendo | Se replica el comportamiento actual |

| **D‑21** | 🔴 **Alta** | **La agregación de 7.12 está invertida.** `Datos!L153 = IF(D143+D149+L139+L151=4,FALSO,VERDADERO)` suma cuatro banderas de *«faltan datos»* y solo devuelve FALSO —que es lo que dispara el aviso en `L154`— cuando **las cuatro** son verdaderas. Consecuencia: si faltan datos en uno, dos o tres de los cuatro subapartados (tornillos, uniones, portalámparas, prensaestopas) **el apartado 7.12 no avisa de nada**. Solo funciona en los extremos: todo completo o todo vacío. El patrón está copiado de `L84` (7.6), donde los operandos son banderas de «OK» y la polaridad sí encaja | ✅ **Corregido en la plantilla del MVP:** se implementa la agregación correcta `faltan = D143 O D149 O L139 O L151`. Es el segundo y último cambio de comportamiento respecto al Excel. **Confirmar**, y revisar si el mismo error existe en otras secciones fuera del MVP |
| **D‑22** | 🟠 Media | **El libro mezcla tres mecanismos de entrada**: controles de formulario heredados (297), **casillas nativas de celda** de Excel 2024 (27 en la hoja de trabajo) y **casillas sin enlazar** (17) que se marcan pero no alimentan ninguna regla. Ver el detalle en la [sección 4](#entradas-que-no-son-controles-de-formulario--d22) | La aplicación unifica los tres en un único tipo de campo booleano. Las 17 sin enlazar pasan a ser datos reales del ensayo — **confirmar que deben validarse**, sobre todo `No blando` / `No aislante` de cada tornillo y `15s agua` / `15s hexano` del marcado |

### Defectos aparecidos al construir la aplicación

Ninguno venía del Excel: son fallos míos al trasladarlo, encontrados al usar la aplicación.
Se recogen porque explican por qué la plantilla y el motor tienen las piezas que tienen.

| ID | Descripción | Estado |
|---|---|---|
| **D‑23** | `R-PROY-01` usaba un campo `faltanSiVacio` que el motor nunca implementó: **siempre devolvía «no faltan datos»**. La cabecera del proyecto no se validaba en absoluto | ✅ Sustituida por el predicado `proyectoCompleto`, con la lista de requisitos en `RequisitosDelProyecto` |
| **D‑24** | El semáforo tomaba la última regla `faltanDatos` del apartado, pero en los que pueden no aplicar la que cierra es el condicional `si` que la envuelve. **18 apartados salían en rojo aunque no aplicaran** | ✅ Se admite `reglaDeCierre` explícito y la deducción incluye los `si` |
| **D‑25** | `fechaPorMuestra` se ignoraba en la interfaz, y el motor lo leía del **bloque padre** en vez del subapartado que declara la regla | ✅ Cada elemento usa su propio `ambiente`; basta con que cualquier muestra tenga fecha |
| **D‑26** | Los subapartados **no mostraban sus condiciones de ensayo**. 17 subapartados —secciones 8, 11, 12, 14, 16 y 17— exigían una fecha que no tenía dónde escribirse: eran imposibles de completar | ✅ Cada subapartado pinta su T/H/fecha |
| **D‑27** | En los grupos repetidos, `porMuestra` se leía del campo hijo en vez del grupo padre: **una sola columna** aunque hubiera 8 muestras. Afectaba a ratings, tamaños, tornillos, uniones, prensaestopas y partes frágiles | ✅ El grupo impone el `porMuestra` a sus hijos |
| **D‑28** | `muestras.max` de la plantilla **no lo leía nadie**: el tope estaba a fuego en el código | ✅ La aplicación lee el valor de la plantilla |

**Estado del borrador:** 0 puntos bloqueantes.

| Estado | Puntos |
|---|---|
| ✅ Cerrados | D‑01 (alcance), D‑02, D‑04, D‑10, D‑11, D‑18, D‑19, D‑20 |
| 🔧 A corregir en la aplicación | **D‑05** (instantes completos en el ensayo de bola), **D‑21** (agregación de 7.12) |
| ⏳ Pendientes de confirmación del laboratorio | **D‑07**, **D‑13**, **D‑14**, **D‑21**, **D‑22** |
| ✅ Resueltos por criterio técnico | D‑03, D‑06, D‑08, D‑09, D‑12, D‑15, D‑16, D‑17 |

**Cambios de comportamiento respecto al Excel.** Son solo dos, ambos marcados en la plantilla con el campo `defecto`:

| Punto | Qué cambia |
|---|---|
| **D‑03** | El peso de avance de la fila «IPX_» (agua) pasa a usar el N/A del grupo de agua en vez del de polvo |
| **D‑21** | El apartado 7.12 avisa cuando falta **alguno** de sus cuatro subapartados, en vez de solo cuando faltan los cuatro |

### Resolución de los puntos menores

| ID | Resolución adoptada |
|---|---|
| **D‑03** | Se renombran las etiquetas cruzadas «primera/segunda cifra». Además se **corrige `AQ8`**, que usaba el N/A del grupo de polvo para la fila de peso «IPX_» (agua): pasa a usar `AR55`. Es el único cambio de comportamiento respecto al Excel, y afecta solo al panel de avance |
| **D‑06** | `L153` se reescribe como expresión booleana explícita. ⚠️ Al hacerlo apareció **D‑21**: la reescritura *no* es de resultado idéntico, porque la fórmula original está invertida |
| **D‑07** | ⏳ **Confirmar:** los equipos de 7.9 figuran como `EQ-CERT-305`/`EQ-CERT-304`, pero en la BBDD y en el resto del libro son `EQ-SAFE-305`/`EQ-SAFE-304`. Se asume `EQ-SAFE` salvo indicación contraria |
| **D‑08** | Las tolerancias del procedimiento (20 ± 5 °C, 100 ± 5 °C, ± 2 °C) **no se validan**, igual que hoy. Se muestran como texto de ayuda junto al campo. Convertirlas en validación real queda para una fase posterior |
| **D‑09** | 17.4 se etiqueta «cableado interno» y 17.5 «cableado externo», manteniendo la lógica idéntica que tienen hoy |
| **D‑12** | `T26` no se implementa: era una regla calculada y nunca usada. El valor de resistencia de tierra (Ω) sigue siendo un campo de datos normal |
| **D‑13** | ⏳ **Confirmar:** ‑2‑3 usa la presión dinámica del viento y ‑2‑5 usa `área × 2400`. Se replican ambos criterios tal cual |
| **D‑14** | ⏳ **Confirmar:** `EN 60598-2-15:2015` parece errata por `EN 60598-2-5:2015`. Afecta al texto impreso de normas aplicadas, por eso necesita confirmación antes de corregirlo |
| **D‑15** | El resumen de la sección 7 (`L39`) sigue sin cubrir las partes ‑2, coherente con la decisión tomada en D‑10 |
| **D‑16** | Los equipos marcados `AMPLIACIÓN`, `PREVISIÓN`, `EQ-SAFE-3xx` y `RESERVADO COPPER-BS` se importan **tal cual y siguen siendo seleccionables**, para mantener la equivalencia con el Excel. Limpieza del catálogo aplazada |
| **D‑17** | Irrelevante en la aplicación: el nombre de hoja corrupto desaparece con la migración |

---

## 12. Cobertura de esta extracción

| Elemento | Total en el libro | En alcance del borrador | Documentado aquí |
|---|---|---|---|
| Fórmulas | 898 | ~870 | Todas revisadas; las de presentación (propagación de rótulos, copia de valores entre hojas) se resumen en patrones en lugar de listarse una a una |
| Controles de formulario | 297 | 289 | Inventariados por hoja, tipo y celda de destino (sección 4) |
| Entradas del motor | 179 | 171 | Sección 4 |
| Reglas de formato condicional | 31 | 30 | Todas (secciones 8 y 10) |
| Bloques de ensayo | ~56 | ~55 | **Los 44 apartados de la plantilla, verificados contra el JSON** |
| Cálculos numéricos | 9 | 9 | Todos (sección 6) |
| Celdas rotas | 26 + 5 controles | **0** | Todas fuera de alcance (D‑01) |

**La norma está completa**: 14 secciones y 44 apartados, en el orden y con los títulos de la hoja «Toma de notas 60598».

---

## 13. Decisiones de desarrollo

### Cerradas

| ID | Decisión |
|---|---|
| **DD‑02** | Los proyectos viven en el **servidor de OneDrive** del laboratorio, junto al resto de documentación e informes |
| **DD‑03** | La aplicación **sustituye al Excel**; no se replica el formato `.xlsx`. La salida es el informe imprimible |
| **DD‑04** | MVP con generales + muestras, **6 Marcado**, **7.12 Tornillos**, **11 IP** y **15.2 Bola**. Entre esos cuatro bloques se ejercitan los ocho patrones P1‑P8 |
| **DD‑05** | **No se migran proyectos en curso.** Los abiertos se terminan en Excel |
| **DD‑06** | Salida única: **A4 vertical**, tantas páginas como hagan falta. ⚠️ El formato lo redefine DD‑20: se genera HTML y el PDF sale de Word o del navegador |
| **DD‑07** | **La aplicación no firma.** Se firma el PDF impreso, fuera del programa |
| **DD‑08** | Perfil de usuario con nombre, DNI, correo, contraseña, recuperación de contraseña y sesión persistente ⚠️ ver observaciones |
| **DD‑09** | **No se migran plantillas.** Cada proyecto se queda con la versión con la que nació |
| **DD‑11** | Se mantienen los pesos y se añade un contador de apartados completados (D‑18) |
| **DD‑12** | Los excels de calentamiento externos **quedan fuera** del programa |
| **DD‑13** | Fotografías y adjuntos **quedan fuera** del programa |
| **DD‑14** | *(criterio técnico)* Comentarios en **texto plano multilínea**, sin formato enriquecido: es lo que hay hoy y simplifica el PDF |
| **DD‑15** | *(criterio técnico)* **Un único documento continuo**; cada apartado empieza en página nueva al imprimir. Portada con código de proyecto, muestras y versión de plantilla |
| **DD‑17** | Requisitos formales de validación de software: **fuera de alcance por ahora** |
| **DD‑18** | Política de retención y copias: **fuera de alcance por ahora**; se delega en el versionado de OneDrive |
| **DD‑19** | **Los registros son modificables.** El documento firmado es el PDF, que se gestiona fuera |

| **DD‑01** | **.NET 8 + WPF.** C# es el lenguaje que el desarrollador ya domina (vía Unity); XAML con *data binding* encaja con un formulario de cientos de campos; distribución como ejecutable único en Windows |
| **DD‑10** | El catálogo de equipos se **importa tal cual está** desde `BBDD Equipos 60598`, incluidos los marcadores `AMPLIACIÓN`, `PREVISIÓN`, `EQ-SAFE-3xx` y `RESERVADO COPPER-BS` (coherente con D‑16) |

*No quedan decisiones de desarrollo abiertas.*

### Arquitectura técnica

```
LumNotas.Core          reglas, plantilla, cálculos, validaciones     C# puro, sin UI, con tests
LumNotas.Storage       lectura/escritura del JSON de proyecto        C# puro, sin UI
LumNotas.Report        generación del informe (HTML con estilos A4, ver DD-20)
LumNotas.App           interfaz WPF (MVVM)
```

`Core` y `Storage` concentran el 70‑80 % del trabajo y **no dependen de WPF**. Si en el futuro se quisiera otra interfaz (web, multiplataforma), se reescribe solo `App`.

| Elemento | Elección |
|---|---|
| Plataforma | .NET 8, Windows |
| Interfaz | WPF + `CommunityToolkit.Mvvm` |
| Fichero de proyecto | JSON (`System.Text.Json`), escritura atómica, en OneDrive |
| Definición de plantilla | JSON versionado, incluido con la aplicación |
| Informe | HTML generado a mano, **sin dependencias**. Se descartó MigraDoc/PDFsharp: añadía una librería externa y daba problemas de resolución de fuentes (DD‑20) |
| Contraseñas | Hash Argon2, restablecimiento local (sin correo) |

### Observaciones técnicas sobre las decisiones tomadas

**OneDrive condiciona el formato de almacenamiento.** No se puede usar SQLite sobre una carpeta sincronizada: el bloqueo de fichero y la sincronización parcial provocan corrupción y copias en conflicto. Formato adoptado en su lugar:

- **Un fichero JSON por proyecto** (`<código>.lumproj.json`), escrito de forma atómica en cada guardado (escritura a temporal + reemplazo).
- Ventajas sobre OneDrive: reemplazo de fichero completo —que OneDrive sincroniza sin problema—, historial de versiones gratuito, recuperación manual si algo falla, y contenido legible sin la aplicación.
- Con la edición **por turnos** ya decidida, no hace falta control de concurrencia. Sí conviene un aviso al abrir si OneDrive marca el fichero como no sincronizado.

**Sobre DD‑08.** El perfil con contraseña tiene un alcance limitado que conviene tener presente: los ficheros de proyecto viven en OneDrive en claro, así que la contraseña de la aplicación no protege los datos —cualquiera con acceso a la carpeta los lee igual—. Su valor real es **atribuir autoría** en el PDF. Además, la recuperación de contraseña por correo exige un servidor de correo o credenciales SMTP, que hoy no existen. Propuesta para el borrador:

- Perfil local con nombre, DNI y correo (para la cabecera del PDF).
- Contraseña almacenada con hash (Argon2), sesión persistente mediante testigo local.
- **Recuperación por restablecimiento local** en lugar de por correo, hasta que haya infraestructura.

**Sobre el DNI.** Es dato personal; conviene confirmar que hace falta en el PDF y no basta con el nombre del técnico.

---

---

## 14. Artefactos generados

| Fichero | Contenido |
|---|---|
| `plantilla/equipos-60598.v1.json` | Catálogo de equipos completo: **43 grupos, 224 entradas, 89 códigos distintos**. Importación literal desde `BBDD Equipos 60598` (DD‑10), con las notas de uso del laboratorio y la trazabilidad de cada celda de origen |
| `plantilla/plantilla-60598.v1.json` | **La norma entera como datos**: 16 secciones y 45 apartados, con campos, checklists, subbloques, grupos repetibles, reglas P1‑P8 y los nueve cálculos. ~140 KB |
| `src/LumNotas.Core` | Motor de reglas: modelo de plantilla, catálogo de equipos, almacén de datos, evaluador de los tipos de regla, predicados y cálculos con nombre, requisitos del proyecto, indicador de avance, estado de apartado y resumen para el tablero |
| `src/LumNotas.Storage` | Un fichero `.lumproj` por proyecto (JSON), con **escritura atómica** (temporal + reemplazo) para que OneDrive lo sincronice sin corromperlo. Más la lista de recientes, los ajustes y el explorador de carpetas |
| `src/LumNotas.Report` | Exportador del informe a HTML con estilos de impresión A4. **Sin dependencias externas** |
| `src/LumNotas.App` | Interfaz WPF. `VentanaPrincipalViewModel` es la ventana con su barra de pestañas; `DocumentoViewModel` es **un proyecto abierto** (árbol con semáforo y formulario generado desde la plantilla); `GestionViewModel` es el tablero, que ocupa otra pestaña. Las plantillas grandes viven en `Window.Resources` y se eligen por tipo |
| `plantilla/plantilla-62031.v1.json`, `plantilla-60529.v1.json`, `plantilla-62262.v1.json` | Las otras tres normas, con sus catálogos `equipos-62031`, `equipos-60529` y `equipos-62262` |
| `tests/LumNotas.Core.Tests` | **169 tests, verificados en verde el 2026‑08‑06.** Cubren los ocho patrones, los nueve cálculos, los defectos corregidos, la integridad de la plantilla, el ciclo de guardado, varios proyectos simultáneos, el informe, el tablero y la planificación (semanas ISO, cambio de año, el gesto de arrastre completo y que planificar y anotar no se pisen) |

Los ficheros de plantilla conservan `origenExcel` en cada elemento para poder auditarlos contra el libro original. Ese campo no se usa en ejecución.

### Contrato de la plantilla

Las reglas dejaron de escribirse en prosa: cada una tiene un **tipo** que el motor sabe evaluar. Los ocho patrones se convirtieron en 19 tipos declarativos (`avisoFecha`, `faltanDatos`, `alMenosUna`, `exactamenteUna`, `todas`, `opcion`, `recuento`, `recuentoDatos`, `duracionMinima`, `duracionEnRango`, `rango`, `noVacio`, `y`, `o`, `si`, `predicado`, `calculo`, `aviso`, `peso`).

Solo **seis reglas** necesitan código a medida (`Predicados.cs`); el resto es configuración. Si esa lista crece mucho, es señal de que falta un patrón en la plantilla, no de que haga falta más código.

### Dos decisiones de diseño que salieron al implementar

**Ámbito de datos.** Una regla lee los campos del elemento que los declara: si está en un subbloque, su ámbito es el id del subbloque. Sin esto, los tres subapartados de IP (2ª cifra, 1ª cifra, humedad) compartirían la misma fecha y se pisarían entre sí.

**Campos derivados.** El Excel copiaba el tamaño de muestra de la sección de generales a la de IP. En la plantilla se declara como `"tipo": "derivado", "de": "generales.tamano"` y el motor resuelve la redirección al leer. Así el dato vive en un único sitio, que es lo que el Excel no podía hacer.

### Cobertura de los tests

| Fichero | Qué asegura |
|---|---|
| `PatronesTests` | Los ocho patrones P1‑P8 contra la plantilla real, no contra una de juguete |
| `CalculosTests` | Los nueve cálculos, incluido que el arco elegido es siempre el más pequeño que cubre el radio |
| `DefectosCorregidosTests` | Que no volvemos a replicar D‑03, D‑05 ni D‑21 — incluye el ensayo de bola que cruza la medianoche |
| `AvanceTests` | Los dos indicadores de avance y su discrepancia (la que motivó D‑18) |
| `PlantillaTests` | Integridad: sin ids repetidos, sin referencias rotas entre reglas, todos los tipos soportados, todos los predicados registrados, y que toda la plantilla se evalúa sin excepciones |
| `PersistenciaTests` | El ciclo guardar/cargar no pierde nada, el proyecto leído produce las mismas reglas que el original, la escritura atómica no deja restos y el fichero es legible sin la aplicación |
| `InformeTests` | Que el HTML se genera con la portada, los apartados y los formatos en `es-ES` |
| `GestionTests` | El tablero: que el avance cuenta secciones y no apartados, que al completar un apartado baja la sección, que las secciones que no aplican no salen, que el escaneo encuentra los proyectos de las subcarpetas y que **un fichero corrupto no tumba el tablero** |
| `PlantillasDeOtrasNormasTests` | Integridad de **todas** las normas instaladas, no solo la de luminarias: ids únicos, referencias entre reglas, `visibleSi` y `reglaDeCierre`, predicados registrados, grupos de equipos existentes, evaluación sin excepciones, que ningún id de bloque se repita entre normas, qué normas se pueden combinar, el grado por muestra y que cada norma exija su propia cabecera. **Los que vigilan la unificación recorren la carpeta**, no una lista escrita a mano, así que una norma nueva queda cubierta por existir |

### La interfaz

La pantalla **se genera a partir de la plantilla**, no está escrita a mano: añadir un apartado al JSON lo hace aparecer en la aplicación sin tocar código. Elementos:

- **Índice de apartados** a la izquierda, con semáforo por estado (faltan datos / completo / no aplica). Sustituye a la hoja `Índice` y al formato condicional que coloreaba las cabeceras.
- **Formulario** a la derecha, con una columna por muestra en vez de las ocho columnas fusionadas del Excel. Los grupos repetidos (tornillos, uniones, prensaestopas) se generan a partir de `grupoRepetido`.
- **Avisos** con los mismos textos que veía el técnico en el Excel, en su apartado y no en una celda perdida.
- **Lo que no aplica no se puede rellenar.** Al marcar «Este apartado no aplica (N/A)», o una exención de subapartado como «La luminaria NO tiene tornillos», los campos, equipos y comentarios de debajo se desactivan. Antes seguían siendo editables y se podían guardar proyectos que decían a la vez que un apartado no aplica y traían sus datos de ensayo. Las casillas que toman la decisión quedan activas, para poder volver atrás.
- **El panel no se desplaza de lado al pulsar una casilla.** WPF, al enfocar un control, pide llevarlo a la vista y el `ScrollViewer` se movía también en horizontal: con la tabla de muestras, marcar una casilla saltaba a otra columna. `MainWindow` rehace la petición con un rectángulo sin ancho, así que sigue subiendo o bajando al tabular entre campos pero deja de moverse de lado.
- **Contador de apartados** en la cabecera. El porcentaje ponderado y la barra de progreso se retiraron (DD‑25) por no aportar nada; el cálculo sigue en `IndicadorDeAvance`.
- **El índice se pliega** con el botón ◀ / ▶ de la cabecera. Con 30 muestras, esos 360 px son la diferencia entre ver tres columnas o siete.
- **Una barra de pestañas y una sola**: los proyectos y el tablero al mismo nivel, con el `+` detrás de la última. La pestaña de delante va en negrita y con fondo más claro.

### Pestañas: varios proyectos a la vez

El técnico suele llevar dos o tres servicios en marcha —mientras una muestra pasa 48 h en la cámara de humedad ensaya otra—, así que la aplicación abre **una pestaña por proyecto**, como un navegador:

- El botón **`+`** o *Archivo · Nueva pestaña* (Ctrl+T) abre una pestaña vacía, que enseña la portada.
- *Archivo · Abrir otro proyecto…* (Ctrl+O) abre en la pestaña de delante si está sin estrenar, y si no en una nueva.
- **Abrir un fichero ya abierto salta a su pestaña** en vez de duplicarlo: dos pestañas sobre el mismo `.lumproj` se pisarían los guardados.
- Cerrar una pestaña con cambios avisa. Cerrar la última deja una vacía, no una ventana en blanco.
- **Al cerrar la aplicación se pregunta por cada pestaña con cambios**, no solo por la de delante.
- Ctrl+S y Ctrl+P actúan sobre la pestaña activa.
- **El tablero de gestión es una pestaña más**, y solo una: se abre desde la portada o desde *Ver · Gestión de proyectos*, y volver a pedirlo salta a la que ya está. No hay pestañas de dos niveles.

Eso obligó a partir en dos lo que era una sola clase: `DocumentoViewModel` es **un proyecto abierto** —datos, motor, cabecera, árbol, ruta y cambios sin guardar— y `VentanaPrincipalViewModel` es la ventana que sostiene la colección de documentos, el tablero y los menús. **El núcleo no se tocó**: `DatosProyecto`, el motor y las plantillas ya recibían todo por parámetro y no guardaban estado global, que es justo lo que hizo viable el cambio.

### La portada

La aplicación arranca en una portada, no sobre un proyecto de luminarias en blanco. Es lo que enseña **una pestaña recién abierta**, igual que la página de inicio de un navegador. Desde ella:

- **Una tarjeta por norma instalada** — sale de `CatalogoDeNormas`, así que dejar caer un `plantilla-*.json` en la carpeta añade su tarjeta sin tocar nada.
- **Abrir proyecto…** y la lista de **recientes**.
- **Gestión de proyectos**, que salta directo al tablero.

En cuanto se elige norma o se abre un fichero, esa misma pestaña pasa a ser la toma de notas.

### Cambios sin guardar

Todo lo que abandona el proyecto abierto pasa por el mismo aviso: proyecto nuevo, abrir, abrir un reciente, volver a la portada y **cerrar la aplicación**. El diálogo es propio, no el `MessageBox` de Windows, para que lo diga el botón y no haya que traducir un «Sí / No»:

- **Guardar cambios** (azul) guarda y luego continúa. Si el proyecto es nuevo pide carpeta y, **si se cancela ahí, no se continúa**: no se puede acabar sin proyecto y sin fichero.
- **Continuar sin guardar** (gris) descarta.
- Cerrar el aviso cancela la acción y no se pierde nada.

### El tablero de gestión de proyectos

Segunda pestaña, pensada para el responsable, no para el técnico. **Columnas = proyectos, tarjetas = secciones pendientes** (a lo Trello).

Cómo encuentra los proyectos: se le indica **una carpeta** (la del laboratorio en OneDrive) y la escanea buscando `*.lumproj`, incluidas subcarpetas. Sin índice ni base de datos — con varios técnicos sincronizando, un índice se desincroniza y miente; el fichero es la única verdad (DD‑27).

| Pieza | Qué hace |
|---|---|
| `ExploradorDeProyectos` | Escanea la carpeta, cachea por fecha de modificación y **aísla los ficheros ilegibles**: uno corrupto sale como tarjeta de error en vez de tumbar el tablero |
| `AnalizadorDeProyectos` | Calcula el resumen reutilizando el mismo `MotorDeReglas` de la toma de notas |
| `EstadoDeApartado` | El semáforo, movido de la interfaz al núcleo para que ambas pestañas usen exactamente la misma lógica |
| `AjustesDeAplicacion` | Recuerda la carpeta elegida entre sesiones |

El avance se cuenta **por secciones** (DD‑28): la sección 7 vale 1 aunque tenga trece apartados dentro. Es la vista que pidió el laboratorio.

### El calendario (línea de tiempo)

Segunda vista de la misma carpeta, pedida el 2026‑08‑06 con Planyway como referencia. El tablero contesta *qué falta por rellenar*; el calendario contesta *cuándo toca cada servicio y qué se ha pasado de plazo*. Se cambia de una a otra con los botones «Tablero» y «Calendario».

**Una tarjeta por toma de notas** (DD‑54). Un servicio con 60598‑1 + ‑2‑3 + IK + 62031 sale como una sola barra, porque todo cuelga de la toma de notas principal.

| Dato | Dónde vive |
|---|---|
| Inicio y fin previstos | `planificacion.inicio` / `.fin` del `.lumproj` |
| Estado | `planificacion.estado`: `porHacer`, `enCurso`, `pendienteCliente`, `terminado` (DD‑51) |
| Recepción de muestras | `planificacion.recepcionMuestras`, **fecha** y no sí/no (DD‑50) |
| Archivado | `planificacion.archivado` (DD‑52) |

**El eje va en semanas ISO** (DD‑55), porque es como planifica el laboratorio. `EjeDeSemanas`, en `LumNotas.Core/Gestion/Planificacion.cs`, calcula las celdas, los meses de la cabecera y la posición en píxeles de cada barra. Está en el núcleo y no en la interfaz para poder probarlo: los años de 53 semanas y las semanas que cruzan de diciembre a enero son justo lo que se rompe en silencio. No hace falta importar los años de ningún sitio — `DateTime` e `ISOWeek` ya los saben.

Lo que se ve de un vistazo:

- **línea roja vertical de «hoy»** y la semana en curso resaltada en la cabecera;
- **barra en rojo** si la fecha de fin ya pasó y el servicio no está terminado — este es el valor de todo el invento, lo demás es decoración;
- **punto blanco** en la barra si las muestras ya están en el laboratorio;
- los servicios **sin fechas** salen en una banda aparte, con un botón «Planificar…», para que no se pierdan de vista.

**Las barras se arrastran con el ratón** (2026‑08‑06): el centro mueve el servicio entero conservando la duración, el borde izquierdo cambia solo el inicio y el derecho solo el fin. Cuatro detalles que no son evidentes:

- **Se ajusta a días enteros.** A zoom mínimo una semana son 26 px, o sea **menos de cuatro píxeles por día**; sin ajuste no se acierta.
- **El gesto se calcula desde el punto de partida, no acumulando.** Ir y volver deja la barra exactamente donde estaba, y si no ha cambiado nada **no se escribe el fichero**.
- **Los bordes topan el uno con el otro**: un fin anterior al inicio no existe.
- **Clic y arrastre se distinguen por 4 píxeles de recorrido.** Por debajo es un clic y abre el diálogo; por encima es arrastre y el clic se anula. Sin ese margen, cada intento de abrir el diálogo movería el servicio un día.

Al soltar, el eje **se conserva mientras las fechas nuevas sigan cabiendo**, para que el calendario no se desplace bajo el ratón. Arrastrar el servicio que marca el extremo sí lo reencuadra, porque el eje siempre deja dos semanas de margen alrededor de lo que hay.

El reparto de responsabilidades: `BarraDePlanificacion` y `ArrastreDeFechas` (núcleo, con tests) llevan la aritmética y el estado del gesto; `ArrastreDeBarra` (interfaz) solo traduce eventos de ratón a llamadas y decide las zonas de los bordes.

Pulsar la barra abre el diálogo de planificación; pulsar el código de la izquierda abre la toma de notas en una pestaña. Filtros: técnico, estado, norma y «ver archivados»; el técnico y la norma se rellenan con lo que haya en los proyectos, no con una lista fija.

**Cómo convive con la toma de notas** (DD‑53). La planificación está dentro del `.lumproj`, pero:

- `RepositorioDeProyectos.ActualizarPlanificacion` es lo único que la escribe: relee el fichero, cambia ese trozo y lo vuelve a guardar entero, **sin tocar un solo dato de ensayo**;
- `Guardar` (el de la toma de notas) **nunca** la escribe desde memoria: la conserva releyéndola del disco.

Sin esa segunda regla, el técnico que tuviera el proyecto abierto desde hacía media hora borraría al guardar las fechas que otro acababa de mover. El laboratorio decidió que **en lo demás manda el último que guarda**.

---

## 15. Estado del proyecto

### Hecho

| | |
|---|---|
| **Norma completa** | 16 secciones, 45 apartados, en el orden del Excel. Los títulos son los del Excel salvo **«Sección 16 y 17 - Bornes con tornillo y sin tornillo»**, que el laboratorio pidió separar en **«Sección 16 - Bornes con tornillos»** y **«Sección 17 - Bornes sin tornillo»** (2026‑08‑01) |
| **Motor de reglas** | Los ocho patrones más combinadores (`y`, `o`, `si`), predicados y cálculos con nombre |
| **Aplicabilidad dinámica** | Los apartados que no aplican desaparecen: partes ‑2 no marcadas, tierra en Clase II, doble aislamiento en Clase I |
| **Cabecera obligatoria** | Sin ella no se muestran las secciones; se marca en rojo lo que falta |
| **Muestras** | Hasta 30, con numeración editable e identificador `EBP_SAFE<código><NN>` |
| **Formularios** | Una columna por muestra, grupos repetibles con botón de añadir, casillas, comentarios y equipos del catálogo |
| **Proyectos** | Abrir, guardar, guardar como, recientes, apertura por doble clic |
| **Pestañas** | Varios proyectos abiertos a la vez, más el tablero. Aviso de cambios sin guardar al cerrar pestaña y al cerrar la aplicación |
| **Informe** | HTML A4 con portada, un apartado por página, equipos, comentarios y avisos |
| **Gestión de proyectos** | Tablero por carpeta: columna por proyecto, tarjeta por sección pendiente, avance contado por secciones |
| **Calendario** | Línea de tiempo en semanas ISO: una tarjeta por servicio, estado, recepción de muestras, archivado, aviso de fuera de plazo y filtros por técnico, estado y norma. **Las barras se arrastran**: el centro mueve, los bordes cambian inicio o fin |
| **Cuatro normas** | 60598‑1, 62031, 60529 (IP) y 62262 (IK), cada una con su plantilla y su catálogo de equipos, elegibles desde la portada. Ver sección 16 |
| **Portada** | Elegir norma, abrir proyecto, recientes o saltar al tablero |
| **Grado por muestra** | IP e IK se eligen en la fila de cada muestra, con el atajo «Luminaria ordinaria». La fila es idéntica en las tres normas que la usan |

### Pendiente

| Prioridad | Qué |
|---|---|
| Baja | **Las tarjetas de clase, Ta y partes ‑2 siguen escritas a mano en el XAML.** Solo se muestran y se exigen donde la norma las declara, pero el asterisco de obligatorio es texto fijo. Se generalizó la cabecera entera el 2026‑08‑01 y **el laboratorio pidió revertirlo**: la pantalla de luminarias se da por buena y no se toca |
| Baja | **`MainWindow.xaml` sigue siendo grande.** Al pasar a pestañas, las plantillas de proyecto y de tablero salieron a `Window.Resources`, que era la mitad del problema. Partirlo en varios diccionarios de recursos terminaría el trabajo |
| Alta | **Selectores de fecha y hora.** Hoy se escriben como texto (`20/07/2026 23:40`). Es lo que más molestará en uso real |
| Alta | **Campos calculados de solo lectura.** El radio del arco de lluvia y las dos fuerzas de carga estática están implementados y con tests en `Calculos.cs`, pero la interfaz no sabe mostrar un campo calculado: se rellenan a mano |
| Media | **Selección automática de equipos IP** (`seleccionAutomaticaEquipos`): declarada en la plantilla, no implementada |
| Media | **Perfil de usuario** (DD‑08). Enlaza con lo siguiente: hoy el técnico es texto libre |
| Media | **Lista de técnicos en desplegable.** Consultado el 2026‑08‑06 al pedir el filtro del calendario. Se dejó **para después**: el filtro se rellena con los nombres que ya hay en los proyectos, sin tocar la pantalla de toma de notas, que está dada por buena. Cuando se haga, el desplegable tiene que **aceptar los nombres ya guardados** aunque no estén en la lista, o los proyectos antiguos se quedan con un técnico inválido |
| Baja | **La cabecera del calendario no se queda fija al desplazarse en vertical.** Con muchos proyectos habrá que congelarla |
| Baja | **El calendario no se desplaza solo al arrastrar contra el borde.** Hay que ampliar el zoom antes de mover un servicio muy lejos |
| Media | **Instalador** y asociación de la extensión `.lumproj` |
| Baja | Con 30 muestras el informe A4 no cabe: habría que girar la tabla o partirla |

### Pendiente del laboratorio

**D‑07** (`EQ-CERT` vs `EQ-SAFE`), **D‑13** (los dos criterios de carga estática), **D‑14** (`EN 60598-2-15`), **D‑21** (confirmar la corrección de 7.12) y **D‑22** (si deben validarse las casillas sin enlazar).

Además, la lista de normas que admite la **62031** (`meta.normasCompatibles`) la puse yo por deducción —IP e IK, porque no los lleva dentro— y **el laboratorio no la ha confirmado**.

### Descartado, y por qué

- **Login con usuario, contraseña y recuperación por email** (consultado el 2026‑08‑02). En una aplicación de escritorio con ficheros JSON en una carpeta compartida, un login **no protege nada**: los datos se abren con el Bloc de notas sin pasar por él, y los permisos que comprueba el propio programa se saltan cerrando el programa. Lo que hace falta para la ISO 17025 no es autenticación sino **trazabilidad** —quién guardó y cuándo—, que es el perfil de usuario ya pendiente (DD‑08) y cuesta un par de días. Si algún día hay servidor, la autenticación se delega en **Entra ID**, que el laboratorio ya paga con Microsoft 365, en vez de escribir la nuestra.
- **Varias ventanas** en lugar de pestañas: se valoró por ser mucho más barato, pero el laboratorio prefirió pestañas.

### Cómo retomar

```bash
dotnet test "…\AplicacionTomaNotas\LumNotas.sln"
dotnet run --project "…\AplicacionTomaNotas\src\LumNotas.App"
```

Si los **169 tests** pasan, el motor y las cuatro plantillas están sanos. La mayoría de los cambios de norma se hacen **editando el JSON de esa norma** en `plantilla/`, sin tocar código; añadir una norma entera es dejar caer un fichero `plantilla-*.json` en esa carpeta.

### Punto ciego cerrado

`visibleSi` y `reglaDeCierre` apuntan a ids de regla y `PlantillaTests` no los comprobaba. Ahora `PlantillasDeOtrasNormasTests` valida esas dos referencias **en todas las plantillas**, y al hacerlo apareció un fallo real en luminarias (ver sección 16). Ese mismo fichero comprueba también que los grupos de equipos que pide cada apartado existen en el catálogo de su norma.

---

## 16. Ampliación al resto de normas

### Qué hay en `TomaDeNotasExcelCompleta.xlsx`

El laboratorio aportó el 2026‑07‑31 un segundo libro (0,79 MB, 16 hojas, **575 controles de formulario**) que contiene **tres tomas de notas**, no una:

| Norma | Hojas | Estado |
|---|---|---|
| **LUM — 60598** | `RESUMEN PROYECTO LUM`, `Toma de notas 60598`, `BBDD Equipos 60598`, `Datos ensayos LUM.` | Ya implementada. El laboratorio confirmó que esta hoja **no cambió**, así que se deja tal cual (DD‑31) |
| **62031 — módulos LED** | `RESUMEN 62031`, `Toma de notas 62031`, `BBDD Equipos 62031`, `Datos ensayos 62031` | Implementada |
| **IP‑IK** | `RESUMEN PROYECTO IP-IK`, `Toma de Notas IP-IK`, `BBDD Equipos IP-IK`, `Datos ensayo IP-IK` | Implementada, **partida en dos**: 60529 (IP) y 62262 (IK), por decisión del laboratorio (DD‑29) |

Además aparece `Datos ensayo IK-LUM`, el motor de cálculo del IK para luminarias, que en el primer libro estaba roto con `#REF!` (§5.15). El IK es su propia toma de notas bajo 62262 (DD‑29) **y además una sección dentro de luminarias**, porque el grado IK se elige por muestra.

### Las cuatro normas

| Norma | Fichero | Secciones | Apartados |
|---|---|---|---|
| Luminarias — EN/IEC 60598‑1 y partes ‑2 | `plantilla-60598.v1.json` | 16 | 45 |
| Módulos LED — EN/IEC 62031 | `plantilla-62031.v1.json` | 16 | 26 |
| Grados IP — EN/IEC 60529 | `plantilla-60529.v1.json` | 3 | 3 |
| Grados IK — EN/IEC 62262 | `plantilla-62262.v1.json` | 2 | 2 |

Cada una trae su catálogo de equipos importado por separado (DD‑34): `equipos-60598`, `equipos-62031` (20 grupos), `equipos-60529` (3 grupos) y `equipos-62262` (1 grupo).

Las cabeceras son **muy distintas** entre normas, que es justo lo que obligó a generalizar:

| | LUM 60598 | 62031 | 60529 | 62262 |
|---|---|---|---|---|
| Temperatura | Ta | **Tc** (cápsula) | — | — |
| Clasificación | Clase I/II/III, **vacía al empezar** | **Del módulo** (independiente / a incorporar / integrado) | — | — |
| Grado IP objetivo | por muestra, obligatorio | — | por muestra, obligatorio | por muestra, opcional |
| Grado IK objetivo | por muestra, con «No IK» | — | por muestra, con «No IK» | por muestra, obligatorio |
| Inmersión (profundidad, tiempo, temperatura) | con IPX7 / IPX8 | — | con IPX7 / IPX8 | — |
| Partes ‑2 | sí | — | — | sí, como **tipología de producto** |
| Prefijo de muestra | `EBP_SAFE` | `EBP_SAFE` | `EBP_SAFE` | **`EBP_CLIM`** |

### Qué hubo que generalizar

El motor, el almacenamiento, el árbol, el informe y el tablero ya eran genéricos. Lo que estaba escrito a mano para luminarias y ahora sale de la plantilla:

| Antes | Ahora |
|---|---|
| `RequisitosDelProyecto` listaba en C# los campos obligatorios | Lee los campos con `obligatorio: true` de la plantilla (DD‑35) |
| `DatosProyecto.PrefijoMuestra` era una constante `EBP_SAFE` | `PatronIdentificador`, tomado de `muestras.identificador.patron` (DD‑33) |
| `Partes2`, `IpPrimeraCifra`, `IpSegundaCifra` eran propiedades de C# | Almacén genérico `Seleccion(campo)` por id de campo. Las tres propiedades siguen existiendo como atajos sobre ese almacén, para que los predicados de luminarias se lean con naturalidad |
| `Clase` era un campo aparte | Vive en el almacén general, bajo `proyecto.clase` |
| `App.xaml.cs` cargaba una plantilla fija | `CatalogoDeNormas` lista las que haya en la carpeta; añadir una norma es **dejar caer un fichero**, sin recompilar |

El formato `.lumproj` gana `normas`, `patronIdentificador` y `selecciones`, y **sigue leyendo el formato antiguo**: los proyectos de luminarias ya guardados abren sin perder nada.

### Cómo se elige la norma

**En la portada**, eligiendo su tarjeta. No hay selector en la cabecera: la norma se decide al empezar y no se conmuta sobre un proyecto a medias, porque cada una tiene su cabecera, sus apartados y sus equipos. Al abrir un `.lumproj` se carga la norma con la que nació, no la que estuviera en pantalla.

### Añadir normas a un servicio

En la cabecera del proyecto, tarjeta **«Otras normas de este servicio»** (DD‑30). Qué se puede añadir a qué lo decide el laboratorio y vive en `meta.normasCompatibles` de cada plantilla, no en el código (DD‑37):

| Toma de notas | Puede añadir |
|---|---|
| Luminarias 60598‑1 | solo 62031. **Ni la 60529 ni la 62262**: el IP y el IK ya están dentro |
| Grados IP 60529 | solo IK 62262 |
| Grados IK 62262 | solo IP 60529 |

### Cabecera propia de cada norma

La pantalla de luminarias **no se toca** (decisión del laboratorio, 2026‑08‑01). Lo que una norma declare y no tenga sitio propio en la ventana se pinta como tarjeta aparte, y en luminarias esa lista sale vacía — hay un test que lo vigila.

| Norma | Añade a la cabecera |
|---|---|
| Módulos LED 62031 | **Tc (ºC)** y **clasificación del módulo LED**: independiente / a incorporar / integrado |
| Grados IK 62262 | **Grado IK objetivo** (IK01…IK11) y la casilla **«Sin grado IP objetivo»** dentro de la tarjeta del grado IP |
| Grados IP 60529 | profundidad y temperatura de inmersión |

**Grado IP en la 62262.** Un servicio de IK puede llevar IP o no, así que hay que pronunciarse: o se marcan cifras o se marca «Sin grado IP objetivo». Las dos opciones son excluyentes — marcar la casilla borra las cifras y las desactiva, y marcar una cifra desmarca la casilla — y dejarlo en blanco cuenta como cabecera incompleta.

El rojo de «falta este dato» **solo aparece donde la norma lo exige**: en la 62262 el grado IP es opcional y las partes ‑2 no existen, así que esas tarjetas ya no se pintan en rojo.
| Módulos LED 62031 | 60529 e IK 62262 — **no lo fijó el laboratorio**, se admite porque la 62031 no los lleva dentro. Pendiente de confirmar |

Al marcar una norma:

- Sus secciones se añaden **al final del índice**, con el número de norma delante del título (`62031 · Sección 6 - Marcado`) para saber a cuál pertenece cada apartado.
- Cada norma evalúa con **su propio motor** y usa **su propio catálogo de equipos**.
- El contador de la cabecera **suma todas las normas** (`IndicadorDeAvance.Resultado.Sumar`).
- El **informe HTML las incluye todas**: la portada lista las normas y cada una abre con su portadilla antes de sus apartados.
- El `.lumproj` recuerda cuáles lleva, y al abrirlo se recargan solas.

Desmarcar una norma retira sus apartados pero **no borra lo anotado**: si se vuelve a marcar, los datos siguen ahí.

Los ids de bloque van prefijados por norma para que los datos no se pisen (DD‑36), con un test que lo vigila.

### Defecto encontrado al ampliar los tests

Al pasar la comprobación de referencias a **todas** las plantillas se cerró el punto ciego de `visibleSi` y `reglaDeCierre`, y apareció uno real en luminarias: el campo «Nº de capas» del ensayo de bola apuntaba a `capas.requiereCapas`, que es una **opción de checklist y no una regla**, así que el campo no llegaba a aparecer nunca. Ahora apunta a `R-15.2-02b`, que evalúa esa misma opción.

### Grado IP por muestra (60598‑1 y 60529)

Como en el Excel, el grado IP objetivo de la 60529 se declara **por muestra**: un mismo servicio puede traer productos con objetivos distintos. Los dos desplegables van **junto a cada muestra**, en la tarjeta de identificación, para que se vea que la elección es de esa muestra y no del proyecto; la tarjeta «Grado IP objetivo» del proyecto desaparece en esa norma.

Se declara con `porMuestra: true` en la plantilla, y de ahí sale todo lo demás: `RequisitosDelProyecto` exige el valor **en todas** las muestras, y los predicados (`hayGradoPrimeraCifra`, `hayGradoSegundaCifra`, `requiereDimensiones`, `requiereDuracionIp5x`) leen los grados con `DatosProyecto.GradosDe`, que mira tanto el del proyecto como el de cada muestra. Sin eso, los apartados de ensayo de la 60529 no llegarían a aparecer.

### La fila de muestra es la misma en las tres normas

Desde el 2026‑08‑04 las tres normas que usan muestras pintan **exactamente la misma fila**, en el mismo orden:

```
Muestra 1   [1]   EBP_SAFE250601   ☐ Luminaria ordinaria   1ª cifra [▾]   2ª cifra [▾]   IK [▾]
```

- **60598** y **60529**: el IK arranca en «No IK» y no es obligatorio.
- **62262**: el IK sí es obligatorio; el IP es opcional y se deja en blanco donde no aplique.

Hay un test que compara los campos `porMuestra` de las tres: si alguien añade uno a una norma y se olvida de las otras, salta.

Lo mismo con la **inmersión**: profundidad, tiempo y temperatura del agua aparecen —y se exigen— en **las tres** con la misma condición, que alguna muestra vaya a **IPX7 o IPX8**. La temperatura arranca en 25 ºC.

Esa comprobación **no lleva la lista de normas escrita a mano**: `TodaNormaConGradoIpPideLosDatosDeInmersion` recorre las plantillas instaladas y exige los tres campos a cualquiera que declare `ipSegundaCifra`. Se hizo así después de que la 62262 se quedara fuera de una lista escrita a mano y marcar IPX8 allí no pidiera nada. Una norma nueva con cifras IP queda cubierta sin tocar el test.

**Solo la norma con la que nació el proyecto decide el prefijo de las muestras.** Añadir otra norma llama a `AplicarA(datos, principal: false)`, que registra la norma pero no toca el patrón: antes, añadir el IP a un servicio de IK renombraba sus muestras de `EBP_CLIM` a `EBP_SAFE` sin avisar.

**Luminarias funciona igual desde el 2026‑08‑01**, a petición del laboratorio: mismos dos desplegables por muestra y misma casilla **«Luminaria ordinaria»**, que rellena IP2X e IPX0 —una ordinaria es IP20— y se desmarca sola si luego se elige otro grado. `Calculos.AlturaEfectiva` mira el objetivo de **cada** muestra, así que el arco de lluvia se parte por la mitad solo en las que van a IPX4.

**La 62262 va igual**, con una fila por muestra que lleva su **grado IK objetivo** —obligatorio en todas— además del grado IP, que ahí es opcional: se deja en blanco en las muestras que no lo tengan. Con eso desaparecieron la tarjeta «Grado IK objetivo» y la casilla «Sin grado IP objetivo», que ya no hacen falta.

El mecanismo es genérico: **cualquier campo de cabecera declarado `porMuestra` se pinta en la fila de su muestra**, con el rótulo que indique `etiquetaCorta`, y no genera tarjeta propia.

### El IK dentro de luminarias

Luminarias tiene su propio **grado IK objetivo por muestra**, con **«No IK» como opción por defecto**, y una sección **«Ensayo de IK - EN/IEC 62262»** que aparece sola en cuanto alguna muestra lleva un grado distinto de «No IK» (`visibleSi: R-60598-hayIk`). Por eso luminarias ya no admite añadir la 62262 como norma aparte.

El valor por defecto de un desplegable se declara con `porDefecto` y **no se guarda hasta que el técnico elige**: así se distingue lo que ha decidido de lo que no ha tocado, y «No IK» elegido a propósito tampoco activa la sección.
