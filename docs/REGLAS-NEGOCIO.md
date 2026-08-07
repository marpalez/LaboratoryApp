# Reglas de negocio — Toma de notas de ensayos de luminarias (EN/IEC 60598)

**Origen:** `TomaDeNotasExcel.xlsx`, plantilla v2.1
**Extraído el:** 2026-07-29 (última modificación del libro: 2026-07-29)
**Versión del documento:** 3.98 — documento de traspaso al día
**Actualizado:** 2026‑08‑06
**Propósito:** nació como documento de revisión previo a programar —cada regla extraída del Excel debía ser **confirmada, corregida o eliminada** por el laboratorio— y hoy es además el **documento de traspaso** de la aplicación: qué hace, por qué se decidió así y qué queda pendiente. Lo que sigue sin confirmar va marcado con ⏳.

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

**El orden lo marca el número, no la fecha.** `D‑xx` son decisiones de negocio y `DD‑xx` de desarrollo, y cada uno se asignó al tomarse; las fechas salen del reloj del equipo y alguna se quedó descolocada. Ante una duda de qué vino antes, manda el número.

| Fecha | Punto | Decisión |
|---|---|---|
| 2026‑07‑29 | Alcance | Solo EN/IEC 60598 (‑1 y partes ‑2). IK y 62031 fuera |
| 2026‑07‑29 | Edición | Varios técnicos, **por turnos**: un fichero por proyecto con registro de autoría, sin edición simultánea. *(Al implementar quedó en «el último en guardar manda», con la planificación escrita por un solo camino — DD‑50.)* |
| 2026‑07‑29 | D‑02 | **Numeración canónica: la de 2024** (7 = Construcción, 11 = IP). La antigua queda solo como alias de visualización |
| 2026‑07‑29 | D‑04 | Se acepta el comportamiento actual: **un 0 cuenta como dato introducido** |
| 2026‑07‑29 | D‑05 | Corregido el diagnóstico: los acondicionamientos son correctos. **Se arregla el ensayo de bola** (comparaciones de solo hora) |
| 2026‑07‑29 | D‑10 | Se acepta para el borrador: el agregador de partes ‑2 **no incluye ‑2‑5 ni ‑2‑22** |
| 2026‑07‑29 | D‑19 | Se acepta: ‑2‑7 sigue sin casilla |
| 2026‑07‑29 | D‑20 | Se acepta: «OTRO» sigue fuera de la comprobación de normas |
| 2026‑07‑29 | D‑18 | **Resuelto:** se mantienen los pesos y se añade un contador de apartados completados |
| 2026‑07‑29 | D‑11 | **Resuelto:** la aplicación no firma. Se firma el PDF impreso, fuera del programa |
| 2026‑07‑29 | DD‑01 | **Stack: .NET 10 + WPF** (nació en .NET 8; ver DD‑107) |
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
| 2026‑08‑07 | DD‑149 | **El nombre de un apartado no se escribe dos veces** (`AltaDeProyecto.SeccionDeDatos`). El aviso de que no se puede guardar mandaba a arreglarlo «en «Datos del proyecto»» — **un apartado que ya no se llamaba así**: con el glosario (DD‑145) el nodo pasó a «Datos de la TdN» y el aviso se quedó atrás, porque lo redacta el núcleo y el rótulo lo pone la ventana y **no se ven entre sí**. Lo peor no era la palabra sino que el aviso **mandaba a un sitio inexistente**, que es justo lo contrario de lo que hace un aviso. Ahora la constante vive en el núcleo y la lee el rótulo, así que no puede haber dos nombres. El test lo comprueba **por la constante y no por el texto**, o volvería a quedarse en verde señalando lo que ya no existe. De paso se corrigieron los rezagados del mismo glosario que sí se ven: los dos avisos de portada («carpeta de tomas de notas»), «Esa toma de notas ya existe», «Cerrar la toma de notas», la cuenta del diálogo de carpetas, el aviso de renombrar técnicos, «Buscando / Leyendo tomas de notas» y el error de lectura del repositorio. **Lo que no se tocó**: `"proyecto"` como ámbito de datos dentro de las plantillas y de cada `.lmnlab`, `CarpetaDeProyectos` en el `ajustes.json` y los nombres de clase — renombrarlos dejaría ilegible lo ensayado (DD‑145) |
| 2026‑08‑07 | DD‑148 | **Los dos «Exportar» son botones discretos** (`BotonDiscreto`): sin fondo, la letra en negrita y un borde gris. Eran negros, como lo más importante de su barra, y lo pidió el laboratorio al revés: **el peso de un botón no lo da la acción sino cuántas veces se pulsa**, y exportar es de higos a brevas mientras «Guardar» y «Actualizar» son de todos los días. Van los dos igual —el listado de la BBDD y el informe de la TdN— porque es la misma acción y no puede cambiar de aspecto según la pestaña, y el estilo está escrito **una sola vez** en `App.xaml` por lo mismo que los textos de planificación (DD‑143). Tiene un detalle que no es evidente: el estilo general resuelve el «encima» **bajando la opacidad**, que sobre un fondo transparente no se ve, así que este lleva su propio disparador que lo tiñe — sin él quedaría un botón que no responde al ratón, y eso se lee como desactivado |
| 2026‑08‑07 | DD‑147 | **«Planificación de la TdN» se rediseña igual que el alta, pero con dos ⓘ y no siete.** Los dos diálogos planifican lo mismo y ahora se parecen: mismas tintas, mismos rótulos, mismo pie con los botones fuera del desplazamiento. El contenido se reparte en **cuatro tarjetas** —fechas, estado e importe, muestras, grupo y archivar— en vez de un chorro de dieciséis controles apilados. **La ⓘ solo donde hace falta un párrafo**: el grupo y archivar. El laboratorio lo pidió así —«no te pases»— y tiene razón: un formulario sembrado de circulitos cansa más que los renglones que sustituye, y el resto de campos se entienden con su rótulo. La semana, el candado y «Quitar fechas» pasan a **un solo renglón**: eran tres filas y las tres hablan de las mismas dos fechas. El título pasa a **«Planificación de la TdN»**, que se había quedado sin aplicar del glosario (DD‑145) |
| 2026‑08‑07 | DD‑146 | **Las aclaraciones fijas del alta pasan a una ⓘ**, y las dos zonas dejan de pesar lo mismo. «Nueva toma de notas» tenía **cinco renglones de texto gris permanente** —el ejemplo del código, para qué son las fechas, qué es un grupo (cuatro líneas desde DD‑143)— que entre todos gastaban un tercio del alto para explicar cosas que se leen una vez en la vida. Van a un círculo gris de 16 px con el texto en el consejo emergente. **No se pierde nada**: el ejemplo del código sigue siempre a la vista, pero **dentro de la caja mientras está vacía**, que era el motivo por el que estaba fijo (quien da de alta tiene que poder copiar el patrón), y el renglón del grupo pasa a enseñar **solo la confirmación** de con qué se va a enlazar, que es lo único que cambia al teclear. La ⓘ va como `ContentControl` **sin foco ni parada de tabulador**: quien rellena de teclado no se la encuentra en medio. **Jerarquía**: lo obligatorio en tarjeta blanca con marco, lo opcional sobre gris hundido — antes se distinguían solo por una pastilla de 4 px y pesaban igual, que es no tener jerarquía. Y la norma pasa a ancho completo, que a media columna cortaba designaciones como «EN 60529:1991 + A1:2000 + A2:2013 + AC:2019‑02…» |
| 2026‑08‑07 | DD‑145 | **Hay un glosario y la interfaz lo cumple.** Cuatro palabras para lo mismo —proyecto, servicio, familia, toma de notas— repartidas por las pantallas según quién escribió cada una. Se fijó con el laboratorio: **toma de notas** (o **TdN** donde no cabe), **servicio** solo para el código de 9, **día** en vez de jornada, y **semana**. Lo que lo decidió no fue el gusto sino una comprobación: **el código de servicio no agrupa nada** —lo que junta las TdN en el calendario es el campo «Grupo», texto libre y vacío por defecto—, así que «servicio» era una palabra sin trabajo que hacer. Se cambiaron **los rótulos, no el código**: `"proyecto"` es el ámbito de datos dentro de las cinco plantillas y de cada `.lmnlab` guardado, y `CarpetaDeProyectos` la clave del `ajustes.json` de cada equipo — renombrarlos dejaría ilegible lo ensayado y borraría las carpetas de los seis ordenadores. La pestaña pasa a **«Planificación de TdN y servicios»**, que es lo único que conserva las dos palabras porque dentro se ven las dos cosas |
| 2026‑08‑07 | DD‑144 | **El importe se rotula «Importe de la familia», no «de la oferta»**, y el trabajo que sale de él se cuenta en **días**, no en «jornadas». No es cosmética: se llamaba «oferta» y la oferta es del **servicio entero**, así que el rótulo empujaba a poner los 8 000 € del trabajo en una familia y dejar las otras tres en blanco — que es exactamente lo que hace que la carga diga que el técnico está libre cuando no lo está. Va con la corrección del texto de grupo (DD‑143): **cada familia lleva sus fechas y su importe**. Cambia en los **tres** sitios donde se lee ese dato —los dos diálogos que lo editan y el panel de planificación de la toma de notas, que solo lo enseña— porque un rótulo distinto para el mismo campo se lee como dos campos distintos |
| 2026‑08‑07 | DD‑143 | **Los dos diálogos que planifican explican lo mismo con las mismas palabras**, y esas palabras están escritas **una sola vez** (`TextosDePlanificacion`). «Nueva toma de notas» y «Planificación del servicio» hacen lo mismo y lo contaban distinto, cada uno con el texto copiado en **dos sitios** —el XAML para el rótulo inicial y el código para cuando se vacía la casilla—: cuatro copias de la misma frase. El XAML las lee con `{x:Static}`, así que ya no hay copia que se pueda quedar atrás. **No es manía de limpieza**: con el texto duplicado, corregir uno y olvidar el otro no es un descuido posible sino lo que pasa siempre — el diálogo de filtros estuvo dos días diciendo algo que había dejado de ser cierto. De paso se corrigieron **tres frases que ya mentían**: el grupo decía que «las fechas y el importe se ponen solo en una de ellas» cuando hoy **cada familia necesita las suyas** —el calendario las encadena cada una durando lo suyo (DD‑123) y la carga cuenta cada importe—; archivar remitía a un botón «Ver archivados» **que ya no existe**, sustituido por los filtros compartidos; y el candado tenía debajo un renglón que repetía su propio consejo emergente, que se quitó |
| 2026‑08‑07 | DD‑142 | **Un servicio terminado no cuenta en la carga del técnico.** La tabla contesta «¿cabe lo que le queda?», y un ensayo hecho no ocupa a nadie. Lo destapó el laboratorio probando: **Raúl, diciembre de 2026, todo terminado y un 122 %** — la tabla avisaba de una sobrecarga que no existía, que es la forma más rápida de que se deje de mirar. No fue una decisión sino un descuido con dos días de vida: DD‑79 ya había dado por malo justo esto, se sacó excluyendo lo terminado del filtro, y el 2026‑08‑05 lo terminado volvió al filtro **sin que nadie se acordara de la carga**. El arreglo va donde tenía que estar desde el principio: **en `CargaPorTecnico`, no en el filtro ni en la vista**, para que lo terminado se siga viendo en el tablero, el calendario y la BBDD y solo deje de contar donde la pregunta es si cabe. Se descarta **antes de calcular los meses**, así que un servicio cerrado en marzo tampoco abre una columna de marzo vacía. `ServicioPlanificado` pasa a llevar el estado **sin valor por defecto**: con uno, la próxima vista que llame a este cálculo se lo dejaría sin poner y los terminados volverían a contar en silencio — exactamente como acaba de pasar |
| 2026‑08‑07 | DD‑141 | **La barra del calendario lleva el color de su estado y nada más.** Se pintaba de rojo cuando la fecha de fin ya había pasado y el servicio no estaba terminado. Se quitó porque **tapaba el estado**: el color de la barra es lo único que lo dice —gris por planificar, morado planificado, azul en curso, ámbar pendiente de cliente, verde terminado— y el rojo lo borraba, de modo que un servicio en curso y otro esperando al cliente se veían iguales por haberse pasado un día de la fecha. **Fuera de plazo no se deja de decir**, se dice donde no compite: en el consejo emergente de la tarjeta y en el «N fuera de plazo» de la cabecera de cada técnico. La propiedad `Retrasado` se queda, que es la que alimenta esas dos |
| 2026‑08‑06 | DD‑140 | **El listado de la BBDD se exporta a HTML en A4 apaisado, y solo el listado.** Mismo formato y mismo motivo que el informe de ensayo (DD‑06): es texto, sin dependencias, y de ahí sale el PDF con Ctrl+P. **Apaisado porque son once columnas**: en vertical quedan 180 mm útiles y habría que partir la tabla o encoger la letra hasta no leerse. **Sale el listado entero, no lo que cabe en el monitor** — si el filtro deja cuatro, cuatro; si hay cien, cien. No es una obviedad: la tabla está virtualizada (DD‑131), así que **en pantalla solo existen las filas que se ven**, y exportar recorriendo lo dibujado habría dado quince pareciendo correcto; se exporta el modelo. Para que el papel no pueda discrepar de la pantalla, **`FilaDeBbdd` se mudó al núcleo** y las columnas se declaran una sola vez (`FilaDeBbdd.Columnas`), de donde beben la tabla y el HTML. **La cabecera lleva el título y la cuenta, y nada más.** Se entregó con tres cosas más —fecha y hora de generación, la línea de filtros y un aviso de que no es un informe de ensayo— y **el laboratorio quitó las tres al revisarlo** (2026‑08‑07). El argumento con el que se habían puesto era que un listado filtrado impreso sin decirlo miente por omisión; pesó más que **el listado se mira en el momento y junto a la pantalla de la que sale**, así que repetir ahí lo que ya se sabe solo robaba sitio a la tabla. Al quitar la fecha hubo que quitarla **también del `<title>`**, que no se ve en la página pero los navegadores lo imprimen en la cabecera de cada hoja: se habría colado por detrás lo que se acababa de quitar por delante. Lo vigila un test, porque las tres son de las que vuelven a aparecer «por si acaso» al tocar la cabecera. **No se pone en el tablero ni en el calendario** (lo descartó el laboratorio y coincide con lo razonable): la BBDD ya es una tabla y exportarla es escribirla; las otras dos son dibujos —tarjetas en columnas, barras sobre un eje de semanas— y llevarlas al papel no sería exportar sino inventar otro documento con otro mantenimiento. **El botón va el último de la barra de mandos** y nació negro, el mismo negro que «Exportar» en la toma de notas. Se quedó sin fondo y en negrita al día siguiente, y con él el otro (DD‑148). Nació en una franja propia encima de la tabla, junto a un «47 tomas de notas», y las dos cosas se corrigieron al verlas: la franja gastaba un renglón entero de tabla para un botón, y la cuenta **ya la daba la línea de estado** dos renglones más arriba —«47 proyectos \| 186 fuera del filtro»—, con lo que decirla otra vez y con otras palabras solo hacía dudar de si hablaban de lo mismo. Que aparezca y desaparezca al cambiar de vista no mueve nada **porque va el último**: delante de él no hay nada que recolocar |
| 2026‑08‑06 | DD‑139 | **Los nodos del índice se anuncian por su rótulo** (`INodoDelIndice.Rotulo`). Los 31 nodos de una toma de notas se anunciaban como `LumNotas.App.ViewModels.SeccionViewModel`: WPF solo deduce el nombre accesible cuando la cabecera es una **cadena**, y aquí son plantillas con rejilla, píldora y punto de color, así que se rinde y llama a `ToString()`. **Lo que lo empujó no fue la accesibilidad sino la verificación**: sin nombres, los guiones que manejan el programa —única forma de comprobar un cambio de pantalla, porque ninguno de los 577 tests toca la interfaz— tienen que pulsar **por posición** (`items.Item(4)`), y eso se rompe en silencio en cuanto una sección deja de aplicar y desaparece del árbol: el guion sigue en verde pero sobre el apartado equivocado. Ahora se busca por «Sección 7 ‑ Construcción», que o encuentra lo que busca o falla. **El nombre no lleva la cuenta de apartados** aunque se vea al lado: cambia sola al rellenar y dejaría de encontrarse; eso vive en el ToolTip, que UIA publica como texto de ayuda. **Y va en el contenedor, no en la plantilla**: el elemento que ve la automatización es el `TreeViewItem`, no la rejilla de dentro — puesto dentro de la plantilla no hace nada, que fue el primer intento y hubo que deshacerlo. Quedan sin nombre 38 botones, 7 desplegables y 4 cajas de la toma de notas: se dejan apuntados, no urgen |
| 2026‑08‑06 | DD‑138 | **Lo que señala el índice lo manda el documento, no el árbol.** Cada nodo lleva su propio `Seleccionado` y `TreeViewItem.IsSelected` se ata a él **en los dos sentidos**. Nace de un fallo: en una toma de notas nueva se pulsaba «Planificación» y **ya no había forma de volver a «Datos del proyecto»**. El motivo es que `TreeView.SelectedItem` es de **solo lectura**, así que la navegación colgaba de `SelectedItemChanged` — y «Planificación» se alcanza por un botón de la barra que no está en el árbol. Al pulsarlo, el índice **seguía señalando «Datos del proyecto»**, con lo que volver a pulsar ese nodo no cambiaba la selección y **no disparaba ningún evento**. Dos fallos por el precio de uno: el índice mentía sobre dónde estabas, y el nodo señalado se volvía el único al que no se podía ir. Con la atadura, salir por un botón **apaga la marca del árbol** y el nodo vuelve a responder. Solo hace falta apagar: al encender uno, el árbol apaga el anterior por su cuenta, porque no deja marcar dos a la vez |
| 2026‑08‑06 | DD‑137 | **El % que enseñan el tablero y el calendario es el ponderado, el mismo que estampa el informe.** Se pidió «un % de proyecto realizado, aunque sea estimación» y el programa sabía contarlo de tres maneras: por **secciones** (`7/16`, lo que ya salía en la tarjeta), por **apartados**, y por **peso** —`IndicadorDeAvance`, heredado del Excel (DD‑11 / D‑18) y ya impreso en el informe—. Se estuvo a punto de elegir el de secciones por ser el que ya estaba a mano, hasta que apareció el tercero: con él, **el tablero habría dicho 6 % y el PDF firmado 45 %**, y nadie sabría cuál mirar. Manda el del informe. Además es el único que se acerca a medir esfuerzo: por secciones, cerrar la sección 7 —trece apartados, un día de trabajo— movía el marcador seis puntos. **Nunca dice 100 % por redondeo**: se trunca hacia abajo salvo cuando no queda peso, porque un 99,6 % redondeado al alza pondría el cartel de acabado en un servicio al que le falta un ensayo. **Y sin pesos declarados no hay número, no un cero**: un cero sería mentira fija hasta en un servicio terminado. No costó rendimiento: el escaneo ya construía el motor de reglas de cada norma y ahora se reutiliza en vez de montar otro, así que 250 proyectos siguen leyéndose en 1,3 s. **Por el camino se dio por buena una lectura falsa de las plantillas** —contar los pesos leyendo el JSON a mano, sin bajar a los subapartados, dio «solo 18 conceptos y endurancia a cero» y con eso se llegó a proponer al laboratorio que rellenara pesos que ya estaban puestos—. Contado con el propio motor, que es lo que había que hacer desde el principio, son **23 conceptos y 217 puntos, con endurancia en 100**. La lección: para saber qué dice una plantilla, se le pregunta al motor, no al fichero |
| 2026‑08‑06 | DD‑136 | **La cabecera de cada columna del tablero lleva los dos iconos del calendario**: la caja cuando las muestras ya están en el laboratorio y el candado cuando las fechas están blindadas. El tablero es donde se decide **qué se coge hoy**, y eso no lo contesta solo el avance: un servicio con doce apartados por delante cuya muestra sigue en el transportista no se puede empezar, y hasta ahora ese dato había que ir a buscarlo al calendario. **No se dibujó nada nuevo** — son los mismos `IconoCaja` e `IconoCandado`, que van como trazo justo para poder cambiar de color según el fondo: blancos sobre la barra, **ámbar** el que empuja a hacer algo y **gris** el que solo informa. La ausencia es la señal contraria: no hay icono de «muestras aún sin llegar», porque dieciséis columnas con un icono tachado no dicen nada. El rótulo va en una **rejilla** y no en un `StackPanel` horizontal, o dejaría de envolver — la misma trampa que ya estaba escrita para las barras del calendario |
| 2026‑08‑06 | DD‑135 | **La portada se rediseña con tres pesos, rejillas y nombres accesibles.** La auditoría encontró que tenía **dos primarias** —el azul y el verde, idénticos de tamaño y forma—, y dos primarias son ninguna: el ojo se partía en dos al entrar y no volvía a tener ancla. Se resuelve con **una primaria por zona** y no una por pantalla: forzar una sola contradiría DD‑80, que dice que tomar notas y gestionar pesan lo mismo, y mentiría sobre para quién es el programa. Tres estilos —`AccionPrimaria` (relleno), `AccionSecundaria` (contorno), `AccionTerciaria` (sin caja)— sustituyen a seis que declaraban **un solo estado** cada uno; ahora los cinco: normal, encima, pulsado, **foco** y desactivado. Falta *cargando* a propósito: ninguna acción de esta pantalla tarda lo suficiente. **Lo que se arregló y era falta de accesibilidad, no de gusto**: el foco de teclado no se veía (WCAG 2.4.7) porque los estilos rehacían la plantilla y perdían el adorno de WPF; el rótulo «RECIENTES» daba 2,54:1 y la píldora de versión 4,06:1, los dos por debajo de AA; los objetivos táctiles de las recientes medían 25 px; y **los dieciséis botones se anunciaban sin nombre** (WCAG 4.1.2) — un lector de pantalla decía «botón» y callaba. **Y de espacio**: cinco rectángulos a todo lo ancho para un nombre de 24 caracteres tiraban el ancho y gastaban el alto; recientes, normas y las cuatro vistas pasan a rejillas de dos columnas, y las vistas ganan icono porque «Tablero», «Calendario», «Carga» y «BBDD» son cuatro palabras abstractas que había que leer una a una. De nueve tamaños de texto a **tres** (26/14/12) y espaciado en 4/8. **Sigue sin resolverse el fondo**: son 16 destinos que ya están todos en la barra de menús, y la única solución real —esconder las normas detrás de un botón— la descartó el laboratorio |
| 2026‑08‑06 | DD‑134 | **La plantilla declara la edición de la norma** (`meta.edicion`), y la portada la enseña: «Luminarias \| Ed. 9». Va en un campo **aparte del año de publicación** y no se deduce de él: son dos cosas distintas y el programa ya las confundió una vez (DD‑101). Es opcional — sin ella se enseña solo el nombre, porque un «Ed.» sin número parece un dato a medio escribir. Al darlas, el laboratorio corrigió dos cosas más: la **60598‑1 de 2021 es la 9.ª y la de 2024 la 10.ª** —la ficha de DD‑101 decía lo contrario— y la **designación de la 60529 estaba mal**: no es `EN 60529:2018` sino `EN 60529:1991 + A1:2000 + A2:2013 + AC:2019-02 + AC:2016-12 + corrigendum May 1993`. Eso obligó a **renombrar su identidad** a `60529_1991`, porque hay un test que exige que la designación lleve dentro el año que identifica a la norma —lo único que separa dos plantillas de la misma— y con la designación nueva dejaba de cumplirse. Con el renombrado hubo que mover también el catálogo de equipos —se llama como su plantilla— y actualizar el `normasCompatibles` de las otras dos que la citaban; **lo cazaron los tests, no la vista**. Se llegó a poner `60529_2018` en `idsAnteriores` para no dejar huérfano lo ya guardado (DD‑95), y **el laboratorio pidió quitarlo**: se está en desarrollo, no hay ensayos que conservar, y una norma mal designada no debe poder abrirse ni por compatibilidad. Se borraron las 36 tomas de notas de prueba que la citaban, sus dos ficheros de las tres carpetas donde estaban publicados, y el reconocimiento del id viejo. **De la 60529 errónea no queda nada** |
| 2026‑08‑06 | DD‑133 | **Se quita la franja oscura del pie de ventana.** Ocupaba sitio en todas las pantallas para no decir nada casi nunca, y una franja que casi siempre está vacía es justo la que nadie mira el día que importa. Lo que se decía por ahí se repartió en dos: **lo que falla va a una ventana** —no se pudo abrir, guardar o exportar, qué falta para poder guardar, y que la toma de notas se registró con otra versión de la norma—, porque un «no se pudo guardar» tiene que interrumpir; y **las confirmaciones se borraron** —«Guardado en…», «Abierto…», «Exportado a…», «Ya estaba abierto en otra pestaña»—, porque ya se veían por otro lado: la ruta está bajo el título, el punto de «sin guardar» desaparece solo al guardar, el informe se abre en el visor y la pestaña salta sola. Se listaron los diez mensajes antes de tocar nada: **borrar la franja sin más habría dejado cuatro fallos en silencio**, incluido el aviso de que no se puede guardar, que es de esta misma mañana (DD‑130). Los modelos de vista no llaman a `MessageBox`: piden `ServiciosDeVentana.Avisar`, igual que ya piden abrir un fichero |
| 2026‑08‑06 | DD‑132 | **Una instalación nueva no trae ningún técnico.** Hasta hoy el programa venía con **seis nombres cableados** —los del laboratorio el día que se escribió el código— y eso está mal por dos motivos: mete personas concretas de un laboratorio concreto dentro del ejecutable, y quien lo instale en otro sitio se encuentra una plantilla ajena que tiene que borrar a mano. La lista la hace cada laboratorio desde `Configuración`, que es de donde tiene que salir. **Lo único que viene es el cajón de los que están sin asignar**, y con el mismo texto que ya usaban el calendario, la carga y los filtros —`(sin técnico)`, con sus paréntesis— y no uno parecido: si el catálogo dijera «Sin técnico» y las vistas «(sin técnico)», un servicio al que alguien le elige el cajón a mano y otro que sencillamente no tiene técnico saldrían en **dos filas distintas queriendo decir lo mismo**. Los paréntesis siguen haciendo su trabajo —nadie se llama así, luego no puede chocar con una persona—, pero **deja de ser cierto que el rótulo no esté en el catálogo**: el test que lo exigía se dio la vuelta, con el motivo escrito dentro. **A quien ya tenga su `tecnicos.json` no le cambia nada**: solo afecta a las instalaciones sin fichero, o con el fichero roto |
| 2026‑08‑06 | DD‑131 | **El tablero y la BBDD se dibujan virtualizados**: solo existen las columnas y las filas que se ven. Con 250 proyectos, cambiar un filtro tardaba **1,7 s** en volver a pintarse, y **se pagaba igual mirando otra vista** —plegar una vista no destruye lo que ya se creó, así que el tablero se rehacía entero por detrás—. Ahora son **141 ms**: doce veces menos. Hicieron falta cuatro cosas a la vez: el panel virtualizador, que la lista sea dueña de **su propio** `ScrollViewer` —dentro de uno de fuera se mide con tamaño infinito y no virtualiza nada, que es la sexta trampa aplicada al alto—, `CanContentScroll` para que cuente elementos, y `ScrollUnit=Pixel` para que aun así se desplace suave. **El calendario se dejó como estaba a propósito**: sus barras van colocadas por margen y no en fila, virtualizarlas obligaría a rehacer el arrastre, la sincronización de las dos columnas y la barra horizontal — lo más delicado de la aplicación — para ganar los ~0,9 s que quedan. Medido antes y después con un banco de 250 proyectos en `Clientes/TECNOnnn/TomaDeNotas/`; ver «Qué se midió y qué no» |
| 2026‑08‑06 | DD‑130 | **El código de la toma de notas se exige entero por los tres caminos**: al dar de alta, para guardar y para empezar a ensayar. Antes la regla dependía de por dónde hubiera entrado la toma de notas — lo creado con «Nueva toma de notas» llevaba sus 14 caracteres y lo abierto de un fichero podía llevar nueve. La longitud se mudó a `CodigoDeServicio`, que ya guardaba las otras dos —nueve del servicio y once de la familia—, y las tres se recortan **del mismo código**: uno corto las deja a todas mal. **Se propuso dejar guardar** con el código a medias, para no dejar atrapados a los proyectos anteriores a la regla, y **el laboratorio lo rechazó el mismo día**: con esa excepción puesta, esos proyectos se quedaban a medias para siempre, porque nada obligaba nunca a completarlos. Así que hay que arreglarlo antes de poder escribir; el aviso dice qué falta y la vista salta a la cabecera. **No se pierde trabajo por el camino**: si al cerrar se elige «Guardar» y el guardado se rechaza, `ConfirmarSiHayCambios` ve que los cambios siguen ahí y **cancela el cierre** en vez de tirarlos. Tampoco afecta a la cadena del grupo (DD‑123), que escribe solo la planificación por otro camino. Con ello los dos avisos pasan de «falta por rellenar» y «no se puede guardar sin» a **«completar»**: desde ahora un campo puede estar escrito y aun así salir en la lista |
| 2026‑08‑06 | DD‑129 | **Una fila del calendario deja de ser un trabajo y pasa a ser un carril**: caben todos los que no se pisen. Antes, un técnico con veinte proyectos daba veinte renglones aunque fueran uno detrás de otro y no coincidieran nunca, y el calendario se leía **bajando** cuando lo que se quiere leer es el tiempo, que va en horizontal. Se recorren por fecha de inicio y **cada uno cae en el primer carril donde quepa**, el más alto disponible; con eso salen tantas filas como trabajos coincidan **el día más cargado**, que es el mínimo, sin probar combinaciones. **Compartir un solo día ya es pisarse**: dos barras pegadas sin hueco se leerían como una sola barra larga. Los carriles son **por técnico**, o dos técnicos que trabajan las mismas semanas compartirían fila y la cabecera dejaría de decir de quién es lo que hay debajo. **Se recolocan al soltar, nunca durante el arrastre** — rehacerlos a media faena destruiría la tarjeta que tiene cogida el ratón. Arrastra dos consecuencias: la columna de la izquierda **se queda sin nombres** —una fila son varios trabajos, y sin agrupar por técnico se encoge a cero, que si no serían 230 px tirados— y **abrir la toma de notas se pasa al botón derecho**, que es donde no estorba al arrastre; abre además la familia que se pulsa y no la cabecera del grupo. El reparto vive en `CarrilesDelCalendario`, en el núcleo, para poder probarlo sin ratón |
| 2026‑08‑05 | DD‑128 | **«Cualquier estado» es lo que hace buscable la BBDD.** Desde que los filtros son un solo juego para las cuatro vistas, sin esta opción no había forma de ver a la vez lo terminado y lo archivado: encontrar un servicio de hace tres años obligaba a probar estados de uno en uno hasta acertar. **No se pone por defecto y sigue sin ser lo normal** —con lo terminado y lo archivado dentro, la carga mensual sale inflada por trabajo que ya no existe—, así que se pide a sabiendas. No confundirla con «Todos», que es el **nombre antiguo de «En desarrollo»**: ya no se ofrece, pero se sigue reconociendo con ese significado para que quien lo tenga guardado no se encuentre el tablero en blanco |
| 2026‑08‑05 | DD‑123 | **Las fechas de un trabajo enlazado se escriben, no solo se dibujan.** Hasta ahora la cadena se calculaba al pintar y cada fichero conservaba las suyas, así que **el diálogo de planificación decía una cosa y la línea de tiempo otra**: dos verdades. Ahora, al guardar cualquier toma de notas de un grupo, `CadenaDelGrupo` pone el trabajo en fila y **escribe** el resultado. Reglas: la primera conserva su fecha de inicio; cada una de las siguientes empieza **al día siguiente** del fin de la anterior —no el mismo día, que contaba la frontera dos veces— y **conserva su duración**; sin fecha de fin se le da una semana; sin ninguna fecha, empieza mañana. **El orden lo dan las fechas de inicio y nada más**: para adelantar una familia se le pone una fecha anterior, y con la misma fecha gana la que se acaba de tocar. No hay número de orden guardado ni botones de «adelantar/atrasar» — sería un segundo dato diciendo lo mismo, y en cuanto se desincronizara habría dos verdades otra vez. **Solo se toca la planificación** (DD‑53): las fechas que rellena el técnico en cada ensayo viven en otro sitio del fichero y la exportación no mira la planificación. Como se escribe en ficheros que nadie ha abierto, **se avisa**: un aviso aparte —el del escaneo lo pisaría— nombra las que se han movido |
| 2026‑08‑05 | DD‑122 | **El calendario dibuja siempre medio año por detrás del último trabajo.** Antes dejaba dos semanas de margen, y arrastrar un trabajo hasta el borde lo dejaba sin calendario debajo donde soltarlo: había que parar, pedir sitio con «▶» y volver. Donde peor se veía era en el **salto de año**, porque el año siguiente ni se dibujaba. Solo por detrás: hacia atrás no se planifica, y estirar por la izquierda solo obligaría a desplazarse más para llegar a lo de hoy. **«▶» sigue estando** para ir más lejos de esos seis meses. De paso se arregló que **el eje se encuadraba con las fechas de la cabecera y no con las del trabajo entero**: un grupo de cuatro familias estiraba el calendario solo lo que ocupaba la primera, así que al arrastrarlo al límite no se dibujaba nada más — que es justo el fallo que dio la cara |
| 2026‑08‑05 | DD‑121 | **Cada familia dura lo suyo.** Lo que manda es su **duración** —lo que va de su inicio a su fin—, no la fecha en que acaba. Antes se razonaba con **fechas de corte**, y en cuanto la fecha de una familia no servía de corte —caía por detrás de donde iba el reparto, o justo en el final del trabajo— sus fechas se tiraban y esa familia se repartía el hueco **a partes iguales** con las de al lado. Lo notó el laboratorio: dos familias planificadas de cinco y de quince días salían **del mismo tamaño**. Pasaba siempre que la primera abarcaba todo el trabajo, que es justo como se planifica al anexar una segunda a un servicio ya metido en el calendario. Del inicio propio de una familia se sigue ignorando **dónde la pone** (DD‑118) pero **ya no cuánto dice que dura**. En consecuencia, **el trabajo acaba donde acaba la cadena** y no en la fecha más tardía que haya escrita: la anexada se planificó sin saber dónde iba a caer. Y **la que no dice cuánto dura, dura como las demás** —la media de las que sí— en vez de partir la barra por la mitad y quitarle el plazo a la que sí estaba planificada |
| 2026‑08‑05 | DD‑120 | **La última familia de un trabajo llega siempre hasta donde acaba el trabajo**, aunque tenga escrita una fecha de fin anterior. No hay nada detrás a lo que dejarle sitio, y es lo que se guarda al estirar ese borde: si cortara por su fecha, al arrastrar quedaría un hueco al final. De paso se arregló un fallo del reparto que venía de DD‑118: cuando una familia tenía una fecha que **no servía de corte** —caía donde ya iba el reparto o fuera del trabajo— y detrás venía otra que sí, la segunda se quedaba **sin sitio**. Ahora las dos se reparten el hueco hasta la fecha buena. Se dieron cuenta los tests, no la vista |
| 2026‑08‑05 | DD‑119 | **Un trabajo enlazado se dibuja como un tren de tarjetas de verdad, una por familia**, en vez de una barra única con los trozos pintados encima. Los trozos eran decoración —no recogían el ratón— y por eso el trabajo entero salía del color de su cabecera y pulsarlo abría siempre la misma toma de notas. Ahora **cada familia tiene su color de estado, su consejo emergente y su planificación**. Van **pegadas, sin hueco** —un hueco mentiría sobre las fechas— y solo se redondean las esquinas **de fuera**, para que se lean como un tren y no como pastillas sueltas. **Cogiendo cualquiera se mueve el trabajo entero**, que es lo que pidió el laboratorio. La lista de tarjetas **se recalcula, nunca se sustituye**: rehacerla a mitad de un arrastre destruiría el elemento que tiene cogido el ratón. La fila pasa a rotularse con el **nombre del grupo**, porque ya no es una toma de notas sino todas |
| 2026‑08‑04 | DD‑118 | **La barra de un trabajo enlazado se parte en un trozo por familia**, con una línea de puntos entre ellos y el código de cada una dentro. Se **encadenan**: cada familia empieza donde acabó la anterior, aunque tenga otra fecha de inicio escrita — lo que interesa ver es la secuencia. El orden lo da el **código**, que es estable, y no las fechas. Las que no traen fecha propia **se reparten a partes iguales** lo que quede hasta la siguiente que sí la tenga, para que ninguna desaparezca; si no la trae ninguna, trozos iguales. **La barra pasa a abarcar de la primera familia a la última** —antes solo las fechas de la cabecera—, y por eso **arrastrarla mueve a todas** y estirar un borde solo toca la del extremo (`RepartoDelArrastre`). Las divisiones **no se arrastran todavía**: eso va aparte |
| 2026‑08‑04 | DD‑117 | **El alta se parte en «Información obligatoria» y «Información opcional»**, y la segunda —que es la planificación entera: técnico 2, fechas, estado, importe, recepción de muestras y grupo— **va plegada por defecto**. Desplegada de serie convertiría en formulario lo que existe para hacerse en cuatro segundos (DD‑83, DD‑85); plegada, quien quiera planificar en el momento la abre y quien no, no la ve. **La carpeta empieza vacía** —para que se vea que hay que elegirla—, aunque el examinador siga abriéndose en la carpeta de proyectos. **No se ofrece archivar**: nace archivado lo que nadie va a mirar. Al crear se escribe el `.lmnlab` y, si se puso algo, su planificación en un segundo paso — que es lo único que la escribe (DD‑53) |
| 2026‑08‑04 | DD‑116 | **El tablero y el calendario encabezan cada servicio con las once primeras del código de la toma de notas** —`TECNO260201`: servicio y número de familia, **sin la edición del documento**—. El de servicio a secas no valía, porque las cuatro familias de un trabajo se llamaban igual y no se distinguían; el código completo tampoco, porque el `-00` se corrige por una errata del técnico y no dice nada de qué hay que ensayar. Sale de `ResumenDeProyecto.Rotulo`, en un solo sitio, para que las dos vistas y el diálogo de planificación no puedan llamarlo de tres formas distintas |
| 2026‑08‑04 | DD‑115 | **No se guarda un `.lmnlab` sin código de la toma de notas y sin técnico 1.** Sin ellos el fichero **no se puede ni nombrar ni atribuir**: el código es lo que le da nombre y lo que lo distingue de las otras familias del trabajo, y el técnico 1 es de quién es. Son los **mismos dos que ya exigía el alta rápida**, a propósito: lo que nace por un camino y lo que nace por el otro tiene que ser igual de identificable. **Exige mucho menos que empezar a ensayar** —sin clase, sin Ta y sin acreditación se sigue guardando—, porque un servicio a medias es el estado normal durante semanas. Al intentarlo, el aviso dice qué falta y **la vista salta a la cabecera**, donde los dos están en rojo |
| 2026‑08‑04 | DD‑114 | **`Ver \| Gestión de proyectos` pasa a ser un submenú** con las cuatro vistas. Se conserva arriba **«Ir a gestión»**, que respeta la vista en la que se estaba —volver desde el menú a media planificación no debe devolverte al tablero—, y debajo van las cuatro por su nombre para quien ya sabe adónde va. Cada una **se marca cuando es la vista activa**: al abrir el menú, la primera pregunta es dónde estoy |
| 2026‑08‑04 | DD‑113 | **La exportación declara con qué se hizo**: cada norma con su designación completa **al lado de su versión de plantilla** —iban por separado, y con dos normas no se sabía cuál de las dos versiones era de cuál—, más la **versión del programa**. La de plantilla dice contra qué reglas se midió; la del programa, con qué software se produjo el documento. Las dos son el rastro que pide la ISO 17025 sobre validación de software. **Quién exportó no se apunta**: lo revisa y firma el director técnico, así que quién pulsó el botón no aporta nada |
| 2026‑08‑04 | DD‑112 | **La exportación HTML lleva una tabla de muestras**, una fila por muestra con su identificador, clase, grado IP y grado IK. Antes iban todas juntas en una línea de la ficha —«IP2X, IPX0»—, que engaña dos veces: mezcla las dos cifras de un mismo grado y junta los de muestras distintas. Un servicio puede traer una IP65 y otra IP20, y quien firma necesita saber cuál es cuál. La ficha gana además **«Laboratorios externos»**, que se dice siempre —con «—» si no hubo ninguno—, porque un hueco en blanco no distingue «no se subcontrató» de «nadie lo rellenó» |
| 2026‑08‑04 | DD‑111 | **La acreditación es obligatoria y admite varias**: «Sin acreditar», ENAC, ENEC y CB. **«Sin acreditar» es excluyente** —marcarla borra las demás y marcar cualquier otra la quita—, porque si no se podría guardar un servicio declarado a la vez como acreditado y sin acreditar. Quién excluye a quién lo declara la plantilla (`opcionExcluyente`), no el código. **Sale en la exportación HTML**: ese documento no es un certificado sino la toma de notas puesta en limpio para que el director técnico la verifique antes de firmarla, así que tiene que ver contra qué acreditación se ensayó |
| 2026‑08‑04 | DD‑110 | **Los laboratorios de fuera van en la toma de notas**, en «Otros colaboradores»: tantas filas como haga falta, cada una con el laboratorio y **el ensayo y el motivo juntos**, porque «fotobiología — no tenemos cámara» explica una subcontratación y «fotobiología» a secas no. Texto libre y no una lista cerrada: los laboratorios cambian, y mantener un catálogo para poder escribir «IMQ Italia» sería una puerta de más. Es **opcional**, y las filas en blanco no llegan al fichero. Va por toma de notas y no por apartado; el día que el informe tenga que declarar subcontratación habrá que bajarlo al apartado |
| 2026‑08‑04 | DD‑109 | **Cuarta vista: BBDD**, el listado de todas las tomas de notas con buscador. **Solo lee**: no es una base de datos aparte sino una lente sobre los mismos `.lmnlab` que ya escanea el tablero — un fichero índice sería una segunda verdad que se desincroniza, y eso se descartó dos veces (DD‑27, DD‑89). **Ignora el filtro compartido y enseña todo**, terminados y archivados incluidos: arranca en «En desarrollo» y lo que se busca aquí casi siempre está terminado, así que con él nacería escondiendo justo lo que se viene a buscar. Filtros propios: texto —que busca **en todas las columnas**, porque quien recuerda un proyecto no sabe por cuál lo recuerda—, IP, IK y acreditación |
| 2026‑08‑04 | DD‑108 | **La barra de gestión se queda en una fila**: «Nueva toma de notas…» pasa a un **«+»** con su color de siempre, y los tres filtros se meten en un botón **«Filtros»** que abre un diálogo. Con esto la barra cabe entera a 620 px sin envolver. **El botón lleva la cuenta de los filtros que están apartando trabajo** —«Filtros (2)»— y se pinta de verde: escondidos y mudos, quien no encontrase su servicio en el tablero pensaría que se ha perdido, cuando lo que hay es un técnico elegido la semana pasada. «En desarrollo» no cuenta como filtro activo, porque es lo puesto al abrir y el aviso quedaría encendido siempre |
| 2026‑08‑04 | DD‑107 | **Se salta de .NET 8 a .NET 10**, antes de repartir el programa. **El soporte de .NET 8 termina el 10 de noviembre de 2026**, y estandarizar seis ordenadores en agosto sobre una versión a la que le quedan tres meses de parches obligaría a repetir la ronda enseguida. .NET 10 es LTS y llega a 2028. El salto costó **cinco líneas** —el `TargetFramework` de cada proyecto— porque el programa **no tiene ni una dependencia externa**: solo los tests traen paquetes. Verificado con los 368 tests de entonces y con la aplicación abierta: portada, calendario y toma de notas iguales. De paso se decide **publicar dependiente del framework** e instalar el *.NET Desktop Runtime* en cada equipo: es gratis (MIT), sin licencias por puesto, y deja las actualizaciones en unos MB en vez de 130 |
| 2026‑08‑04 | DD‑106 | **La ventana mínima baja de 740 a 620**, para que quepa en media pantalla y se pueda trabajar con el programa al lado de otra cosa. Se gana estrechando las tarjetas de norma (134→118) y los rellenos de la portada, pero **lo que de verdad lo permitía era arreglar dos recortes silenciosos**: la barra de gestión era un `StackPanel` horizontal —que no envuelve nunca, así que perdía «Actualizar» y los tres filtros—, y el título de la toma de notas se metía por encima del contador. La barra es ahora un `WrapPanel` y el título se recorta con puntos suspensivos. El índice de secciones deja de llevar 360 fijos y se lleva un 30 % del ancho, con tope en 360: **por encima de 1200 px de ventana el comportamiento es idéntico al de antes** |
| 2026‑08‑04 | DD‑105 | **La pestaña dice el código de la toma de notas y el título dice la norma con su año.** La lengüeta pasa de `TECNO2602 \| Luminarias` a `TECNO260201-00` —con las cuatro familias de un trabajo abiertas, las cuatro ponían lo mismo y no se distinguían—, y el título pasa de `Toma de notas \| TECNO2602 \| fichero.lmnlab` a `EN IEC 60598-1:2024 + A11:2024 \| TECNO260201-00`. **En el título la norma va entera y no como «Luminarias»**: hay dos años de la 60598 instalados a la vez y anotar contra el que no era no se vería hasta emitir. Sin código, «Sin código» en la pestaña y «sin código» en el título |
| 2026‑08‑04 | DD‑104 | **La cabecera pide un «Código de la toma de notas», obligatorio y el primero de todos**, del estilo `TECNO260201-00`. Es **lo que identifica esta toma de notas y no el servicio**: un trabajo con cuatro familias de luminarias tiene cuatro, y las cuatro comparten el de servicio. De él salen dos cosas: el **código de servicio**, que son sus **nueve primeras** y se rellena solo aunque se puede corregir a mano, y el **nombre del fichero**, `TdN_60598_TECNO260201-00.lmnlab`. Con esto **desaparece el `xx-00`** que el programa pegaba y el técnico tenía que sustituir renombrando: el número de familia y la edición se teclean una vez, en la cabecera |
| 2026‑08‑04 | DD‑103 | **La extensión pasa de `.lumproj` a `.lmnlab`**, para que el fichero lleve el nombre del programa —LumenLab— y no el del objeto que había dentro cuando se eligió. **Los `.lumproj` se siguen abriendo y escaneando**, y al guardarlos **se quedan donde están y como están**: renombrarlos solos movería el registro de un ensayo sin que nadie lo pida. Lo nuevo nace ya con `.lmnlab`. Es transitorio; cuando no quede ninguno se borran `ExtensionAnterior` y `Patrones` |
| 2026‑08‑03 | DD‑102 | **Un solo estilo de botón para toda la aplicación**, con esquinas redondeadas, declarado en `App.xaml` — no en la ventana, porque los diálogos son ventanas aparte y no lo verían. Rehacer la plantilla obliga a dibujar a mano los cuatro estados; encima y pulsado se resuelven **con opacidad y no con un color fijo**, para que funcionen igual sobre el gris de por defecto y sobre los botones que traen el suyo. Los de tamaño fijo que solo llevan un símbolo van con el estilo **`BotonIcono`**, que quita el relleno: con los 14 px a cada lado del estilo general, a un botón de 30‑34 px le quedan cuatro para el símbolo, y **WPF no avisa — lo encoge y lo recorta**. Picó dos veces (el `+` de pestañas y el de plegar el índice) antes de que la excepción tuviera nombre en vez de repetirse a mano |
| 2026‑08‑03 | DD‑101 | **Lo que distingue una norma de otra es el AÑO DE PUBLICACIÓN, no la «edición».** Son dos cosas distintas —la 60598‑1 va por su novena edición y se publicó en 2024— y durante un tiempo este documento y el código llamaron «edición» al año. Corregido en las cinco plantillas (`anioDePublicacion`, **el año y solo el año**), en el código, en los tests y aquí. La designación completa con sus enmiendas vive en `titulo`, que es lo que se lee y lo que sale en el informe |
| 2026‑08‑03 | DD‑100 | **La portada tiene un recuadro de avisos, y no existe si no hay nada que hacer.** Ocho condiciones sin solaparse: carpeta de proyectos sin elegir o inalcanzable; carpeta compartida sin elegir, inalcanzable o sin normas publicadas; normas locales sin publicar o más nuevas que las publicadas; y ficheros ilegibles en el último escaneo. **Casi todas fallaban en silencio** — sin carpeta de proyectos, las tres vistas de gestión salen vacías y eso es indistinguible de no tener trabajo. Rojo si algo no funciona, ámbar si solo está descuadrado, y cada línea trae el botón que lo arregla. **Nada en verde**: un recuadro que casi siempre está deja de leerse |
| 2026‑08‑03 | DD‑99 | **`Configuración | Normas instaladas…` avisa de lo que este equipo tiene y el laboratorio no.** Desde que se publica la primera tanda, el programa lee de la carpeta compartida y **deja de mirar la local**: dejar caer una norma nueva no producía ninguna señal — el fichero estaba, no aparecía y nada lo explicaba. Ahora se comparan las dos carpetas y se dice qué falta por publicar, en ámbar y junto al botón que lo resuelve |
| 2026‑08‑03 | DD‑98 | **Existe la plantilla de luminarias de 2021**, pedida por retrocompatibilidad. **Entre los dos años cambia la numeración de las secciones, no lo que se anota** — confirmado por el laboratorio. Es la de 2024 con la numeración que le toca, sacada de la tabla de equivalencias del propio libro. Las secciones 16 y 17 se quedan con su número de 2024 por no tener equivalente antiguo |
| 2026‑08‑03 | DD‑97 | **El fichero de plantilla se llama `plantilla-<id>_<version>.json`** —plantilla-60598-1_2024_1.0.0.json—, y su catálogo de equipos igual. El **año** en el nombre es lo que permite que dos convivan en la misma carpeta; la **versión**, saber qué hay instalado sin abrir nada. **De un mismo id solo cuenta la más alta**: si no, publicar una corrección enseñaría dos tarjetas de la misma norma. La tarjeta enseña el número de norma y el año, no el id |
| 2026‑08‑03 | DD‑96 | **El trabajo se mide en horas, no en días: importe ÷ 105 × 1,3**, a 8 h por jornada. La regla anterior —÷ 80 € = un día— la dio el laboratorio mal y la corrigió: son **80 €/hora**. La cuenta sale a 80,77 €/h, que cuadra con la tarifa. **No es un matiz**: un servicio de 2 000 € pasa de 25 días a poco más de 3 y toda la tabla de carga baja unas ocho veces. Los tres números se editan por separado, con la equivalencia en euros por hora a la vista |
| 2026‑08‑02 | DD‑95 | **El `id` de una plantilla lleva norma, parte y año de publicación** —`60598-1_2024`— y no solo el número. Hoy es `60598` y el año no está en la identidad, así que publicar la norma nueva **remide en silencio los ensayos anteriores**. La `version` se queda **fuera del id**: se sube por corregir una errata, y meterla dentro dejaría huérfano cada proyecto en cada corrección. Cada plantilla declara `idsAnteriores`, de modo que los proyectos ya guardados siguen encontrando la suya: **la migración vive en el JSON, no en el código**. El **nombre del fichero no cambia** de momento: sigue siendo `TdN_60598_…` |
| 2026‑08‑02 | DD‑94 | **La versión de plantilla con la que se registró un ensayo se lee y se enseña.** Se guardaba desde el principio y no se leía nunca. El **informe declara la del registro**, no la instalada al imprimir —decir la de hoy es atribuirle al ensayo una plantilla que no se usó—, y al abrir un proyecto grabado con otra versión se avisa, sin bloquear |
| 2026‑08‑02 | DD‑93 | **La norma pasa a ser obligatoria en el alta**, y el alta sale de la portada. Sin norma no hay apartados que rellenar y el nombre del fichero la lleva dentro, así que dejarla para después obligaba a renombrarlo. En la portada, elegir norma y dar de alta eran dos caminos para lo mismo: el alta se queda en `Archivo` y en la barra del tablero. **Lo que falta se ve en rojo** en el rótulo del campo, y vuelve al gris al rellenarse — como las casillas obligatorias de la toma de notas |
| 2026‑08‑02 | DD‑92 | **«(sin técnico)» es una opción del filtro de técnico**, no un técnico del catálogo. Aparece sola cuando hay servicios sin responsable y enseña justo esos, que es lo que hay que repartir. Mismo rótulo que usan el calendario y la carga, escrito en un solo sitio. **Crear un técnico llamado «Sin técnico» sería peor**: saldría como persona en todas las tomas de notas y la carga le sumaría días como si tuviera capacidad |
| 2026‑08‑02 | DD‑91 | **El fichero se llama `TdN_<norma>_<código>xx-00.lmnlab`**, p. ej. `TdN_60598_LEDC42502xx-00.lmnlab`. El **`xx` es un hueco** para el número de toma de notas —un servicio puede llevar varias familias— y el **`00` es la revisión del documento**, que sube al corregir algo ya emitido. **Los pone el técnico renombrando**, no el programa: numerar y reeditar son decisiones del laboratorio, y un programa que las tomara solo acabaría renumerando un registro ya firmado |
| 2026‑08‑02 | DD‑90 | **Las tomas de notas de un mismo trabajo se enlazan con un campo «Grupo»**, y el calendario las enseña en una sola barra. Resuelve DD‑88: el jefe planifica un trabajo y el técnico sigue viendo las cuatro familias en el tablero. **Manda la cabecera** —la que lleva las fechas—, y el importe va solo en ella; la barra enseña la suma, así que repetirlo deja de ser invisible. El enlace vive dentro de cada fichero, no en un índice aparte |
| 2026‑08‑02 | DD‑89 | **No hay objeto «proyecto», y no lo habrá.** El registro del ensayo es el `.lmnlab` y tiene que estar en la carpeta de tomas de notas de su servicio; un fichero de proyecto aparte sería un documento sin ensayo detrás y **rompería la trazabilidad**. Revierte DD‑83: la portada vuelve a dos zonas y el alta rápida crea una toma de notas, no un proyecto. Lo que agrupa varias tomas de notas de un mismo trabajo se resolverá **enlazándolas** desde el calendario, no creando un fichero por encima |
| 2026‑08‑02 | DD‑88 | **Un proyecto puede llevar varias familias de luminarias, cada una con su toma de notas.** Es el motivo por el que en su día se decidió no tener objeto «proyecto». Se planifica **el trabajo entero**, y se resuelve enlazando las tomas de notas (DD‑90), no agrupándolas en un fichero |
| 2026‑08‑02 | DD‑87 | ↩️ **Deshecha el mismo día por DD‑89.** **La portada tiene tres zonas: Proyectos, Gestión, y la toma de notas suelta en gris y abajo.** Elegir norma era la puerta de entrada del programa; desde que los proyectos se dan de alta, empezar por ahí es casi siempre crear un servicio que ya existía. No se quita —hay ensayos que no llegan a ser proyecto— pero deja de competir con «Proyectos» |
| 2026‑08‑02 | DD‑86 | **Al guardar un proyecto nuevo se avisa si ese servicio ya existe** en la carpeta, con la opción de abrir el que hay. Nace de separar el alta de la toma de notas: un técnico puede empezar sin saber que su proyecto ya estaba creado. Los códigos se comparan sin mayúsculas, espacios ni guiones. **Se puede crear otro a sabiendas** —un reensayo repite código— porque quien decide es la persona. Es una red, no la solución: lo que evita el error es la pantalla de inicio |
| 2026‑08‑02 | DD‑85 | **Para dar de alta un proyecto solo son obligatorios el nombre y el técnico 1.** Precisa DD‑83: no es que el formulario deba ser corto, es que **nada más puede bloquear** — norma y técnico 2 se pueden dejar en blanco. Lo que la norma exige para ensayar (`RequisitosDelProyecto`) no tiene voz en el alta. `Archivo \| Nuevo proyecto…`, también en la portada y en la barra del tablero; crea el `.lmnlab` y va al calendario **sin abrir la toma de notas** |
| 2026‑08‑02 | DD‑84 | **Qué norma es la principal la apunta el proyecto**, y se guarda en el `.lmnlab`. Antes se deducía del patrón de muestras —que es la consecuencia de haberla elegido, no la elección—, y dos normas del mismo patrón lo dejaban en manos del orden alfabético. De paso se cierra un fallo latente: abrir un servicio de dos normas podía cargarlo por la añadida y **guardar renombraba todas las muestras** |
| 2026‑08‑02 | DD‑83 | **El proyecto pasa a ser el centro y la toma de notas una parte suya.** Un proyecto tiene fechas, muestras, importe y certificaciones que no son de ninguna norma. **Con una restricción por encima de todo: crear un proyecto son cuatro cosas y Aceptar**, y nada de su cabecera puede ser obligatorio — el PM planifica antes de que exista un solo dato de ensayo. Se hace en tres pasos; el 1 (la identidad se lee de un solo sitio, sin tocar el formato) ya está |
| 2026‑08‑02 | DD‑82 | **`Ayuda \| Reportar un problema…`**: el correo de quien mantiene el programa, con los datos que hacen falta para reproducir un fallo y acceso al registro de errores. **No envía nada por su cuenta** —eso obligaría a llevar una contraseña de servidor en el ejecutable, lo mismo que se descartó en DD‑67—; abre el programa de correo del equipo, que es quien tiene las credenciales |
| 2026‑08‑02 | DD‑81 | **El programa se llama «LumenLab»**, escrito en `<Product>` del `.csproj` y en ningún otro sitio: portada, barra de título y «Acerca de» lo leen del ejecutable. Se titulaba por lo que hacía al principio, y ya hace dos cosas. La **versión** se ve en la portada, que es lo primero que se pregunta ante un fallo. **En la portada no hay ajustes**: las carpetas se cambian en *Configuración* |
| 2026‑08‑02 | DD‑80 | **La portada se parte por la mitad**: tomar notas a la izquierda, gestión a la derecha, dos columnas iguales. La gestión ya no es un añadido sino la mitad de la aplicación. Cada una de sus tres vistas tiene enlace directo; el menú, en cambio, respeta la vista en la que se estaba |
| 2026‑08‑02 | DD‑79 | **Ninguna opción general del filtro trae lo terminado ni lo archivado**, en las tres vistas. Se quitó «Todos»: en la carga, un servicio cerrado seguía sumando sus días e inflaba el porcentaje del técnico con trabajo que ya no existe. Lo cerrado se pide por su nombre, con «Terminado» o «Archivados» |
| 2026‑08‑02 | DD‑78 | **En el tablero, cada norma añadida ocupa una sola línea**, con la cuenta de toda su toma de notas: no se va hasta que está entera completa. La principal se sigue detallando sección a sección. Desplegar la 62031 dentro de un servicio de luminarias enterraba lo importante |
| 2026‑08‑02 | DD‑77 | **El tablero mide cada proyecto contra las normas que él lleva**, no contra la que esté abierta. Antes, un servicio de luminarias con 62031 no enseñaba los apartados de la 62031 y podía darse por terminado a medias; y uno de IP se medía con las reglas de luminarias. Con varias normas, cada sección lleva su número delante |
| 2026‑08‑02 | DD‑76 | **Los filtros de técnico y norma también son comunes a las tres vistas.** El laboratorio pidió el de técnico en el tablero; en vez de duplicarlo se subió a la barra común, donde ya estaba el de estado. En el calendario solo quedan sus ajustes propios |
| 2026‑08‑02 | DD‑75 | **Un solo filtro de estado para las tres vistas**, por defecto «En desarrollo». Antes el tablero tenía su casilla y el calendario su desplegable, y podían contradecirse. Cambiarlo no vuelve a escanear, y se dice siempre cuántos quedan fuera |
| 2026‑08‑02 | DD‑74 | **Para ocultar manda el estado que puso la persona, no el que deduce el programa.** Con el calculado, un servicio con todo relleno pero pendiente del cliente salía como terminado sin que nadie lo dijera. **Archivar oculta ahora en las tres vistas**, no solo en el calendario |
| 2026‑08‑02 | DD‑73 | **El escaneo va en segundo plano y no bloquea la ventana.** Con una carpeta de clientes grande, congelarse varios segundos parece que el programa se ha colgado; mientras dura se sigue viendo lo anterior y se cuenta por dónde va |
| 2026‑08‑02 | DD‑72 | **Lo ya analizado se guarda entre sesiones**, validado por fecha, tamaño y versión de la plantilla. Sin esto, cada arranque releía y analizaba todos los proyectos aunque no hubiera cambiado ninguno. La caché va en el perfil del usuario, no en la carpeta compartida: escribirla varios a la vez sobre OneDrive solo daría conflictos |
| 2026‑08‑02 | DD‑71 | **Los proyectos y la configuración compartida están en carpetas distintas.** Lo corrigió el laboratorio: los proyectos cuelgan de la carpeta de clientes, cada uno en su rama, y la configuración vive aparte. Son dos ajustes; si el segundo se deja en blanco se usa el primero |
| 2026‑08‑02 | DD‑70 | **La carpeta del laboratorio se elige en «Configuración» y se pregunta sola la primera vez.** Estaba solo en el tablero, de cuando esa carpeta era únicamente dónde estaban los proyectos; al gobernar también normas, técnicos, tarifa y versión, un técnico que no abriera el tablero no la elegía nunca y trabajaba aislado sin saberlo |
| 2026‑08‑02 | DD‑69 | **El aviso de versión nueva avisa, no bloquea.** Con la aplicación en varios equipos, lo grave es trabajar meses con una versión vieja sin saberlo; impedir trabajar porque un fichero de OneDrive dice otra cosa sería peor que el problema |
| 2026‑08‑02 | DD‑68 | **Las normas se leen de la carpeta compartida, no de junto al ejecutable.** Con una copia por equipo, dos técnicos podían rellenar versiones distintas de la misma norma sin enterarse. La copia local queda de respaldo y, cuando se usa, **se avisa** |
| 2026‑08‑06 | DD‑67 | **Sin contraseña en «Configuración».** Se planteó al aparecer los importes y se descartó: protegía la lista de técnicos y la tarifa mientras los importes seguían visibles por otros tres caminos. Lo que corresponde, si algún día hace falta, son permisos de carpeta. Ver «Descartado, y por qué» |
| 2026‑08‑06 | DD‑66 | **El importe de la oferta vive en la planificación, no en la toma de notas.** Es dato comercial: no se anota como un ensayo ni sale en el informe que se firma |
| 2026‑08‑06 | DD‑65 | **El trabajo de un servicio se reparte entre meses por días entre semana**, suponiendo esfuerzo uniforme. Es impreciso en un servicio suelto y suficiente sobre el conjunto de un técnico; la alternativa exacta —teclear días por mes— no la rellenaría nadie |
| 2026‑08‑06 | DD‑64 | **La ocupación de un técnico se mide en días ocupados, no sumando duraciones.** Dos servicios a la vez no ocupan el doble, y sumarlos exageraría la carga justo de quien lleva varios en paralelo |
| 2026‑08‑06 | DD‑63 | **El calendario se agrupa por técnico responsable.** La columna izquierda dejó de listar servicios —el código ya va en la barra— para responder a lo que el responsable necesita: cuántos lleva cada uno y cuánto le ocupan |
| 2026‑08‑06 | DD‑62 | **El menú se llama «Configuración», no «Admin».** No hay roles ni permisos que administrar; cualquiera que abra el programa puede editar las listas. Ahí caben luego equipos y perfil de usuario |
| 2026‑08‑06 | DD‑61 | **Los técnicos se eligen de una lista compartida** (`tecnicos.json` en la carpeta de proyectos). Escribirlos a mano daba la misma persona con tres grafías y rompía el filtro por técnico. Un nombre guardado que no esté en la lista se sigue ofreciendo, para no dejar sin técnico a los proyectos antiguos |
| 2026‑08‑06 | DD‑60 | **La cabecera del calendario se sincroniza con un `ScrollViewer`, no con una transformación.** Con la ventana pequeña, el recorte de maquetación de WPF dejaba sin dibujar las semanas que no cabían |
| 2026‑08‑06 | DD‑59 | **Arrastrar no saca la barra del calendario dibujado.** Se salía y quedaba flotando en blanco, sin semanas debajo: no se veía en qué fecha se estaba soltando. Para ir más lejos se pide sitio con «▶» |
| 2026‑08‑06 | DD‑58 | **El calendario no guarda años: los calcula.** Encuadra lo que hay, se camina con ◀ ▶ hasta donde haga falta y se acota a ±5 años alrededor de hoy —ventana que se mueve sola, así que no caduca—. Una fecha fuera de ese horizonte no encuadra nada y baja a la banda de abajo: es una errata, no una planificación |
| 2026‑08‑06 | DD‑57 | **El gesto de arrastre vive en el núcleo** (`BarraDePlanificacion`), no en el modelo de vista. Los eventos de ratón no se pueden automatizar en este equipo, así que la única forma de comprobar el gesto es que su lógica esté fuera de la interfaz |
| 2026‑08‑06 | DD‑56 | **El arrastre se ajusta a días, no a semanas.** Se planifica por semanas, pero un servicio empieza el día que empieza; el número de semana se enseña como ayuda, no como rejilla obligatoria |
| 2026‑08‑06 | DD‑55 | **El calendario mide en semanas ISO**, no en días ni en meses: es la unidad con la que planifica el laboratorio («entra en la S32»). La aritmética vive en `EjeDeSemanas`, dentro del núcleo, para poder probarla |
| 2026‑08‑06 | DD‑54 | **Una tarjeta por toma de notas.** Un servicio con 60598‑1 + ‑2‑3 + IK + 62031 es **una sola tarjeta**: todo cuelga de la toma de notas principal |
| 2026‑08‑06 | DD‑53 | **La planificación vive en el `.lmnlab` pero no la gestiona la toma de notas.** Solo la escribe el calendario; al guardar desde una pestaña se conserva releyéndola del disco. Sin esto, el técnico que tuviera el proyecto abierto pisaría al guardar las fechas que otro acababa de mover |
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

