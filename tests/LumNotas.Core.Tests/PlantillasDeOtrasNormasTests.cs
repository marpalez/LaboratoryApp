using LumNotas.Core.Datos;
using LumNotas.Core.Gestion;
using LumNotas.Core.Motor;
using LumNotas.Core.Plantilla;

namespace LumNotas.Core.Tests;

/// <summary>
/// El laboratorio ensaya contra varias normas y cada una tiene su propia toma de notas.
/// Estos tests se pasan sobre <b>todas</b> las plantillas de la carpeta, no solo sobre la
/// de luminarias: una norma nueva queda cubierta por el simple hecho de existir.
/// </summary>
public class PlantillasDeOtrasNormasTests
{
    public static TheoryData<string> Plantillas()
    {
        var datos = new TheoryData<string>();
        foreach (var ruta in Directory.GetFiles(Contexto.CarpetaDePlantillas(), "plantilla-*.json").OrderBy(r => r))
            datos.Add(Path.GetFileName(ruta));
        return datos;
    }

    private static PlantillaEnsayos Cargar(string fichero)
        => PlantillaEnsayos.Cargar(Path.Combine(Contexto.CarpetaDePlantillas(), fichero));

    private static IEnumerable<Regla> ReglasDe(PlantillaEnsayos p)
        => p.Proyecto.Reglas.Concat(p.Bloques().SelectMany(PlantillaEnsayos.ReglasDe));

    [Fact]
    public void EstanLasCuatroNormasDelLaboratorio()
    {
        var ids = Contexto.TodasLasPlantillas().Select(p => p.Meta.Id).ToList();

        Assert.Contains("60598", ids);
        Assert.Contains("62031", ids);
        Assert.Contains("60529", ids);
        Assert.Contains("62262", ids);
    }

    [Theory]
    [MemberData(nameof(Plantillas))]
    public void LaPlantillaCargaYTieneLoMinimo(string fichero)
    {
        var plantilla = Cargar(fichero);

        Assert.False(string.IsNullOrWhiteSpace(plantilla.Meta.Id));
        Assert.NotEmpty(plantilla.Secciones);
        Assert.NotEmpty(plantilla.Bloques());
        Assert.NotEmpty(plantilla.Proyecto.Campos);
        Assert.Contains(plantilla.Proyecto.Campos, c => c.Obligatorio);
    }

    [Theory]
    [MemberData(nameof(Plantillas))]
    public void LosIdentificadoresNoSeRepiten(string fichero)
    {
        var plantilla = Cargar(fichero);

        Assert.Empty(plantilla.Bloques().GroupBy(b => b.Id).Where(g => g.Count() > 1).Select(g => g.Key));
        Assert.Empty(ReglasDe(plantilla).GroupBy(r => r.Id).Where(g => g.Count() > 1).Select(g => g.Key));
    }

    /// <summary>
    /// Cubre el punto ciego que tenía <c>PlantillaTests</c>: <c>visibleSi</c> y
    /// <c>reglaDeCierre</c> también apuntan a reglas, y un id mal escrito ahí no se
    /// manifestaba como error sino como un apartado que no aparecía.
    /// </summary>
    [Theory]
    [MemberData(nameof(Plantillas))]
    public void VisibleSiYReglaDeCierreApuntanAReglasQueExisten(string fichero)
    {
        var plantilla = Cargar(fichero);
        var existentes = ReglasDe(plantilla).Select(r => r.Id).ToHashSet();
        var rotas = new List<string>();

        void Comprobar(string? referencia, string donde)
        {
            if (referencia is null) return;
            if (!existentes.Contains(referencia.TrimStart('!'))) rotas.Add($"{donde} → {referencia}");
        }

        foreach (var bloque in plantilla.Bloques())
        {
            Comprobar(bloque.VisibleSi, $"{bloque.Id}.visibleSi");
            Comprobar(bloque.ReglaDeCierre, $"{bloque.Id}.reglaDeCierre");

            foreach (var campo in bloque.Campos)
                Comprobar(campo.VisibleSi, $"{bloque.Id}.{campo.Id}.visibleSi");

            foreach (var sub in bloque.SubBloques)
                foreach (var campo in sub.Campos)
                    Comprobar(campo.VisibleSi, $"{sub.Id}.{campo.Id}.visibleSi");
        }

        Assert.Empty(rotas);
    }

