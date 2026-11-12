import { Fragment, useEffect, useState } from 'react'
import { api, fecha, numero, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Indicador, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

// Compras = reposición de mercadería. Suman stock y actualizan el costo de cada producto.
// Los pagos operativos (luz, alquiler…) van en la pantalla Gastos.
export default function Compras({ irA }) {
  const avisar = useAvisos()
  const [compras, setCompras] = useState(null)
  const [proveedores, setProveedores] = useState([])
  const [filtros, setFiltros] = useState({ desde: '', hasta: '', proveedorId: '' })
  const [nueva, setNueva] = useState(false)
  const [abierta, setAbierta] = useState(null)

  const cargar = () => api.compras(filtros).then(setCompras).catch((e) => avisar(e.message, 'error'))
  useEffect(() => { cargar() }, [filtros])
  useEffect(() => { api.proveedores().then(setProveedores).catch(() => {}) }, [])

  const filtrar = (campo) => (e) => setFiltros({ ...filtros, [campo]: e.target.value })
  const hayFiltros = filtros.desde || filtros.hasta || filtros.proveedorId
  const total = compras?.reduce((s, c) => s + c.montoTotal, 0) ?? 0

  return (
    <>
      <Encabezado titulo="Compras" subtitulo="Reposición de mercadería: suma stock y actualiza costos">
        <button className="btn btn-primario" onClick={() => setNueva(true)}><Icono nombre="mas" /> Nueva compra</button>
      </Encabezado>

      <div className="barra-filtros">
        <label className="campo-linea">Desde <input type="date" value={filtros.desde} onChange={filtrar('desde')} /></label>
        <label className="campo-linea">Hasta <input type="date" value={filtros.hasta} onChange={filtrar('hasta')} /></label>
        <select value={filtros.proveedorId} onChange={filtrar('proveedorId')}>
          <option value="">Todos los proveedores</option>
          {proveedores.map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}
        </select>
        {hayFiltros && <button className="btn btn-enlace" onClick={() => setFiltros({ desde: '', hasta: '', proveedorId: '' })}>Limpiar</button>}
      </div>

      {!compras ? <Cargando /> : (
        <>
          <div className="indicadores">
            <Indicador titulo="Total comprado" valor={pesos(total)} detalle={`${compras.length} ${compras.length === 1 ? 'compra' : 'compras'}`} icono="compras" tono="violeta" />
            <Indicador titulo="Compra promedio" valor={pesos(compras.length ? total / compras.length : 0)} icono="stock" />
          </div>

          <div className="tarjeta sin-relleno">
            <div className="tabla-scroll">
              <table>
                <thead><tr><th>#</th><th>Fecha</th><th>Proveedor</th><th>Comprobante</th><th className="num">Productos</th><th className="num">Total</th><th></th></tr></thead>
                <tbody>
                  {compras.map((c) => (
                    <Fragment key={c.id}>
                      <tr className="fila-clic" onClick={() => setAbierta(abierta === c.id ? null : c.id)}>
                        <td>{c.id}</td><td>{fecha(c.fecha)}</td>
                        <td className="celda-principal">{c.proveedor}</td>
                        <td>{c.numeroComprobante || <span className="texto-suave">—</span>}</td>
                        <td className="num">{c.items.length}</td>
                        <td className="num celda-principal">{pesos(c.montoTotal)}</td>
                        <td className="num texto-suave">{abierta === c.id ? '▲' : '▼'}</td>
                      </tr>
                      {abierta === c.id && (
                        <tr className="fila-detalle">
                          <td colSpan={7}>
                            {c.observaciones && <p className="texto-suave">{c.observaciones}</p>}
                            <table>
                              <tbody>
                                {c.items.map((i) => (
                                  <tr key={i.productoId}>
                                    <td>{i.producto}</td>
                                    <td className="num">{numero(i.cantidad)} × {pesos(i.costoUnitario)}</td>
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
                  {compras.length === 0 && <Vacio columnas={7}>{hayFiltros ? 'No hay compras con esos filtros' : 'Todavía no hay compras'}</Vacio>}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}

      {nueva && (
        <FormularioCompra
          onCerrar={() => setNueva(false)}
          onIrAProveedores={() => irA('proveedores')}
          onGuardado={(msg) => { avisar(msg); setNueva(false); cargar() }} />
      )}
    </>
  )
}

function FormularioCompra({ onCerrar, onGuardado, onIrAProveedores }) {
  const [productos, setProductos] = useState([])
  const [proveedores, setProveedores] = useState(null)
  const [proveedorId, setProveedorId] = useState('')
  const [comprobante, setComprobante] = useState('')
  const [observaciones, setObservaciones] = useState('')
  const [items, setItems] = useState([])               // [{ producto, cantidad, costoUnitario }]
  const [nuevo, setNuevo] = useState({ productoId: '', cantidad: '1', costoUnitario: '' })
  const [error, setError] = useState('')

  useEffect(() => {
    Promise.all([api.productos(), api.proveedores({ soloActivos: true })])
      .then(([prod, prov]) => { setProductos(prod); setProveedores(prov) })
      .catch((e) => setError(e.message))
  }, [])

  const total = items.reduce((s, i) => s + i.cantidad * i.costoUnitario, 0)

  const elegirProducto = (id) => {
    const p = productos.find((x) => x.id === +id)
    setNuevo({ ...nuevo, productoId: id, costoUnitario: p ? String(p.costoActual) : '' })
  }

  const agregar = () => {
    const producto = productos.find((p) => p.id === +nuevo.productoId)
    if (!producto || !(+nuevo.cantidad > 0) || nuevo.costoUnitario === '') return
    setItems([...items.filter((i) => i.producto.id !== producto.id),
              { producto, cantidad: +nuevo.cantidad, costoUnitario: +nuevo.costoUnitario }])
    setNuevo({ productoId: '', cantidad: '1', costoUnitario: '' })
  }

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    try {
      const compra = await api.registrarCompra({
        proveedorId: +proveedorId,
        numeroComprobante: comprobante,
        observaciones,
        items: items.map((i) => ({ productoId: i.producto.id, cantidad: i.cantidad, costoUnitario: i.costoUnitario }))
      })
      onGuardado(`Compra #${compra.id} a ${compra.proveedor} registrada: ${pesos(compra.montoTotal)} — stock actualizado`)
    } catch (err) { setError(err.message) }
  }

  if (proveedores && proveedores.length === 0) {
    return (
      <Modal titulo="Nueva compra" onCerrar={onCerrar}>
        <div className="alerta">Para registrar una compra primero tenés que cargar al menos un proveedor activo.</div>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button type="button" className="btn btn-primario" onClick={onIrAProveedores}>Ir a Proveedores</button>
        </div>
      </Modal>
    )
  }

  return (
    <Modal titulo="Nueva compra de mercadería" onCerrar={onCerrar} ancho={680}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}

        <div className="grilla-2">
          <Campo etiqueta="Proveedor">
            <select value={proveedorId} onChange={(e) => setProveedorId(e.target.value)} required autoFocus>
              <option value="">Elegir proveedor…</option>
              {(proveedores ?? []).map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}
            </select>
          </Campo>
          <Campo etiqueta="Nº de factura / remito">
            <input value={comprobante} onChange={(e) => setComprobante(e.target.value)} maxLength={50} placeholder="Opcional" />
          </Campo>
        </div>

        <div className="fila-agregar">
          <select value={nuevo.productoId} onChange={(e) => elegirProducto(e.target.value)}>
            <option value="">Elegir producto…</option>
            {productos.map((p) => <option key={p.id} value={p.id}>{p.codigoSku} · {p.nombre} (stock {numero(p.stockActual)})</option>)}
          </select>
          <input type="number" step="any" min="0" placeholder="Cant." title="Cantidad" value={nuevo.cantidad}
                 onChange={(e) => setNuevo({ ...nuevo, cantidad: e.target.value })} />
          <input type="number" step="0.01" min="0" placeholder="Costo unit." title="Costo unitario" value={nuevo.costoUnitario}
                 onChange={(e) => setNuevo({ ...nuevo, costoUnitario: e.target.value })} />
          <button type="button" className="btn btn-secundario" onClick={agregar} disabled={!nuevo.productoId}>Agregar</button>
        </div>

        <table>
          <thead><tr><th>Producto</th><th className="num">Cant.</th><th className="num">Costo unit.</th><th className="num">Subtotal</th><th></th></tr></thead>
          <tbody>
            {items.map((i) => (
              <tr key={i.producto.id}>
                <td>{i.producto.nombre}</td><td className="num">{numero(i.cantidad)}</td>
                <td className="num">{pesos(i.costoUnitario)}</td><td className="num">{pesos(i.cantidad * i.costoUnitario)}</td>
                <td><button type="button" className="btn-icono peligro" onClick={() => setItems(items.filter((x) => x !== i))}><Icono nombre="cerrar" tamano={16} /></button></td>
              </tr>
            ))}
            {items.length === 0 && <Vacio columnas={5}>Agregá los productos comprados</Vacio>}
          </tbody>
        </table>

        <Campo etiqueta="Observaciones">
          <input value={observaciones} onChange={(e) => setObservaciones(e.target.value)} maxLength={500} placeholder="Opcional" />
        </Campo>
        <p className="texto-suave">Al registrar, se suma el stock y el costo de cada producto pasa a ser el de esta compra.</p>

        <div className="modal-pie">
          <strong className="total-modal">Total {pesos(total)}</strong>
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario" disabled={!items.length || !proveedorId}>Registrar compra</button>
        </div>
      </form>
    </Modal>
  )
}
