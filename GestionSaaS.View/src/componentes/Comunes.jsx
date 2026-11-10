import Icono from './Icono.jsx'

/** Tarjeta de indicador (KPI). */
export function Indicador({ titulo, valor, detalle, icono, tono = 'azul' }) {
  return (
    <div className="indicador">
      <div className={`indicador-icono tono-${tono}`}><Icono nombre={icono} tamano={20} /></div>
      <div>
        <div className="indicador-titulo">{titulo}</div>
        <div className="indicador-valor">{valor}</div>
        {detalle && <div className="indicador-detalle">{detalle}</div>}
      </div>
    </div>
  )
}

/** Encabezado de página con título y acciones a la derecha. */
export function Encabezado({ titulo, subtitulo, children }) {
  return (
    <div className="encabezado">
      <div>
        <h2>{titulo}</h2>
        {subtitulo && <p className="subtitulo">{subtitulo}</p>}
      </div>
      <div className="encabezado-acciones">{children}</div>
    </div>
  )
}

export function Etiqueta({ tono = 'gris', children }) {
  return <span className={`etiqueta etiqueta-${tono}`}>{children}</span>
}

export function Vacio({ children, columnas }) {
  return <tr><td colSpan={columnas} className="vacio">{children}</td></tr>
}

export function Cargando() {
  return <div className="cargando">Cargando…</div>
}

/** Campo de formulario con etiqueta. */
export function Campo({ etiqueta, ayuda, children }) {
  return (
    <label className="campo">
      <span>{etiqueta}</span>
      {children}
      {ayuda && <small>{ayuda}</small>}
    </label>
  )
}