    [Theory]
    [MemberData(nameof(Plantillas))]
    public void LasReferenciasEntreReglasApuntanAReglasQueExisten(string fichero)
    {
        var plantilla = Cargar(fichero);
        var existentes = ReglasDe(plantilla).Select(r => r.Id).ToHashSet();

        var rotas = (from r in ReglasDe(plantilla)
                     from referencia in r.FaltanSi.Concat(r.De).Concat(r.CuandoTodas)
                         .Concat(new[] { r.Condicion, r.Entonces, r.SoloSi, r.FinSiNo }.Where(x => x is not null)!)
                     where !existentes.Contains(referencia!.TrimStart('!'))
                     select $"{r.Id} → {referencia}").ToList();

        Assert.Empty(rotas);
    }

    [Theory]
    [MemberData(nameof(Plantillas))]
    public void LosPredicadosQueUsaEstanRegistrados(string fichero)
    {
        var usados = ReglasDe(Cargar(fichero))
            .Where(r => r.Tipo == "predicado" && r.Nombre is not null)
            .Select(r => r.Nombre!)
            .Distinct();

        Assert.All(usados, nombre => Assert.Contains(nombre, Predicados.Registrados));
    }

    [Theory]
    [MemberData(nameof(Plantillas))]
    public void TodasLasReglasSeEvaluanSinExcepcionSobreUnProyectoVacio(string fichero)
    {
        var plantilla = Cargar(fichero);
        var motor = new MotorDeReglas(plantilla, new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 });
        var fallos = new List<string>();

        foreach (var regla in ReglasDe(plantilla))
        {
            if (regla.Tipo == "seleccionAutomaticaEquipos") continue;
            try { motor.Evaluar(regla.Id); }
            catch (Exception ex) { fallos.Add($"{regla.Id} ({regla.Tipo}): {ex.Message}"); }
        }