> **Numeración canónica: la de 2024** (decisión de 2026‑07‑29), la misma que usa la hoja de toma de notas. La numeración antigua que aparece en `Índice`, en el panel de pesos y en parte de las hojas de equipos se conserva **solo como alias de visualización** y se traduce con la tabla siguiente.

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
- Designaciones: IEC 62031:2018 / EN IEC 62031:2020 + A11:2021.
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

## 7. Designaciones y años de las normas

| ID | Origen | Regla |
|---|---|---|
| `R-NOR-01` | `Datos!BI4:BI14`, `BJ4:BJ14` | Para cada norma, si su factor es verdadero se emite su texto IEC y su texto EN; si no, un espacio |
| `R-NOR-02` | `Datos!BH5:BH14` | El factor de cada parte ‑2 es su casilla de proyecto (`D6`…`N6`) |
| `R-NOR-03` | `Datos!BN10` / `BN12` | Texto final = concatenación de los 11 fragmentos IEC / EN |
| ~~`R-NOR-04`~~ | `Datos!BQ4` | 🚫 62031 — fuera de alcance |
| ~~`R-NOR-05`~~ | `Datos!BQ5` | 🚫 IK — fuera de alcance |
| `R-NOR-06` | `Datos!BF14` | La norma «Otro» toma su texto de `RESUMEN!D27` |

