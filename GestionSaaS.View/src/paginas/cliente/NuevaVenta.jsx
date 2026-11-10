import { useEffect, useMemo, useState } from 'react'
import { api, MEDIOS_PAGO, numero, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Encabezado, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'

export default function NuevaVenta() {
  const avisar = useAvisos()
  const [productos, setProductos] = useState([])
  const [buscar, setBuscar] = useState('')
  const [carrito, setCarrito] = useState([])          // [{ producto, cantidad }]
  const [medioPago, setMedioPago] = useState('Efectivo')
  const [enviando, setEnviando] = useState(false)

  const cargar = () => api.productos().then(setProductos).catch((e) => avisar(e.message, 'error'))
  useEffect(() => { cargar() }, [])

  const filtrados = useMemo(() => {
    const t = buscar.trim().toLowerCase()
    return productos.filter((p) => !t || p.nombre.toLowerCase().includes(t) || p.codigoSku.toLowerCase().includes(t))
  }, [productos, buscar])

  const enCarrito = (id) => carrito.find((i) => i.producto.id === id)?.cantidad ?? 0

  const agregar = (producto) => {
    if (enCarrito(producto.id) + 1 > producto.stockActual) return avisar(`No hay más stock de ${producto.nombre}`, 'error')
    setCarrito((c) => c.some((i) => i.producto.id === producto.id)
      ? c.map((i) => (i.producto.id === producto.id ? { ...i, cantidad: i.cantidad + 1 } : i))
      : [...c, { producto, cantidad: 1 }])
  }

  const cambiarCantidad = (id, valor) =>
    setCarrito((c) => c.map((i) => (i.producto.id === id ? { ...i, cantidad: valor } : i)))

  const quitar = (id) => setCarrito((c) => c.filter((i) => i.producto.id !== id))

  const total = carrito.reduce((s, i) => s + (+i.cantidad || 0) * i.producto.precioVenta, 0)
  const invalido = carrito.some((i) => !(+i.cantidad > 0) || +i.cantidad > i.producto.stockActual)

  const confirmar = async () => {
    setEnviando(true)
    try {
      const venta = await api.registrarVenta({
        medioPago,
        items: carrito.map((i) => ({ productoId: i.producto.id, cantidad: +i.cantidad }))
      })
      avisar(`Venta #${venta.id} registrada: ${pesos(venta.totalVenta)}`)
      setCarrito([])
      cargar()
    } catch (e) { avisar(e.message, 'error') } finally { setEnviando(false) }
  }

  return (
    <>
      <Encabezado titulo="Nueva venta" subtitulo="Elegí los productos y confirmá el cobro" />

      <div className="venta">
        <section className="tarjeta">
          <div className="buscador">
            <Icono nombre="buscar" />
            <input placeholder="Buscar producto por nombre o SKU" value={buscar} onChange={(e) => setBuscar(e.target.value)} autoFocus />
          </div>
          <div className="catalogo">
            {filtrados.map((p) => {
              const disponible = p.stockActual - enCarrito(p.id)
              return (
                <button key={p.id} className="producto-boton" disabled={disponible <= 0} onClick={() => agregar(p)}>
                  <span className="producto-nombre">{p.nombre}</span>
                  <span className="producto-precio">{pesos(p.precioVenta)}</span>
                  <span className={`producto-stock ${p.stockBajo ? 'texto-rojo' : ''}`}>
                    {disponible > 0 ? `Stock ${numero(disponible)}` : 'Sin stock'}
                  </span>
                </button>
              )
            })}
            {filtrados.length === 0 && <p className="texto-suave">No se encontraron productos.</p>}
          </div>
        </section>

        <section className="tarjeta carrito">
          <h3>Detalle</h3>
          <table>
            <thead><tr><th>Producto</th><th className="num">Cant.</th><th className="num">Subtotal</th><th></th></tr></thead>
            <tbody>
              {carrito.map((i) => {
                const excede = +i.cantidad > i.producto.stockActual
                return (
                  <tr key={i.producto.id}>
                    <td>
                      <div className="celda-principal">{i.producto.nombre}</div>
                      <div className="celda-secundaria">{pesos(i.producto.precioVenta)} c/u</div>
                    </td>
                    <td className="num">
                      <input className={`input-cantidad ${excede ? 'invalido' : ''}`} type="number" step="any" min="0"
                             value={i.cantidad} onChange={(e) => cambiarCantidad(i.producto.id, e.target.value)}
                             title={excede ? `Máximo ${numero(i.producto.stockActual)}` : ''} />
                    </td>
                    <td className="num">{pesos((+i.cantidad || 0) * i.producto.precioVenta)}</td>
                    <td><button className="btn-icono peligro" onClick={() => quitar(i.producto.id)} title="Quitar"><Icono nombre="cerrar" tamano={16} /></button></td>
                  </tr>
                )
              })}
              {carrito.length === 0 && <Vacio columnas={4}>Tocá un producto para agregarlo</Vacio>}
            </tbody>
          </table>

          <div className="carrito-pie">
            <label className="campo">
              <span>Medio de pago</span>
              <select value={medioPago} onChange={(e) => setMedioPago(e.target.value)}>
                {Object.entries(MEDIOS_PAGO).map(([v, t]) => <option key={v} value={v}>{t}</option>)}
              </select>
            </label>
            <div className="carrito-total"><span>Total</span><strong>{pesos(total)}</strong></div>
            <button className="btn btn-primario btn-bloque btn-grande" disabled={!carrito.length || invalido || enviando} onClick={confirmar}>
              {enviando ? 'Registrando…' : 'Confirmar venta'}
            </button>
          </div>
        </section>
      </div>
    </>
  )
}
