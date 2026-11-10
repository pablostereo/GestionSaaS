import { useEffect } from 'react'
import Icono from './Icono.jsx'

export default function Modal({ titulo, onCerrar, children, ancho = 480 }) {
  useEffect(() => {
    const teclado = (e) => e.key === 'Escape' && onCerrar()
    window.addEventListener('keydown', teclado)
    return () => window.removeEventListener('keydown', teclado)
  }, [onCerrar])

  return (
    <div className="modal-fondo" onMouseDown={(e) => e.target === e.currentTarget && onCerrar()}>
      <div className="modal" style={{ maxWidth: ancho }} role="dialog" aria-modal="true" aria-label={titulo}>
        <div className="modal-cabecera">
          <h3>{titulo}</h3>
          <button className="btn-icono" onClick={onCerrar} aria-label="Cerrar"><Icono nombre="cerrar" /></button>
        </div>
        {children}
      </div>
    </div>
  )
}