En el borrador, el texto de normas aplicadas se compone únicamente de `R-NOR-01` a `R-NOR-03`, que no dependen de ninguna celda rota.

Catálogo de designaciones vigentes en la plantilla, con su año (dato maestro que debe ser editable sin tocar código):

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
62031       IEC 62031:2018                            EN IEC 62031:2020 + A11:2021
IK          IEC 62262:2002+AMD1:2021 + IEC TR 62696:2011   EN 62262:2002 + A1:2021 + IEC TR 62696:2011
```

### Qué designación enseña el programa

Cada plantilla declara la suya en `meta.designacion`, y es **lo que encabeza la toma de notas abierta** (DD‑105). Las cinco instaladas, **confirmadas por el laboratorio el 2026‑08‑04**:

| Plantilla | `meta.designacion` | `meta.edicion` |
|---|---|---|
| `60598-1_2024` | `EN IEC 60598-1:2024 + A11:2024` | 10 |
| `60598-1_2021` | `EN IEC 60598-1:2021 + A11:2022` | 9 |
| `62031_2020_A11` | `EN IEC 62031:2020 + A11:2021` | 2 |
| `60529_1991` | `EN 60529:1991 + A1:2000 + A2:2013 + AC:2019-02 + AC:2016-12 + corrigendum May 1993` | 2.2 |
| `62262_2002_A1` | `EN 62262:2002 + A1:2021` | 1.1 |

Es la designación a secas: **sin el nombre comercial** («Luminarias — ») y **sin la coletilla** («y partes ‑2»), que sí van en `titulo` porque ahí describen, mientras que aquí lo que hace falta es identificar contra qué se ensaya. `RotulosTests` comprueba que **todas llevan su año dentro**: es lo único que separa dos plantillas de la misma norma, y es el error que este rótulo existe para evitar.

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
| ~~**D‑02**~~ | ✅ **Resuelto** | **Dos numeraciones de norma conviven**: `Índice`, el panel de pesos y parte de las hojas de equipos usan la antigua; la toma de notas y el resto del motor, la de 2024 | **Canónica: la de 2024.** La antigua queda como alias de visualización. Tabla de equivalencia en la [sección 5](#equivalencia-de-numeraciones). Queda un punto menor por confirmar: el rótulo 5.2 «cableado interno» ↔ 8.2 «cableado externo» |
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
| **D‑14** | 🟡 Baja | La designación EN de la parte ‑2‑5 figura como `EN 60598-2-15:2015` (posible errata por `2-5`) | Verificar contra el catálogo de normas |
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

| **DD‑01** | **.NET 10 + WPF.** C# es el lenguaje que el desarrollador ya domina (vía Unity); XAML con *data binding* encaja con un formulario de cientos de campos; distribución como ejecutable único en Windows |
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
| Plataforma | .NET 10, Windows |
| Interfaz | WPF + MVVM **a mano**, sin librería externa: `ObservableObject`, `Comando` y `ComandoCon<T>` en `Base.cs` son cien líneas y evitan una dependencia más |
| Fichero de proyecto | JSON (`System.Text.Json`), escritura atómica, en OneDrive. Extensión `.lmnlab` |
| Definición de plantilla | JSON versionado, leído de la carpeta compartida del laboratorio |
| Informe | HTML generado a mano, **sin dependencias**. Se descartó MigraDoc/PDFsharp: añadía una librería externa y daba problemas de resolución de fuentes (DD‑20) |
| Contraseñas | **No hay** (DD‑67). Se estudiaron y se descartaron; si algún día hace falta control de acceso, son permisos de carpeta |

> **Lo que este apartado planeaba y no sobrevivió.** Se escribió antes de programar, y conviene leerlo sabiendo qué cambió: se preveía `CommunityToolkit.Mvvm` y se acabó escribiendo el MVVM a mano; se preveía contraseña con hash Argon2 y se descartó entera; se preveía **edición por turnos** y acabó siendo **el último en guardar manda**, con la planificación escrita por un solo camino para que no se pisen (DD‑50). El resto se cumplió.

### Observaciones técnicas sobre las decisiones tomadas

**OneDrive condiciona el formato de almacenamiento.** No se puede usar SQLite sobre una carpeta sincronizada: el bloqueo de fichero y la sincronización parcial provocan corrupción y copias en conflicto. Formato adoptado en su lugar:

- **Un fichero JSON por toma de notas**, con extensión `.lmnlab` y el nombre que fija el laboratorio (DD‑91), escrito de forma atómica en cada guardado (escritura a temporal + reemplazo). La extensión es propia para que el fichero se vea como un documento del laboratorio y no como algo técnico; por dentro es JSON, igual que un `.xlsx` es un zip.

> **Se llamó `.lumproj` hasta el 2026‑08‑04** (DD‑103). Los que existan **se siguen abriendo, escaneando y contando**, y guardarlos no los renombra: mover el registro de un ensayo sin que nadie lo pida es peor que convivir con dos extensiones una temporada. Todo pasa por `RepositorioDeProyectos`: `Extension` es la de ahora, `ExtensionAnterior` la vieja y **`Patrones` es lo que hay que usar al recorrer una carpeta** — hay cuatro sitios que escanean, y dejarse uno significa que unos proyectos salen en el tablero y otros no, sin que nada lo explique. El día que no quede ninguno, se borran las dos últimas.
- Ventajas sobre OneDrive: reemplazo de fichero completo —que OneDrive sincroniza sin problema—, historial de versiones gratuito, recuperación manual si algo falla, y contenido legible sin la aplicación.
- **Manda el último en guardar**, y la planificación la escribe un solo camino releyendo el fichero (DD‑50), de modo que mover una fecha en el calendario no pisa lo que esté anotando el técnico ni al revés. Sigue conviniendo un aviso al abrir si OneDrive marca el fichero como no sincronizado.

**Sobre DD‑08.** *(Lo que sigue es del borrador. La contraseña se descartó entera en DD‑67 por el motivo que este mismo párrafo anticipaba; lo que queda vivo del perfil es saber **quién** usa el programa, sin contraseña.)* El perfil con contraseña tiene un alcance limitado que conviene tener presente: los ficheros de proyecto viven en OneDrive en claro, así que la contraseña de la aplicación no protege los datos —cualquiera con acceso a la carpeta los lee igual—. Su valor real es **atribuir autoría** en el PDF. Además, la recuperación de contraseña por correo exige un servidor de correo o credenciales SMTP, que hoy no existen. Propuesta para el borrador:

- Perfil local con nombre, DNI y correo (para la cabecera del PDF).
- Contraseña almacenada con hash (Argon2), sesión persistente mediante testigo local.
- **Recuperación por restablecimiento local** en lugar de por correo, hasta que haya infraestructura.

**Sobre el DNI.** Es dato personal; conviene confirmar que hace falta en el PDF y no basta con el nombre del técnico.

---

---

## 14. Artefactos generados

| Fichero | Contenido |
|---|---|
| `plantilla/equipos-60598-1_2024_1.0.0.json` | Catálogo de equipos completo: **43 grupos, 224 entradas, 89 códigos distintos**. Importación literal desde `BBDD Equipos 60598` (DD‑10), con las notas de uso del laboratorio y la trazabilidad de cada celda de origen |
| `plantilla/plantilla-60598-1_2024_1.0.0.json` | **La norma entera como datos**: 16 secciones y 45 apartados, con campos, checklists, subbloques, grupos repetibles, reglas P1‑P8 y los nueve cálculos. ~140 KB |
| `src/LumNotas.Core` | Motor de reglas: modelo de plantilla, catálogo de equipos, almacén de datos, evaluador de los tipos de regla, predicados y cálculos con nombre, requisitos del proyecto, indicador de avance, estado de apartado y resumen para el tablero. En `Gestion/` vive además todo lo que no es ensayo: eje de semanas ISO, gesto de arrastre, ocupación, lista de técnicos, capacidad mensual, carga por técnico, filtros, **alta de una toma de notas** (`AltaDeProyecto`), **nombre del fichero** (`NombreDeTomaDeNotas`), **aviso de servicios repetidos** (`ProyectosRepetidos`) y **enlace de las familias de un trabajo** (`EnlaceDeTomasDeNotas`) — todo **fuera de la interfaz para poder probarlo** |
| `src/LumNotas.Storage` | Un fichero `.lmnlab` por proyecto (JSON), con **escritura atómica** (temporal + reemplazo) para que OneDrive lo sincronice sin corromperlo. Más la lista de recientes, los ajustes y el explorador de carpetas |
| `src/LumNotas.Report` | Exportador del informe a HTML con estilos de impresión A4. **Sin dependencias externas** |
| `src/LumNotas.App` | Interfaz WPF. `VentanaPrincipalViewModel` es la ventana con su barra de pestañas; `DocumentoViewModel` es **un proyecto abierto** (árbol con semáforo y formulario generado desde la plantilla); `GestionViewModel` es la pestaña de gestión, con sus **cuatro** vistas (`CalendarioViewModel`, `CargaViewModel`, `BbddViewModel` y el tablero, que va en el propio `GestionViewModel`). Las plantillas grandes viven en `Window.Resources` y se eligen por tipo |
| Ficheros compartidos | Junto a los proyectos, en la carpeta de OneDrive: `tecnicos.json` (la lista del laboratorio) y `capacidad.json` (tarifa y días por mes). Se editan desde `Configuración` y valen para todos |
| `plantilla/plantilla-<id>_<version>.json` | Una por norma y año, p. ej. `plantilla-60598-1_2024_1.0.0.json`, con su catálogo `equipos-60598-1_2024_1.0.0.json` al lado |
| `tests/LumNotas.Core.Tests` | **562 tests, verificados en verde el 2026‑08‑06.** Cubren los ocho patrones, los nueve cálculos, los defectos corregidos, la integridad de la plantilla, el ciclo de guardado, varios proyectos simultáneos, el informe, el tablero y la planificación (semanas ISO, cambio de año, el gesto de arrastre completo, que años lejanos y erratas de año no rompan el eje, y que planificar y anotar no se pisen), y la lista de técnicos (que quitar no toque los proyectos y corregir sí), y la ocupación por técnico (que lo solapado no cuente dos veces), y la carga mensual (tarifa, capacidad, reparto entre meses y que no se pierda trabajo), y el escaneo de la matrioska de clientes (que encuentre lo hondo, que la caché caduque bien, que una rama rota no lo tumbe y que una norma añadida ocupe una sola línea con la cuenta de toda ella), y **qué proyecto se ve** (`FiltrosDeGestion`, que decide por las cuatro vistas a la vez), y **cuándo se ensayó** (que solo cuenten las fechas de verdad y que el periodo case por solapamiento), y **los carriles del calendario** (`CarrilesDelCalendario`: que lo que no se pisa quepa en una fila, que tocarse un día no baste para compartirla, que se suba al hueco libre de arriba y que no se gasten más filas de las que hacen falta), y **el código entero** (que las tres longitudes encajen, que los tres caminos exijan lo mismo, y que un código a medias no llegue al disco) |

### Glosario: cómo se llaman las cosas

Fijado con el laboratorio el 2026‑08‑07. **Vale para todo lo que se ve**; el código y los comentarios se quedaron como estaban a propósito (ver el final).

| Concepto | Palabra | Ejemplo |
|---|---|---|
| El documento de 14 caracteres. **Lo que se planifica**: fechas, importe, estado | **toma de notas** en frases · **TdN** en botones y columnas | `TECNO260201-00` |
| El encargo del cliente, 9 caracteres | **servicio**, y casi siempre como «código de servicio» | `TECNO2602` |
| Enlazar varias TdN a mano. Voluntario, vacío por defecto | **grupo** | «Grupo: torres 2026» |
| Unidad de trabajo | **día** | «≈ 25 h (3,2 días)» |
| Unidad de planificación | **semana** | «3W», «S32» |

**Retiradas:** *proyecto* → toma de notas · *jornada* → día · *familia* (como rótulo) → toma de notas · *servicio* en prosa → toma de notas.

**Las dos zonas de la portada se llaman «TOMA DE NOTAS» y «GESTIÓN DE SERVICIOS»**, y ninguna lleva renglón de apoyo debajo: los tenía —«Abre una toma de notas», «Mira cómo van todas las TdN»— y se quitaron el 2026‑08‑07 porque describían lo que el botón de debajo ya dice. El margen que gastaban se heredó al rótulo de la zona, para que las dos columnas sigan arrancando a la misma altura.

#### Por qué «servicio» casi no aparece

Porque el programa casi no lo usa. Se comprobó antes de decidirlo: **el código de servicio no agrupa nada**. Lo que junta varias TdN en una barra del calendario es el campo **«Grupo»**, texto libre que se teclea a mano y que **está vacío por defecto porque el laboratorio decide caso a caso** si quiere agrupar. Dos TdN con el mismo `TECNO2602` no se enlazan solas.

Así que el código de servicio es un dato **derivado** que se enseña en la cabecera y se imprime en el informe, y nada más. Sobrevive en «Código de servicio» y en el rótulo de la pestaña.

#### Los tres cortes del código

```
TECNO260201-00
└───────┘         9  → servicio
└─────────┘      11  → familia
└────────────┘   14  → toma de notas
            └┘       → edición del documento
