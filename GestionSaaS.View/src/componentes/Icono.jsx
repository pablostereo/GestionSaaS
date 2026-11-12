// Íconos SVG simples (trazo), sin dependencias
const ICONOS = {
  panel: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /></>,
  clientes: <><circle cx="9" cy="8" r="3.5" /><path d="M2.5 20c.8-3.5 3.4-5.5 6.5-5.5s5.7 2 6.5 5.5" /><circle cx="17" cy="9" r="2.5" /><path d="M16.5 14.6c2.4.3 4.2 2 4.9 4.6" /></>,
  inicio: <path d="M3 10.5 12 3l9 7.5V21h-6v-6H9v6H3z" />,
  productos: <><path d="M21 8 12 3 3 8v8l9 5 9-5z" /><path d="m3 8 9 5 9-5M12 13v8" /></>,
  venta: <><circle cx="9" cy="20" r="1.3" /><circle cx="18" cy="20" r="1.3" /><path d="M2 3h3l2.6 12.2h11.2L21 7H6" /></>,
  ventas: <path d="M6 2h12v20l-3-2-3 2-3-2-3 2zM9 7h6M9 11h6M9 15h4" />,
  compras: <><path d="M1 4h13v11H1zM14 8h4l3 3v4h-7" /><circle cx="5.5" cy="18" r="2" /><circle cx="17.5" cy="18" r="2" /></>,
  gastos: <><rect x="2" y="6" width="20" height="13" rx="2" /><path d="M2 10h20M6 15h4" /></>,
  proveedores: <path d="M3 21V8l6-4v4l6-4v4l6-4v17zM7 17h2M11 17h2M15 17h2" />,
  salir: <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" />,
  mas: <path d="M12 5v14M5 12h14" />,
  editar: <path d="M4 20h4L19 9l-4-4L4 16zM13.5 6.5l4 4" />,
  llave: <><circle cx="7.5" cy="15.5" r="3.5" /><path d="m10 13 9-9M16 7l2 2M14 9l2 2" /></>,
  borrar: <path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14" />,
  encender: <path d="M12 3v9M6.4 6.4a8 8 0 1 0 11.2 0" />,
  stock: <path d="M4 20V10M10 20V4M16 20v-7M22 20H2" />,
  alerta: <path d="M12 3 2 20h20zM12 10v4M12 17h.01" />,
  menu: <path d="M3 6h18M3 12h18M3 18h18" />,
  cerrar: <path d="M6 6l12 12M18 6 6 18" />,
  buscar: <><circle cx="11" cy="11" r="7" /><path d="m20 20-4-4" /></>
}

export default function Icono({ nombre, tamano = 18 }) {
  return (
    <svg width={tamano} height={tamano} viewBox="0 0 24 24" fill="none" stroke="currentColor"
         strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      {ICONOS[nombre]}
    </svg>
  )
}
