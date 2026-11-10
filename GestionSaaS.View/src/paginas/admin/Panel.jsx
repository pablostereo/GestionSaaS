import { useEffect, useState } from 'react'
import { api, fecha, pesos } from '../../api.js'
import { Cargando, Encabezado, Etiqueta, Indicador, Vacio } from '../../componentes/Comunes.jsx'

export default function Panel({ irA }) {
  const [clientes, setClientes] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => { api.clientes().then(setClientes).catch((e) => setError(e.message)) }, [])

  if (error) return <div className="alerta alerta-error">{error}</div>
  if (!clientes) return <Cargando />

  const activos = clientes.filter((c) => c.activo).length
  const totalVendido = clientes.reduce((s, c) => s + c.totalVendido, 0)
  const cantidadVentas = clientes.reduce((s, c) => s + c.cantidadVentas, 0)
  const ranking = [...clientes].sort((a, b) => b.totalVendido - a.totalVendido).slice(0, 5)
  const ingresos = clientes.filter((c) => c.ultimoIngreso)
    .sort((a, b) => new Date(b.ultimoIngreso) - new Date(a.ultimoIngreso)).slice(0, 5)

  return (
    <>
      <Encabezado titulo="Panel" subtitulo="Resumen de todos los clientes del sistema">
        <button className="btn btn-primario" onClick={() => irA('clientes')}>Gestionar clientes</button>
      </Encabezado>

      <div className="indicadores">
        <Indicador titulo="Clientes" valor={clientes.length} detalle={`${activos} activos`} icono="clientes" />
        <Indicador titulo="Dados de baja" valor={clientes.length - activos} icono="encender" tono="rojo" />
        <Indicador titulo="Ventas registradas" valor={cantidadVentas} icono="ventas" tono="violeta" />
        <Indicador titulo="Total vendido" valor={pesos(totalVendido)} icono="stock" tono="verde" />
      </div>

      <div className="columnas">
        <section className="tarjeta">
          <h3>Clientes con más ventas</h3>
          <table>
            <thead><tr><th>Comercio</th><th className="num">Ventas</th><th className="num">Total</th></tr></thead>
            <tbody>
              {ranking.map((c) => (
                <tr key={c.id}>
                  <td>{c.nombreComercio} {!c.activo && <Etiqueta tono="rojo">Baja</Etiqueta>}</td>
                  <td className="num">{c.cantidadVentas}</td>
                  <td className="num">{pesos(c.totalVendido)}</td>
                </tr>
              ))}
              {ranking.length === 0 && <Vacio columnas={3}>Sin clientes</Vacio>}
            </tbody>
          </table>
        </section>

        <section className="tarjeta">
          <h3>Últimos ingresos</h3>
          <table>
            <thead><tr><th>Comercio</th><th>Último ingreso</th></tr></thead>
            <tbody>
              {ingresos.map((c) => (
                <tr key={c.id}><td>{c.nombreComercio}</td><td>{fecha(c.ultimoIngreso)}</td></tr>
              ))}
              {ingresos.length === 0 && <Vacio columnas={2}>Ningún cliente ingresó todavía</Vacio>}
            </tbody>
          </table>
        </section>
      </div>
    </>
  )
}