```

**El corte de 11 se sigue usando para rotular** las tarjetas del tablero y las barras del calendario, y se queda así aunque el laboratorio avise de que los dos dígitos de familia no siempre casan con una familia real: da igual, porque **las muestras se renombran dentro de cada TdN**.

Dos ediciones de la misma familia —`-00` y `-01`— son **dos tomas de notas distintas y cuentan dos veces** en la carga. Es lo que el laboratorio quiere: si existe la `-01` es porque la `-00` se archivó, y eso lo decide quien trabaja, no el programa.

#### La regla de TdN

**TdN** donde el sitio manda: botones, cabeceras de columna, rótulos de campo, la línea de estado.
**Toma de notas** en cualquier frase que se lea seguida.

Nunca las dos en el mismo renglón.

#### Lo que no se tocó, y por qué

**Las claves de datos.** `"proyecto"` es el *ámbito* dentro de las cinco plantillas y dentro de **cada `.lmnlab` guardado** — 34 apariciones en código más las plantillas. Renombrarlo dejaría ilegible todo lo ensayado. Igual con `CarpetaDeProyectos`, que es la clave del `ajustes.json` de cada equipo: cambiarla borraría las carpetas configuradas de los seis ordenadores.

**Los nombres de código.** `ResumenDeProyecto`, `DialogoNuevoProyecto`, `ExploradorDeProyectos`… No los ve nadie y son cientos de renombrados con riesgo de romper lo que funciona. Se dejan.

> Ahí está además la mejor prueba de que «servicio» era ambiguo: el código tiene `ServicioDeTecnicos` y `ServicioDeCapacidad`, donde significa *componente de software*, junto a `ServicioPlanificado`, donde significa el encargo del cliente. La misma palabra para dos cosas dentro del mismo programa.

### Cómo se escriben los rótulos

**Sin puntos suspensivos.** Ni en botones, ni en menús, ni en textos de la interfaz: «Planificar», no «Planificar…»; «Elegir carpeta», no «Elegir carpeta…». Es una decisión del laboratorio (2026‑08‑05) y vale para lo que se añada a partir de ahora.

Se dejaron a propósito los de los **comentarios del código** donde significan «etcétera» o un rango —`M1…Mn`, `tornillos, uniones…`, `EBP_SAFE…`—: ahí no son un rótulo, son notación.

**Un texto que sale en dos sitios se escribe en uno** (DD‑143). Los dos diálogos que planifican compartían tres explicaciones y las llevaban copiadas en cuatro sitios; ahora viven en `TextosDePlanificacion` y el XAML las lee con `{x:Static}`. La regla vale para lo que venga: **si dos pantallas dicen lo mismo, la frase es una constante, no dos literales**. Es lo único que impide el fallo que ya ha aparecido tres veces esta semana — corregir una copia, olvidar la otra, y que el programa siga explicando durante días algo que dejó de ser verdad.

**El separador es `|`, no `·`** (2026‑08‑06). Vale para todo lo que el programa compone juntando dos datos: la línea de estado de gestión —«250 proyectos | 201 fuera del filtro | leídos en 0,9 s»—, el resumen de filtros, los avisos de la portada, el detalle de las barras del calendario, las columnas de acreditación y colaboradores de la BBDD, la ocupación por técnico, la carga, el título de los apartados y el subtexto de las normas. Catorce ficheros.

Tres sitios **conservan el punto medio a propósito**, porque ahí no separa dos cosas:

- Dentro de las **plantillas**, donde es contenido de la norma: «Ensayo de humedad · inicio», «E14 ó B15 · 1,2 Nm», la tabla de energías del IK. Y en un caso es un **signo de multiplicar** —`Fuerza = 0,5 · 1,225 · área · 1,2 · v²`—: cambiarlo habría corrompido la fórmula.
- En el **informe HTML**, que es el documento que sale del laboratorio y no la interfaz. Tiene dos tests que lo comprueban.
- En «Acerca de», donde encabeza cada línea como viñeta.

**Los rótulos de la portada no llevan punto final** (2026‑08‑06). «Abre una toma de notas», no «Abre una toma de notas.». Son rótulos, no frases. Los **avisos** de esa misma pantalla sí lo llevan: son oraciones completas, y varios encadenan dos.

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
| `AltaDeProyectoTests` | Que dar de alta exige **solo el nombre, el técnico y la norma**, y que lo que la norma pide para ensayar **no bloquea el alta** |
| `NombreDeFicheroTests` | Que el nombre del fichero es el que fija el laboratorio, con todas las normas instaladas y con el código escrito de cualquier manera |
| `ProyectosRepetidosTests` | Que un servicio que ya existe se reconoce aunque el código se teclee con otras mayúsculas o espacios, y que un fichero ilegible no provoca un aviso falso |
| VersionDePlantillaTests | Que la versión con la que se registró vuelve al abrir, que guardar la actualiza y que **el informe declara la del registro** y no la instalada |
| `CargaTests` | La cuenta del laboratorio en horas, que cuadre con los 80 €/h, el reparto entre meses y que un `capacidad.json` anterior al cambio no deje la carga a cero |
| `EnlaceTests` | El enlace de las familias de un trabajo: quién hace de cabecera, que el avance y el importe son los del grupo entero y que el enlace sobrevive a que el técnico guarde |
| `PlantillasDeOtrasNormasTests` | Integridad de **todas** las normas instaladas, no solo la de luminarias: ids únicos, referencias entre reglas, `visibleSi` y `reglaDeCierre`, predicados registrados, grupos de equipos existentes, evaluación sin excepciones, que ningún id de bloque se repita entre normas, qué normas se pueden combinar, el grado por muestra y que cada norma exija su propia cabecera. **Los que vigilan la unificación recorren la carpeta**, no una lista escrita a mano, así que una norma nueva queda cubierta por existir |
| `CodigoDeServicioTests` | Que el de servicio son las nueve primeras del de la toma de notas, que **lo escrito a mano no se pisa**, y que el rótulo del tablero y el calendario son las once primeras — servicio y familia, sin la edición |
| `AcreditacionTests` | Que se admiten varias a la vez, que **«Sin acreditar» no admite compañía** en los dos sentidos, y que las cinco normas la exigen |
| `GradosDelServicioTests` | El IP y el IK mayores de un servicio con la regla del laboratorio: manda la segunda cifra, la primera desempata, la «X» cuenta como 0 y «No IK» no es un grado bajo sino no haberlo |
| `BusquedaDeProyectosTests` | El buscador del listado: que busca en todas las columnas, que los filtros se suman y que los desplegables solo ofrecen valores que existen |
| `CadenaDelGrupoTests` | Cómo se pone en fila un trabajo enlazado (DD‑123): que la que empieza antes encabeza y no se mueve, que cada una arranca **al día siguiente** conservando su duración, los valores por defecto —una semana sin fin, mañana sin nada—, que una fecha anterior **adelanta** y que con la misma fecha gana la que se acaba de tocar. Y lo que evita escribir por escribir: una cadena ya colocada no se reescribe, solo se toca lo que de verdad se mueve, y recolocar dos veces da lo mismo. Cierra con el puente entre las dos piezas: **lo que se escribe es exactamente lo que se dibuja** |
| `TramosDelGrupoTests` | En qué se parte la barra de un trabajo enlazado: que las familias se encadenan, que el orden lo dan las fechas de inicio (DD‑123), que las que no traen fecha no desaparecen y que los trozos suman la barra entera. Que **cada una dura lo suyo** (DD‑121): dos planificadas de cinco y quince días salen de cinco y de quince, la anexada no copia el tamaño de la primera, el trabajo acaba donde acaba la cadena y **mover no deforma nada**. Y lo que hace falta para arrastrar el tren (DD‑119, DD‑120): que estirar un extremo **solo agranda la tarjeta de ese extremo**, que las fracciones **siempre suman uno**, y que **siempre hay un tramo por familia y en su orden**, que es lo que permite recalcular las tarjetas en el sitio en vez de rehacerlas |
| `RepartoDelArrastreTests` | Qué se escribe al soltar esa barra: que mover el trabajo desplaza a todas manteniendo las distancias, que estirar un borde toca solo la del extremo y que **una toma de notas suelta se comporta exactamente como antes** |
| `RequisitosParaGuardarTests` | Que guardar exige código y técnico 1 y **nada más**, y que son los mismos dos que pide el alta |
| `RotulosTests` | Cómo se identifica una toma de notas abierta: que cada familia tiene su propia pestaña y que el título lleva la norma **con su año**, que es lo que evita anotar contra la edición equivocada |
| `ResumenDeFiltrosTests` | Que el botón «Filtros» delata cuántos están apartando trabajo, y que «En desarrollo» no cuenta como filtro activo pero sí se nombra |

### Las once trampas de WPF

Cada una costó un rato y todas comparten el mismo carácter: **WPF no avisa**. No falla, no lanza nada, no escribe en ningún sitio — simplemente hace otra cosa. Van explicadas donde mordieron; esto es el índice, porque a estas alturas están repartidas por medio documento.

| # | Qué pasa | Dónde mordió |
|---|---|---|
| 1 | `ReleaseMouseCapture()` levanta `LostMouseCapture` **en el acto**, no al final del método | Arrastre de barras: se cancelaba solo justo antes de guardar |
| 2 | Un elemento que pide más ancho del que le dan **se recorta**, sin decir nada | Cabecera del calendario: en blanco al encoger la ventana |
| 3 | Un evento de la interfaz que revienta **tumba la aplicación entera** | Elegir un técnico cerraba el programa de golpe |
| 4 | Perder la captura del ratón **da el `Click` por cancelado** | Pulsar una barra dejó de abrir su diálogo al añadir el arrastre |
| 5 | Dentro de un `ItemsPanelTemplate` **no resuelven las ataduras** | Portada: no se le podía decir al panel cuántas columnas |
| 6 | Un `StackPanel` horizontal **no envuelve jamás**; lo que no cabe no se dibuja | Barra de gestión: desaparecían botones y filtros al estrechar |
| 7 | `DataContext` y `Visibility` en el mismo elemento **se estorban** | La BBDD se dibujaba encima de las otras tres vistas |
| 8 | Lo que se arrastra **no se puede recrear** mientras dura el gesto | Tren de tarjetas: el arrastre se cancelaba a mitad |
| 9 | `RequestBringIntoView` atendido en el `ScrollViewer` **llega tarde** | La cabecera saltaba de lado al marcar una norma |
| 10 | `TreeView.SelectedItem` es de **solo lectura**: no se le puede decir qué señalar, y el árbol solo avisa cuando la selección **cambia** | Tras pulsar «Planificación» no se podía volver a «Datos del proyecto»: el nodo seguía señalado, así que pulsarlo no cambiaba nada y no disparaba el evento |
| 11 | El nombre accesible **solo se deduce de un contenido que sea cadena**; con una plantilla dentro, WPF llama a `ToString()` | El índice entero se anunciaba como `LumNotas.App.ViewModels.SeccionViewModel`, y los botones con panel dentro, sin nombre |

Dos corolarios de la sexta, que valen por sí solos: dentro de un `ScrollViewer` horizontal **el ancho disponible es infinito**, así que `HorizontalAlignment="Stretch"` no estira y `MaxWidth` no acota; y por lo mismo `TextTrimming` **no pone puntos suspensivos** dentro de un `StackPanel` horizontal — el texto se corta a mitad de letra.

Y un tercero, que salió al virtualizar (DD‑131): **una lista dentro de un `ScrollViewer` de fuera se mide con tamaño infinito, así que no puede virtualizar nunca**. Da igual poner `IsVirtualizing="True"`: si algo por encima le da alto infinito, la lista concluye que se ve entera y crea sus 250 filas. Para virtualizar, la lista tiene que ser **dueña de su propio desplazamiento**.

### La trampa que no es de WPF: la caché del escaneo

Merece sitio propio porque no se parece a las otras y volverá a morder.

**Al añadir un campo al resumen del proyecto hay que subir `CacheDeResumenes.Formato`.** Si no, el tablero sirve resúmenes viejos —guardados con la forma anterior, sin el campo nuevo— y el dato sale en blanco. No falla nada: el cálculo está bien, los tests pasan, y la pantalla miente.

Pasó el 2026‑08‑06 con el porcentaje ponderado (DD‑137), y en su versión más engañosa: la tarjeta enseñaba **las semanas sí y el porcentaje no**, porque las semanas se leen del fichero en cada escaneo y el porcentaje venía de la caché. Se sospechó del motor de reglas, se montó un programa aparte para medirlo y salió correcto — y el aviso llevaba meses escrito encima de la propia constante.

Un comentario no bastó, así que ahora lo vigila un test: cuenta los campos guardados de `ResumenDeProyecto` y falla si aparece uno nuevo sin que la marca de forma suba. **Un aviso que hay que leer no es una salvaguarda; una que falla el build, sí.**

### Qué se midió y qué no

**Ninguna de las mejoras de rendimiento se hizo por intuición** (2026‑08‑06). Hay un banco de pruebas —fuera de la solución— que genera 250 proyectos en `Clientes/TECNOnnn/TomaDeNotas/` y cronometra el escaneo; y la propia ventana se midió con un cronómetro a `DispatcherPriority.ContextIdle`, que es lo único que corre **después** de maquetar y pintar.

Los proyectos de prueba **se clonan de los reales** y se les cambia código, técnico y fechas: con ficheros vacíos el motor no trabaja y el escaneo sale más rápido de lo que es. El banco tiene dos modos: `generar`, que hace una carpeta desde cero, y `escenario`, que **solo añade** —200 archivados y 50 vivos repartidos entre los técnicos del laboratorio— y por eso se puede apuntar a una carpeta que ya tenga cosas. Ninguno de los dos borra nada que no haya puesto él: la primera versión hacía un `Directory.Delete` a secas, y apuntarla a la carpeta de proyectos de verdad —que es exactamente lo que uno quiere hacer para probar sobre OneDrive— se habría llevado el trabajo del laboratorio por delante.

Sirvió sobre todo para **descartar tres cosas que parecían el problema y no lo eran**:

| Sospecha | Medida | Veredicto |
|---|---|---|
| El escaneo de la carpeta | 0,6 s en frío, **74 ms** en caliente | No es el problema |
| El recálculo del núcleo (filtrar, calendario, carga) | **7 ms** entre los tres | No es el problema |
| `EntradaDeCalendario` recalculando `Inicio`/`Fin`/`Tramos` ~8 veces por proyecto | **6,5 ms** | Se **descartó** cachearlo: habría tocado el arrastre para no ganar nada |
| Rellenar las colecciones de una en una | Reemplazarlas en bloque salió **igual** | Se dejó hecho por higiene, no por velocidad |
| Crear los controles de las 250 columnas del tablero | **1,7 s** | **Era esto** |

Dos avisos para quien vuelva a medir. El primero: **el escaneo va en segundo plano**, así que cronometrar «pulsar Actualizar» no mide nada — la ventana queda libre en el acto y el trabajo llega después. El segundo: **un cliente de accesibilidad hace que WPF construya objetos que un usuario normal no paga**, así que una medida tomada con un script de UIA puede estar midiendo el propio medidor; aquí se comprobó repitiéndola sin tocar accesibilidad, y el número aguantó.

Lo que **no** se tocó, a sabiendas: el volcado de la caché de resúmenes (189 ms de los 294 que cuesta reescanear tras mover una barra) y el doble recorrido del árbol de carpetas (69 → 38 ms). Los dos son arreglables y los dos tocan el guardado o la detección de proyectos; con la ventana ya respondiendo en 141 ms, no compensan el riesgo.

### La interfaz

La pantalla **se genera a partir de la plantilla**, no está escrita a mano: añadir un apartado al JSON lo hace aparecer en la aplicación sin tocar código. Elementos:

- **Índice de apartados** a la izquierda, con semáforo por estado (faltan datos / completo / no aplica). Sustituye a la hoja `Índice` y al formato condicional que coloreaba las cabeceras.
- **Formulario** a la derecha, con una columna por muestra en vez de las ocho columnas fusionadas del Excel. Los grupos repetidos (tornillos, uniones, prensaestopas) se generan a partir de `grupoRepetido`.
- **Avisos** con los mismos textos que veía el técnico en el Excel, en su apartado y no en una celda perdida.
- **Lo que no aplica no se puede rellenar.** Al marcar «Este apartado no aplica (N/A)», o una exención de subapartado como «La luminaria NO tiene tornillos», los campos, equipos y comentarios de debajo se desactivan. Antes seguían siendo editables y se podían guardar proyectos que decían a la vez que un apartado no aplica y traían sus datos de ensayo. Las casillas que toman la decisión quedan activas, para poder volver atrás.
- **El panel no se desplaza de lado al pulsar una casilla.** WPF, al enfocar un control, pide llevarlo a la vista y el `ScrollViewer` se movía también en horizontal: con la tabla de muestras, marcar una casilla saltaba a otra columna; en la cabecera, marcar una norma añadida daba un salto de lado, porque su casilla ocupa **todo el ancho del contenido** y enseñarla entera obliga a recorrerlo. Se **cancela la petición y se hace a mano solo el desplazamiento vertical**, que es lo que hace falta al tabular entre campos.

> **Novena trampa de WPF: atender `RequestBringIntoView` en el `ScrollViewer` llega tarde.** Quien desplaza no es él, sino el **`ScrollContentPresenter` de su plantilla**, que está **por debajo** en el árbol visual. El evento burbujea de abajo arriba, así que cuando llega al `ScrollViewer` el presentador ya lo ha atendido y la vista ya se ha movido: marcarlo como atendido allí no deshace nada. El manejador va **dentro**, en el `ContentControl` que cuelga del `ScrollViewer` — ahí el evento pasa antes que por el presentador y sí se le puede cortar el paso.
>
> Con esto cayeron dos intentos previos. El primero rehacía la petición con un **rectángulo sin ancho**: quitaba media enfermedad, pero ese rectángulo pide **el punto x=0 del control**, así que con el panel desplazado a la derecha volvía de un salto a la izquierda. El segundo cancelaba el evento y desplazaba a mano solo en vertical —lo correcto— pero **desde el `ScrollViewer`**, o sea tarde. Medido con automatización: al enfocar la casilla de la 62031, el desplazamiento horizontal pasaba de 0 % a 2,47 % (unos 17 DIP). Con el manejador dentro se queda en 0 %, y desde un panel ya desplazado al 45 % sigue clavado en 45 % mientras el vertical hace su trabajo.
- **Contador de apartados** en la cabecera. El porcentaje ponderado y la barra de progreso se retiraron (DD‑25) por no aportar nada; el cálculo sigue en `IndicadorDeAvance`.
- **El índice se pliega** con el botón ◀ / ▶ de la cabecera. Con 30 muestras, esos 360 px son la diferencia entre ver tres columnas o siete.
- **Una barra de pestañas y una sola**: los proyectos y el tablero al mismo nivel, con el `+` detrás de la última. La pestaña de delante va en negrita y con fondo más claro.
- **Menús**: `Archivo` (nuevo proyecto, pestañas, abrir, guardar, exportar), `Ver` (**Inicio** y el tablero), `Configuración`, que guarda lo que es del laboratorio entero y no de un proyecto —las **dos carpetas**, la **lista de técnicos**, la **capacidad y tarifa** y las **normas instaladas**—, y `Ayuda`, con **«Reportar un problema…»** y «Acerca de…». Se llamó *Configuración* y no *Admin* porque no hay roles ni permisos que administrar (DD‑62).
- **El técnico se elige de una lista, no se escribe.** Técnico 1 y Técnico 2 son desplegables en las cuatro normas, vacíos de partida. Técnico 1 es el **responsable** del servicio y por él se agrupa el calendario y se calcula la carga.

### Pestañas: varios proyectos a la vez

El técnico suele llevar dos o tres servicios en marcha —mientras una muestra pasa 48 h en la cámara de humedad ensaya otra—, así que la aplicación abre **una pestaña por proyecto**, como un navegador:

- El botón **`+`** o *Archivo · Nueva pestaña* (Ctrl+T) abre una pestaña vacía, que enseña la portada.
- *Archivo · Abrir otro proyecto…* (Ctrl+O) abre en la pestaña de delante si está sin estrenar, y si no en una nueva.
- **Abrir un fichero ya abierto salta a su pestaña** en vez de duplicarlo: dos pestañas sobre el mismo `.lmnlab` se pisarían los guardados.
- Cerrar una pestaña con cambios avisa. Cerrar la última deja una vacía, no una ventana en blanco.
- **Al cerrar la aplicación se pregunta por cada pestaña con cambios**, no solo por la de delante.
- Ctrl+S y Ctrl+P actúan sobre la pestaña activa.
- **El tablero de gestión es una pestaña más**, y solo una: se abre desde la portada —por cualquiera de sus cuatro vistas— o desde *Ver · Gestión de proyectos*, y volver a pedirlo salta a la que ya está. No hay pestañas de dos niveles.

Eso obligó a partir en dos lo que era una sola clase: `DocumentoViewModel` es **un proyecto abierto** —datos, motor, cabecera, árbol, ruta y cambios sin guardar— y `VentanaPrincipalViewModel` es la ventana que sostiene la colección de documentos, el tablero y los menús. **El núcleo no se tocó**: `DatosProyecto`, el motor y las plantillas ya recibían todo por parámetro y no guardaban estado global, que es justo lo que hizo viable el cambio.

### La portada

La aplicación arranca en una portada, no sobre un proyecto de luminarias en blanco. Es lo que enseña **una pestaña recién abierta**, igual que la página de inicio de un navegador.

Encabeza **el nombre del programa y su versión** (DD‑81), no el de una de sus dos mitades: se llamaba «Toma de notas de ensayos», que dejó de ser verdad en cuanto la gestión pasó a ser la otra mitad. El nombre está escrito en **un solo sitio**, `<Product>` del `.csproj`; la portada, la barra de título y «Acerca de» lo leen del ejecutable. La versión se enseña en la portada, y no solo en «Acerca de», porque es lo primero que hay que preguntar cuando alguien avisa de un fallo.

Debajo, **«Software de toma de notas primarias para ensayos y gestión de proyectos.»** — sin citar ninguna norma: el programa lleva cuatro y las que se dejen caer en la carpeta, así que nombrar una sola describía mal lo que hace.

**Partida por la mitad** (DD‑80): a la izquierda la toma de notas, a la derecha gestionar.

**Tres pesos y una primaria por zona** (DD‑135): relleno de color para la primera de cada mitad, contorno para lo frecuente, sin caja para lo esporádico. Recientes, normas y las cuatro vistas van en **rejillas de dos columnas**; las vistas llevan icono.

| Toma de notas (izquierda) | Gestión (derecha) |
|---|---|
| **Abrir existente**, en azul | **Planificar nueva TdN**, en verde |
| Los **cuatro últimos abiertos**, bajo «RECIENTES», en dos por dos | **Tablero** · qué falta por rellenar |
| «O crea una nueva con la norma que necesites» y **una tarjeta por norma instalada** — sale de `CatalogoDeNormas`, así que dejar caer un `plantilla-*.json` añade la suya sin tocar nada | **Calendario** · cuándo toca cada uno |
| | **Carga** y **BBDD** |

**El orden lo puso el laboratorio** (2026‑08‑06): primero abrir, luego lo reciente, y las normas al final. Antes las tarjetas encabezaban el panel. El día a día es volver a una toma de notas empezada; estrenar norma es lo que menos ocurre, así que estaba lo raro arriba y lo frecuente abajo.

Cada mitad tiene **un solo botón lleno de color** y es el que más se pulsa: azul a la izquierda, verde a la derecha, mismo tamaño y mismo redondeo.

En la portada se enseñan **cuatro** recientes, no todos, y **solo el nombre**: la carpeta se quitó de la fila —con dos líneas por fila, cinco filas no cabían en una pantalla de 800— y se quedó en el consejo emergente, que es donde se mira para desempatar dos servicios parecidos. `Archivo | Proyectos recientes` los sigue teniendo todos, que para eso hay que ir a buscarlo.

**Las tarjetas de norma salen ordenadas por su título**, con luminarias primero por ser la de uso más frecuente; el resto queda «Grados IK», «Grados IP», «Módulos LED». No hay ninguna lista de orden escrita en el código **a propósito**: una lista fija habría que tocarla cada vez que entre una norma nueva, y lo que se quiere es que dejar caer un JSON baste. Si algún día el laboratorio quiere otro orden, el sitio es un campo en la propia plantilla, no el C#.

> **Hubo una zona «Proyectos» y duró un día.** El 2026‑08‑02 la portada se partió en tres —Proyectos, Gestión y «toma de notas sin proyecto asignado»— dando por hecho que existiría un objeto proyecto por encima de las tomas de notas. El laboratorio lo paró: **el registro del ensayo es el `.lmnlab`**, y un fichero de proyecto flotando fuera de la carpeta de tomas de notas sería un documento sin ensayo detrás (DD‑89). Sin ese objeto, «con proyecto» y «sin proyecto» dejan de distinguir nada y la tercera zona sobra. Queda escrito para que a nadie se le ocurra volver a partirla sin leer DD‑89.

**El alta rápida no está en la portada** (DD‑93). Estuvo, junto a las tarjetas de norma, y era ofrecer dos caminos para lo mismo: elegir norma y dar de alta acaban las dos en una toma de notas. Se queda en `Archivo | Nueva toma de notas…` y en la barra del tablero, que es donde está el responsable cuando se le ocurre.

**En la portada no hay ajustes.** Estuvo un rato la carpeta de proyectos con su «Cambiar carpetas…», y el laboratorio lo quitó el mismo día: la pantalla de inicio es para empezar a trabajar, y una ruta de configuración a un clic invita a tocarla sin querer. Las carpetas se cambian donde se cambia todo lo demás, en *Configuración*.

**Cada vista tiene su enlace directo**, no solo el tablero: entrar a gestión y buscar después el botón de la vista eran dos pasos para una sola intención. El menú *Ver · Gestión de proyectos* hace lo contrario a propósito — **respeta la vista en la que se estaba**, porque volver desde el menú a media planificación no debería devolverte al tablero.

Las dos columnas son `1.35*` y `*`, **en proporción y sin ancho fijo ni mínimos**: así nadie puede pedir más ancho del que hay, que es lo que provocaba el **recorte silencioso de maquetación** de WPF (segunda trampa del calendario). La de la izquierda se lleva algo más porque es la que tiene que caber tres tarjetas de norma en fila; la de gestión se apaña con menos.

**Las tarjetas de norma son todas del mismo tamaño y se recolocan solas** (DD‑102): tres por fila cuando cabe y dos al estrechar la ventana. Es un `WrapPanel` con tarjetas de tamaño fijo, no un `UniformGrid` — a este hay que decirle cuántas columnas, y **desde un `ItemsPanelTemplate` no se llega al ancho del contenedor** (quinta trampa, abajo). Antes se ajustaban al texto y quedaban desparejas: «IP» estrecha y «Módulos LED» ancha, sin más motivo que la longitud del nombre.

Los tres botones de gestión **encogen dejando que su texto pase a dos líneas** en vez de recortarse, que es lo que permite ganar el sitio de la tercera columna. Verificado en la ventana más pequeña que se admite, **620 px** (DD‑106): cinco tarjetas iguales en dos filas y los rótulos de gestión partidos, sin nada recortado.

En cuanto se elige norma o se abre un fichero, esa misma pestaña pasa a ser la toma de notas. Para volver, **`Ver | Inicio`**: salta a una pestaña vacía si la hay y abre una si no queda ninguna — no cierra nada ni deja pestañas iguales acumuladas.

### Qué hace falta para guardar

**Dos cosas, y solo dos: el código de la toma de notas —entero, 14 caracteres— y el técnico 1** (DD‑115, DD‑130). Sin ellos el fichero no se puede ni nombrar ni atribuir.

Conviene no confundir tres listas que se parecen y no son la misma:

| Lista | Qué decide | Dónde vive |
|---|---|---|
| **Para dar de alta** | Crear el `.lmnlab` desde el responsable | `AltaDeProyecto` — código entero, técnico 1 y norma |
| **Para guardar** | Escribir el fichero en disco | `RequisitosParaGuardar` — código entero y técnico 1 |
| **Para ensayar** | Que aparezcan los apartados de ensayo | `RequisitosDelProyecto` — lo que exija cada norma |

Sobre el **código** las tres dicen lo mismo (DD‑130): entero o nada. Lo que las separa son los demás datos —clase, Ta, acreditación, partes ‑2—, que solo hacen falta para ensayar.

**La tercera es mucho más larga, y guardar no la mira.** Un servicio sin clase, sin Ta y sin acreditación tiene que poder guardarse: así está durante semanas, y bloquear ahí obligaría al técnico a inventarse datos para no perder lo anotado.

Al intentar guardar sin lo mínimo, **la vista salta a la cabecera**: decir que falta algo sin enseñar dónde obliga a buscarlo, y ahí los dos campos ya están en rojo.

> **Un fichero anterior a DD‑104 no traía código de toma de notas**, así que al abrirlo y guardarlo pedirá que se ponga. Es lo buscado: ese código es lo que lo distingue de las otras familias del servicio.

### Cambios sin guardar

Todo lo que abandona el proyecto abierto pasa por el mismo aviso: proyecto nuevo, abrir, abrir un reciente, volver a la portada y **cerrar la aplicación**. El diálogo es propio, no el `MessageBox` de Windows, para que lo diga el botón y no haya que traducir un «Sí / No»:

- **Guardar cambios** (azul) guarda y luego continúa. Si el proyecto es nuevo pide carpeta y, **si se cancela ahí, no se continúa**: no se puede acabar sin proyecto y sin fichero.
- **Continuar sin guardar** (gris) descarta.
- Cerrar el aviso cancela la acción y no se pierde nada.

### El tablero de gestión de proyectos

Pestaña propia, pensada para el responsable y no para el técnico. Tiene **cuatro vistas de la misma carpeta**, que responden a cuatro preguntas distintas:

| Vista | Pregunta | Filtro |
|---|---|---|
| **Tablero** | ¿Qué falta por rellenar? | El compartido |
| **Calendario** | ¿Cuándo toca cada servicio? | El compartido |
| **Carga** | ¿Cabe? | El compartido |
| **BBDD** | ¿Dónde está aquel proyecto de hace meses? | **El suyo propio** |

**La BBDD queda fuera del filtro compartido a propósito** (DD‑109): las tres primeras miran lo que hay en marcha y arrancan en «En desarrollo»; la cuarta mira hacia atrás, y lo que se busca en ella casi siempre está terminado.

El tablero es lo primero que se construyó: **columnas = proyectos, tarjetas = secciones pendientes** (a lo Trello).

**La cabecera de cada columna lleva los mismos dos iconos que la barra del calendario** (DD‑136, pedido el 2026‑08‑06): la **caja** cuando las muestras ya están en el laboratorio y el **candado** cuando las fechas están blindadas. Son los mismos `IconoCaja` e `IconoCandado`, dibujados como trazo para que tomen el color de donde se pongan — blancos sobre la barra de color, ámbar y gris sobre la tarjeta gris.

Van aquí porque el tablero es donde se decide **qué se coge hoy**, y eso no depende solo de lo que quede por rellenar: un servicio al que le faltan doce apartados pero cuya muestra sigue en el transportista no se puede empezar. Sin el icono, ese dato había que ir a buscarlo al calendario.

Los colores no son decorativos: **ámbar el que empuja a hacer algo** —las muestras están aquí— y **gris el que solo informa** —no muevas estas fechas—. Los dos pasan el 3:1 de contraste no textual sobre el fondo `#EEF0F3` (4,4 y 4,2). La ausencia es la señal contraria: no hay icono de «muestras aún sin llegar», porque dieciséis columnas con un icono tachado no dicen nada.

