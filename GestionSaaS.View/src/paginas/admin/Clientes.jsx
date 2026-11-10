import { useEffect, useState } from 'react'
import { api, fecha, fechaCorta, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Etiqueta, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

const generarContrasena = () =>
  Array.from(crypto.getRandomValues(new Uint8Array(10)), (b) => 'abcdefghjkmnpqrstuvwxyz23456789'[b % 31]).join('')

export default function Clientes() {
  const avisar = useAvisos()
  const [clientes, setClientes] = useState(null)
  const [buscar, setBuscar] = useState('')
  const [estado, setEstado] = useState('todos')
  const [editando, setEditando] = useState(null)       // null | {} (nuevo) | cliente
  const [contrasenaDe, setContrasenaDe] = useState(null)
  const [credenciales, setCredenciales] = useState(null) // { titulo, email, contrasena }

  const cargar = () => api.clientes().then(setClientes).catch((e) => avisar(e.message, 'error'))
  useEffect(() => { cargar() }, [])

  const cambiarEstado = async (c) => {
    const accion = c.activo ? 'dar de baja' : 'reactivar'
    if (!confirm(`¿Seguro que querés ${accion} a "${c.nombreComercio}"?` +
                 (c.activo ? '\n\nNo va a poder ingresar al sistema hasta que lo reactives.' : ''))) return
    try {
      await api.cambiarEstadoCliente(c.id, !c.activo)
      avisar(c.activo ? `${c.nombreComercio} dado de baja` : `${c.nombreComercio} reactivado`)
      cargar()
    } catch (e) { avisar(e.message, 'error') }
  }

  if (!clientes) return <Cargando />

  const texto = buscar.trim().toLowerCase()
  const visibles = clientes.filter((c) =>
    (estado === 'todos' || (estado === 'activos') === c.activo) &&
    (!texto || c.nombreComercio.toLowerCase().includes(texto) || c.email.includes(texto)))

  return (
    <>
      <Encabezado titulo="Clientes" subtitulo="Alta, modificación y baja de comercios">
        <button className="btn btn-primario" onClick={() => setEditando({})}><Icono nombre="mas" /> Nuevo cliente</button>
      </Encabezado>

      <div className="barra-filtros">
        <div className="buscador">
          <Icono nombre="buscar" />
          <input placeholder="Buscar por nombre o email" value={buscar} onChange={(e) => setBuscar(e.target.value)} />
        </div>
        <select value={estado} onChange={(e) => setEstado(e.target.value)}>
          <option value="todos">Todos</option>
          <option value="activos">Activos</option>
          <option value="inactivos">Dados de baja</option>
        </select>
      </div>

      <div className="tarjeta sin-relleno">
        <div className="tabla-scroll">
          <table>
            <thead>
              <tr>
                <th>Comercio</th><th>Estado</th><th>Alta</th><th>Último ingreso</th>
                <th className="num">Productos</th><th className="num">Ventas</th><th className="num">Total vendido</th><th></th>
              </tr>
            </thead>
            <tbody>
              {visibles.map((c) => (
                <tr key={c.id} className={c.activo ? '' : 'fila-apagada'}>
                  <td>
                    <div className="celda-principal">{c.nombreComercio}</div>
                    <div className="celda-secundaria">{c.email}</div>
                  </td>
                  <td>{c.activo ? <Etiqueta tono="verde">Activo</Etiqueta> : <Etiqueta tono="rojo">Baja</Etiqueta>}</td>
                  <td>{fechaCorta(c.fechaAlta)}</td>
                  <td>{fecha(c.ultimoIngreso)}</td>
                  <td className="num">{c.cantidadProductos}</td>
                  <td className="num">{c.cantidadVentas}</td>
                  <td className="num">{pesos(c.totalVendido)}</td>
                  <td>
                    <div className="acciones">
                      <button className="btn-icono" title="Editar" onClick={() => setEditando(c)}><Icono nombre="editar" /></button>
                      <button className="btn-icono" title="Restablecer contraseña" onClick={() => setContrasenaDe(c)}><Icono nombre="llave" /></button>
                      <button className={`btn-icono ${c.activo ? 'peligro' : 'exito'}`} title={c.activo ? 'Dar de baja' : 'Reactivar'}
                              onClick={() => cambiarEstado(c)}><Icono nombre="encender" /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {visibles.length === 0 && <Vacio columnas={8}>No hay clientes que coincidan</Vacio>}
            </tbody>
          </table>
        </div>
      </div>

      {editando && (
        <FormularioCliente cliente={editando} onCerrar={() => setEditando(null)}
                           onGuardado={(msg, cred) => { avisar(msg); setEditando(null); if (cred) setCredenciales(cred); cargar() }} />
      )}
      {contrasenaDe && (
        <FormularioContrasena cliente={contrasenaDe} onCerrar={() => setContrasenaDe(null)}
                              onGuardado={(msg, cred) => { avisar(msg); setContrasenaDe(null); setCredenciales(cred) }} />
      )}
      {credenciales && <VentanaCredenciales {...credenciales} onCerrar={() => setCredenciales(null)} />}
    </>
  )
}

function FormularioCliente({ cliente, onCerrar, onGuardado }) {
  const esNuevo = !cliente.id
  const [form, setForm] = useState({
    nombreComercio: cliente.nombreComercio ?? '',
    email: cliente.email ?? '',
    contrasena: esNuevo ? generarContrasena() : ''
  })
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)

  const guardar = async (e) => {
    e.preventDefault()
    setError(''); setGuardando(true)
    try {
      if (esNuevo) {
        await api.crearCliente(form)
        onGuardado('Cliente creado', { titulo: 'Cliente creado', email: form.email.trim().toLowerCase(), contrasena: form.contrasena })
      } else {
        await api.actualizarCliente(cliente.id, { nombreComercio: form.nombreComercio, email: form.email })
        onGuardado('Cliente actualizado')
      }
    } catch (err) { setError(err.message) } finally { setGuardando(false) }
  }

  return (
    <Modal titulo={esNuevo ? 'Nuevo cliente' : `Editar ${cliente.nombreComercio}`} onCerrar={onCerrar}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <Campo etiqueta="Nombre del comercio">
          <input value={form.nombreComercio} onChange={(e) => setForm({ ...form, nombreComercio: e.target.value })} required autoFocus maxLength={150} />
        </Campo>
        <Campo etiqueta="Email" ayuda="También es el usuario con el que el cliente ingresa al sistema.">
          <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required maxLength={256} />
        </Campo>
        {esNuevo && (
          <Campo etiqueta="Contraseña inicial" ayuda="Pasásela al cliente. Mínimo 6 caracteres.">
            <div className="campo-con-boton">
              <input value={form.contrasena} onChange={(e) => setForm({ ...form, contrasena: e.target.value })} required minLength={6} />
              <button type="button" className="btn btn-secundario" onClick={() => setForm({ ...form, contrasena: generarContrasena() })}>Generar</button>
            </div>
          </Campo>
        )}
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario" disabled={guardando}>{guardando ? 'Guardando…' : 'Guardar'}</button>
        </div>
      </form>
    </Modal>
  )
}

function FormularioContrasena({ cliente, onCerrar, onGuardado }) {
  const [contrasena, setContrasena] = useState(generarContrasena())
  const [error, setError] = useState('')

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    try {
      await api.restablecerContrasena(cliente.id, contrasena)
      onGuardado('Contraseña restablecida', { titulo: 'Contraseña restablecida', email: cliente.email, contrasena })
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo="Restablecer contraseña" onCerrar={onCerrar}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <p className="texto-suave">Usuario: <strong>{cliente.email}</strong></p>
        <Campo etiqueta="Nueva contraseña" ayuda="Mínimo 6 caracteres.">
          <div className="campo-con-boton">
            <input value={contrasena} onChange={(e) => setContrasena(e.target.value)} required minLength={6} autoFocus />
            <button type="button" className="btn btn-secundario" onClick={() => setContrasena(generarContrasena())}>Generar</button>
          </div>
        </Campo>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario">Restablecer</button>
        </div>
      </form>
    </Modal>
  )
}

/** Muestra los datos de acceso para pasárselos al cliente (la contraseña no se puede volver a ver). */
function VentanaCredenciales({ titulo, email, contrasena, onCerrar }) {
  const avisar = useAvisos()
  const texto = `Usuario: ${email}
Contraseña: ${contrasena}`
  const copiar = async () => {
    try { await navigator.clipboard.writeText(texto); avisar('Copiado al portapapeles') }
    catch { avisar('No se pudo copiar, copialo a mano', 'error') }
  }

  return (
    <Modal titulo={titulo} onCerrar={onCerrar} ancho={420}>
      <p className="texto-suave">Pasale estos datos al cliente. La contraseña no se puede volver a consultar.</p>
      <div className="credenciales">
        <div><span>Usuario</span><strong>{email}</strong></div>
        <div><span>Contraseña</span><strong>{contrasena}</strong></div>
      </div>
      <div className="modal-pie">
        <button className="btn btn-secundario" onClick={copiar}>Copiar</button>
        <button className="btn btn-primario" onClick={onCerrar}>Listo</button>
      </div>
    </Modal>
  )
}