        Assert.Empty(fallos);
    }

    /// <summary>
    /// Cada norma trae su catálogo de equipos importado por separado. Si un apartado
    /// pide un grupo que no está en él, el técnico se queda sin poder marcar equipos y
    /// nada lo avisa en ejecución.
    /// </summary>
    [Theory]
    [MemberData(nameof(Plantillas))]
    public void LosGruposDeEquiposQueSePidenExistenEnElCatalogo(string fichero)
    {
        var plantilla = Cargar(fichero);
        var catalogo = CatalogoDeEquipos.Junto(
            Path.Combine(Contexto.CarpetaDePlantillas(), fichero), plantilla);

        var disponibles = catalogo.Grupos.Select(g => g.Id).ToHashSet();

        var rotos = plantilla.Bloques()
            .Where(b => !string.IsNullOrWhiteSpace(b.Equipos) && !disponibles.Contains(b.Equipos!))
            .Select(b => $"{b.Id} → {b.Equipos}")
            .ToList();

        Assert.Empty(rotos);
    }

    /// <summary>
    /// Con varias normas en un mismo proyecto los datos comparten almacén, así que dos
    /// plantillas distintas no pueden usar el mismo id de bloque: se pisarían los datos.
    /// </summary>
    [Fact]
    public void NingunBloqueSeLlamaIgualEnDosNormasDistintas()
    {
        var colisiones = Contexto.TodasLasPlantillas()
            .SelectMany(p => p.Bloques().Select(b => (Norma: p.Meta.Id, b.Id)))
            .GroupBy(x => x.Id)
            .Where(g => g.Select(x => x.Norma).Distinct().Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.Norma).Distinct())}")
            .ToList();

        Assert.Empty(colisiones);
    }

    [Fact]
    public void ElIkUsaElPrefijoDeMuestraDelLaboratorioParaEseServicio()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };

        ik.AplicarA(datos);

        Assert.Equal("EBP_CLIM12345202601", datos.IdentificadorDeMuestra(1));
        Assert.Contains("62262", datos.Normas);
    }

    [Fact]
    public void ElMetodoDeGolpeoSoloSeExigeDeIk07EnAdelante()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };

        datos.Establecer("proyecto", "gradoIk", "IK05", 1);
        Assert.False(new MotorDeReglas(ik, datos).EsVerdadera("R-62262-ik-metodoElegible"));

        datos.Establecer("proyecto", "gradoIk", "IK10", 1);
        Assert.True(new MotorDeReglas(ik, datos).EsVerdadera("R-62262-ik-metodoElegible"));
    }

    /// <summary>
    /// Un servicio de luminarias puede llevar además la 62031 y el IK 62262. El informe
    /// tiene que traer los apartados de todas: es el registro del ensayo completo.
    /// </summary>
    [Fact]
    public void ElInformeIncluyeLosApartadosDeLasNormasAnadidas()
    {
        var lum = Contexto.Plantilla;
        var led = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62031");
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };

        var soloLuminarias = new Report.ExportadorDeInforme(lum).GenerarHtml(datos);
        var conLasTres = new Report.ExportadorDeInforme(lum)
        {
            Adicionales =
            [
                new Report.ExportadorDeInforme.NormaAdicional(led, Core.Plantilla.CatalogoDeEquipos.Vacio),
                new Report.ExportadorDeInforme.NormaAdicional(ik, Core.Plantilla.CatalogoDeEquipos.Vacio)
            ]
        }.GenerarHtml(datos);

        // Apartados propios de cada norma añadida, que no existen en luminarias.
        Assert.DoesNotContain("Riesgo por luz azul", soloLuminarias);
        Assert.Contains("Riesgo por luz azul", conLasTres);
        Assert.Contains("Resistencia a los impactos mecánicos externos", conLasTres);

        // Y la portada dice qué normas lleva el documento.
        Assert.Contains("Normas incluidas", conLasTres);
        Assert.DoesNotContain("Normas incluidas", soloLuminarias);
    }

    /// <summary>
    /// Qué normas se pueden añadir a cada una lo decide el laboratorio y vive en la
    /// plantilla. Luminarias no admite la 60529 porque ya lleva el IP dentro (sección 11);
    /// el IP y el IK solo se admiten entre sí.
    /// </summary>
    [Fact]
    public void CadaNormaAdmiteSoloLasQueElLaboratorioPermite()
    {
        var admitidas = Contexto.TodasLasPlantillas()
            .ToDictionary(p => p.Meta.Id, p => p.Meta.NormasCompatibles ?? []);

        // Luminarias no admite ni IP ni IK: los lleva dentro.
        Assert.Equal(["62031"], admitidas["60598"]);
        Assert.Equal(["62262"], admitidas["60529"]);
        Assert.Equal(["60529"], admitidas["62262"]);
    }

    /// <summary>
    /// La cabecera de luminarias tiene su propio sitio en la ventana y no debe cambiar:
    /// si esta plantilla declarase un campo fuera de esa lista aparecería una tarjeta
    /// nueva en una pantalla que el laboratorio ya da por buena.
    /// </summary>
    [Fact]
    public void LaCabeceraDeLuminariasNoPideNadaFueraDeLoQueYaMuestraLaVentana()
    {
        string[] conSitioPropio =
        [
            "codigoServicio", "tecnico1", "tecnico2", "numeroMuestras",
            "numeracionMuestras", "inicioNumeracion", "comentariosGenerales",
            "ta", "clase", "partes2", "ipPrimeraCifra", "ipSegundaCifra", "sinGradoIp"
        ];

        // Los campos por muestra tampoco generan tarjeta: se pintan junto a su muestra.
        var extra = Contexto.Plantilla.Proyecto.Campos
            .Where(c => !c.PorMuestra)
            .Select(c => c.Id)
            .Except(conSitioPropio)
            .ToList();

        // Los únicos que sí llevan tarjeta propia son los de inmersión, que además solo
        // aparecen con objetivo IPX7 o IPX8. El laboratorio los pidió igual que en la 60529.
        Assert.Equal(["profundidadInmersion", "tiempoInmersion", "temperaturaInmersion"], extra);
    }

    [Fact]
    public void ElIkPideSuGradoObjetivoPorMuestra()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");

        var grado = ik.Proyecto.Campos.Single(c => c.Id == "gradoIk");
        Assert.True(grado.PorMuestra);
        Assert.False(grado.Multiple);
        Assert.True(grado.Obligatorio);
        Assert.Contains("IK10", grado.Opciones);
        Assert.Equal("IK", grado.EtiquetaCorta);

        // El grado IP también va por muestra, pero aquí es opcional: se puede tener o no.
        foreach (var id in new[] { "ipPrimeraCifra", "ipSegundaCifra" })
        {
            var cifra = ik.Proyecto.Campos.Single(c => c.Id == id);
            Assert.True(cifra.PorMuestra);
            Assert.False(cifra.Obligatorio);
        }

        Assert.True(ik.Proyecto.Campos.Single(c => c.Id == "luminariaOrdinaria").PorMuestra);
    }

    /// <summary>
    /// Un servicio de IK puede ser de una luminaria o de otro producto, y hay que decirlo
    /// antes de empezar: sin ello no se muestran los apartados de ensayo.
    /// </summary>
    [Fact]
    public void ElIkExigeLaTipologiaDeProductoAntesDeEmpezar()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var campo = ik.Proyecto.Campos.Single(c => c.Id == "partes2");

        Assert.Equal("Tipología de producto (luminarias u otros)", campo.Etiqueta);
        Assert.True(campo.Obligatorio);
        Assert.Contains("OTRO", campo.Opciones);

        // En luminarias sigue llamándose como siempre.
        Assert.Equal("Partes -2 aplicables",
            Contexto.Plantilla.Proyecto.Campos.Single(c => c.Id == "partes2").Etiqueta);

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "gradoIk", "IK08", 1);

        Assert.Contains(RequisitosDelProyecto.Faltantes(ik, datos), f => f.Contains("Tipología"));

        datos.Partes2.Add("OTRO");
        Assert.Empty(RequisitosDelProyecto.Faltantes(ik, datos));
    }

    /// <summary>
    /// El grado IK se exige en todas las muestras; el IP es opcional y se deja en blanco
    /// en las que no lo tengan.
    /// </summary>
    [Fact]
    public void EnElIkSeExigeElGradoDeCadaMuestraYElIpEsOpcional()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Partes2.Add("OTRO");

        datos.Establecer("proyecto", "gradoIk", "IK08", 1);
        Assert.Contains(RequisitosDelProyecto.Faltantes(ik, datos), f => f.Contains("IK"));

        datos.Establecer("proyecto", "gradoIk", "IK10", 2);

        // Sin tocar el IP, la cabecera ya está completa.
        Assert.Empty(RequisitosDelProyecto.Faltantes(ik, datos));

        datos.Establecer("proyecto", "ipSegundaCifra", "IPX5", 1);
        Assert.Empty(RequisitosDelProyecto.Faltantes(ik, datos));
    }

    /// <summary>
    /// Luminarias no tiene la casilla, así que su cabecera se sigue exigiendo igual que
    /// antes: el grado IP es obligatorio en las dos cifras.
    /// </summary>
    [Fact]
    public void EnLuminariasElGradoIpSigueSiendoObligatorio()
    {
        Assert.DoesNotContain(Contexto.Plantilla.Proyecto.Campos, c => c.Id == "sinGradoIp");

        var datos = Contexto.ProyectoVacio();
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "ta", 25d);
        datos.Partes2.Add("-2-1");

        var faltan = RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos);

        Assert.Contains(faltan, f => f.Contains("1ª cifra"));
        Assert.Contains(faltan, f => f.Contains("2ª cifra"));
    }

    [Fact]
    public void LosModulosLedPidenTcYSuClasificacion()
    {
        var led = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62031");

        var tc = led.Proyecto.Campos.Single(c => c.Id == "tc");
        Assert.Equal("numero", tc.Tipo);
        Assert.True(tc.Obligatorio);

        var clasificacion = led.Proyecto.Campos.Single(c => c.Id == "clasificacionModulo");
        Assert.Equal(["Independiente", "A incorporar", "Integrado"], clasificacion.Opciones);
        Assert.False(clasificacion.Multiple);
    }

    /// <summary>
    /// Con la cabecera rellena, la 62262 tiene que darse por completa: si no, sus
    /// apartados no llegan a aparecer.
    /// </summary>
    [Fact]
    public void LaCabeceraDelIkSePuedeCompletar()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };

        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "gradoIk", "IK08", 1);
        datos.Establecer("proyecto", "gradoIk", "IK08", 2);
        datos.Partes2.Add("OTRO");

        Assert.Empty(RequisitosDelProyecto.Faltantes(ik, datos));
    }

    [Fact]
    public void LaCabeceraDeLosModulosLedSePuedeCompletar()
    {
        var led = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62031");
        var datos = new DatosProyecto { CodigoServicio = "202612345", NumeroMuestras = 2 };

        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "tc", 65d);
        datos.Establecer("proyecto", "clasificacionModulo", "A incorporar");

        Assert.Empty(RequisitosDelProyecto.Faltantes(led, datos));
    }

    /// <summary>
    /// Los ratings y datos generales se toman en todos los servicios, así que ese
    /// apartado no puede marcarse como no aplicable. El Excel sí lo permitía.
    /// </summary>
    [Fact]
    public void LosRatingsYDatosGeneralesNoSePuedenMarcarComoNoAplicables()
    {
        var generales = Contexto.Plantilla.Bloque("generales");

        Assert.Null(generales.Na);
        Assert.All(PlantillaEnsayos.ReglasDe(generales), r => Assert.Null(r.SiNoAplica));
    }

    /// <summary>
    /// En la 60529 el grado IP objetivo va por muestra, como en el Excel: un mismo
    /// servicio puede traer productos con objetivos distintos. Hay que rellenarlo en
    /// todas las muestras, no en una cualquiera.
    /// </summary>
    [Fact]
    public void EnLa60529ElGradoIpVaPorMuestraYSePideEnTodas()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");

        var primera = ip.Proyecto.Campos.Single(c => c.Id == "ipPrimeraCifra");
        var segunda = ip.Proyecto.Campos.Single(c => c.Id == "ipSegundaCifra");

        Assert.True(primera.PorMuestra);
        Assert.True(segunda.PorMuestra);
        Assert.False(primera.Multiple);
        Assert.Equal(["IP1X", "IP2X", "IP3X", "IP4X", "IP5X", "IP6X"], primera.Opciones);
        Assert.Equal(10, segunda.Opciones.Count);

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");

        // Solo la primera muestra rellena: sigue faltando la segunda.
        datos.Establecer("proyecto", "ipPrimeraCifra", "IP2X", 1);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX4", 1);
        Assert.NotEmpty(RequisitosDelProyecto.Faltantes(ip, datos));

        // IPX5, no IPX7: así el caso queda centrado en el grado por muestra y no
        // arrastra los datos de inmersión, que tienen su propio test.
        datos.Establecer("proyecto", "ipPrimeraCifra", "IP6X", 2);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX5", 2);
        Assert.Empty(RequisitosDelProyecto.Faltantes(ip, datos));
    }

    /// <summary>
    /// Las reglas tienen que ver el grado aunque esté declarado por muestra: si no, los
    /// apartados de ensayo de la 60529 no llegarían a aparecer.
    /// </summary>
    [Fact]
    public void LasReglasVenElGradoIpDeclaradoPorMuestra()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };

        var motorVacio = new MotorDeReglas(ip, datos);
        Assert.False(motorVacio.EsVerdadera("R-60529-hayPrimeraCifra"));
        Assert.False(motorVacio.EsVerdadera("R-60529-haySegundaCifra"));

        datos.Establecer("proyecto", "ipPrimeraCifra", "IP5X", 2);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX4", 2);

        var motor = new MotorDeReglas(ip, datos);
        Assert.True(motor.EsVerdadera("R-60529-hayPrimeraCifra"));
        Assert.True(motor.EsVerdadera("R-60529-haySegundaCifra"));
        Assert.True(motor.EsVerdadera("R-60529-esIp5x"));
        Assert.True(motor.EsVerdadera("R-60529-requiereArco"));
    }

    /// <summary>
    /// La profundidad y la temperatura del agua solo se anotan si alguna muestra se
    /// sumerge, que es lo que distingue al IPX7 y al IPX8.
    /// </summary>
    [Fact]
    public void LosDatosDeInmersionSoloSePidenConIpx7OIpx8()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");

        foreach (var id in new[] { "profundidadInmersion", "tiempoInmersion", "temperaturaInmersion" })
            Assert.Equal("R-60529-inmersion", ip.Proyecto.Campos.Single(c => c.Id == id).VisibleSi);

        Assert.Equal(25d, ip.Proyecto.Campos.Single(c => c.Id == "temperaturaInmersion").NumeroPorDefecto);

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };
        Assert.False(new MotorDeReglas(ip, datos).EsVerdadera("R-60529-inmersion"));

        datos.Establecer("proyecto", "ipSegundaCifra", "IPX5", 1);
        Assert.False(new MotorDeReglas(ip, datos).EsVerdadera("R-60529-inmersion"));

        // Basta con que una muestra se sumerja.
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX8", 2);
        Assert.True(new MotorDeReglas(ip, datos).EsVerdadera("R-60529-inmersion"));
    }

    /// <summary>
    /// Son obligatorios, pero solo cuando aplican: un servicio sin inmersión no puede
    /// quedarse bloqueado pidiendo la profundidad del agua.
    /// </summary>
    [Fact]
    public void LosDatosDeInmersionSoloSeExigenCuandoAplican()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "ipPrimeraCifra", "IP2X", 1);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX4", 1);

        // Sin inmersión, la cabecera está completa aunque falten esos dos datos.
        Assert.Empty(RequisitosDelProyecto.Faltantes(ip, datos));

        // Con IPX8 pasan a exigirse.
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX8", 1);
        var faltan = RequisitosDelProyecto.Faltantes(ip, datos);
        Assert.Contains(faltan, f => f.Contains("Profundidad"));
        Assert.Contains(faltan, f => f.Contains("Tiempo"));
        Assert.Contains(faltan, f => f.Contains("Temperatura"));

        datos.Establecer("proyecto", "profundidadInmersion", 1.0);
        datos.Establecer("proyecto", "tiempoInmersion", 30.0);
        datos.Establecer("proyecto", "temperaturaInmersion", 25.0);
        Assert.Empty(RequisitosDelProyecto.Faltantes(ip, datos));
    }

    /// <summary>
    /// Luminarias declara el grado IP por muestra igual que la 60529, con el mismo atajo
    /// de «luminaria ordinaria». El arco de lluvia usa el objetivo de cada muestra.
    /// </summary>
    [Fact]
    public void EnLuminariasElGradoIpTambienVaPorMuestra()
    {
        var lum = Contexto.Plantilla;

        Assert.True(lum.Proyecto.Campos.Single(c => c.Id == "ipPrimeraCifra").PorMuestra);
        Assert.True(lum.Proyecto.Campos.Single(c => c.Id == "ipSegundaCifra").PorMuestra);
        Assert.True(lum.Proyecto.Campos.Single(c => c.Id == "luminariaOrdinaria").PorMuestra);

        var datos = Contexto.ProyectoVacio(muestras: 2);
        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "ta", 25d);
        datos.Partes2.Add("-2-1");

        datos.Establecer("proyecto", "ipPrimeraCifra", "IP2X", 1);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX0", 1);
        Assert.NotEmpty(RequisitosDelProyecto.Faltantes(lum, datos));   // falta la muestra 2

        datos.Establecer("proyecto", "ipPrimeraCifra", "IP6X", 2);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX4", 2);
        Assert.Empty(RequisitosDelProyecto.Faltantes(lum, datos));
    }

    /// <summary>
    /// La altura efectiva se parte por la mitad solo en las muestras cuyo objetivo es
    /// IPX4, no en todas porque una lo sea.
    /// </summary>
    [Fact]
    public void LaAlturaEfectivaMiraElObjetivoDeCadaMuestra()
    {
        var datos = Contexto.ProyectoVacio(muestras: 2);
        datos.Establecer("generales", "tamano[0].alto", 100d, 1);
        datos.Establecer("generales", "tamano[0].alto", 100d, 2);

        datos.Establecer("proyecto", "ipSegundaCifra", "IPX3", 1);
        datos.Establecer("proyecto", "ipSegundaCifra", "IPX4", 2);

        var motor = Contexto.Motor(datos);

        Assert.Equal(100d, Calculos.AlturaEfectiva(motor, "generales", 1));
        Assert.Equal(50d, Calculos.AlturaEfectiva(motor, "generales", 2));
    }

    /// <summary>
    /// En luminarias el IK se elige por muestra y arranca en «No IK». Su sección solo
    /// aparece cuando alguna muestra lleva un grado de verdad.
    /// </summary>
    [Fact]
    public void LaSeccionDeIkDeLuminariasApareceSoloSiAlgunaMuestraLoLleva()
    {
        var lum = Contexto.Plantilla;

        var campo = lum.Proyecto.Campos.Single(c => c.Id == "gradoIk");
        Assert.True(campo.PorMuestra);
        Assert.False(campo.Obligatorio);
        Assert.Equal("No IK", campo.TextoPorDefecto);
        Assert.Equal("No IK", campo.Opciones[0]);

        var datos = Contexto.ProyectoVacio(muestras: 2);
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-60598-hayIk"));

        // Elegir «No IK» expresamente tampoco lo activa.
        datos.Establecer("proyecto", "gradoIk", "No IK", 1);
        Assert.False(Contexto.Motor(datos).EsVerdadera("R-60598-hayIk"));

        datos.Establecer("proyecto", "gradoIk", "IK08", 2);
        var motor = Contexto.Motor(datos);
        Assert.True(motor.EsVerdadera("R-60598-hayIk"));
        Assert.True(motor.EsVerdadera("R-60598-ik-metodoElegible"));

        Assert.True(EstadoDeApartado.EsVisible(motor, lum.Bloque("60598.ik")));
    }

    /// <summary>
    /// La clase de aislamiento arranca vacía y hay que elegirla. Antes el desplegable
    /// enseñaba «I» sin que nadie lo hubiera elegido, así que un proyecto de Clase II se
    /// guardaba como I si el técnico no caía en tocarlo.
    /// </summary>
    [Fact]
    public void LaClaseSeExigeYNoTieneValorPorDefecto()
    {
        var campo = Contexto.Plantilla.Proyecto.Campos.Single(c => c.Id == "clase");

        Assert.True(campo.Obligatorio);
        Assert.Null(campo.PorDefecto);
        Assert.Equal(["I", "II", "III"], campo.Opciones);

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };
        Assert.Contains(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos), f => f == "Clase");

        datos.Establecer("proyecto", "clase", "II");
        Assert.DoesNotContain(RequisitosDelProyecto.Faltantes(Contexto.Plantilla, datos), f => f == "Clase");
    }

    /// <summary>
    /// La fila de cada muestra es la misma en las tres normas que la usan: casilla de
    /// luminaria ordinaria, las dos cifras del IP y el grado IK, en ese orden.
    /// </summary>
    [Theory]
    [InlineData("60598")]
    [InlineData("60529")]
    [InlineData("62262")]
    public void LaFilaDeMuestraEsIgualEnTodasLasNormas(string norma)
    {
        var porMuestra = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == norma)
            .Proyecto.Campos.Where(c => c.PorMuestra).Select(c => c.Id).ToList();

        Assert.Equal(["luminariaOrdinaria", "ipPrimeraCifra", "ipSegundaCifra", "gradoIk"], porMuestra);
    }

    /// <summary>
    /// <b>Toda</b> norma que deje marcar la segunda cifra del IP tiene que pedir los
    /// datos de inmersión igual que las demás: los tres campos, obligatorios, visibles
    /// solo con objetivo IPX7 o IPX8 y con el agua a 25 ºC por defecto.
    /// <para>
    /// Se comprueba sobre las plantillas instaladas, no sobre una lista escrita a mano:
    /// una norma nueva que declare cifras IP queda cubierta sin tocar este test. Antes
    /// era una lista y la 62262 se quedó fuera, así que marcar IPX8 allí no pedía nada.
    /// </para>
    /// </summary>
    [Fact]
    public void TodaNormaConGradoIpPideLosDatosDeInmersion()
    {
        string[] deInmersion = ["profundidadInmersion", "tiempoInmersion", "temperaturaInmersion"];
        var fallos = new List<string>();

        var conIp = Contexto.TodasLasPlantillas()
            .Where(p => p.Proyecto.Campos.Any(c => c.Id == "ipSegundaCifra"))
            .ToList();

        Assert.NotEmpty(conIp);   // si no hay ninguna, el test no estaría comprobando nada

        foreach (var plantilla in conIp)
        {
            var norma = plantilla.Meta.Id;

            foreach (var id in deInmersion)
            {
                var campo = plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == id);

                if (campo is null) { fallos.Add($"{norma}: falta «{id}»"); continue; }
                if (!campo.Obligatorio) fallos.Add($"{norma}.{id}: debería ser obligatorio");
                if (campo.VisibleSi is null) fallos.Add($"{norma}.{id}: sin «visibleSi»");
            }

            if (plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == "temperaturaInmersion")
                is { NumeroPorDefecto: not 25d })
                fallos.Add($"{norma}: el agua debería venir a 25 ºC por defecto");

            // Y la condición tiene que ser la inmersión de verdad, no una regla cualquiera.
            if (plantilla.Proyecto.Campos.FirstOrDefault(c => c.Id == "profundidadInmersion")?.VisibleSi
                is not { } regla) continue;

            var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 2 };
            if (new MotorDeReglas(plantilla, datos).EsVerdadera(regla))
                fallos.Add($"{norma}: pide inmersión sin que ninguna muestra la lleve");

            datos.Establecer("proyecto", "ipSegundaCifra", "IPX8", 2);
            if (!new MotorDeReglas(plantilla, datos).EsVerdadera(regla))
                fallos.Add($"{norma}: no pide inmersión con una muestra en IPX8");
        }

        Assert.Empty(fallos);
    }

    /// <summary>
    /// Solo la norma con la que nació el proyecto decide cómo se nombran las muestras:
    /// añadir otra no puede renombrarlas.
    /// </summary>
    [Fact]
    public void UnaNormaAnadidaNoCambiaElPrefijoDeLasMuestras()
    {
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };

        ik.AplicarA(datos);
        ip.AplicarA(datos, principal: false);

        Assert.Equal("EBP_CLIM12345202601", datos.IdentificadorDeMuestra(1));
        Assert.Contains("60529", datos.Normas);

        // Y la principal es la que se aplicó como tal, no la última en llegar.
        Assert.Equal("62262", datos.NormaPrincipal);
    }

    // ---- cuál es la principal la dice el proyecto --------------------------

    /// <summary>
    /// El proyecto <b>apunta</b> con qué norma nació, en vez de dejar que se deduzca
    /// después del patrón de muestras.
    /// <para>
    /// El caso que lo obligaba: IP 60529 y módulos LED 62031 <b>comparten patrón</b>
    /// —las dos nombran <c>EBP_SAFE…</c>—, así que deducirlo no daba respuesta y se
    /// acababa decidiendo por orden alfabético, que no significa nada.
    /// </para>
    /// </summary>
    [Fact]
    public void ConDosNormasDelMismoPatronMandaLaQueElProyectoApunto()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");
        var led = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62031");

        // Nace como módulos LED, con el IP añadido detrás.
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };
        led.AplicarA(datos);
        ip.AplicarA(datos, principal: false);

        Assert.Equal("62031", datos.NormaPrincipal);

        // El tablero detalla la 62031 y resume la 60529 en una línea, se le pasen las
        // plantillas en el orden que se le pasen.
        foreach (var orden in new[] { new[] { ip, led }, [led, ip] })
        {
            var resumen = AnalizadorDeProyectos.Analizar(orden, datos, "x.lumproj", DateTime.Now);
            Assert.Contains(resumen.SeccionesPendientes, s => s.Titulo == ip.Meta.Titulo);
        }
    }

    /// <summary>
    /// Si la norma principal se quita del servicio desde la toma de notas, no puede
    /// seguir detallándose una norma que el proyecto ya no lleva: se vuelve a deducir.
    /// </summary>
    [Fact]
    public void SiLaPrincipalYaNoEstaEntreLasSuyasSeVuelveADeducir()
    {
        var luminarias = Contexto.Plantilla;
        var led = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62031");

        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };
        led.AplicarA(datos);
        luminarias.AplicarA(datos, principal: false);

        // Se retira la 62031 del servicio, pero el proyecto sigue apuntándola.
        datos.Normas.Remove("62031");
        Assert.Equal("62031", datos.NormaPrincipal);

        // No se cae ni detalla la que ya no está: manda luminarias, como siempre.
        var resumen = AnalizadorDeProyectos.Analizar([luminarias], datos, "x.lumproj", DateTime.Now);
        Assert.Null(resumen.Error);
        Assert.NotEmpty(resumen.SeccionesPendientes);
    }

    /// <summary>Solo luminarias declara clase de aislamiento; el resto no la pide.</summary>
    [Fact]
    public void SoloLuminariasPideClaseDeAislamiento()
    {
        var conClase = Contexto.TodasLasPlantillas()
            .Where(p => p.Proyecto.Campos.Any(c => c.Id == "clase"))
            .Select(p => p.Meta.Id)
            .ToList();

        Assert.Equal(["60598"], conClase);
    }

    [Fact]
    public void LasNormasCompatiblesDeclaradasExisten()
    {
        var instaladas = Contexto.TodasLasPlantillas().Select(p => p.Meta.Id).ToHashSet();

        var rotas = Contexto.TodasLasPlantillas()
            .SelectMany(p => (p.Meta.NormasCompatibles ?? []).Select(c => (Norma: p.Meta.Id, Compatible: c)))
            .Where(x => !instaladas.Contains(x.Compatible))
            .Select(x => $"{x.Norma} → {x.Compatible}")
            .ToList();

        Assert.Empty(rotas);
    }

    [Fact]
    public void ElAvanceSumaElDeTodasLasNormasDelProyecto()
    {
        var lum = Contexto.Plantilla;
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };

        var soloLum = new IndicadorDeAvance(new MotorDeReglas(lum, datos)).Calcular();
        var soloIk = new IndicadorDeAvance(new MotorDeReglas(ik, datos)).Calcular();
        var juntos = IndicadorDeAvance.Resultado.Sumar([soloLum, soloIk]);

        Assert.Equal(soloLum.ApartadosAplicables + soloIk.ApartadosAplicables, juntos.ApartadosAplicables);
        Assert.Equal(soloLum.PesoTotal + soloIk.PesoTotal, juntos.PesoTotal);
        Assert.True(juntos.ApartadosAplicables > soloLum.ApartadosAplicables);
    }

    /// <summary>
    /// Cada norma decide qué es obligatorio en su cabecera: la 60529 no pide Ta ni clase
    /// de aislamiento, que son de luminarias, y la de IK pide el grado IK objetivo.
    /// </summary>
    [Fact]
    public void CadaNormaExigeSuPropiaCabecera()
    {
        var ip = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "60529");
        var ik = Contexto.TodasLasPlantillas().Single(p => p.Meta.Id == "62262");
        var datos = new DatosProyecto { CodigoServicio = "123452026", NumeroMuestras = 1 };

        var faltanIp = RequisitosDelProyecto.Faltantes(ip, datos);
        var faltanIk = RequisitosDelProyecto.Faltantes(ik, datos);

        Assert.DoesNotContain(faltanIp, f => f.Contains("Ta"));
        Assert.DoesNotContain(faltanIp, f => f.Contains("Clase"));
        Assert.Contains(faltanIp, f => f.Contains("1ª cifra"));
        Assert.Contains(faltanIk, f => f.Contains("IK"));

        datos.Establecer("proyecto", "tecnico1", "D. Martínez");
        datos.Establecer("proyecto", "gradoIk", "IK08", 1);
        datos.Partes2.Add("OTRO");

        Assert.Empty(RequisitosDelProyecto.Faltantes(ik, datos));
    }
}