> El rótulo va en una **rejilla** y no en un `StackPanel` horizontal. Dentro de un `StackPanel` el ancho es infinito y el código dejaría de envolver — es la misma trampa que ya estaba documentada en las barras del calendario.

#### Las tres formas de contar el avance, y cuál manda

El programa sabe medir lo hecho de tres maneras distintas, y las tres siguen existiendo porque contestan a preguntas distintas (DD‑137):

| Cuenta | Se ve en | Contesta |
|---|---|---|
| **Peso** (`PorcentajePonderado`) | el `45 %` del tablero, del calendario y del informe | ¿cuánto trabajo queda? |
| **Secciones** | el `7/16 secciones` de la tarjeta | ¿por dónde voy? |
| **Apartados** | el `12/45 apartados` de la toma de notas y del informe | ¿cuántas casillas quedan? |

**El porcentaje es siempre el ponderado.** No es una preferencia estética: es el que ya lleva impreso el informe que se firma, y dos porcentajes distintos con el mismo nombre serían peores que ninguno. Los pesos —3, 5, 10 y **100 para endurancia**— los declara la plantilla y salieron del Excel del laboratorio, así que **el programa no decide cuánto vale un ensayo**: lo lee.

> Conviene saber cómo se mueve ese número antes de fiarse de él. Con endurancia valiendo 100 de 217, **el marcador se pasa media vida por debajo del 55 % y luego salta**. No está roto: endurancia dura semanas y el Excel lo ponderó así a conciencia (D‑18). Pero es la razón por la que la tarjeta sigue enseñando también `7/16 secciones`, que sí avanza a pasos regulares.

Dos reglas que parecen detalles y no lo son:

- **Nunca 100 % por redondeo.** Se trunca hacia abajo salvo cuando de verdad no queda peso. Un 99,6 % redondeado al alza pondría el cartel de acabado en un servicio al que le falta un ensayo, y eso se firma.
- **Sin pesos, ningún número.** Nulo, no cero. Un cero fijo diría «0 %» hasta en un servicio terminado, y nadie sabría si es que no se ha hecho nada o que no se sabe.

El renglón de la tarjeta —`3W | 45 % | 7/16 secciones`— lo arma el resumen, no cada vista, y **lo que no hay se cae**: sin fechas no hay `3W`, sin pesos no hay `%`.

#### El porcentaje dentro de la barra del calendario

En el calendario van los mismos dos datos detrás del código, **más apagados**, para que al estrecharse la barra lo que se pierda sea el añadido y no el nombre. Pero medido con 47 servicios reales, **no cabe casi nunca**: ni con el zoom al máximo entra `EDISO260909 | 3W | 100 %` en una barra de tres semanas.

Por eso el porcentaje **se dice también con la forma**: lo hecho se aclara sobre el color del estado, al modo de un Gantt. Un relleno no gasta ancho —se lee igual en una barra de una semana que en una de seis— y sobrevive al recorte del texto. Va en columnas `*` y no en píxeles para que se reparta solo mientras se arrastra la barra, sin recalcular nada en cada latido del gesto.

> **`W` es duración; `S` es número de semana.** El eje rotula `S32` —«entra en la S32»— y la barra rotula `3W` —«tres semanas de trabajo»—. Son dos cosas distintas y por eso llevan letras distintas.

#### Qué proyectos se miran

Un solo desplegable **«Mostrar»** para las cuatro vistas: el responsable decide una vez qué le interesa y el tablero, el calendario, la carga y la BBDD hablan de lo mismo. Por defecto, **«En desarrollo»** — todo menos lo archivado.

| Opción | Qué enseña |
|---|---|
| **En desarrollo** | **todos los estados menos lo archivado**, terminados incluidos |
| Un estado concreto | ese estado, **sin** lo archivado: quien busca «En curso» no quiere lo que se apartó |
| Archivados | solo lo apartado de en medio |
| Cualquier estado | todo a la vez, archivado incluido. Es lo que hace buscable la BBDD |

> **Lo único que esconde es archivar.** Hasta el 2026‑08‑05 «En desarrollo» dejaba fuera también lo terminado, y se cambió porque escondía trabajo vivo: un servicio terminado la semana pasada se sigue mirando —hay que facturarlo, el cliente pregunta—. **El texto del diálogo se quedó atrás** y siguió diciendo «deja fuera lo terminado y lo archivado» durante dos días, hasta que el laboratorio lo cazó leyéndolo (2026‑08‑07). La misma frase estaba copiada en otros cuatro sitios. Cuando cambia una regla hay que buscar la frase, no solo el código.

**Lo archivado se pide por su nombre** (DD‑79). Había una opción «Todos» que traía literalmente todo, y el laboratorio la quitó de en medio (2026‑08‑02): quien la elegía buscaba «los proyectos de todos los técnicos», no «también los de 2019». Como sin lo archivado «Todos» decía exactamente lo mismo que «En desarrollo», se dejó una sola entrada en vez de dos idénticas; el nombre viejo se sigue reconociendo para que quien lo tenga guardado no se encuentre el tablero en blanco.

> Aquí decía que **ninguna opción general traía lo terminado**, y dejó de ser cierto tres días después (2026‑08‑05): lo terminado volvió a «En desarrollo» porque un servicio terminado la semana pasada se sigue mirando. **Y con ello volvió el problema que lo había sacado**: en la carga *«un servicio terminado seguía sumando sus días al mes, así que el porcentaje del técnico salía inflado con trabajo que ya nadie va a hacer»*. Se arrastró dos días hasta que el laboratorio lo cazó probando (DD‑142). Ya no lo arregla el filtro sino **el propio cálculo de carga**, que es donde tenía que estar: lo terminado se sigue viendo en el tablero, en el calendario y en la BBDD, y solo deja de contar donde la pregunta es «¿cabe?».

**Manda el estado que puso la persona, no el que deduce el programa** (DD‑74). Antes «terminado» significaba dos cosas: el tablero usaba el calculado —todas las secciones rellenas— y el calendario el manual. Así, un servicio con todo relleno pero **esperando confirmación del cliente salía como terminado** sin que nadie lo hubiera dicho. Lo calculado se queda como lo que es: el avance, `12/16 secciones`.

Los dos mecanismos de ocultar tienen papeles distintos:

- **Terminado** → el trabajo está hecho. Sale de la vista **solo**, sin que nadie tenga que acordarse de archivar, porque marcar el estado ya forma parte del trabajo.
- **Archivado** → «quítamelo de en medio» pase lo que pase: lo cancelado, lo aparcado, lo viejo que estorba. Antes solo ocultaba en el calendario; ahora **oculta en las tres vistas**, que es lo que cualquiera esperaría al pulsarlo.

Junto al estado están los otros dos filtros, **técnico y norma**, que también valen para las tres vistas. El de técnico estaba solo en el calendario y el laboratorio lo pidió también en el tablero (2026‑08‑02); en vez de duplicarlo se subió a la barra común, igual que el de estado.

Así, **todo lo que decide qué proyectos se miran está en una sola fila**, y en el calendario solo quedan sus ajustes propios: agrupar por técnico, zoom y navegación.

Cambiar de filtro **no vuelve a escanear**: se filtra lo ya leído, así que es instantáneo. Y la línea de estado dice siempre cuántos se han quedado fuera —«3 proyectos · 2 fuera del filtro»—, porque un filtro que esconde en silencio es peor que no tenerlo.

Las listas de técnicos y normas salen **de los proyectos que hay**, no de una lista fija. Si el elegido desaparece —se archivó el último servicio de ese técnico— se vuelve a «(todos)» en vez de dejar el tablero vacío sin explicar por qué.

**«(sin técnico)» es una opción más** del filtro de técnico (DD‑92), y aparece sola cuando hay algún servicio sin responsable. Enseña justo los que están sin asignar, que es lo que el responsable quiere ver para repartirlos. Va **al final de la lista**, después de las personas: es un cajón, no un compañero.

Es el **mismo rótulo** con el que el calendario agrupa las filas y la carga nombra su última línea, y está escrito en un solo sitio (`CargaPorTecnico.SinTecnico`). Si cada vista se inventara el suyo, filtrar por «(sin técnico)» dejaría de casar con lo que enseña el calendario. Lleva paréntesis a propósito: nadie se llama así, de modo que no puede chocar con un técnico de verdad.

> **No hace falta —ni conviene— crear un técnico llamado «Sin técnico»** en `Configuración | Técnicos`. Saldría como una persona en el desplegable de Técnico 1 de todas las tomas de notas y, en cuanto se asignara, la carga lo trataría como alguien con capacidad y sumaría días a su nombre. El hueco vacío ya se agrupa solo.

Cómo encuentra los proyectos: se le indica **la carpeta de proyectos** y la escanea buscando `*.lmnlab`, incluidas todas sus subcarpetas. Sin índice ni base de datos — con varios técnicos sincronizando, un índice se desincroniza y miente; el fichero es la única verdad (DD‑27).

| Pieza | Qué hace |
|---|---|
| `ExploradorDeProyectos` | Escanea la carpeta y **aísla los ficheros ilegibles**: uno corrupto sale como tarjeta de error en vez de tumbar el tablero. Preparado para árboles grandes, ver más abajo |
| `CacheDeResumenes` | Lo ya analizado, guardado **entre sesiones** en el perfil del usuario |
| `AnalizadorDeProyectos` | Calcula el resumen reutilizando el mismo `MotorDeReglas` de la toma de notas |
| `EstadoDeApartado` | El semáforo, movido de la interfaz al núcleo para que ambas pestañas usen exactamente la misma lógica |
| `FiltroDeEstado` | Qué proyectos se miran, común a las tres vistas |
| `Ajustes` | Recuerda entre sesiones las dos carpetas y si ya se preguntaron |

El avance se cuenta **por secciones** (DD‑28): la sección 7 vale 1 aunque tenga trece apartados dentro. Es la vista que pidió el laboratorio.

**Cada proyecto se mide contra las normas que él dice llevar** (DD‑77). Hasta el 2026‑08‑02 el tablero medía todo contra una sola plantilla —la que estuviera abierta—, con dos consecuencias feas: un servicio de luminarias **con módulos LED 62031 no enseñaba los apartados de la 62031**, y podía darse por terminado con media toma de notas sin rellenar; y un servicio de IP se evaluaba contra las reglas de luminarias, que no son las suyas.

Ahora se cargan las plantillas instaladas una vez por sesión y cada proyecto se analiza con las suyas.

**La norma principal se detalla; cada norma añadida ocupa una sola línea** (DD‑78):

```
Sección 16 - Bornes con tornillos      1 de 1 apartados pendientes
Sección 17 - Bornes sin tornillo       1 de 1 apartados pendientes
Ensayo de IK - EN/IEC 62262            1 de 1 apartados pendientes
Módulos LED — EN/IEC 62031            26 de 26 apartados pendientes
```

Esa línea trae la cuenta de **toda** su toma de notas, así que **no desaparece hasta que la norma entera está completa**. Es lo que pidió el laboratorio: al responsable le interesa el detalle de lo que está ensayando y, de lo añadido, solo si queda algo por hacer. Desplegar la 62031 entera dentro de un servicio de luminarias enterraba lo importante — y encaja con cómo se veía ya el IK, que al vivir dentro de la plantilla de luminarias siempre fue una línea.

Cuál es la principal lo delata **cómo se nombran las muestras**: `EBP_SAFE…` en las de seguridad y `EBP_CLIM…` en IK, y ese patrón lo fija la norma con la que nació el proyecto. Si eso no lo aclara, manda luminarias.

Un proyecto que no apunte sus normas —los guardados antes de que se registraran— se mide con la de por defecto, para no dejarlo sin medir.

#### Escanear una matrioska de carpetas

Los proyectos del laboratorio cuelgan de la carpeta de clientes, cada uno en su rama y con años de historia detrás. El tablero **lee entero cada `.lmnlab`** que encuentra —el estado sale de aplicarle las reglas—, así que sobre un árbol grande eso se nota. Cuatro medidas, por orden de lo que aportan:

| | |
|---|---|
| **Caché entre sesiones** | Lo ya analizado se guarda en el perfil del usuario. Un proyecto que no ha cambiado **no se vuelve a leer**, ni siquiera tras reiniciar |
| **En segundo plano** | El escaneo ya no bloquea la ventana. Mientras dura se sigue viendo lo anterior, se cuenta por dónde va y hay una barra de actividad |
| **Lectura en paralelo** | Ocho proyectos a la vez: sobre OneDrive el tiempo se va esperando al disco, no calculando |
| **`IgnoreInaccessible`** | En una carpeta de clientes con años siempre hay alguna rama sin permisos. Sin esto, **una sola abortaba el escaneo entero** |

Medido con **400 proyectos en 40 clientes**: primera vez **461 ms**, sesiones siguientes **107 ms**. Sobre OneDrive la diferencia es mayor, porque ahí cada lectura cuesta bastante más que en disco local.

La caché se valida por **fecha, tamaño y versión de la plantilla**: el resumen sale de aplicar las reglas de una norma, así que al publicar una versión nueva deja de valer sola. Y **solo se reescribe si ha cambiado algo** — sin esa comprobación, un refresco sin cambios reescribía el fichero entero y se comía tres cuartas partes de lo ahorrado.

Va en el perfil del usuario y no en la carpeta compartida: es una caché de este equipo, y varios técnicos escribiéndola a la vez sobre OneDrive solo daría conflictos. Si se corrompe, se tira y se rehace.

### Las dos carpetas del laboratorio

**Son dos carpetas de OneDrive distintas** (confirmado por el laboratorio el 2026‑08‑02), elegidas una vez en cada equipo:

| Ajuste | Qué guarda |
|---|---|
| **Carpeta de proyectos** | los `*.lmnlab` |
| **Carpeta compartida** | `plantilla/` (las normas), `tecnicos.json`, `capacidad.json`, `version.json` |

Los proyectos **no están todos juntos**: cuelgan de la carpeta de clientes, cada uno en su rama.

```
clientes/antares/antar2504/01/tomadenotas/antar2504.lmnlab
clientes/moonoff/moono2304/01/tomadenotas/moono2304.lmnlab
```

Basta con apuntar a `clientes`: se busca en ella **y en todas sus subcarpetas**, así que la profundidad y la forma de cada rama dan igual.

Si la carpeta compartida se deja en blanco **se usa la de proyectos**, para no romper una instalación que las tenga juntas.

> **Los ficheros compartidos se escriben donde diga la configuración, y eso incluye equivocarse.** El 2026‑08‑03 aparecieron un `tecnicos.json` y un `capacidad.json` sueltos **en la carpeta del código fuente**, de cuando esa era la carpeta configurada. No se rompió nada —la aplicación simplemente no los leía y usaba los valores de partida—, pero durante días la lista de técnicos que se veía no era la que alguien había editado. Si algo editado no aparece, lo primero es mirar `Configuración | Carpetas`, que dice de dónde sale cada cosa.

Se eligen en **`Configuración | Carpetas: proyectos y compartida…`** —el menú las nombra las dos porque en «Carpeta del laboratorio…», en singular, no se adivinaba que dentro estuvieran ambas—, que enseña de cada una qué contiene —proyectos encontrados, normas publicadas, técnicos, tarifa y versión— y avisa en rojo si una ruta guardada ha dejado de estar accesible. **Se pregunta sola la primera vez** que se abre el programa en un equipo.

> La carpeta compartida tiene que ser **de lectura para todo el mundo**, aunque solo unos pocos escriban en ella. Si los técnicos no pudieran leerla, se quedarían sin normas ni lista de técnicos.

> **Por qué se movió ahí** (2026‑08‑02): estaba solo en el botón «Elegir carpeta…» del tablero, de cuando esa carpeta era únicamente «dónde están los proyectos». Al pasar a gobernar también normas, técnicos, tarifa y versión, quedó en el sitio equivocado: **un técnico que no abriera nunca el tablero no la elegiría jamás**, y trabajaría con las normas de su equipo y su propia lista de técnicos sin enterarse. Justo lo que se quería evitar. El botón del tablero sigue estando y hace lo mismo.

Al cambiarla se releen los técnicos, la tarifa, la versión publicada y los proyectos. **Las normas no se recargan en caliente**: se resuelven una vez por sesión, y cambiarlas con proyectos abiertos dejaría unas pestañas con una versión de la norma y otras con otra. El diálogo avisa de que hay que reiniciar.

### Una sola versión de las normas para todo el laboratorio

Resuelto el 2026‑08‑02. Hasta entonces la carpeta `plantilla` viajaba **junto al ejecutable**, así que cada equipo llevaba su copia: dos técnicos podían estar rellenando **versiones distintas de la misma norma** sin enterarse. En un laboratorio acreditado eso es un problema, no una molestia.

**Ahora manda la carpeta compartida.** Las normas se buscan en `<carpeta de proyectos>/plantilla`; la copia del equipo queda como respaldo.

| Situación | Qué se usa |
|---|---|
| Hay normas publicadas en la carpeta compartida | Las compartidas |
| No están publicadas todavía | Las del equipo, **avisando** |
| No se llega a la carpeta (OneDrive sin conexión) | Las del equipo, **avisando** |

**Y avisa de lo que este equipo tiene y el laboratorio no** (DD‑99). Publicada la primera tanda, el programa lee de la carpeta compartida y **deja de mirar la local**: dejar caer una norma nueva en `plantilla/` no producía **ninguna** señal — el fichero estaba, no aparecía en el programa y nada lo explicaba. Ahora se comparan las dos carpetas y se dice qué falta por publicar, distinguiendo lo que no está de lo que está con una versión anterior. En ámbar y junto al botón que lo resuelve.

El aviso importa tanto como la resolución: seguir trabajando con una versión distinta a la del compañero **no puede pasar inadvertido**. Se ve en `Configuración | Normas instaladas…`, que además lista las normas con su versión y tiene el botón **«Publicar en la carpeta compartida»** — que es la migración entera, de «cada equipo con su copia» a «una sola versión para todos».

Las plantillas y sus catálogos de equipos **se publican juntos**: el catálogo se busca al lado de su plantilla, así que separarlos dejaría apartados sin equipos.

### Cómo se identifica una norma

El `id` de una plantilla lleva **norma, parte y año de publicación** (DD‑95):

| | |
|---|---|
| `id` | `60598-1_2024` | `62031_2020_A11` | `60529_1991` | `62262_2002_A1` — **por la designación EN**, que es contra la que ensaya el laboratorio |
| `idsAnteriores` | `["60598"]` — con lo que se conoció antes |
| `codigoDeFichero` | `60598` — lo corto, que es lo que va en el nombre del fichero |
| `anioDePublicacion` | `2024` — **el año y solo el año**, para poder enseñarlo sin descomponer el id |
| `titulo` | La designación completa, con sus enmiendas: «Módulos LED — EN IEC 62031:2020 + A11:2021». **Sale en el informe**, así que es lo que dice contra qué norma se ensayó |
| `version` | `1.0.0` — **nuestra**, y va aparte |

> **Año de publicación, no «edición»** (DD‑101). Una norma tiene las dos cosas y no son lo mismo: la 60598‑1 de 2021 es la **novena** edición y la de 2024 la **décima**, y lo que el laboratorio usa para distinguir una de otra —y lo que lleva el id— es **el año**. Durante un tiempo este documento y el código llamaron «edición» al año; está corregido. Desde el 2026‑08‑06 la edición **también se guarda**, en `meta.edicion`, pero solo para enseñarla (DD‑134).
>
> Esta ficha decía que la novena edición era la de 2024. **Era falso**, y lo corrigió el laboratorio al dar las cinco ediciones (2026‑08‑06). Queda escrito porque el error estuvo tres días en el documento que se usa para traspasar el proyecto.

**El año está en la identidad porque un ensayo hecho contra la norma de un año tiene que seguir midiéndose contra esa.** Cuando el id era solo `60598`, publicar la norma nueva sustituía el fichero y **remedía en silencio** todos los ensayos anteriores.

**La versión se queda fuera del id.** Se sube por corregir una errata nuestra; meterla dentro crearía una identidad nueva en cada corrección y **dejaría huérfano cada proyecto**.

**El fichero se llama `plantilla-<id>_<version>.json`** (DD‑97): `plantilla-60598-1_2024_1.0.0.json`, con su catálogo de equipos al lado y con el mismo nombre. El **año** va dentro porque es lo que permite que la de 2024 y la de 2021 estén en la misma carpeta; la **versión**, para saber qué hay instalado sin abrir nada.

**Dos años de la misma norma conviven sin tocar código**: son dos ficheros con ids distintos, y el catálogo lista lo que haya en la carpeta. Dos tarjetas en la portada — la vieja para lo que está en marcha, la nueva para lo que empiece. La tarjeta enseña el **número de norma** y el año debajo (*«60598 · Luminarias · 2024»*); el id entero no cabe y al técnico le dice menos.

> **De un mismo id, solo cuenta la versión más alta.** Con la versión en el nombre, publicar una corrección deja las dos plantillas en la carpeta; sin esta regla la portada enseñaría dos tarjetas de la misma norma y habría que adivinar cuál. La anterior se queda de respaldo y deja de contar.

**La migración vive en el JSON, no en C#.** Cada proyecto guardado lleva el id que existía el día que se guardó; `idsAnteriores` dice que sigue siendo la misma norma. Cambiar el esquema no rompió ningún proyecto y no hizo falta convertir nada.

> **El nombre del fichero no cambia**: sigue siendo `TdN_60598_…`. Por eso `codigoDeFichero` va aparte del id — el id creció y el laboratorio quiere el nombre corto.

#### La 60598-1 de 2021

Pedida el 2026‑08‑03 **por retrocompatibilidad**: el laboratorio necesita poder trabajar contra la **EN 60598‑1:2021 + A11:2022**. Existe como `plantilla-60598-1_2021_1.0.0.json`, con id `60598-1_2021`, y convive con la 2024 en la misma carpeta.

**Entre los dos años cambia la numeración de las secciones, no lo que se anota** (DD‑98). Así que la plantilla de 2021 es la de 2024 con la numeración que le toca: 3 Marcado, 4 Construcción, 5 Cableado, 7 Tierra, 8 Choque, 9 IP, 10 Aislamiento, 11 Líneas de fuga, 12 Endurancia, 13 Fuego. Sale de la tabla de equivalencias de la sección 5, extraída del propio libro.

Se planteó la duda de si además cambiaba algún límite, método o ensayo —los dos libros del laboratorio traen **una sola** hoja «Toma de notas 60598», así que el Excel no lo aclaraba— y **lo confirmó el laboratorio**: no cambia. Queda anotado en `meta.nota` del propio fichero, con la fecha.

**Las secciones 16 y 17 —bornes con y sin tornillo— se quedan con su número de 2024**, porque no tienen equivalente en la tabla de numeraciones antiguas del libro.

**Dos años de la misma norma no se pueden mezclar en un servicio**: ninguna se declara compatible con la otra, y hay un test que lo vigila. Comparten ids de bloque —son la misma toma de notas con otra numeración— y en un mismo proyecto se pisarían los datos.

**Solo la 2024 declara `idsAnteriores: ["60598"]`.** Los proyectos guardados antes de DD‑95 dicen «60598» a secas, y eso significa *la que estaba instalada entonces*, que era la 2024. Si la 2021 lo reclamara también, un proyecto viejo se mediría contra una u otra según el orden de la carpeta; hay un test que impide que dos plantillas reclamen el mismo id anterior.

### Con qué versión de la plantilla se registró cada ensayo

Cada `.lmnlab` guarda la versión de la plantilla con la que se escribió. **Se guardaba desde el principio y no se leía nunca**: estaba en el fichero para quien lo abriera con un editor. Ahora vuelve al proyecto al abrirlo (DD‑94), y con eso:

- **El informe declara la versión con la que se registró**, no la instalada el día que se imprime. Si no coinciden, dice las dos: `1.0.0 (registrado) · 1.1.0 (instalada al generar este informe)`. Antes declaraba siempre la de hoy, que es atribuirle al ensayo una plantilla que no se usó.
- **Al abrir un proyecto grabado con otra versión se avisa.** No bloquea: evita que el técnico vea cambiar el avance de un día para otro y lo tome por un fallo suyo cuando fue una corrección de la plantilla.

**Subir la versión y cambiar el `id` no son lo mismo**, y la diferencia no es de formato: es si los ensayos ya hechos siguen valiendo.

| | Qué cambió | Lo ya guardado |
|---|---|---|
| **`version` 1.0.0 → 1.1.0** | Nuestra transcripción: una errata, un campo que faltaba, una regla mal escrita | Sigue valiendo. El ensayo fue el mismo; lo que estaba mal era nuestra copia |
| **`id` nuevo** | La norma: otro año, otro criterio, otro ensayo | No es comparable, y debe seguir midiéndose contra la suya |

> **Dentro de un mismo `id`, los identificadores de campo solo se añaden — nunca se renombran.** Los datos se guardan por `bloque/campo/muestra`; renombrar un campo deja los datos viejos huérfanos **sin error y sin aviso**, en blanco, y el técnico no sabe por qué. Si hace falta renombrar, es que ya no es la misma toma de notas y toca `id` nuevo.

