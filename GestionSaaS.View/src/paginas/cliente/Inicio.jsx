import { useEffect, useState } from 'react'
import { api, fecha, MEDIOS_PAGO, numero, pesos } from '../../api.js'
import { Cargando, Encabezado, Indicador, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'

const inicioDelMes = () => { const d = new Date(); return new Date(d.getFullYear(), d.getMonth(), 1) }
const inicioDelDia = () => { const d = new Date(); d.setHours(0, 0, 0, 0); return d }

export default function Inicio({ irA }) {
  const [datos, setDatos] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    Promise.all([api.ventas(), api.compras(), api.productos()])
      .then(([ventas, compras, productos]) => setDatos({ ventas, compras, productos }))
      .catch((e) => setError(e.message))
  }, [])

  if (error) return <div className="alerta alerta-error">{error}</div>
  if (!datos) return <Cargando />

  const { ventas, compras, productos } = datos
  const mes = inicioDelMes(), hoy = inicioDelDia()
  const ventasHoy = ventas.filter((v) => new Date(v.fecha) >= hoy)
  const ventasMes = ventas.filter((v) => new Date(v.fecha) >= mes)
  const comprasMes = compras.filter((c) => new Date(c.fecha) >= mes)
  const stockBajo = productos.filter((p) => p.stockBajo)
  const suma = (lista, campo) => lista.reduce((s, x) => s + x[campo], 0)

  return (
    <>
      <Encabezado titulo="Inicio" subtitulo="Resumen de tu comercio">
        <button className="btn btn-primario" onClick={() => irA('nuevaVenta')}><Icono nombre="venta" /> Nueva venta</button>
      </Encabezado>

      <div className="indicadores">
        <Indicador titulo="Vendido hoy" valor={pesos(suma(ventasHoy, 'totalVenta'))} detalle={`${ventasHoy.length} ${ventasHoy.length === 1 ? "venta" : "ventas"}`} icono="venta" tono="verde" />
        <Indicador titulo="Vendido en el mes" valor={pesos(suma(ventasMes, 'totalVenta'))} detalle={`${ventasMes.length} ${ventasMes.length === 1 ? "venta" : "ventas"}`} icono="ventas" />
        <Indicador titulo="Compras del mes" valor={pesos(suma(comprasMes, 'montoTotal'))} detalle={`${comprasMes.length} ${comprasMes.length === 1 ? "compra" : "compras"}`} icono="compras" tono="violeta" />
        <Indicador titulo="Stock bajo" valor={stockBajo.length} detalle={`de ${productos.length} productos`} icono="alerta" tono={stockBajo.length ? 'rojo' : 'verde'} />
      </div>

      <div className="columnas">
        <section className="tarjeta">
          <div className="tarjeta-cabecera">
            <h3>Productos para reponer</h3>
            <button className="btn btn-enlace" onClick={() => irA('compras')}>Registrar compra →</button>
          </div>
          <table>
            <thead><tr><th>Producto</th><th className="num">Stock</th><th className="num">Mínimo</th></tr></thead>
            <tbody>
              {stockBajo.map((p) => (
                <tr key={p.id}><td>{p.nombre}</td><td className="num texto-rojo">{numero(p.stockActual)}</td><td className="num">{numero(p.stockMinimo)}</td></tr>
              ))}
              {stockBajo.length === 0 && <Vacio columnas={3}>Todo el stock está por encima del mínimo 👍</Vacio>}
            </tbody>
          </table>
        </section>

        <section className="tarjeta">
          <div className="tarjeta-cabecera">
            <h3>Últimas ventas</h3>
            <button className="btn btn-enlace" onClick={() => irA('ventas')}>Ver todas →</button>
          </div>
          <table>
            <thead><tr><th>Fecha</th><th>Medio</th><th className="num">Total</th></tr></thead>
            <tbody>
              {ventas.slice(0, 6).map((v) => (
                <tr key={v.id}><td>{fecha(v.fecha)}</td><td>{MEDIOS_PAGO[v.medioPago]}</td><td className="num">{pesos(v.totalVenta)}</td></tr>
              ))}
              {ventas.length === 0 && <Vacio columnas={3}>Todavía no hay ventas</Vacio>}
            </tbody>
          </table>
        </section>
      </div>
    </>
  )
}
