import { useState } from 'react'
import Icono from './Icono.jsx'

/** Estructura general: menú lateral (según rol) + contenido. */
export default function Layout({ usuario, menu, pagina, onNavegar, onSalir, children }) {
  const [abierto, setAbierto] = useState(false)
  const esAdmin = usuario.rol === 'Admin'

  const navegar = (clave) => { onNavegar(clave); setAbierto(false) }

  return (
    <div className="layout">
      <aside className={`lateral ${abierto ? 'abierto' : ''}`}>
        <div className="marca">
          <div className="marca-logo">G</div>
          <div>
            <div className="marca-nombre">GestionSaaS</div>
            <div className="marca-sub">{esAdmin ? 'Administración' : usuario.nombreComercio}</div>
          </div>
          <button className="btn-icono solo-movil" onClick={() => setAbierto(false)} aria-label="Cerrar menú">
            <Icono nombre="cerrar" />
          </button>
        </div>

        <nav className="menu">
          {Object.entries(menu).map(([clave, item]) => (
            <button key={clave} className={`menu-item ${pagina === clave ? 'activo' : ''}`} onClick={() => navegar(clave)}>
              <Icono nombre={item.icono} />
              <span>{item.titulo}</span>
            </button>
          ))}
        </nav>

        <div className="usuario">
          <div className="usuario-avatar">{usuario.nombre.slice(0, 1).toUpperCase()}</div>
          <div className="usuario-datos">
            <div className="usuario-nombre">{usuario.nombre}</div>
            <div className="usuario-email">{usuario.email}</div>
          </div>
          <button className="btn-icono" onClick={onSalir} title="Cerrar sesión" aria-label="Cerrar sesión">
            <Icono nombre="salir" />
          </button>
        </div>
      </aside>

      {abierto && <div className="lateral-fondo" onClick={() => setAbierto(false)} />}

      <div className="contenido">
        <header className="barra-movil">
          <button className="btn-icono" onClick={() => setAbierto(true)} aria-label="Abrir menú"><Icono nombre="menu" /></button>
          <span>{menu[pagina]?.titulo}</span>
        </header>
        <main>{children}</main>
      </div>
    </div>
  )
}