### Saber qué versión del programa se está usando

El mismo problema, un piso más arriba: con la aplicación copiada en varios ordenadores, lo que no puede pasar es que alguien trabaje meses con una versión antigua **sin saberlo**.

- **`Ayuda | Acerca de…`** enseña la versión del programa, las normas cargadas con su versión y de dónde salen. Es también lo que permite responder **con qué versión se registró cada ensayo**, parte de lo que pide la ISO 17025 sobre validación de software.
- Quien instala una versión nueva la **publica** desde ahí. Se escribe un `version.json` en la carpeta compartida.
- Los equipos que sigan con una anterior ven **una banda ámbar al arrancar**. Se puede quitar hasta el siguiente arranque.

**«Publicar» no reparte nada.** Es la duda que le surge a cualquiera al leer el botón, así que conviene dejarlo claro: no copia el programa, no instala y no actualiza a nadie. Lo único que hace es **anotar un número en la carpeta compartida** —«la versión buena es la 1.2.0, publicada tal día por tal persona»—, y los demás equipos comparan ese número con el suyo al arrancar. Instalar sigue siendo cosa aparte; esto solo evita que nadie se quede atrás sin enterarse. Junto al número se puede dejar una nota de qué cambia, que es lo que los demás leen en el aviso.

**Las normas se listan con su designación, una por línea** (DD‑105). Iban con el nombre corto y todas seguidas, y con los dos años de la 60598 instalados se leía «Luminarias v1.0.0 · Luminarias v1.0.0»: la ventana que existe para saber qué hay instalado era justo la que no lo decía. `RotulosTests` comprueba que **no haya dos iguales en la lista**.

**Es un aviso, no un candado.** Dejar sin trabajar a un laboratorio porque un fichero de OneDrive dice otra cosa sería peor que el problema que resuelve. Y ante cualquier duda —un número de versión que no se entiende— no avisa: es preferible callar que avisar en falso todos los días.

> **«Publicar» no actualiza a nadie, y el botón invita a creer que sí.** Lo único que hace es escribir un número en la carpeta compartida; los demás equipos lo leen al arrancar y enseñan el aviso, pero **siguen ejecutando su copia vieja hasta que alguien instale la nueva en cada máquina**. Es un detector de desactualización, no un repartidor. Lo que sí actualizará solo es **ClickOnce**, que está pendiente; cuando esté, esto se queda como red de seguridad para quien nunca cierra el programa —y por la trazabilidad de quién publicó qué y cuándo, que para la ISO 17025 no sobra.

> **Para que esto sirva hay que subir `<Version>` en `LumNotas.App.csproj` en cada entrega.** Si no se sube, los demás equipos no se enteran de nada. Está comentado en el propio fichero.

### Avisar de un fallo

`Ayuda | Reportar un problema…` enseña **el correo de quien mantiene el programa** y, debajo, los datos sin los cuales casi ningún fallo se puede reproducir: versión, equipo, usuario, Windows, fecha y la ruta del registro de errores. Todo copiable de una vez, y con un botón que abre el explorador **con `errores.log` ya seleccionado** para poder arrastrarlo al correo.

- **«Escribir el correo»** abre el programa de correo del equipo con destinatario, asunto —que ya lleva la versión— y un cuerpo con tres preguntas: qué hacías, qué esperabas y qué pasó.
- Si el equipo no tiene programa de correo asociado, lo dice y queda **«Copiar dirección»**, que es lo que hace falta para escribir desde el correo del navegador.

**El programa no envía nada por su cuenta** (DD‑82). Mandar el correo directamente obligaría a llevar una contraseña de servidor dentro del ejecutable, que es exactamente lo que se descartó al hablar de recuperar contraseñas (DD‑67). Quien tiene las credenciales es el equipo, no la aplicación.

La dirección está **escrita en el código**, no en la carpeta compartida: si dependiera de la configuración, un equipo mal configurado se quedaría sin saber a quién avisar justo cuando algo va mal.

### El proyecto y sus tomas de notas

Todo empezó por la toma de notas, y por eso la toma de notas está en el centro. **Ya no le corresponde.** Un proyecto tiene fechas, llegada de muestras, estado, importe y —en cuanto se pidan— ENAC, ENEC, CB o EMC; nada de eso es de una norma, y la toma de notas ha pasado a ser *una parte* del proyecto, no al revés. Con dos normas en el mismo servicio ya se ven las costuras: hay dos cabeceras y un solo cliente.

> **Cómo acabó.** El objeto proyecto se descartó (DD‑89) y esos datos viven en la toma de notas: la acreditación desde DD‑111 y los laboratorios de fuera desde DD‑110. Que sean del servicio y no de la norma sigue siendo cierto; lo que se decidió es que **el sitio donde constan es el `.lmnlab`**, porque es el único fichero que existe.

**Adónde va**: un proyecto con cabecera propia —cliente, código, técnicos, certificaciones— y **N tomas de notas colgando**, cada una con lo suyo de ensayo. En pantalla no sería una vista nueva: el árbol que ya existe gana un nivel (`Proyecto → norma → sección → apartado`) y desaparece el prefijo con el que hoy se distinguen las secciones de cada norma.

**La restricción que manda sobre todo lo demás** (DD‑83, precisada en DD‑85): el responsable da de alta un proyecto y **ya tiene tarjeta que planificar** sin rellenar un solo dato de ensayo — eso es cosa del técnico cuando empiece. De ahí dos reglas que ningún cambio puede romper:

- **Para dar de alta una toma de notas solo hacen falta el nombre, el técnico 1 y la norma.** No es que el formulario tenga que ser corto: es que **nada más puede bloquear**. El técnico 2 se puede dejar en blanco. La norma empezó siendo opcional y el laboratorio la hizo obligatoria (DD‑93): una toma de notas sin norma no tiene apartados que rellenar, y el nombre del fichero la lleva dentro, así que dejarla para después obligaba a renombrarlo.
- **Lo que falta se ve en rojo.** El rótulo del campo se pone rojo mientras esté vacío y vuelve al gris en cuanto se rellena — el mismo criterio que las casillas obligatorias de la toma de notas. Rojo mientras esté vacío, no rojo para siempre: así el rojo señala trabajo pendiente y no decora.
- **Nada de la cabecera del proyecto puede ser obligatorio.** Un proyecto a medias es el estado normal durante semanas, no un error. En particular, la cabecera del proyecto **no puede heredar los `obligatorio: true` de las plantillas**, o el camino rápido muere ahí.

Quién decide qué bloquea es `AltaDeProyecto`, en el núcleo y con tests; el diálogo solo lo pregunta. Lo que la norma exige para *ensayar* sigue estando donde estaba, en `RequisitosDelProyecto`, y **no tiene voz en el alta**: un proyecto recién creado tiene la cabecera casi entera por rellenar y eso es lo correcto.

**Cómo se está haciendo**, en tres pasos que se entregan por separado y no dejan el programa a medias:

| | Paso | Estado |
|---|---|---|
| 1 | La identidad del proyecto deja de pedirse por su clave en la cabecera y pasa a leerse de un solo sitio. **Sin cambiar el formato del fichero ni nada en pantalla** | **hecho** el 2026‑08‑02 |
| 2 | **Qué norma es la principal la dice el proyecto**, no se deduce | **hecho** el 2026‑08‑02 |
| 3 | **Diálogo «Nuevo proyecto»**: dar de alta sin pasar por la toma de notas | **hecho** el 2026‑08‑02 |
| 4 | El árbol gana un nivel: cabecera propia del proyecto y una rama por norma | pendiente |

Del **paso 3**: `Archivo | Nuevo proyecto…`, y también desde la mitad de gestión de la portada y desde la barra del tablero — que son los tres sitios donde está el responsable cuando se le ocurre. Se rellenan nombre y técnico, se elige carpeta y el `.lmnlab` queda en disco con su tarjeta ya en el calendario, **sin abrir su toma de notas**: quien da de alta un proyecto lo hace para planificarlo.

Antes, lo mismo eran cuatro pasos: elegir norma en la portada, escribir en la cabecera de la toma de notas, «Guardar como» y buscar carpeta. Se comprueba además que no se pise un proyecto que ya exista con ese nombre en esa carpeta — dos servicios del mismo cliente llamados igual es un descuido frecuente, y ahí se perdería el trabajo del otro.

#### Un proyecto, varias familias

Salió al llegar aquí, y es el motivo por el que en su día se decidió **no** tener objeto «proyecto»: en el laboratorio **un proyecto puede llevar cuatro familias de luminarias, cada una con su propia toma de notas**. Cada familia tiene sus muestras, su clase, su Ta y su grado IP; lo único que comparten es el cliente, el código, los técnicos y la oferta.

Los niveles reales son tres, no dos:

```
Proyecto        cliente, código, técnicos, fechas, importe, ENAC/EMC
└ Familia       una toma de notas: sus muestras, su clase, su Ta
   └ Norma      60598, y las que se le añadan
```

**Lo que hay hoy** —un `.lmnlab` por toma de notas— ya sirve para esto: cuatro familias son cuatro ficheros. Y tiene una virtud que no conviene perder: **cuatro técnicos pueden trabajar a la vez en cuatro familias del mismo proyecto**, porque son cuatro ficheros distintos. Meter las cuatro en uno reabriría el problema de los dos escritores que ya se resolvió con la planificación.

**Se resuelve enlazándolas** (DD‑90), no creando un fichero por encima. En el diálogo de planificación hay un campo **«Grupo»**: se escribe el mismo nombre en las tomas de notas del mismo trabajo y el calendario las enseña **en una sola barra**.

| | |
|---|---|
| **Dónde vive el enlace** | Dentro de cada `.lmnlab`, con su planificación. Sin fichero de grupo, sin índice — y **viaja con el fichero** si se mueve de carpeta |
| **Qué enseña la barra** | Una **tarjeta por familia**, pegadas unas a otras, cada una con su código, **su color y su consejo emergente** (DD‑119). El avance y el importe son los **del trabajo entero**: un servicio no está hecho porque lo esté la primera familia |
| **Cuánto abarca** | De la **primera** familia hasta donde acaba la **cadena** (DD‑121). No hasta la fecha más tardía que haya escrita: una familia anexada se planificó sin saber dónde iba a caer |
| **Cómo se pone en fila** | Al guardar cualquiera de ellas, `CadenaDelGrupo` **escribe** las fechas (DD‑123): la primera conserva su inicio y cada una de las siguientes empieza **al día siguiente** del fin de la anterior, conservando su duración. Sin fin, una semana; sin ninguna fecha, mañana |
| **Quién va delante** | Lo dicen **las fechas de inicio**. Para adelantar una familia se le pone una fecha anterior; con la misma fecha gana la que se acaba de tocar. Sin número de orden guardado y sin botones |
| **Cómo se dibuja** | Lo que pone en el fichero, tal cual: las tarjetas se tocan porque una acaba el día antes de que empiece la siguiente. Cuando las fechas todavía no están encadenadas —datos viejos, o la vista previa de un arrastre—, `TramosDelGrupo` las coloca respetando lo que dura cada una (DD‑121) |
| **Pulsar** | Abre la planificación de **esa** familia, no la de la cabecera |
| **Cómo se rotula la fila** | Con el **nombre del grupo tal como se tecleó**, seguido de **«(agrupación)»** — `ANTAR2504 (agrupación)`. No con el código de la cabecera: la fila ya no es una toma de notas, son todas, y cada una lleva el suyo escrito dentro. La coletilla hace falta porque el nombre del grupo lo pone una persona y puede parecerse a cualquier cosa: sin ella no se distingue de una toma de notas suelta que se llamara igual |
| **Arrastrar** | Cogiendo **cualquiera** de las tarjetas se mueve el trabajo entero, manteniendo las distancias; los **bordes de fuera** —izquierdo de la primera, derecho de la última— estiran y tocan solo esa familia. Los bordes **de dentro** todavía no mueven la frontera: se comportan como el centro |
| **El tablero** | No cambia: una columna por familia, que es la unidad de trabajo del técnico |

El nombre del grupo se compara **sin mayúsculas, espacios ni guiones**, igual que los códigos de servicio: se teclea a mano en cada una de las cuatro y una mayúscula no puede desenlazarlas.

**Por qué así y no con un fichero de grupo**: mismo motivo que no hay índice de proyectos (DD‑27) y que no hay objeto proyecto (DD‑89). Un fichero que dice quién va con quién es una segunda verdad que se desincroniza; el enlace dentro de cada toma de notas se lee abriéndola y no puede quedarse huérfano.

> **El importe sigue yendo en una sola.** El grupo enseña la **suma** de lo que lleven sus miembros: con la cabecera llevando el único importe, coincide con el de la oferta. Si en la barra aparece **el cuádruple de lo que costó el trabajo**, es que se ha repetido en las cuatro — y ese error, que antes era invisible y disparaba la carga del técnico, ahora se ve.

#### Cómo se llama el fichero

El nombre lo fija el laboratorio (DD‑91) y se lee de un vistazo en el explorador, sin abrir nada:

```
TdN_60598_TECNO260201-00.lmnlab
└─┬─┘ └─┬─┘ └──────┬──────┘
  │     │          └──────── código de la toma de notas
  │     └─────────────────── la norma principal
  └───────────────────────── es una toma de notas
```

**El código entra tal cual** (DD‑104): es el que se teclea en la cabecera, y ya lleva dentro el número de familia y la edición.

```
TECNO260201-00
└───┬───┘└┬┘└┬┘
    │     │  └── edición del documento
    │     └───── nº de familia dentro del servicio
    └─────────── código de servicio (las 9 primeras)
```

**Las dos últimas parejas las decide el laboratorio, no el programa**: numerar familias y decidir que algo se reedita son decisiones suyas, y un programa que las tomara solo acabaría renumerando un registro ya firmado. Lo que sí hace el programa es **dejar de estorbar**: antes pegaba un `xx-00` de relleno que el técnico tenía que sustituir **renombrando el fichero**, y ahora se escribe una vez en la cabecera y el nombre sale ya correcto.

Lo componen los dos caminos que crean fichero —el alta rápida y el «Guardar como» de la toma de notas— desde el mismo sitio, para que no puedan divergir. Sin norma elegida queda `TdN_TECNO260201-00` y sin código `TdN_60598`; el `TdN_` no se pierde nunca, porque es lo que dice qué es el fichero.

#### Cómo se identifica una toma de notas abierta

Dos rótulos, y cada uno responde a una pregunta distinta (DD‑105):

| | Dice | Ejemplo |
|---|---|---|
| **La lengüeta de la pestaña** | En cuál de las tomas de notas abiertas estoy | `TECNO260201-00 •` |
| **El título, arriba** | Contra qué norma estoy anotando | `EN IEC 60598-1:2024 + A11:2024 \| TECNO260201-00 •` |

El punto final es la marca de **cambios sin guardar**, y por eso el separador es `|` y nunca un punto: se leía `ALVEI2306 • · Luminarias`, que parecen dos separadores seguidos. Sin código puesto, la pestaña dice **«Sin código»** y el título **«sin código»**.

**La norma va entera y con su año, no como «Luminarias».** El laboratorio tiene dos años de la 60598 instalados a la vez; con el nombre corto, anotar contra el año que no era no se vería hasta emitir el informe. Sale de `meta.designacion`, que **declara cada plantilla**: recortarla del título con una regla de C# sobre dónde está el guion sería inventarse la designación de una norma. Si una plantilla no la trae, se usa su título — añadir una norma no puede exigir rellenar todos los campos nuevos.

El título es también el de la **ventana de Windows**, la del alt‑tab. Los dos dicen lo mismo a propósito: norma y código son exactamente lo que ya lleva dentro el nombre del fichero, así que la barra de tareas no pierde nada y se lee mejor.

Las dos cadenas viven en `RotulosDeTomaDeNotas`, en el núcleo, **para que tengan tests**: ninguna de las pruebas toca la interfaz, así que lo que se quede en el ViewModel no se comprueba nunca.

#### Los dos códigos

La cabecera pide **dos** y no es redundancia (DD‑104):

| | Qué identifica | Ejemplo | Quién lo pone |
|---|---|---|---|
| **Código de la toma de notas** | Este documento — una familia del trabajo | `TECNO260201-00` | Se teclea. Es el único obligatorio de verdad |
| **Código de servicio** | El trabajo entero, que puede tener cuatro familias | `TECNO2602` | Se rellena solo con las 9 primeras del de arriba |

**El de servicio se puede corregir a mano y no se vuelve a pisar.** Hay servicios cuyo código no son las nueve primeras; si el programa insistiera, el técnico lo arreglaría y se lo desharía en la siguiente pulsación. La regla está en `CodigoDeServicio.Sugerir`, en el núcleo y con tests: se rellena solo mientras lo que haya sea exactamente lo que dedujo el programa.

De cada uno cuelga algo distinto, y por eso hacen falta los dos: el **nombre del fichero** y el **aviso de duplicados** van por el de la toma de notas; los **identificadores de muestra** (`EBP_SAFETECNO260201`) y la agrupación del calendario, por el de servicio.

> **El aviso de duplicados cambió de criterio con esto.** Comparaba códigos de servicio, y con cuatro familias por trabajo habría saltado en la segunda, la tercera y la cuarta —lo normal, no un descuido— hasta que nadie lo leyera. Ahora compara el de la toma de notas, que es lo que de verdad no debe repetirse. Los ficheros anteriores a este campo no lo traen y se siguen comparando por el de servicio: es lo único que tienen, y dejarlos fuera sería perder la red de seguridad justo en los más antiguos.

### BBDD: encontrar un servicio de hace meses

La cuarta vista (DD‑109). Nació de algo que hoy se hace de viva voz: *«¿te acuerdas de aquel proyecto de Antares con IP65?»*.

| | |
|---|---|
| **Columnas** | Código de la toma de notas · acreditación · técnico 1 · técnico 2 · norma · nº de muestras · IP · IK · estado · laboratorio externo |
| **Filtros propios** | Caja de búsqueda, IP, IK y acreditación |
| **Qué enseña** | **Todo**, terminados y archivados incluidos |

**Solo lee.** No hay fichero que mantener: sale del mismo escaneo que alimenta el tablero, el calendario y la carga. Pulsar el código abre esa toma de notas en una pestaña, y ahí se edita — aquí no.

**Ignora el filtro compartido, y es a propósito.** Los otros tres arrancan en «En desarrollo», que deja fuera lo archivado; pero lo que se busca en la BBDD suele ser viejo y bastantes de esos servicios están apartados. Con el filtro puesto, la vista nacería escondiendo justo lo que se viene a buscar.

**La caja busca en todas las columnas** —código, técnicos, norma, acreditación, laboratorio externo— porque quien recuerda un proyecto no sabe por cuál lo recuerda. Y los desplegables ofrecen **los valores que de verdad hay** en los proyectos leídos: una lista fija ofrecería grados que nadie ha ensayado y, peor, escondería los que sí.

> **El IP y el IK son por muestra; en el listado se enseña el mayor.** Y «mayor» aquí es un criterio del laboratorio, no un orden físico: **manda la segunda cifra** —la del agua— y la primera solo desempata, así que `IP28` es mayor que `IP54`. Proteger del polvo y proteger del agua no se comparan, pero en una columna hay que poner un valor; por eso la regla vive en `GradosDelServicio`, en un solo sitio y con tests. La «X» cuenta como 0 y **«Luminaria ordinaria» es IP20**. En el IK, «No IK» no es un grado bajo: es no haber ensayo, y deja la celda vacía.

> **Al añadir un campo al resumen hay que subir `CacheDeResumenes.Formato`.** La caché guarda resúmenes entre sesiones; sin subirlo, los guardados con la forma anterior se seguirían dando por buenos y el listado enseñaría huecos en blanco durante días —hasta que cada fichero se tocara por su cuenta— sin que nada lo explicara.

#### El mismo servicio, dos ficheros

Dar de alta los proyectos por un lado y tomar notas por otro trajo un problema que **no es de software**: un técnico puede ponerse a tomar notas **sin saber que su proyecto ya estaba creado**. Acabarían con el mismo servicio partido en dos ficheros —uno con los datos y otro con las fechas— y ninguno completo.

**Al guardar un proyecto nuevo se mira si ese servicio ya existe** en la carpeta del laboratorio (DD‑86). Si lo hay, se enseña cuál, de quién es y dónde está, con tres salidas:

| | |
|---|---|
| **Abrir el que ya existe** | lo habitual. Se abre en otra pestaña; la del técnico se queda intacta |
| **Crear otro igualmente** | a sabiendas: un reensayo o un servicio partido pueden repetir código |
| Cancelar | |

Los códigos se comparan **sin mayúsculas, espacios ni guiones**: «ANTAR2504», «antar 2504» y «ANTAR‑2504» son el mismo servicio para el laboratorio, y compararlos en crudo dejaría pasar el caso más probable — el técnico escribiéndolo a su manera. Un código en blanco no choca con nada, y un fichero ilegible tampoco: su código no es fiable.

Se comprueba **releyendo el disco**, no mirando el último escaneo: el proyecto con el que se choca puede haberlo dado de alta el responsable hace cinco minutos desde otro equipo, que es justo el caso que hay que cazar. Con la caché cuesta una décima de segundo, y solo se hace al guardar un proyecto **nuevo** — una vez en la vida de cada uno. La misma comprobación se hace al **dar de alta**: repetir un alta es igual de fácil que empezar dos veces.

> **Esto es una red, no la solución.** Atrapa el error venga de donde venga, pero no evita que ocurra. Lo que lo evita es que la portada deje de ofrecerle al técnico «empieza un servicio nuevo» como primera acción y le enseñe **sus** proyectos — y para eso el programa tiene que saber quién es, que es el perfil de usuario pendiente desde DD‑08.

> **Lo que queda del paso 3 se ha movido al 4 a propósito.** Estaba previsto dejar el código y los técnicos en *solo lectura* dentro de la toma de notas, y hacerlo ahora los dejaría sin ningún sitio donde corregirlos: la cabecera de la norma es hoy el único. Primero el panel del proyecto, y entonces sí.

Del **paso 1**: `codigoServicio` y `numeroMuestras` **ya estaban** promovidos a datos del proyecto desde el principio; los técnicos no. Ahora `DatosProyecto.Tecnico1` y `Tecnico2` son el único sitio que sabe dónde vive ese dato — antes la clave estaba escrita en seis puntos de cuatro proyectos distintos. Se sigue guardando donde siempre, así que **los proyectos ya escritos se leen igual** y el informe sigue enseñando el técnico; hay tests que lo fijan en los dos sentidos.

Del **paso 2** (DD‑84): la norma principal se apunta al elegirla —`PlantillaEnsayos.AplicarA(principal: true)`— y se guarda en el `.lmnlab`. Antes se reconstruía del **patrón con el que se nombran las muestras**, que es una *consecuencia* de haberla elegido y no la elección; con dos normas del mismo patrón —IP 60529 y módulos LED 62031 nombran las dos `EBP_SAFE…`— la pregunta se quedaba sin respuesta y acababa decidiéndola el orden alfabético.

> **Un fallo latente que salió al hacerlo.** Al abrir un proyecto, la norma con la que se cargaba era *la primera de las suyas que estuviera instalada*, leída de un `HashSet` cuyo orden no es de fiar. En un servicio de dos normas podía abrirse por la añadida, y entonces **guardar reescribía el patrón de muestras** —`EBP_SAFE` por `EBP_CLIM`— y con él el identificador de todas las muestras del servicio. Ahora manda la principal que apunta el proyecto, y abrir y guardar ya no puede cambiarla.

Los ficheros anteriores no traen el campo: se quedan sin principal apuntada y se sigue deduciendo, como siempre. **No hay migración**, ni la habrá para el paso 3: cada fichero se cura cuando se guarde. Nadie lanza una conversión masiva sobre OneDrive.

> Cuando llegue el momento de guardar la cabecera del proyecto, vale la misma regla que con la planificación: **un solo camino la escribe**, y guardar desde la toma de notas la conserva releyéndola del disco. Dos escritores es el problema que ya se resolvió una vez.

### Los técnicos del laboratorio

Hasta el 2026‑08‑06 el técnico se escribía a mano, y eso producía **la misma persona con tres grafías distintas** —«D. Martínez», «Daniel Martinez», «daniel martínez»—, lo que rompe el filtro del calendario y cualquier recuento por técnico. Ahora **Técnico 1 y Técnico 2 se eligen de una lista**, que arranca sin nadie y es obligatoria en el caso de Técnico 1.

**Técnico 1 es el responsable del proyecto**: es el que sale en el tablero y por el que filtra el calendario.

La lista se edita en **«Configuración | Técnicos…»**. Se llamó *Configuración* y no *Admin* porque no hay roles ni permisos que administrar —cualquiera que abra el programa puede editarla— y porque ahí caben luego los catálogos de equipos y el perfil de usuario.

| Dónde vive | `tecnicos.json` en la **carpeta de proyectos** (la compartida). Mientras no esté elegida, en la carpeta de plantillas |
|---|---|
| Por qué ahí | Añadir un técnico se hace una vez y lo ve todo el laboratorio, en vez de repetirlo en cada equipo |
| Lista de partida | **Ninguno** (DD‑132). Solo viene `(sin técnico)`, el cajón de lo que está sin repartir, para que el desplegable no salga en blanco. Antes venían seis nombres cableados: personas de un laboratorio concreto metidas en el ejecutable |
| Por qué el cajón lleva ese nombre y no «Sin técnico» | Es **el mismo texto** con el que agrupan el calendario, la carga y los filtros. Con dos nombres parecidos, lo elegido a mano y lo que no tiene técnico saldrían en dos filas distintas queriendo decir lo mismo |
| Qué dice el diálogo | **Dónde va a parar la lista, y son tres casos**: la compartida, el respaldo en la de proyectos, o solo este equipo. Decía «se guarda en la carpeta de proyectos» —falso desde que las dos carpetas se separaron— y mandaba a elegirla a «Gestión de proyectos», que tampoco es donde está. Es el texto que lee quien mantiene la lista para saber si lo que escribe lo verá alguien más, así que equivocarse ahí es peor que callar. Corregido el 2026‑08‑06; la ruta real va en el consejo emergente |

**Las dos operaciones destructivas no son simétricas** (D‑23, decisión del laboratorio):

- **Quitar** a un técnico **no toca ningún proyecto**. El ensayo lo hizo esa persona aunque ya no esté en la lista.
- **Corregir** su nombre **sí se propaga** a los proyectos que lo lleven. Una errata no es una persona distinta, y si no se propagase, el filtro por técnico dejaría de encontrar sus servicios.

Un nombre guardado que no esté en la lista —los proyectos anteriores a todo esto— **se sigue ofreciendo en su desplegable**, para que ese proyecto no se quede sin técnico.

> **Tercera trampa de WPF, esta cara: la aplicación se cerraba de golpe al elegir un técnico.**
> La lista de un desplegable **no se puede reconstruir desde el `set` de su propia selección**. Aquí `Tecnicos` era una propiedad calculada que devolvía una lista nueva y el `set` de Técnico 1 la notificaba: el `ComboBox` recibía un `ItemsSource` distinto, volvía a resolver su selección, la escribía otra vez, y vuelta a empezar. `StackOverflowException`, **que no se puede capturar**, así que la aplicación desaparecía sin dejar ni registro de error.
> La cura es que la colección **no se sustituya nunca**: es una `ObservableCollection` que solo se ajusta por dentro, y los `set` comparan antes de escribir. Al no reproducirse desde código —hace falta el clic real— la confirmación salió del **registro de eventos de Windows**, que sí anotó el `StackOverflowException`.

