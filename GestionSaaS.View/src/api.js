// Conexión con GestionSaaS.Api + manejo de la sesión (token JWT)

const CLAVE_SESION = 'gestionsaas.sesion'

function leerSesion() {
  try {
    const s = JSON.parse(localStorage.getItem(CLAVE_SESION))
    return s && new Date(s.expira) > new Date() ? s : null
  } catch { return null }
}

let sesion = leerSesion()
let alExpirar = () => {}

export const obtenerSesion = () => sesion
export const alExpirarSesion = (fn) => { alExpirar = fn }

export function guardarSesion(s) {
  sesion = s
  try { localStorage.setItem(CLAVE_SESION, JSON.stringify(s)) } catch { /* sin storage */ }
}

export function cerrarSesion() {
  sesion = null
  try { localStorage.removeItem(CLAVE_SESION) } catch { /* sin storage */ }
}

async function pedir(metodo, url, cuerpo) {
  const headers = { 'Content-Type': 'application/json' }
  if (sesion) headers.Authorization = `Bearer ${sesion.token}`

  let res
  try {
    res = await fetch(`/api${url}`, { method: metodo, headers, body: cuerpo ? JSON.stringify(cuerpo) : undefined })
  } catch {
    throw new Error('No se pudo conectar con la Api. ¿Está corriendo GestionSaaS.Api?')
  }

  if (res.status === 204) return null
  const datos = await res.json().catch(() => null)

  if (res.status === 401 && sesion) {
    cerrarSesion()
    alExpirar(datos?.title || 'La sesión expiró. Ingresá nuevamente.')
  }
  if (res.status === 403) throw new Error('No tenés permiso para esta acción.')
  if (!res.ok) throw new Error(datos?.title || `Error ${res.status}`)
  return datos
}

const qs = (params) => {
  const p = Object.entries(params).filter(([, v]) => v !== '' && v != null && v !== false)
  return p.length ? '?' + new URLSearchParams(p).toString() : ''
}

export const api = {
  // Autenticación
  login: (email, contrasena) => pedir('POST', '/auth/login', { email, contrasena }),

  // Administrador
  clientes: () => pedir('GET', '/clientes'),
  crearCliente: (c) => pedir('POST', '/clientes', c),
  actualizarCliente: (id, c) => pedir('PUT', `/clientes/${id}`, c),
  cambiarEstadoCliente: (id, activo) => pedir('PUT', `/clientes/${id}/activo?valor=${activo}`),
  restablecerContrasena: (id, contrasena) => pedir('PUT', `/clientes/${id}/contrasena`, { contrasena }),

  // Cliente
  productos: (filtros = {}) => pedir('GET', `/productos${qs(filtros)}`),
  crearProducto: (p) => pedir('POST', '/productos', p),
  actualizarProducto: (id, p) => pedir('PUT', `/productos/${id}`, p),
  ajustarStock: (id, a) => pedir('POST', `/productos/${id}/ajuste-stock`, a),
  eliminarProducto: (id) => pedir('DELETE', `/productos/${id}`),
  ventas: (filtros = {}) => pedir('GET', `/ventas${qs(filtros)}`),
  registrarVenta: (v) => pedir('POST', '/ventas', v),
  compras: (filtros = {}) => pedir('GET', `/compras${qs(filtros)}`),
  registrarCompra: (c) => pedir('POST', '/compras', c)
}

// --- Constantes y formatos ---
export const MEDIOS_PAGO = {
  Efectivo: 'Efectivo', TarjetaDebito: 'Tarjeta débito', TarjetaCredito: 'Tarjeta crédito',
  Transferencia: 'Transferencia', MercadoPago: 'Mercado Pago', CuentaCorriente: 'Cuenta corriente'
}
export const TIPOS_COMPRA = { Mercaderia: 'Mercadería', Gasto: 'Gasto', Servicio: 'Servicio', Otro: 'Otro' }

export const pesos = (n) => Number(n ?? 0).toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })
export const numero = (n) => Number(n ?? 0).toLocaleString('es-AR', { maximumFractionDigits: 3 })
export const fecha = (f) => (f ? new Date(f).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' }) : '—')
export const fechaCorta = (f) => (f ? new Date(f).toLocaleDateString('es-AR') : '—')
export const hoyISO = () => new Date().toLocaleDateString('sv')   // yyyy-mm-dd en hora local
