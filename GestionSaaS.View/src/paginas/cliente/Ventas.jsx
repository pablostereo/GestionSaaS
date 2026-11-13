import { Fragment, useEffect, useState } from 'react'
import { api, fecha, MEDIOS_PAGO, numero, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Cargando, Encabezado, Indicador, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'

export default function Ventas({ irA }) {
  const avisar = useAvisos()
  const [ventas, setVentas] = useState(null)
  const [desde, setDesde] = useState('')
  const [hasta, setHasta] = useState('')
  const [abierta, setAbierta] = useState(null)

  useEffect(() => {
    api.ventas({ desde, hasta }).then(setVentas).catch((e) => avisar(e.message, 'error'))
  }, [desde, hasta])

  const total = ventas?.reduce((s, v) => s + v.totalVenta, 0) ?? 0
  const ganancia = ventas?.reduce((s, v) => s + v.ganancia, 0) ?? 0
  const porMedio = Object.entries((ventas ?? []).reduce((m, v) => ({ ...m, [v.medioPago]: (m[v.medioPago] ?? 0) + v.totalVenta }), {}))
    .sort((a, b) => b[1] - a[1])

  return (
    <>
      <Encabezado titulo="Ventas" subtitulo="Historial de ventas">
        <button className="btn btn-primario" onClick={() => irA('nuevaVenta')}><Icono nombre="mas" /> Nueva venta</button>
      </Encabezado>

      <div className="barra-filtros">
        <label className="campo-linea">Desde <input type="date" value={desde} onChange={(e) => setDesde(e.target.value)} /></label>
        <label className="campo-linea">Hasta <input type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} /></label>
        {(desde || hasta) && <button className="btn btn-enlace" onClick={() => { setDesde(''); setHasta('') }}>Limpiar</button>}
      </div>

      {!ventas ? <Cargando /> : (
        <>
          <div className="indicadores">
            <Indicador titulo="Total del período" valor={pesos(total)} detalle={`${ventas.length} ${ventas.length === 1 ? "venta" : "ventas"}`} icono="ventas" tono="verde" />
            <Indicador titulo="Ganancia bruta" valor={pesos(ganancia)} detalle={`Margen ${total ? ((ganancia / total) * 100).toLocaleString('es-AR', { maximumFractionDigits: 1 }) : 0}%`} icono="estadisticas" tono={ganancia >= 0 ? 'verde' : 'rojo'} />
            <Indicador titulo="Ticket promedio" valor={pesos(ventas.length ? total / ventas.length : 0)} icono="stock" />
            {porMedio[0] && <Indicador titulo="Medio más usado" valor={MEDIOS_PAGO[porMedio[0][0]]} detalle={pesos(porMedio[0][1])} icono="venta" tono="violeta" />}
          </div>

          <div className="tarjeta sin-relleno">
            <div className="tabla-scroll">
              <table>
                <thead><tr><th>#</th><th>Fecha</th><th>Medio de pago</th><th className="num">Productos</th><th className="num">Ganancia</th><th className="num">Total</th><th></th></tr></thead>
                <tbody>
                  {ventas.map((v) => (
                    <Fragment key={v.id}>
                      <tr className="fila-clic" onClick={() => setAbierta(abierta === v.id ? null : v.id)}>
                        <td>{v.id}</td><td>{fecha(v.fecha)}</td><td>{MEDIOS_PAGO[v.medioPago]}</td>
                        <td className="num">{v.items.length}</td>
                        <td className={`num ${v.ganancia < 0 ? 'texto-rojo' : ''}`}>{pesos(v.ganancia)}</td>
                        <td className="num celda-principal">{pesos(v.totalVenta)}</td>
                        <td className="num texto-suave">{abierta === v.id ? '▲' : '▼'}</td>
                      </tr>
                      {abierta === v.id && (
                        <tr className="fila-detalle">
                          <td colSpan={7}>
                            <table>
                              <thead>
                                <tr><th>Producto</th><th className="num">Cant. × precio</th><th className="num">Costo unit. (al vender)</th><th className="num">Ganancia</th><th className="num">Subtotal</th></tr>
                              </thead>
                              <tbody>
                                {v.items.map((i) => (
                                  <tr key={i.productoId}>
                                    <td>{i.producto}</td>
                                    <td className="num">{numero(i.cantidad)} × {pesos(i.precioUnitario)}</td>
                                    <td className="num">{pesos(i.costoUnitario)}</td>
                                    <td className={`num ${i.ganancia < 0 ? 'texto-rojo' : ''}`}>{pesos(i.ganancia)}</td>
                                    <td className="num">{pesos(i.subtotal)}</td>
                                  </tr>
                                ))}
                              </tbody>
                            </table>
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  ))}
                  {ventas.length === 0 && <Vacio columnas={7}>No hay ventas en el período</Vacio>}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </>
  )
}