### La carga por técnico (tercera vista)

Pedida el 2026‑08‑06. El tablero dice *qué falta*, el calendario *cuándo*, y esta *si cabe*. Tabla de **técnicos × meses** con el porcentaje de ocupación, en `Gestión de proyectos | Carga`.

**Cómo se calcula**, con la regla del laboratorio:

1. **Trabajo de un servicio, en horas: importe ÷ 105 × 1,3.** Es la cuenta con la que el laboratorio estima el trabajo, y sale a **80,77 € por hora** — cuadra con la tarifa de 80 €/h que factura. Una oferta de 2 000 € son unas **25 horas**.
2. **De horas a jornadas**, dividiendo entre las **8 horas** que tiene un día. Hace falta porque la capacidad del mes se declara en días. Esas 25 horas son algo más de **tres jornadas**.
3. **Reparto entre meses** en proporción a los **días entre semana** que el servicio tiene en cada uno. Los fines de semana no cuentan.
4. **Comparación con la capacidad del mes**, que no es igual todo el año.

| Mes | Días de trabajo que caben |
|---|---|
| Agosto | **10** (dos semanas laborables) |
| Diciembre | **15** (tres semanas) |
| Los demás | **22** |

**Los tres números de la cuenta se editan por separado** —divisor, factor y horas por jornada— y no reducidos a uno solo: así el diálogo enseña la misma fórmula que usa el laboratorio y se reconoce. Debajo se ve en qué se traduce (*«Sale a 80,77 € por hora»*), que es la comprobación de que sigue cuadrando con la tarifa. Se editan en **`Configuración | Capacidad y tarifa…`**, con los doce valores del calendario, y se guardan junto a la lista de técnicos en la carpeta compartida.

> **La regla anterior era ÷ 80 € = 1 **día**, y estaba mal** (DD‑96). El laboratorio lo corrigió el 2026‑08‑03: son **80 €/hora**, no por día. La diferencia no es de matiz — un servicio de 2 000 € pasa de 25 días a poco más de 3, y **toda la tabla de carga baja unas ocho veces**. Un `capacidad.json` escrito con la regla vieja no trae los campos nuevos y se rellena con los de partida, así que no deja la carga a cero.

Verde por debajo del 85 %, ámbar hasta el 100 %, **rojo por encima**: ahí el técnico está sobrevendido. Un servicio de 10 000 € planificado entero en agosto sale al **155 %**, que es exactamente el aviso que se buscaba — medio laboratorio está de vacaciones.

**Lo terminado no cuenta** (DD‑142). Es la única de las cuatro vistas que esconde algo por su cuenta, y tiene motivo: las otras tres contestan «qué hay» y esta contesta «¿cabe?». Un mes cerrado sale en blanco, y el panel de abajo lo dice para que nadie lo lea como un fallo.

**Terminado y archivado no se esconden igual, y el panel lo explica** (reescrito por el laboratorio el 2026‑08‑07). Es una asimetría real y confunde si no se dice:

| | Cómo se queda fuera | ¿Se puede ver su carga? |
|---|---|---|
| **Terminado** | en el **cálculo** | **nunca**, ni eligiendo el filtro |
| **Archivado** | en el **filtro** | sí: eligiendo «Archivados» o «Cualquier estado» |

El texto perdió a cambio la leyenda de colores —«verde por debajo del 85 %, ámbar hasta el 100 %, rojo por encima»— y el recordatorio de que agosto no da lo mismo que marzo. Se quitó a sabiendas: el umbral del 85 % no se adivina, pero el panel no daba para todo y el laboratorio prefirió explicar lo que confunde antes que lo que se intuye.

**Dos cosas que conviene tener presentes:**

- **El reparto supone esfuerzo uniforme**, y en un laboratorio no lo es: montaje, dos días en cámara sin tocar nada, medida. En un servicio suelto el reparto mensual es impreciso; sobre el conjunto de un técnico los errores se compensan y sirve para planificar, que es para lo que se usa. La alternativa —teclear días por mes a mano— no la rellenaría nadie.
- **Un servicio sin importe no cuenta**, y se avisa con «N sin importe» junto al técnico, en vez de rebajar su carga en silencio.

El importe es **dato comercial, no de ensayo**: se guarda con la planificación y **no aparece en el informe** que se firma. Aviso dado al laboratorio: el `.lmnlab` es texto plano en la carpeta compartida, así que quien tenga acceso a la carpeta ve los importes.

### El calendario (línea de tiempo)

Pedido el 2026‑08‑06 con Planyway como referencia. Contesta *cuándo toca cada servicio y qué se ha pasado de plazo*. Es una de las **tres vistas de la misma carpeta**, que se eligen con los botones «Tablero», «Calendario» y «Carga».

**Una tarjeta por toma de notas** (DD‑54). Un servicio con 60598‑1 + ‑2‑3 + IK + 62031 sale como una sola barra, porque todo cuelga de la toma de notas principal — ahí las normas se suman, no se reparten.

**Y un tren de tarjetas por trabajo enlazado**, una por familia y pegadas unas a otras (DD‑118, DD‑119). No confundir las dos cosas: **varias normas dentro de una toma de notas** dan una barra entera; **varias tomas de notas del mismo trabajo** dan varias tarjetas seguidas.

| Dato | Dónde vive |
|---|---|
| Inicio y fin previstos | `planificacion.inicio` / `.fin` del `.lmnlab` |
| Estado | `planificacion.estado`: `porHacer`, `enCurso`, `pendienteCliente`, `terminado` (DD‑51) |
| Recepción de muestras | `planificacion.recepcionMuestras`, **fecha** y no sí/no (DD‑50) |
| Archivado | `planificacion.archivado` (DD‑52) |

**El eje va en semanas ISO** (DD‑55), porque es como planifica el laboratorio. `EjeDeSemanas`, en `LumNotas.Core/Gestion/Planificacion.cs`, calcula las celdas, los meses de la cabecera y la posición en píxeles de cada barra. Está en el núcleo y no en la interfaz para poder probarlo: los años de 53 semanas y las semanas que cruzan de diciembre a enero son justo lo que se rompe en silencio. No hace falta importar los años de ningún sitio — `DateTime` e `ISOWeek` ya los saben.

#### La columna de la izquierda es de técnicos, no de servicios

Cambiado el 2026‑08‑06 a petición del laboratorio. El código del servicio **ya va escrito dentro de su barra**, así que repetirlo a la izquierda no aportaba nada. Lo que le falta al responsable es lo contrario: **cuántos servicios lleva cada técnico y cuánto tiempo le ocupan**.

Por defecto los servicios se **agrupan por Técnico 1**, el responsable. Cada grupo abre con una cabecera —nombre en azul, «3 proyectos» al lado y «N fuera de plazo» en rojo si los hay— y bajo ella van sus carriles. Se puede desagrupar con la casilla «Agrupar por técnico». Los servicios sin responsable van al final, bajo «(sin técnico)», que es donde se ve lo que falta por asignar.

**La ocupación cuenta días, no suma duraciones** (`Ocupacion.Dias`). Dos servicios que se solapan no ocupan el doble: el técnico está ocupado una vez. Sumar duraciones exageraría la carga justo de quien lleva varios a la vez, que es a quien se busca. Los tramos pegados —uno acaba el lunes, otro empieza el martes— cuentan como uno solo.

Las dos columnas —cabeceras y barras— **recorren la misma lista de filas** con las mismas alturas: por eso van alineadas y no hay que sincronizar nada. **La cabecera del eje reserva ese mismo ancho**, atado a la misma propiedad: si reservara un hueco que el cuerpo no gasta, las semanas dejarían de caer sobre sus barras.

#### Una fila ya no es un trabajo, es un carril

Cambiado el 2026‑08‑06. Antes cada trabajo se llevaba **su propia fila**, así que un técnico con veinte proyectos daba veinte renglones aunque fueran uno detrás de otro y no coincidieran nunca. El calendario se leía **bajando**, cuando lo que se quiere leer es el tiempo, que va en horizontal.

Ahora los trabajos comparten fila mientras no se pisen. Se recorren por fecha de inicio y **cada uno cae en el primer carril donde quepa**, el más alto disponible; solo se abre carril nuevo cuando ya no cabe en ninguno. Tomando siempre el más alto no hace falta probar combinaciones: salen tantos carriles como trabajos coincidan **el día más cargado**, que es el mínimo posible.

Veinte proyectos seguidos pasan de veinte filas a **una**. Y lo que se cuenta hacia abajo deja de ser cuántos proyectos hay y pasa a ser **cuántos coinciden a la vez**, que es lo que el responsable necesita ver.

| Regla | Por qué |
|---|---|
| **Compartir un solo día ya es pisarse** | Dos barras pegadas sin hueco se leen como una sola barra larga, y el calendario mentiría sobre cuándo acaba cada una. Con un día de por medio sí comparten carril |
| La hora se descarta | Las fechas del calendario son días; una guardada con hora mandaba a un carril nuevo un trabajo que empieza cuando el anterior ya ha terminado |
| Un fin anterior al inicio vale por un solo día | Es un dato mal guardado, y dejar el carril ocupado hacia atrás descolocaría a todos los demás |
| **Los carriles son por técnico**, no del calendario entero | Si se repartieran todos juntos, dos técnicos que trabajan las mismas semanas compartirían fila y la cabecera dejaría de decir de quién es lo que hay debajo |

El reparto está en `CarrilesDelCalendario`, en el núcleo, para poder probarlo sin ratón. **Los carriles se recolocan al soltar una barra, nunca durante el arrastre**: rehacerlos a media faena destruiría la tarjeta que tiene cogida el ratón y WPF daría el gesto por perdido.

Dos cosas que arrastra el cambio:

- **La columna de la izquierda se queda sin nombres.** Una fila son varios trabajos, así que no hay *un* nombre que poner. No se pierde nada —el código va escrito dentro de cada barra— pero **sin agrupar por técnico esa columna no tiene nada que decir y se encoge a cero**, que si no serían 230 píxeles de calendario tirados.
- **Abrir la toma de notas se pasa al botón derecho.** Vivía en el nombre de la izquierda, que ha desaparecido. Ahora es un menú contextual sobre la barra, y abre **la familia que se pulsa**, no la cabecera del grupo. El botón izquierdo se queda para lo de siempre: arrastrar, y con un clic abrir la planificación.

Lo que se ve de un vistazo:

- **línea roja vertical de «hoy»** y la semana en curso resaltada en la cabecera;
- **la barra lleva el color de su estado, y solo el de su estado** (DD‑141). Estuvo pintándose de rojo cuando la fecha de fin ya había pasado y el servicio no estaba terminado, y el laboratorio lo quitó el 2026‑08‑07: el rojo **tapaba el estado**, que es lo único que la barra dice con el color, y un servicio en curso y otro pendiente de cliente se veían iguales por haberse pasado un día. Fuera de plazo se sigue diciendo donde no estorba — en el consejo emergente de la tarjeta y en el **«N fuera de plazo»** de la cabecera del técnico;
- **icono de caja** en la barra si las muestras ya están en el laboratorio, y también en la banda de abajo —muestras aquí y todavía sin planificar es lo que corre prisa—. Va dibujado como trazo, no como imagen, para verse nítido a cualquier tamaño y tomar el color de donde se ponga;
- los servicios **sin fechas** salen en una banda aparte, con un botón «Planificar», para que no se pierdan de vista.

**Las barras se arrastran con el ratón** (2026‑08‑06): el centro mueve el servicio entero conservando la duración, el borde izquierdo cambia solo el inicio y el derecho solo el fin. Cuatro detalles que no son evidentes:

- **Se ajusta a días enteros.** A zoom mínimo una semana son 26 px, o sea **menos de cuatro píxeles por día**; sin ajuste no se acierta.
- **El gesto se calcula desde el punto de partida, no acumulando.** Ir y volver deja la barra exactamente donde estaba, y si no ha cambiado nada **no se escribe el fichero**.
- **Los bordes topan el uno con el otro**: un fin anterior al inicio no existe.
- **La barra no sale del calendario dibujado.** Se podía, y quedaba flotando en blanco, sin semanas debajo y sin saber en qué fecha se estaba soltando. Para llevar un servicio más lejos se pide sitio con «▶» y luego se arrastra.
- **Clic y arrastre se distinguen por 4 píxeles de recorrido.** Por debajo es un clic y abre el diálogo; por encima es arrastre y el clic se anula. Sin ese margen, cada intento de abrir el diálogo movería el servicio un día.

#### Varios años, sin fecha de caducidad

Consultado el 2026‑08‑06 por si el programa «se quedaba obsoleto» al llegar 2027. **No hay ningún calendario almacenado**: el eje se calcula a partir de las fechas de los proyectos, y `DateTime` y las semanas ISO funcionan igual en 2027, en 2040 o en 2110. Un servicio planificado en 2027 se dibuja solo, sin tocar nada.

Lo que sí hay son tres reglas para que eso no se pague en velocidad:

| Regla | Por qué |
|---|---|
| El eje encuadra **lo que hay**: dos semanas de margen por delante y **medio año por detrás** del último trabajo (DD‑122) | Sin esa cola, arrastrar un trabajo hasta el borde lo dejaba sin calendario debajo donde soltarlo, y el año siguiente ni se dibujaba. Hacia atrás no hace falta: no se planifica en el pasado |
| Lo encuadran las fechas del **trabajo entero**, no las de su cabecera | Un grupo de cuatro familias estiraba el calendario solo lo que ocupaba la primera |
| Botones **◀ Hoy ▶**, que añaden u ocultan 8 semanas vacías | Así se llega a cualquier año para planificar allí, sin dibujarlos todos de golpe. Siguen haciendo falta para ir más allá de esos seis meses |
| **Horizonte de ±5 años** alrededor de hoy, y tope de `MaximoSemanas = 520` | Un año tecleado mal (3026 en vez de 2026) generaría **cien mil semanas** y colgaría la aplicación |

El horizonte **se mueve con la fecha de hoy**, así que no caduca. Un proyecto cuyas fechas caigan fuera **no encuadra el calendario y no se pierde**: baja a la banda «Sin fechas o fuera del periodo», que es justo donde se ve que hay una errata que corregir.

Coste real: el caso habitual son 30–60 semanas; el peor caso posible, 520 celdas de cabecera más 520 líneas de rejilla, que WPF dibuja sin despeinarse.

Al soltar, el eje **se conserva mientras las fechas nuevas sigan cabiendo**, para que el calendario no se desplace bajo el ratón. Con medio año de cola por detrás (DD‑122) eso es lo normal: arrastrar hacia adelante ya casi nunca lo reencuadra.

El reparto de responsabilidades: `BarraDePlanificacion` y `ArrastreDeFechas` (núcleo, con tests) llevan la aritmética y el estado del gesto; `ArrastreDeBarra` (interfaz) solo traduce eventos de ratón a llamadas y decide las zonas de los bordes.

Al llegar al borde de la vista, **el calendario se desplaza solo** mientras se arrastra. Hace falta un temporizador y no basta con el movimiento del ratón: al topar con el borde el técnico se queda quieto esperando que avance, y sin latido no avanzaría nunca.

> **Dos trampas de WPF que ya mordieron.**
>
> 1. `ReleaseMouseCapture()` levanta `LostMouseCapture` **en el acto**, no al final del método. El manejador de ese evento es el que cancela un arrastre interrumpido, así que si se suelta la captura antes de borrar el estado del gesto, la barra se cancela sola justo antes de guardarse y vuelve a su sitio. En `ArrastreDeBarra.Terminar` **el estado se borra primero y la captura se suelta después**.
> 2. **Cuando un elemento pide más ancho del que le dan, WPF le aplica un recorte de maquetación.** La cabecera del calendario era un panel movido con `TranslateTransform`: con la ventana maximizada cabía entera y funcionaba, pero con la ventana pequeña las semanas que no cabían **no llegaban a dibujarse** y la cabecera se quedaba en blanco al desplazarse. La cabecera va ahora dentro de su propio `ScrollViewer` —que mide el contenido sin límite— atado al de las barras con `ScrollSincronizado`. **Un fallo de maquetación que solo aparece al encoger la ventana casi siempre es este recorte.**

**Pulsar la barra abre su configuración**; pulsar el código de la izquierda abre la toma de notas en una pestaña. Desde el diálogo, **«Quitar fechas»** devuelve el servicio a la banda de pendientes de planificar sin perder su estado ni la recepción de muestras. No se admite guardar con una sola fecha: o las dos, o ninguna, porque con una el servicio no se puede dibujar y se quedaría en un limbo entre planificado y pendiente.

> **Cuarta trampa de WPF.** El clic sobre la barra dejó de abrir el diálogo al añadir el arrastre: `ArrastreDeBarra` suelta la captura del ratón en el `PreviewMouseLeftButtonUp`, y al perder la captura WPF **da el `Click` del botón por cancelado**. La solución no es devolver el evento sino asumirlo: el comportamiento decide si el gesto fue clic o arrastre y ejecuta lo que toque. Filtros: técnico, estado, norma y «ver archivados»; el técnico y la norma se rellenan con lo que haya en los proyectos, no con una lista fija.

> **Octava trampa, hermana de la primera: si la lista se rehace, el arrastre se pierde.** Al pasar de una barra a un tren de tarjetas (DD‑119), lo que se arrastra ya no es un elemento fijo de la plantilla sino **un elemento de un `ItemsControl`**. Si al mover la barra se notifica la colección entera, WPF **destruye y vuelve a crear** las tarjetas, se lleva por delante la que tiene cogido el ratón, salta `LostMouseCapture` y el trabajo vuelve solo a su sitio a mitad del gesto. Por eso la lista se construye **una vez** y durante el arrastre se les **recalcula el tramo a los objetos que ya existen**. Regla: **lo que se arrastra no se puede recrear mientras dura el gesto.**
>
> Con ella va otra: **el panel de las tarjetas se mueve con ellas**, así que ya no vale de referencia para medir el recorrido del ratón —daría un desplazamiento que se persigue a sí mismo—. La fila se marca con `ArrastreDeBarra.Carril` y es contra ella contra la que se mide; la fila sí se desplaza con el calendario, que es justo lo que el arrastre necesita para seguir al ratón cuando llega al borde.

**La barra: una fila y nada más** (DD‑108). `Tablero · Calendario · Carga · + · Elegir carpeta… · Actualizar · Filtros`. El **«+»** es el alta rápida, con el verde de gestión y el rótulo entero en el consejo emergente. Los tres filtros —estado, técnico y norma— viven dentro de **«Filtros»**, que abre un diálogo donde además cabe explicar cada uno, cosa que en la barra no cabía. Siguen aplicándose **al elegirlos**, no al cerrar: son los mismos de siempre contra el mismo modelo, y meter un «Aceptar» sería cambiar cómo se comportan por haberlos movido de sitio.

> **Un filtro escondido y mudo es peor que un filtro visible.** Al meterlos en el diálogo desaparecía de la barra la única pista de que el tablero no lo enseña todo. Por eso el botón dice **«Filtros (2)»** y se pinta de verde cuando alguno aparta trabajo, y su consejo emergente resume qué se está viendo. Lo decide `ResumenDeFiltros`, en el núcleo y con tests, y ahí está la regla fina: **«En desarrollo» no cuenta como filtro activo** —es lo que hay puesto al abrir, y contarlo dejaría el aviso encendido siempre hasta que nadie volviera a mirarlo—, pero **sí se nombra en el resumen**, porque tampoco lo enseña todo: deja fuera lo archivado.

> **Séptima trampa de WPF: `DataContext` y `Visibility` en el mismo elemento se estorban.** El `DataContext` se aplica **al propio elemento**, no solo a sus hijos, así que también manda sobre sus ataduras. Poniendo `DataContext="{Binding Bbdd}"` y `Visibility="{Binding VistaBbdd}"` en el mismo `Grid`, la visibilidad pasa a buscar «VistaBbdd» **dentro de `BbddViewModel`**, donde no existe: la atadura falla **sin decir nada**, la visibilidad se queda en su valor de por defecto —visible— y la vista aparece encima de las otras tres. La solución es la que ya usaban el calendario y la carga: **dos `Grid` anidados**, el de fuera decide si se ve y el de dentro cambia el contexto.
>
> Es de la misma familia que la segunda y la sexta: **WPF no avisa de las ataduras rotas**. Cuando algo aparece donde no debe o no aparece donde debe, lo primero que hay que mirar es contra qué `DataContext` se está resolviendo.

> **Sexta trampa de WPF, y la que más veces ha picado: un `StackPanel` horizontal no envuelve jamás.** La barra de gestión —vistas, botones, tres filtros— era uno, y al estrechar la ventana **lo que no cabía dejaba de dibujarse**: desaparecían «Actualizar» y los filtros de estado, técnico y norma, sin error y sin barra de desplazamiento. Un `WrapPanel` lo arregla y encima se lee mejor. **Cualquier fila de mandos que pueda quedarse sin sitio va en `WrapPanel`, no en `StackPanel`.** Es la misma familia que la segunda trampa: WPF no avisa de que algo no cupo.
>
> Con ella va un corolario: **dentro de un `ScrollViewer` horizontal el ancho disponible es infinito**, así que `HorizontalAlignment="Stretch"` no estira nada y `MaxWidth` no fija ninguna anchura. Los campos de la cabecera se quedaron en un hilo al intentarlo. Ahí los anchos tienen que ser fijos.
>
> Y un segundo corolario, que se vio al estrechar las tarjetas del calendario: **dentro de un `StackPanel` horizontal, `TextTrimming` no hace nada**. Como el ancho que recibe el texto es infinito, nunca se cree que le falta sitio, así que no pone los puntos suspensivos y **lo que sobra lo recorta el `ClipToBounds` del borde, a mitad de letra**. Un `Grid` con una columna `Auto` para el icono y una `*` para el texto sí le dice lo que le queda.

> **Quinta trampa de WPF.** **Dentro de un `ItemsPanelTemplate` no resuelven las ataduras**: ni `ElementName` ni `RelativeSource AncestorType=ItemsControl`. La plantilla se aplica fuera del árbol donde viven esos nombres, así que la atadura no falla con ruido —simplemente no llega nunca el valor y el panel se queda con el suyo de por defecto. Costó un rato en la portada, intentando decirle a un `UniformGrid` cuántas columnas según el ancho. **Si hace falta que el panel reaccione al tamaño, se usa un `WrapPanel` con elementos de tamaño fijo**, que se recoloca solo y no necesita que nadie le cuente nada.

