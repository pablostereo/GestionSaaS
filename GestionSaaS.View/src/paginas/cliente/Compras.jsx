import { Fragment, useEffect, useState } from 'react'
import { api, fecha, numero, pesos, TIPOS_COMPRA } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Etiqueta, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

const TONO_TIPO = { Mercaderia: 'azul', Gasto: 'naranja', Servicio: 'violeta', Otro: 'gris' }

export default function Compras() {
  const avisar = useAvisos()
  const [compras, setCompras] = useState(null)
  const [nueva, setNueva] = useState(false)
  const [abierta, setAbierta] = useState(null)

  const cargar = () => api.compras().then(setCompras).catch((e) => avisar(e.message, 'error'))
  useEffect(() => { cargar() }, [])

  return (
    <>
      <Encabezado titulo="Compras" subtitulo="Mercadería (suma stock), gastos y servicios">
        <button className="btn btn-primario" onClick={() => setNueva(true)}><Icono nombre="mas" /> Nueva compra</button>
      </Encabezado>

      {!compras ? <Cargando /> : (
        <div className="tarjeta sin-relleno">
          <div className="tabla-scroll">
            <table>
              <thead><tr><th>#</th><th>Fecha</th><th>Tipo</th><th>Descripción</th><th className="num">Monto</th><th></th></tr></thead>
              <tbody>
                {compras.map((c) => (
                  <Fragment key={c.id}>
                    <tr className={c.items.length ? 'fila-clic' : ''} onClick={() => c.items.length && setAbierta(abierta === c.id ? null : c.id)}>
                      <td>{c.id}</td><td>{fecha(c.fecha)}</td>
                      <td><Etiqueta tono={TONO_TIPO[c.tipo]}>{TIPOS_COMPRA[c.tipo]}</Etiqueta></td>
                      <td>{c.descripcion || <span className="texto-suave">—</span>}</td>
                      <td className="num celda-principal">{pesos(c.montoTotal)}</td>
                      <td className="num texto-suave">{c.items.length ? (abierta === c.id ? '▲' : '▼') : ''}</td>
                    </tr>
                    {abierta === c.id && (
                      <tr className="fila-detalle">
                        <td colSpan={6}>
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
                {compras.length === 0 && <Vacio columnas={6}>Todavía no hay compras</Vacio>}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {nueva && <FormularioCompra onCerrar={() => setNueva(false)} onGuardado={(msg) => { avisar(msg); setNueva(false); cargar() }} />}
    </>
  )
}

function FormularioCompra({ onCerrar, onGuardado }) {
  const [productos, setProductos] = useState([])
  const [tipo, setTipo] = useState('Mercaderia')
  const [descripcion, setDescripcion] = useState('')
  const [monto, setMonto] = useState('')
  const [items, setItems] = useState([])               // [{ producto, cantidad, costoUnitario }]
  const [nuevo, setNuevo] = useState({ productoId: '', cantidad: '1', costoUnitario: '' })
  const [error, setError] = useState('')

  useEffect(() => { api.productos().then(setProductos).catch((e) => setError(e.message)) }, [])

  const esMercaderia = tipo === 'Mercaderia'
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
      const compra = await api.registrarCompra(esMercaderia
        ? { tipo, descripcion, items: items.map((i) => ({ productoId: i.producto.id, cantidad: i.cantidad, costoUnitario: i.costoUnitario })) }
        : { tipo, descripcion, montoTotal: +monto })
      onGuardado(`Compra #${compra.id} registrada: ${pesos(compra.montoTotal)}${esMercaderia ? ' — stock actualizado' : ''}`)
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo="Nueva compra" onCerrar={onCerrar} ancho={680}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}

        <div className="selector">
          {Object.entries(TIPOS_COMPRA).map(([v, t]) => (
            <button key={v} type="button" className={tipo === v ? 'activo' : ''} onClick={() => setTipo(v)}>{t}</button>
          ))}
        </div>

        <div className={esMercaderia ? '' : 'grilla-2'}>
          <Campo etiqueta={esMercaderia ? 'Proveedor / descripción' : 'Descripción'}>
            <input value={descripcion} onChange={(e) => setDescripcion(e.target.value)} maxLength={500} placeholder={esMercaderia ? 'Distribuidora…' : 'Luz, alquiler…'} />
          </Campo>
          {!esMercaderia && (
            <Campo etiqueta="Monto"><input type="number" step="0.01" min="0" value={monto} onChange={(e) => setMonto(e.target.value)} required /></Campo>
          )}
        </div>

        {esMercaderia && (
          <>
            <div className="fila-agregar">
              <select value={nuevo.productoId} onChange={(e) => elegirProducto(e.target.value)}>
                <option value="">Elegir producto…</option>
                {productos.map((p) => <option key={p.id} value={p.id}>{p.nombre} (stock {numero(p.stockActual)})</option>)}
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
            <p className="texto-suave">Al registrar, se suma el stock y el costo de cada producto pasa a ser el de esta compra.</p>
          </>
        )}

        <div className="modal-pie">
          {esMercaderia && <strong className="total-modal">Total {pesos(total)}</strong>}
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario" disabled={esMercaderia ? !items.length : !(+monto > 0)}>Registrar compra</button>
        </div>
      </form>
    </Modal>
  )
}
