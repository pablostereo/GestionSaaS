import { useEffect, useState } from 'react'
import { api, fechaCorta, hoyISO, numero, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Cargando, Encabezado, Etiqueta, Indicador, Vacio } from '../../componentes/Comunes.jsx'
import GraficoBarras from '../../componentes/GraficoBarras.jsx'
import Icono from '../../componentes/Icono.jsx'

// --- Rango de fechas (en hora local) ---
const iso = (d) => d.toLocaleDateString('sv')
const haceDias = (n) => { const d = new Date(); d.setDate(d.getDate() - n); return iso(d) }
const inicioDelMes = () => { const d = new Date(); return iso(new Date(d.getFullYear(), d.getMonth(), 1)) }

const RANGOS = {
  '7d': { titulo: 'Últimos 7 días', calcular: () => ({ desde: haceDias(6), hasta: hoyISO() }) },
  mes: { titulo: 'Este mes', calcular: () => ({ desde: inicioDelMes(), hasta: hoyISO() }) },
  '30d': { titulo: 'Últimos 30 días', calcular: () => ({ desde: haceDias(29), hasta: hoyISO() }) },
  otro: { titulo: 'Personalizado' }
}

const pesosCompacto = (n) =>
  '$ ' + Number(n).toLocaleString('es-AR', { notation: n >= 10000 ? 'compact' : 'standard', maximumFractionDigits: 1 })
const porcentaje = (parte, total) =>
  total ? `${((parte / total) * 100).toLocaleString('es-AR', { maximumFractionDigits: 1 })}%` : '—'
const plural = (n, uno, varios) => `${numero(n)} ${n === 1 ? uno : varios}`

