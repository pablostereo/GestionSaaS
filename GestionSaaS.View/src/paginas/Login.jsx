import { useState } from 'react'
import { api } from '../api.js'

// Usuarios que crea la Api al iniciar una base vacía (ver Constantes.cs)
const DEMO = [
  { rol: 'Administrador', email: 'admin@gestionsaas.local', contrasena: 'Admin123!' },
  { rol: 'Cliente demo', email: 'demo@gestionsaas.local', contrasena: 'Demo123!' }
]

export default function Login({ onIngreso, motivo }) {
  const [email, setEmail] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  const ingresar = async (e) => {
    e.preventDefault()
    setError(''); setEnviando(true)
    try {
      onIngreso(await api.login(email, contrasena))
    } catch (err) {
      setError(err.message)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login">
      <div className="login-tarjeta">
        <div className="login-marca">
          <div className="marca-logo grande">G</div>
          <h1>GestionSaaS</h1>
          <p>Stock, ventas y compras</p>
        </div>

        {motivo && !error && <div className="alerta alerta-aviso">{motivo}</div>}
        {error && <div className="alerta alerta-error">{error}</div>}

        <form onSubmit={ingresar} className="formulario">
          <label className="campo">
            <span>Email</span>
            <input type="email" autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
          </label>
          <label className="campo">
            <span>Contraseña</span>
            <input type="password" autoComplete="current-password" value={contrasena} onChange={(e) => setContrasena(e.target.value)} required />
          </label>
          <button className="btn btn-primario btn-bloque" disabled={enviando}>
            {enviando ? 'Ingresando…' : 'Ingresar'}
          </button>
        </form>

        <div className="login-demo">
          <span>Usuarios de prueba</span>
          <div>
            {DEMO.map((u) => (
              <button key={u.email} type="button" className="btn btn-secundario btn-chico"
                      onClick={() => { setEmail(u.email); setContrasena(u.contrasena); setError('') }}>
                {u.rol}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
