import { useEffect, useState } from 'react'
import { alExpirarSesion, cerrarSesion, guardarSesion, obtenerSesion } from './api.js'
import { AvisosProvider } from './componentes/Avisos.jsx'
import Layout from './componentes/Layout.jsx'
import Login from './paginas/Login.jsx'
// Administrador
import Panel from './paginas/admin/Panel.jsx'
import Clientes from './paginas/admin/Clientes.jsx'
// Cliente
import Inicio from './paginas/cliente/Inicio.jsx'
import Estadisticas from './paginas/cliente/Estadisticas.jsx'
import Productos from './paginas/cliente/Productos.jsx'
import NuevaVenta from './paginas/cliente/NuevaVenta.jsx'
import Ventas from './paginas/cliente/Ventas.jsx'
import Compras from './paginas/cliente/Compras.jsx'
import Gastos from './paginas/cliente/Gastos.jsx'
import Proveedores from './paginas/cliente/Proveedores.jsx'

const MENU_ADMIN = {
  panel: { titulo: 'Panel', icono: 'panel', Pagina: Panel },
  clientes: { titulo: 'Clientes', icono: 'clientes', Pagina: Clientes }
}

const MENU_CLIENTE = {
  inicio: { titulo: 'Inicio', icono: 'inicio', Pagina: Inicio },
  estadisticas: { titulo: 'Estadísticas', icono: 'estadisticas', Pagina: Estadisticas },
  nuevaVenta: { titulo: 'Nueva venta', icono: 'venta', Pagina: NuevaVenta },
  productos: { titulo: 'Productos', icono: 'productos', Pagina: Productos },
  ventas: { titulo: 'Ventas', icono: 'ventas', Pagina: Ventas },
  compras: { titulo: 'Compras', icono: 'compras', Pagina: Compras },
  gastos: { titulo: 'Gastos', icono: 'gastos', Pagina: Gastos },
  proveedores: { titulo: 'Proveedores', icono: 'proveedores', Pagina: Proveedores }
}

export default function App() {
  const [sesion, setSesion] = useState(obtenerSesion())
  const [motivoSalida, setMotivoSalida] = useState('')
  const [pagina, setPagina] = useState(null)

  // Si la Api responde 401 (token vencido o cliente dado de baja), vuelve al login
  useEffect(() => alExpirarSesion((motivo) => { setSesion(null); setMotivoSalida(motivo) }), [])

  const ingresar = (s) => { guardarSesion(s); setSesion(s); setMotivoSalida(''); setPagina(null) }
  const salir = () => { cerrarSesion(); setSesion(null) }

  if (!sesion) {
    return <AvisosProvider><Login onIngreso={ingresar} motivo={motivoSalida} /></AvisosProvider>
  }

  const menu = sesion.usuario.rol === 'Admin' ? MENU_ADMIN : MENU_CLIENTE
  const actual = menu[pagina] ? pagina : Object.keys(menu)[0]
  const { Pagina } = menu[actual]

  return (
    <AvisosProvider>
      <Layout usuario={sesion.usuario} menu={menu} pagina={actual} onNavegar={setPagina} onSalir={salir}>
        <Pagina irA={setPagina} />
      </Layout>
    </AvisosProvider>
  )
}
