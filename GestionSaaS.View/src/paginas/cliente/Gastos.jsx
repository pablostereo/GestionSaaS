import { useEffect, useState } from 'react'
import { api, CATEGORIAS_GASTO, fechaCorta, hoyISO, MEDIOS_PAGO, pesos } from '../../api.js'
import { useAvisos } from '../../componentes/Avisos.jsx'
import { Campo, Cargando, Encabezado, Etiqueta, Indicador, Vacio } from '../../componentes/Comunes.jsx'
import Icono from '../../componentes/Icono.jsx'
import Modal from '../../componentes/Modal.jsx'

// Gastos = egresos operativos (luz, alquiler, impuestos…). No tocan productos ni stock.
const inicioDelMesISO = () => { const d = new Date(); return new Date(d.getFullYear(), d.getMonth(), 1).toLocaleDateString('sv') }

export default function Gastos() {
  const avisar = useAvisos()
  const [gastos, setGastos] = useState(null)
  const [filtros, setFiltros] = useState({ desde: inicioDelMesISO(), hasta: '', categoria: '' })
  const [editando, setEditando] = useState(null)     // null | {} (nuevo) | gasto

  const cargar = () => api.gastos(filtros).then(setGastos).catch((e) => avisar(e.message, 'error'))
  useEffect(() => { cargar() }, [filtros])

  const filtrar = (campo) => (e) => setFiltros({ ...filtros, [campo]: e.target.value })

  const eliminar = async (g) => {
    if (!confirm(`¿Eliminar el gasto "${g.descripcion}" de ${pesos(g.monto)}?`)) return
    try { await api.eliminarGasto(g.id); avisar('Gasto eliminado'); cargar() }
    catch (e) { avisar(e.message, 'error') }
  }

  const total = gastos?.reduce((s, g) => s + g.monto, 0) ?? 0
  const porCategoria = Object.entries((gastos ?? []).reduce((m, g) => ({ ...m, [g.categoria]: (m[g.categoria] ?? 0) + g.monto }), {}))
    .sort((a, b) => b[1] - a[1])

  return (
    <>
      <Encabezado titulo="Gastos" subtitulo="Egresos operativos: no afectan el stock">
        <button className="btn btn-primario" onClick={() => setEditando({})}><Icono nombre="mas" /> Nuevo gasto</button>
      </Encabezado>

      <div className="barra-filtros">
        <label className="campo-linea">Desde <input type="date" value={filtros.desde} onChange={filtrar('desde')} /></label>
        <label className="campo-linea">Hasta <input type="date" value={filtros.hasta} onChange={filtrar('hasta')} /></label>
        <select value={filtros.categoria} onChange={filtrar('categoria')}>
          <option value="">Todas las categorías</option>
          {Object.entries(CATEGORIAS_GASTO).map(([v, t]) => <option key={v} value={v}>{t}</option>)}
        </select>
        <button className="btn btn-enlace" onClick={() => setFiltros({ desde: '', hasta: '', categoria: '' })}>Ver todo</button>
      </div>

      {!gastos ? <Cargando /> : (
        <>
          <div className="indicadores">
            <Indicador titulo="Total del período" valor={pesos(total)} detalle={`${gastos.length} ${gastos.length === 1 ? 'gasto' : 'gastos'}`} icono="gastos" tono="rojo" />
            {porCategoria[0] && <Indicador titulo="Categoría principal" valor={CATEGORIAS_GASTO[porCategoria[0][0]]} detalle={pesos(porCategoria[0][1])} icono="stock" tono="violeta" />}
          </div>

          <div className="tarjeta sin-relleno">
            <div className="tabla-scroll">
              <table>
                <thead><tr><th>Fecha</th><th>Categoría</th><th>Descripción</th><th>Medio de pago</th><th className="num">Monto</th><th></th></tr></thead>
                <tbody>
                  {gastos.map((g) => (
                    <tr key={g.id}>
                      <td>{fechaCorta(g.fecha)}</td>
                      <td><Etiqueta tono="naranja">{CATEGORIAS_GASTO[g.categoria]}</Etiqueta></td>
                      <td className="celda-principal">{g.descripcion}</td>
                      <td>{MEDIOS_PAGO[g.medioPago]}</td>
                      <td className="num celda-principal">{pesos(g.monto)}</td>
                      <td>
                        <div className="acciones">
                          <button className="btn-icono" title="Editar" onClick={() => setEditando(g)}><Icono nombre="editar" /></button>
                          <button className="btn-icono peligro" title="Eliminar" onClick={() => eliminar(g)}><Icono nombre="borrar" /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                  {gastos.length === 0 && <Vacio columnas={6}>No hay gastos en el período</Vacio>}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}

      {editando && (
        <FormularioGasto gasto={editando} onCerrar={() => setEditando(null)}
                         onGuardado={(msg) => { avisar(msg); setEditando(null); cargar() }} />
      )}
    </>
  )
}

function FormularioGasto({ gasto, onCerrar, onGuardado }) {
  const esNuevo = !gasto.id
  const [form, setForm] = useState({
    descripcion: gasto.descripcion ?? '',
    monto: gasto.monto ?? '',
    fecha: gasto.fecha ? new Date(gasto.fecha).toLocaleDateString('sv') : hoyISO(),
    categoria: gasto.categoria ?? '',
    medioPago: gasto.medioPago ?? 'Efectivo'
  })
  const [error, setError] = useState('')
  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  const guardar = async (e) => {
    e.preventDefault()
    setError('')
    const datos = { ...form, monto: +form.monto }
    try {
      if (esNuevo) await api.registrarGasto(datos)
      else await api.actualizarGasto(gasto.id, datos)
      onGuardado(esNuevo ? `Gasto registrado: ${pesos(datos.monto)}` : 'Gasto actualizado')
    } catch (err) { setError(err.message) }
  }

  return (
    <Modal titulo={esNuevo ? 'Nuevo gasto' : 'Editar gasto'} onCerrar={onCerrar} ancho={560}>
      <form onSubmit={guardar} className="formulario">
        {error && <div className="alerta alerta-error">{error}</div>}
        <Campo etiqueta="Descripción / motivo">
          <input value={form.descripcion} onChange={cambiar('descripcion')} required maxLength={300} autoFocus placeholder="Ej.: Factura de luz de octubre" />
        </Campo>
        <div className="grilla-2">
          <Campo etiqueta="Monto"><input type="number" step="0.01" min="0.01" value={form.monto} onChange={cambiar('monto')} required /></Campo>
          <Campo etiqueta="Fecha"><input type="date" value={form.fecha} max={hoyISO()} onChange={cambiar('fecha')} required /></Campo>
          <Campo etiqueta="Categoría">
            <select value={form.categoria} onChange={cambiar('categoria')} required>
              <option value="">Elegir…</option>
              {Object.entries(CATEGORIAS_GASTO).map(([v, t]) => <option key={v} value={v}>{t}</option>)}
            </select>
          </Campo>
          <Campo etiqueta="Medio de pago">
            <select value={form.medioPago} onChange={cambiar('medioPago')} required>
              {Object.entries(MEDIOS_PAGO).map(([v, t]) => <option key={v} value={v}>{t}</option>)}
            </select>
          </Campo>
        </div>
        <p className="texto-suave">Los gastos solo registran la salida de dinero. Para reponer mercadería usá Compras.</p>
        <div className="modal-pie">
          <button type="button" className="btn btn-secundario" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-primario">Guardar</button>
        </div>
      </form>
    </Modal>
  )
}
