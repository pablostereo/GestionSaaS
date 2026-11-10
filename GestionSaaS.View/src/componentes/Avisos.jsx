import { createContext, useCallback, useContext, useState } from 'react'

// Notificaciones flotantes (abajo a la derecha). Uso: const avisar = useAvisos(); avisar('Guardado')
const Contexto = createContext(() => {})
export const useAvisos = () => useContext(Contexto)

export function AvisosProvider({ children }) {
  const [avisos, setAvisos] = useState([])

  const avisar = useCallback((texto, tipo = 'ok') => {
    const id = Math.random()
    setAvisos((a) => [...a, { id, texto, tipo }])
    setTimeout(() => setAvisos((a) => a.filter((x) => x.id !== id)), tipo === 'error' ? 6000 : 3500)
  }, [])

  return (
    <Contexto.Provider value={avisar}>
      {children}
      <div className="avisos" role="status">
        {avisos.map((a) => <div key={a.id} className={`aviso aviso-${a.tipo}`}>{a.texto}</div>)}
      </div>
    </Contexto.Provider>
  )
}
