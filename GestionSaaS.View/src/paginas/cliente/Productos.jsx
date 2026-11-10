import { useEffect, useState } from 'react'
import { api, numero, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Etiqueta, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

export default function Productos() {
  const avisar = useAvisos()
  const [productos, setProductos] = useState(null)
  const [buscar, setBuscar] = useState('')
  const [soloStockBajo, setSoloStockBajo] = useState(false)
  const [editando, setEditando] = useState(null)     // null | {} (nuevo) | producto
  const [ajustando, setAjustando] = useState(null)

  const cargar = () =>
    api.productos({ buscar: buscar.trim(), stockBajo: soloStockBajo }).then(setProductos).catch((e) => avisar(e.message, 'error'))

  // Búsqueda con una pequeña demora para no llamar a la Api en cada tecla
  useEffect(() => { const t = setTimeout(cargar, 250); return () => clearTimeout(t) }, [buscar, soloStockBajo])

  const eliminar = async (p) => {
    if (!confirm(`¿Eliminar "${p.nombre}"?`)) return
    try { await api.eliminarProducto(p.id); avisar('Producto eliminado'); cargar() }
    catch (e) { avisar(e.message, 'error') }
  }

  const listo = (msg) => { avisar(msg); setEditando(null); setAjustando(null); cargar() }

  return (
    <>
      <Encabezado titulo="Productos" subtitulo="Catálogo, precios y stock">
        <button className="btn btn-primario" onClick={() => setEditando({})}><Icono nombre="mas" /> Nuevo producto</button>
      </Encabezado>

      <div className="barra-filtros">
        <div className="buscador">
          <Icono nombre="buscar" />
          <input placeholder="Buscar por nombre o SKU" value={buscar} onChange={(e) => setBuscar(e.target.value)} />
        </div>
        <label className="check">
          <input type="checkbox" checked={soloStockBajo} onChange={(e) => setSoloStockBajo(e.target.checked)} />
          Solo stock bajo
        </label>
      </div>

      {!productos ? <Cargando /> : (
        <div className="tarjeta sin-relleno">
          <div className="tabla-scroll">
            <table>
              <thead>
                <tr>
                  <th>SKU</th><th>Nombre</th><th className="num">Precio</th><th className="num">Costo</th>
                  <th className="num">Margen</th><th className="num">Stock</th><th className="num">Mínimo</th><th></th>
                </tr>
              </thead>
              <tbody>
                {productos.map((p) => {
                  const margen = p.costoActual > 0 ? ((p.precioVenta - p.costoActual) / p.costoActual) * 100 : null
                  return (
                    <tr key={p.id}>
                      <td><code>{p.codigoSku}</code></td>
                      <td className="celda-principal">{p.nombre}</td>
                      <td className="num">{pesos(p.precioVenta)}</td>
                      <td className="num">{pesos(p.costoActual)}</td>
                      <td className="num">{margen == null ? '—' : `${margen.toFixed(0)}%`}</td>
                      <td className="num">
                        {p.stockBajo ? <Etiqueta tono="rojo">{numero(p.stockActual)}</Etiqueta> : numero(p.stockActual)}
                      </td>
                      <td className="num">{numero(p.stockMinimo)}</td>
                      <td>
                        <div className="acciones">
                          <button className="btn-icono" title="Ajustar stock" onClick={() => setAjustando(p)}><Icono nombre="stock" /></button>
                          <button className="btn-icono" title="Editar" onClick={() => setEditando(p)}><Icono nombre="editar" /></button>
                          <button className="btn-icono peligro" title="Eliminar" onClick={() => eliminar(p)}><Icono nombre="borrar" /></button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
                {productos.length === 0 && <Vacio columnas={8}>No hay productos que coincidan</Vacio>}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {editando && <FormularioProducto producto={editando} onCerrar={() => setEditando(null)} onGuardado={listo} />}
      {ajustando && <FormularioAjuste producto={ajustando} onCerrar={() => setAjustando(null)} onGuardado={listo} />}
    </>
  )
}

function FormularioProducto({ producto, onCerrar, onGuardado }) {
  const esNuevo = !producto.id
  const [form, setForm] = useState({
    codigoSku: producto.codigoSku ?? '', nombre: producto.nombre ?? '',
    precioVenta: producto.precioVenta ?? '', costoActual: producto.costoActual ?? '',
    stockMinimo: producto.stockMinimo ?? '', stockInicial: ''
  })
  const [error, setError] = useState('')
  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    const datos = {
      codigoSku: form.codigoSku, nombre: form.nombre,
      precioVenta: +form.precioVenta, costoActual: +form.costoActual,
      stockMinimo: +form.stockMinimo, stockInicial: +form.stockInicial || 0
    }
    try {
      if (esNuevo) await api.crearProducto(datos)
      else await api.actualizarProducto(producto.id, datos)
      onGuardado(esNuevo ? 'Producto creado' : 'Producto actualizado')
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo={esNuevo ? 'Nuevo producto' : `Editar ${producto.nombre}`} onCerrar={onCerrar} ancho={560}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <div className="grilla-2">
          <Campo etiqueta="SKU"><input value={form.codigoSku} onChange={cambiar('codigoSku')} required maxLength={50} autoFocus /></Campo>
          <Campo etiqueta="Nombre"><input value={form.nombre} onChange={cambiar('nombre')} required maxLength={200} /></Campo>
          <Campo etiqueta="Precio de venta"><input type="number" step="0.01" min="0" value={form.precioVenta} onChange={cambiar('precioVenta')} required /></Campo>
          <Campo etiqueta="Costo"><input type="number" step="0.01" min="0" value={form.costoActual} onChange={cambiar('costoActual')} required /></Campo>
          <Campo etiqueta="Stock mínimo" ayuda="Debajo de esto se marca para reponer.">
            <input type="number" step="any" min="0" value={form.stockMinimo} onChange={cambiar('stockMinimo')} required />
          </Campo>
          {esNuevo
            ? <Campo etiqueta="Stock inicial"><input type="number" step="any" min="0" value={form.stockInicial} onChange={cambiar('stockInicial')} placeholder="0" /></Campo>
            : <Campo etiqueta="Stock actual" ayuda="Se modifica con ventas, compras o ajustes."><input value={numero(producto.stockActual)} disabled /></Campo>}
        </div>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario">Guardar</button>
        </div>
      </form>
    </Modal>
  )
}

function FormularioAjuste({ producto, onCerrar, onGuardado }) {
  const [tipo, setTipo] = useState('ingreso')
  const [cantidad, setCantidad] = useState('')
  const [motivo, setMotivo] = useState('')
  const [error, setError] = useState('')

  const resultado = producto.stockActual + (tipo === 'ingreso' ? 1 : -1) * (+cantidad || 0)

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    try {
      await api.ajustarStock(producto.id, { cantidad: (tipo === 'ingreso' ? 1 : -1) * +cantidad, motivo })
      onGuardado(`Stock de ${producto.nombre}: ${numero(resultado)}`)
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo={`Ajustar stock — ${producto.nombre}`} onCerrar={onCerrar}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <div className="selector">
          <button type="button" className={tipo === 'ingreso' ? 'activo' : ''} onClick={() => setTipo('ingreso')}>+ Ingreso</button>
          <button type="button" className={tipo === 'egreso' ? 'activo' : ''} onClick={() => setTipo('egreso')}>− Egreso</button>
        </div>
        <div className="grilla-2">
          <Campo etiqueta="Cantidad"><input type="number" step="any" min="0" value={cantidad} onChange={(e) => setCantidad(e.target.value)} required autoFocus /></Campo>
          <Campo etiqueta="Motivo"><input value={motivo} onChange={(e) => setMotivo(e.target.value)} placeholder="Inventario, rotura…" /></Campo>
        </div>
        <p className="texto-suave">
          Stock actual <strong>{numero(producto.stockActual)}</strong> → queda en{' '}
          <strong className={resultado < 0 ? 'texto-rojo' : ''}>{numero(resultado)}</strong>
        </p>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario" disabled={!cantidad || resultado < 0}>Aplicar ajuste</button>
        </div>
      </form>
    </Modal>
  )
}