export default function Estadisticas({ irA }) {
  const avisar = useAvisos()
  const [rango, setRango] = useState('30d')
  const [fechas, setFechas] = useState(RANGOS['30d'].calcular())
  const [metrica, setMetrica] = useState('monto')   // 'monto' | 'cantidad'
  const [datos, setDatos] = useState(null)
  const [cargando, setCargando] = useState(false)

  useEffect(() => {
    if (!fechas.desde || !fechas.hasta) return
    setCargando(true)
    api.estadisticas(fechas)
      .then(setDatos)
      .catch((e) => avisar(e.message, 'error'))
      .finally(() => setCargando(false))
  }, [fechas])

  const elegirRango = (clave) => {
    setRango(clave)
    if (RANGOS[clave].calcular) setFechas(RANGOS[clave].calcular())
  }

  return (
    <>
      <Encabezado titulo="Estadísticas" subtitulo="Cómo viene tu comercio: ventas, horarios, productos y resultado">
        {cargando && datos && <span className="texto-suave">Actualizando…</span>}
      </Encabezado>

      <div className="barra-filtros">
        <div className="selector">
          {Object.entries(RANGOS).map(([clave, r]) => (
            <button key={clave} type="button" className={rango === clave ? 'activo' : ''} onClick={() => elegirRango(clave)}>{r.titulo}</button>
          ))}
        </div>
        {rango === 'otro' && (
          <>
            <label className="campo-linea">Desde <input type="date" value={fechas.desde} max={fechas.hasta || hoyISO()} onChange={(e) => setFechas({ ...fechas, desde: e.target.value })} /></label>
            <label className="campo-linea">Hasta <input type="date" value={fechas.hasta} min={fechas.desde} max={hoyISO()} onChange={(e) => setFechas({ ...fechas, hasta: e.target.value })} /></label>
          </>
        )}
        {datos && <span className="texto-suave">{fechaCorta(datos.desde + 'T12:00')} al {fechaCorta(datos.hasta + 'T12:00')}</span>}
      </div>

      {!datos ? <Cargando /> : (
        <div className={cargando ? 'atenuado' : ''}>
          <Balance balance={datos.balance} />

          {datos.balance.cantidadVentas === 0 ? (
            <div className="tarjeta vacio-grande">
              <Icono nombre="estadisticas" tamano={28} />
              <p>No hay ventas en este período. Cuando registres ventas vas a ver acá los días y horarios fuertes y el ranking de productos.</p>
              <button className="btn btn-primario" onClick={() => irA('nuevaVenta')}>Registrar una venta</button>
            </div>
          ) : (
            <>
              <div className="fila-titulo">
                <h3>¿Cuándo vendés más?</h3>
                <div className="selector selector-chico">
                  <button type="button" className={metrica === 'monto' ? 'activo' : ''} onClick={() => setMetrica('monto')}>Monto</button>
                  <button type="button" className={metrica === 'cantidad' ? 'activo' : ''} onClick={() => setMetrica('cantidad')}>Cantidad de ventas</button>
                </div>
              </div>
              <div className="columnas">
                <section className="tarjeta">
                  <h3>Por día de la semana</h3>
                  <GraficoVentas filas={datos.porDiaSemana} metrica={metrica} etiqueta={(d) => d.nombre.slice(0, 3)} clave={(d) => d.dia} />
                </section>
                <section className="tarjeta">
                  <h3>Por horario <span className="texto-suave">(franjas de 2 horas)</span></h3>
                  <GraficoVentas filas={datos.porFranjaHoraria} metrica={metrica} etiqueta={(f) => f.etiqueta} etiquetaCorta={(f) => String(f.horaDesde).padStart(2, '0')} clave={(f) => f.horaDesde} densa />
                </section>
              </div>

              <div className="columnas espacio-arriba">
                <section className="tarjeta">
                  <h3>Top 5 más rentables</h3>
                  <p className="texto-suave nota">Ganancia = (precio de venta − costo al momento de vender) × unidades.</p>
                  <div className="tabla-scroll"><table className="tabla-compacta">
                    <thead><tr><th>Producto</th><th className="num">Unid.</th><th className="num">Ganancia</th><th className="num">Margen</th></tr></thead>
                    <tbody>
                      {datos.topRentables.map((p) => (
                        <tr key={p.productoId}>
                          <td><span className="celda-principal">{p.nombre}</span> <code>{p.codigoSku}</code></td>
                          <td className="num">{numero(p.unidades)}</td>
                          <td className={`num ${p.ganancia < 0 ? 'texto-rojo' : ''}`}>{pesos(p.ganancia)}</td>
                          <td className="num">{p.margenPorcentaje == null ? '—' : `${numero(p.margenPorcentaje)}%`}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table></div>
                </section>
                <section className="tarjeta">
                  <h3>Top 5 más vendidos</h3>
                  <p className="texto-suave nota">Por unidades vendidas en el período.</p>
                  <div className="tabla-scroll"><table className="tabla-compacta">
                    <thead><tr><th>Producto</th><th className="num">Unid.</th><th className="num">Facturado</th></tr></thead>
                    <tbody>
                      {datos.topVendidos.map((p) => (
                        <tr key={p.productoId}>
                          <td><span className="celda-principal">{p.nombre}</span> <code>{p.codigoSku}</code></td>
                          <td className="num">{numero(p.unidades)}</td>
                          <td className="num">{pesos(p.ingresos)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table></div>
                </section>
              </div>
            </>
          )}

          <SinRotacion productos={datos.sinMovimiento} />
        </div>
      )}
    </>
  )
}

function Balance({ balance: b }) {
  const positivo = b.resultadoNeto >= 0
  return (
    <>
      <div className="indicadores">
        <Indicador titulo="Ventas" valor={pesos(b.ingresos)} icono="ventas" tono="azul"
                   detalle={`${plural(b.cantidadVentas, 'venta', 'ventas')} · ticket ${pesos(b.ticketPromedio)}`} />
        <Indicador titulo="Costo de mercadería vendida" valor={pesos(b.costoMercaderiaVendida)} icono="productos" tono="violeta"
                   detalle={`Ganancia bruta ${pesos(b.gananciaBruta)}`} />
        <Indicador titulo="Gastos operativos" valor={pesos(b.gastosOperativos)} icono="gastos" tono="rojo"
                   detalle={`${porcentaje(b.gastosOperativos, b.ingresos)} de las ventas`} />
        <Indicador titulo={positivo ? 'Ganancia neta' : 'Pérdida neta'} valor={pesos(b.resultadoNeto)} icono="estadisticas"
                   tono={positivo ? 'verde' : 'rojo'} detalle={`Margen neto ${porcentaje(b.resultadoNeto, b.ingresos)}`} />
      </div>
      <p className="texto-suave nota">
        Resultado estimado = Ventas − Costo de mercadería vendida − Gastos operativos.
        El costo de cada producto es el que tenía al momento de la venta.
      </p>
    </>
  )
}

function GraficoVentas({ filas, metrica, etiqueta, etiquetaCorta, clave, densa }) {
  const esMonto = metrica === 'monto'
  const datos = filas.map((f) => ({
    clave: clave(f),
    etiqueta: etiqueta(f),
    etiquetaCorta: etiquetaCorta?.(f),
    valor: esMonto ? f.monto : f.cantidad,
    detalle: esMonto ? plural(f.cantidad, 'venta', 'ventas') : pesos(f.monto)
  }))
  return (
    <GraficoBarras datos={datos} densa={densa}
                   nombreValor={esMonto ? 'Monto' : 'Ventas'}
                   formato={esMonto ? pesos : (n) => plural(n, 'venta', 'ventas')}
                   formatoEje={esMonto ? pesosCompacto : numero} />
  )
}

function SinRotacion({ productos }) {
  const inmovilizado = productos.reduce((s, p) => s + p.valorInmovilizado, 0)
  return (
    <section className="tarjeta espacio-arriba">
      <div className="tarjeta-cabecera">
        <h3><Icono nombre="alerta" tamano={16} /> Productos sin rotación</h3>
        {productos.length > 0 && <Etiqueta tono="naranja">{pesos(inmovilizado)} inmovilizados</Etiqueta>}
      </div>
      <p className="texto-suave nota">Tienen stock y no se vendieron en el período. Pensá en promociones o combos para liquidarlos.</p>
      <div className="tabla-scroll">
        <table className="tabla-compacta">
          <thead><tr><th>Producto</th><th className="num">Stock</th><th className="num">Valor al costo</th><th>Última venta</th></tr></thead>
          <tbody>
            {productos.map((p) => (
              <tr key={p.productoId}>
                <td><span className="celda-principal">{p.nombre}</span> <code>{p.codigoSku}</code></td>
                <td className="num">{numero(p.stockActual)}</td>
                <td className="num">{pesos(p.valorInmovilizado)}</td>
                <td>{p.ultimaVenta ? fechaCorta(p.ultimaVenta) : <span className="texto-suave">Nunca</span>}</td>
              </tr>
            ))}
            {productos.length === 0 && <Vacio columnas={4}>Todos los productos con stock tuvieron ventas en el período 👍</Vacio>}
          </tbody>
        </table>
      </div>
    </section>
  )
}