**Cómo convive con la toma de notas** (DD‑53). La planificación está dentro del `.lmnlab`, pero:

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
| **Gestión de proyectos** | Cuatro vistas de la misma carpeta —tablero, calendario, carga y BBDD— con **un solo juego de filtros para las cuatro**: estado, técnico, norma, grado IP, grado IK, acreditación y periodo de ensayo, más una **caja de buscar** en la barra. Los seis primeros viven en el botón «Filtros», que lleva la cuenta de los que apartan trabajo. **«(sin técnico)» se puede pedir** para ver lo que falta por repartir, y **«Cualquier estado»** es lo que hace buscable la BBDD (DD‑128). El tablero da columna por proyecto y tarjeta por sección pendiente |
| **Calendario** | Línea de tiempo en semanas ISO, **agrupada por técnico responsable**: estado, recepción de muestras, archivado y aviso de fuera de plazo. **Las barras se arrastran**: el centro mueve, los bordes cambian inicio o fin. Pulsarlas abre su configuración. Se dibuja siempre **medio año por detrás** del último trabajo, así que siempre hay sitio donde soltar y el salto de año sale solo (DD‑122) |
| **El calendario cabe** | Pensado para veinte proyectos a la vez (2026‑08‑05): **cada técnico se pliega** —y se recuerda plegado—, «SIN FECHAS O FUERA DEL PERIODO» también, las filas bajan a 36 px **sin encoger la tarjeta**, que sigue midiendo 30, y la cabecera del técnico pasa a una línea: nombre y «n proyectos» juntos. Se fueron las semanas que ocupaba, la ruta de la carpeta, «◀ Hoy ▶» y el rótulo del periodo. Por el tiempo se camina con la **barra horizontal, que ahora está siempre a la vista**: cada columna tiene su propio desplazamiento vertical, atados entre sí (`ScrollSincronizado.EnVerticalCon`), porque con un solo desplazamiento envolviendo las dos la barra quedaba al final del contenido. **Y la columna de nombres reserva debajo un hueco del alto de esa barra**: sin él dispone de 17 px más, llega al final antes y los nombres se separan de sus barras al bajar. El hueco va ahí y no una barra propia, porque un `ScrollViewer` que permite desplazamiento horizontal mide su contenido con ancho infinito y los nombres largos dejarían de recortarse (la sexta trampa) |
| **Los proyectos comparten fila** | Una fila es un **carril**, no un trabajo (DD‑129): caben todos los que no se pisen, cada uno colocado lo más arriba posible, y solo se abre fila nueva cuando dos coinciden. Veinte proyectos seguidos pasan de veinte filas a una, y **lo que se cuenta hacia abajo es cuántos coinciden a la vez**. Con ello, la columna de la izquierda se queda **solo con las cabeceras de técnico** —el código va escrito dentro de cada barra— y desaparece del todo al desagrupar. Abrir la toma de notas se hace ahora con el **botón derecho** sobre la barra |
| **Carga por técnico** | Tabla técnicos × meses en porcentaje de ocupación. El trabajo sale del importe (÷ 105 × 1,3 = horas, a 8 h por jornada) y se reparte entre meses por días entre semana, contra una capacidad de 22 días —10 en agosto, 15 en diciembre—. **Lo agrupado cuenta como todo lo demás, cada familia con su importe** (2026‑08‑05): cuatro familias son cuatro ensayos que ocupan cuatro veces. Ojo: una familia **sin fechas propias no entra**, aunque tenga importe — y en los grupos anteriores a DD‑123 solo la cabecera tenía fechas |
| **Técnicos** | Lista compartida del laboratorio, elegible en Técnico 1 y 2 de todas las normas. Se edita en `Configuración`; corregir un nombre se propaga a los proyectos, quitarlo no los toca |
| **Cuatro normas** | 60598‑1, 62031, 60529 (IP) y 62262 (IK), cada una con su plantilla y su catálogo de equipos, elegibles desde la portada. Ver sección 16 |
| **Portada** | Partida por la mitad: normas, abrir y los **tres últimos abiertos** a la izquierda; a la derecha, la zona de gestión encabezada por **«Planificar nueva TdN»** —en verde relleno, porque es lo único de esa zona que crea en vez de mirar— y debajo tablero, calendario, carga y BBDD. Encabezada por el nombre del programa y su **versión**. Antes el alta solo estaba en `Archivo`, y quien planifica tenía que ir a buscarla a la zona del que toma notas (2026‑08‑06) |
| **Alta rápida** | `Archivo \| Nueva toma de notas` crea el `.lmnlab` con **nombre, técnico y norma**, sin pasar por la cabecera ni por «Guardar como». Lo que falta se ve en rojo. El responsable ya tiene tarjeta que planificar antes de que exista un dato de ensayo |
| **Ventanas compactas** | Las tres que más crecían se apretaron sin quitar mandos (2026‑08‑05 y 06). **Filtros** pasó de 1351 a 934 px: los cuatro valores cortos en dos parejas —norma con acreditación, IP con IK— y fuera el recuadro que repetía por escrito lo que ya dicen los desplegables, que sigue en el consejo emergente del botón. **Nueva toma de notas**: técnico y norma en dos columnas, las explicaciones largas al consejo emergente, y fuera la frase que enumeraba los campos que faltan —los rótulos ya salen en rojo y «Crear» está apagado—. El aviso de esa ventana se quedó solo para lo que no se ve: un fallo al crear |
| **El código, completo** | 14 caracteres exactos —`TECNO260201-00`—, con el ejemplo siempre a la vista bajo la caja (2026‑08‑06). De ese código salen el de servicio (nueve), el de familia (once), el identificador de las muestras y el nombre del fichero: uno a medias deja los cuatro mal, y corregirlo después obliga a renombrar. **Se exige por los tres caminos** —al dar de alta, para guardar y para ensayar—, porque si no la regla dependería de por dónde hubiera entrado. En el alta, «Crear» sigue apagado; en la cabecera, el campo va en rojo, **no se guarda** y los apartados de ensayo no aparecen hasta que esté entero. Un proyecto anterior a la regla hay que arreglarlo antes de poder escribirlo, y es a propósito (DD‑130). **El fichero no se renombra solo** al corregir el código: mover el registro de un ensayo sin que nadie lo pida no se hace, así que para que el nombre case se usa «Guardar como». La longitud vive en `CodigoDeServicio`, junto a las otras dos que se recortan del mismo código |
| **Nombre de fichero** | `TdN_60598_TECNO260201-00.lmnlab`, compuesto en un solo sitio para los dos caminos que crean fichero. El nº de familia y la edición van dentro del código, que se teclea una vez en la cabecera |
| **Los dos códigos** | El de la **toma de notas** (`TECNO260201-00`) identifica el documento y nombra el fichero; el de **servicio** (`TECNO2602`) son sus nueve primeras, se rellena solo y **se puede corregir sin que se vuelva a pisar**. El aviso de duplicados va por el primero, los identificadores de muestra por el segundo |
| **Aviso de duplicados** | Al crear una toma de notas se comprueba si ese servicio ya existe en la carpeta, comparando sin mayúsculas ni espacios. Se ofrece abrir la que hay, o crear otra a sabiendas |
| **Trabajos de varias familias** | Campo **«Grupo»** en la planificación: las tomas de notas del mismo trabajo se enlazan y el calendario las enseña como **un tren de tarjetas, una por familia**, pegadas unas a otras, cada una con su código, su color de estado, su consejo emergente y su planificación al pulsarla (DD‑118, DD‑119). El avance y el importe salen sumados, y la fila se rotula con el nombre del grupo más «(agrupación)». El tablero las sigue viendo por separado |
| **El trabajo va en fila** | Al guardar cualquiera de sus tomas de notas, el grupo **se recoloca y se escribe** (DD‑123): cada una empieza al día siguiente de que acabe la anterior y conserva su duración; sin fin, una semana; sin nada, mañana. **El orden lo dan las fechas de inicio** —adelantar una familia es ponerle una fecha anterior— y se avisa de cuáles se han movido. Se acabaron las dos verdades: el diálogo, el calendario, la BBDD y la exportación leen lo mismo |
| **Arrastrar el trabajo** | Cogiendo cualquier tarjeta se mueve entero, conservando las distancias; los bordes de fuera lo estiran y tocan solo la familia de ese extremo (`RepartoDelArrastre`). El gesto se guarda **de una vez**, no familia a familia, para que la cadena no se recoloque contra datos a medias |
| **BBDD** | Cuarta vista de gestión: el listado de las tomas de notas. Solo lee — es una lente sobre el mismo escaneo, no un fichero que mantener (DD‑109). **Ya no tiene filtros propios**: obedece al juego compartido, incluido el estado, así que para lo archivado hay que pedir «Cualquier estado». Enseña además **cuándo se ensayó** cada una |
| **Cuándo se ensayó** | Al dar un servicio por **terminado**, el programa apunta solo la primera y la última fecha escritas en su toma de notas (`FechasDelEnsayo`). De ahí sale el **filtro por periodo** de la BBDD: «qué se hizo en el primer trimestre» se contesta sin que el técnico teclee un dato más. No sustituyen a las fechas de la planificación —aquellas son las previstas, estas las que ocurrieron— y entra lo que **se solapa** con el periodo, no solo lo que cabe dentro |
| **Planificación en la toma de notas** | Botón **«Planificación»** entre «Guardar» y «Exportar», con el verde de gestión. Enseña fechas, recepción de muestras, estado, archivado, importe y agrupación, y avisa de lo que corre prisa: fuera de plazo, muestras sin llegar, archivado. El **estado se edita ahí mismo** y se escribe al elegirlo, con `ActualizarPlanificacion` — nunca con `Guardar`, así que no pisa datos de ensayo (DD‑53 en el sentido contrario). Lo demás se edita en el mismo diálogo del tablero. **No sale en el informe**: `LumNotas.Report` no menciona la planificación. Su botón **«Ver en el calendario»** salta a gestión buscando las once primeras del código y **deja los filtros como haga falta para que el servicio se vea**: fuerza el estado solo si está terminado o archivado —los dos casos que lo esconden— y pone el técnico responsable, o «(todos)» si ese técnico no está en la lista. Al principio no tocaba los filtros y llevaba a un calendario vacío; parecía roto (2026‑08‑05) |
| **Estados de un servicio** | Por planificar, **Planificado**, En curso, Pendiente cliente y Terminado, en ese orden (2026‑08‑05). «Por planificar» se llamaba «Por hacer»; el nombre interno se queda como estaba porque es lo que llevan escrito los ficheros ya guardados |
| **Lo único que esconde es archivar** | «En desarrollo» trae todo lo que no esté archivado, **terminados incluidos** (2026‑08‑05). Antes los dejaba fuera y eso escondía trabajo vivo: un servicio terminado la semana pasada se sigue mirando. Pedir un estado concreto trae ese y solo ese, y sigue dejando fuera lo archivado |
| **Fechas bloqueadas** | Interruptor en **los dos sitios donde se ponen fechas** —el diálogo de planificación y el alta rápida— y **candado dibujado en la tarjeta**, junto al icono de la caja. Con él puesto no las mueve nadie: ni el diálogo —las casillas se apagan—, ni el arrastre —basta con que una familia esté bloqueada para que no se arrastre el trabajo, porque coger una tarjeta las mueve todas—, ni **la cadena de un grupo**, que es la vía por la que se rompería sin que nadie lo viera venir. Es para lo comprometido con el cliente |
| **Cerrar el servicio** | Al **exportar** —siempre, esté como esté— y al completarse el último apartado —una vez por pestaña—, se ofrece pasar el servicio a terminado o archivado. Es lo que evita que la toma de notas quede hecha y el calendario siga diciendo que está en curso durante semanas |
| **Acreditación** | Obligatoria y múltiple: «Sin acreditar», ENAC, ENEC y CB, con «Sin acreditar» excluyente. Sale en la exportación HTML, que no es un certificado sino la toma de notas que verifica y firma el director técnico (DD‑111) |
| **Laboratorios externos** | Recuadro «Otros colaboradores» en la cabecera: tantas filas como haga falta, cada una con el laboratorio y **el ensayo y el motivo juntos**. Opcional, texto libre, y consta en la exportación (DD‑110) |
| **Los dos códigos** | El de la **toma de notas** (`TECNO260201-00`) identifica el documento y nombra el fichero; el de **servicio** son sus nueve primeras, se rellena solo y se puede corregir sin que se vuelva a pisar (DD‑104) |
| **Grado del servicio** | El IP y el IK de cada muestra suben al listado como **el mayor**, con la regla del laboratorio: manda la segunda cifra y la primera desempata (DD‑112) |
| **Lo que hace falta para guardar** | Código de la toma de notas —**entero**, 14 caracteres (DD‑130)— y técnico 1, y nada más: sin ellos el fichero no se puede ni nombrar ni atribuir. Ensayar exige mucho más, pero un servicio a medias tiene que poder guardarse (DD‑115) |
| **.NET 10** | Se saltó de .NET 8 antes de repartir el programa, porque su soporte termina en noviembre de 2026. Costó cinco líneas: el programa no tiene ni una dependencia externa (DD‑107) |
| **Reportar un problema** | `Ayuda` da el correo de quien mantiene el programa con los datos que hacen falta para reproducir un fallo y acceso al registro de errores |
| **Normas con año** | El `id` de una plantilla lleva norma, parte y año (`60598-1_2024`), y el fichero se llama `plantilla-<id>_<version>.json`. **Dos años de la misma norma conviven en la misma carpeta**; de un mismo id solo cuenta la versión más alta. Los proyectos guardados con el id antiguo siguen encontrando la suya por `idsAnteriores` |
| **Trazabilidad de plantilla** | Cada `.lmnlab` guarda con qué versión se registró, el informe **declara esa** y no la instalada al imprimir, y al abrir un proyecto grabado con otra versión se avisa |
| **Carga en horas** | El trabajo se mide `importe ÷ 105 × 1,3` = horas, a 8 h por jornada — la cuenta del laboratorio, que sale a 80,77 €/h. Los tres números se editan por separado, con la equivalencia a la vista |
| **Grado por muestra** | IP e IK se eligen en la fila de cada muestra, con el atajo «Luminaria ordinaria». La fila es idéntica en las tres normas que la usan |
| **Dos carpetas de OneDrive** | Una de proyectos y otra compartida (normas, técnicos, tarifa, versión). Se eligen en `Configuración` y se preguntan solas la primera vez |
| **Una versión para todos** | Las normas se leen de la carpeta compartida y se publican desde `Configuración`; `Ayuda \| Acerca de` enseña la versión y avisa si el laboratorio ha publicado una más nueva |
| **Árboles grandes** | Escaneo en segundo plano, en paralelo, con caché entre sesiones y saltando las ramas sin permisos. 400 proyectos: 461 ms la primera vez, 107 ms las siguientes |

### Pendiente

**Lo primero, y no es código: nadie ha rellenado todavía un ensayo real de principio a fin.** Ni un servicio completo con sus muestras, su informe impreso y firmado. Todo lo construido encima es una apuesta razonada, pero una apuesta. Un servicio de verdad en paralelo con el Excel dirá más que tres funciones nuevas — y hasta que eso pase, no conviene repartir el programa a seis ordenadores.

| Prioridad | Qué |
|---|---|
| Alta | **Filtro por antigüedad al escanear** («leer solo los últimos 2 años»). Propuesto el 2026‑08‑02 y **pendiente de confirmar el valor por defecto**. Es el único filtro que evita *leer*, no solo *ver*: importa en un equipo nuevo y cuando se publica una norma, que invalida la caché entera. Debe decir siempre cuántos proyectos ha dejado fuera |
| Media | **Calibración de los equipos.** Ya se registra qué equipo se usó en cada apartado; si el catálogo llevara su fecha de calibración, el programa podría avisar de que un equipo estaba fuera de calibración el día del ensayo. Media función hecha y sin aprovechar — es la no conformidad que detecta el programa antes que el auditor |
| Media | **Duplicar una toma de notas.** Un trabajo lleva varias familias y todas comparten cliente, código y técnicos. El alta rápida y el enlace por grupo alivian la mitad del problema; lo que falta es **arrancar una familia desde otra ya rellena**, sin repetir la cabecera cuatro veces |
| Media | **Arrastrar la frontera entre dos familias** para mover su fecha de corte. Es **una sola fecha** —el fin de la familia de la izquierda; el inicio de la siguiente sale de la cadena (DD‑123)—, así que el dato es sencillo. Con el tren de tarjetas (DD‑119) la frontera ya es el **borde real** de un elemento real, así que lo que queda es que `ArrastreDeBarra` distinga ese borde del de fuera y escriba solo esa fecha. Se dejó aparte a propósito, para que si algo se rompe se sepa qué lo rompió |
| Media | **Editor de estados: cuatro más, con su color.** Diseño acordado con el laboratorio el 2026‑08‑07; **nadie lo ha pedido formalmente todavía**, se anota para no volver a decidirlo. Ver «Cómo sería el editor de estados» más abajo |
| Baja | **Exportar el calendario o la carga**, para enviarlos a quien no abre el programa. El listado de la BBDD ya se exporta (DD‑140) y el exportador de HTML está hecho; lo que falta es decidir **qué es exportar un dibujo** — una línea de tiempo y una tabla de porcentajes no se llevan al papel escribiendo las mismas filas |
| Baja | **Los selectores de fecha dicen «Select a date»**, en inglés: es el texto de fondo por defecto de WPF. Afecta a la planificación y al alta. Se arregla poniéndoles el idioma |
| Baja | **Quedan controles sin nombre accesible en la toma de notas.** Medido el 2026‑08‑06 sobre un proyecto real: **38 botones de 46**, los **7 desplegables** y las **4 cajas de texto** se anuncian vacíos. Son dos fallos distintos: los botones repiten el de la portada (DD‑135) y el del índice (DD‑139) —contenido que es un panel, así que WPF no sabe cuál pieza es el nombre—; las cajas y los desplegables son otra cosa, **la etiqueta vive en otra celda de la rejilla y nada la ata al campo**, que es lo que resuelve `AutomationProperties.LabeledBy`. Falta repasar también las cuatro vistas de gestión. **No corre prisa mientras nadie use lector de pantalla ni control por voz**, pero encarece cada verificación: obliga a manejar el programa por posición en vez de por nombre |
| Baja | **Arrastrar la frontera entre dos familias** de un mismo trabajo para mover su fecha de corte. Es **una sola fecha** —el fin de la de la izquierda; el inicio de la siguiente sale de la cadena (DD‑123)—, y desde el tren de tarjetas esa frontera ya es el **borde real** de un elemento real: lo que falta es que `ArrastreDeBarra` distinga ese borde del de fuera |
| Baja | **Las tarjetas de clase, Ta y partes ‑2 siguen escritas a mano en el XAML.** Solo se muestran y se exigen donde la norma las declara, pero el asterisco de obligatorio es texto fijo. Se generalizó la cabecera entera el 2026‑08‑01 y **el laboratorio pidió revertirlo**: la pantalla de luminarias se da por buena y no se toca |
| **Alta** | **`MainWindow.xaml` se ha hecho grande de verdad.** Ya son **cuatro** vistas de gestión —tablero, calendario, carga y BBDD— dentro del mismo fichero, además de la toma de notas y la portada. Toca partirlo en diccionarios de recursos, uno por vista. **Ha vuelto a subir de prioridad**: con la BBDD y el tren de tarjetas, encontrar dónde tocar cuesta ya más que el cambio en sí |
| Alta | **Selectores de fecha y hora en la toma de notas.** Hoy se escriben como texto (`20/07/2026 23:40`). Es lo que más molestará en uso real. La planificación ya usa `DatePicker`, así que el patrón a seguir está hecho |
| Alta | **Campos calculados de solo lectura.** El radio del arco de lluvia y las dos fuerzas de carga estática están implementados y con tests en `Calculos.cs`, pero la interfaz no sabe mostrar un campo calculado: se rellenan a mano |
| Media | **Selección automática de equipos IP** (`seleccionAutomaticaEquipos`): declarada en la plantilla, no implementada |
| Media | **Perfil de usuario** (DD‑08). Con la lista de técnicos ya hecha, lo que falta es saber **quién** está usando el programa, para firmar quién guardó cada cosa |
| Baja | **La cabecera del calendario no se queda fija al desplazarse en vertical.** Con muchos proyectos habrá que congelarla |
| Media | **Instalador** y asociación de la extensión `.lmnlab`. La recomendación es **ClickOnce** publicando a una carpeta de red —cada equipo instala una vez y **se actualiza solo al arrancar**—, con **Inno Setup** como plan B. Publicar **dependiente del framework** e instalar el *.NET Desktop Runtime* en cada equipo (DD‑107): son unos MB por actualización en vez de 130, y los parches del runtime llegan por Windows Update. SmartScreen avisará la primera vez por no estar firmado; firmar cuesta 200‑400 €/año. **No repartirlo hasta que el programa haya pasado un ensayo real completo**: sería repartir el mismo problema a seis ordenadores |
| Baja | Con 30 muestras el informe A4 no cabe: habría que girar la tabla o partirla |

#### Cómo sería el editor de estados

Acordado con el laboratorio el 2026‑08‑07, sin escribir código. Se anota entero porque **la parte difícil no es el editor sino decidir qué puede tocarse**, y eso ya está decidido.

**Cuatro estados más, y solo cuatro.** El tope no es pereza: en el calendario **el color de la barra es lo único que dice el estado** (DD‑141), y con doce colores no se distingue ninguno. Nueve ya es mucho.

**El comportamiento no se pregunta, viene con el hueco.** De los cuatro, **dos no cuentan en la carga y dos sí**. Así el laboratorio elige metiendo el estado en un hueco o en otro, y nadie tiene que contestar «¿este ocupa al técnico?» cada vez — que es exactamente la pregunta que se contestó mal y costó DD‑142. **Los cuatro pueden estar fuera de plazo**: el único exento sigue siendo el que cuenta como acabado.

| | Los cinco de ahora | Los cuatro nuevos |
|---|---|---|
| Borrar | **nunca** | no; se **desactivan** |
| Renombrar y elegir color | sí | sí |
| Cuenta en la carga | como hoy | dos sí, dos no |
| Puede ir fuera de plazo | como hoy | los cuatro |

**No se borra, se desactiva.** Es la regla que ya existe para los técnicos (`ConNombreSuelto`): un técnico que sale de la lista **se sigue ofreciendo en los proyectos que ya lo llevan**, para no dejarlos sin responsable. Igual aquí: un estado desactivado deja de ofrecerse para trabajo nuevo y los proyectos que lo tengan lo siguen enseñando. Con eso desaparece la pregunta incómoda de «¿qué hago con los quince proyectos que usaban el estado que acabo de borrar?».

**Tres cosas hay que arreglar antes de empezar**, y las tres son deuda que ya existe:

1. **El filtro compara por la etiqueta** (`FiltroDeEstado.Pasa` mira `EtiquetaDe(estado) == filtro`). En cuanto el nombre sea editable, renombrar un estado rompe el filtro en silencio. Tiene que comparar por identidad — el mismo cambio que se hizo con el desplegable de normas (DD‑134).
2. **`Terminado` está escrito en siete sitios** donde significa algo: la carga, el aviso de retraso —dos veces—, el cierre al exportar, `YaEstaCerrado` y las fechas reales del ensayo. Eso pasa a ser una marca del estado («este cuenta como acabado») en vez de una comparación con una constante.
3. **Hay que subir `CacheDeResumenes.Formato`.** Cambia la forma del resumen, y sin subirla el tablero enseñará estados en blanco durante días. Ya mordió una vez (DD‑137).

**El color no puede ser libre del todo**: las barras llevan texto e iconos **blancos** encima. O paleta cerrada, o color libre con el texto ajustándose solo y un aviso cuando no llegue al contraste mínimo. Paleta cerrada es más rápido y no hay forma de estropearlo.

### Pendiente del laboratorio

**D‑07** (`EQ-CERT` vs `EQ-SAFE`), **D‑13** (los dos criterios de carga estática), **D‑14** (`EN 60598-2-15`), **D‑21** (confirmar la corrección de 7.12) y **D‑22** (si deben validarse las casillas sin enlazar).

**D‑23 — confirmar que el reparto de pesos sigue valiendo ahora que se ve en tres sitios.** No es que falten pesos: las cinco plantillas los declaran y son fielmente los del Excel —23 conceptos, 217 puntos en luminarias—. Lo que hay que confirmar es **la forma de la curva**, porque desde DD‑137 ese número sale del tablero y del calendario y no solo del informe. **Endurancia pesa 100 de 217**, casi la mitad (D‑18, que se cerró aceptándolo). Consecuencia práctica: un servicio con todo hecho menos endurancia enseñará **≈54 %**, y cerrar ese único ensayo lo sube de golpe a 100. En proyectos con pocas secciones aplicables endurancia puede ser el 70‑80 %. Es defendible —endurancia dura semanas— pero conviene que el laboratorio lo mire sabiendo que ahora es lo que ve el responsable de un vistazo. Es un campo del JSON: si se quiere cambiar, no hace falta tocar código.

Además, la lista de normas que admite la **62031** (`meta.normasCompatibles`) la puse yo por deducción —IP e IK, porque no los lleva dentro— y **el laboratorio no la ha confirmado**.

### Descartado, y por qué

- **Login con usuario, contraseña y recuperación por email** (consultado el 2026‑08‑02). En una aplicación de escritorio con ficheros JSON en una carpeta compartida, un login **no protege nada**: los datos se abren con el Bloc de notas sin pasar por él, y los permisos que comprueba el propio programa se saltan cerrando el programa. Lo que hace falta para la ISO 17025 no es autenticación sino **trazabilidad** —quién guardó y cuándo—, que es el perfil de usuario ya pendiente (DD‑08) y cuesta un par de días. Si algún día hay servidor, la autenticación se delega en **Entra ID**, que el laboratorio ya paga con Microsoft 365, en vez de escribir la nuestra.
- **Varias ventanas** en lugar de pestañas: se valoró por ser mucho más barato, pero el laboratorio prefirió pestañas.
- **Contraseña para entrar en «Configuración»** (consultado el 2026‑08‑06, al aparecer los importes de las ofertas). Se descartó tras verlo, **porque protegía lo que no importaba**: detrás de ese menú solo están la lista de técnicos y la tarifa, mientras que los importes viven en los `.lmnlab` y se ven de tres maneras que no pasan por ahí —el diálogo de cualquier barra del calendario, el Bloc de notas sobre el fichero, y la vista Carga—. Es echar la llave al cajón de los bolígrafos con la caja fuerte abierta.
  Añadido a esto, la recuperación no tenía salida decente: sin servidor, la recuperación por correo obligaría a incrustar las credenciales del servidor de correo en el programa, de donde las saca cualquiera. **Si algún día hay que impedir que alguien vea los importes, la respuesta son los permisos de la carpeta** —OneDrive o Windows sobre una subcarpeta—, que sí los respeta el sistema operativo, no una comprobación que hace el propio programa y que se salta cerrándolo.

### Cómo retomar

```bash
dotnet build "…\AplicacionTomaNotas\LumNotas.sln"
dotnet test "…\AplicacionTomaNotas\LumNotas.sln"
dotnet run --project "…\AplicacionTomaNotas\src\LumNotas.App"
```

> **`dotnet test` NO reconstruye la aplicación.** Compila el núcleo y los tests, y se para
> ahí: `LumNotas.App` no es un proyecto de pruebas. Quien cambie algo de la ventana, pase
> los tests en verde y abra el programa a mirar, estará mirando **el ejecutable de antes**
> — y concluirá que su cambio no funciona. Pasó tres veces en un solo día (2026‑08‑05).
> Por eso el `build` va primero en la lista de arriba.
>
> Si el `build` falla con `MSB3027 / MSB3021`, no es un error de código: hay una instancia
> del programa abierta y el `.exe` está bloqueado. Ciérrala.

Si los **562 tests** pasan, el motor y las cinco plantillas están sanos. La mayoría de los cambios de norma se hacen **editando el JSON de esa norma** en `plantilla/`, sin tocar código; añadir una norma entera es dejar caer un fichero `plantilla-*.json` en esa carpeta.

**Dónde poner cada cosa**, que es lo que más se ha repetido al construir esto:

| Si es… | Va en… |
|---|---|
| Reglas, cálculos, aritmética de fechas, gestos | `LumNotas.Core` — **para poder probarlo** |
| Ficheros: proyectos, caché, escaneo | `LumNotas.Storage` |
| Un filtro nuevo del tablero | `GestionViewModel`, **no** en una vista: valen para las tres |
| Algo compartido por el laboratorio | La carpeta compartida, y reflejado en `Configuración \| Carpetas…` |

**Ninguno de los 562 tests toca la interfaz**, así que lo que se apoye en WPF hay que escribirlo de forma que la lógica quede fuera. No es purismo: sacar la lógica del gesto al núcleo fue la única manera de comprobar el arrastre del calendario.

> Matiz al «no se puede automatizar el ratón» de DD‑57: **inyectar clics sí funciona**, pero solo si la ventana está en primer plano —si no, el clic se lo lleva la que esté encima y parece que la automatización está rota—. Aun así la regla de DD‑57 no cambia: la lógica del gesto sigue en el núcleo, porque un clic inyectado comprueba que el botón responde, no que la aritmética del arrastre sea correcta.

> **Confirmado el 2026‑08‑07 por tercera vía.** Se montó un banco aparte —un WPF mínimo que abre una ventana propia y llama a `DialogoNuevoProyecto.Preguntar` con ella de dueña, el camino de verdad— y **el diálogo tampoco salió**: la ventana del banco sí, el modal no, y el programa se quedó dentro de `ShowDialog` esperando. **El bucle modal corre, la ventana no se materializa.** Es del entorno y no hay forma de rodearlo desde dentro del programa; lo que queda es comprobar el inventario —que compilar ya garantiza: un `x:Name` que desapareciera o un manejador que faltara son error de compilación— y pedirle al laboratorio que lo mire.

> **Lo que la automatización no alcanza: los diálogos, en algunas sesiones.** El 2026‑08‑06, al probar la exportación del listado, ningún botón que abriera un diálogo hizo nada al accionarlo por UIA — ni el nuevo ni «Elegir carpeta», «Filtros» o «Abrir TdN existente», que funcionan a diario. Se comprobó que la ventana principal **seguía habilitada**, luego no había ningún modal abierto: el diálogo no llegaba a crearse. Los botones que solo cambian de vista sí respondían, y en la misma sesión `Add-Type` no podía escribir en `%TEMP%`. Es del entorno, no del programa. **Cuando pase, no hay que insistir**: se verifica el generador con tests y se produce el fichero por un programa aparte que llame al mismo código sin el diálogo — que es como se comprobó DD‑140 sobre los 233 proyectos reales.

> **Lo que sí se puede comprobar a mano, y durante meses se dio por imposible.** **UI Automation sí acciona esta aplicación**: botones, menús y submenús, leyendo además si una entrada está marcada. Lo que fallaba no era la automatización sino dónde se buscaba: **los diálogos y los menús emergentes de WPF no cuelgan del árbol de la ventana principal**. Hay que buscarlos desde el escritorio (`AutomationElement.RootElement`) acotando por identificador de proceso, o enumerando ventanas con `EnumWindows`. Con eso se puede recorrer un menú, abrir un diálogo y fotografiarlo — que es como se han verificado los últimos cambios de pantalla.

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
| Luminarias — EN IEC 60598‑1:2024 + A11:2024 | `plantilla-60598-1_2024_1.0.0.json` | 16 | 45 |
| Módulos LED — EN IEC 62031:2020 + A11:2021 | `plantilla-62031_2020_A11_1.0.0.json` | 16 | 26 |
| Grados de protección IP — EN 60529:1991 + enmiendas | `plantilla-60529_1991_1.0.0.json` | 3 | 3 |
| Grados de protección IK — EN 62262:2002 + A1:2021 | `plantilla-62262_2002_A1_1.0.0.json` | 2 | 2 |

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

El formato `.lmnlab` gana `normas`, `patronIdentificador` y `selecciones`, y **sigue leyendo el formato antiguo**: los proyectos de luminarias ya guardados abren sin perder nada.

### Cómo se elige la norma

**En la portada**, eligiendo su tarjeta. No hay selector en la cabecera: la norma se decide al empezar y no se conmuta sobre un proyecto a medias, porque cada una tiene su cabecera, sus apartados y sus equipos. Al abrir un `.lmnlab` se carga la norma con la que nació, no la que estuviera en pantalla.

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
- El `.lmnlab` recuerda cuáles lleva, y al abrirlo se recargan solas.

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
