import { useEffect, useState } from 'react'
import { api, fechaCorta, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Etiqueta, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

export default function Proveedores() {
  const avisar = useAvisos()
  const [proveedores, setProveedores] = useState(null)
  const [buscar, setBuscar] = useState('')
  const [verInactivos, setVerInactivos] = useState(false)
  const [editando, setEditando] = useState(null)     // null | {} (nuevo) | proveedor

  const cargar = () =>
    api.proveedores({ buscar: buscar.trim(), soloActivos: !verInactivos }).then(setProveedores).catch((e) => avisar(e.message, 'error'))

  useEffect(() => { const t = setTimeout(cargar, 250); return () => clearTimeout(t) }, [buscar, verInactivos])

  const cambiarEstado = async (p) => {
    if (p.activo && !confirm(`¿Desactivar a ${p.nombre}? No va a aparecer al cargar compras nuevas.`)) return
    try {
      await api.cambiarEstadoProveedor(p.id, !p.activo)
      avisar(p.activo ? 'Proveedor desactivado' : 'Proveedor reactivado')
      cargar()
    } catch (e) { avisar(e.message, 'error') }
  }

  return (
    <>
      <Encabezado titulo="Proveedores" subtitulo="A quién le comprás la mercadería">
        <button className="btn btn-primario" onClick={() => setEditando({})}><Icono nombre="mas" /> Nuevo proveedor</button>
      </Encabezado>

      <div className="barra-filtros">
        <div className="buscador">
          <Icono nombre="buscar" />
          <input placeholder="Buscar por nombre o CUIT" value={buscar} onChange={(e) => setBuscar(e.target.value)} />
        </div>
        <label className="check">
          <input type="checkbox" checked={verInactivos} onChange={(e) => setVerInactivos(e.target.checked)} />
          Mostrar inactivos
        </label>
      </div>

      {!proveedores ? <Cargando /> : (
        <div className="tarjeta sin-relleno">
          <div className="tabla-scroll">
            <table>
              <thead>
                <tr>
                  <th>Nombre</th><th>CUIT</th><th>Contacto</th>
                  <th className="num">Compras</th><th className="num">Total comprado</th><th>Última compra</th><th></th>
                </tr>
              </thead>
              <tbody>
                {proveedores.map((p) => (
                  <tr key={p.id}>
                    <td className="celda-principal">
                      {p.nombre} {!p.activo && <Etiqueta tono="gris">Inactivo</Etiqueta>}
                    </td>
                    <td>{p.cuit || <span className="texto-suave">—</span>}</td>
                    <td>
                      {p.telefono && <div>{p.telefono}</div>}
                      {p.email && <div className="texto-suave">{p.email}</div>}
                      {!p.telefono && !p.email && <span className="texto-suave">—</span>}
                    </td>
                    <td className="num">{p.cantidadCompras}</td>
                    <td className="num">{pesos(p.totalComprado)}</td>
                    <td>{fechaCorta(p.ultimaCompra)}</td>
                    <td>
                      <div className="acciones">
                        <button className="btn-icono" title="Editar" onClick={() => setEditando(p)}><Icono nombre="editar" /></button>
                        <button className={`btn-icono ${p.activo ? 'peligro' : ''}`} title={p.activo ? 'Desactivar' : 'Reactivar'}
                                onClick={() => cambiarEstado(p)}><Icono nombre="encender" /></button>
                      </div>
                    </td>
                  </tr>
                ))}
                {proveedores.length === 0 && <Vacio columnas={7}>No hay proveedores que coincidan</Vacio>}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {editando && (
        <FormularioProveedor proveedor={editando} onCerrar={() => setEditando(null)}
                             onGuardado={(msg) => { avisar(msg); setEditando(null); cargar() }} />
      )}
    </>
  )
}

function FormularioProveedor({ proveedor, onCerrar, onGuardado }) {
  const esNuevo = !proveedor.id
  const [form, setForm] = useState({
    nombre: proveedor.nombre ?? '', cuit: proveedor.cuit ?? '',
    telefono: proveedor.telefono ?? '', email: proveedor.email ?? ''
  })
  const [error, setError] = useState('')
  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    try {
      if (esNuevo) await api.crearProveedor(form)
      else await api.actualizarProveedor(proveedor.id, form)
      onGuardado(esNuevo ? `Proveedor ${form.nombre.trim()} creado` : 'Proveedor actualizado')
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo={esNuevo ? 'Nuevo proveedor' : `Editar ${proveedor.nombre}`} onCerrar={onCerrar} ancho={560}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <Campo etiqueta="Nombre / razón social">
          <input value={form.nombre} onChange={cambiar('nombre')} required maxLength={150} autoFocus />
        </Campo>
        <div className="grilla-2">
          <Campo etiqueta="CUIT"><input value={form.cuit} onChange={cambiar('cuit')} maxLength={20} placeholder="Opcional" /></Campo>
          <Campo etiqueta="Teléfono"><input value={form.telefono} onChange={cambiar('telefono')} maxLength={50} placeholder="Opcional" /></Campo>
        </div>
        <Campo etiqueta="Email"><input type="email" value={form.email} onChange={cambiar('email')} maxLength={256} placeholder="Opcional" /></Campo>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario">Guardar</button>
        </div>
      </form>
    </Modal>
  )
}
